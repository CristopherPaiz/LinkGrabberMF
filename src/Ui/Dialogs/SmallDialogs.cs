using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    public class AddLinksForm : DarkForm
    {
        readonly TextBox box = MakeTextBox();

        public string Value { get { return box.Text; } }

        public AddLinksForm() : base(Strings.AddTitle, 620, 330)
        {
            box.Multiline = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.Left = 18;
            box.Top = 44;
            box.Width = 584;
            box.Height = 220;

            var add = new IconButton(Strings.ButtonAdd, Glyph.Add, true) { Left = 18, Top = 278 };
            add.Click += (s, e) => DialogResult = DialogResult.OK;

            Controls.AddRange(new Control[]
            {
                new Label { Text = Strings.AddPrompt, Left = 18, Top = 16, AutoSize = true },
                box,
                add
            });
        }
    }

    public class InputForm : DarkForm
    {
        readonly TextBox box = MakeTextBox();

        public string Value { get { return box.Text.Trim(); } }

        public InputForm(string title, string label, string value) : base(title, 460, 130)
        {
            box.Text = value;
            box.Left = 18;
            box.Top = 42;
            box.Width = 424;

            var save = new IconButton(Strings.ButtonAccept, null, true) { Left = 18, Top = 82 };
            save.Click += (s, e) => DialogResult = DialogResult.OK;
            AcceptButton = save;

            Controls.AddRange(new Control[] { new Label { Text = label, Left = 18, Top = 16, AutoSize = true }, box, save });
        }
    }

    public class CloseForm : DarkForm
    {
        readonly CheckBox remember = new CheckBox { Text = Strings.CloseRemember, Left = 18, Top = 66, AutoSize = true };

        public CloseAction Choice { get; private set; }
        public bool Remember { get { return remember.Checked; } }

        public CloseForm() : base(Strings.CloseTitle, 480, 170)
        {
            var message = new Label { Text = Strings.CloseMessage, Left = 18, Top = 16, Width = 444, Height = 44 };
            var row = new FlowLayoutPanel { Left = 18, Top = 112, Width = 450, Height = 40 };
            row.Controls.Add(MakeChoice(Strings.CloseTray, CloseAction.Tray, true));
            row.Controls.Add(MakeChoice(Strings.CloseMinimize, CloseAction.Minimize, false));
            row.Controls.Add(MakeChoice(Strings.CloseExit, CloseAction.Exit, false));
            Controls.AddRange(new Control[] { message, remember, row });
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
