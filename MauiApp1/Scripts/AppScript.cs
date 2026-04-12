using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MauiApp1.Scripts
{
    public struct AppInfo
    {
        public string Name { get; set; }
        public string ExePath { get; set; }
        public int SteamAppId { get; set; }
    }
    public static class AppScript
    {
        public static int page = 1;

        public static ObservableCollection<AppInfo> apps = new ObservableCollection<AppInfo>();


    }
}
