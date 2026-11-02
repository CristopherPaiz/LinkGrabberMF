using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Services;

namespace LinkGrab.Tests
{
    public static class ErrorReportTests
    {
        static Exception Thrown()
        {
            try
            {
                try { throw new ArgumentException("interno"); }
                catch (Exception e) { throw new InvalidOperationException("externo", e); }
            }
            catch (Exception e) { return e; }
        }

        public static void IncludesWhatIsNeededToDiagnose()
        {
            string report = ErrorReport.Build(Thrown());
            foreach (string part in new[] { AppInfo.Version, "InvalidOperationException", "externo", "ArgumentException", "interno", "Thrown" })
                Check.True(report.Contains(part), "falta " + part);
        }

        public static void HidesTheUserFolder()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string report = ErrorReport.Build(new Exception(@"no existe " + folder + @"\Desktop\x.txt"));
            Check.True(!report.Contains(folder), "se ve la carpeta de usuario");
            Check.True(report.Contains(@"%USERPROFILE%\Desktop\x.txt"), "falta la ruta oculta");
        }
    }

    public static class ErrorGuardTests
    {
        public static void RepeatedErrorIsShownOnce()
        {
            string message = "repetido " + Guid.NewGuid();
            Check.True(ErrorGuard.IsNew(new Exception(message)), "primera vez");
            Check.True(!ErrorGuard.IsNew(new Exception(message)), "segunda vez");
            Check.True(ErrorGuard.IsNew(new InvalidOperationException(message)), "otro tipo");
        }

        // The real path: an error thrown in a window timer, sync and async, while the message loop runs.
        public static void AppKeepsRunningAfterErrorsOnTheWindowThread()
        {
            var reports = new List<string>();
            int ticks = 0;
            Exception problem = null;

            var thread = new Thread(() =>
            {
                try
                {
                    ErrorGuard.Present = (report, fatal) => reports.Add(report);
                    ErrorGuard.Install();
                    using (var timer = new System.Windows.Forms.Timer { Interval = 10 })
                    {
                        timer.Tick += async (s, e) =>
                        {
                            ticks++;
                            if (ticks == 1) throw new Exception("fallo directo " + Guid.NewGuid());
                            if (ticks == 2)
                            {
                                await Task.Yield();
                                throw new Exception("fallo async " + Guid.NewGuid());
                            }
                            if (ticks >= 4) Application.ExitThread();
                        };
                        timer.Start();
                        Application.Run();
                    }
                }
                catch (Exception e)
                {
                    problem = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            Check.True(thread.Join(10000), "el bucle de mensajes no terminó");
            Check.Equal(null, problem);
            Check.Equal(2, reports.Count);
            Check.True(ticks >= 4, "la app dejó de responder");
        }
    }
}
