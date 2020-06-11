using System;
using Microsoft.DotNet.PlatformAbstractions;
using StashBot.Data;
using StashBot.Models;
using StashBot.Models.ArgumentModels;
using StashBot.Services;
using StashBot.Utilities;

namespace StashBot.Handlers.CommandHandlers
{
    public class InfoCommandHandler
    {
        public static Help Help = new Help
        {
            Command = "info",
            Description = "Bot information and status"
        };

        public static void Invoke(CommandHandlerArguments arguments)
        {
            var thisProcess = System.Diagnostics.Process.GetCurrentProcess();

            var version = ReflectionUtilities.GetVersion();
            var runtime = ReflectionUtilities.GetEnvironment();

            string processMemoryUsage = Convert.ToDecimal(thisProcess.WorkingSet64 / 1000000).ToString();
            DateTime processStartTime = thisProcess.StartTime;
            string systemHostname = System.Net.Dns.GetHostName();
            string systemOpSys = RuntimeEnvironment.OperatingSystem;
            string systemOpSysVersion = RuntimeEnvironment.OperatingSystemVersion;
            string systemTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss zzz");
            string userId = arguments.TelegramUser.Id.ToString();
            string userLanguageCode = arguments.TelegramUser.Language.ToString();

            TimeSpan timeSinceStart = DateTime.Now.ToUniversalTime().Subtract(processStartTime.ToUniversalTime());
            string uptime = timeSinceStart.ToString("d'd 'h'h 'm'm 's's'");

            int queueAmountInt = QueueData.CountQueuedQueueItems();
            string queueAmount = queueAmountInt.ToString();
            string queueApproxDays = QueueUtlities.CalculateQueueApproxDays(queueAmountInt).ToString("0.00");
            string totalQueueAmount = QueueData.CountQueueItems().ToString();
            string usersAmount = AuthorData.CountAuthors().ToString();

            string name = AppSettings.Config_Name;
            string descriptionPartial = "";
            string owner = AppSettings.Config_Owner;

            if (name == "StashBot")
            {
                descriptionPartial = "A lovely Telegram bot for queuing posts for a channel.";
            }
            else
            {
                descriptionPartial = "This bot is powered by <b>StashBot</b>; a lovely self-hosted Telegram bot for queuing posts for a channel.";
            }

            string outputText = $@"<b>{name}</b> | {version}
—
<i>{descriptionPartial} Code can be found <a href=""https://github.com/electricduck/stashbot"">on Github</a>, licensed under <a href=""https://ducky.mit-license.org/"">MIT</a>. This particular instance is ran by {owner}.</i>

<i>There is <b>{queueAmount} queued posts</b>, amounting to approximately <b>{queueApproxDays} days</b>; with <b>{totalQueueAmount} total posts</b>, and <b>{usersAmount} users</b>. <b>{Constants.Cats} cats</b> have been generated.</i> 
—
<b>⚙️ Bot</b>
Memory: <code>{processMemoryUsage}mb</code>
️Uptime: <code>{uptime}</code>
Env.: <code>{runtime}</code>
—
<b>🖥️ System</b>
Host: <code>{systemHostname}</code>
OS: <code>{systemOpSys} {systemOpSysVersion}</code>
Time: <code>{systemTime}</code>
—
<b>👤 You</b>
ID: <code>{userId}</code>
Language: <code>{userLanguageCode}</code>";

            TelegramApiService.SendTextMessage(
                new SendTextMessageArguments
                {
                    Text = outputText
                },
                Program.BotClient,
                arguments.TelegramMessageEvent
            );
        }
    }
}