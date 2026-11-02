using System;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Ui.Controls;

namespace LinkGrab.Ui.Dialogs
{
    public class SettingsForm : DarkForm
    {
        static readonly CloseAction[] CloseValues = { CloseAction.Ask, CloseAction.Tray, CloseAction.Minimize, CloseAction.Exit };
        static readonly object[] CloseLabels = { Strings.CloseAsk, Strings.CloseTray, Strings.CloseMinimize, Strings.CloseExit };

        readonly Settings settings;
        readonly CheckBox useJdFolder = new CheckBox { Left = 16, Top = 14, AutoSize = true };
        readonly TextBox folder = MakeTextBox();
        readonly IconButton browse = new IconButton(Strings.ButtonBrowse) { Left = 414, Top = 40 };
        readonly CheckBox autostart = new CheckBox { Text = Strings.SettingsAutostart, Left = 16, Top = 90, AutoSize = true };
        readonly CheckBox autoRenew = new CheckBox { Text = Strings.SettingsAutoRenew, Left = 16, Top = 118, AutoSize = true };
        readonly CheckBox notify = new CheckBox { Text = Strings.SettingsNotify, Left = 16, Top = 146, AutoSize = true };
        readonly NumericUpDown poll = new NumericUpDown { Minimum = Limits.MinPollSeconds, Maximum = Limits.MaxPollSeconds, Left = 240, Top = 179, Width = 70, BackColor = Theme.Panel, ForeColor = Theme.Text };
        readonly ComboBox onClose = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Left = 240, Top = 215, Width = 200, BackColor = Theme.Panel, ForeColor = Theme.Text, FlatStyle = FlatStyle.Flat };

        public SettingsForm(Settings settings, string jdFolder) : base(Strings.SettingsTitle, 520, 340)
        {
            this.settings = settings;
            useJdFolder.Text = jdFolder != null ? string.Format(Strings.SettingsUseJdFolderAt, jdFolder) : Strings.SettingsUseJdFolder;
            folder.Left = 16;
            folder.Top = 44;
            folder.Width = 390;
            onClose.Items.AddRange(CloseLabels);

            LoadValues();
            WireEvents();

            var save = new IconButton(Strings.ButtonSave, null, true) { Left = 330, Top = 286 };
            save.Click += (s, e) => SaveAndClose();
            var cancel = new IconButton(Strings.ButtonCancel) { Left = 424, Top = 286 };
            cancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                useJdFolder, folder, browse, autostart, autoRenew, notify,
                new Label { Text = Strings.SettingsPoll, Left = 16, Top = 182, AutoSize = true }, poll,
                new Label { Text = Strings.SettingsOnClose, Left = 16, Top = 218, AutoSize = true }, onClose,
                save, cancel
            });
        }

        void LoadValues()
        {
            useJdFolder.Checked = settings.BaseFolder == "";
            folder.Text = settings.BaseFolder;
            autostart.Checked = settings.Autostart;
            autoRenew.Checked = settings.AutoRenew;
            notify.Checked = settings.Notifications;
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
            settings.PollSeconds = (int)poll.Value;
            settings.CloseAction = CloseValues[onClose.SelectedIndex];
            DialogResult = DialogResult.OK;
        }
    }
}
