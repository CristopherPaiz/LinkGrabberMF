using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // Microsoft's WebView2 DLLs travel compressed inside the exe and are unpacked
    // the first time the built-in browser opens, once per version.
    public static class WebViewLibs
    {
        public static string LoaderFolder
        {
            get { return Path.Combine(AppFiles.ProfileRoot, AppFolders.Libraries, AppInfo.Version); }
        }

        public static string BrowserProfile
        {
            get { return Path.Combine(AppFiles.ProfileRoot, AppFolders.BrowserProfile); }
        }

        public static void LoadOnDemand()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                string name = new AssemblyName(e.Name).Name + ".dll";
                if (Array.IndexOf(WebViewFiles.All, name) < 0) return null;
                Unpack();
                return Assembly.LoadFrom(Path.Combine(LoaderFolder, name));
            };
        }

        static void Unpack()
        {
            Directory.CreateDirectory(LoaderFolder);
            foreach (string name in WebViewFiles.All) UnpackFile(name, Path.Combine(LoaderFolder, name));
        }

        // Writes to a temporary name first so an interrupted copy never looks finished.
        static void UnpackFile(string name, string target)
        {
            if (File.Exists(target)) return;
            string partial = target + WebViewFiles.PartialSuffix;
            using (Stream source = Assembly.GetExecutingAssembly().GetManifestResourceStream(name + WebViewFiles.PackedSuffix))
            using (var unpacked = new GZipStream(source, CompressionMode.Decompress))
            using (FileStream output = File.Create(partial))
            {
                unpacked.CopyTo(output);
            }
            File.Move(partial, target);
        }
    }
}
