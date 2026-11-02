using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;
using LinkGrab.Ui.Controls;
using LinkGrab.Ui.Dialogs;
using LinkGrab.Ui.Grid;
using Timer = System.Windows.Forms.Timer;

namespace LinkGrab.Ui.Main
{
    // Window lifecycle and shared state. Layout, actions and JD2 sync live in the other MainForm files.
    public partial class MainForm : Form
    {
        readonly Settings settings = Settings.Load();
        readonly List<FileItem> items = new List<FileItem>();
        readonly ItemGrid grid = new ItemGrid();
        readonly Label message = new Label();
        readonly Label counts = new Label();
        readonly Label jdDot = new Label();
        readonly Label jdInfo = new Label();
        readonly ActivityBar activity = new ActivityBar();
        readonly NotifyIcon tray = new NotifyIcon();
        readonly Icon iconNormal = Painter.MakeAppIcon(false);
        readonly Icon iconAlert = Painter.MakeAppIcon(true);
        readonly Timer poll = new Timer();
        readonly JdSender sender;
        readonly StuckDownloadWatcher watcher;
        bool exiting;

        static string SessionFile { get { return AppFiles.Path(AppFileNames.Session); } }

        public MainForm()
        {
            sender = new JdSender(settings);
            watcher = new StuckDownloadWatcher(items);

            Text = AppInfo.Name;
            Icon = iconNormal;
            Size = new Size(Theme.S(1400), Theme.S(780));
            MinimumSize = new Size(Theme.S(1400), Theme.S(520));
            BackColor = Theme.Back;
            ForeColor = Theme.Text;
            Font = Theme.Normal;
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            BuildLayout();
            BuildTray();
            BuildRowMenu();
            LoadSession();
            grid.Bind(items);
            grid.SelectionChanged += (s, e) => UpdateSummary();
            grid.RowActionClicked += OnRowAction;
            UpdateSummary();

            poll.Interval = settings.PollMilliseconds;
            poll.Tick += async (s, e) => await SyncJd();
            poll.Start();

            Shown += async (s, e) => await SyncJd();
            FormClosing += OnClosing;
            KeyDown += OnKeyDown;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.DarkTitleBar(this);
        }

        void ShowWindow()
        {
            Show();
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
        }

        void ExitApp()
        {
            exiting = true;
            Close();
        }

        void OnClosing(object source, FormClosingEventArgs e)
        {
            if (exiting || e.CloseReason != CloseReason.UserClosing)
            {
                Shutdown();
                return;
            }

            CloseAction action = settings.CloseAction;
            if (action == CloseAction.Ask && !AskCloseAction(out action))
            {
                e.Cancel = true;
                return;
            }

            if (action == CloseAction.Exit)
            {
                Shutdown();
                return;
            }

            e.Cancel = true;
            if (action == CloseAction.Minimize) WindowState = FormWindowState.Minimized;
            else Hide();
        }

        bool AskCloseAction(out CloseAction action)
        {
            action = CloseAction.Ask;
            using (var dialog = new CloseForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return false;
                action = dialog.Choice;
                if (dialog.Remember)
                {
                    settings.CloseAction = action;
                    settings.Save();
                }
                return true;
            }
        }

        void Shutdown()
        {
            SaveSession();
            tray.Visible = false;
        }

        async void OnKeyDown(object source, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.Handled = true;
                string text = Clipboard.GetText();
                await RunExclusive(() => AddLinks(text));
            }
            else if (e.Control && e.KeyCode == Keys.A)
            {
                e.Handled = true;
                grid.SelectAll();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                await RunExclusive(RemoveSelected);
            }
            else if (e.KeyCode == Keys.F5)
            {
                e.Handled = true;
                await RunJd(SyncWithJd);
            }
            else if (e.KeyCode == Keys.F1)
            {
                e.Handled = true;
                OpenHelp();
            }
        }

        void OpenHelp()
        {
            using (var help = new HelpForm()) help.ShowDialog(this);
        }

        void OpenSettings()
        {
            using (var dialog = new SettingsForm(settings, sender.JdFolder))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                settings.Save();
                poll.Interval = settings.PollMilliseconds;
            }
        }

        void LoadSession()
        {
            try
            {
                if (File.Exists(SessionFile)) items.AddRange(Json.Read<List<FileItem>>(File.ReadAllText(SessionFile)));
            }
            catch (Exception)
            {
                SetStatus(Strings.SessionUnreadable, true);
            }
        }

        void SaveSession()
        {
            try
            {
                File.WriteAllText(SessionFile, Json.Write(items));
            }
            catch (Exception e)
            {
                SetStatus(string.Format(Strings.SessionUnsaved, e.Message), true);
            }
        }
    }
}
