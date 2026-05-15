using MauiApp1.Scripts;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        Scripts.AppInfo currentGame = new Scripts.AppInfo();
        Scripts.BranchInfo currentBranch = new Scripts.BranchInfo();
        bool _popupOpen = false;
        public MainPage()
        {
            Database.CreateDatabase();
            InitializeComponent();
            SavesCollectionView.ItemsSource = AppScript.saves;
            AppCollectionView.ItemsSource = AppScript.apps;

            LoadStartData(); // tymczasowe wczytywanie zapisów do wyświetlenia, docelowo będzie to pobieranie z folderu z zapisami i formatowanie ich do listy Saves, która jest powiązana z interfejsem użytkownika (SavesCollectionView)
            
        }

        private void LoadStartData() // tymczasowa funkcja do wczytywania przykładowych zapisów, docelowo będzie to pobieranie z folderu z zapisami i formatowanie ich do listy Saves, która jest powiązana z interfejsem użytkownika (SavesCollectionView)
        {
            AppScript.saves.Clear();
            AppScript.apps.Clear();

            foreach (var game in Database.GetAllGames())
            {
                AppScript.apps.Add(new Scripts.AppInfo
                {
                    Id = game.Id,
                    Name = game.Name,
                    ExePath = game.GamePath,
                    SavePath = game.SavePath,
                    SteamAppId = game.SteamAppId,
                    AutoSave = game.AutoSave,
                    AutoSaveInterval = game.AutoSavePeriod
                });
            }
        }

        private void AddSave_Clicked(object sender, EventArgs e)
        {
            if (currentGame.ExePath != null && currentGame.ExePath != "")
            {
                Backuper.Backup(
                    Path.GetDirectoryName(currentGame.ExePath) ?? "",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameSaves"),
                    currentGame.Name,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
                );
            }
        }
        private void AddApp_Clicked(object sender, EventArgs e)
        {
            
            Database.AddGame("New App", 0, "", "", false, 0);
            int lastid = (Database.GetGameCount() > 0 ? Database.GetAllGames().Last().Id : 1) - 1; // pobieranie id ostatnio dodanej gry, jeśli nie ma żadnej gry to id będzie 1 i to jest odejmowane o 1, ponieważ id w bazie danych zaczyna się od 1, a w aplikacji chcemy, żeby zaczynało się od 0, więc odejmujemy 1 od id pobranego z bazy danych
            AppScript.apps.Add(new Scripts.AppInfo { Id = lastid, Name = "New App", ExePath = "", SteamAppId = 0 });

        }
        async void OpenPopup_Clicked(object sender, EventArgs e)
        {
            steamAID.Text = "";
            exePathEntry.Text = "";
            savePathEntry.Text = "";

            ProcessPicker.Title = "Wybierz proces... ( Ładowanie listy procesów )";
            _ = LoadProcessesAsync();

            name_settings.Text = currentGame.Name;
            exePathEntry.Text = currentGame.ExePath;
            savePathEntry.Text = currentGame.SavePath;
            steamAID.Text = currentGame.SteamAppId > 0 ? currentGame.SteamAppId.ToString() : "";

            PopupOverlay.IsVisible = true;

            // stan początkowy
            PopupOverlay.Opacity = 0;

            PopupContent.Scale = 0.8;
            PopupContent.Opacity = 0;
            PopupContent.TranslationY = 50;

            await Task.WhenAll(
                PopupOverlay.FadeToAsync(1, 200),
                PopupContent.FadeToAsync(1, 200),
                PopupContent.ScaleToAsync(1, 250, Easing.CubicOut),
                PopupContent.TranslateToAsync(0, 0, 250, Easing.CubicOut)
            );
            _popupOpen = true;
        }

        async Task LoadProcessesAsync()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessPicker.ItemsSource = null;
                ProcessPicker.SelectedIndex = -1;
                ProcessPicker.SelectedItem = null;
            });

            var processes = await Task.Run(() => ProcessHelpers.GetUserProcesses());

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessPicker.ItemsSource = processes;
                ProcessPicker.Title = "Wybierz proces...";
            });
        }

        async void ClosePopup_Clicked(object sender, EventArgs e)
        {
            _popupOpen = false;

            await Task.WhenAll(
                PopupOverlay.FadeToAsync(0, 200),
                PopupContent.FadeToAsync(0, 150),
                PopupContent.ScaleToAsync(0.8, 150, Easing.CubicIn),
                PopupContent.TranslateToAsync(0, 50, 150, Easing.CubicIn)
            );

            PopupOverlay.IsVisible = false;

            gameBanner.Source = await HTMLConnection.GetImageSourceAsync(currentGame.SteamAppId.ToString());
        }

        async void OpenPopupSettings_Clicked(object sender, EventArgs e)
        { 
            ClosePopup_Clicked(sender, e);

            PopupOverlaySettings.IsVisible = true;

            // stan początkowy
            PopupOverlaySettings.Opacity = 0;

            PopupContentSettings.Scale = 0.8;
            PopupContentSettings.Opacity = 0;
            PopupContentSettings.TranslationY = 50;

            await Task.WhenAll(
                PopupOverlaySettings.FadeToAsync(1, 200),
                PopupContentSettings.FadeToAsync(1, 200),
                PopupContentSettings.ScaleToAsync(1, 250, Easing.CubicOut),
                PopupContentSettings.TranslateToAsync(0, 0, 250, Easing.CubicOut)
            );
        }

        async void ClosePopupSettings_Clicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                PopupOverlaySettings.FadeToAsync(0, 200),
                PopupContentSettings.FadeToAsync(0, 150),
                PopupContentSettings.ScaleToAsync(0.8, 150, Easing.CubicIn),
                PopupContentSettings.TranslateToAsync(0, 50, 150, Easing.CubicIn)
            );
            PopupOverlaySettings.IsVisible = false;
        }
        
        private async void AppCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                currentGame = (Scripts.AppInfo)e.CurrentSelection[0];
                DetailsGrid.IsVisible = true;
                gameBanner.Source = await HTMLConnection.GetImageSourceAsync(currentGame.SteamAppId.ToString()); // pobieranie zdjęcia do baneru
            }

        }
        async void DeleteCurrent_Clicked(object sender, EventArgs e)
        {
            AppScript.apps.Remove(currentGame);
            Database.DeleteGame(currentGame.Id);
            AppCollectionView.SelectedItem = null;
            DetailsGrid.IsVisible = false;
            ClosePopup_Clicked(sender, e);
        }

        private void Name_Unfocused(object sender, FocusEventArgs e)
        {
            currentGame.Name = name_settings.Text;
            Database.UpdateGame(currentGame.Id, currentGame.Name, currentGame.SteamAppId, currentGame.ExePath, currentGame.SavePath, currentGame.AutoSave, currentGame.AutoSaveInterval);
        }

        private void ProcessPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_popupOpen)
                {
                    exePathEntry.Text = ProcessHelpers.GetProcessPath(ProcessPicker.SelectedItem?.ToString() ?? "") ?? "";
                    currentGame.ExePath = exePathEntry.Text;
                    steamAID.Text = SteamHelper.GetSteamAppIdFromExe(currentGame.ExePath)?.ToString() ?? "0";
                    currentGame.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
                    Database.UpdateGame(currentGame.Id, currentGame.Name, currentGame.SteamAppId, currentGame.ExePath, currentGame.SavePath, currentGame.AutoSave, currentGame.AutoSaveInterval);
                }
            } 
            catch 
            { 
                // nic   
            }
        }

        private void exePathEntry_Unfocused(object sender, FocusEventArgs e)
        {
            currentGame.ExePath = exePathEntry.Text;
            steamAID.Text = SteamHelper.GetSteamAppIdFromExe(currentGame.ExePath)?.ToString() ?? "0";
            currentGame.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
            Database.UpdateGame(currentGame.Id, currentGame.Name, currentGame.SteamAppId, currentGame.ExePath, currentGame.SavePath, currentGame.AutoSave, currentGame.AutoSaveInterval);
        }
        private void savePathEntry_Unfocused(object sender, FocusEventArgs e)
        {
            currentGame.SavePath = savePathEntry.Text;
            Database.UpdateGame(currentGame.Id, currentGame.Name, currentGame.SteamAppId, currentGame.ExePath, currentGame.SavePath, currentGame.AutoSave, currentGame.AutoSaveInterval);
        }
        private void steamAIDEntry_Unfocused(object sender, FocusEventArgs e)
        {
            currentGame.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
            Database.UpdateGame(currentGame.Id, currentGame.Name, currentGame.SteamAppId, currentGame.ExePath, currentGame.SavePath, currentGame.AutoSave, currentGame.AutoSaveInterval);
        }
        public void SwitchBranch_Clicked(object sender, EventArgs e)
        {
            if (BranchEntry.IsEnabled == false)
            {
                BranchEntry.IsEnabled = true;
                SaveBranchButton.IsEnabled = true;
                AddBranchButton.IsEnabled = true;
                BranchEntryStack.IsVisible = true;
                BranchPicker.IsEnabled = false;
                BranchPicker.IsVisible = false;
            }
            else
            {
                BranchEntry.IsEnabled = false;
                BranchEntryStack.IsVisible = false;
                SaveBranchButton.IsEnabled = false;
                AddBranchButton.IsEnabled = false;
                BranchPicker.IsEnabled = true;
                BranchPicker.IsVisible = true;
            }
        }
        void OnBorderSizeChanged(object sender, EventArgs e)
        {
            var view = (VisualElement)sender;

            if (view.Width <= 0 || view.Height <= 0)
                return;

            // bierzemy mniejszy wymiar, zawsze idealny kwadrat
            double size = Math.Min(view.Width, view.Height);

            // zabezpieczenie przed zapętleniem
            if (Math.Abs(view.Width - view.Height) < 0.5)
                return;

            view.WidthRequest = size;
            view.HeightRequest = size;
        }

        private void SaveBranchButton_Clicked(object sender, EventArgs e)
        {
            Database.GetAllBranches().Where(b => b.GameId == currentGame.Id).ToList().ForEach(b =>
            {
                if(b.Name == BranchEntry.Text)
                {
                    return; // nazwa już istnieje, nie chcemy duplikatów
                }
            });
            
        }
        private void AddBranchButton_Clicked(object sender, EventArgs e)
        {
            Database.GetAllBranches().Where(b => b.GameId == currentGame.Id).ToList().ForEach(b =>
            {
                if (b.Name == BranchEntry.Text)
                {
                    return; // nazwa już istnieje, nie chcemy duplikatów
                }
            });

        }
    }
}
