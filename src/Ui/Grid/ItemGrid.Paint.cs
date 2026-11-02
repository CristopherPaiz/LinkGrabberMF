using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;

namespace LinkGrab.Ui.Grid
{
    public partial class ItemGrid
    {
        void PaintEmptyState(object sender, PaintEventArgs e)
        {
            if (Rows.Count > 0) return;
            string text = items.Count == 0 ? Strings.EmptyList : Strings.EmptyFilter;
            TextRenderer.DrawText(e.Graphics, text, Theme.Normal, ClientRectangle, Theme.Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        void PaintCell(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                PaintHeader(e);
                return;
            }

            DataGridViewRow row = Rows[e.RowIndex];
            if (row.Tag == null) return;

            Color back = row.Selected ? Theme.Selected : e.RowIndex == hoverRow ? Theme.Hover : Theme.Panel;
            using (var brush = new SolidBrush(back)) e.Graphics.FillRectangle(brush, e.CellBounds);
            DrawBottomLine(e);

            var folder = row.Tag as FolderRow;
            if (e.ColumnIndex == ColCheck) CellPainter.CheckBox(e.Graphics, e.CellBounds, row.Selected, false);
            else if (folder != null) PaintFolderCell(e, folder);
            else PaintItemCell(e, (FileItem)row.Tag);
            e.Handled = true;
        }

        void PaintHeader(DataGridViewCellPaintingEventArgs e)
        {
            using (var brush = new SolidBrush(Theme.Panel)) e.Graphics.FillRectangle(brush, e.CellBounds);
            if (e.ColumnIndex == ColCheck)
            {
                bool all = Rows.Count > 0 && SelectedRows.Count == Rows.Count;
                CellPainter.CheckBox(e.Graphics, e.CellBounds, all, !all && SelectedRows.Count > 0);
                var arrow = new Rectangle(e.CellBounds.X + Theme.S(HeaderCheckWidth), e.CellBounds.Y, e.CellBounds.Width - Theme.S(HeaderCheckWidth), e.CellBounds.Height);
                TextRenderer.DrawText(e.Graphics, Glyph.Down, Theme.IconsSmall, arrow, Theme.Muted, Painter.TextFlags);
                DrawBottomLine(e);
                e.Handled = true;
                return;
            }

            if (e.ColumnIndex == ColAction)
            {
                bool enabled = CopyTargets().Count > 0;
                Rectangle button = CellPainter.ActionAreas(e.CellBounds, new[] { CopyAllLabel() })[0];
                if (copiedAll > 0) CellPainter.Confirmed(e.Graphics, button, CopyAllLabel());
                else CellPainter.CopyAll(e.Graphics, button, CopyAllLabel(), button.Contains(PointToClient(Cursor.Position)), enabled);
                DrawBottomLine(e);
                e.Handled = true;
                return;
            }

            Rectangle area = e.CellBounds;
            area.X += Theme.S(10);
            TextRenderer.DrawText(e.Graphics, e.FormattedValue as string, Theme.Small, area, Theme.Muted, Painter.TextFlags);
            DrawBottomLine(e);
            e.Handled = true;
        }

        static void DrawBottomLine(DataGridViewCellPaintingEventArgs e)
        {
            using (var pen = new Pen(Theme.Line)) e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Bottom - 1, e.CellBounds.Right, e.CellBounds.Bottom - 1);
        }

        void PaintFolderCell(DataGridViewCellPaintingEventArgs e, FolderRow folder)
        {
            List<FileItem> group = ItemsIn(folder);
            Rectangle r = e.CellBounds;
            switch (e.ColumnIndex)
            {
                case ColName:
                    PaintFolderName(e.Graphics, r, folder, group.Count);
                    break;
                case ColSize:
                    CellPainter.RightText(e.Graphics, Theme.FormatSize(group.Sum(i => i.Size)), r, Theme.Muted);
                    break;
                case ColProgress:
                    List<FileItem> downloads = group.Where(i => i.JdWhere == JdLocation.Downloads).ToList();
                    if (downloads.Count > 0) CellPainter.Progress(e.Graphics, r, (int)downloads.Average(i => i.JdFinished ? 100 : i.JdProgress), downloads.All(i => i.JdFinished));
                    break;
                case ColState:
                    int problems = group.Count(i => i.NeedsAttention);
                    string text = problems > 0 ? string.Format(Strings.FolderProblems, problems) : string.Format(Strings.FolderDone, group.Count(i => i.JdFinished), group.Count);
                    CellPainter.State(e.Graphics, r, text, problems > 0 ? Theme.Warn : Theme.Muted);
                    break;
                case ColAction:
                    PaintActions(e, ItemPresenter.ActionsOf(group), null);
                    break;
            }
        }

