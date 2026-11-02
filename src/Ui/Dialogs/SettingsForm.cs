using System;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    public class SettingsForm : DarkForm
    {
        const int FieldColumn = 240;
        const int BrowseGap = 8;

        static readonly CloseAction[] CloseValues = { CloseAction.Ask, CloseAction.Tray, CloseAction.Minimize, CloseAction.Exit };
        static readonly object[] CloseLabels = { Strings.CloseAsk, Strings.CloseTray, Strings.CloseMinimize, Strings.CloseExit };

        readonly Settings settings;
        readonly CheckBox useJdFolder = new CheckBox { Left = Gutter, Top = 16, AutoSize = true };
        readonly TextBox folder = MakeTextBox();
        readonly IconButton browse = new IconButton(Strings.ButtonBrowse) { Left = Gutter, Top = 44 };
        readonly CheckBox autostart = new CheckBox { Text = Strings.SettingsAutostart, Left = Gutter, Top = 94, AutoSize = true };
        readonly CheckBox autoRenew = new CheckBox { Text = Strings.SettingsAutoRenew, Left = Gutter, Top = 122, AutoSize = true };
        readonly CheckBox notify = new CheckBox { Text = Strings.SettingsNotify, Left = Gutter, Top = 150, AutoSize = true };
        readonly CheckBox watchClipboard = new CheckBox { Text = Strings.SettingsWatchClipboard, Left = Gutter, Top = 178, AutoSize = true };
        readonly NumericUpDown poll = new NumericUpDown { Minimum = Limits.MinPollSeconds, Maximum = Limits.MaxPollSeconds, Left = FieldColumn, Top = 211, Width = 70, BackColor = Theme.Panel, ForeColor = Theme.Text };
        readonly ComboBox onClose = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = FieldColumn, Top = 247, Width = 200, BackColor = Theme.Panel, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };

        public SettingsForm(Settings settings, string jdFolder) : base(Strings.SettingsTitle, 520, 350)
        {
            this.settings = settings;
            useJdFolder.Text = jdFolder != null ? string.Format(Strings.SettingsUseJdFolderAt, jdFolder) : Strings.SettingsUseJdFolder;
            folder.Left = Gutter;
            folder.Top = 49;
            folder.Width = 390;
            onClose.Items.AddRange(CloseLabels);

            LoadValues();
            WireEvents();

            var save = new IconButton(Strings.ButtonSave, null, true);
            save.Click += (s, e) => SaveAndClose();
            var cancel = new IconButton(Strings.ButtonCancel) { DialogResult = DialogResult.Cancel };
            CancelButton = cancel;

            Controls.AddRange(new Control[]
            {
                useJdFolder, folder, browse, autostart, autoRenew, notify, watchClipboard,
                new Label { Text = Strings.SettingsPoll, Left = LabelGutter, Top = 214, AutoSize = true }, poll,
                new Label { Text = Strings.SettingsOnClose, Left = LabelGutter, Top = 250, AutoSize = true }, onClose
            });
            AddFooter(save, cancel);
        }

        // Button widths depend on the screen scale, so the right edge is worked out once everything is scaled.
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int right = ClientSize.Width - Theme.S(Gutter);
            browse.Left = right - browse.Width;
            folder.Width = browse.Left - Theme.S(BrowseGap) - folder.Left;
            onClose.Width = right - onClose.Left;
        }

        void LoadValues()
        {
            useJdFolder.Checked = settings.BaseFolder == "";
            folder.Text = settings.BaseFolder;
            autostart.Checked = settings.Autostart;
            autoRenew.Checked = settings.AutoRenew;
            notify.Checked = settings.Notifications;
            watchClipboard.Checked = settings.WatchClipboard;
            poll.Value = Math.Max(Limits.MinPollSeconds, Math.Min(Limits.MaxPollSeconds, settings.PollSeconds));
            onClose.SelectedIndex = Math.Max(0, Array.IndexOf(CloseValues, settings.CloseAction));
            UpdateFolderControls();
        }

        void WireEvents()
        {
            useJdFolder.CheckedChanged += (s, e) => UpdateFolderControls();
            browse.Click += (s, e) =>
            {
                using (var picker = new FolderBrowserDialog { SelectedPath = folder.Text })
                {
                    if (picker.ShowDialog(this) == DialogResult.OK) folder.Text = picker.SelectedPath;
                }
            };
        }

        void UpdateFolderControls()
        {
            folder.Enabled = !useJdFolder.Checked;
            browse.Enabled = !useJdFolder.Checked;
        }

        void SaveAndClose()
        {
            settings.BaseFolder = useJdFolder.Checked ? "" : folder.Text.Trim();
            settings.Autostart = autostart.Checked;
            settings.AutoRenew = autoRenew.Checked;
            settings.Notifications = notify.Checked;
            settings.WatchClipboard = watchClipboard.Checked;
            settings.PollSeconds = (int)poll.Value;
            settings.CloseAction = CloseValues[onClose.SelectedIndex];
            DialogResult = DialogResult.OK;
        }
    }
}
