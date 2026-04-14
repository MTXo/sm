using MauiApp1.Scripts;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        Scripts.AppInfo current = new Scripts.AppInfo();
        bool _popupOpen = false;
        public MainPage()
        {
            InitializeComponent();
            AppCollectionView.ItemsSource = AppScript.apps;
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

        private void AppCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0)
            {
                current = (Scripts.AppInfo)e.CurrentSelection[0];
                DetailsGrid.IsVisible = true;
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
        private void steamAIDEntry_Unfocused(object sender, FocusEventArgs e)
        {
            current.SteamAppId = int.TryParse(steamAID.Text, out int id) ? id : 0;
        }
    }
}
