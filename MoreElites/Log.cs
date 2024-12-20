using System.Diagnostics;
using BepInEx.Logging;

namespace MoreElites
{
    internal static class Log
    {
        private static ManualLogSource _logger;
        public static void Init(ManualLogSource log) => _logger = log;

        [Conditional("DEBUG")]
        public static void Debug(object message) => LogData(LogLevel.Debug, message);
        public static void Info(object message) => LogData(LogLevel.Info, message);
        public static void Message(object message) => LogData(LogLevel.Message, message);
        public static void Warning(object message) => LogData(LogLevel.Warning, message);
        public static void Error(object message) => LogData(LogLevel.Error, message);
        public static void Fatal(object message) => LogData(LogLevel.Fatal, message);

        public static void LogData(LogLevel logLevel, object message) => _logger.Log(logLevel, message);
    }
}
