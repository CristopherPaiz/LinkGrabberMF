using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;
using LinkGrab.Ui.Dialogs;
using LinkGrab.Ui.Grid;
using LinkState = LinkGrab.Models.LinkState;

namespace LinkGrab.Ui.Main
{
    public partial class MainForm
    {
        bool busy;

        async Task RunExclusive(Func<Task> work)
        {
            if (busy) return;
            busy = true;
            actions.ForEach(b => b.Enabled = false);
            UpdateActivity();
            try
            {
                await work();
            }
            catch (Exception e)
            {
                SetStatus(e.Message, true);
            }
            finally
            {
                actions.ForEach(b => b.Enabled = true);
                busy = false;
                UpdateActivity();
                SaveSession();
            }
        }

        // Actions that talk to JD2 first make sure it is there, and explain what to do if not.
        async Task RunJd(Func<Task> work)
        {
            if (busy) return;
            bool online = await Task.Run(() => JDownloaderClient.ApiOnline());
            if (online || ShowApiHelp()) await RunExclusive(work);
        }

        bool ShowApiHelp()
        {
            ShowWindow();
            using (var help = new ApiHelpForm()) return help.ShowDialog(this) == DialogResult.OK;
        }

        async void OnRowAction(RowAction action, List<FileItem> targets)
        {
            if (action == RowAction.GetLink) await RunJd(() => GetLinks(targets));
            else await RunJd(() => SendItems(targets));
        }

        async Task PromptForLinks()
        {
            string text;
            using (var dialog = new AddLinksForm())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                text = dialog.Value;
            }
            await RunExclusive(() => AddLinks(text));
        }

        async Task AddLinks(string text)
        {
            List<string> urls = Regex.Matches(text ?? "", WebConstants.UrlPattern).Cast<Match>().Select(m => m.Value).ToList();
            if (urls.Count == 0)
            {
                SetStatus(Strings.NoLinksFound, true);
                return;
            }

            int added = 0;
            string failure = null;
            foreach (string url in urls)
            {
                SetStatus(string.Format(Strings.Reading, url));
                try
                {
                    string current = url;
                    List<FileItem> found = await Task.Run(() => MediaFireClient.Expand(current));
                    List<FileItem> fresh = found.Where(f => items.All(i => i.Key != f.Key)).ToList();
                    items.AddRange(fresh);
                    added += fresh.Count;
                }
                catch (Exception e)
                {
                    failure = string.Format(Strings.AddedWithFailure, added, url, e.Message);
                }
            }

            grid.Rebuild();
            UpdateSummary();
            if (failure != null) SetStatus(failure, true);
            else SetStatus(string.Format(Strings.Added, added));
        }

        Task ResolveAll(List<FileItem> targets)
        {
            return LinkService.ResolveAll(targets, (done, total) =>
            {
                SetStatus(string.Format(Strings.Resolving, done, total));
                grid.Invalidate();
            });
        }

        // With nothing selected, this acts on every file.
        List<FileItem> SelectedOrAll()
        {
            List<FileItem> selected = grid.SelectedItems();
            return selected.Count > 0 ? selected : items.ToList();
        }

        Task SendSelected()
        {
            return SendItems(SelectedOrAll());
        }

        async Task SendItems(List<FileItem> chosen)
        {
            List<FileItem> targets = chosen.Where(i => i.CanSend).ToList();
            if (targets.Count == 0)
            {
                SetStatus(Strings.NothingToSend);
                return;
            }

            await ResolveAll(targets.Where(i => i.LinkState != LinkState.Ready || i.IsRemoved).ToList());
            List<FileItem> ready = targets.Where(i => i.LinkState == LinkState.Ready).ToList();
            if (ready.Count > 0) await sender.Add(ready);

            int pending = targets.Count - ready.Count;
            SetStatus(pending > 0 ? string.Format(Strings.SentPending, ready.Count, pending) : string.Format(Strings.Sent, ready.Count), pending > 0);
            await SyncJd();
        }

        async Task VerifySelected()
        {
            List<FileItem> targets = SelectedOrAll().Where(i => i.DirectUrl != null).ToList();
            if (targets.Count == 0)
            {
                SetStatus(Strings.NothingToVerify);
                return;
            }

            await LinkService.VerifyAll(targets);
            int bad = targets.Count(i => i.LinkState != LinkState.Ready);
            SetStatus(string.Format(Strings.Verified, targets.Count, bad), bad > 0);
            UpdateSummary();
            grid.Invalidate();
        }

