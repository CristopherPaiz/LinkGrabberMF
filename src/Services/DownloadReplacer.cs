using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Puts a new link right where an existing download is, then drops the old one.
    public static class DownloadReplacer
    {
        // The link stays disabled until it is in place because reaching the list can start it.
        public static long Replace(FileItem item, string destination)
        {
            var known = new HashSet<long>(JDownloaderClient.GrabberLinks().Select(l => l.Uuid));
            JDownloaderClient.AddLinks(new[] { JdApi.DirectLinkPrefix + item.DirectUrl }, JdApi.TransitPackage, destination, false);

            JdLink fresh = WaitFor(() => JDownloaderClient.GrabberLinks().FirstOrDefault(l => !known.Contains(l.Uuid) && Matches(l, item)));
            if (fresh == null) throw new Exception(string.Format(Strings.ErrorJdNoLink, item.Name));

            long[] id = { fresh.Uuid };
            JDownloaderClient.SetGrabberEnabled(id, false);
            JDownloaderClient.MoveToDownloads(id);
            if (WaitFor(() => JDownloaderClient.DownloadLinks().FirstOrDefault(l => l.Uuid == fresh.Uuid)) == null)
                throw new Exception(string.Format(Strings.ErrorJdNoMove, item.Name));

            JDownloaderClient.MoveDownloadAfter(fresh.Uuid, item.JdUuid, item.JdPackage);
            JDownloaderClient.RemoveFromDownloads(new[] { item.JdUuid });
            JDownloaderClient.SetDownloadEnabled(id, true);
            return fresh.Uuid;
        }

        internal static bool Matches(JdLink link, FileItem item)
        {
            return link.Url != null && (link.Url == item.DirectUrl || link.Url.Split('/').Contains(item.Key));
        }

        static JdLink WaitFor(Func<JdLink> find)
        {
            for (int step = 0; step < Timing.JdWaitSteps; step++)
            {
                JdLink link = find();
                if (link != null) return link;
                Thread.Sleep(Timing.JdWaitStepMs);
            }
            return null;
        }
    }
}
