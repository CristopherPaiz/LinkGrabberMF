using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // A link from a site with no rule, such as mega.nz.
    public class UnsupportedSiteException : Exception
    {
        public UnsupportedSiteException(string url) : base(string.Format(Strings.ErrorUnsupportedSite, LinkText.SiteOf(url))) { }
    }

    public static class LinkText
    {
        // The name people know a site by, or the text itself if it isn't a url.
        public static string SiteOf(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || uri.Host.Length == 0) return url;
            return uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host.Substring(4) : uri.Host;
        }

        // Every url in pasted text, however it is separated.
        public static List<string> Urls(string text)
        {
            return Regex.Matches(text ?? "", WebConstants.UrlPattern).Cast<Match>().Select(m => m.Value).ToList();
        }

        // MediaFire pages only; direct download links aren't something to add, and this app copies those itself.
        public static List<string> MediaFirePages(string text)
        {
            return Urls(text).Where(IsMediaFirePage).ToList();
        }

        public static bool IsMediaFirePage(string url)
        {
            if (url == null) return false;
            return Regex.IsMatch(url, MediaFireApi.FolderPattern, RegexOptions.IgnoreCase)
                || Regex.IsMatch(url, MediaFireApi.FilePattern, RegexOptions.IgnoreCase);
        }
    }
}
