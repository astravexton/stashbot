using System;
using System.Collections.Generic;
using StashBot.I18n.Strings;
using StashBot.Models;

namespace StashBot.I18n
{
    public class Localization
    {
        public static string GetPhrase(Phrase phrase, string language, string[] replacements = null)
        {
            Dictionary<Phrase, string> phrases = null;
            Dictionary<Phrase, string> defaultPhrases = EnStrings.StringDictionary;
            string phraseOutput = "(Phrase not found)";
            bool foundPhrase = false;

            switch(language) {
                case "de":
                    phrases = DeStrings.StringDictionary;
                    break;
                case "fr":
                    phrases = FrStrings.StringDictionary;
                    break;
                case "en":
                default:
                    phrases = defaultPhrases;
                    break;
            }

            if (phrases.ContainsKey(phrase))
            {
                phrases.TryGetValue(phrase, out phraseOutput);
                foundPhrase = true;
            } else {
                if(defaultPhrases.ContainsKey(phrase))
                {
                    defaultPhrases.TryGetValue(phrase, out phraseOutput);
                    foundPhrase = true;
                }
            }

            if(replacements != null)
            {
                int replacementIndex = 0;

                foreach(var replacement in replacements)
                {
                    phraseOutput = phraseOutput.Replace($"[{replacementIndex}]", replacements[replacementIndex]);
                    replacementIndex++;
                }
            }

            phraseOutput = phraseOutput
                .Replace("<br />", $"{Environment.NewLine}");

            if(foundPhrase) {
                return phraseOutput;
            } else {
                throw new Exception("Phrase not found.");
            }
        }

        public static string GetPhrase(Phrase phrase, TelegramUser user, string[] replacements = null)
        {
            return GetPhrase(phrase, user.Language, replacements);
        }

        public enum Phrase
        {   // Next: 55
            AlreadyBeenQueued = 11,
            Author = 40,
            CannotDeleteXFromChannel = 22,
            CannotFetchFile = 54,
            CannotFindAuthor = 17,
            CannotFindMessageX = 24,
            CannotRemovePermissionFromSelf = 32,
            CreatedNewAuthor = 15,
            Delete = 8,
            DeleteOthers = 34,
            DeletedFromQueue = 3,
            DeletedXFromChannel = 21,
            Earlier = 18,
            Earliest = 19,
            FlushQueue = 35,
            FlushDanglingUsers = 44,
            FlushRemovedPosts = 43,
            FlushedRemovedPosts = 47,
            FlushedXDanglingUsers = 50,
            InvalidArgsSeeHelp = 53,
            Later = 4,
            Latest = 7,
            LoadingQueue = 20,
            LinkContainsNoMedia = 12,
            ManageUsers = 36,
            MessageID = 41,
            Name = 27,
            NoDanglingUsers = 49,
            NotSet = 25,
            NothingIsPosted = 23,
            NothingIsQueued = 1,
            NoPermissionFlushDanglingUsers = 51,
            NoPermissionFlushRemovedPosts = 48,
            NoPermissionManageUsers = 31,
            NoPermissionPostQueue = 14,
            NoPermissionRemovePost = 9,
            NoPermissionTools = 46,
            NoPermissionViewQueue = 2,
            PostSuccessfullyQueued = 10,
            Posted = 38,
            Posts = 30,
            ProfileUpdated = 29,
            Queue = 33,
            Queued = 37,
            RefreshProfile = 42,
            RefreshedProfileHelloX = 52,
            ServiceNotSupported = 13,
            Sooner = 5,
            Soonest = 6,
            Tools = 45,
            User = 26,
            Username = 28,
            WelcomeFirstAuthor = 16
        }
    }
}