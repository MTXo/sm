using MauiApp1.Scripts;
using System.Collections.ObjectModel;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        Scripts.AppInfo current = new Scripts.AppInfo();
        bool _popupOpen = false;
        public ObservableCollection<ZipArchiveInfo> Saves { get; set; } = new();
        public MainPage()
        {
            InitializeComponent();
            SavesCollectionView.ItemsSource = Saves;
            AppCollectionView.ItemsSource = AppScript.apps;

            LoadSaves(); // tymczasowe wczytywanie zapisów do wyświetlenia, docelowo będzie to pobieranie z folderu z zapisami i formatowanie ich do listy Saves, która jest powiązana z interfejsem użytkownika (SavesCollectionView)
            
        }

        private void LoadSaves() // tymczasowa funkcja do wczytywania przykładowych zapisów, docelowo będzie to pobieranie z folderu z zapisami i formatowanie ich do listy Saves, która jest powiązana z interfejsem użytkownika (SavesCollectionView)
        {
            Saves.Clear();

            Saves.Add(new ZipArchiveInfo
            {
                FullPath = @"C:\test1.zip",
                FileName = "test1.zip",
                Timestamp = DateTime.Now,
                GameName = "Game 1",
                Description = "Save 1",
                FileSize = "354382"
            });

            Saves.Add(new ZipArchiveInfo
            {
                FullPath = @"C:\test2.zip",
                FileName = "test2.zip",
                Timestamp = DateTime.Now.AddDays(-1),
                GameName = "Game 2",
                Description = "Save 2",
                FileSize = "29555555"
            });
        }

        private void AddSave_Clicked(object sender, EventArgs e)
        {
            if (current.ExePath != null && current.ExePath != "")
            {
                Backuper.Backup(
                    Path.GetDirectoryName(current.ExePath) ?? "",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameSaves"),
                    current.Name,
                    DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")
                );
            }
        }
        private void AddApp_Clicked(object sender, EventArgs e)
        {
            AppScript.apps.Add(new Scripts.AppInfo { Name = "New App", ExePath = "", SteamAppId = 0 });
        }
        async void OpenPopup_Clicked(object sender, EventArgs e)
        {
            steamAID.Text = "";
            exePathEntry.Text = "";

            ProcessPicker.Title = "Wybierz proces... ( Ładowanie listy procesów )";
            _ = LoadProcessesAsync();

            name_settings.Text = current.Name;
            exePathEntry.Text = current.ExePath;
            steamAID.Text = current.SteamAppId > 0 ? current.SteamAppId.ToString() : "";

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

            gameBanner.Source = await HTMLConnection.GetImageSourceAsync(current.SteamAppId.ToString());
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
                current = (Scripts.AppInfo)e.CurrentSelection[0];
                DetailsGrid.IsVisible = true;
                gameBanner.Source = await HTMLConnection.GetImageSourceAsync(current.SteamAppId.ToString()); // pobieranie zdjęcia do baneru
            }

        }
        async void DeleteCurrent_Clicked(object sender, EventArgs e)
        {
            AppScript.apps.Remove(current);
            AppCollectionView.SelectedItem = null;
            DetailsGrid.IsVisible = false;
            ClosePopup_Clicked(sender, e);
        }

        private void Name_Unfocused(object sender, FocusEventArgs e)
        {
            current.Name = name_settings.Text;
        }

        private void ProcessPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (_popupOpen)
                {
                    exePathEntry.Text = ProcessHelpers.GetProcessPath(ProcessPicker.SelectedItem?.ToString() ?? "") ?? "";
                    current.ExePath = exePathEntry.Text;
                    steamAID.Text = SteamHelper.GetSteamAppIdFromExe(current.ExePath)?.ToString() ?? "0";
                    current.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
                }
            } 
            catch 
            { 
                // nic   
            }
        }

        private void exePathEntry_Unfocused(object sender, FocusEventArgs e)
        {
            current.ExePath = exePathEntry.Text;
            steamAID.Text = SteamHelper.GetSteamAppIdFromExe(current.ExePath)?.ToString() ?? "0";
            current.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
        }
        private void savePathEntry_Unfocused(object sender, FocusEventArgs e)
        {
            current.SavePath = savePathEntry.Text;
        }
        private void steamAIDEntry_Unfocused(object sender, FocusEventArgs e)
        {
            current.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
        }
        public void SwitchBranch_Clicked(object sender, EventArgs e)
        {
            if (BranchEntry.IsEnabled == false)
            {
                BranchEntry.IsEnabled = true;
                SaveBranchButton.IsEnabled = true;
                BranchEntryStack.IsVisible = true;
                BranchPicker.IsEnabled = false;
                BranchPicker.IsVisible = false;
            }
            else
            {
                BranchEntry.IsEnabled = false;
                BranchEntryStack.IsVisible = false;
                SaveBranchButton.IsEnabled = false;
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

        }
    }
}
