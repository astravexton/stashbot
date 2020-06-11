using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StashBot.Models
{
    public class Author
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public long TelegramId { get; set; }
        public string TelegramName { get; set; }
        public string TelegramUsername { get; set; }
        public string TelegramUsernameUpper { get; set; }

        public bool CanDeleteOthers { get; set; } = false;
        public bool CanFlushQueue { get; set; } = false;
        public bool CanManageAuthors { get; set; } = false;
        public bool CanQueue { get; set; } = false;

        public DateTime TelegramDetailsLastUpdatedAt { get; set; }

        public List<QueueItem> QueueItems { get; set; }
    }
}