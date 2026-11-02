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
            AddSection(Strings.HelpRequirements, HelpContent.Requirements);
            content.Controls.Add(MakeHeading(Strings.HelpProject));
            content.Controls.Add(MakeLink(AppInfo.Repository, AppInfo.Repository, new Padding(0, 0, 0, Theme.S(24))));
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

        static LinkLabel MakeLink(string text, string url, Padding margin)
        {
            var link = new LinkLabel
            {
                Text = text,
                AutoSize = true,
                TabStop = false,
                Margin = margin,
                LinkColor = Theme.Accent,
                ActiveLinkColor = Theme.Accent,
                VisitedLinkColor = Theme.Accent
            };
            link.LinkClicked += (s, e) => Process.Start(url);
            return link;
        }

        // The explanation, with the page first and the direct download after it.
        static Control MakeDescription(HelpEntry entry, int width)
        {
            var text = new Label { Text = entry.Text, ForeColor = Theme.Muted, AutoSize = true, MaximumSize = new Size(width, 0) };
            if (entry.PageUrl == null && entry.DirectUrl == null) return text;

            var links = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, Theme.S(2), 0, 0) };
            if (entry.PageUrl != null) links.Controls.Add(MakeLink(Strings.LinkPage, entry.PageUrl, new Padding(0, 0, Theme.S(14), 0)));
            if (entry.DirectUrl != null) links.Controls.Add(MakeLink(Strings.LinkDirect, entry.DirectUrl, new Padding(0)));

            var stack = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Margin = new Padding(0) };
            stack.Controls.Add(text);
            stack.Controls.Add(links);
            return stack;
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
            row.Controls.Add(MakeDescription(entry, ContentWidth - Theme.S(GlyphColumn + TitleColumn)), 2, 0);
            return row;
        }
    }
}
