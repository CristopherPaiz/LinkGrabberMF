using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    // Turns a file page into a direct link using the rules in rules.ini.
    public static class LinkResolver
    {
        const string TagPattern = @"<[a-zA-Z][^>]*>";
        const string AttrPattern = @"\s{0}\s*=\s*[""']([^""']*)[""']";

        public static string Resolve(string url)
        {
            Rule rule = RuleSet.For(url);
            if (rule == null) throw new UnsupportedSiteException(url);
            if (rule.Render == RuleKeys.RenderBrowser) throw new ChallengeException(Strings.ErrorRuleBrowser);

            string link = Extract(Http.Get(url), rule);
            if (link != null) return link;
            if (rule.Render == RuleKeys.RenderHttp) throw new Exception(string.Format(Strings.ErrorLinkNotFound, rule.Name));
            throw new ChallengeException(Strings.ErrorNeedsBrowser);
        }

        // Finds the first tag containing rule.Element and returns its first usable attribute.
        public static string Extract(string html, Rule rule)
        {
            foreach (Match tag in Regex.Matches(html, TagPattern))
            {
                if (tag.Value.IndexOf(rule.Element, StringComparison.OrdinalIgnoreCase) < 0) continue;

                foreach (string spec in rule.Attrs)
                {
                    string link = ReadAttribute(tag.Value, spec);
                    if (link != null && link.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return link;
                }
            }
            return null;
        }

        static string ReadAttribute(string tag, string spec)
        {
            bool isBase64 = spec.EndsWith(RuleKeys.Base64Suffix);
            string attr = isBase64 ? spec.Substring(0, spec.Length - RuleKeys.Base64Suffix.Length) : spec;
            Match value = Regex.Match(tag, string.Format(AttrPattern, Regex.Escape(attr)), RegexOptions.IgnoreCase);
            if (!value.Success) return null;

            string text = WebUtility.HtmlDecode(value.Groups[1].Value).Trim();
            return isBase64 ? DecodeBase64(text) : text;
        }

        static string DecodeBase64(string text)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(text)); }
            catch (FormatException) { return null; }
        }
    }
}
