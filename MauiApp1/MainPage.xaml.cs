using MauiApp1.Scripts;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            OnAppearing();

        }
        private async void OnAppearing()   // lub w konstruktorze + Loaded event
        {
            myImage.Source = await HTMLConnection.GetImageSourceAsync("322170");
        }
    }
}
