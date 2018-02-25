using System;

namespace Polkovnik.DroidInjector.Fody
{
    internal static class Logger
    {
        public static bool DebugEnabled { get; set; }
        public static event Action<string> Log;

        public static void Debug(string message)
        {
            if (DebugEnabled)
                Log?.Invoke(message);
        }

        public static void Info(string message)
        {
            Log?.Invoke(message);
        }
    }
}
