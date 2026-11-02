using System;
using System.IO;
using System.Reflection;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // Everything the app creates lives in a folder next to the exe,
    // or under the user's profile when that location can't be written.
    public static class AppFiles
    {
        static string folder;

        public static string ProfileRoot
        {
            get { return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppInfo.Name); }
        }

        static string Folder
        {
            get
            {
                if (folder == null) folder = PickFolder();
                return folder;
            }
        }

        static string PickFolder()
        {
            string exeFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string beside = System.IO.Path.Combine(exeFolder, AppFolders.DataBesideExe);
            try
            {
                Directory.CreateDirectory(beside);
                string probe = System.IO.Path.Combine(beside, AppFileNames.WritableProbe);
                File.WriteAllText(probe, "");
                File.Delete(probe);
                return beside;
            }
            catch (Exception)
            {
                string fallback = System.IO.Path.Combine(ProfileRoot, AppFolders.FallbackData);
                Directory.CreateDirectory(fallback);
                return fallback;
            }
        }

        public static string Path(string name)
        {
            return System.IO.Path.Combine(Folder, name);
        }

        // rules.ini ships inside the exe and is written out once so it stays editable.
        public static void EnsureRules()
        {
            string target = Path(AppFileNames.Rules);
            if (File.Exists(target)) return;
            using (Stream source = Assembly.GetExecutingAssembly().GetManifestResourceStream(AppFileNames.Rules))
            using (FileStream output = File.Create(target))
            {
                source.CopyTo(output);
            }
        }
    }
}
