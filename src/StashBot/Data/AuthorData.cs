using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StashBot.Models;

namespace StashBot.Data
{
    public class AuthorData
    {
        public static bool CanAuthorDeleteOthers(long telegramId)
        {
            Author author = GetAuthor(telegramId, false);

            if (author == null)
            {
                return false;
            }
            else
            {
                return author.CanDeleteOthers;
            }
        }

        public static bool CanAuthorFlushQueue(long telegramId)
        {
            Author author = GetAuthor(telegramId, false);

            if (author == null)
            {
                return false;
            }
            else
            {
                return author.CanFlushQueue;
            }
        }

        public static bool CanAuthorManageAuthors(long telegramId)
        {
            Author author = GetAuthor(telegramId, false);

            if (author == null)
            {
                return false;
            }
            else
            {
                return author.CanManageAuthors;
            }
        }

        public static bool CanAuthorQueue(long telegramId)
        {
            Author author = GetAuthor(telegramId, false);

            if (author == null)
            {
                return false;
            }
            else
            {
                return author.CanQueue;
            }
        }

        public static Author CreateAuthor(int telegramId)
        {
            TelegramUser user = new TelegramUser
            {
                Id = telegramId
            };

            return CreateAuthor(user);

        }

        public static Author CreateAuthor(TelegramUser user)
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == user.Id);

                if (author == null)
                {
                    author = new Author
                    {
                        TelegramId = user.Id,
                        TelegramName = user.Name,
                        TelegramUsername = user.Username,
                        TelegramDetailsLastUpdatedAt = DateTime.UtcNow
                    };

                    db.Authors.Add(author);
                    db.SaveChanges();
                }

                return author;
            }
        }

        public static int CountAuthors()
        {
            using (var db = new StashBotDbContext())
            {
                return db.Authors
                    .ToList()
                    .Count();
            }

        }

        public static int CountAuthorQueue(long telegramId)
        {
            using (var db = new StashBotDbContext())
            {
                return db.Authors
                    .Where(a => a.TelegramId == telegramId)
                    .Include(a => a.QueueItems)
                    .FirstOrDefault()
                    .QueueItems
                    .Count();
            }
        }

        public static void DeleteAuthorRange(List<Author> authors)
        {
            using (var db = new StashBotDbContext())
            {
                db.Authors.RemoveRange(authors);
                db.SaveChanges();
            }
        }

        public static bool DoesAuthorExist(TelegramUser user)
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == user.Id);

                if(author != null)
                {
                    return true;
                } else {
                    return false;
                }
            }
        }

        public static Author GetAuthor(long telegramId, bool createIfNotExist = true)
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == telegramId);

                if (createIfNotExist)
                {
                    if (author == null)
                    {
                        author = new Author
                        {
                            TelegramId = telegramId,
                            TelegramDetailsLastUpdatedAt = DateTime.UtcNow
                        };

                        db.Authors.Add(author);
                        db.SaveChanges();
                    }
                    else
                    {
                        db.SaveChanges();
                    }
                }

                return author;
            }
        }

        public static Author GetAuthorByTelegramId(long telegramId)
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                        .FirstOrDefault(a => a.TelegramId == telegramId);
                return author;
            }
        }

        public static Author GetAuthorByTelegramUsername(string telegramUsername, bool addAt = true)
        {
            telegramUsername = telegramUsername.ToUpper();

            using (var db = new StashBotDbContext())
            {
                if (addAt)
                {
                    telegramUsername = $"@{telegramUsername}";
                }

                Author author = db.Authors
                        .FirstOrDefault(a => a.TelegramUsernameUpper == telegramUsername);
                return author;
            }
        }

        public static List<Author> GetAuthors()
        {
            using (var db = new StashBotDbContext())
            {
                return db.Authors
                    .Include(a => a.QueueItems)
                    .ToList();
            }
        }

        public static Author UpdateAuthorTelegramProfile(TelegramUser user)
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == user.Id);

                if (author != null)
                {
                    if (!(
                        author.TelegramName == user.Name &&
                        author.TelegramUsername == user.Username &&
                        author.TelegramUsernameUpper == user.Username.ToUpper()
                    ))
                    {
                        author.TelegramName = user.Name;
                        author.TelegramUsername = user.Username;
                        author.TelegramUsernameUpper = user.Username.ToUpper();
                        author.TelegramDetailsLastUpdatedAt = DateTime.UtcNow;

                        db.SaveChanges();
                    }
                }

                return author;
            }
        }

        public static void SetAuthorDeleteOthersPermission(
            long telegramId,
            bool canDeleteOthers
        )
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == telegramId);
                author.CanDeleteOthers = canDeleteOthers;
                db.SaveChanges();
            }
        }

        public static void SetAuthorFlushQueuePermission(
            long telegramId,
            bool canFlushQueue
        )
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == telegramId);
                author.CanFlushQueue = canFlushQueue;
                db.SaveChanges();
            }
        }

        public static void SetAuthorQueuePermission(
            long telegramId,
            bool canQueue
        )
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == telegramId);
                author.CanQueue = canQueue;
                db.SaveChanges();
            }
        }

        public static void SetAuthorManageAuthorsPermission(
            long telegramId,
            bool canManageAuthors
        )
        {
            using (var db = new StashBotDbContext())
            {
                Author author = db.Authors
                    .FirstOrDefault(a => a.TelegramId == telegramId);
                author.CanManageAuthors = canManageAuthors;
                db.SaveChanges();
            }
        }
    }
}