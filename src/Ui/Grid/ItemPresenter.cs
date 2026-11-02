using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LinkGrab.Constants;
using LinkGrab.Models;
using LinkGrab.Services;

namespace LinkGrab.Ui.Grid
{
    public enum RowAction { None, GetLink, Copy, CopyAll, Reload, Send, SendAgain, SendFolder, Replace }

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
            if (item.Resolving) return new ItemState(Strings.StateResolving, Theme.Accent);
            if (item.Verifying) return new ItemState(Strings.StateVerifying, Theme.Accent);
            if (item.JdFinished) return new ItemState(Strings.StateDone, Theme.Good);
            if (item.JdRunning) return new ItemState(Strings.StateDownloading, Theme.Accent);
            if (item.LinkState == LinkState.Challenge) return new ItemState(Strings.StateBlocked, Theme.Warn);
            if (item.LinkState == LinkState.Expired) return new ItemState(Strings.StateExpired, Theme.Warn);
            if (item.LinkState == LinkState.Error) return new ItemState(Strings.StateError, Theme.Bad);
            if (item.JdUsesPage) return new ItemState(Strings.StateJdPage, Theme.Warn);
            if (item.JdProblem) return new ItemState(item.JdStatus, Theme.Warn);
            if (item.JdWhere == JdLocation.Grabber) return new ItemState(Strings.StateGrabber, Theme.Good);
            if (item.JdWhere == JdLocation.Downloads) return new ItemState(item.JdStatus, Theme.Text);
            if (item.IsRemoved) return new ItemState(Strings.StateRemoved, Theme.Muted);
            if (item.Sent) return new ItemState(Strings.StateSending, Theme.Muted);
            if (item.LinkState == LinkState.Ready) return new ItemState(Strings.StateReady, Theme.Good);
            return new ItemState(Strings.StateUnsent, Theme.Muted);
        }

        // Icon and color by file type, so a long list reads at a glance.
        public static ItemState IconOf(FileItem item)
        {
            switch (FileKinds.Of(item.Name))
            {
                case FileKind.Archive: return new ItemState(Glyph.Archive, Theme.Orange);
                case FileKind.Video: return new ItemState(Glyph.Video, Theme.Violet);
                case FileKind.Audio: return new ItemState(Glyph.Audio, Theme.Pink);
                case FileKind.Image: return new ItemState(Glyph.Image, Theme.Good);
                case FileKind.Pdf: return new ItemState(Glyph.Pdf, Theme.Bad);
                case FileKind.Document: return new ItemState(Glyph.File, Theme.Accent);
                case FileKind.Disc: return new ItemState(Glyph.Disc, Theme.Cyan);
                case FileKind.Program: return new ItemState(Glyph.Program, Theme.Text);
                default: return new ItemState(Glyph.File, Theme.Muted);
            }
        }

        // A file without a link offers to get one; once it has one, to copy or renew it.
        public static List<RowAction> ActionsOf(FileItem item)
        {
            var actions = new List<RowAction>();
            if (item.JdFinished || item.JdRunning || item.Resolving || item.Verifying) return actions;

            if (item.WantsLink)
            {
                actions.Add(RowAction.GetLink);
            }
            else if (item.DirectUrl != null)
            {
                actions.Add(RowAction.Copy);
                actions.Add(RowAction.Reload);
            }
            if (item.CanSend) actions.Add(item.IsRemoved ? RowAction.SendAgain : RowAction.Send);
            // Getting the link already swaps it in JD2, so Replace only shows once there is one.
            else if (item.JdUsesPage && !item.WantsLink) actions.Add(RowAction.Replace);
            return actions;
        }

        public static List<RowAction> ActionsOf(List<FileItem> folder)
        {
            var actions = new List<RowAction>();
            if (folder.Any(i => i.WantsLink && !i.Resolving)) actions.Add(RowAction.GetLink);
            if (folder.Any(i => i.CanSend)) actions.Add(RowAction.SendFolder);
            else if (folder.Any(i => i.JdUsesPage && !i.WantsLink && !i.Resolving)) actions.Add(RowAction.Replace);
            return actions;
        }

        // The selection if there is one; otherwise everything the current filter shows. Only working links.
        public static List<FileItem> CopyTargets(IEnumerable<FileItem> all, List<FileItem> selected, ItemFilter filter)
        {
            IEnumerable<FileItem> source = selected.Count > 0 ? selected : all.Where(i => Matches(i, filter));
            return source.Where(i => i.DirectUrl != null && !i.WantsLink).ToList();
        }

        public static string CopyLabel(bool anySelected, int links)
        {
            return anySelected ? string.Format(Strings.ActionCopyCount, links) : Strings.ActionCopyAll;
        }

        public static bool RequiresJd(RowAction action)
        {
            return action == RowAction.Send || action == RowAction.SendAgain || action == RowAction.SendFolder || action == RowAction.Replace;
        }

        public static string LabelOf(RowAction action)
        {
            switch (action)
            {
                case RowAction.GetLink: return Strings.ActionGetLink;
                case RowAction.Copy: return Strings.ActionCopy;
                case RowAction.Reload: return Strings.ActionReload;
                case RowAction.Send: return Strings.ActionSend;
                case RowAction.Replace: return Strings.ActionReplace;
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
