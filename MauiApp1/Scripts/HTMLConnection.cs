using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp1.Scripts
{
    internal class HTMLConnection
    {
        public static async Task<ImageSource?> GetImageSourceAsync(string steamAppId)
        {
            try
            {
                var httpClient = new HttpClient();
                var url = $"https://cdn.cloudflare.steamstatic.com/steam/apps/{steamAppId}/library_600x900.jpg";
                var bytes = await httpClient.GetByteArrayAsync(url);
                if (bytes != null)
                {
                    ImageSource image = ImageSource.FromStream(() => new MemoryStream(bytes));
                    return image;
                }
                return ImageSource.FromFile("nimo.png");
            }
            catch
            {
                return ImageSource.FromFile("nimo.png");
            }
        }
    }
}
