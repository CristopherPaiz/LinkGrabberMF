using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LinkGrab.Ui
{
    public static class Painter
    {
        public const TextFormatFlags TextFlags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding
            | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;

        const int TrayIconSize = 32;

        public static void FillRounded(Graphics g, Color color, Rectangle area, int radius)
        {
            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundedPath(area, radius))
            using (var brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }
            g.SmoothingMode = previous;
        }

        static GraphicsPath RoundedPath(Rectangle area, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(area.X, area.Y, d, d, 180, 90);
            path.AddArc(area.Right - d, area.Y, d, d, 270, 90);
            path.AddArc(area.Right - d, area.Bottom - d, d, d, 0, 90);
            path.AddArc(area.X, area.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Draws the tray icon, with a red dot when something needs attention.
        public static Icon MakeAppIcon(bool alert)
        {
            using (var bitmap = new Bitmap(TrayIconSize, TrayIconSize))
            using (Graphics g = Graphics.FromImage(bitmap))
            using (var arrow = new Pen(Color.White, 4.5f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Theme.Accent)) g.FillEllipse(brush, 1, 1, 30, 30);
                arrow.StartCap = LineCap.Round;
                arrow.EndCap = LineCap.Round;
                g.DrawLine(arrow, 16, 8, 16, 21);
                g.DrawLine(arrow, 10, 16, 16, 22);
                g.DrawLine(arrow, 22, 16, 16, 22);
                if (alert)
                {
                    using (var brush = new SolidBrush(Theme.Bad)) g.FillEllipse(brush, 17, 0, 15, 15);
                }
                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
    }
}
