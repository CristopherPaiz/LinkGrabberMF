using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;

namespace LinkGrab.Ui.Grid
{
    public class FolderRow
    {
        public string Path { get; set; }
        public string Name { get { return FolderPaths.Leaf(Path); } }
        public int Depth { get { return FolderPaths.Depth(Path); } }
    }

    // The file list: rows, selection and clicks. Cell drawing is in ItemGrid.Paint.cs.
    public partial class ItemGrid : DataGridView
    {
        const int ColCheck = 0, ColName = 1, ColSize = 2, ColProgress = 3, ColState = 4, ColAction = 5;
        const int HeaderCheckWidth = 30;

        readonly HashSet<string> collapsed = new HashSet<string>();
        List<FileItem> items = new List<FileItem>();
        ItemFilter filter = ItemFilter.All;
        int hoverRow = -1;
        bool actionsEnabled = true;
        RowAction hoverAction = RowAction.None;
        int hoverActionRow = -1;
        HashSet<FileItem> confirming = new HashSet<FileItem>();
        int copiedAll;
        Timer confirmTimer;

        public event Action<RowAction, List<FileItem>> RowActionClicked;

        public ItemGrid()
        {
            ApplyStyle();
            AddColumns();
            CellPainting += PaintCell;
            Paint += PaintEmptyState;
            CellClick += OnCellClick;
            CellDoubleClick += OnCellDoubleClick;
            CellMouseDown += OnCellMouseDown;
            CellMouseEnter += (s, e) => SetHover(e.RowIndex);
            CellMouseMove += OnCellMouseMove;
            CellMouseLeave += OnCellMouseLeave;
            MouseLeave += (s, e) => SetHover(-1);
            CellToolTipTextNeeded += OnToolTipNeeded;
            SelectionChanged += (s, e) =>
            {
                InvalidateColumn(ColCheck);
                InvalidateCell(Columns[ColCheck].HeaderCell);
                InvalidateCell(Columns[ColAction].HeaderCell);
            };
            ColumnHeaderMouseClick += OnHeaderClick;
        }

        public ContextMenuStrip SelectionMenu { get; set; }

        // Keeps only the files that match; a folder row isn't chosen just for holding one.
        public void SelectWhere(Func<FileItem, bool> wanted)
        {
            ClearSelection();
            foreach (DataGridViewRow row in Rows)
            {
                var item = row.Tag as FileItem;
                if (item != null && wanted(item)) row.Selected = true;
            }
        }

