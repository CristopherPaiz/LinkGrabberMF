using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    public class ErrorForm : DarkForm
    {
        const int FormWidth = 640;

        readonly IconButton copy = new IconButton(Strings.ButtonCopy, null, true);
        readonly string report;

        public ErrorForm(string report, bool fatal) : base(Strings.ErrorTitle, FormWidth, 420)
        {
            this.report = report;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;

            var box = MakeTextBox();
            box.Text = report;
            box.ReadOnly = true;
            box.Multiline = true;
            box.WordWrap = false;
            box.ScrollBars = ScrollBars.Both;
            box.Left = Gutter;
            box.Top = 92;
            box.Width = FormWidth - Gutter * 2;
            box.Height = 256;
            box.HandleCreated += (s, e) => Theme.DarkScrollBars(box);

            copy.Click += (s, e) => CopyReport();
            var close = new IconButton(fatal ? Strings.ButtonExit : Strings.ButtonContinue) { DialogResult = DialogResult.Cancel };
            CancelButton = close;

            Controls.AddRange(new Control[]
            {
                new Label { Text = fatal ? Strings.ErrorHeadingFatal : Strings.ErrorHeading, Font = Theme.Bold, Left = LabelGutter, Top = 18, AutoSize = true },
                new Label { Text = Strings.ErrorPrompt, ForeColor = Theme.Muted, Left = LabelGutter, Top = 46, Width = FormWidth - Gutter * 2, Height = 40 },
                box
            });
            AddFooter(copy, close);
            Shown += (s, e) => copy.Focus();
        }

        void CopyReport()
        {
            try
            {
                Clipboard.SetText(report);
                copy.Text = Strings.ErrorCopied;
                copy.Invalidate();
            }
            catch (ExternalException)
            {
                copy.Text = Strings.ErrorCopyFailed;
                copy.Fit();
                copy.Invalidate();
            }
        }

        // Showing the error must never be what fails.
        public static void Present(string report, bool fatal)
        {
            try
            {
                using (var form = new ErrorForm(report, fatal)) form.ShowDialog();
            }
            catch (Exception)
            {
                MessageBox.Show(report, Strings.ErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
