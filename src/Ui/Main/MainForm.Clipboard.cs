using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinkGrab.Constants;
using LinkGrab.Services;
using LinkGrab.Ui.Dialogs;

namespace LinkGrab.Ui.Main
{
    // Adds MediaFire links as soon as they are copied, and says so. Nothing else on the clipboard is kept.
    public partial class MainForm
    {
        const int WmClipboardUpdate = 0x031D;
        const int BusyCheckMs = 300;

        readonly CopyRepeatFilter copyRepeats = new CopyRepeatFilter();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool AddClipboardFormatListener(IntPtr window);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RemoveClipboardFormatListener(IntPtr window);

        protected override void OnHandleDestroyed(EventArgs e)
        {
            RemoveClipboardFormatListener(Handle);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WmClipboardUpdate) OnClipboardChanged();
        }

        async void OnClipboardChanged()
        {
            if (!settings.WatchClipboard) return;

            List<string> links = LinkText.MediaFirePages(ReadClipboard());
            string copied = string.Join(Environment.NewLine, links);
            if (links.Count == 0 || copyRepeats.IsRepeat(copied, DateTime.UtcNow)) return;

            while (busy) await Task.Delay(BusyCheckMs);

            // Files already listed need no request; a folder can only be checked by reading it.
            if (links.All(l => items.Any(i => i.Key == MediaFireClient.KeyOf(l))))
            {
                SetStatus(Strings.AlreadyInList);
                return;
            }

            int added = 0;
            await RunExclusive(async () => added = await AddLinks(copied));
            if (added > 0 && settings.Notifications)
                Toast.Show(Glyph.Link, Strings.ToastCapturedTitle, string.Format(Strings.ToastCapturedBody, added), Strings.ToastActionOpen, Theme.Accent, ShowWindow);
        }

        // Another program may be holding the clipboard for a moment.
        static string ReadClipboard()
        {
            try
            {
                return Clipboard.GetText();
            }
            catch (ExternalException)
            {
                return "";
            }
        }

        // Returns whether it worked, for the same reason.
        static bool WriteClipboard(IEnumerable<string> links)
        {
            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, links));
                return true;
            }
            catch (ExternalException)
            {
                return false;
            }
        }
    }
}
