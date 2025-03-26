using System;

namespace Server
{
    public static class Utils
    {
        public static string GetFormattedTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }
}
