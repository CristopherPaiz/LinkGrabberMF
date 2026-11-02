using System;
using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Constants;

namespace LinkGrab.Ui.Controls
{
    public class IconButton : Button
    {
        const float HueRest = 0.16f;
        const float HueHover = 0.30f;
        const int SidePadding = 14;
        const int GlyphColumn = 24;
        const int MinWidth = 84;

        readonly string glyph;
        readonly bool primary;
        bool hovering;
        bool blocked;
        bool flashing;
        string textBeforeFlash;
        Timer flash;

        public Color? Tint { get; set; }

        // Colors the icon and washes the background, keeping the text readable.
        public Color? Hue { get; set; }

        // Looks disabled and ignores clicks, but unlike Enabled = false it can still show a tooltip saying why.
        public bool Blocked
        {
            get { return blocked; }
            set
            {
                if (blocked == value) return;
                blocked = value;
                Cursor = value ? Cursors.Default : Cursors.Hand;
                Invalidate();
            }
        }

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

        // Says for a moment that the click worked, then goes back to what it was.
        public void Flash(string message)
        {
            if (flash == null)
            {
                flash = new Timer { Interval = Timing.ButtonFlashMs };
                flash.Tick += (s, e) => EndFlash();
            }
            if (!flash.Enabled) textBeforeFlash = Text;
            flash.Stop();

            Text = message;
            flashing = true;
            Fit();
            Invalidate();
            flash.Start();
        }

        void EndFlash()
        {
            flash.Stop();
            flashing = false;
            Text = textBeforeFlash;
            Fit();
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && flash != null) flash.Dispose();
            base.Dispose(disposing);
        }

        bool Dimmed { get { return !Enabled || blocked; } }

        public void Fit()
        {
            int width = ContentWidth() + Theme.S(SidePadding) * 2;
            Size = new Size(Math.Max(width, Theme.S(MinWidth)), Theme.S(34));
        }

        int ContentWidth()
        {
            int text = TextRenderer.MeasureText(Text, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            return glyph == null ? text : text + Theme.S(GlyphColumn);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!blocked) base.OnClick(e);
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

            Color fore = flashing ? Theme.Back : Dimmed ? Theme.Muted : Tint != null ? Theme.Back : primary ? Color.White : Theme.Text;
            int x = (Width - ContentWidth()) / 2;
            if (glyph != null)
            {
                Color icon = flashing || Dimmed || Tint != null || Hue == null ? fore : Hue.Value;
                TextRenderer.DrawText(g, glyph, Theme.Icons, new Rectangle(x, 0, Theme.S(20), Height), icon, Painter.TextFlags);
                x += Theme.S(GlyphColumn);
            }
            TextRenderer.DrawText(g, Text, Font, new Rectangle(x, 0, Width - x, Height), fore, Painter.TextFlags);
        }

        Color FillColor()
        {
            if (flashing) return Theme.Good;
            if (Dimmed) return Theme.Panel;
            if (Tint == null && !primary && Hue != null) return Theme.Blend(Theme.Panel, Hue.Value, hovering ? HueHover : HueRest);
            Color fill = Tint ?? (primary ? Theme.Accent : Theme.Panel);
            if (!hovering) return fill;
            return Tint != null || primary ? ControlPaint.Light(fill, 0.15f) : Theme.Hover;
        }
    }
}
