using MauiApp1.Scripts;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        Scripts.AppInfo current = new Scripts.AppInfo();
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
            ProcessPicker.Title = "Wybierz proces... ( Ładowanie listy procesów )";
            _ = LoadProcessesAsync();
            name_settings.Text = current.Name;
            exePathEntry.Text = current.ExePath;
            PopupOverlay.IsVisible = true;

            // stan początkowy
            PopupOverlay.Opacity = 0;

            PopupContent.Scale = 0.8;
            PopupContent.Opacity = 0;
            PopupContent.TranslationY = 50;

            // animacje równoległe
            await Task.WhenAll(
                PopupOverlay.FadeToAsync(1, 200),

                PopupContent.FadeToAsync(1, 200),
                PopupContent.ScaleToAsync(1, 250, Easing.CubicOut),
                PopupContent.TranslateToAsync(0, 0, 250, Easing.CubicOut)
            );
        }
        async Task LoadProcessesAsync()
        {
            var processes = await Task.Run(() => ProcessHelpers.GetUserProcesses());

            // wracamy na UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProcessPicker.ItemsSource = processes;
                ProcessPicker.Title = "Wybierz proces...";
            });
        }
        async void ClosePopup_Clicked(object sender, EventArgs e)
        {
            await Task.WhenAll(
                PopupOverlay.FadeToAsync(0, 200),

                PopupContent.FadeToAsync(0, 150),
                PopupContent.ScaleToAsync(0.8, 150, Easing.CubicIn),
                PopupContent.TranslateToAsync(0, 50, 150, Easing.CubicIn)
            );

            PopupOverlay.IsVisible = false;
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
            exePathEntry.Text = ProcessHelpers.GetProcessPath(ProcessPicker.SelectedItem?.ToString() ?? "") ?? "";
            current.ExePath = exePathEntry.Text;
        }

        private void exePathEntry_Unfocused(object sender, FocusEventArgs e)
        {
            current.ExePath = exePathEntry.Text;
        }
    }
}
