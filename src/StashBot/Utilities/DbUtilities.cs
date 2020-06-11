using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace StashBot.Utilities
{
    public class DbUtilities
    {
        public static void BackupDatabase()
        {
            if (!Directory.Exists("_backup"))
            {
                Directory.CreateDirectory("_backup");
            }

            File.Copy(
                "stashbot.db",
                $"_backup/stashbot_{DateTime.Now.ToString("yyyyMMddHHmmss")}.db"
            );
        }

        public static void MigrateDatabase()
        {
            var context = new StashBotDbContext();
            context.Database.Migrate();
        }
    }
}