using System;
using System.IO;
using LinkGrab.Constants;
using LinkGrab.Services;

namespace LinkGrab.Models
{
    public enum CloseAction { Ask, Tray, Minimize, Exit }

    public class Settings
    {
        public Settings()
        {
            BaseFolder = "";
            Notifications = true;
            PollSeconds = Limits.DefaultPollSeconds;
        }

        // Empty means "use JDownloader's own download folder".
        public string BaseFolder { get; set; }
        public bool Autostart { get; set; }
        public bool AutoRenew { get; set; }
        public bool Notifications { get; set; }
        public int PollSeconds { get; set; }
        public CloseAction CloseAction { get; set; }

        public int PollMilliseconds
        {
            get { return Math.Max(Limits.MinPollSeconds, PollSeconds) * 1000; }
        }

        static string FilePath { get { return AppFiles.Path(AppFileNames.Settings); } }

        public static Settings Load()
        {
            try { return File.Exists(FilePath) ? Json.Read<Settings>(File.ReadAllText(FilePath)) : new Settings(); }
            catch (Exception) { return new Settings(); }
        }

        public void Save()
        {
            File.WriteAllText(FilePath, Json.Write(this));
        }
    }
}
