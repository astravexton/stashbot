using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using StashBot.Exceptions;
using StashBot.Handlers.CommandHandlers;
using StashBot.I18n;
using StashBot.Models.ArgumentModels;
using StashBot.Utilities;

namespace StashBot.Handlers
{
    public class BotEventHandler
    {
        public static void Bot_OnMessage(object sender, MessageEventArgs telegramMessageEvent)
        {
            Task.Run(() =>
            {
                CommandHandlerArguments arguments = new CommandHandlerArguments { };

                try
                {
                    if (telegramMessageEvent.Message.Text != null)
                    {
                        string messageText = telegramMessageEvent.Message.Text.ToString();

                        Regex commandAndArgumentsRegex = new Regex(@"^([\/][a-z]{1,100})([ ])*([\/a-zA-Z0-9_:.,*-@ ]*)$");
                        Match parsedCommand = commandAndArgumentsRegex.Match(messageText);

                        arguments.TelegramUser = TelegramUtilities.GetTelegramUser(telegramMessageEvent);

                        if (messageText.StartsWith("/"))
                        {
                            arguments.Command = parsedCommand.Groups[1].Value.Replace("/", "").ToLower();
                            arguments.CommandArgument = parsedCommand.Groups[3].Value;
                            arguments.CommandArguments = parsedCommand.Groups[3].Value.Split(" ");
                            arguments.TelegramMessageEvent = telegramMessageEvent;

                            if (String.IsNullOrEmpty(arguments.CommandArguments[0].ToString()))
                            {
                                arguments.CommandArguments = null;
                            }

                            switch (arguments.Command)
                            {
                                case "catpls":
                                    CatPlsCommandHandler.Invoke(arguments);
                                    break;
                                case "help":
                                    HelpCommandHandler.Invoke(arguments);
                                    break;
                                case "info":
                                    InfoCommandHandler.Invoke(arguments);
                                    break;
                                case "post":
                                    PostCommandHandler.Invoke(arguments);
                                    break;
                                case "start":
                                    UserCommandHandler.InvokeSetup(arguments);
                                    break;
                                case "tools":
                                case "tool":
                                    ToolsCommandHandler.Invoke(arguments);
                                    break;
                                case "user":
                                case "set":
                                    UserCommandHandler.Invoke(arguments);
                                    break;
                                case "view":
                                    ViewCommandHandler.Invoke(arguments);
                                    break;
                            }
                        }
                    }
                }
                catch (ArgumentException)
                {
                    MessageUtilities.SendWarningMessage(
                        Localization.GetPhrase(
                            Localization.Phrase.InvalidArgsSeeHelp,
                            arguments.TelegramUser,
                            new string[] {
                                arguments.Command
                            }
                        ),
                        telegramMessageEvent);
                }
                catch (CommandHandlerException e)
                {
                    MessageUtilities.SendWarningMessage(e.Message, telegramMessageEvent);
                }
                catch (Exception e)
                {
                    MessageUtilities.SendErrorMessage(e, telegramMessageEvent);
                }
            });
        }

        public async static void Bot_OnCallbackQuery(object sender, CallbackQueryEventArgs telegramCallbackQueryEvent)
        {
            CommandHandlerArguments arguments = new CommandHandlerArguments { };

            try
            {
                Regex commandAndArgumentsRegex = new Regex(@"^([a-z_]{1,100})([:]){0,1}([\/a-zA-Z0-9_:.,-@ ]*)$");
                Match parsedCommand = commandAndArgumentsRegex.Match(telegramCallbackQueryEvent.CallbackQuery.Data);

                arguments.Command = parsedCommand.Groups[1].Value;
                arguments.CommandArguments = parsedCommand.Groups[3].Value.Split(":");
                arguments.TelegramCallbackQueryEvent = telegramCallbackQueryEvent;
                arguments.TelegramUser = TelegramUtilities.GetTelegramUser(telegramCallbackQueryEvent);

                switch (arguments.Command)
                {
                    case "tools_flush":
                        await ToolsCommandHandler.InvokeFlush(arguments);
                        break;
                    case "tools_purgeusers":
                        ToolsCommandHandler.InvokePurgeUsers(arguments);
                        break;
                    case "tools_refreshprofile":
                        await ToolsCommandHandler.InvokeRefreshProfile(arguments);
                        break;
                    case "user_perm":
                        await UserCommandHandler.InvokeSetPermission(arguments);
                        break;
                    case "view_del":
                        await ViewCommandHandler.InvokeDelete(arguments);
                        break;
                    case "view_nav":
                        await ViewCommandHandler.InvokeChange(arguments);
                        break;
                }
            }
            catch (ArgumentException)
            {
                MessageUtilities.SendWarningMessage(
                    Localization.GetPhrase(
                            Localization.Phrase.InvalidArgsSeeHelp,
                            arguments.TelegramUser,
                            new string[] {
                                arguments.Command
                            }
                        ),
                        arguments.TelegramCallbackQueryEvent);
            }
            catch (CommandHandlerException e)
            {
                MessageUtilities.SendWarningMessage(e.Message, arguments.TelegramCallbackQueryEvent);
            }
            catch (Exception e)
            {
                MessageUtilities.SendErrorMessage(e, arguments.TelegramCallbackQueryEvent);
            }
        }

        public static void Bot_OnInlineQueryRecieved(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine("Hello!");
        }
    }
}