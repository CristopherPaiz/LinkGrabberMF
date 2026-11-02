using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;
using LinkGrab.Ui.Dialogs;
using LinkGrab.Ui.Grid;

namespace LinkGrab.Ui.Main
{
    public partial class MainForm
    {
        JdConnection connection = JdConnection.Unknown;
        bool syncing;
        bool collecting;

        void UpdateActivity()
        {
            activity.Active = busy || collecting;
        }

        // Runs on a timer. While JD2 is away the list stays as it was,
        // so nothing looks removed just because JD2 closed.
        async Task SyncJd()
        {
            if (syncing) return;
            syncing = true;
            JdSnapshot snapshot = null;
            try
            {
                snapshot = await Task.Run(() =>
                {
                    sender.LoadJdFolder();
                    return JdSnapshot.Fetch();
                });
            }
            catch (Exception)
            {
                // Handled below: no snapshot means JD2 didn't answer.
            }

            try
            {
                if (snapshot == null) await ReportJdAway();
                else await ApplySnapshot(snapshot);
            }
            finally
            {
                syncing = false;
                RevealWindow();
            }
        }

        async Task ReportJdAway()
        {
            bool open = await Task.Run(() => JDownloaderClient.JdOpen());
            SetConnection(open ? JdConnection.ApiOff : JdConnection.Closed);
            RevealWindow();
        }

        async Task ApplySnapshot(JdSnapshot snapshot)
        {
            SetConnection(JdConnection.Online);
            collecting = snapshot.Collecting;
            UpdateActivity();
            snapshot.ApplyTo(items);

            string state = JdStateText.For(snapshot.State, collecting, snapshot.Downloading);
            jdInfo.Text = snapshot.Speed > 0
                ? string.Format(Strings.JdWithSpeed, state, Theme.FormatSize(snapshot.Speed))
                : string.Format(Strings.JdPrefix, state);
            RevealWindow();

            int before = items.Count;
            List<string> fresh = await watcher.Check(snapshot);
            if (fresh.Count > 0) NotifyProblems(fresh);

            if (items.Count != before || grid.Filter != ItemFilter.All) grid.Rebuild();
            else grid.Invalidate();
            UpdateSummary();

            if (settings.AutoRenew && !busy) await RunExclusive(RenewExpired);
        }

        void SetConnection(JdConnection value)
        {
            JdConnection previous = connection;
            connection = value;
            UpdateActions();

            jdDot.ForeColor = value == JdConnection.Online ? Theme.Good : Theme.Bad;
            jdInfo.Cursor = value == JdConnection.Online ? System.Windows.Forms.Cursors.Default : System.Windows.Forms.Cursors.Hand;
            if (value == JdConnection.Closed) jdInfo.Text = Strings.JdClosed;
            if (value == JdConnection.ApiOff) jdInfo.Text = Strings.JdApiOff;
            if (value == previous || !settings.Notifications) return;

            if (value == JdConnection.Closed) Toast.Show(Glyph.Stop, Strings.ToastClosedTitle, Strings.ToastClosedBody, Strings.ToastActionSteps, Theme.Bad, () => ShowApiHelp());
            else if (value == JdConnection.ApiOff) Toast.Show(Glyph.Warning, Strings.ToastApiTitle, Strings.ToastApiBody, Strings.ToastActionSteps, Theme.Warn, () => ShowApiHelp());
            else if (previous != JdConnection.Unknown) Toast.Show(Glyph.Done, Strings.ToastBackTitle, Strings.ToastBackBody, Strings.ToastActionOpen, Theme.Good, ShowWindow);
        }

        void NotifyProblems(List<string> names)
        {
            if (!settings.Notifications) return;
            string heading = names.Count == 1 ? Strings.ToastOneProblem : string.Format(Strings.ToastManyProblems, names.Count);
            string body = string.Join(", ", names.Take(Limits.NotificationNames)) + (names.Count > Limits.NotificationNames ? "..." : "");
            Toast.Show(Glyph.Warning, heading, string.Format(Strings.ToastProblemBody, body), Strings.ToastActionOpen, Theme.Warn, ShowWindow);
        }

        // Brings JD2 and this list into line: imports what only JD2 has and reports what it no longer has.
        async Task SyncWithJd()
        {
            JdSnapshot snapshot = await Task.Run(() => JdSnapshot.Fetch());
            int imported = await JdImporter.ImportMissing(snapshot, items, (done, total) => SetStatus(string.Format(Strings.Importing, done, total)));

            snapshot.ApplyTo(items);
            grid.Rebuild();
            UpdateSummary();
            SetStatus(string.Format(Strings.Synced, imported, items.Count(i => i.Sent && !i.InJd)));
            sync.Flash(Strings.ButtonSynced);
        }

        async Task RenewExpired()
        {
            List<FileItem> expired = items.Where(i => i.LinkState == LinkState.Expired && i.InJd && !i.JdRunning && !i.JdFinished).ToList();
            if (expired.Count == 0) return;

            await RenewLinks(expired);
            SetStatus(string.Format(Strings.AutoRenewed, expired.Count(i => i.LinkState == LinkState.Ready)));
        }
    }
}
