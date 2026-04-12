using System;
using System.Collections.Generic;
using System.Text;
using MauiApp1;

namespace MauiApp1.Scripts
{
    internal class ShowSaves
    {
        public List<ZipArchiveInfo> GetFormattedSaves(string folderPath)
        {
            var saves = Backuper.GetArchives(folderPath);
            List<ZipArchiveInfo> sortedSaves = new List<ZipArchiveInfo>(saves);

            if (saves.Count == 0)
            {
                Console.WriteLine("Brak zapisów.");
                return sortedSaves;
            }

            foreach (var save in saves)
            {
                var saveLabels = new Border
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10),
                    Margin = new Thickness(5),
                    Content = CreateSaveGrid(save)
                };
                sortedSaves.Add(save);
            }

            return sortedSaves;
        }

        private Grid CreateSaveGrid(ZipArchiveInfo save)
        {
            string date = save.Timestamp.ToString("dd.MM.yyyy") ?? "—";
            string time = save.Timestamp.ToString("HH:mm:ss") ?? "—";
            string fileSize = FormatFileSize(save.FileSize);

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                },
            };
            var descLabel = CreateLabel($"{save.Description}");
            var nameLabel = CreateLabel($"{save.FileName}");
            var dateLabel = CreateLabel($"{date}");
            var timeLabel = CreateLabel($"{time}");
            var fileSizeLabel = CreateLabel($"{fileSize}");
            grid.Add(descLabel, 0, 0);
            grid.Add(nameLabel, 0, 1);
            grid.Add(dateLabel, 0, 2);
            grid.Add(timeLabel, 0, 3);
            grid.Add(fileSizeLabel, 0, 4);

            return grid;
        }

        private Label CreateLabel(string text)
        {
            var label = new Label
            {
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Black,
            };
            label.SetAppThemeColor(Label.TextColorProperty, Colors.Black, Colors.White);

            return label;
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            double kb = bytes / 1024.0;
            if (kb < 1024)
                return $"{kb:F1} KB";

            double mb = kb / 1024.0;
            if (mb < 1024)
                return $"{mb:F1} MB";

            double gb = mb / 1024.0;
            return $"{gb:F2} GB";
        }
    }
}
