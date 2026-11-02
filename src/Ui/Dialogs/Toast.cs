using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Constants;

namespace LinkGrab.Ui.Dialogs
{
    // Bottom-right notification that never takes focus away from what you are doing.
    public class Toast : Form
    {
        const int WsExToolWindow = 0x80, WsExNoActivate = 0x8000000, WsExTopmost = 0x8;
        const int ScreenGap = 10;
        const int ToastWidth = 360;
        const int MaxVisible = 3;
        const int TextLeft = 46;
        static readonly List<Toast> Open = new List<Toast>();

        // Where notifications appear; the main window points this at its own screen.
        public static Func<Rectangle> WorkArea { get; set; }

        readonly Timer life = new Timer { Interval = Timing.ToastLifeMs };

        Toast(string glyph, string heading, string body, string action, Color accent, Action onClick)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(Theme.S(ToastWidth), Theme.S(122));
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            Font = Theme.Normal;
            Cursor = Cursors.Hand;

            var bar = new Panel { BackColor = accent, Dock = DockStyle.Left, Width = Theme.S(5) };
            var icon = new Label { Text = glyph, Font = Theme.Icons, ForeColor = accent, Left = Theme.S(16), Top = Theme.S(34), AutoSize = true };
            var close = new Label { Text = Glyph.Gone, Font = Theme.IconsSmall, ForeColor = Theme.Muted, Left = Theme.S(ToastWidth - 26), Top = Theme.S(10), AutoSize = true };

            Controls.AddRange(new Control[]
            {
                bar, icon, close,
                MakeLine(AppInfo.Name, Theme.Small, Theme.Muted, 10, 18),
                MakeLine(heading, Theme.Bold, Theme.Text, 30, 22),
                MakeLine(body, Theme.Normal, Theme.Muted, 52, 40),
                MakeLine(action, Theme.Small, accent, 94, 18)
            });

            EventHandler click = (s, e) =>
            {
                Close();
                onClick();
            };
            Click += click;
            foreach (Control control in Controls)
            {
                if (control != close) control.Click += click;
            }
            close.Click += (s, e) => Close();

            life.Tick += (s, e) => Close();
            FormClosed += (s, e) =>
            {
                life.Dispose();
                Open.Remove(this);
                Arrange();
            };
        }

        static Label MakeLine(string text, Font font, Color color, int top, int height)
        {
            return new Label
            {
                UseMnemonic = false,
                AutoEllipsis = true,
                Text = text,
                Font = font,
                ForeColor = color,
                Left = Theme.S(TextLeft),
                Top = Theme.S(top),
                Width = Theme.S(ToastWidth - TextLeft - 30),
                Height = Theme.S(height)
            };
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams options = base.CreateParams;
                options.ExStyle |= WsExToolWindow | WsExNoActivate | WsExTopmost;
                return options;
            }
        }

        public static void Show(string glyph, string heading, string body, string action, Color accent, Action onClick)
        {
            while (Open.Count >= MaxVisible) Open[0].Close();
            var toast = new Toast(glyph, heading, body, action, accent, onClick);
            Open.Add(toast);
            Arrange();
            toast.Show();
            toast.life.Start();
        }

        // Bottom-right corner of the given screen area, leaving room for the notifications below it.
        public static Point Place(Rectangle area, Size size, int heightBelow)
        {
            int gap = Theme.S(ScreenGap);
            return new Point(area.Right - size.Width - gap, area.Bottom - size.Height - gap - heightBelow);
        }

        static void Arrange()
        {
            Rectangle area = WorkArea();
            int below = 0;
            foreach (Toast toast in Open)
            {
                toast.Location = Place(area, toast.Size, below);
                below += toast.Height + Theme.S(ScreenGap);
            }
        }
    }
}
