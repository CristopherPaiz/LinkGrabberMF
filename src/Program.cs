using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Services;
using LinkGrab.Ui.Dialogs;
using LinkGrab.Ui.Main;

namespace LinkGrab
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            ErrorGuard.Present = ErrorForm.Present;
            ErrorGuard.Install();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Application.Run covers errors while the window is open; this covers starting up.
            try
            {
                Run();
            }
            catch (Exception e)
            {
                ErrorGuard.Report(e, true);
            }
        }

        static void Run()
        {
            bool first;
            using (new Mutex(true, AppInfo.InstanceMutex, out first))
            {
                if (!first)
                {
                    MessageBox.Show(Strings.AlreadyRunning, AppInfo.Name);
                    return;
                }
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                AppFiles.EnsureRules();
                WebViewLibs.LoadOnDemand();
                Application.Run(new MainForm());
            }
        }
    }
}
