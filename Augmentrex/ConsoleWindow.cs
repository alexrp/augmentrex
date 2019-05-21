using System;
using System.Reflection;
using Vanara.PInvoke;

namespace Augmentrex
{
    sealed class ConsoleWindow : IDisposable
    {
        bool _disposed;

        public ConsoleWindow(AugmentrexContext context)
        {
            context.Info("Allocating game console window... ");

            if (Kernel32.AllocConsole())
            {
                context.SuccessLine("OK.");

                var asmName = Assembly.GetExecutingAssembly().GetName();

                Console.Title = $"{asmName.Name} {asmName.Version} - Game Process";
            }
            else
                context.WarningLine("{0}.", Win32.GetLastError());
        }

        ~ConsoleWindow()
        {
            RealDispose();
        }

        public void Dispose()
        {
            RealDispose();
            GC.SuppressFinalize(this);
        }

        void RealDispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Kernel32.FreeConsole();
        }
    }
}
