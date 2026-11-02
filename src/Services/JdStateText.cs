using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // What the status bar says JD2 is doing.
    public static class JdStateText
    {
        // JD2 answers with names such as STOPPED_STATE; running links also count, whatever it answers.
        public static string For(string state, bool collecting, bool downloading)
        {
            string name = (state ?? "").Replace(JdApi.StateSuffix, "");
            if (collecting) return Strings.JdCollecting;
            if (name == JdApi.StateStopping) return Strings.JdStopping;
            if (name == JdApi.StatePause) return Strings.JdPaused;
            if (downloading || name == JdApi.StateRunning) return Strings.JdDownloading;
            return Strings.JdIdle;
        }
    }
}
