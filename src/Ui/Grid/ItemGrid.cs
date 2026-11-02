using System;
using System.Collections.Generic;
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
        const int ColName = 0, ColSize = 1, ColProgress = 2, ColState = 3, ColAction = 4;

        readonly HashSet<string> collapsed = new HashSet<string>();
        List<FileItem> items = new List<FileItem>();
        ItemFilter filter = ItemFilter.All;
        int hoverRow = -1;

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
            MouseLeave += (s, e) => SetHover(-1);
            CellToolTipTextNeeded += OnToolTipNeeded;
        }

        public ContextMenuStrip RowMenu { get; set; }

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
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnName, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = Theme.S(200) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnSize, Width = Theme.S(84) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnProgress, Width = Theme.S(110) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = Strings.ColumnState, Width = Theme.S(200) });
            Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "", Width = Theme.S(134), Resizable = DataGridViewTriState.False });
            foreach (DataGridViewColumn column in Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        void ApplyStyle()
        {
            Dock = DockStyle.Fill;
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

            if (e.ColumnIndex == ColAction) RaiseRowAction(tag);
            else if (folder != null && e.ColumnIndex == ColName && ClickedChevron(e.RowIndex, folder)) ToggleFolder(folder);
        }

        bool ClickedChevron(int row, FolderRow folder)
        {
            int left = GetCellDisplayRectangle(ColName, row, false).X;
            return PointToClient(Cursor.Position).X <= left + CellPainter.ChevronEnd(folder.Depth);
        }

        void RaiseRowAction(object tag)
        {
            if (RowActionClicked == null) return;
            var item = tag as FileItem;
            if (item != null)
            {
                RowAction action = ItemPresenter.ActionOf(item);
                if (action != RowAction.None) RowActionClicked(action, new List<FileItem> { item });
                return;
            }
            List<FileItem> group = ItemsIn((FolderRow)tag);
            if (group.Any(i => i.CanSend)) RowActionClicked(RowAction.SendFolder, group);
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
            if (e.RowIndex < 0 || e.ColumnIndex != ColState) return;
            var item = Rows[e.RowIndex].Tag as FileItem;
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
