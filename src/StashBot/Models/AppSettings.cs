
namespace StashBot.Models
{
    public class AppSettings
    {
        public static string ApiKeys_Telegram { get; set; }
        public static long Config_ChannelId { get; set; }
        public static string Config_Name { get; set; }
        public static string Config_Owner { get; set; }
        public static bool Config_Poll { get; set; }
        public static int Config_PostInterval { get; set; }
    }
}