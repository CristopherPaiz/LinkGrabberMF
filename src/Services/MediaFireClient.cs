using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using LinkGrab.Constants;
using LinkGrab.Models;

namespace LinkGrab.Services
{
    // Lists MediaFire folders through its public API and expands pasted urls into items.
    public static class MediaFireClient
    {
        const string FolderInfoQuery = "folder/get_info.php?folder_key={0}";
        const string FolderContentQuery = "folder/get_content.php?folder_key={0}&content_type={1}&chunk={2}";
        const string FileInfoQuery = "file/get_info.php?quick_key={0}";
        const string FormatSuffix = "&response_format=json";

        // Pulls the file key out of any MediaFire url, page or direct.
        public static string KeyOf(string url)
        {
            Match match = Regex.Match(url ?? "", MediaFireApi.AnyKeyPattern, RegexOptions.IgnoreCase);
            if (!match.Success) return null;
            return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
        }

        public static FileItem FromKey(string key, string folder)
        {
            return ToItem(Call(string.Format(FileInfoQuery, key)).file_info, folder);
        }

        public static List<FileItem> Expand(string url)
        {
            Match folder = Regex.Match(url, MediaFireApi.FolderPattern, RegexOptions.IgnoreCase);
            if (folder.Success)
            {
                string key = folder.Groups[2].Success ? folder.Groups[2].Value : folder.Groups[1].Value;
                var items = new List<FileItem>();
                WalkFolder(key, SafeName(Call(string.Format(FolderInfoQuery, key)).folder_info.name), items);
                return items;
            }

            Match file = Regex.Match(url, MediaFireApi.FilePattern, RegexOptions.IgnoreCase);
            if (file.Success) return new List<FileItem> { FromKey(file.Groups[1].Value, "") };

            if (RuleSet.For(url) == null) throw new Exception(Strings.ErrorNoRule);
            return new List<FileItem> { new FileItem { Key = url, Name = url, PageUrl = url } };
        }

        static void WalkFolder(string key, string path, List<FileItem> items)
        {
            foreach (ApiFile file in ListAll(key, MediaFireApi.ContentFiles, c => c.files))
                items.Add(ToItem(file, path));

            foreach (ApiFolder sub in ListAll(key, MediaFireApi.ContentFolders, c => c.folders))
                WalkFolder(sub.folderkey, Path.Combine(path, SafeName(sub.name)), items);
        }

        static List<T> ListAll<T>(string key, string type, Func<ApiContent, List<T>> select)
        {
            var all = new List<T>();
            for (int chunk = 1; ; chunk++)
            {
                ApiContent content = Call(string.Format(FolderContentQuery, key, type, chunk)).folder_content;
                all.AddRange(select(content) ?? new List<T>());
                if (content.more_chunks != MediaFireApi.MoreChunks) return all;
            }
        }

        static FileItem ToItem(ApiFile file, string folder)
        {
            return new FileItem
            {
                Key = file.quickkey,
                Name = file.filename,
                Size = file.size,
                PageUrl = file.links.normal_download,
                Folder = folder
            };
        }

        static ApiResponse Call(string query)
        {
            ApiResponse response;
            try
            {
                response = Json.Read<ApiEnvelope>(Http.Get(MediaFireApi.Base + query + FormatSuffix)).response;
            }
            catch (WebException e)
            {
                throw new Exception(string.Format(Strings.ErrorMediaFireRejected, e.Message));
            }
            if (response.result != MediaFireApi.Success) throw new Exception(string.Format(Strings.ErrorMediaFire, response.message));
            return response;
        }

        static string SafeName(string name)
        {
            return string.Join("_", name.Split(Path.GetInvalidFileNameChars())).Trim();
        }

        // Field names mirror MediaFire's JSON.
        class ApiEnvelope { public ApiResponse response = null; }

        class ApiResponse
        {
            public string result = null;
            public string message = null;
            public ApiFolderInfo folder_info = null;
            public ApiFile file_info = null;
            public ApiContent folder_content = null;
        }

        class ApiFolderInfo { public string name = null; }
        class ApiFolder { public string folderkey = null; public string name = null; }
        class ApiLinks { public string normal_download = null; }

        class ApiFile
        {
            public string quickkey = null;
            public string filename = null;
            public long size = 0;
            public ApiLinks links = null;
        }

        class ApiContent
        {
            public string more_chunks = null;
            public List<ApiFolder> folders = null;
            public List<ApiFile> files = null;
        }
    }
}
