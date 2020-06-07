using System.Collections.Generic;

namespace StashBot.I18n.Strings
{
    public class EnStrings
    {
        public static Dictionary<Localization.Phrase, string> StringDictionary = new Dictionary<Localization.Phrase, string>() {
            { Localization.Phrase.AlreadyBeenQueued, "This has already been queued" },
            { Localization.Phrase.Author, "Poster" },
            { Localization.Phrase.CannotDeleteXFromChannel, "Cannot delete #[0] from channel"},
            { Localization.Phrase.CannotFetchFile, "Telegram cannot fetch this file (<code>[1]</code>):<br />[0]"},
            { Localization.Phrase.CannotFindAuthor, "Cannot find <code>[0]</code>"},
            { Localization.Phrase.CannotFindMessageX, "Cannot find message #<code>[0]</code>"},
            { Localization.Phrase.CannotRemovePermissionFromSelf, "You cannot remove this permission from yourself"},
            { Localization.Phrase.CreatedNewAuthor, "New user has started the bot, <b>[0]</b> (<code>[1]</code>). Set permissions with <code>/user [1]</code>." },
            { Localization.Phrase.Delete, "Delete" },
            { Localization.Phrase.DeleteOthers, "Delete Others" },
            { Localization.Phrase.DeletedFromQueue, "Deleted from queue"},
            { Localization.Phrase.DeletedXFromChannel, "Deleted #[0] from channel"},
            { Localization.Phrase.Earlier, "Earlier" },
            { Localization.Phrase.Earliest, "Earliest" },
            { Localization.Phrase.FlushDanglingUsers, "Purge Dangling Users" },
            { Localization.Phrase.FlushQueue, "Flush Queue" },
            { Localization.Phrase.FlushRemovedPosts, "Flush Removed Posts" },
            { Localization.Phrase.FlushedRemovedPosts, "Flushed removed posts" },
            { Localization.Phrase.FlushedXDanglingUsers, "Purged [0] dangling users" },
            { Localization.Phrase.InvalidArgsSeeHelp, "Invalid arguments: see <code>/help [0]</code> for details" },
            { Localization.Phrase.Later, "Later" },
            { Localization.Phrase.Latest, "Latest" },
            { Localization.Phrase.LinkContainsNoMedia, "This link contains no media or does not exist" },
            { Localization.Phrase.LoadingQueue, "Loading Queue..." },
            { Localization.Phrase.ManageUsers, "Manage Users" },
            { Localization.Phrase.MessageID, "Message ID" },
            { Localization.Phrase.Name, "Name" },
            { Localization.Phrase.NoDanglingUsers, "No dangling users to purge" },
            { Localization.Phrase.NotSet, "Not set"},
            { Localization.Phrase.NothingIsPosted, "Nothing has been posted" },
            { Localization.Phrase.NothingIsQueued, "Nothing is queued" },
            { Localization.Phrase.NoPermissionFlushDanglingUsers, "You do not have permission to purge dangling users" },
            { Localization.Phrase.NoPermissionFlushRemovedPosts, "You do not have permission to flush removed posts" },
            { Localization.Phrase.NoPermissionManageUsers, "You do not have permission to manage users"},
            { Localization.Phrase.NoPermissionPostQueue, "You do not have permission to queue new posts" },
            { Localization.Phrase.NoPermissionRemovePost, "You do not have permission to remove this post" },
            { Localization.Phrase.NoPermissionTools, "You do not have permission to use tools" },
            { Localization.Phrase.NoPermissionViewQueue, "You do not have permission to view the queue"},
            { Localization.Phrase.PostSuccessfullyQueued, "Post successfully queued" },
            { Localization.Phrase.Posted, "Posted" },
            { Localization.Phrase.Posts, "Posts" },
            { Localization.Phrase.ProfileUpdated, "Profile Updated"},
            { Localization.Phrase.Queue, "Queue" },
            { Localization.Phrase.Queued, "Queued"},
            { Localization.Phrase.RefreshProfile, "Refresh Profile" },
            { Localization.Phrase.RefreshedProfileHelloX, "Refreshed profile. Hello [0]!" },
            { Localization.Phrase.ServiceNotSupported, "This service is not supported"},
            { Localization.Phrase.Sooner, "Sooner" },
            { Localization.Phrase.Soonest, "Soonest" },
            { Localization.Phrase.Tools, "Tools" },
            { Localization.Phrase.User, "User" },
            { Localization.Phrase.Username, "Username" },
            { Localization.Phrase.WelcomeFirstAuthor, $"<b>Welcome to StashBot, [0]!</b><br />Set your permissions with <code>/user [0]</code> (or just <code>/user</code>)." }
        };
    }
}