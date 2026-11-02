using System;
using System.Web.Script.Serialization;
using LinkGrab.Constants;
using LinkGrab.Services;

namespace LinkGrab.Models
{
    public enum LinkState { None, Ready, Expired, Error, Challenge }

    public enum JdLocation { None, Grabber, Downloads }

    // One file in the list: where it comes from, its link, and what JD2 last said about it.
    public class FileItem
    {
        public FileItem()
        {
            Folder = "";
            JdStatus = "";
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string PageUrl { get; set; }
        public string DirectUrl { get; set; }
        public string Folder { get; set; }
        public LinkState LinkState { get; set; }
        public bool Sent { get; set; }
        public DateTime SentAt { get; set; }

        public JdLocation JdWhere { get; set; }
        public long JdUuid { get; set; }
        public long JdPackage { get; set; }
        public string JdStatus { get; set; }
        public bool JdRunning { get; set; }
        public bool JdFinished { get; set; }
        public bool JdProblem { get; set; }
        public int JdProgress { get; set; }

        [ScriptIgnore]
        public bool Resolving { get; set; }

        [ScriptIgnore]
        public bool Verifying { get; set; }

        [ScriptIgnore]
        public string JdUrl { get; set; }

        public bool InJd { get { return JdWhere != JdLocation.None; } }

        // JD2 caught the page itself, for example from the clipboard, instead of receiving the direct link.
        public bool JdUsesPage
        {
            get { return InJd && !JdFinished && !JdRunning && LinkText.IsMediaFirePage(JdUrl); }
        }

        public bool IsRemoved
        {
            get { return Sent && !InJd && DateTime.UtcNow - SentAt.ToUniversalTime() > Timing.SentGrace; }
        }

        public bool CanSend { get { return !InJd && (!Sent || IsRemoved); } }

        public bool NeedsLink
        {
            get
            {
                return !JdFinished && !JdRunning
                    && (LinkState == LinkState.Challenge || LinkState == LinkState.Expired || LinkState == LinkState.Error);
            }
        }

        // Also covers a file just added, which has no direct link yet but nothing wrong with it,
        // and one JD2 only has as a page.
        public bool WantsLink
        {
            get { return !JdFinished && !JdRunning && (NeedsLink || (DirectUrl == null && (!InJd || JdUsesPage))); }
        }

        public bool NeedsAttention { get { return !JdFinished && (JdProblem || NeedsLink || JdUsesPage); } }

        public void MarkSent()
        {
            Sent = true;
            SentAt = DateTime.UtcNow;
        }

        public void ClearJdState()
        {
            JdWhere = JdLocation.None;
            JdUuid = 0;
            JdUrl = null;
            JdPackage = 0;
            JdStatus = "";
            JdRunning = false;
            JdFinished = false;
            JdProblem = false;
            JdProgress = 0;
        }
    }
}
