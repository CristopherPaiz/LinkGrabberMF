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
    // Every step to connect with JD2, and it closes as soon as the API answers.
    public class ApiHelpForm : DarkForm
    {
        const string JdExecutable = @"JDownloader 2.0\JDownloader2.exe";
        static readonly Environment.SpecialFolder[] JdRoots =
        {
            Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolder.ProgramFiles,
            Environment.SpecialFolder.ProgramFilesX86
        };

        readonly Label heading = new Label { Font = Theme.Bold, Left = LabelGutter, Top = 18, AutoSize = true };
        readonly Label steps = new Label { Left = LabelGutter, Top = 50, Width = 524, Height = 270 };
        readonly IconButton open = new IconButton(Strings.ButtonOpenJd, null, true);
        readonly Timer check = new Timer { Interval = Timing.ApiHelpCheckMs };
        readonly string jdPath = FindJd();
        bool checking;

        public ApiHelpForm() : base(Strings.JdWindowTitle, 560, 400)
        {
            open.Click += (s, e) => Process.Start(jdPath);
            var close = new IconButton(Strings.ButtonClose);
            close.Click += (s, e) => Close();

            heading.Text = Strings.JdConnectHeading;
            steps.Text = Strings.JdConnectSteps;

            Controls.AddRange(new Control[] { heading, steps });
            if (jdPath != null) AddFooter(open, close);
            else AddFooter(close);

            check.Tick += async (s, e) => await Check();
            Shown += async (s, e) =>
            {
                check.Start();
                await Check();
            };
            FormClosed += (s, e) => check.Stop();
        }

        static string FindJd()
        {
            return JdRoots.Select(root => Path.Combine(Environment.GetFolderPath(root), JdExecutable)).FirstOrDefault(File.Exists);
        }

        async Task Check()
        {
            if (checking) return;
            checking = true;
            bool api = await Task.Run(() => JDownloaderClient.ApiOnline());
            checking = false;

            if (api) DialogResult = DialogResult.OK;
        }
    }
}
