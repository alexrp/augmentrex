using Augmentrex.Memory;
using EasyHook;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                channel.Line();
                channel.Error("Injected assembly crashed: {0}", e.ExceptionObject);
            };

            channel.Ping();
            channel.KeepAlive();

            Task.Factory.StartNew(async () =>
            {
                while (channel.ExitCode == null)
                {
                    channel.KeepAlive();

                    await Task.Delay(channel.Configuration.IpcKeepAlive);
                }
            }, TaskCreationOptions.LongRunning);

            var process = Process.GetCurrentProcess();
            var mod = process.MainModule;
            var addr = (MemoryAddress)mod.BaseAddress;
            var size = mod.ModuleMemorySize;

            channel.Info("Making main module ({0}..{1}) writable...", addr, addr + (MemoryOffset)size);

            if (!MemoryWin32.VirtualProtectEx(process.Handle, addr, (UIntPtr)size, 0x04, out var _))
                channel.Error("Could not make main module writable. Code modification will not be possible.");

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
        }
    }
}
