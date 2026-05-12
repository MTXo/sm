using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace MauiApp1.Scripts
{
    public class AppInfo : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        private string _exePath = string.Empty;
        public string ExePath
        {
            get => _exePath;
            set
            {
                _exePath = value;
                OnPropertyChanged();
            }
        }
        public int SteamAppId { get; set; }
        public string Shortcut => JustHelpers.GetShortcut(Name);

        private string _savePath = string.Empty;

        public string SavePath
        {
            get => _savePath;
            set
            {
                _savePath = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public static class AppScript
    {
        public static int page = 1;

        public static ObservableCollection<AppInfo> apps = new ObservableCollection<AppInfo>();


    }
}
