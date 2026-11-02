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
        static readonly List<Toast> Open = new List<Toast>();

        readonly Timer life = new Timer { Interval = Timing.ToastLifeMs };

        Toast(string heading, string body, Color accent, Action onClick)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(Theme.S(360), Theme.S(92));
            BackColor = Theme.Panel;
            ForeColor = Theme.Text;
            Font = Theme.Normal;
            Cursor = Cursors.Hand;

            var bar = new Panel { BackColor = accent, Dock = DockStyle.Left, Width = Theme.S(5) };
            var top = new Label { UseMnemonic = false, Text = heading, Font = Theme.Bold, Left = Theme.S(18), Top = Theme.S(12), Width = Theme.S(330), Height = Theme.S(22), AutoEllipsis = true };
            var text = new Label { UseMnemonic = false, Text = body, ForeColor = Theme.Muted, Left = Theme.S(18), Top = Theme.S(36), Width = Theme.S(330), Height = Theme.S(46), AutoEllipsis = true };
            Controls.AddRange(new Control[] { bar, top, text });

            EventHandler click = (s, e) =>
            {
                Close();
                onClick();
            };
            Click += click;
            top.Click += click;
            text.Click += click;

            life.Tick += (s, e) => Close();
            FormClosed += (s, e) =>
            {
                life.Dispose();
                Open.Remove(this);
                Arrange();
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

        public static void Show(string heading, string body, Color accent, Action onClick)
        {
            var toast = new Toast(heading, body, accent, onClick);
            Open.Add(toast);
            Arrange();
            toast.Show();
            toast.life.Start();
        }

        static void Arrange()
        {
            Rectangle area = Screen.PrimaryScreen.WorkingArea;
            int bottom = area.Bottom - ScreenGap;
            foreach (Toast toast in Open)
            {
                toast.Location = new Point(area.Right - toast.Width - ScreenGap, bottom - toast.Height);
                bottom -= toast.Height + ScreenGap;
            }
        }
    }
}
