using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Services;
using LinkGrab.Ui.Controls;
using Timer = System.Windows.Forms.Timer;

namespace LinkGrab.Ui.Dialogs
{
    // Tells the person what is missing, JD2 itself or its API, and closes as soon as it answers.
    public class ApiHelpForm : DarkForm
    {
        const string JdExecutable = @"JDownloader 2.0\JDownloader2.exe";
        static readonly Environment.SpecialFolder[] JdRoots =
        {
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolder.ProgramFiles,
            Environment.SpecialFolder.ProgramFilesX86
        };

        readonly Label heading = new Label { Font = Theme.Bold, Left = 20, Top = 16, AutoSize = true };
        readonly Label steps = new Label { Left = 20, Top = 48, Width = 520, Height = 220 };
        readonly IconButton open = new IconButton(Strings.ButtonOpenJd, null, true) { Left = 20, Top = 284 };
        readonly Timer check = new Timer { Interval = Timing.ApiHelpCheckMs };
        readonly string jdPath = FindJd();
        bool checking;

        public ApiHelpForm() : base(Strings.JdWindowTitle, 560, 340)
        {
            open.Click += (s, e) => Process.Start(jdPath);
            var close = new IconButton(Strings.ButtonClose) { Left = 220, Top = 284 };
            close.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { heading, steps, open, close });
            ShowSituation(true);

            check.Tick += async (s, e) => await Check();
            Shown += async (s, e) =>
            {
                check.Start();
                await Check();
            };
            FormClosed += (s, e) => check.Stop();
        }

        public void ShowSituation(bool closed)
        {
            heading.Text = closed ? Strings.JdClosedHeading : Strings.JdApiHeading;
            steps.Text = closed ? Strings.JdClosedSteps : Strings.JdApiSteps;
            open.Visible = closed && jdPath != null;
        }

        static string FindJd()
        {
            return JdRoots.Select(root => Path.Combine(Environment.GetFolderPath(root), JdExecutable)).FirstOrDefault(File.Exists);
        }

        async Task Check()
        {
            if (checking) return;
            checking = true;
            bool api = false, jdOpen = false;
            await Task.Run(() =>
            {
                api = JDownloaderClient.ApiOnline();
                jdOpen = api || JDownloaderClient.JdOpen();
            });
            checking = false;

            if (api) DialogResult = DialogResult.OK;
            else ShowSituation(!jdOpen);
        }
    }
}