        void PaintFolderName(Graphics g, Rectangle r, FolderRow folder, int count)
        {
            int x = r.X + CellPainter.IndentOf(folder.Depth);
            string chevron = collapsed.Contains(folder.Path) ? Glyph.Right : Glyph.Down;
            TextRenderer.DrawText(g, chevron, Theme.IconsSmall, new Rectangle(x, r.Y, Theme.S(16), r.Height), Theme.Muted, Painter.TextFlags);
            CellPainter.Folder(g, x + Theme.S(19), r.Y + (r.Height - Theme.S(14)) / 2);

            int textX = x + Theme.S(42);
            int width = TextRenderer.MeasureText(folder.Name, Theme.Bold, Size.Empty, TextFormatFlags.NoPrefix).Width;
            TextRenderer.DrawText(g, folder.Name, Theme.Bold, new Rectangle(textX, r.Y, r.Right - textX, r.Height), Theme.Text, Painter.TextFlags);
            TextRenderer.DrawText(g, count.ToString(), Theme.Small, new Rectangle(textX + width + Theme.S(6), r.Y, Theme.S(50), r.Height), Theme.Muted, Painter.TextFlags);
        }

        void PaintItemCell(DataGridViewCellPaintingEventArgs e, FileItem item)
        {
            Rectangle r = e.CellBounds;
            switch (e.ColumnIndex)
            {
                case ColName:
                    int x = r.X + CellPainter.ChevronEnd(FolderPaths.Depth(item.Folder));
                    TextRenderer.DrawText(e.Graphics, Glyph.File, Theme.Icons, new Rectangle(x, r.Y, Theme.S(20), r.Height), Theme.Muted, Painter.TextFlags);
                    TextRenderer.DrawText(e.Graphics, item.Name, Theme.Normal, new Rectangle(x + Theme.S(24), r.Y, r.Right - x - Theme.S(28), r.Height), Theme.Text, Painter.TextFlags);
                    break;
                case ColSize:
                    CellPainter.RightText(e.Graphics, Theme.FormatSize(item.Size), r, Theme.Muted);
                    break;
                case ColProgress:
                    if (item.JdWhere == JdLocation.Downloads) CellPainter.Progress(e.Graphics, r, item.JdFinished ? 100 : item.JdProgress, item.JdFinished);
                    break;
                case ColState:
                    ItemState state = ItemPresenter.StateOf(item);
                    CellPainter.State(e.Graphics, r, state.Text, state.Color);
                    break;
                case ColAction:
                    PaintActions(e, ItemPresenter.ActionsOf(item), item);
                    break;
            }
        }

        void PaintActions(DataGridViewCellPaintingEventArgs e, List<RowAction> actions, FileItem item)
        {
            List<Rectangle> areas = CellPainter.ActionAreas(e.CellBounds, actions.Select(a => LabelOf(a, item)));
            Point mouse = PointToClient(Cursor.Position);
            for (int i = 0; i < actions.Count; i++)
            {
                if (IsConfirming(actions[i], item))
                {
                    CellPainter.Confirmed(e.Graphics, areas[i], Strings.ActionCopied);
                    continue;
                }
                bool hover = e.RowIndex == hoverRow && areas[i].Contains(mouse);
                CellPainter.Action(e.Graphics, areas[i], ItemPresenter.LabelOf(actions[i]), hover, IsActionEnabled(actions[i]));
            }
        }
    }
}
