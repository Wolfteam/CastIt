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
                logger?.Info(msg);
            }
            else
            {
                logger?.Info(msg);
                Debug.WriteLine(msg, "INFO");
                Console.WriteLine(msg);
            }
        }

        public static void LogWarn(this IMvxLog logger, string msg)
        {
            if (!Player.CanLog)
            {
                return;
            }

            logger?.Warn(msg);
            Debug.WriteLine(msg, "WARNING");
            Console.WriteLine(msg);
        }

        public static void LogError(this IMvxLog logger, string msg, Exception ex)
        {
            if (!Player.CanLog)
            {
                return;
            }
            logger?.Error(ex, msg);
            Debug.WriteLine(msg, "ERROR");
            Console.WriteLine(msg);
        }

        public static void LogTrace(this IMvxLog logger, string msg)
        {
            if (!Player.CanLog)
            {
                return;
            }
            logger?.Trace(msg);
            if (logger?.IsTraceEnabled() == true)
            {
                Debug.WriteLine(msg, "TRACE");
                Console.WriteLine(msg);
            }
        }
    }
}
