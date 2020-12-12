using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace CastIt.GoogleCast.Extensions
{
    internal static class LogExtensions
    {
        public static void LogInfo(this ILogger logger, string msg)
        {
            logger?.LogInformation(msg);
            if (Player.CanLogToConsole)
            {
                Debug.WriteLine(msg, "INFO");
                Console.WriteLine(msg);
            }
        }

        public static void LogWarn(this ILogger logger, string msg)
        {
            logger?.LogWarning(msg);
            if (Player.CanLogToConsole)
            {
                Debug.WriteLine(msg, "WARNING");
                Console.WriteLine(msg);
            }
        }

        public static void LogError(this ILogger logger, string msg, Exception ex)
        {
            logger?.LogError(ex, msg);
            if (Player.CanLogToConsole)
            {
                Debug.WriteLine(msg, "ERROR");
                Console.WriteLine(msg);
            }
        }

        public static void LogTrace(this ILogger logger, string msg)
        {
            if (!Player.CanLogTrace)
            {
                return;
            }
            Debug.WriteLine(msg, "TRACE");
            Console.WriteLine(msg);
        }
    }
}
