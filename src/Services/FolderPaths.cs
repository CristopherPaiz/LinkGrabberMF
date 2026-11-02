using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkGrab.Services
{
    // Item folders are relative paths such as "Parte 1\Season 01"; "" means no folder.
    public static class FolderPaths
    {
        public const char Separator = '\\';
        const char JdFractionSlash = '⁄';

        public static string[] Parts(string path) { return path.Split(Separator); }

        public static string Leaf(string path)
        {
            string[] parts = Parts(path);
            return parts[parts.Length - 1];
        }

        public static string Parent(string path)
        {
            int last = path.LastIndexOf(Separator);
            return last < 0 ? "" : path.Substring(0, last);
        }

        public static string Join(IEnumerable<string> parts) { return string.Join(Separator.ToString(), parts); }

        public static int Depth(string path) { return path == "" ? -1 : Parts(path).Length - 1; }

        public static bool IsInside(string folder, string ancestor)
        {
            return folder == ancestor || folder.StartsWith(ancestor + Separator, StringComparison.Ordinal);
        }

        // Each folder from the root down to the path itself.
        public static IEnumerable<string> Chain(string path)
        {
            string[] parts = Parts(path);
            return Enumerable.Range(1, parts.Length).Select(count => Join(parts.Take(count)));
        }

        // JD2's own MediaFire crawler names packages "Parent⁄Child" with a fraction slash.
        public static string FromPackage(string package)
        {
            return package.Replace(JdFractionSlash, Separator).Replace('/', Separator);
        }
    }
}
