using MvvmCross.Logging;
using System;
using System.Diagnostics;

namespace CastIt.GoogleCast.Extensions
{
    internal static class LogExtensions
    {
        public static void LogInfo(this IMvxLog logger, string msg)
        {
            if (!Player.CanLog)
            {
                return;
            }

            logger?.Info(msg);
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }

        public static void LogError(this IMvxLog logger, string msg, Exception ex)
        {
            if (!Player.CanLog)
            {
                return;
            }

            logger?.Error(ex, msg);
            Debug.WriteLine(msg);
            Console.WriteLine(msg);
        }
    }
}
