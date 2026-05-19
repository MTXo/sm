using MauiApp1.Scripts;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private ObservableCollection<SaveDisplayInfo> displaySaves = new ObservableCollection<SaveDisplayInfo>();

        Scripts.AppInfo currentGame = new Scripts.AppInfo();
        Scripts.BranchInfo currentBranch = new Scripts.BranchInfo();

        bool _popupOpen = false;


        private string _searchText = string.Empty;
        public MainPage()
        {

            InitializeComponent();
            Database.CreateDatabase();
            LoadStartData();

            // SavesCollectionView.ItemsSource = AppScript.saves;

            AppCollectionView.ItemsSource = AppScript.apps;
            BranchPicker.ItemsSource = AppScript.branches;
            BranchPicker.ItemDisplayBinding = new Binding("Name");

            AppCollectionView.SelectionChanged += AppCollectionView_SelectionChanged;
            SavesSearchBar.TextChanged += SavesSearchBar_TextChanged;

        }

        private void LoadStartData()
        {
            AppScript.saves.Clear();
            AppScript.apps.Clear();
            AppScript.branches.Clear();
            displaySaves.Clear();

            // Gry
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
                    AutoSaveInterval = game.AutoSavePeriod,
                    LastSelectedBranch = game.LastSelectedBranch,
                });
            }

            foreach (var branch in Database.GetAllBranches())
            {
                AppScript.branches.Add(new Scripts.BranchInfo
                {
                    Id = branch.Id,
                    Name = branch.Name,
                    GameId = branch.GameId
                });
            }

            // Save'y + Branch info
            var branchesDict = AppScript.branches.ToDictionary(b => b.Id, b => b);

            foreach (var save in Database.GetAllSaves())
            {
                var saveInfo = new Scripts.SaveInfo
                {
                    Id = save.Id,
                    FileName = save.FileName,
                    BranchId = save.BranchId,
                    SaveTime = save.Date
                };

                AppScript.saves.Add(saveInfo);

                branchesDict.TryGetValue(save.BranchId, out var branch);

                displaySaves.Add(new SaveDisplayInfo
                {
                    Save = saveInfo,
                    BranchName = branch?.Name ?? "Nieznana gałąź"
                });
            }

            AppCollectionView.ItemsSource = AppScript.apps;
            BranchPicker.ItemsSource = AppScript.branches;
            BranchPicker.ItemDisplayBinding = new Binding("Name");

            SavesCollectionView.ItemsSource = new ObservableCollection<SaveDisplayInfo>();
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
            int lastid = (Database.GetGameCount() > 0 ? Database.GetAllGames().Last().Id : 1); // pobieranie id ostatnio dodanej gry, jeśli nie ma żadnej gry to id będzie 1 i to jest odejmowane o 1, ponieważ id w bazie danych zaczyna się od 1, a w aplikacji chcemy, żeby zaczynało się od 0, więc odejmujemy 1 od id pobranego z bazy danych
            AppScript.apps.Add(new Scripts.AppInfo { Id = lastid, Name = "New App", ExePath = "", SteamAppId = 0 });
            Database.AddBranch("Default", lastid);
            AppScript.branches.Add(new Scripts.BranchInfo
            {
                Id = Database.GetAllBranches().Last().Id,
                Name = "Default",
                GameId = lastid,
            });
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
                BranchPicker.ItemsSource = AppScript.branches.Where(b => b.GameId == currentGame.Id).ToList();
                if(BranchPicker.ItemsSource.Cast<Scripts.BranchInfo>().Any())
                {
                    BranchPicker.SelectedIndex = currentGame.LastSelectedBranch;
                    currentBranch = (Scripts.BranchInfo)BranchPicker.SelectedItem;
                    BranchEntry.Text = currentBranch.Name;
                }
                DetailsGrid.IsVisible = true;
                gameBanner.Source = await HTMLConnection.GetImageSourceAsync(currentGame.SteamAppId.ToString()); // pobieranie zdjęcia do baneru
            }
            if (e.CurrentSelection.FirstOrDefault() is Scripts.AppInfo selectedGame)
            {
                currentGame = selectedGame;

                var gameBranchIds = AppScript.branches
                    .Where(b => b.GameId == selectedGame.Id)
                    .Select(b => b.Id)
                    .ToHashSet();

                var filtered = displaySaves
                    .Where(d => gameBranchIds.Contains(d.BranchId))
                    .ToList();

                SavesCollectionView.ItemsSource = filtered;
            }
            else
            {
                SavesCollectionView.ItemsSource = displaySaves; // jeśli nic nie wybrano
            }
            UpdateSavesList();
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
            if (String.IsNullOrEmpty(BranchEntry.Text))
            {
                return;
            }
            bool dont = false;
            Database.GetAllBranches().Where(b => b.GameId == currentGame.Id).ToList().ForEach(b =>
            {
                if(b.Name == BranchEntry.Text)
                {
                    dont = true;
                }
            });
            if(dont)
            {
                BranchEntry.Text = currentBranch.Name;
                return;
            }
            if (currentBranch.Id != -1)
            {
                Database.UpdateBranch(currentBranch.Id, BranchEntry.Text);
                try
                {
                    AppScript.branches.Where(b => b.Id == currentBranch.Id).FirstOrDefault()?.Name = BranchEntry.Text;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Gratulacje - właśnie znalazłeś błąd, który nie powinien się wydarzyć. Gratulacje! Oto szczegóły błędu: " + ex.Message);
                }
            }
            BranchPicker.ItemsSource = AppScript.branches.Where(b => b.GameId == currentGame.Id).ToList();
            currentBranch = AppScript.branches.Where(b => b.Id == currentBranch.Id).FirstOrDefault() ?? new Scripts.BranchInfo();
            BranchPicker.SelectedItem = currentBranch;

        }
        private void AddBranchButton_Clicked(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(BranchEntry.Text))
            {
                return;
            }
            bool doNot = false;
            Database.GetAllBranches().Where(b => b.GameId == currentGame.Id).ToList().ForEach(b =>
            {
                if (b.Name == BranchEntry.Text)
                {
                    doNot = true; // nazwa już istnieje, nie chcemy duplikatów
                }
            });
            if (doNot) return;
            Database.AddBranch(BranchEntry.Text, currentGame.Id);
            AppScript.branches.Add(new BranchInfo { Id = Database.GetAllBranches().Last().Id, Name = BranchEntry.Text, GameId = currentGame.Id });
            Database.UpdateGameSelectedBranch(currentGame.Id, Database.GetAllBranches().Last().Id);
            AppScript.apps.Where(g => g.Id == currentGame.Id).FirstOrDefault()?.LastSelectedBranch = Database.GetAllBranches().Last().Id;
            BranchPicker.ItemsSource = AppScript.branches.Where(b => b.GameId == currentGame.Id).ToList();
            BranchPicker.SelectedItem = AppScript.branches.Last();
            currentBranch = AppScript.branches.Last();
        }
        private void BranchPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BranchPicker.SelectedItem is Scripts.BranchInfo branch)
            {
                currentBranch = branch;
                BranchEntry.Text = branch.Name;

                Database.UpdateGameSelectedBranch(currentGame.Id, currentBranch.Id);
                AppScript.apps.Where(g => g.Id == currentGame.Id).FirstOrDefault()?.LastSelectedBranch = currentBranch.Id;

                UpdateSavesList();
            }
        }
        private void SaveSnapshot_Clicked(object sender, EventArgs e)
        {
            if (currentGame.Id != -1 && currentBranch.Id != -1 && !string.IsNullOrEmpty(currentGame.SavePath))
            {
                {
                    DateTime now = DateTime.Now;
                    string hash = AppScript.ReturnUniqueValue(now, currentBranch.Id.ToString());
                    Database.AddSave(now, currentBranch.Id, hash);
                    AppScript.saves.Add(new SaveInfo { Id = Database.GetAllSaves().Last().Id, FileName = hash, BranchId = currentBranch.Id, SaveTime = now });
                    Backuper.Backup(
                        Path.GetDirectoryName(currentGame.SavePath) ?? "",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MW Save Manager", "Backups"),
                        currentGame.Name,
                        hash
                    );
                }
            }
            RefreshNewSaveInDisplay();
        }
        private void RefreshNewSaveInDisplay()
        {
            var lastSaveFromDb = Database.GetAllSaves().LastOrDefault();
            if (lastSaveFromDb == null) return;

            var newSaveInfo = new Scripts.SaveInfo
            {
                Id = lastSaveFromDb.Id,
                FileName = lastSaveFromDb.FileName,
                BranchId = lastSaveFromDb.BranchId,
                SaveTime = lastSaveFromDb.Date
            };

            var newDisplay = new SaveDisplayInfo
            {
                Save = newSaveInfo,
                BranchName = currentBranch.Name 
            };

            displaySaves.Add(newDisplay);

            UpdateSavesList();
        }
        private void SavesSearchBar_TextChanged(object? sender, TextChangedEventArgs e)
        {
            string search = e.NewTextValue?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(search))
            {
                UpdateSavesList();
                return;
            }

            // Pobierz aktualnie wyświetlane save'y
            var currentList = SavesCollectionView.ItemsSource as IEnumerable<SaveDisplayInfo> ?? displaySaves;

            var filtered = currentList
                .Where(s =>
                    (s.FileName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.BranchName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.SaveTime.ToString("dd.MM.yyyy HH:mm")?.Contains(search, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            SavesCollectionView.ItemsSource = filtered;
        }
        private void UpdateSavesList()
        {
            if (currentGame?.Id <= 0)
            {
                SavesCollectionView.ItemsSource = new ObservableCollection<SaveDisplayInfo>();
                return;
            }

            var gameBranchIds = AppScript.branches
                .Where(b => b.GameId == currentGame?.Id)
                .Select(b => b.Id)
                .ToHashSet();

            var filtered = displaySaves
                .Where(d => gameBranchIds.Contains(d.BranchId))
                .ToList();

            if (currentBranch?.Id > 0)
            {
                filtered = filtered.Where(d => d.BranchId == currentBranch.Id).ToList();
            }

            // Sortowanie od najnowszego
            filtered = filtered.OrderByDescending(d => d.SaveTime).ToList();

            SavesCollectionView.ItemsSource = filtered;
        }
    }
}
