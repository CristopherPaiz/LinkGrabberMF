using System;
using LinkGrab.Constants;

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

        public bool InJd { get { return JdWhere != JdLocation.None; } }

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

        public bool NeedsAttention { get { return !JdFinished && (JdProblem || NeedsLink); } }

        public void MarkSent()
        {
            Sent = true;
            SentAt = DateTime.UtcNow;
        }

        public void ClearJdState()
        {
            JdWhere = JdLocation.None;
            JdUuid = 0;
            JdPackage = 0;
            JdStatus = "";
            JdRunning = false;
            JdFinished = false;
            JdProblem = false;
            JdProgress = 0;
        }
    }
}
