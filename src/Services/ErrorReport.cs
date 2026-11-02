using System;
using System.Text;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // The text someone copies and sends when something goes wrong.
    public static class ErrorReport
    {
        const string HiddenUserFolder = "%USERPROFILE%";

        public static string Build(Exception error)
        {
            var report = new StringBuilder();
            report.AppendLine(AppInfo.Name + " " + AppInfo.Version);
            report.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            report.AppendLine(Environment.OSVersion.VersionString + ", .NET " + Environment.Version);
            report.AppendLine();
            report.Append(error);
            return HideUserFolder(report.ToString());
        }

        // Stack traces can contain paths, and the user's folder usually has their name in it.
        static string HideUserFolder(string text)
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return folder.Length == 0 ? text : text.Replace(folder, HiddenUserFolder);
        }
    }
}
