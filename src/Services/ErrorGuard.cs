using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LinkGrab.Services
{
    // Last line of defense for errors nothing else handled.
    // Errors on the window's thread let the app continue; .NET always ends the app after one
    // on a background thread, so those can only be reported first.
    public static class ErrorGuard
    {
        static readonly HashSet<string> seen = new HashSet<string>();

        // Receives the report and whether the app is about to close. Set once at startup.
        public static Action<string, bool> Present { get; set; }

        public static void Install()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Report(e.Exception, false);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportOnOwnThread(e.ExceptionObject as Exception, true);
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved();
                ReportOnOwnThread(e.Exception, false);
            };
        }

        // A timer failing every few seconds should not open a window every few seconds.
        public static bool IsNew(Exception error)
        {
            lock (seen) return seen.Add(error.GetType().FullName + error.Message);
        }

        public static void Report(Exception error, bool fatal)
        {
            if (error == null || (!fatal && !IsNew(error))) return;
            Present(ErrorReport.Build(error), fatal);
        }

        // Windows and the clipboard need an STA thread, which background threads aren't.
        static void ReportOnOwnThread(Exception error, bool fatal)
        {
            var thread = new Thread(() => Report(error, fatal)) { IsBackground = !fatal };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            if (fatal) thread.Join();
        }
    }
}
