using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;
using LinkGrab.Ui.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace LinkGrab.Ui.Dialogs
{
    // Shows pages that need a person, one at a time, inside the app.
    // This window only watches the page until the rule finds the download link.
    public class BrowserForm : Form
    {
        readonly List<FileItem> queue;
        readonly Func<FileItem, string, Task> onCaptured;
        readonly WebView2 view = new WebView2();
        readonly Label title = new Label { AutoSize = true, UseMnemonic = false, Font = Theme.Bold };
        readonly Label hint = new Label { AutoSize = true, ForeColor = Theme.Muted };
        readonly Timer watcher = new Timer { Interval = Timing.BrowserCheckMs };
        int index = -1;
        bool checking;
        Rule rule;
        string host;

        public BrowserForm(List<FileItem> queue, Func<FileItem, string, Task> onCaptured)
        {
            this.queue = queue;
            this.onCaptured = onCaptured;

            Text = Strings.BrowserTitle;
            Size = new Size(Theme.S(1000), Theme.S(760));
            MinimumSize = new Size(Theme.S(640), Theme.S(480));
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Back;
            ForeColor = Theme.Text;
            Font = Theme.Normal;

            view.Dock = DockStyle.Fill;
            view.DefaultBackgroundColor = Theme.Back;
            Controls.Add(view);
            Controls.Add(BuildHeader());

            watcher.Tick += async (s, e) => await CheckPage();
            Shown += async (s, e) => await Start();
            FormClosed += (s, e) => watcher.Stop();
        }

        Control BuildHeader()
        {
            var header = new TableLayoutPanel { Dock = DockStyle.Top, Height = Theme.S(66), ColumnCount = 3, RowCount = 2, Padding = new Padding(Theme.S(14), Theme.S(8), Theme.S(14), Theme.S(6)), BackColor = Theme.Back };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var reload = new IconButton(Strings.ButtonReload, Glyph.Sync);
            reload.Click += (s, e) => { if (view.CoreWebView2 != null) view.CoreWebView2.Reload(); };
            var skip = new IconButton(Strings.ButtonSkip) { Margin = new Padding(0) };
            skip.Click += (s, e) => Next();

            header.Controls.Add(title, 0, 0);
            header.Controls.Add(hint, 0, 1);
            header.Controls.Add(reload, 1, 0);
            header.Controls.Add(skip, 2, 0);
            header.SetRowSpan(reload, 2);
            header.SetRowSpan(skip, 2);
            return header;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.DarkTitleBar(this);
        }

        async Task Start()
        {
            string failure = null, detail = null;
            try
            {
                CoreWebView2Environment.SetLoaderDllFolderPath(WebViewLibs.LoaderFolder);
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(null, WebViewLibs.BrowserProfile);
                await view.EnsureCoreWebView2Async(environment);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                failure = Strings.BrowserNoRuntime;
                detail = string.Format(Strings.BrowserInstall, WebViewFiles.InstallUrl);
            }
            catch (Exception e)
            {
                failure = Strings.BrowserFailed;
                detail = e.Message;
            }

            if (failure != null)
            {
                title.Text = failure;
                hint.Text = detail;
                return;
            }

            ConfigureBrowser(view.CoreWebView2);
            Next();
            watcher.Start();
        }

        void ConfigureBrowser(CoreWebView2 core)
        {
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.NewWindowRequested += (s, e) => e.Handled = true;
            core.DownloadStarting += (s, e) => e.Cancel = true;
            core.NavigationStarting += (s, e) => { if (!IsAllowed(e.Uri)) e.Cancel = true; };
        }

        // Keeps the window on the file's site, so ads can't send it anywhere else.
        bool IsAllowed(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri)) return false;
            if (uri.Scheme == "about") return true;
            return uri.Host.EndsWith(host, StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(WebConstants.CloudflareHost, StringComparison.OrdinalIgnoreCase);
        }

        void Next()
        {
            index++;
            if (index >= queue.Count)
            {
                Close();
                return;
            }

            FileItem item = queue[index];
            rule = RuleSet.For(item.PageUrl);
            host = new Uri(item.PageUrl).Host.Replace("www.", "");
            title.Text = string.Format(Strings.BrowserProgress, index + 1, queue.Count, item.Name);
            hint.Text = Strings.BrowserHint;
            hint.ForeColor = Theme.Muted;
            view.CoreWebView2.Navigate(item.PageUrl);
        }

        async Task CheckPage()
        {
            if (checking || rule == null || index >= queue.Count) return;
            checking = true;
            try
            {
                string raw = await view.CoreWebView2.ExecuteScriptAsync(WebConstants.ReadHtmlScript);
                string html = Json.Read<string>(raw);
                string link = html == null ? null : LinkResolver.Extract(html, rule);
                if (link == null) return;

                hint.Text = Strings.BrowserCaptured;
                await onCaptured(queue[index], link);
                Next();
            }
            catch (Exception e)
            {
                hint.Text = e.Message;
                hint.ForeColor = Theme.Bad;
            }
            finally
            {
                checking = false;
            }
        }
    }
}
