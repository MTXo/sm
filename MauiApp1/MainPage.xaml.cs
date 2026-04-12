using MauiApp1.Scripts;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            AppCollectionView.ItemsSource = AppScript.apps;
        }

        private void AddApp_Clicked(object sender, EventArgs e)
        {
            AppScript.apps.Add(new Scripts.AppInfo { Name = "New App", ExePath = "", SteamAppId = 0 });
        }
    }
}
