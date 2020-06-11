using System;
using Microsoft.DotNet.PlatformAbstractions;
using Telegram.Bot.Args;
using StashBot.Models;
using StashBot.Models.ArgumentModels;
using StashBot.Services;

namespace StashBot.Utilities
{
    public class MessageUtilities
    {
        public static void AlertSuccessMessage(string successText, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            Program.BotClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQueryEventArgs.CallbackQuery.Id,
                text: CreateSuccessMessage(successText)
            );
        }

        public static void AlertWarningMessage(string warningText, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            Program.BotClient.AnswerCallbackQueryAsync(
                callbackQueryId: callbackQueryEventArgs.CallbackQuery.Id,
                text: CreateWarningMessage(warningText)
            );
        }

        public static string CreateSuccessMessage(string successText)
        {
            return $"✅ {successText}";
        }

        public static string CreateWarningMessage(string warningText)
        {
            return $"⚠️ {warningText}";
        }

        public static void PrintErrorMessage(Exception e, Guid errorGuid)
        {
            string errorMessage = errorGuid.ToString();

            if(errorGuid == Guid.Empty)
            {
                errorMessage = "Oops!";
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{RenderEmoji("🚫")}{errorMessage}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{e.Message}{Environment.NewLine}{e.StackTrace}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintInfoMessage(string title, string description = "")
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{RenderEmoji("ℹ️")}{title}");
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!String.IsNullOrEmpty(description))
            {
                Console.WriteLine($"{description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintSuccessMessage(string title, string description = "")
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{RenderEmoji("✅")}{title}");
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!String.IsNullOrEmpty(description))
            {
                Console.WriteLine($"{description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintWarningMessage(string title, string description = "")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{RenderEmoji("⚠️")}{title}");
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!String.IsNullOrEmpty(description))
            {
                Console.WriteLine($"{description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void SendErrorMessage(Exception exception, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            SendErrorMessage(exception, callbackQueryEventArgs.CallbackQuery.Message.Chat.Id);
        }

        public static void SendErrorMessage(Exception exception, MessageEventArgs telegramMessageEvent)
        {
            SendErrorMessage(exception, telegramMessageEvent.Message.Chat.Id);
        }

        public static void SendErrorMessage(Exception exception, long chatId)
        {
            Guid errorGuid = Guid.NewGuid();

            SendTextMessageArguments output = new SendTextMessageArguments
            {
                ChatId = chatId,
                Text = $@"🚫 {exception.Message}
<code>{errorGuid}</code>
—
<b>This is an error. Please forward me to @{AppSettings.Config_Owner}.</b>"
            };

            PrintErrorMessage(exception, errorGuid);
            TelegramApiService.SendTextMessage(output, Program.BotClient, null);
        }
        public static void SendSuccessMessage(string successText, MessageEventArgs telegramMessageEvent)
        {
            SendSuccessMessage(successText, telegramMessageEvent.Message.Chat.Id);
        }

        public static void SendSuccessMessage(string successText, long chatId)
        {
            SendTextMessageArguments output = new SendTextMessageArguments
            {
                ChatId = chatId,
                Text = CreateSuccessMessage(successText)
            };

            TelegramApiService.SendTextMessage(output, Program.BotClient, null);
        }

        public static void SendWarningMessage(string warningText, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            SendWarningMessage(warningText, callbackQueryEventArgs.CallbackQuery.Message.Chat.Id);
        }

        public static void SendWarningMessage(string warningText, MessageEventArgs telegramMessageEvent)
        {
            SendWarningMessage(warningText, telegramMessageEvent.Message.Chat.Id);
        }

        public static void SendWarningMessage(string warningText, long chatId)
        {
            SendTextMessageArguments output = new SendTextMessageArguments
            {
                ChatId = chatId,
                Text = CreateWarningMessage(warningText)
            };

            TelegramApiService.SendTextMessage(output, Program.BotClient, null);
        }

        public static void PrintStartupMessage()
        {
            string figlet = @" ____  _            _     ____        _
/ ___|| |_ __ _ ___| |__ | __ )  ___ | |_
\___ \| __/ _` / __| '_ \|  _ \ / _ \| __|
 ___) | || (_| \__ | | | | |_) | (_) | |_
|____/ \__\__,_|___|_| |_|____/ \___/ \__|
==========================================";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(figlet);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Version ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(ReflectionUtilities.GetVersion());
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(" using ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ReflectionUtilities.GetEnvironment());
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("---");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static string RenderEmoji(string emoji)
        {
            switch(RuntimeEnvironment.OperatingSystemPlatform)
            {
                case Platform.Windows:
                    return $"{emoji} ";
                default:
                    if (emoji.Length == 1)
                    {
                        return $"{emoji} ";
                    }
                    else
                    {
                        return $"{emoji}  ";
                    }
            }
        }
    }
}