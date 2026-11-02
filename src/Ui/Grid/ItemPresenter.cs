using System.Drawing;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Ui.Grid
{
    public enum RowAction { None, GetLink, Send, SendAgain, SendFolder }

    public enum ItemFilter { All, Problems, InJd, Done, Removed }

    public struct ItemState
    {
        public string Text;
        public Color Color;

        public ItemState(string text, Color color)
        {
            Text = text;
            Color = color;
        }
    }

    // Decides how an item reads on screen and which action its row offers.
    public static class ItemPresenter
    {
        public static ItemState StateOf(FileItem item)
        {
            if (item.JdFinished) return new ItemState(Strings.StateDone, Theme.Good);
            if (item.JdRunning) return new ItemState(Strings.StateDownloading, Theme.Accent);
            if (item.LinkState == LinkState.Challenge) return new ItemState(Strings.StateBlocked, Theme.Warn);
            if (item.LinkState == LinkState.Expired) return new ItemState(Strings.StateExpired, Theme.Warn);
            if (item.LinkState == LinkState.Error) return new ItemState(Strings.StateError, Theme.Bad);
            if (item.JdProblem) return new ItemState(item.JdStatus, Theme.Warn);
            if (item.JdWhere == JdLocation.Grabber) return new ItemState(Strings.StateGrabber, Theme.Text);
            if (item.JdWhere == JdLocation.Downloads) return new ItemState(item.JdStatus, Theme.Text);
            if (item.IsRemoved) return new ItemState(Strings.StateRemoved, Theme.Muted);
            if (item.Sent) return new ItemState(Strings.StateSending, Theme.Muted);
            if (item.LinkState == LinkState.Ready) return new ItemState(Strings.StateReady, Theme.Good);
            return new ItemState(Strings.StateUnsent, Theme.Muted);
        }

        public static RowAction ActionOf(FileItem item)
        {
            if (item.JdFinished || item.JdRunning) return RowAction.None;
            if (item.NeedsLink) return RowAction.GetLink;
            if (!item.CanSend) return RowAction.None;
            return item.IsRemoved ? RowAction.SendAgain : RowAction.Send;
        }

        public static string LabelOf(RowAction action)
        {
            switch (action)
            {
                case RowAction.GetLink: return Strings.ActionGetLink;
                case RowAction.Send: return Strings.ActionSend;
                case RowAction.SendAgain: return Strings.ActionSendAgain;
                case RowAction.SendFolder: return Strings.ActionSendFolder;
                default: return null;
            }
        }

        public static bool Matches(FileItem item, ItemFilter filter)
        {
            switch (filter)
            {
                case ItemFilter.Problems: return item.NeedsAttention;
                case ItemFilter.InJd: return item.InJd;
                case ItemFilter.Done: return item.JdFinished;
                case ItemFilter.Removed: return item.IsRemoved;
                default: return true;
            }
        }
    }
}
