using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Swaps LinkGrabber links inside the package they already are in. Adding them straight
    // with the package name would make JD2 open a second package with the same name.
    public static class GrabberReplacer
    {
        public static void Replace(List<FileItem> items, string destination)
        {
            var known = new HashSet<long>(JDownloaderClient.GrabberLinks().Select(l => l.Uuid));
            JDownloaderClient.AddLinks(items.Select(i => JdApi.DirectLinkPrefix + i.DirectUrl), JdApi.TransitPackage, destination, false);

            Dictionary<FileItem, JdLink> fresh = WaitForAll(items, known);
            foreach (var pair in fresh)
            {
                JDownloaderClient.MoveGrabberAfter(pair.Value.Uuid, pair.Key.JdUuid, pair.Key.JdPackage);
            }
            if (fresh.Count > 0) JDownloaderClient.RemoveFromGrabber(fresh.Keys.Select(i => i.JdUuid));
            foreach (var pair in fresh)
            {
                pair.Key.JdUuid = pair.Value.Uuid;
                pair.Key.JdUrl = pair.Value.Url;
            }

            FileItem missing = items.FirstOrDefault(i => !fresh.ContainsKey(i));
            if (missing != null) throw new Exception(string.Format(Strings.ErrorJdNoLink, missing.Name));
        }

        static Dictionary<FileItem, JdLink> WaitForAll(List<FileItem> items, HashSet<long> known)
        {
            var found = new Dictionary<FileItem, JdLink>();
            for (int step = 0; step < Timing.JdWaitSteps && found.Count < items.Count; step++)
            {
                if (step > 0) Thread.Sleep(Timing.JdWaitStepMs);
                List<JdLink> links = JDownloaderClient.GrabberLinks().Where(l => !known.Contains(l.Uuid)).ToList();
                foreach (FileItem item in items.Where(i => !found.ContainsKey(i)).ToList())
                {
                    JdLink link = links.FirstOrDefault(l => DownloadReplacer.Matches(l, item) && found.Values.All(f => f.Uuid != l.Uuid));
                    if (link != null) found[item] = link;
                }
            }
            return found;
        }
    }
}
