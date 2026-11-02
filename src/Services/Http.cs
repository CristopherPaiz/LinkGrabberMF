using System;
using System.IO;
using System.Net;
using System.Text;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    public enum ProbeResult { FileAlive, PageOk, Challenge, Dead }

    // A person has to open this page in the built-in browser to get past it.
    public class ChallengeException : Exception
    {
        public ChallengeException(string message) : base(message) { }
    }

    public static class Http
    {
        public static string Get(string url, int timeoutMs = Timing.HttpTimeoutMs)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.UserAgent = WebConstants.UserAgent;
            request.Timeout = timeoutMs;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            try
            {
                using (WebResponse response = request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                if (IsChallenge(e.Response)) throw new ChallengeException(Strings.ErrorChallenge);
                throw;
            }
        }

        // Asks the server about a url without downloading it.
        public static ProbeResult Probe(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.UserAgent = WebConstants.UserAgent;
            request.Timeout = Timing.ProbeTimeoutMs;
            request.AllowAutoRedirect = false;
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK) return ProbeResult.Dead;
                    bool isPage = (response.ContentType ?? "").StartsWith(WebConstants.HtmlType, StringComparison.OrdinalIgnoreCase);
                    return isPage ? ProbeResult.PageOk : ProbeResult.FileAlive;
                }
            }
            catch (WebException e)
            {
                if (e.Response == null) throw;
                return IsChallenge(e.Response) ? ProbeResult.Challenge : ProbeResult.Dead;
            }
        }

        static bool IsChallenge(WebResponse response)
        {
            return response != null && response.Headers[WebConstants.ChallengeHeader] == WebConstants.ChallengeValue;
        }
    }
}
