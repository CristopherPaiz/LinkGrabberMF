using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinkGrab.Ui.Controls
{
    // Filter entry in the sidebar.
    public class NavItem : Control
    {
        readonly string glyph;
        bool hovering;
        bool active;
        int count;

        public NavItem(string text, string glyph)
        {
            this.glyph = glyph;
            Text = text;
            Height = Theme.S(36);
            Dock = DockStyle.Top;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        public Color? CountColor { get; set; }

        public bool Active
        {
            get { return active; }
            set
            {
                active = value;
                Invalidate();
            }
        }

        public int Count
        {
            get { return count; }
            set
            {
                if (count == value) return;
                count = value;
                Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovering = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Theme.Side);
            int margin = Theme.S(8);
            if (active || hovering)
            {
                var area = new Rectangle(margin, Theme.S(2), Width - margin * 2, Height - Theme.S(4));
                Painter.FillRounded(g, active ? Theme.Selected : Theme.Hover, area, Theme.S(5));
            }

            TextRenderer.DrawText(g, glyph, Theme.Icons, new Rectangle(Theme.S(18), 0, Theme.S(22), Height), active ? Theme.Accent : Theme.Muted, Painter.TextFlags);
            TextRenderer.DrawText(g, Text, active ? Theme.Bold : Theme.Normal, new Rectangle(Theme.S(44), 0, Width - Theme.S(44), Height), Theme.Text, Painter.TextFlags);
            if (count > 0)
            {
                TextRenderer.DrawText(g, count.ToString(), Theme.Small, new Rectangle(0, 0, Width - Theme.S(18), Height), CountColor ?? Theme.Muted, Painter.TextFlags | TextFormatFlags.Right);
            }
        }
    }
}
