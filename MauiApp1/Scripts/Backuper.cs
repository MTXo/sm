using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace MauiApp1.Scripts
{
    public struct ZipArchiveInfo
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }

        public DateTime Timestamp { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
    }
    class Backuper
    {
        public static void Backup(
            string sourceFolderPath,
                    string destinationFolderPath,
                    string name1,
                    string name2)
        {
            // Walidacja
            if (!Directory.Exists(sourceFolderPath))
                throw new DirectoryNotFoundException($"Folder źródłowy nie istnieje: {sourceFolderPath}");

            if (!Directory.Exists(destinationFolderPath))
                Directory.CreateDirectory(destinationFolderPath);

            // Format daty: hh-mm-ss_dd-mm-yyyy
            string timestamp = DateTime.Now.ToString("HH-mm-ss_dd-MM-yyyy");

            // Oczyszczenie nazw z niedozwolonych znaków
            name1 = SanitizeFileName(name1);
            name2 = SanitizeFileName(name2);

            // Finalna nazwa pliku
            string zipFileName = $"{timestamp}_{name1}_{name2}.zip";

            // Pełna ścieżka
            string zipFullPath = Path.Combine(destinationFolderPath, zipFileName);

            // Kompresja
            ZipFile.CreateFromDirectory(sourceFolderPath, zipFullPath, CompressionLevel.Optimal, true);
        }

        private static string SanitizeFileName(string input)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '?');
            }
            return input;
        }

        public static void Restore(
            string targetFolderPath,
            string zipFilePath)
        {
            // Walidacja
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException($"Nie znaleziono pliku ZIP: {zipFilePath}");

            // Usuń folder jeśli istnieje
            if (Directory.Exists(targetFolderPath))
            {
                Directory.Delete(targetFolderPath, true);
            }

            // Utwórz folder na nowo
            Directory.CreateDirectory(targetFolderPath);

            // Rozpakuj ZIP
            ZipFile.ExtractToDirectory(zipFilePath, targetFolderPath);
        }
        public static List<ZipArchiveInfo> GetArchives(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder nie istnieje: {folderPath}");

            var result = new List<ZipArchiveInfo>();

            var files = Directory.GetFiles(folderPath, "*.zip");

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);

                // Rozbijamy: timestamp_name1_name2
                var parts = fileName.Split('_');

                if (parts.Length < 4)
                    continue; // nie pasuje do formatu

                try
                {
                    // timestamp = hh-mm-ss_dd-mm-yyyy
                    string timestampString = $"{parts[0]}_{parts[1]}";

                    DateTime timestamp = DateTime.ParseExact(
                        timestampString,
                        "HH-mm-ss_dd-MM-yyyy",
                        CultureInfo.InvariantCulture
                    );

                    // name1 i name2 (mogą mieć underscore, więc składamy resztę)
                    string name1 = parts[2];
                    string name2 = string.Join("_", parts.Skip(3));

                    result.Add(new ZipArchiveInfo
                    {
                        FullPath = file,
                        FileName = Path.GetFileName(file),
                        Timestamp = timestamp,
                        Name1 = name1,
                        Name2 = name2
                    });
                }
                catch
                {
                    // ignorujemy pliki które nie pasują
                }
            }

            // opcjonalnie sortowanie (najnowsze pierwsze)
            return result
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }
    }
}
