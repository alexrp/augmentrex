using EasyHook;
using System;
using System.Reflection;

namespace Augmentrex
{
    static class Program
    {
        static int Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Log.Line();
                Log.Error("Host assembly crashed: {0}", e.ExceptionObject);
                Console.ReadLine();
            };

            var asm = Assembly.GetExecutingAssembly();
            var asmName = asm.GetName();

            Log.Info("{0} {1} initializing...", asmName.Name, asmName.Version);

            var process = Launcher.AttachOrLaunch();

            if (process == null)
            {
                Log.Error("Could not attach to or launch Hellgate: London.");
                return 1;
            }

            try
            {
                var chanName = IpcChannel.Create();

                Log.Info("Connecting to IPC channel '{0}'...", chanName);

                var chan = IpcChannel.Connect(chanName);

                chan.Ping();

                Log.Info("Injecting process '{0}' ({1})...", process.ProcessName, process.Id);

                var location = asm.Location;

                RemoteHooking.Inject(process.Id, InjectionOptions.DoNotRequireStrongName, location, location, chanName);

                while (chan.Wait(Configuration.Instance.IpcTimeout))
                {
                    if (chan.ExitCode is int c)
                    {
                        Log.Important("Exiting...");

                        if (chan.KillRequested)
                            process.Kill();

                        return c;
                    }

                    chan.Reset();
                }

                Log.Line();
                Log.Error("Lost connection to the game. Exiting...");
                Console.ReadLine();

                return 1;
            }
            finally
            {
                Launcher.Dispose();
            }
        }
    }
}
