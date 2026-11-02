using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LinkGrab.Ui.Grid
{
    // Drawing primitives for the cells of the item grid.
    public static class CellPainter
    {
        const int IndentStep = 20;
        const int IndentStart = 8;
        const int ChevronWidth = 18;

        public static int IndentOf(int depth)
        {
            return Theme.S(IndentStart + IndentStep * depth);
        }

        public static int ChevronEnd(int depth)
        {
            return Theme.S(IndentStart + IndentStep * depth + ChevronWidth);
        }

        public static void Folder(Graphics g, int x, int y)
        {
            Painter.FillRounded(g, Theme.FolderTab, new Rectangle(x, y, Theme.S(9), Theme.S(6)), Theme.S(2));
            Painter.FillRounded(g, Theme.FolderTint, new Rectangle(x, y + Theme.S(3), Theme.S(18), Theme.S(11)), Theme.S(2));
        }

        public static void RightText(Graphics g, string text, Rectangle r, Color color)
        {
            Rectangle area = r;
            area.Width -= Theme.S(10);
            TextRenderer.DrawText(g, text, Theme.Small, area, color, Painter.TextFlags | TextFormatFlags.Right);
        }

        public static void Progress(Graphics g, Rectangle r, int percent, bool finished)
        {
            int height = Theme.S(5);
            var track = new Rectangle(r.X + Theme.S(10), r.Y + (r.Height - height) / 2, r.Width - Theme.S(52), height);
            Painter.FillRounded(g, Theme.Line, track, height / 2);
            int fill = track.Width * Math.Min(100, percent) / 100;
            if (fill >= height) Painter.FillRounded(g, finished ? Theme.Good : Theme.Accent, new Rectangle(track.X, track.Y, fill, height), height / 2);
            TextRenderer.DrawText(g, percent + "%", Theme.Small, new Rectangle(track.Right + Theme.S(4), r.Y, Theme.S(38), r.Height), Theme.Muted, Painter.TextFlags);
        }

        public static void State(Graphics g, Rectangle r, string text, Color color)
        {
            int dot = Theme.S(7);
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(color)) g.FillEllipse(brush, r.X + Theme.S(10), r.Y + (r.Height - dot) / 2, dot, dot);
            g.SmoothingMode = previous;
            TextRenderer.DrawText(g, text, Theme.Small, new Rectangle(r.X + Theme.S(24), r.Y, r.Width - Theme.S(28), r.Height), color, Painter.TextFlags);
        }

        public static void Action(Graphics g, Rectangle r, string text, bool hover)
        {
            var area = new Rectangle(r.X + Theme.S(4), r.Y + Theme.S(5), r.Width - Theme.S(14), r.Height - Theme.S(10));
            Painter.FillRounded(g, hover ? Theme.Accent : Theme.Line, area, Theme.S(4));
            TextRenderer.DrawText(g, text, Theme.Small, area, hover ? Color.White : Theme.Text, Painter.TextFlags | TextFormatFlags.HorizontalCenter);
        }
    }
}
