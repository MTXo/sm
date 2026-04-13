using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace MauiApp1.Scripts
{
    public static class SteamHelper
    {
        public static int? GetSteamAppIdFromExe(string exePath)
        {
            if (!File.Exists(exePath))
                return null;

            string startDir = Path.GetDirectoryName(exePath)!;

            // 1. sprawdź steam_appid.txt w 3 poziomach (0,1,2)
            var txtResult = SearchSteamAppIdTxt(startDir);
            if (txtResult != null)
                return txtResult;

            // 2. sprawdź appmanifest (Steam standard)
            var manifestResult = SearchAppManifest(startDir);
            if (manifestResult != null)
                return manifestResult;

            return null;
        }

        private static int? SearchSteamAppIdTxt(string root)
        {
            foreach (var dir in EnumerateDepth(root, 2))
            {
                string file = Path.Combine(dir, "steam_appid.txt");

                if (File.Exists(file))
                {
                    string content = File.ReadAllText(file).Trim();

                    if (int.TryParse(content, out int id))
                        return id;
                }
            }

            return null;
        }

        private static int? SearchAppManifest(string root)
        {
            foreach (var dir in EnumerateDepth(root, 2))
            {
                var files = Directory.GetFiles(dir, "appmanifest_*.acf");

                foreach (var file in files)
                {
                    string text = File.ReadAllText(file);

                    // Steam format:
                    // "appid"        "123456"
                    var match = Regex.Match(text, "\"appid\"\\s+\"(\\d+)\"");

                    if (match.Success && int.TryParse(match.Groups[1].Value, out int id))
                        return id;
                }
            }

            return null;
        }

        private static IEnumerable<string> EnumerateDepth(string root, int depth)
        {
            var list = new Queue<(string path, int level)>();
            list.Enqueue((root, 0));

            while (list.Count > 0)
            {
                var (path, level) = list.Dequeue();

                yield return path;

                if (level >= depth)
                    continue;

                try
                {
                    foreach (var dir in Directory.GetDirectories(path))
                        list.Enqueue((dir, level + 1));
                }
                catch
                {
                    // brak dostępu
                }
            }
        }
    }
}

