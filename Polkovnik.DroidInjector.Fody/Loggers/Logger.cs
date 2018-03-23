using System;

namespace Polkovnik.DroidInjector.Fody.Loggers
{
    internal static class Logger
    {
        private static LogLevel _logLevel;
        private static Action<string> _log;

        public static void Init(LogLevel level, Action<string> logAction)
        {
            _logLevel = level;
            _log = logAction;
        }

        public static void Write(string message)
        {
            if (_logLevel >= LogLevel.Full)
                _log?.Invoke(message);
        }

        public static void Debug(string message)
        {
            if (_logLevel >= LogLevel.Debug)
                _log?.Invoke(message);
        }

        public static void Info(string message)
        {
            _log?.Invoke(message);
        }

        public static void LogExecute(object @object)
        {
            Write($"Execute :: {@object.GetType().Name} :: {@object}");
        }
    }
}
