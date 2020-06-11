using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using StashBot.Data;
using StashBot.Exceptions;
using StashBot.I18n;
using StashBot.Models;
using StashBot.Models.ArgumentModels;
using StashBot.Services;
using StashBot.Utilities;

namespace StashBot.Handlers.CommandHandlers
{
    public class UserCommandHandler
    {
        public static Help Help = new Help
        {
            Arguments = new List<HelpArgument> {
              new HelpArgument {
                  Example = "63391517, @theducky",
                  Explanation = "User ID of Telegram user, or username of user if it exists on the database",
                  Name = "User ID",
                  Position = 1
              },
            },
            Command = "user",
            Description = "Manage user permissions"
        };

        public static void Invoke(CommandHandlerArguments arguments, bool reload = false)
        {
            bool hasPermission = AuthorData.CanAuthorManageAuthors(arguments.TelegramUser.Id);

            if (arguments.CommandArguments == null)
            {
                if (hasPermission)
                {
                    arguments.CommandArguments = new string[] { arguments.TelegramUser.Id.ToString() };
                }
                else
                {
                    throw new ArgumentException();
                }
            }

            if (hasPermission)
            {
                long authorId = 0;
                string authorIdOrUsernameArgument = arguments.CommandArguments[0];

                if (!(long.TryParse(authorIdOrUsernameArgument, out authorId)))
                {
                    if (arguments.CommandArguments[0].ToString().StartsWith("@"))
                    {
                        Author authorByUsername = AuthorData.GetAuthorByTelegramUsername(arguments.CommandArguments[0], false);

                        if (authorByUsername != null)
                        {
                            authorId = authorByUsername.TelegramId;
                        }
                        else
                        {
                            throw new CommandHandlerException(
                                Localization.GetPhrase(
                                    Localization.Phrase.CannotFindAuthor,
                                    arguments.TelegramUser,
                                    new string[] {
                                        arguments.CommandArguments[0]
                                    }
                                )
                            );
                        }
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }

                Author author = AuthorData.GetAuthor(authorId, true);

                if (author.TelegramId == arguments.TelegramUser.Id)
                {
                    if (!(
                        author.TelegramName == arguments.TelegramUser.Name &&
                        author.TelegramUsername == arguments.TelegramUser.Username
                    ))
                    {
                        author = AuthorData.UpdateAuthorTelegramProfile(arguments.TelegramUser);
                    }
                }

                string notSetText = Localization.GetPhrase(Localization.Phrase.NotSet, arguments.TelegramUser);

                string authorNameText = (String.IsNullOrEmpty(author.TelegramName)) ? $"<i>({notSetText})</i>" : author.TelegramName;
                string authorUsernameText = (String.IsNullOrEmpty(author.TelegramUsername)) ? $"<i>({notSetText})</i>" : author.TelegramUsername;
                string authorLastUpdatedText = author.TelegramDetailsLastUpdatedAt.ToString("dd-MMM-yy hh:mm:ss zz");
                string authorLink = $"<a href=\"tg://user?id={authorId}\">{authorId}</a>";
                int authorPostCount = AuthorData.CountAuthorQueue(author.TelegramId);
                int queueCount = QueueData.CountQueueItems();
                decimal queuePercentage = 0;

                if (queueCount > 0)
                {
                    queuePercentage = ((decimal)authorPostCount / queueCount) * 100;
                }

                var userPermissionKeyboard = GetPermissionKeyboard(author, arguments.TelegramUser);
                var userDetailsText = $@"👤 <b>{Localization.GetPhrase(Localization.Phrase.User, arguments.TelegramUser)}:</b> {authorLink}
—
<b>{Localization.GetPhrase(Localization.Phrase.Name, arguments.TelegramUser)}:</b> {authorNameText}
<b>{Localization.GetPhrase(Localization.Phrase.Username, arguments.TelegramUser)}:</b> {authorUsernameText}
<b>{Localization.GetPhrase(Localization.Phrase.ProfileUpdated, arguments.TelegramUser)}:</b> <code>{authorLastUpdatedText}</code>
<b>{Localization.GetPhrase(Localization.Phrase.Posts, arguments.TelegramUser)}:</b> <code>{authorPostCount}</code> (<code>{queuePercentage.ToString("0.00")}%</code>)";

                TelegramApiService.SendTextMessage(
                    new SendTextMessageArguments
                    {
                        ReplyMarkup = userPermissionKeyboard,
                        Text = userDetailsText
                    },
                    Program.BotClient,
                    arguments.TelegramMessageEvent
                );
            }
            else
            {
                throw new CommandHandlerException(Localization.GetPhrase(Localization.Phrase.NoPermissionManageUsers, arguments.TelegramUser));
            }
        }

        public async static Task InvokeSetPermission(CommandHandlerArguments arguments)
        {
            if (AuthorData.CanAuthorManageAuthors(arguments.TelegramUser.Id))
            {
                Author author = AuthorData.GetAuthor(Convert.ToInt64(arguments.CommandArguments[0]), false);
                string permission = arguments.CommandArguments[1].ToString();
                bool setting = Convert.ToBoolean(arguments.CommandArguments[2].ToString());
                bool settingUpdated = true;

                switch (permission)
                {
                    case "CanDeleteOthers":
                        AuthorData.SetAuthorDeleteOthersPermission(author.TelegramId, setting);
                        author.CanDeleteOthers = setting;
                        break;
                    case "CanFlushQueue":
                        AuthorData.SetAuthorFlushQueuePermission(author.TelegramId, setting);
                        author.CanFlushQueue = setting;
                        break;
                    case "CanManageAuthors":
                        if (author.CanManageAuthors == true && author.TelegramId == arguments.TelegramUser.Id)
                        {
                            settingUpdated = false;
                        }
                        else
                        {
                            AuthorData.SetAuthorManageAuthorsPermission(author.TelegramId, setting);
                            author.CanManageAuthors = setting;
                        }
                        break;
                    case "CanQueue":
                        AuthorData.SetAuthorQueuePermission(author.TelegramId, setting);
                        author.CanQueue = setting;
                        break;
                }

                if (settingUpdated)
                {
                    await Program.BotClient.EditMessageReplyMarkupAsync(
                        arguments.TelegramCallbackQueryEvent.CallbackQuery.Message.Chat.Id,
                        arguments.TelegramCallbackQueryEvent.CallbackQuery.Message.MessageId,
                        GetPermissionKeyboard(author, arguments.TelegramUser)
                    );
                }
                else
                {
                    await Program.BotClient.AnswerCallbackQueryAsync(
                        callbackQueryId: arguments.TelegramCallbackQueryEvent.CallbackQuery.Id,
                        text: MessageUtilities.CreateWarningMessage(Localization.GetPhrase(Localization.Phrase.CannotRemovePermissionFromSelf, arguments.TelegramUser))
                    );
                }
            }
            else
            {
                await Program.BotClient.DeleteMessageAsync(
                    chatId: arguments.TelegramCallbackQueryEvent.CallbackQuery.Message.Chat.Id,
                    messageId: arguments.TelegramCallbackQueryEvent.CallbackQuery.Message.MessageId
                );

                throw new CommandHandlerException(Localization.GetPhrase(Localization.Phrase.NoPermissionManageUsers, arguments.TelegramUser));
            }
        }

        public static void InvokeSetup(CommandHandlerArguments arguments)
        {
            int authorCount = AuthorData.CountAuthors();

            if (authorCount == 0)
            {
                Author author = AuthorData.CreateAuthor(arguments.TelegramUser);
                AuthorData.SetAuthorManageAuthorsPermission(author.TelegramId, true);
                MessageUtilities.SendSuccessMessage(
                    Localization.GetPhrase(
                        Localization.Phrase.WelcomeFirstAuthor,
                        arguments.TelegramUser,
                        new string[] {
                            author.TelegramName,
                            author.TelegramId.ToString()
                        }
                    ),
                    arguments.TelegramMessageEvent);
            }
            else
            {
                if (!AuthorData.DoesAuthorExist(arguments.TelegramUser))
                {
                    Author newAuthor = AuthorData.CreateAuthor(arguments.TelegramUser);

                    List<Author> authorsThatCanManageUsers = AuthorData
                        .GetAuthors()
                        .Where(a => a.CanManageAuthors == true)
                        .ToList();

                    foreach (Author authorThatCanManageUser in authorsThatCanManageUsers)
                    {
                        MessageUtilities.SendSuccessMessage(
                            Localization.GetPhrase(
                                Localization.Phrase.CreatedNewAuthor,
                                arguments.TelegramUser,
                                new string[] {
                                    newAuthor.TelegramName,
                                    newAuthor.TelegramId.ToString()
                                }
                            ),
                        authorThatCanManageUser.TelegramId);
                    }
                }
            }
        }

        private static InlineKeyboardMarkup GetPermissionKeyboard(Author author, TelegramUser user)
        {
            const string tick = "✔️";
            const string cross = "✖️";

            string canDeleteOthersStatus = author.CanDeleteOthers ? tick : cross;
            string canFlushQueueStatus = author.CanFlushQueue ? tick : cross;
            string canManageAuthorsStatus = author.CanManageAuthors ? tick : cross;
            string canQueueStatus = author.CanQueue ? tick : cross;

            return new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData($"{canQueueStatus} {Localization.GetPhrase(Localization.Phrase.Queue, user)}", $"user_perm:{author.TelegramId}:CanQueue:{!author.CanQueue}")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData($"{canDeleteOthersStatus} {Localization.GetPhrase(Localization.Phrase.DeleteOthers, user)}", $"user_perm:{author.TelegramId}:CanDeleteOthers:{!author.CanDeleteOthers}")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData($"{canFlushQueueStatus} {Localization.GetPhrase(Localization.Phrase.FlushQueue, user)}", $"user_perm:{author.TelegramId}:CanFlushQueue:{!author.CanFlushQueue}")
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData($"{canManageAuthorsStatus} {Localization.GetPhrase(Localization.Phrase.ManageUsers, user)}", $"user_perm:{author.TelegramId}:CanManageAuthors:{!author.CanManageAuthors}")
                }
            });
        }
    }
}