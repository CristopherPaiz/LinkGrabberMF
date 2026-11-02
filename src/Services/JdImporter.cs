using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Adds to the list the MediaFire links that only JD2 knows about.
    public static class JdImporter
    {
        public static async Task<int> ImportMissing(JdSnapshot snapshot, List<FileItem> items, Action<int, int> progress)
        {
            var known = new HashSet<string>(items.Select(i => i.Key));
            var wanted = new Dictionary<string, string>();
            foreach (JdLink link in snapshot.Grabber) Collect(link, snapshot.GrabberPackages, known, wanted);
            foreach (JdLink link in snapshot.Downloads) Collect(link, snapshot.DownloadPackages, known, wanted);

            int imported = 0;
            await LinkService.ForEachParallel(wanted, async entry =>
            {
                try
                {
                    FileItem item = await Task.Run(() => MediaFireClient.FromKey(entry.Key, entry.Value));
                    item.MarkSent();
                    items.Add(item);
                    imported++;
                    progress(imported, wanted.Count);
                }
                catch (Exception)
                {
                    // A file that can't be read stays only in JD2; the next sync tries again.
                }
            });
            return imported;
        }

        static void Collect(JdLink link, Dictionary<long, string> packages, HashSet<string> known, Dictionary<string, string> wanted)
        {
            string key = MediaFireClient.KeyOf(link.Url);
            if (key == null || known.Contains(key) || wanted.ContainsKey(key)) return;
            string package;
            wanted[key] = packages.TryGetValue(link.PackageUuid, out package) ? FolderPaths.FromPackage(package) : "";
        }
    }
}
