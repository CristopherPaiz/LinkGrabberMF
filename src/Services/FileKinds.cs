using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LinkGrab.Services
{
    public enum FileKind { Other, Archive, Video, Audio, Image, Pdf, Document, Disc, Program }

    // Tells what a file is from its name, to show a fitting icon.
    public static class FileKinds
    {
        // Split archives: name.r00, name.001, name.7z.001.
        static readonly Regex ArchivePart = new Regex(@"\.(r\d{2}|\d{3})$", RegexOptions.IgnoreCase);

        static readonly Dictionary<string, FileKind> ByExtension = Build(
            Kind(FileKind.Archive, "rar", "zip", "7z", "tar", "gz", "tgz", "bz2", "xz", "zst", "cab", "lzh"),
            Kind(FileKind.Video, "mp4", "mkv", "avi", "mov", "wmv", "flv", "webm", "m4v", "ts", "mpg", "mpeg", "3gp", "vob"),
            Kind(FileKind.Audio, "mp3", "flac", "wav", "aac", "ogg", "m4a", "opus", "wma", "alac", "aiff"),
            Kind(FileKind.Image, "png", "jpg", "jpeg", "gif", "webp", "bmp", "tif", "tiff", "psd", "svg", "heic", "ico"),
            Kind(FileKind.Pdf, "pdf"),
            Kind(FileKind.Document, "doc", "docx", "txt", "rtf", "odt", "xls", "xlsx", "csv", "ppt", "pptx", "epub", "mobi", "cbr", "cbz", "srt", "ass", "vtt", "md"),
            Kind(FileKind.Disc, "iso", "img", "bin", "cue", "mdf", "mds", "nrg", "vhd", "vhdx"),
            Kind(FileKind.Program, "exe", "msi", "apk", "xapk", "bat", "cmd", "jar", "appx", "msix", "dmg", "deb"));

        static KeyValuePair<FileKind, string[]> Kind(FileKind kind, params string[] extensions)
        {
            return new KeyValuePair<FileKind, string[]>(kind, extensions);
        }

        static Dictionary<string, FileKind> Build(params KeyValuePair<FileKind, string[]>[] kinds)
        {
            return kinds.SelectMany(k => k.Value.Select(e => new KeyValuePair<string, FileKind>(e, k.Key)))
                .ToDictionary(p => p.Key, p => p.Value);
        }

        public static FileKind Of(string name)
        {
            if (string.IsNullOrEmpty(name)) return FileKind.Other;
            if (ArchivePart.IsMatch(name)) return FileKind.Archive;

            // Not Path.GetExtension: names from the web may hold characters a path can't.
            int dot = name.LastIndexOf('.');
            string extension = dot < 0 ? "" : name.Substring(dot + 1).ToLowerInvariant();
            FileKind kind;
            return ByExtension.TryGetValue(extension, out kind) ? kind : FileKind.Other;
        }
    }
}
