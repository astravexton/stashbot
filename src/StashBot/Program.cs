﻿using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using StashBot.Data;
using StashBot.Handlers;
using StashBot.Models;
using StashBot.Services;
using StashBot.Utilities;

namespace StashBot
{
    class Program
    {
        public static ITelegramBotClient BotClient;

        static void Main(string[] args)
        {
            try
            {
                MessageUtilities.PrintStartupMessage();

                if(!File.Exists("stashbot.db"))
                {
                    throw new Exception("Database does not exist. Please run 'dotnet ef database restore' to create");
                }

                MessageUtilities.PrintInfoMessage("Backing up database");
                DbUtilities.BackupDatabase();

                MessageUtilities.PrintInfoMessage("Consuming settings file");
                SetupApp();

                BotClient = new TelegramBotClient(AppSettings.ApiKeys_Telegram);

                MessageUtilities.PrintInfoMessage("Connecting to Telegram");
                if (!BotClient.TestApiAsync().Result)
                {
                    throw new Exception("Telegram API key invalid");
                }

                HelpData.CompileHelp();
            }
            catch(Exception e)
            {
                MessageUtilities.PrintErrorMessage(e, Guid.Empty);
                Environment.Exit(1);
            }

            MessageUtilities.PrintSuccessMessage("Initial startup successful");

            try
            {
                MessageUtilities.PrintInfoMessage("Listening to messages");
                BotClient.OnMessage += BotEventHandler.Bot_OnMessage;
                BotClient.OnCallbackQuery += BotEventHandler.Bot_OnCallbackQuery;
                BotClient.OnInlineQuery += BotEventHandler.Bot_OnInlineQueryRecieved;
                BotClient.StartReceiving();
            }
            catch (Exception e)
            {
                MessageUtilities.PrintErrorMessage(e, Guid.Empty);
                Environment.Exit(1);
            }

            try
            {
                if (AppSettings.Config_Poll)
                {
                    MessageUtilities.PrintInfoMessage("Polling queue");
                    QueueService.PollQueue();
                }
                else
                {
                    MessageUtilities.PrintWarningMessage("Polling has been disabled in the configuration");
                }
            }
            catch (Exception e)
            {
                MessageUtilities.PrintErrorMessage(e, Guid.Empty);
            }

            Thread.Sleep(int.MaxValue);
        }
        public static void SetupApp()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .Build();

            AppSettings.ApiKeys_Telegram = configuration.GetSection("apiKeys")["telegram"];
            AppSettings.Config_ChannelId = Convert.ToInt64(configuration.GetSection("config")["channel"]);
            AppSettings.Config_Owner = configuration.GetSection("config")["owner"];
            AppSettings.Config_Poll = Convert.ToBoolean(configuration.GetSection("config")["poll"]);
            AppSettings.Config_PostInterval = Convert.ToInt32(configuration.GetSection("config")["postInterval"]);
        }
    }
}
