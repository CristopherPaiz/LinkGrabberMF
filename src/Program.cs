using System;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Services;
using LinkGrab.Ui.Main;

namespace LinkGrab
{
    public static class Program
    {
        [STAThread]
        static void Main()
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
                WebViewLibs.Install();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}
