using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Works out why a download stopped by asking the site directly,
    // because JD2's text depends on its language.
    public class StuckDownloadWatcher
    {
        readonly List<FileItem> items;
        readonly Dictionary<long, DateTime> probed = new Dictionary<long, DateTime>();
        readonly HashSet<long> alerted = new HashSet<long>();

        public StuckDownloadWatcher(List<FileItem> items)
        {
            this.items = items;
        }

        // Returns the names of files that just started needing attention.
        public async Task<List<string>> Check(JdSnapshot snapshot)
        {
            DateTime now = DateTime.UtcNow;
            List<JdLink> due = snapshot.Downloads.Where(l => l.IsStuck)
                .Where(l => !probed.ContainsKey(l.Uuid) || now - probed[l.Uuid] > Timing.ProbeEvery)
                .Take(Limits.MaxProbesPerSync).ToList();

            var fresh = new List<string>();
            foreach (JdLink link in due)
            {
                probed[link.Uuid] = now;
                string name = await Evaluate(link, snapshot.DownloadPackages);
                if (name != null && alerted.Add(link.Uuid)) fresh.Add(name);
            }
            return fresh;
        }

        async Task<string> Evaluate(JdLink link, Dictionary<long, string> packages)
        {
            FileItem item = JdSnapshot.Find(link, items.ToDictionary(i => i.Key));
            bool saysCloudflare = (link.Status ?? "").IndexOf(JdApi.Cloudflare, StringComparison.OrdinalIgnoreCase) >= 0;
            string url = item != null ? item.PageUrl : link.Url;
            if (item == null && MediaFireClient.KeyOf(url) == null) return saysCloudflare ? link.Name : null;

            ProbeResult result;
            try { result = await Task.Run(() => Http.Probe(url)); }
            catch (WebException) { return null; }

            if (result != ProbeResult.Challenge && !saysCloudflare) return await CheckDirectLink(item);

            if (item == null) item = await Import(link, packages);
            if (item == null) return link.Name;

            item.LinkState = result == ProbeResult.Challenge ? LinkState.Challenge : LinkState.Expired;
            return item.Name;
        }

        // Direct links die on their own; a dead one needs a new token, not a challenge.
        static async Task<string> CheckDirectLink(FileItem item)
        {
            if (item == null || item.DirectUrl == null) return null;
            ProbeResult direct;
            try { direct = await Task.Run(() => Http.Probe(item.DirectUrl)); }
            catch (WebException) { return null; }
            if (direct == ProbeResult.FileAlive) return null;

            item.LinkState = LinkState.Expired;
            return item.Name;
        }

        // A download added straight to JD2 joins the list once it fails, so it can be repaired too.
        async Task<FileItem> Import(JdLink link, Dictionary<long, string> packages)
        {
            try
            {
                string key = MediaFireClient.KeyOf(link.Url);
                string package;
                string folder = packages.TryGetValue(link.PackageUuid, out package) ? FolderPaths.FromPackage(package) : "";
                FileItem item = await Task.Run(() => MediaFireClient.FromKey(key, folder));
                item.MarkSent();
                JdSnapshot.ApplyDownloadLink(item, link);
                items.Add(item);
                return item;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
