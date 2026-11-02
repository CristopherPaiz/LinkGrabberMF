using System;
using System.IO;
using System.Reflection;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // WebView2's DLLs ship next to the exe in lib\.
    public static class WebViewLibs
    {
        public static string LoaderFolder
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), AppFolders.Libraries); }
        }

        public static string BrowserProfile
        {
            get { return Path.Combine(AppFiles.ProfileRoot, AppFolders.BrowserProfile); }
        }

        public static bool AreInstalled
        {
            get { return File.Exists(Path.Combine(LoaderFolder, WebViewFiles.Core)) && File.Exists(Path.Combine(LoaderFolder, WebViewFiles.Loader)); }
        }

        // .NET only looks beside the exe by default, so it needs to be told about lib\.
        public static void UseLibFolder()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                string path = Path.Combine(LoaderFolder, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }
    }
}