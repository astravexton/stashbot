using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using StashBot.Models;
using StashBot.Utilities;

namespace StashBot.Services.ScrapeServices
{
    public class TwitterScrapeService
    {
        private string service = "Twitter";
        private string nitterUrl = "https://nitter.net";

        // TODO: Handle the possibility Twitter allows mixed video/image albums
        public QueueItem ScrapeTwitterUrl(string url, int mediaIndex, string customName)
        {
            QueueItem returnItem = null;

            url = url.Replace("https://mobile.twitter.com", "https://twitter.com");

            if (url.StartsWith("https://twitter.com"))
            {
                var galleryDlOutput = GalleryDlService.GetJsonFromUrl(url);

                bool useNitter = false;
                Scrape extracted = new Scrape { };

                foreach (var item in galleryDlOutput.Children())
                {
                    if (item[0].ToString() == "ValueError")
                    {
                        useNitter = true;
                    }

                    if (item[0].ToString() == "HttpError")
                    {
                        throw new Exception($"Failed to connect to {service}");
                    }

                    if (useNitter)
                    {
                        extracted = ExtractViaNitter(url, mediaIndex, customName);
                        break;
                    }
                    else
                    {

                        switch (Convert.ToInt32(item[0]))
                        {
                            case 2:
                                extracted.Name = item[0].Next["author"]["nick"].ToString();
                                extracted.SourceUrl = item[0].Next["tweet_id"].ToString(0);
                                extracted.Username = item[0].Next["author"]["name"].ToString();
                                break;
                            case 3:
                                extracted.Media.Add(YoutubeDlService.GetExtractedPathFromUrl(url));
                                extracted.Type = QueueItem.MediaType.Video;
                                extracted.HasMedia = true;
                                break;
                            case 7:
                                extracted.Media.Add(item[1][0].ToString().Replace(":orig", ""));
                                extracted.Type = QueueItem.MediaType.Image;
                                extracted.HasMedia = true;
                                break;
                        }
                    }
                }

                if (extracted.HasMedia)
                {
                    extracted.Name = String.IsNullOrEmpty(customName) ? extracted.Name : customName;
                    extracted.SourceUrl = (extracted.SourceUrl != null) ? extracted.SourceUrl : $"https://twitter.com/{extracted.Username}/status/{extracted.SourceId}";
                    extracted.UsernameUrl = (extracted.UsernameUrl != null) ? extracted.UsernameUrl : $"https://twitter.com/{extracted.Username}";

                    if (url.Contains("/photo/")) // NOTE: This will cause issues for the Twitter account @photo
                    {
                        mediaIndex = Convert.ToInt32(url.Substring(url.IndexOf("/photo/") + "/photo/".Length)) - 1;
                    }

                    returnItem = new QueueItem
                    {
                        MediaUrl = (mediaIndex + 1 > extracted.Media.Count || mediaIndex < 0) ?
                            extracted.Media[0] :
                            extracted.Media[mediaIndex],
                        Name = extracted.Name,
                        SourceName = service,
                        SourceUrl = extracted.SourceUrl,
                        Type = extracted.Type,
                        UsernameUrl = extracted.UsernameUrl
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

        private Scrape ExtractViaNitter(string url, int mediaIndex, string customName)
        {
            Scrape returnModel = new Scrape { };

            url = url.Replace("https://mobile.twitter.com", "https://twitter.com");

            if (url.StartsWith("https://twitter.com"))
            {
                url = url.Replace("https://twitter.com", nitterUrl);
            }

            if (url.StartsWith(nitterUrl) || url.StartsWith("https://nitter.net"))
            {
                var web = new HtmlWeb();
                var document = web.Load(url);
                var documentNode = document.DocumentNode;

                returnModel.Name = documentNode.SelectNodes("//div[contains(@class, 'main-tweet')]//a[contains(@class, 'fullname')]")[0].InnerText;
                returnModel.Username = documentNode.SelectNodes("//div[contains(@class, 'main-tweet')]//a[contains(@class, 'username')]")[0].InnerText.Replace("@", "");
                returnModel.SourceUrl = documentNode.SelectNodes("//div[contains(@class, 'nav-item')]//a[contains(@class, 'icon-bird')]")[0].Attributes["href"].Value;

                if(returnModel.SourceUrl.Contains('?'))
                {
                    returnModel.SourceUrl = returnModel.SourceUrl.Substring(0, returnModel.SourceUrl.LastIndexOf('?'));
                }

                HtmlNodeCollection extractedImages = documentNode.SelectNodes("//div[contains(@class, 'main-tweet')]//a[contains(@class, 'still-image')]");
                HtmlNodeCollection extractedVideos = documentNode.SelectNodes("//div[contains(@class, 'main-tweet')]//div[contains(@class, 'gallery-video')]");

                if(extractedImages == null && extractedVideos == null)
                {
                    // HACK: GIF?
                    extractedVideos = documentNode.SelectNodes("//div[contains(@class, 'main-tweet')]//video//source");
                }

                if (extractedImages != null)
                {
                    foreach (var extractedImage in extractedImages)
                    {
                        returnModel.Media.Add(nitterUrl + extractedImage.Attributes["href"].Value);
                        returnModel.Type = QueueItem.MediaType.Image;
                        returnModel.HasMedia = true;
                    }
                }

                if (extractedVideos != null)
                {
                    returnModel.Media.Add(YoutubeDlService.GetExtractedPathFromUrl(returnModel.SourceUrl));
                    returnModel.Type = QueueItem.MediaType.Video;
                    returnModel.HasMedia = true;
                }

                if (!returnModel.HasMedia)
                {
                    return null;
                }
            }
            else
            {
                throw new Exception($"URL is not compatible with {service} scraper");
            }

            return returnModel;
        }
    }
}