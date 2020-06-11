using System;
using StashBot.Data;
using StashBot.Exceptions;
using StashBot.Models.ArgumentModels;
using StashBot.Services;

namespace StashBot.Handlers.CommandHandlers
{
    public class HelpCommandHandler
    {

        public static void Invoke(CommandHandlerArguments arguments)
        {
            if (arguments.CommandArguments == null)
            {
                arguments.CommandArguments = new string[] { "help" };
                Invoke(arguments);
            }
            else
            {
                string command = arguments.CommandArguments[0].Replace("/", "");
                var help = HelpData.GetHelp(command);

                if (help.Available)
                {
                    if (help.Help.DescriptionIsItalicized)
                    {
                        help.Help.Description = $"<i>{help.Help.Description}</i>";
                    }

                    string helpText = $@"❓ <b>Help:</b> <code>/{help.Help.Command}</code>
—
{help.Help.Description}";

                    if (help.Help.Arguments != null)
                    {
                        helpText += $"{Environment.NewLine}—";

                        foreach (var argument in help.Help.Arguments)
                        {
                            helpText += $"{Environment.NewLine}<code>&lt;{argument.Position}&gt;</code> <b>{argument.Name}</b> ";
                            helpText += argument.Optional ? "<i>(optional)</i> " : "";
                            helpText += $"- {argument.Explanation}";

                            if (!String.IsNullOrEmpty(argument.Example))
                            {
                                helpText += $" <i>(e.g. {argument.Example})</i>";
                            }
                        }
                    }

                    TelegramApiService.SendTextMessage(
                        new SendTextMessageArguments
                        {
                            Text = helpText
                        },
                        Program.BotClient,
                        arguments.TelegramMessageEvent
                    );
                }
                else
                {
                    throw new CommandHandlerException($"No help available for <code>/{command}</code>");
                }
            }
        }
    }
}