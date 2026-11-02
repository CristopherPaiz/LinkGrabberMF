using System;
using System.IO;
using System.Reflection;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // Microsoft's signed WebView2 DLLs travel unmodified inside the exe and are copied once per version.
    public static class WebViewLibs
    {
        public static string LoaderFolder
        {
            get { return Path.Combine(AppFiles.ProfileRoot, AppFolders.WebViewLibs, AppInfo.Version); }
        }

        public static string BrowserProfile
        {
            get { return Path.Combine(AppFiles.ProfileRoot, AppFolders.BrowserProfile); }
        }

        public static void Install()
        {
            string folder = LoaderFolder;
            Directory.CreateDirectory(folder);
            foreach (string name in WebViewFiles.All) CopyOut(name, Path.Combine(folder, name));

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                string path = Path.Combine(folder, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }

        static void CopyOut(string name, string target)
        {
            if (File.Exists(target)) return;
            using (Stream source = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            using (FileStream output = File.Create(target))
            {
                source.CopyTo(output);
            }
        }
    }
}
