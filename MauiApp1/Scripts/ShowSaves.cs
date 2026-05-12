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
            var saves = Backuper.GetArchives(folderPath); // pobieranie listy zapisów z folderu
            List<ZipArchiveInfo> savesList = new List<ZipArchiveInfo>(saves); // tworzenie listy zapisów do wyświetlenia

            // sprawdzanie czy nie będzie problemu z brakiem zapisów
            if (saves.Count == 0)
            {
                Console.WriteLine("Brak zapisów.");
                return savesList;
            }

            // dla każdego zapisu tworzenie elementu UI do wyświetlenia informacji o zapisie
            foreach (var save in saves)
            {
                var saveLabels = new Border
                {
                    BackgroundColor = Colors.LightGray,
                    Padding = new Thickness(10),
                    Margin = new Thickness(5),
                    Content = CreateSaveGrid(save)
                };
                savesList.Add(save);
            }

            return savesList;
        }

        private Grid CreateSaveGrid(ZipArchiveInfo save)
        {
            string date = save.Timestamp.ToString("dd.MM.yyyy") ?? "—"; // data
            string time = save.Timestamp.ToString("HH:mm:ss") ?? "—"; // godzina
            string fileSize = save.FileSize; // rozmiar który jest formatowany do czytelnej formy (B, KB, MB, GB)

            var grid = new Grid
            {
                // definiowanie układu siatki z 5 kolumnami o równych szerokościach
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                },
            };
            // tworzenie etykiet dla opisu, nazwy pliku, daty, czasu i rozmiaru pliku
            var descLabel = CreateLabel($"{save.Description}");
            var nameLabel = CreateLabel($"{save.FileName}");
            var dateLabel = CreateLabel($"{date}");
            var timeLabel = CreateLabel($"{time}");
            var fileSizeLabel = CreateLabel($"{fileSize}");
            // dodawanie etykiet do siatki w odpowiednich kolumnach
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
    }
}