        // With nothing selected, this acts on everything that lacks a working link.
        Task GetLinksForSelection()
        {
            List<FileItem> selected = grid.SelectedItems();
            return GetLinks(selected.Count > 0 ? selected : items.Where(i => i.NeedsLink).ToList());
        }

        // Tries a plain request first; whatever that can't get opens in the built-in browser, one by one.
        async Task GetLinks(List<FileItem> chosen)
        {
            List<FileItem> targets = chosen.Where(i => !i.JdFinished && !i.JdRunning).ToList();
            if (targets.Count == 0)
            {
                SetStatus(Strings.NothingToGet);
                return;
            }

            await RenewLinks(targets);
            List<FileItem> blocked = targets.Where(i => i.LinkState == LinkState.Challenge).ToList();
            if (blocked.Count > 0) OpenBrowser(blocked);

            int failed = targets.Count(i => i.LinkState != LinkState.Ready);
            SetStatus(string.Format(Strings.LinksObtained, targets.Count - failed, targets.Count), failed > 0);
            await SyncJd();
        }

        async Task RenewLinks(List<FileItem> targets)
        {
            await ResolveAll(targets);
            foreach (FileItem item in targets.Where(i => i.LinkState == LinkState.Ready && i.InJd).ToList())
                await sender.Reinsert(item);
        }

        void OpenBrowser(List<FileItem> queue)
        {
            ShowWindow();
            using (var browser = new BrowserForm(queue, OnLinkCaptured)) browser.ShowDialog(this);
        }

        async Task OnLinkCaptured(FileItem item, string link)
        {
            item.DirectUrl = link;
            item.LinkState = LinkState.Ready;
            grid.Invalidate();
            UpdateSummary();
            if (item.InJd) await sender.Reinsert(item);
            SaveSession();
        }

        Task RenameFolder()
        {
            string folder = grid.CurrentFolder;
            if (folder == "") return Done(Strings.SelectFolder, true);

            List<FileItem> group = items.Where(i => FolderPaths.IsInside(i.Folder, folder)).ToList();
            if (group.Any(i => i.InJd)) return Done(Strings.FolderInJd, true);

            using (var dialog = new InputForm(Strings.RenameTitle, Strings.RenamePrompt, FolderPaths.Leaf(folder)))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Value == "") return Done("", false);
                string parent = FolderPaths.Parent(folder);
                string renamed = parent == "" ? dialog.Value : parent + FolderPaths.Separator + dialog.Value;
                group.ForEach(i => i.Folder = renamed + i.Folder.Substring(folder.Length));
            }
            grid.Rebuild();
            return Done(Strings.FolderRenamed, false);
        }

        Task Done(string text, bool error)
        {
            SetStatus(text, error);
            return Task.FromResult(0);
        }

        void CopyLinks()
        {
            List<string> links = grid.SelectedItems().Select(i => i.DirectUrl ?? i.PageUrl).ToList();
            if (links.Count == 0) return;
            Clipboard.SetText(string.Join(Environment.NewLine, links));
            SetStatus(string.Format(Strings.Copied, links.Count));
        }

        async Task StartDownloads()
        {
            List<long> ids = grid.SelectedItems().Where(i => i.JdWhere == JdLocation.Grabber).Select(i => i.JdUuid).ToList();
            await Task.Run(() =>
            {
                if (ids.Count > 0) JDownloaderClient.MoveToDownloads(ids);
                JDownloaderClient.Start();
            });
            SetStatus(ids.Count > 0 ? string.Format(Strings.MovedAndStarted, ids.Count) : Strings.Started);
            await SyncJd();
        }

        async Task StopDownloads()
        {
            await Task.Run(() => JDownloaderClient.Stop());
            SetStatus(Strings.Stopped);
            await SyncJd();
        }

        // Only removes from this list; whatever is in JD2 stays there.
        Task RemoveSelected()
        {
            List<FileItem> chosen = grid.SelectedItems();
            if (chosen.Count == 0) return Done(Strings.SelectToRemove, true);

            items.RemoveAll(chosen.Contains);
            grid.ClearSelection();
            grid.Rebuild();
            UpdateSummary();
            return Done(string.Format(Strings.Removed, chosen.Count), false);
        }
    }
}
