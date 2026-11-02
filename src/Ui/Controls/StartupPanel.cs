using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Constants;

namespace LinkGrab.Ui.Controls
{
    // Covers the window until the first check of JD2 says which buttons can be used.
    public class StartupPanel : Panel
    {
        const int IconSize = 48;
        const int BarWidth = 220;

        readonly Icon icon;
        readonly ActivityBar bar = new ActivityBar { Active = true };

        public StartupPanel(Icon icon)
        {
            this.icon = icon;
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BackColor = Theme.Back;
            DoubleBuffered = true;
            bar.Size = new Size(Theme.S(BarWidth), Theme.S(3));
            Controls.Add(bar);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            bar.Location = new Point((Width - bar.Width) / 2, Height / 2 + Theme.S(66));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int size = Theme.S(IconSize);
            int top = Height / 2 - Theme.S(70);
            e.Graphics.DrawIcon(icon, new Rectangle((Width - size) / 2, top, size, size));

            DrawCentered(e.Graphics, AppInfo.Name, Theme.Brand, Theme.Text, top + size + Theme.S(10));
            DrawCentered(e.Graphics, Strings.StartupChecking, Theme.Normal, Theme.Muted, top + size + Theme.S(44));
        }

        void DrawCentered(Graphics g, string text, Font font, Color color, int y)
        {
            TextRenderer.DrawText(g, text, font, new Rectangle(0, y, Width, Theme.S(30)), color, Painter.TextFlags | TextFormatFlags.HorizontalCenter);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) bar.Active = false;
            base.Dispose(disposing);
        }
    }
}
