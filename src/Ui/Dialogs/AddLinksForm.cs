using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Services;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    public class AddLinksForm : DarkForm
    {
        readonly TextBox box = new TextBox
        {
            Multiline = true,
            AcceptsReturn = true,
            ScrollBars = ScrollBars.Vertical,
            WordWrap = false,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            BackColor = Theme.Panel,
            ForeColor = Theme.Text
        };
        readonly Label placeholder = new Label { Text = Strings.AddPlaceholder, ForeColor = Theme.Muted, BackColor = Theme.Panel, AutoSize = true };
        readonly Label found = new Label { Font = Theme.Small, AutoSize = true, Anchor = AnchorStyles.Left };
        readonly Panel border = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Line, Padding = new Padding(1), Margin = new Padding(0, Theme.S(14), 0, Theme.S(14)) };
        readonly IconButton add = new IconButton(Strings.ButtonAdd, Glyph.Add, true) { Margin = new Padding(0) };

        public string Value { get { return box.Text; } }

        public AddLinksForm() : base(Strings.AddTitle, 640, 400)
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(Theme.S(20)) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            root.Controls.Add(new Label { Text = Strings.AddHeading, Font = Theme.Brand, AutoSize = true, Margin = new Padding(0, 0, 0, Theme.S(4)) }, 0, 0);
            root.Controls.Add(new Label { Text = Strings.AddPrompt, ForeColor = Theme.Muted, AutoSize = true, Margin = new Padding(0) }, 0, 1);
            root.Controls.Add(BuildEditor(), 0, 2);
            root.Controls.Add(BuildFooter(), 0, 3);
            Controls.Add(root);

            box.HandleCreated += (s, e) => Theme.DarkScrollBars(box);
            box.TextChanged += (s, e) => UpdateCount();
            box.KeyDown += OnBoxKey;
            box.Enter += (s, e) => border.BackColor = Theme.Accent;
            box.Leave += (s, e) => border.BackColor = Theme.Line;
            placeholder.Click += (s, e) => box.Focus();
            add.Click += (s, e) => DialogResult = DialogResult.OK;
            Shown += (s, e) => box.Focus();
            UpdateCount();
        }

        Control BuildEditor()
        {
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Panel, Padding = new Padding(Theme.S(10)) };
            placeholder.Location = new Point(Theme.S(10), Theme.S(10));
            inner.Controls.Add(placeholder);
            inner.Controls.Add(box);
            border.Controls.Add(inner);
            return border;
        }

        Control BuildFooter()
        {
            var cancel = new IconButton(Strings.ButtonCancel) { DialogResult = DialogResult.Cancel };
            CancelButton = cancel;

            var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(add);

            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.Controls.Add(found, 0, 0);
            footer.Controls.Add(buttons, 1, 0);
            return footer;
        }

        void UpdateCount()
        {
            int count = LinkText.Urls(box.Text).Count;
            placeholder.Visible = box.TextLength == 0;
            add.Enabled = count > 0;
            found.ForeColor = count > 0 ? Theme.Good : Theme.Muted;
            found.Text = count == 0 ? Strings.AddCountNone : count == 1 ? Strings.AddCountOne : string.Format(Strings.AddCountMany, count);
        }

        void OnBoxKey(object source, KeyEventArgs e)
        {
            if (!e.Control) return;
            if (e.KeyCode == Keys.A)
            {
                box.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                if (add.Enabled) DialogResult = DialogResult.OK;
            }
        }
    }
}
