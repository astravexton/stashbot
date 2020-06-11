using System;
using System.Reflection;

namespace StashBot.Utilities
{
    public class ReflectionUtilities
    {
        public static string GetEnvironment()
        {
            var v = Environment.Version;
            var n = (v.Major >= 5) ? ".NET" : ".NET Core";
            return $"{n} {v.Major}.{v.Minor}";
        }

        public static string GetVersion()
        {
            var v = Assembly.GetEntryAssembly().GetName().Version;
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }
    }
}