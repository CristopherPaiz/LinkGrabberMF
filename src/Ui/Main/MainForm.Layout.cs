using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;
using LinkGrab.Ui.Controls;
using LinkGrab.Ui.Grid;

namespace LinkGrab.Ui.Main
{
    public partial class MainForm
    {
        readonly Dictionary<ItemFilter, NavItem> nav = new Dictionary<ItemFilter, NavItem>();
        readonly List<Button> actions = new List<Button>();
        readonly Dictionary<IconButton, string> jdActions = new Dictionary<IconButton, string>();
        readonly List<ToolStripItem> jdMenuItems = new List<ToolStripItem>();
        readonly ToolTip tips = new ToolTip();
        readonly IconButton activateJd = new IconButton(Strings.ButtonActivateApi, Glyph.Warning) { Tint = Theme.Warn, Visible = false };
        IconButton getLinks;
        IconButton verify;
        IconButton sync;

        void BuildLayout()
        {
            var host = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Back, Padding = new Padding(Theme.S(12), 0, Theme.S(12), Theme.S(10)) };
            host.Controls.Add(grid);

            var content = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Back };
            content.Controls.Add(host);
            content.Controls.Add(BuildToolbar());

            Controls.Add(content);
            Controls.Add(BuildSidebar());
            Controls.Add(BuildStatusBar());
            SetStatus(Strings.Hello);
        }

        Control BuildSidebar()
        {
            var entries = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Side };
            AddNav(entries, ItemFilter.Removed, Strings.NavRemoved, Glyph.Gone, null);
            AddNav(entries, ItemFilter.Done, Strings.NavDone, Glyph.Done, Theme.Good);
            AddNav(entries, ItemFilter.InJd, Strings.NavInJd, Glyph.Download, null);
            AddNav(entries, ItemFilter.Problems, Strings.NavProblems, Glyph.Warning, Theme.Warn);
            AddNav(entries, ItemFilter.All, Strings.NavAll, Glyph.List, null);
            nav[ItemFilter.All].Active = true;

            var brand = new Label { Text = AppInfo.Name, Font = Theme.Brand, Dock = DockStyle.Top, Height = Theme.S(58), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(Theme.S(16), 0, 0, 0) };
            var footer = new Panel { Dock = DockStyle.Bottom, Height = Theme.S(84), BackColor = Theme.Side, Padding = new Padding(0, 0, 0, Theme.S(12)) };
            footer.Controls.Add(WindowEntry(Strings.ButtonHelp, Glyph.Help, Strings.TipHelp, OpenHelp));
            footer.Controls.Add(WindowEntry(Strings.ButtonSettings, Glyph.Settings, Strings.TipSettings, OpenSettings));

            var sidebar = new Panel { Dock = DockStyle.Left, Width = Theme.S(210), BackColor = Theme.Side };
            sidebar.Controls.Add(entries);
            sidebar.Controls.Add(footer);
            sidebar.Controls.Add(brand);
            return sidebar;
        }

        void AddNav(Panel host, ItemFilter filter, string text, string glyph, Color? countColor)
        {
            var item = new NavItem(text, glyph) { CountColor = countColor };
            item.Click += (s, e) =>
            {
                foreach (var entry in nav) entry.Value.Active = entry.Key == filter;
                grid.Filter = filter;
                UpdateSummary();
            };
            nav[filter] = item;
            host.Controls.Add(item);
        }

        Control BuildToolbar()
        {
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = Theme.S(56), Padding = new Padding(Theme.S(12), Theme.S(11), 0, 0), BackColor = Theme.Back, WrapContents = false };

            var add = new IconButton(Strings.ButtonAdd, Glyph.Add, true);
            add.Click += async (s, e) => await PromptForLinks();
            actions.Add(add);
            tips.SetToolTip(add, Strings.TipAdd);
            toolbar.Controls.Add(add);

            activateJd.Click += async (s, e) => { if (ShowApiHelp()) await SyncJd(); };
            tips.SetToolTip(activateJd, Strings.TipActivate);
            toolbar.Controls.Add(activateJd);

            toolbar.Controls.Add(JdAction(Strings.ButtonSend, Glyph.Send, Theme.Violet, Strings.TipSend, SendSelected));
            getLinks = LocalAction(Strings.ButtonGetLinks, Glyph.Link, Theme.Cyan, Strings.TipGetLinks, GetLinksForSelection);
            toolbar.Controls.Add(getLinks);
            verify = LocalAction(Strings.ButtonVerify, Glyph.Check, Theme.Good, Strings.TipVerify, VerifySelected);
            toolbar.Controls.Add(verify);
            toolbar.Controls.Add(MakeGap());
            toolbar.Controls.Add(JdAction(Strings.ButtonStart, Glyph.Play, Theme.Good, Strings.TipStart, StartDownloads));
            toolbar.Controls.Add(JdAction(Strings.ButtonStop, Glyph.Stop, Theme.Bad, Strings.TipStop, StopDownloads));
            toolbar.Controls.Add(MakeGap());
            toolbar.Controls.Add(LocalAction(Strings.ButtonRemove, Glyph.Delete, Theme.Bad, Strings.TipRemove, RemoveSelected));
            sync = JdAction(Strings.ButtonSync, Glyph.Sync, Theme.Accent, Strings.TipSync, SyncWithJd);
            toolbar.Controls.Add(sync);
            return toolbar;
        }

        static Control MakeGap()
        {
            return new Panel { Width = Theme.S(1), Height = Theme.S(22), BackColor = Theme.Line, Margin = new Padding(Theme.S(4), Theme.S(6), Theme.S(10), 0) };
        }

        IconButton LocalAction(string text, string glyph, Color hue, string tip, Func<Task> work)
        {
            var button = new IconButton(text, glyph) { Hue = hue };
            button.Click += async (s, e) => await RunExclusive(work);
            actions.Add(button);
            tips.SetToolTip(button, tip);
            return button;
        }

        IconButton JdAction(string text, string glyph, Color hue, string tip, Func<Task> work)
        {
            var button = new IconButton(text, glyph) { Hue = hue };
            button.Click += async (s, e) => await RunJd(work);
            jdActions[button] = tip;
            tips.SetToolTip(button, tip);
            return button;
        }

        NavItem WindowEntry(string text, string glyph, string tip, Action open)
        {
            var entry = new NavItem(text, glyph);
            entry.Click += (s, e) => open();
            tips.SetToolTip(entry, tip);
            return entry;
        }

        Control BuildStatusBar()
        {
            var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, BackColor = Theme.Side, Padding = new Padding(Theme.S(12), 0, Theme.S(12), 0) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            foreach (Label label in new[] { message, counts, jdDot, jdInfo })
            {
                label.AutoSize = true;
                label.Font = Theme.Small;
                label.ForeColor = Theme.Muted;
                label.Anchor = AnchorStyles.Left;
                label.UseMnemonic = false;
            }
            counts.Margin = new Padding(0, 0, Theme.S(18), 0);
            jdDot.Text = "●";
            jdDot.Margin = new Padding(0);
            jdInfo.Margin = new Padding(0);
            jdInfo.Click += (s, e) => { if (connection != Services.JdConnection.Online) ShowApiHelp(); };

            row.Controls.Add(message, 0, 0);
            row.Controls.Add(counts, 1, 0);
            row.Controls.Add(jdDot, 2, 0);
            row.Controls.Add(jdInfo, 3, 0);

            activity.Dock = DockStyle.Top;
            activity.Height = Theme.S(2);

            var bar = new Panel { Dock = DockStyle.Bottom, Height = Theme.S(32), BackColor = Theme.Side };
            bar.Controls.Add(row);
            bar.Controls.Add(activity);
            return bar;
        }

        void BuildTray()
        {
            ContextMenuStrip menu = MakeMenu();
            menu.Items.Add(Strings.TrayOpen, null, (s, e) => ShowWindow());
            menu.Items.Add(Strings.TrayExit, null, (s, e) => ExitApp());
            tray.Icon = iconNormal;
            tray.Text = AppInfo.Name;
            tray.ContextMenuStrip = menu;
            tray.DoubleClick += (s, e) => ShowWindow();
            tray.Visible = true;
        }

        void BuildRowMenu()
        {
            ContextMenuStrip menu = MakeMenu();
            menu.Items.Add(Strings.MenuGetLink, null, async (s, e) => await RunExclusive(GetLinksForSelection));
            jdMenuItems.Add(menu.Items.Add(Strings.MenuSend, null, async (s, e) => await RunJd(SendSelected)));
            menu.Items.Add(Strings.MenuVerify, null, async (s, e) => await RunExclusive(VerifySelected));
            menu.Items.Add(Strings.MenuCopy, null, (s, e) => CopyLinks());
            menu.Items.Add(Strings.MenuRename, null, async (s, e) => await RunExclusive(RenameFolder));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(Strings.MenuRemove, null, async (s, e) => await RunExclusive(RemoveSelected));
            grid.RowMenu = menu;

            ContextMenuStrip choose = MakeMenu();
            choose.Items.Add(Strings.SelectAll, null, (s, e) => grid.SelectAll());
            choose.Items.Add(Strings.SelectNone, null, (s, e) => grid.ClearSelection());
            choose.Items.Add(new ToolStripSeparator());
            choose.Items.Add(Strings.SelectNoLink, null, (s, e) => grid.SelectWhere(i => i.WantsLink));
            choose.Items.Add(Strings.SelectWithLink, null, (s, e) => grid.SelectWhere(i => i.DirectUrl != null));
            choose.Items.Add(Strings.SelectUnsent, null, (s, e) => grid.SelectWhere(i => i.CanSend));
            choose.Items.Add(Strings.SelectProblems, null, (s, e) => grid.SelectWhere(i => i.NeedsAttention));
            grid.SelectionMenu = choose;
        }

        static ContextMenuStrip MakeMenu()
        {
            return new ContextMenuStrip { BackColor = Theme.Panel, ForeColor = Theme.Text, ShowImageMargin = false, ShowItemToolTips = true, RenderMode = ToolStripRenderMode.System };
        }

        void UpdateSummary()
        {
            nav[ItemFilter.All].Count = items.Count;
            nav[ItemFilter.Problems].Count = items.Count(i => i.NeedsAttention);
            nav[ItemFilter.InJd].Count = items.Count(i => i.InJd);
            nav[ItemFilter.Done].Count = items.Count(i => i.JdFinished);
            nav[ItemFilter.Removed].Count = items.Count(i => i.IsRemoved);

            counts.Text = DescribeCounts();
            UpdateGetLinksButton();
            UpdateTray(nav[ItemFilter.Problems].Count);
        }

        string DescribeCounts()
        {
            List<FileItem> selected = grid.SelectedItems();
            if (selected.Count > 0) return string.Format(Strings.CountSelected, selected.Count, Theme.FormatSize(selected.Sum(i => i.Size)));

            string total = items.Count == 1 ? Strings.CountOne : string.Format(Strings.CountMany, items.Count);
            return items.Count == 0 ? total : string.Format(Strings.CountWithSize, total, Theme.FormatSize(items.Sum(i => i.Size)));
        }

        // Whatever needs JD2 turns off while it can't answer; the rest of the list keeps working.
        void UpdateActions()
        {
            bool canUseJd = JdConnections.CanUse(connection);
            string why = connection == JdConnection.Closed ? Strings.TipNeedsJdOpen : Strings.TipNeedsApi;

            actions.ForEach(b => b.Enabled = !busy);
            foreach (var action in jdActions)
            {
                action.Key.Enabled = !busy;
                action.Key.Blocked = !canUseJd;
                tips.SetToolTip(action.Key, canUseJd ? action.Value : why);
            }
            foreach (ToolStripItem item in jdMenuItems)
            {
                item.Enabled = canUseJd;
                item.ToolTipText = canUseJd ? null : why;
            }
            grid.ActionsEnabled = canUseJd;
            grid.DisabledActionTip = why;

            activateJd.Text = connection == JdConnection.Closed ? Strings.ButtonOpenJdShort : Strings.ButtonActivateApi;
            activateJd.Fit();
            activateJd.Visible = !canUseJd;
        }

        void UpdateGetLinksButton()
        {
            int pending = items.Count(i => i.WantsLink);
            getLinks.Text = pending > 0 ? string.Format(Strings.ButtonGetLinksCount, pending) : Strings.ButtonGetLinks;
            getLinks.Tint = items.Any(i => i.NeedsLink) ? Theme.Warn : (Color?)null;
            getLinks.Fit();
            getLinks.Invalidate();
        }

        void UpdateTray(int problems)
        {
            Icon wanted = problems > 0 ? iconAlert : iconNormal;
            if (tray.Icon != wanted) tray.Icon = wanted;
            string tip = problems > 0 ? string.Format(Strings.TrayProblems, problems) : AppInfo.Name;
            if (tray.Text != tip) tray.Text = tip;
        }

        void SetStatus(string text, bool error = false)
        {
            message.Text = text;
            message.ForeColor = error ? Theme.Bad : Theme.Muted;
        }
    }
}