        void OnHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == ColAction && RowActionClicked != null && CopyAllArea().Contains(PointToClient(Cursor.Position)))
            {
                List<FileItem> targets = CopyTargets();
                if (targets.Count > 0) RowActionClicked(RowAction.CopyAll, targets);
            }
            if (e.ColumnIndex != ColCheck) return;
            if (e.X >= Theme.S(HeaderCheckWidth) && SelectionMenu != null) SelectionMenu.Show(Cursor.Position);
            else if (Rows.Count > 0 && SelectedRows.Count == Rows.Count) ClearSelection();
            else SelectAll();
        }

        // A click on a row's box adds or removes only that row, unlike a click anywhere else.
        protected override void OnMouseDown(MouseEventArgs e)
        {
            HitTestInfo hit = HitTest(e.X, e.Y);
            if (e.Button == MouseButtons.Left && hit.Type == DataGridViewHitTestType.Cell && hit.ColumnIndex == ColCheck)
            {
                ToggleRow(hit.RowIndex);
                return;
            }
            base.OnMouseDown(e);
        }

        // Repaints only when the button under the mouse changes, not on every movement.
        void OnCellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != ColAction) return;
            bool overCopyAll = e.RowIndex < 0 && CopyAllArea().Contains(PointToClient(Cursor.Position));
            RowAction action = e.RowIndex >= 0 ? ActionUnderMouse(e.RowIndex) : overCopyAll ? RowAction.Copy : RowAction.None;
            if (action == hoverAction && e.RowIndex == hoverActionRow) return;

            hoverAction = action;
            hoverActionRow = e.RowIndex;
            InvalidateActionCell(e.RowIndex);
        }

        void OnCellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != ColAction || hoverAction == RowAction.None) return;
            hoverAction = RowAction.None;
            InvalidateActionCell(e.RowIndex);
        }

        void InvalidateActionCell(int row)
        {
            if (row < 0) InvalidateCell(Columns[ColAction].HeaderCell);
            else if (row < Rows.Count) InvalidateCell(ColAction, row);
        }

        List<FileItem> CopyTargets()
        {
            return ItemPresenter.CopyTargets(items, SelectedItems(), filter);
        }

        string CopyAllLabel()
        {
            if (copiedAll > 0) return string.Format(Strings.ActionCopiedCount, copiedAll);
            return ItemPresenter.CopyLabel(SelectedItems().Count > 0, CopyTargets().Count);
        }

        // Turns the copy buttons green for a moment, so a click visibly did something.
        public void ConfirmCopied(List<FileItem> copied, bool fromHeader)
        {
            confirming = new HashSet<FileItem>(copied);
            copiedAll = fromHeader ? copied.Count : 0;
            if (confirmTimer == null)
            {
                confirmTimer = new Timer { Interval = Timing.ButtonFlashMs };
                confirmTimer.Tick += (s, e) =>
                {
                    confirmTimer.Stop();
                    confirming.Clear();
                    copiedAll = 0;
                    Invalidate();
                };
            }
            confirmTimer.Stop();
            confirmTimer.Start();
            Invalidate();
        }

        bool IsConfirming(RowAction action, FileItem item)
        {
            return action == RowAction.Copy && item != null && confirming.Contains(item);
        }

        string LabelOf(RowAction action, FileItem item)
        {
            return IsConfirming(action, item) ? Strings.ActionCopied : ItemPresenter.LabelOf(action);
        }

        Rectangle CopyAllArea()
        {
            Rectangle header = GetCellDisplayRectangle(ColAction, -1, false);
            return CellPainter.ActionAreas(header, new[] { CopyAllLabel() })[0];
        }

        void ToggleRow(int index)
        {
            bool chosen = !Rows[index].Selected;
            Rows[index].Selected = chosen;

            var folder = Rows[index].Tag as FolderRow;
            if (folder == null) return;
            foreach (DataGridViewRow row in Rows)
            {
                if (IsInFolder(row.Tag, folder)) row.Selected = chosen;
            }
        }

        static bool IsInFolder(object tag, FolderRow folder)
        {
            var item = tag as FileItem;
            if (item != null) return FolderPaths.IsInside(item.Folder, folder.Path);
            var inner = tag as FolderRow;
            return inner != null && FolderPaths.IsInside(inner.Path, folder.Path);
        }

        public ContextMenuStrip RowMenu { get; set; }
        public string DisabledActionTip { get; set; }

        public bool ActionsEnabled
        {
            get { return actionsEnabled; }
            set
            {
                if (actionsEnabled == value) return;
                actionsEnabled = value;
                Invalidate();
            }
        }

        public void Bind(List<FileItem> source)
        {
            items = source;
            Rebuild();
        }

        public ItemFilter Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                Rebuild();
            }
        }

        void AddColumns()
        {
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Width = Theme.S(50), Resizable = DataGridViewTriState.False });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnName, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = Theme.S(200) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnSize, Width = Theme.S(84) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnProgress, Width = Theme.S(110) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnState, Width = Theme.S(200) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Width = Theme.S(200), Resizable = DataGridViewTriState.False });
            foreach (DataGridViewColumn column in Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        void ApplyStyle()
        {
            Dock = DockStyle.Fill;
            DoubleBuffered = true;
            BackgroundColor = Theme.Panel;
            BorderStyle = BorderStyle.None;
            CellBorderStyle = DataGridViewCellBorderStyle.None;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            EnableHeadersVisualStyles = false;
            RowHeadersVisible = false;
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToResizeRows = false;
            ReadOnly = true;
            MultiSelect = true;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            StandardTab = true;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ColumnHeadersHeight = Theme.S(34);
            RowTemplate.Height = Theme.S(32);
        }

        // Selected rows, with folders standing for everything inside them.
        public List<FileItem> SelectedItems()
        {
            var result = new HashSet<FileItem>();
            foreach (DataGridViewRow row in SelectedRows)
            {
                var folder = row.Tag as FolderRow;
                if (folder != null) result.UnionWith(ItemsIn(folder));
                else if (row.Tag is FileItem) result.Add((FileItem)row.Tag);
            }
            return result.ToList();
        }

        public string CurrentFolder
        {
            get
            {
                if (CurrentRow == null) return "";
                var folder = CurrentRow.Tag as FolderRow;
                if (folder != null) return folder.Path;
                var item = CurrentRow.Tag as FileItem;
                return item != null ? item.Folder : "";
            }
        }

        List<FileItem> ItemsIn(FolderRow folder)
        {
            return items.Where(i => FolderPaths.IsInside(i.Folder, folder.Path)).ToList();
        }

        public void Rebuild()
        {
            int scroll = FirstDisplayedScrollingRowIndex;
            var selected = new HashSet<object>(SelectedRows.Cast<DataGridViewRow>().Select(r => SelectionKey(r.Tag)));
            List<object> rows = BuildRows();

            // Rebuilding what is already on screen would flicker and lose the scroll position for nothing.
            if (rows.Select(SelectionKey).SequenceEqual(Rows.Cast<DataGridViewRow>().Select(r => SelectionKey(r.Tag))))
            {
                Invalidate();
                return;
            }

            Rows.Clear();
            if (rows.Count > 0) Rows.Add(rows.Count);
            for (int i = 0; i < rows.Count; i++) Rows[i].Tag = rows[i];
            for (int i = 0; i < rows.Count; i++) Rows[i].Selected = selected.Contains(SelectionKey(rows[i]));
            if (rows.Count > 0 && selected.Count == 0) ClearSelection();

            if (scroll >= 0 && scroll < Rows.Count) FirstDisplayedScrollingRowIndex = scroll;
            Invalidate();
        }

        List<object> BuildRows()
        {
            List<FileItem> visible = items.Where(i => ItemPresenter.Matches(i, filter)).ToList();
            var paths = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (FileItem item in visible.Where(i => i.Folder != ""))
                paths.UnionWith(FolderPaths.Chain(item.Folder));

            var rows = new List<object>();
            foreach (string path in paths.Where(IsVisibleFolder))
            {
                rows.Add(new FolderRow { Path = path });
                if (!collapsed.Contains(path)) rows.AddRange(FilesIn(visible, path));
            }
            rows.AddRange(FilesIn(visible, ""));
            return rows;
        }

        static IEnumerable<FileItem> FilesIn(List<FileItem> visible, string folder)
        {
            return visible.Where(i => i.Folder == folder).OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase);
        }

        bool IsVisibleFolder(string path)
        {
            return !FolderPaths.Chain(path).Where(p => p != path).Any(collapsed.Contains);
        }

        static object SelectionKey(object tag)
        {
            var folder = tag as FolderRow;
            return folder != null ? folder.Path : tag;
        }

        void OnCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            object tag = Rows[e.RowIndex].Tag;
            var folder = tag as FolderRow;

            if (e.ColumnIndex == ColAction) RaiseRowAction(e.RowIndex, tag);
            else if (folder != null && e.ColumnIndex == ColName && ClickedChevron(e.RowIndex, folder)) ToggleFolder(folder);
        }

        bool ClickedChevron(int row, FolderRow folder)
        {
            int left = GetCellDisplayRectangle(ColName, row, false).X;
            return PointToClient(Cursor.Position).X <= left + CellPainter.ChevronEnd(folder.Depth);
        }

        void RaiseRowAction(int row, object tag)
        {
            RowAction action = ActionUnderMouse(row);
            if (RowActionClicked == null || action == RowAction.None || !IsActionEnabled(action)) return;

            var item = tag as FileItem;
            RowActionClicked(action, item != null ? new List<FileItem> { item } : ItemsIn((FolderRow)tag));
        }

        RowAction ActionUnderMouse(int row)
        {
            object tag = Rows[row].Tag;
            var item = tag as FileItem;
            List<RowAction> actions = item != null ? ItemPresenter.ActionsOf(item) : ItemPresenter.ActionsOf(ItemsIn((FolderRow)tag));

            Rectangle cell = GetCellDisplayRectangle(ColAction, row, false);
            List<Rectangle> areas = CellPainter.ActionAreas(cell, actions.Select(a => LabelOf(a, item)));
            Point mouse = PointToClient(Cursor.Position);
            for (int i = 0; i < actions.Count; i++)
            {
                if (areas[i].Contains(mouse)) return actions[i];
            }
            return RowAction.None;
        }

        bool IsActionEnabled(RowAction action)
        {
            return actionsEnabled || !ItemPresenter.RequiresJd(action);
        }

        void OnCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var folder = Rows[e.RowIndex].Tag as FolderRow;
            if (folder != null) ToggleFolder(folder);
        }

        void OnCellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || e.RowIndex < 0 || RowMenu == null) return;
            if (!Rows[e.RowIndex].Selected)
            {
                ClearSelection();
                Rows[e.RowIndex].Selected = true;
            }
            CurrentCell = Rows[e.RowIndex].Cells[ColName];
            RowMenu.Show(Cursor.Position);
        }

        void OnToolTipNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var item = Rows[e.RowIndex].Tag as FileItem;
            if (e.ColumnIndex == ColAction && !IsActionEnabled(ActionUnderMouse(e.RowIndex))) e.ToolTipText = DisabledActionTip;
            if (e.ColumnIndex != ColState) return;
            if (item != null) e.ToolTipText = item.JdStatus;
        }

        void ToggleFolder(FolderRow folder)
        {
            if (!collapsed.Remove(folder.Path)) collapsed.Add(folder.Path);
            Rebuild();
        }

        void SetHover(int row)
        {
            if (hoverRow == row) return;
            int previous = hoverRow;
            hoverRow = row;
            if (previous >= 0 && previous < Rows.Count) InvalidateRow(previous);
            if (row >= 0 && row < Rows.Count) InvalidateRow(row);
        }
    }
}
