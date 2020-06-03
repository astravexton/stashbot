using System;
using System.Collections.Generic;
using StashBot.Models;

namespace StashBot.Services.ScrapeServices
{
    public class InstagrmScrapeService
    {
        private string service = "Instagram";

        public QueueItem ScrapeInstagramUrl(string url)
        {
            QueueItem returnItem = null;

            if (url.StartsWith("https://instagram.com") ||
                url.StartsWith("https://www.instagram.com"))
            {
                var galleryDlOutput = GalleryDlService.GetJsonFromUrl(url);

                bool hasMedia = false;

                string name = String.Empty;
                string source = String.Empty;
                string username = String.Empty;

                List<string> media = new List<string>();
                QueueItem.MediaType mediaType = QueueItem.MediaType.Image;

                foreach (var item in galleryDlOutput.Children())
                {
                    if (item[0].ToString() == "ValueError")
                    {
                        throw new Exception($"Failed to download item from {service}");
                    }

                    if (item[0].ToString() == "HttpError")
                    {
                        throw new Exception($"Failed to connect to {service}");
                    }

                    switch (Convert.ToInt32(item[0]))
                    {
                        case 2:
                            name = item[0].Next["username"].ToString();
                            source = item[0].Next["shortcode"].ToString();
                            username = item[0].Next["username"].ToString();
                            break;
                        case 3:
                            media.Add(item[1].ToString());
                            mediaType = QueueItem.MediaType.Image;
                            hasMedia = true;
                            break;
                    }
                }

                if (hasMedia)
                {
                    source = $"https://www.instagram.com/p/{source}/";
                    username = $"https://www.instagram.com/{username}/";

                    var selectedMedia = media[0];

                    returnItem = new QueueItem
                    {
                        MediaUrl = selectedMedia,
                        Name = name,
                        SourceName = service,
                        SourceUrl = source,
                        Type = mediaType,
                        UsernameUrl = username
                    };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new Exception($"URL is not compatible with {service} scraper");
            }

            return returnItem;
        }
    }
}