using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Constants;

namespace LinkGrab.Ui.Controls
{
    // Thin animated bar shown while the app or JD2 is working.
    public class ActivityBar : Control
    {
        readonly Timer tick = new Timer { Interval = Timing.ActivityFrameMs };
        int position;
        bool active;

        public ActivityBar()
        {
            DoubleBuffered = true;
            tick.Tick += (s, e) =>
            {
                position = (position + Theme.S(8)) % (Width + SlideWidth);
                Invalidate();
            };
        }

        static int SlideWidth { get { return Theme.S(200); } }

        public bool Active
        {
            get { return active; }
            set
            {
                if (active == value) return;
                active = value;
                tick.Enabled = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Theme.Line);
            if (!active) return;
            using (var brush = new SolidBrush(Theme.Accent)) e.Graphics.FillRectangle(brush, position - SlideWidth, 0, SlideWidth, Height);
        }
    }
}
