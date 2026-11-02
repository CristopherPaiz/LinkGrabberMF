using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LinkGrab.Constants;

namespace LinkGrab.Services
{
    public enum JdConnection { Unknown, Online, ApiOff, Closed }

    public static class JdConnections
    {
        // Before the first check, actions stay on and check for themselves.
        public static bool CanUse(JdConnection connection)
        {
            return connection == JdConnection.Online || connection == JdConnection.Unknown;
        }
    }

    public class JdLink
    {
        public long Uuid { get; set; }
        public long PackageUuid { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        public string Availability { get; set; }
        public bool Running { get; set; }
        public bool Finished { get; set; }
        public long BytesLoaded { get; set; }
        public long BytesTotal { get; set; }

        // Any status on a stopped, unfinished link means JD2 has something to report,
        // whatever language that text is in.
        public bool IsStuck { get { return !Finished && !Running && !string.IsNullOrEmpty(Status); } }
    }

    // Client for JDownloader's local API. Arguments travel as JSON values in the query string.
    public static class JDownloaderClient
    {
        const string GrabberQuery = "{\"url\":true,\"availability\":true,\"bytesTotal\":true,\"maxResults\":-1,\"startAt\":0}";
        const string DownloadQuery = "{\"url\":true,\"status\":true,\"finished\":true,\"running\":true,\"bytesLoaded\":true,\"bytesTotal\":true,\"maxResults\":-1,\"startAt\":0}";
        const string PackageQuery = "{\"maxResults\":-1,\"startAt\":0}";
        const string NoPackages = "[]";

        public static string Base = JdApi.DefaultBase;
        public static string ClickNLoad = JdApi.DefaultClickNLoad;

        class Reply<T> { public T data = default(T); }

        static string BuildUrl(string route, string[] jsonArgs)
        {
            if (jsonArgs.Length == 0) return Base + route;
            return Base + route + "?" + string.Join("&", jsonArgs.Select(Uri.EscapeDataString));
        }

        static T Call<T>(string route, params string[] jsonArgs)
        {
            return Json.Read<Reply<T>>(Http.Get(BuildUrl(route, jsonArgs), Timing.JdTimeoutMs)).data;
        }

        static void Send(string route, params string[] jsonArgs)
        {
            Http.Get(BuildUrl(route, jsonArgs), Timing.JdTimeoutMs);
        }

        static string Ids(IEnumerable<long> ids)
        {
            return Json.Write(ids.ToArray());
        }

        public static void AddLinks(IEnumerable<string> links, string packageName, string destination, bool autostart)
        {
            var query = new Dictionary<string, object>
            {
                { "links", string.Join("\r\n", links) },
                { "destinationFolder", destination },
                { "autostart", autostart }
            };
            if (!string.IsNullOrEmpty(packageName)) query["packageName"] = packageName;
            Send(JdRoutes.AddLinks, Json.Write(query));
        }

        public static List<JdLink> GrabberLinks() { return Call<List<JdLink>>(JdRoutes.GrabberLinks, GrabberQuery); }
        public static List<JdLink> DownloadLinks() { return Call<List<JdLink>>(JdRoutes.DownloadLinks, DownloadQuery); }
        public static Dictionary<long, string> GrabberPackages() { return Packages(JdRoutes.GrabberPackages); }
        public static Dictionary<long, string> DownloadPackages() { return Packages(JdRoutes.DownloadPackages); }

        static Dictionary<long, string> Packages(string route)
        {
            return Call<List<JdLink>>(route, PackageQuery).ToDictionary(p => p.Uuid, p => p.Name);
        }

        public static void RemoveFromGrabber(IEnumerable<long> ids) { Send(JdRoutes.GrabberRemove, Ids(ids), NoPackages); }
        public static void RemoveFromDownloads(IEnumerable<long> ids) { Send(JdRoutes.DownloadRemove, Ids(ids), NoPackages); }
        public static void SetGrabberEnabled(IEnumerable<long> ids, bool enabled) { Send(JdRoutes.GrabberEnabled, Json.Write(enabled), Ids(ids), NoPackages); }
        public static void SetDownloadEnabled(IEnumerable<long> ids, bool enabled) { Send(JdRoutes.DownloadEnabled, Json.Write(enabled), Ids(ids), NoPackages); }
        public static void MoveDownloadAfter(long link, long afterLink, long package) { Send(JdRoutes.DownloadMove, Ids(new[] { link }), Json.Write(afterLink), Json.Write(package)); }

        // JDownloader may start downloading as soon as links reach the download list.
        public static void MoveToDownloads(IEnumerable<long> ids) { Send(JdRoutes.GrabberToDownloads, Ids(ids), NoPackages); }

        public static void Start() { Call<bool>(JdRoutes.Start); }
        public static void Stop() { Call<bool>(JdRoutes.Stop); }
        public static string State() { return Call<string>(JdRoutes.State); }
        public static long Speed() { return Call<long>(JdRoutes.Speed); }
        public static bool IsCollecting() { return Call<bool>(JdRoutes.IsCollecting); }

        public static string DefaultFolder()
        {
            return Call<string>(JdRoutes.ConfigGet, Json.Write(JdApi.GeneralSettings), "null", Json.Write(JdApi.DefaultFolderKey));
        }

        public static bool ApiOnline() { return Answers(Base + JdRoutes.State); }
        public static bool JdOpen() { return Answers(ClickNLoad); }

        static bool Answers(string url)
        {
            try
            {
                Http.Get(url, Timing.JdPingTimeoutMs);
                return true;
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
