using System.Collections.Generic;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;

namespace LinkGrab.Tests
{
    public static class JdStateTextTests
    {
        public static void StoppedMeansConnectedWithoutDownloads()
        {
            Check.Equal(Strings.JdIdle, JdStateText.For("STOPPED_STATE", false, false));
            Check.Equal(Strings.JdIdle, JdStateText.For("IDLE_STATE", false, false));
            Check.Equal(Strings.JdIdle, JdStateText.For(null, false, false));
        }

        public static void ReadsNamesWithAndWithoutSuffix()
        {
            Check.Equal(Strings.JdDownloading, JdStateText.For("RUNNING_STATE", false, false));
            Check.Equal(Strings.JdDownloading, JdStateText.For("RUNNING", false, false));
            Check.Equal(Strings.JdPaused, JdStateText.For("PAUSE_STATE", false, false));
            Check.Equal(Strings.JdStopping, JdStateText.For("STOPPING_STATE", false, false));
        }

        public static void ActiveDownloadsWinOverAStoppedAnswer()
        {
            Check.Equal(Strings.JdDownloading, JdStateText.For("STOPPED_STATE", false, true));
        }

        public static void PausedStaysPausedWhileLinksAreActive()
        {
            Check.Equal(Strings.JdPaused, JdStateText.For("PAUSE_STATE", false, true));
        }

        public static void CollectingComesFirst()
        {
            Check.Equal(Strings.JdCollecting, JdStateText.For("RUNNING_STATE", true, true));
        }
    }

    public static class JdLinkTests
    {
        public static void StatusOnAStoppedLinkIsStuck()
        {
            Check.True(new JdLink { Status = "Bloqueado" }.IsStuck, "debía estar atascado");
        }

        public static void RunningFinishedOrSilentLinksAreNotStuck()
        {
            Check.True(!new JdLink { Status = "x", Running = true }.IsStuck, "corriendo");
            Check.True(!new JdLink { Status = "x", Finished = true }.IsStuck, "terminado");
            Check.True(!new JdLink().IsStuck, "sin estado");
        }
    }

    public static class JdClientTests
    {
        public static void ApiOnlineFollowsTheServer()
        {
            using (new FakeJd()) Check.True(JDownloaderClient.ApiOnline(), "debía responder");

            string previous = JDownloaderClient.Base;
            JDownloaderClient.Base = "http://localhost:1";
            Check.True(!JDownloaderClient.ApiOnline(), "no debía responder");
            JDownloaderClient.Base = previous;
        }

        public static void AddLinksSendsEverythingEncoded()
        {
            using (var jd = new FakeJd())
            {
                JDownloaderClient.AddLinks(new[] { "http://a/1", "http://a/2?x=1&y=2" }, "Parte 1", @"C:\Bajadas", false);
                var sent = Json.Read<Dictionary<string, object>>(jd.LastQuery);
                Check.Equal("http://a/1\r\nhttp://a/2?x=1&y=2", sent["links"]);
                Check.Equal("Parte 1", sent["packageName"]);
                Check.Equal(@"C:\Bajadas", sent["destinationFolder"]);
                Check.Equal(false, sent["autostart"]);
            }
        }

        public static void AddLinksLeavesOutAnEmptyPackage()
        {
            using (var jd = new FakeJd())
            {
                JDownloaderClient.AddLinks(new[] { "http://a/1" }, "", "C:\\x", true);
                Check.True(!Json.Read<Dictionary<string, object>>(jd.LastQuery).ContainsKey("packageName"), "no debía ir packageName");
            }
        }
    }

    public static class JdSnapshotTests
    {
        const string RunningLink = "[{\"uuid\":7,\"packageUUID\":9,\"url\":\"https://download1.mediafire.com/tok/abc/f.zip\",\"running\":true,\"bytesLoaded\":50,\"bytesTotal\":200}]";

        static FileItem Item(string key)
        {
            return new FileItem { Key = key, Name = key, PageUrl = "https://www.mediafire.com/file/" + key };
        }

        public static void IdleJdIsNotDownloading()
        {
            using (new FakeJd()) Check.True(!JdSnapshot.Fetch().Downloading, "no hay descargas");
        }

        public static void SpeedOrRunningLinksMeanDownloading()
        {
            using (var jd = new FakeJd())
            {
                jd.Answer(JdRoutes.Speed, "1024");
                Check.True(JdSnapshot.Fetch().Downloading, "por velocidad");
                jd.Answer(JdRoutes.Speed, "0").Answer(JdRoutes.DownloadLinks, RunningLink);
                Check.True(JdSnapshot.Fetch().Downloading, "por enlace activo");
            }
        }

        public static void ApplyFindsItemByKeyInsideTheDirectUrl()
        {
            using (var jd = new FakeJd())
            {
                jd.Answer(JdRoutes.DownloadLinks, RunningLink);
                FileItem item = Item("abc");
                JdSnapshot.Fetch().ApplyTo(new[] { item });
                Check.Equal(JdLocation.Downloads, item.JdWhere);
                Check.Equal(7L, item.JdUuid);
                Check.Equal(25, item.JdProgress);
                Check.True(item.JdRunning && item.Sent, "debía estar descargando y enviado");
            }
        }

        public static void ApplyMarksOfflineGrabberLinks()
        {
            using (var jd = new FakeJd())
            {
                jd.Answer(JdRoutes.GrabberLinks, "[{\"uuid\":1,\"url\":\"https://www.mediafire.com/file/abc/f\",\"availability\":\"OFFLINE\"}]");
                FileItem item = Item("abc");
                JdSnapshot.Fetch().ApplyTo(new[] { item });
                Check.Equal(JdLocation.Grabber, item.JdWhere);
                Check.True(item.JdProblem, "offline es un problema");
            }
        }

        public static void ApplyClearsWhatJdNoLongerHas()
        {
            using (new FakeJd())
            {
                FileItem item = Item("abc");
                item.JdWhere = JdLocation.Downloads;
                item.JdRunning = true;
                JdSnapshot.Fetch().ApplyTo(new[] { item });
                Check.Equal(JdLocation.None, item.JdWhere);
                Check.True(!item.JdRunning, "ya no corre");
            }
        }

        public static void UnknownOrEmptyUrlsMatchNothing()
        {
            var byKey = new Dictionary<string, FileItem> { { "abc", Item("abc") } };
            Check.Equal(null, JdSnapshot.Find(new JdLink { Url = "https://otro.com/zzz" }, byKey));
            Check.Equal(null, JdSnapshot.Find(new JdLink(), byKey));
        }
    }
}
