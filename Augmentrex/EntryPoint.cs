using Augmentrex.Memory;
using EasyHook;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace Augmentrex
{
    public sealed class EntryPoint : IEntryPoint
    {
        public EntryPoint(RemoteHooking.IContext context, string channelName)
        {
        }

        public void Run(RemoteHooking.IContext context, string channelName)
        {
            RemoteHooking.WakeUpProcess();

            var channel = IpcChannel.Connect(channelName);

            channel.Ping();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                channel.Line();
                channel.Error("Injected assembly crashed: {0}", e.ExceptionObject);
            };

            channel.KeepAlive();

            Task.Factory.StartNew(async () =>
            {
                while (channel.ExitCode == null)
                {
                    channel.KeepAlive();

                    await Task.Delay(channel.Configuration.IpcKeepAlive);
                }
            }, TaskCreationOptions.LongRunning);

            channel.Info("Creating console window for injected assembly...");

            if (Kernel32.AllocConsole())
            {
                var asmName = Assembly.GetExecutingAssembly().GetName();

                Console.Title = $"{asmName.Name} {asmName.Version} - Game Process";
            }
            else
                channel.Warning("Could not allocate console window: {0}", Win32.GetLastError());

            using var process = Process.GetCurrentProcess();
            var mod = process.MainModule;
            var addr = (MemoryAddress)mod.BaseAddress;
            var size = mod.ModuleMemorySize;

            channel.Info("Making main module ({0}..{1}) writable...", addr, addr + (MemoryOffset)size);

            if (!Kernel32.VirtualProtectEx(process.Handle, addr, size, Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE, out var _))
                channel.Warning("Could not make main module writable: {0}", Win32.GetLastError());

            CommandInterpreter.LoadCommands();

            var interp = new CommandInterpreter(process, channel);
            var rc = channel.Configuration.RunCommands;

            if (rc.Length != 0)
            {
                channel.Info("Running startup commands...");

                foreach (var cmd in rc)
                    interp.RunCommand(cmd);
            }

            channel.Important("Injection completed successfully. Type 'help' for a list of commands.");
            channel.Line();

            interp.RunLoop();

            Kernel32.FreeConsole();
        }
    }
}
