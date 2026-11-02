using System;

namespace LinkGrab.Constants
{
    public static class AppInfo
    {
        public const string Name = "LinkGrab";
        public const string InstanceMutex = "LinkGrab.SingleInstance";
        public const string Version = "1.0.0.0";
        public const string Author = "Cristopher Paiz";
        public const string Description = "Envía enlaces de MediaFire a JDownloader 2";
        public const string Repository = "https://github.com/CristopherPaiz/LinkGrabberMF";
    }

    public static class AppFolders
    {
        public const string DataBesideExe = "LinkGrabData";
        public const string FallbackData = "data";
        public const string Libraries = "lib";
        public const string BrowserProfile = "browser";
    }

    public static class AppFileNames
    {
        public const string Rules = "rules.ini";
        public const string Session = "session.json";
        public const string Settings = "settings.json";
        public const string WritableProbe = ".write";
    }

    public static class WebViewFiles
    {
        public const string Core = "Microsoft.Web.WebView2.Core.dll";
        public const string WinForms = "Microsoft.Web.WebView2.WinForms.dll";
        public const string Loader = "WebView2Loader.dll";
        public const string InstallUrl = "https://developer.microsoft.com/microsoft-edge/webview2/";
    }

    public static class Limits
    {
        public const int MaxParallelRequests = 4;
        public const int LinksPerJdRequest = 10;
        public const int MaxProbesPerSync = 3;
        public const int MinPollSeconds = 2;
        public const int MaxPollSeconds = 300;
        public const int DefaultPollSeconds = 5;
        public const int NotificationNames = 3;
    }

    public static class Timing
    {
        public const int HttpTimeoutMs = 30000;
        public const int ProbeTimeoutMs = 20000;
        public const int JdTimeoutMs = 15000;
        public const int JdPingTimeoutMs = 3000;
        public const int JdWaitStepMs = 500;
        public const int JdWaitSteps = 40;
        public const int ApiHelpCheckMs = 3000;
        public const int BrowserCheckMs = 800;
        public const int ToastLifeMs = 9000;
        public const int ActivityFrameMs = 30;
        public static readonly TimeSpan ProbeEvery = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan SentGrace = TimeSpan.FromSeconds(60);
    }

    public static class JdApi
    {
        public const string DefaultBase = "http://127.0.0.1:3128";
        public const string DefaultClickNLoad = "http://127.0.0.1:9666/flash/";
        public const string TransitPackage = "LinkGrab";
        public const string Offline = "OFFLINE";
        public const string StateRunning = "RUNNING";
        public const string StatePause = "PAUSE";
        public const string StateStopping = "STOPPING";
        public const string GeneralSettings = "org.jdownloader.settings.GeneralSettings";
        public const string DefaultFolderKey = "defaultdownloadfolder";
        public const string Cloudflare = "cloudflare";
    }

    public static class JdRoutes
    {
        public const string AddLinks = "/linkgrabberv2/addLinks";
        public const string GrabberLinks = "/linkgrabberv2/queryLinks";
        public const string GrabberPackages = "/linkgrabberv2/queryPackages";
        public const string GrabberRemove = "/linkgrabberv2/removeLinks";
        public const string GrabberEnabled = "/linkgrabberv2/setEnabled";
        public const string GrabberToDownloads = "/linkgrabberv2/moveToDownloadlist";
        public const string IsCollecting = "/linkgrabberv2/isCollecting";
        public const string DownloadLinks = "/downloadsV2/queryLinks";
        public const string DownloadPackages = "/downloadsV2/queryPackages";
        public const string DownloadRemove = "/downloadsV2/removeLinks";
        public const string DownloadEnabled = "/downloadsV2/setEnabled";
        public const string DownloadMove = "/downloadsV2/moveLinks";
        public const string Start = "/downloadcontroller/start";
        public const string Stop = "/downloadcontroller/stop";
        public const string State = "/downloadcontroller/getCurrentState";
        public const string Speed = "/downloadcontroller/getSpeedInBps";
        public const string ConfigGet = "/config/get";
    }

    public static class MediaFireApi
    {
        public const string Base = "https://www.mediafire.com/api/1.4/";
        public const string Success = "Success";
        public const string MoreChunks = "yes";
        public const string ContentFiles = "files";
        public const string ContentFolders = "folders";
        public const string FolderPattern = @"mediafire\.com/folder/([a-z0-9]+)(?:[^#]*)(?:#([a-z0-9]+))?";
        public const string FilePattern = @"mediafire\.com/(?:file|file_premium|download)/([a-z0-9]+)";
        public const string AnyKeyPattern = FilePattern + @"|download\d+\.mediafire\.com/[^/]+/([a-z0-9]+)/";
    }

    public static class WebConstants
    {
        public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0 Safari/537.36";
        public const string ChallengeHeader = "cf-mitigated";
        public const string ChallengeValue = "challenge";
        public const string HtmlType = "text/html";
        public const string UrlPattern = @"https?://\S+";
        public const string ReadHtmlScript = "document.documentElement.outerHTML";
        public const string CloudflareHost = "cloudflare.com";
    }

    public static class RuleKeys
    {
        public const string Match = "match";
        public const string Element = "element";
        public const string Attr = "attr";
        public const string Render = "render";
        public const string RenderAuto = "auto";
        public const string RenderHttp = "http";
        public const string RenderBrowser = "browser";
        public const string Base64Suffix = ":base64";
        public const char Comment = ';';
    }
}
