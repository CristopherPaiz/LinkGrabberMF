using System;
using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    // Base for fixed-size dialogs whose child positions are written at 96 DPI.
    public class DarkForm : Form
    {
        const int TallControl = 30;

        public DarkForm(string text, int width, int height)
        {
            Text = text;
            ClientSize = new Size(Theme.S(width), Theme.S(height));
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Back;
            ForeColor = Theme.Text;
            Font = Theme.Normal;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Theme.DarkTitleBar(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            foreach (Control control in Controls)
            {
                if (control.Dock != DockStyle.None) continue;
                control.Left = Theme.S(control.Left);
                control.Top = Theme.S(control.Top);
                if (control.AutoSize || control is IconButton) continue;
                control.Width = Theme.S(control.Width);
                if (control.Height > TallControl) control.Height = Theme.S(control.Height);
            }
        }

        protected static TextBox MakeTextBox()
        {
            return new TextBox { BackColor = Theme.Panel, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
        }
    }
}
