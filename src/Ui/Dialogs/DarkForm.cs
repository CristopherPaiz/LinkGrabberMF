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
        const int ButtonGap = 8;
        const int FooterHeight = 70;

        // Left edge of the content, and of labels, whose text starts a little after their edge.
        protected const int Gutter = 20;
        protected const int LabelGutter = 18;

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
                if ((control.AutoSize && !(control is TextBox)) || control is IconButton) continue;
                control.Width = Theme.S(control.Width);
                if (control.Height > TallControl) control.Height = Theme.S(control.Height);
            }
        }

        // Lines buttons up against the bottom right corner; the first one given ends up on the right.
        protected void AddFooter(params IconButton[] buttons)
        {
            var footer = new Panel { Dock = DockStyle.Bottom, Height = Theme.S(FooterHeight) };
            int right = ClientSize.Width - Theme.S(Gutter);
            foreach (IconButton button in buttons)
            {
                button.Location = new Point(right - button.Width, Theme.S(16));
                right = button.Left - Theme.S(ButtonGap);
                footer.Controls.Add(button);
            }
            Controls.Add(footer);
        }

        protected static TextBox MakeTextBox()
        {
            return new TextBox { BackColor = Theme.Panel, ForeColor = Theme.Text, BorderStyle = BorderStyle.FixedSingle };
        }
    }
}
