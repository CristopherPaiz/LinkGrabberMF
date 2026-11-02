using System;
using System.Collections.Generic;
using System.Linq;
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
            UpdateActions();
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
                busy = false;
                UpdateActions();
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
            switch (action)
            {
                case RowAction.GetLink:
                    await RunExclusive(() => GetLinks(targets.Where(i => i.WantsLink).ToList()));
                    break;
                case RowAction.Reload:
                    await RunExclusive(() => GetLinks(targets));
                    break;
                case RowAction.Copy:
                    CopyToClipboard(targets, false);
                    break;
                case RowAction.CopyAll:
                    CopyToClipboard(targets, true);
                    break;
                default:
                    await RunJd(() => SendItems(targets));
                    break;
            }
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

        // Returns how many files are new to the list.
        async Task<int> AddLinks(string text)
        {
            List<string> urls = LinkText.Urls(text);
            if (urls.Count == 0)
            {
                SetStatus(Strings.NoLinksFound, true);
                return 0;
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
                catch (UnsupportedSiteException e)
                {
                    failure = added > 0 ? string.Format(Strings.AddedWithUnsupported, added, e.Message) : e.Message;
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
            return added;
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
            List<FileItem> chosen = SelectedOrAll().Where(i => i.DirectUrl != null).ToList();
            if (chosen.Count == 0)
            {
                SetStatus(Strings.NothingToVerify, true);
                return;
            }

            chosen.ForEach(i => i.Verifying = true);
            grid.Invalidate();
            await LinkService.VerifyAll(chosen);
            chosen.ForEach(i => i.Verifying = false);

            int bad = chosen.Count(i => i.LinkState != LinkState.Ready);
            SetStatus(string.Format(Strings.Verified, chosen.Count, bad), bad > 0);
            if (bad == 0) verify.Flash(Strings.ButtonVerified);
            UpdateSummary();
            grid.Invalidate();
        }

        // Only what lacks a working link: asking again for a good one would waste a request.
        // With nothing selected, that means the whole list.
        Task GetLinksForSelection()
        {
            List<FileItem> selected = grid.SelectedItems();
            return GetLinks((selected.Count > 0 ? selected : items).Where(i => i.WantsLink).ToList());
        }

        // JD2's copy can only be replaced while it answers; until then the new link stays here.
        bool CanReinsert(FileItem item)
        {
            return item.InJd && JdConnections.CanUse(connection);
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
            if (blocked.Count > 0)
            {
                SetStatus(string.Format(Strings.GettingBlocked, blocked.Count));
                SetResolving(blocked, true);
                await ShowBrowser(blocked);
                SetResolving(blocked, false);
            }

            List<FileItem> obtained = targets.Where(i => i.LinkState == LinkState.Ready && i.DirectUrl != null).ToList();
            bool copied = obtained.Count > 0 && WriteClipboard(obtained.Select(i => i.DirectUrl));
            if (copied) grid.ConfirmCopied(obtained, false);
            SetStatus(string.Format(copied ? Strings.LinksCopied : Strings.LinksObtained, obtained.Count, targets.Count), obtained.Count < targets.Count);
            await SyncJd();
        }

        // One at a time, with each row showing its turn, so a long list doesn't look like a burst to the site.
        async Task RenewLinks(List<FileItem> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                FileItem item = targets[i];
                SetStatus(string.Format(Strings.Resolving, i + 1, targets.Count));
                item.Resolving = true;
                grid.Invalidate();

                await LinkService.Resolve(item);

                item.Resolving = false;
                grid.Invalidate();
                if (item.LinkState == LinkState.Ready && CanReinsert(item)) await sender.Reinsert(item);
            }
            UpdateSummary();
        }

        void SetResolving(List<FileItem> targets, bool value)
        {
            targets.ForEach(i => i.Resolving = value);
            grid.Invalidate();
        }

        // Kept apart so WebView2 only loads when a page really needs it.
        // Not a modal dialog: while it works off screen, this window has to stay responsive.
        async Task ShowBrowser(List<FileItem> queue)
        {
            var closed = new TaskCompletionSource<bool>();
            using (var browser = new BrowserForm(queue, OnLinkCaptured, ShowWindow))
            {
                browser.FormClosed += (s, e) => closed.TrySetResult(true);
                browser.Show(this);
                await closed.Task;
            }
        }

        async Task OnLinkCaptured(FileItem item, string link)
        {
            item.DirectUrl = link;
            item.LinkState = LinkState.Ready;
            item.Resolving = false;
            grid.Invalidate();
            UpdateSummary();
            if (CanReinsert(item)) await sender.Reinsert(item);
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
            CopyToClipboard(grid.SelectedItems(), false);
        }

        void CopyToClipboard(List<FileItem> chosen, bool fromHeader)
        {
            List<string> links = chosen.Select(i => i.DirectUrl ?? i.PageUrl).ToList();
            if (links.Count == 0) return;
            if (!WriteClipboard(links))
            {
                SetStatus(Strings.CopyFailed, true);
                return;
            }
            grid.ConfirmCopied(chosen, fromHeader);
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
