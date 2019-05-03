using System;

namespace Augmentrex
{
    static class Log
    {
        static readonly object _lock = new object();

        static void Output(ConsoleColor color, string format, params object[] args)
        {
            lock (_lock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(format, args);
                Console.ResetColor();
            }
        }

        public static void Important(string format, params object[] args)
        {
            Output(ConsoleColor.Green, format, args);
        }

        public static void Info(string format, params object[] args)
        {
            Output(ConsoleColor.White, format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            Output(ConsoleColor.Yellow, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Output(ConsoleColor.Red, format, args);
        }

        public static void Line()
        {
            Output(ConsoleColor.White, string.Empty);
        }
    }
}
