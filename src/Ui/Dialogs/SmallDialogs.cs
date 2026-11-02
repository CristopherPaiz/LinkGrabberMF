using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    public class InputForm : DarkForm
    {
        const int FormWidth = 460;

        readonly TextBox box = MakeTextBox();

        public string Value { get { return box.Text.Trim(); } }

        public InputForm(string title, string label, string value) : base(title, FormWidth, 150)
        {
            box.Text = value;
            box.Left = Gutter;
            box.Top = 44;
            box.Width = FormWidth - Gutter * 2;

            var save = new IconButton(Strings.ButtonAccept, null, true);
            save.Click += (s, e) => DialogResult = DialogResult.OK;
            AcceptButton = save;
            var cancel = new IconButton(Strings.ButtonCancel) { DialogResult = DialogResult.Cancel };
            CancelButton = cancel;

            Controls.AddRange(new Control[] { new Label { Text = label, Left = LabelGutter, Top = 18, AutoSize = true }, box });
            AddFooter(save, cancel);
        }
    }

    public class CloseForm : DarkForm
    {
        const int FormWidth = 520;

        readonly CheckBox remember = new CheckBox { Text = Strings.CloseRemember, Left = Gutter, Top = 68, AutoSize = true };

        public CloseAction Choice { get; private set; }
        public bool Remember { get { return remember.Checked; } }

        public CloseForm() : base(Strings.CloseTitle, FormWidth, 170)
        {
            var message = new Label { Text = Strings.CloseMessage, Left = LabelGutter, Top = 18, Width = FormWidth - Gutter * 2, Height = 44 };
            Controls.AddRange(new Control[] { message, remember });
            AddFooter(
                MakeChoice(Strings.CloseTray, CloseAction.Tray, true),
                MakeChoice(Strings.CloseMinimize, CloseAction.Minimize, false),
                MakeChoice(Strings.CloseExit, CloseAction.Exit, false));
        }

        IconButton MakeChoice(string text, CloseAction action, bool primary)
        {
            var button = new IconButton(text, null, primary);
            button.Click += (s, e) =>
            {
                Choice = action;
                DialogResult = DialogResult.OK;
            };
            return button;
        }
    }
}
