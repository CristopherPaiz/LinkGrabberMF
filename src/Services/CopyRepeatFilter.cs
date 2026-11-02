using System;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // Windows can announce one copy several times in a row. Copying the same text later is a new copy.
    public class CopyRepeatFilter
    {
        string lastText;
        DateTime lastTime;

        public bool IsRepeat(string text, DateTime now)
        {
            bool repeat = text == lastText && now - lastTime < Timing.ClipboardRepeatWindow;
            lastText = text;
            lastTime = now;
            return repeat;
        }
    }
}
