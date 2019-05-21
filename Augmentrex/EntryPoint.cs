using Augmentrex.Commands;
using Augmentrex.Memory;
using Augmentrex.Plugins;
using EasyHook;
using System;
using System.Diagnostics;
using Vanara.PInvoke;

namespace Augmentrex
{
    public sealed class EntryPoint : IEntryPoint
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public EntryPoint(RemoteHooking.IContext context, string channelName)
#pragma warning restore IDE0060 // Remove unused parameter
        {
        }

        public void Run(RemoteHooking.IContext context, string channelName)
        {
            AugmentrexContext ctx = null;

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                ctx?.Line();
                ctx?.ErrorLine("Injected assembly crashed: {0}", e.ExceptionObject);
            };

            ctx = new AugmentrexContext(false, Process.GetProcessById(context.HostPID), Process.GetCurrentProcess(), channelName, null);

            try
            {
                var mod = ctx.Game.MainModule;
                var addr = (MemoryAddress)mod.BaseAddress;
                var size = mod.ModuleMemorySize;

                ctx.Info("Making main module ({0}..{1}) writable... ", addr, addr + (MemoryOffset)size);

                if (Kernel32.VirtualProtectEx(ctx.Game.Handle, addr, size, Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE, out var _))
                    ctx.SuccessLine("OK.");
                else
                    ctx.WarningLine("{0}.", Win32.GetLastError());

                ctx.InfoLine("Waking up process... ");

                RemoteHooking.WakeUpProcess();

                PluginManager.LoadPlugins(ctx);
                CommandInterpreter.LoadCommands(false);

                var interp = new CommandInterpreter(ctx);
                var rc = ctx.Configuration.RunCommands;

                if (rc.Length != 0)
                {
                    ctx.InfoLine("Running startup commands...");

                    foreach (var cmd in rc)
                        interp.RunCommand(cmd);
                }

                ctx.SuccessLine("Injection completed successfully. Type 'help' for a list of commands.");
                ctx.Line();

                interp.RunLoop();
            }
            finally
            {
                PluginManager.UnloadPlugins();

                if (ctx is IDisposable disp)
                    disp.Dispose();
            }
        }
    }
}
