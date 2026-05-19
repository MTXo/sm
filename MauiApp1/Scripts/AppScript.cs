using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace MauiApp1.Scripts
{
    public class AppInfo : INotifyPropertyChanged
    {
        public int Id { get; set; } = -1;
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
        
        public bool AutoSave { get; set; }

        public int AutoSaveInterval { get; set; }

        public int LastSelectedBranch { get; set; } = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class BranchInfo : INotifyPropertyChanged
    {
        public int Id { get; set; } = -1;
        private string _name = string.Empty;
        private int _gameId;

        public string Name         {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }
        public int GameId
        {
            get => _gameId;
            set
            {
                _gameId = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class SaveInfo : INotifyPropertyChanged
    {
        public int Id { get; set; } = -1;
        private string _fileName = string.Empty;
        private int _branchId;
        private DateTime _saveTime;
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }
        public int BranchId
        {
            get => _branchId;
            set
            {
                _branchId = value;
                OnPropertyChanged();
            }
        }
        public DateTime SaveTime
        {
            get => _saveTime;
            set
            {
                _saveTime = value;
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
        public static ObservableCollection<BranchInfo> branches = new ObservableCollection<BranchInfo>();
        public static ObservableCollection<SaveInfo> saves = new ObservableCollection<SaveInfo>();

        public static string ReturnUniqueValue(DateTime date, string ID)
        {
            var result = default(byte[]);

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write(date.Ticks);
                    writer.Write(ID);
                }

                stream.Position = 0;

                using (var hash = SHA256.Create())
                {
                    result = hash.ComputeHash(stream);
                }
            }

            var text = new string[20];

            for (var i = 0; i < text.Length; i++)
            {
                text[i] = result[i].ToString("x2");
            }

            return string.Concat(text);
        }
    }
}
