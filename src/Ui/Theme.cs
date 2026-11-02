using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LinkGrab.Ui
{
    public static class Theme
    {
        const int DwmUseDarkMode = 20;
        const double BytesPerUnit = 1024;
        static readonly string[] SizeUnits = { "B", "KB", "MB", "GB", "TB" };

        public static readonly Color Back = Color.FromArgb(17, 19, 24);
        public static readonly Color Side = Color.FromArgb(23, 25, 31);
        public static readonly Color Panel = Color.FromArgb(29, 32, 39);
        public static readonly Color Hover = Color.FromArgb(38, 42, 51);
        public static readonly Color Selected = Color.FromArgb(36, 56, 94);
        public static readonly Color Line = Color.FromArgb(44, 48, 58);
        public static readonly Color Text = Color.FromArgb(232, 234, 238);
        public static readonly Color Muted = Color.FromArgb(146, 152, 164);
        public static readonly Color Accent = Color.FromArgb(79, 140, 255);
        public static readonly Color Good = Color.FromArgb(87, 200, 130);
        public static readonly Color Warn = Color.FromArgb(245, 170, 70);
        public static readonly Color Bad = Color.FromArgb(240, 100, 100);
        public static readonly Color FolderTint = Color.FromArgb(240, 196, 92);
        public static readonly Color FolderTab = Color.FromArgb(200, 158, 62);

        public static readonly Font Normal = new Font("Segoe UI", 9.5f);
        public static readonly Font Small = new Font("Segoe UI", 8.5f);
        public static readonly Font Bold = new Font("Segoe UI Semibold", 9.5f);
        public static readonly Font Brand = new Font("Segoe UI Semibold", 14f);
        public static readonly Font Icons = new Font("Segoe MDL2 Assets", 10.5f);
        public static readonly Font IconsSmall = new Font("Segoe MDL2 Assets", 8f);

        static readonly float Factor = ReadFactor();

        static float ReadFactor()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) return g.DpiX / 96f;
        }

        // Converts a size written at 96 DPI to this screen's scale.
        public static int S(int value)
        {
            return (int)Math.Round(value * Factor);
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        public static void DarkTitleBar(Form form)
        {
            int enabled = 1;
            DwmSetWindowAttribute(form.Handle, DwmUseDarkMode, ref enabled, sizeof(int));
        }

        public static string FormatSize(long bytes)
        {
            if (bytes <= 0) return "";
            double value = bytes;
            int unit = 0;
            while (value >= BytesPerUnit && unit < SizeUnits.Length - 1)
            {
                value /= BytesPerUnit;
                unit++;
            }
            return value.ToString(unit == 0 ? "0" : "0.0") + " " + SizeUnits[unit];
        }
    }

    public static class Glyph
    {
        public const string Add = "", Send = "", Link = "", Check = "", Play = "", Stop = "";
        public const string Delete = "", Rename = "", Sync = "", Settings = "", File = "", Help = "";
        public const string Right = "", Down = "", Warning = "", List = "", Done = "", Download = "", Gone = "";
    }
}
