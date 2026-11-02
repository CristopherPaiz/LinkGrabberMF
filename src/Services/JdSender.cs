using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Sends items to JD2 grouped by folder, and swaps links already there.
    public class JdSender
    {
        readonly Settings settings;
        string jdFolder;

        public JdSender(Settings settings)
        {
            this.settings = settings;
        }

        public string JdFolder { get { return jdFolder; } }

        public void LoadJdFolder()
        {
            if (jdFolder == null) jdFolder = JDownloaderClient.DefaultFolder();
        }

        // JDownloader appends the package name to the destination folder itself.
        string DestinationOf(string folder)
        {
            string root = settings.BaseFolder;
            if (root == "")
            {
                LoadJdFolder();
                root = jdFolder;
            }
            return Path.Combine(root, FolderPaths.Parent(folder));
        }

        public async Task Add(List<FileItem> targets)
        {
            foreach (var group in targets.GroupBy(i => i.Folder))
            {
                string folder = group.Key;
                string package = folder == "" ? null : FolderPaths.Leaf(folder);
                List<string> links = group.Select(i => JdApi.DirectLinkPrefix + i.DirectUrl).ToList();

                for (int start = 0; start < links.Count; start += Limits.LinksPerJdRequest)
                {
                    List<string> batch = links.Skip(start).Take(Limits.LinksPerJdRequest).ToList();
                    await Task.Run(() => JDownloaderClient.AddLinks(batch, package, DestinationOf(folder), settings.Autostart));
                }
                foreach (FileItem item in group) item.MarkSent();
            }
        }

        public Task Reinsert(FileItem item)
        {
            return Reinsert(new List<FileItem> { item });
        }

        // Each link keeps its package and its place in it, in LinkGrabber and in downloads.
        public async Task Reinsert(List<FileItem> targets)
        {
            foreach (FileItem item in targets.Where(i => i.JdWhere == JdLocation.Downloads))
            {
                string destination = DestinationOf(item.Folder);
                item.JdUuid = await Task.Run(() => DownloadReplacer.Replace(item, destination));
            }

            List<FileItem> grabber = targets.Where(i => i.JdWhere == JdLocation.Grabber).ToList();
            for (int start = 0; start < grabber.Count; start += Limits.LinksPerJdRequest)
            {
                List<FileItem> batch = grabber.Skip(start).Take(Limits.LinksPerJdRequest).ToList();
                await Task.Run(() => GrabberReplacer.Replace(batch, DestinationOf("")));
            }

            foreach (FileItem item in targets)
            {
                item.MarkSent();
                item.JdProblem = false;
            }
        }
    }
}
