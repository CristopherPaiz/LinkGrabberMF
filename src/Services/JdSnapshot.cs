using System.Collections.Generic;
using System.Linq;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Everything read from JD2 in one sync, and how it maps onto the list.
    public class JdSnapshot
    {
        public List<JdLink> Grabber { get; private set; }
        public List<JdLink> Downloads { get; private set; }
        public Dictionary<long, string> GrabberPackages { get; private set; }
        public Dictionary<long, string> DownloadPackages { get; private set; }
        public bool Collecting { get; private set; }
        public string State { get; private set; }
        public long Speed { get; private set; }

        public bool Downloading { get { return Speed > 0 || Downloads.Any(l => l.Running); } }

        public static JdSnapshot Fetch()
        {
            return new JdSnapshot
            {
                Grabber = JDownloaderClient.GrabberLinks(),
                Downloads = JDownloaderClient.DownloadLinks(),
                GrabberPackages = JDownloaderClient.GrabberPackages(),
                DownloadPackages = JDownloaderClient.DownloadPackages(),
                Collecting = JDownloaderClient.IsCollecting(),
                State = JDownloaderClient.State(),
                Speed = JDownloaderClient.Speed()
            };
        }

        public void ApplyTo(IEnumerable<FileItem> items)
        {
            Dictionary<string, FileItem> byKey = items.ToDictionary(i => i.Key);
            foreach (FileItem item in byKey.Values) item.ClearJdState();

            foreach (JdLink link in Grabber)
            {
                FileItem item = Find(link, byKey);
                if (item != null) ApplyGrabberLink(item, link);
            }
            foreach (JdLink link in Downloads)
            {
                FileItem item = Find(link, byKey);
                if (item != null) ApplyDownloadLink(item, link);
            }
        }

        static void ApplyGrabberLink(FileItem item, JdLink link)
        {
            // A page caught again from the clipboard doesn't hide the direct link JD2 already has.
            if (item.InJd && !LinkText.IsMediaFirePage(item.JdUrl) && LinkText.IsMediaFirePage(link.Url)) return;

            bool offline = link.Availability == JdApi.Offline;
            item.JdWhere = JdLocation.Grabber;
            item.JdUrl = link.Url;
            item.JdUuid = link.Uuid;
            item.JdPackage = link.PackageUuid;
            item.JdProblem = offline;
            item.JdStatus = offline ? Strings.StateGrabberOffline : Strings.StateGrabber;
            item.Sent = true;
        }

        public static void ApplyDownloadLink(FileItem item, JdLink link)
        {
            item.JdWhere = JdLocation.Downloads;
            item.JdUrl = link.Url;
            item.JdUuid = link.Uuid;
            item.JdPackage = link.PackageUuid;
            item.JdRunning = link.Running;
            item.JdFinished = link.Finished;
            item.JdProgress = link.BytesTotal > 0 ? (int)(link.BytesLoaded * 100 / link.BytesTotal) : 0;
            item.JdProblem = link.IsStuck;
            item.JdStatus = Describe(link);
            item.Sent = true;
            if ((link.Running || link.Finished) && item.LinkState != LinkState.None) item.LinkState = LinkState.Ready;
        }

        static string Describe(JdLink link)
        {
            if (link.Finished) return Strings.StateDone;
            if (link.Running) return link.Status ?? Strings.StateDownloading;
            if (!string.IsNullOrEmpty(link.Status)) return link.Status;
            return link.BytesLoaded > 0 ? Strings.StateStopped : Strings.StateQueued;
        }

        // JD2 keeps the MediaFire quickkey as a path segment of both page and direct urls.
        public static FileItem Find(JdLink link, Dictionary<string, FileItem> byKey)
        {
            if (link.Url == null) return null;
            FileItem item;
            if (byKey.TryGetValue(link.Url, out item)) return item;

            foreach (string segment in link.Url.Split('/'))
            {
                if (byKey.TryGetValue(segment, out item)) return item;
            }
            return null;
        }
    }
}
