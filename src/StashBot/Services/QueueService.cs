using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StashBot.Data;
using StashBot.I18n;
using StashBot.Models;
using StashBot.Models.ReturnModels.QueueServiceReturns;
using StashBot.Models.ReturnModels.ServiceReturnModels;
using StashBot.Services.ScrapeServices;
using StashBot.Utilities;

namespace StashBot.Services
{
    public class QueueService
    {
        public static void PollQueue()
        {
            Task.Run(() =>
            {
                try
                {
                    double sleepTimeFromConfig = AppSettings.Config_PostInterval;
                    double sleepTime = 0;
                    int queueCount = QueueData.CountQueuedQueueItems();
                    QueueItem soonestQueuedItem = null;
                    QueueItem latestPostedItem = QueueData.GetLatestPostedQueueItem();
                    bool continueOnFromPreviousStart = true;

                    while (true)
                    {
                        if (queueCount == 0)
                        {
                            continueOnFromPreviousStart = false;
                            Thread.Sleep(10000); // 10 seconds
                            queueCount = QueueData.CountQueuedQueueItems();
                        }
                        else
                        {
                            if (continueOnFromPreviousStart)
                            {
                                if (latestPostedItem != null)
                                {
                                    var adjustedSleepTime = sleepTimeFromConfig - (DateTime.UtcNow - latestPostedItem.PostedAt).TotalMilliseconds;

                                    if (adjustedSleepTime >= 0)
                                    {
                                        sleepTime = adjustedSleepTime;
                                    }
                                    else
                                    {
                                        sleepTime = 0;
                                    }
                                }
                                else
                                {
                                    sleepTime = sleepTimeFromConfig;
                                }

                                continueOnFromPreviousStart = false;
                            }

                            Thread.Sleep((int)sleepTime);
                            soonestQueuedItem = QueueData.GetSoonestQueuedQueueItem();
                            bool wasPosted = PostQueueItem(soonestQueuedItem);

                            if (wasPosted)
                            {
                                sleepTime = sleepTimeFromConfig;
                            }
                            else
                            {
                                sleepTime = 0;
                            }

                            queueCount = QueueData.CountQueuedQueueItems();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageUtilities.PrintErrorMessage(e, Guid.Empty);
                }
            });
        }

        public static bool PostQueueItem(QueueItem post)
        {
            if (post != null)
            {
                DateTime postedAt = DateTime.UtcNow;
                var caption = GetQueueCaption(post);
                Message message = null;
                bool failed = false;
                string failureReason = "";

                try
                {
                    switch (post.Type)
                    {
                        case QueueItem.MediaType.Image:
                            message = Program.BotClient.SendPhotoAsync(
                                caption: caption.CaptionText,
                                chatId: AppSettings.Config_ChannelId,
                                parseMode: ParseMode.Html,
                                photo: post.MediaUrl
                            ).Result;
                            break;
                        case QueueItem.MediaType.Video:
                            message = Program.BotClient.SendVideoAsync(
                                caption: caption.CaptionText,
                                chatId: AppSettings.Config_ChannelId,
                                parseMode: ParseMode.Html,
                                video: post.MediaUrl
                            ).Result;
                            break;
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.Contains("Bad Request:"))
                    {
                        failed = true;
                        failureReason = e.Message;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (!failed)
                {
                    QueueData.SetQueueItemAsPosted(post.Id, postedAt, message.MessageId);
                    MessageUtilities.PrintSuccessMessage($"Posted #{post.Id} at {postedAt.ToString("yyyy-MM-dd hh:mm:ss zzz")}");
                    return true;
                }
                else
                {
                    QueueData.SetQueueItemAsPostFailed(post.Id, postedAt, failureReason);
                    MessageUtilities.PrintWarningMessage($"Unable to post #{post.Id}: {failureReason}");
                    return false;
                }
            }

            return false;
        }

        public static QueueServiceReturn QueueLink(
            string url,
            TelegramUser user,
            int mediaIndex = 0,
            string customName = ""
        )
        {
            QueueServiceReturn returnModel = new QueueServiceReturn { };
            QueueItem itemToQueue = null;

            if (
                url.StartsWith("https://mobile.twitter.com") ||
                url.StartsWith("https://twitter.com")
            )
            {
                TwitterScrapeService _twitterScrapeService = new TwitterScrapeService();
                itemToQueue = _twitterScrapeService.ScrapeTwitterUrl(url, mediaIndex, customName);
            }
            else if (
                url.StartsWith("https://instagram.com") ||
                url.StartsWith("https://www.instagram.com")
            )
            {
                InstagramScrapeService _instagramScrapeService = new InstagramScrapeService();
                itemToQueue = _instagramScrapeService.ScrapeInstagramUrl(url, mediaIndex);
            }
            else if (
                url.StartsWith("https://flickr.com") ||
                url.StartsWith("https://www.flickr.com/")
            )
            {
                FlickrScrapeService _flickrScrapeService = new FlickrScrapeService();
                itemToQueue = _flickrScrapeService.ScrapeFlickrUrl(url, mediaIndex);
            }
            else
            {
                returnModel.Status = QueueServiceReturn.QueueServiceReturnStatus.ServiceNotSupported;
                return returnModel;
            }

            if (itemToQueue != null)
            {
                var duplicate = QueueData.GetQueueItemBySourceUrl(itemToQueue.SourceUrl);

                if (duplicate != null)
                {
                    returnModel.Status = QueueServiceReturn.QueueServiceReturnStatus.Duplicate;
                }
                else
                {
                    QueueData.AddQueueItem(itemToQueue, user);
                    returnModel.Status = QueueServiceReturn.QueueServiceReturnStatus.Queued;
                }
            }
            else
            {
                returnModel.Status = QueueServiceReturn.QueueServiceReturnStatus.SourceUrlNotFound;
            }

            return returnModel;
        }

        public static void RemoveQueueItem(
            int id
        )
        {
            QueueData.DeleteQueueItem(id, false);
        }

        // TODO: Move this to utilities
        public static GetQueueCaptionReturn GetQueueCaption(
            QueueItem queueItem,
            bool advanced = false,
            TelegramUser user = null
        )
        {
            GetQueueCaptionReturn returnModel = new GetQueueCaptionReturn { };

            string creditText = "";
            string mediaTypeText = "";
            string sourceText = "";

            switch (queueItem.Type)
            {
                case QueueItem.MediaType.Image:
                    mediaTypeText = "📷 | ";
                    break;
                case QueueItem.MediaType.Video:
                    mediaTypeText = "📹 | ";
                    break;
            }

            if (!String.IsNullOrEmpty(queueItem.Name))
            {
                if (!String.IsNullOrEmpty(queueItem.UsernameUrl))
                {
                    creditText += $"👤 <a href=\"{queueItem.UsernameUrl}\">{queueItem.Name}</a>";
                }
                else
                {
                    creditText += $"👤 <i>{queueItem.Name}</i>";
                }
            }
            else
            {
                creditText += $"👤 <i>(unknown)</i>";
            }

            if (!String.IsNullOrEmpty(queueItem.SourceUrl))
            {
                string sourceName = String.IsNullOrEmpty(queueItem.SourceName) ? "source" : queueItem.SourceName;

                creditText += " | ";
                sourceText += $"<a href=\"{queueItem.SourceUrl}\">🔗 {sourceName}</a>";
            }

            returnModel.CaptionText = $"{mediaTypeText}{creditText}{sourceText}";

            if (advanced)
            {
                var statusDateString = "";
                var messageIdString = "";

                switch (queueItem.Status)
                {
                    case QueueItem.QueueStatus.Queued:
                        statusDateString = GenerateStatusDateStringForAdvancedCaption(queueItem.QueuedAt, Localization.GetPhrase(Localization.Phrase.Queued, user));
                        break;
                    case QueueItem.QueueStatus.Deleted:
                        statusDateString = GenerateStatusDateStringForAdvancedCaption(queueItem.DeletedAt, "Deleted");
                        break;
                    case QueueItem.QueueStatus.Posted:
                        statusDateString = GenerateStatusDateStringForAdvancedCaption(queueItem.PostedAt, Localization.GetPhrase(Localization.Phrase.Posted, user));
                        break;
                }

                if (queueItem.Status == QueueItem.QueueStatus.Posted)
                {
                    messageIdString = $"{Environment.NewLine}#️⃣ <b>{Localization.GetPhrase(Localization.Phrase.MessageID, user)}:</b> ";

                    if (queueItem.MessageId != 0)
                    {
                        messageIdString += $"<code>{queueItem.MessageId}</code>";
                    }
                    else
                    {
                        messageIdString += $"<i>({Localization.GetPhrase(Localization.Phrase.NotSet, user)})</i>";
                    }
                }

                string authorNameLink = $"<a href=\"tg://user?id={queueItem.Author.TelegramId}\">{queueItem.Author.TelegramName}</a>";

                string advancedText = $@"—
📩 <b>{Localization.GetPhrase(Localization.Phrase.Author, user)}:</b> {authorNameLink}{statusDateString}{messageIdString}";

                returnModel.CaptionText += $"{Environment.NewLine}{advancedText}";
            }

            return returnModel;
        }

        private static string GenerateStatusDateStringForAdvancedCaption(DateTime date, string statusText)
        {
            return $"{Environment.NewLine}{GeneratorUtilities.GenerateClockEmoji(date)} <b>{statusText}:</b> <code>{date.ToString("dd-MMM-yy hh:mm:ss zz")}</code>";
        }
    }
}
