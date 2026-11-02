using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using LinkGrab.Constants;

namespace LinkGrab.Ui.Dialogs
{
    // Explains what every button, state, filter and shortcut does.
    public class HelpForm : DarkForm
    {
        const int Width_ = 680;
        const int GlyphColumn = 34;
        const int TitleColumn = 190;

        readonly FlowLayoutPanel content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Theme.Back
        };

        public HelpForm() : base(Strings.HelpTitle, Width_, 620)
        {
            content.Padding = new Padding(Theme.S(18), Theme.S(14), Theme.S(18), Theme.S(14));
            content.Controls.Add(MakeParagraph(Strings.HelpIntro));
            AddSection(Strings.HelpButtons, HelpContent.Buttons);
            AddSection(Strings.HelpStates, HelpContent.States);
            AddSection(Strings.HelpFilters, HelpContent.Filters);
            AddSection(Strings.HelpKeys, HelpContent.Keys);
            content.Controls.Add(MakeHeading(Strings.HelpProject));
            content.Controls.Add(MakeRepositoryLink());
            Controls.Add(content);
        }

        static int ContentWidth { get { return Theme.S(Width_ - 60); } }

        void AddSection(string title, HelpEntry[] entries)
        {
            content.Controls.Add(MakeHeading(title));
            foreach (HelpEntry entry in entries) content.Controls.Add(MakeRow(entry));
        }

        static Label MakeHeading(string title)
        {
            return new Label { Text = title, Font = Theme.Brand, AutoSize = true, Margin = new Padding(0, Theme.S(16), 0, Theme.S(6)) };
        }

        static LinkLabel MakeRepositoryLink()
        {
            var link = new LinkLabel
            {
                Text = AppInfo.Repository,
                AutoSize = true,
                TabStop = false,
                Margin = new Padding(0, 0, 0, Theme.S(24)),
                LinkColor = Theme.Accent,
                ActiveLinkColor = Theme.Accent,
                VisitedLinkColor = Theme.Accent
            };
            link.LinkClicked += (s, e) => Process.Start(AppInfo.Repository);
            return link;
        }

        static Label MakeParagraph(string text)
        {
            return new Label { Text = text, AutoSize = true, MaximumSize = new Size(ContentWidth, 0) };
        }

        static Control MakeRow(HelpEntry entry)
        {
            var row = new TableLayoutPanel { AutoSize = true, ColumnCount = 3, Width = ContentWidth, Margin = new Padding(0, 0, 0, Theme.S(6)) };
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.S(GlyphColumn)));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.S(TitleColumn)));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            row.Controls.Add(new Label { Text = entry.Glyph ?? "", Font = Theme.Icons, ForeColor = Theme.Accent, AutoSize = true }, 0, 0);
            row.Controls.Add(new Label { Text = entry.Title, Font = Theme.Bold, AutoSize = true, MaximumSize = new Size(Theme.S(TitleColumn - 8), 0) }, 1, 0);
            row.Controls.Add(new Label { Text = entry.Text, ForeColor = Theme.Muted, AutoSize = true, MaximumSize = new Size(ContentWidth - Theme.S(GlyphColumn + TitleColumn), 0) }, 2, 0);
            return row;
        }
    }
}
