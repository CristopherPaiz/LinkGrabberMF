using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinkGrab.Ui.Controls
{
    public class IconButton : Button
    {
        readonly string glyph;
        readonly bool primary;
        bool hovering;

        public Color? Tint { get; set; }

        public IconButton(string text, string glyph = null, bool primary = false)
        {
            this.glyph = glyph;
            this.primary = primary;
            Text = text;
            Font = Theme.Normal;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Margin = new Padding(0, 0, Theme.S(6), 0);
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Fit();
        }

        public void Fit()
        {
            int width = TextRenderer.MeasureText(Text, Font).Width + Theme.S(glyph == null ? 26 : 44);
            Size = new Size(width, Theme.S(34));
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
            g.Clear(Parent != null ? Parent.BackColor : Theme.Back);
            Painter.FillRounded(g, FillColor(), new Rectangle(0, 0, Width - 1, Height - 1), Theme.S(5));

            Color fore = !Enabled ? Theme.Muted : Tint != null ? Theme.Back : primary ? Color.White : Theme.Text;
            int x = Theme.S(12);
            if (glyph != null)
            {
                TextRenderer.DrawText(g, glyph, Theme.Icons, new Rectangle(x, 0, Theme.S(20), Height), fore, Painter.TextFlags);
                x += Theme.S(22);
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, Width - x, Height), fore, Painter.TextFlags);
        }

        Color FillColor()
        {
            if (!Enabled) return Theme.Panel;
            Color fill = Tint ?? (primary ? Theme.Accent : Theme.Panel);
            if (!hovering) return fill;
            return Tint != null || primary ? ControlPaint.Light(fill, 0.15f) : Theme.Hover;
        }
    }
}
