using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp1.Scripts
{
    internal class HTMLConnection
    {
        public static async Task<Image?> GetImageAsync(ZipArchiveInfo game)
        {
            long steamAppId = GetSteamAppId(game);
            if (steamAppId == 0)
                return null;

            ImageSource? imageSource = await GetImageSourceAsync(steamAppId.ToString());
            if (imageSource == null)
                return null;

            return new Image { Source = imageSource };
        }
        public static async Task<ImageSource?> GetImageSourceAsync(string steamAppId)
        {
            try
            {
                var httpClient = new HttpClient();
                var url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{steamAppId}/library_600x900.jpg";
                var bytes = await httpClient.GetByteArrayAsync(url);
                ImageSource image = ImageSource.FromStream( () => new MemoryStream(bytes));
                
                return image;
            }
            catch
            {
                return null;
            }
        }

        private static long GetSteamAppId(ZipArchiveInfo game)
        {
            if (game.FullPath == null || string.IsNullOrEmpty(game.FullPath) || !File.Exists(game.FullPath))
                return 0;

            string gameFolder = game.FullPath;


            string appIdTxt = Path.Combine(gameFolder, "steam_appid.txt");
            if (File.Exists(appIdTxt))
            {
                string content = File.ReadAllText(appIdTxt).Trim();
                if (long.TryParse(content, out long id) && id > 0)
                    return id;
            }
            return 0;
        }
    }
}
