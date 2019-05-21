using System;

namespace Augmentrex
{
    static class Log
    {
        static readonly object _lock = new object();

        static void Output(ConsoleColor color, bool line, string format, params object[] args)
        {
            lock (_lock)
            {
                Console.ForegroundColor = color;

                try
                {
                    Console.Write(format, args);

                    if (line)
                        Console.WriteLine();
                }
                finally
                {
                    Console.ResetColor();
                }
            }
        }

        public static void Info(string format, params object[] args)
        {
            Output(ConsoleColor.White, false, format, args);
        }

        public static void InfoLine(string format, params object[] args)
        {
            Output(ConsoleColor.White, true, format, args);
        }

        public static void Warning(string format, params object[] args)
        {
            Output(ConsoleColor.Yellow, false, format, args);
        }

        public static void WarningLine(string format, params object[] args)
        {
            Output(ConsoleColor.Yellow, true, format, args);
        }

        public static void Error(string format, params object[] args)
        {
            Output(ConsoleColor.Red, false, format, args);
        }

        public static void ErrorLine(string format, params object[] args)
        {
            Output(ConsoleColor.Red, true, format, args);
        }

        public static void Success(string format, params object[] args)
        {
            Output(ConsoleColor.Green, false, format, args);
        }

        public static void SuccessLine(string format, params object[] args)
        {
            Output(ConsoleColor.Green, true, format, args);
        }

        public static void Debug(string format, params object[] args)
        {
            Output(ConsoleColor.Magenta, false, format, args);
        }

        public static void DebugLine(string format, params object[] args)
        {
            Output(ConsoleColor.Magenta, true, format, args);
        }

        public static void Color(ConsoleColor color, string format, params object[] args)
        {
            Output(color, false, format, args);
        }

        public static void ColorLine(ConsoleColor color, string format, params object[] args)
        {
            Output(color, true, format, args);
        }

        public static void Line()
        {
            Output(ConsoleColor.White, true, string.Empty);
        }
    }
}
