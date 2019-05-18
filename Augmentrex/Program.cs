using EasyHook;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Vanara.PInvoke;

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
            var name = $"{asmName.Name} {asmName.Version}";

            Console.Title = $"{name} - Host Process";

            Log.Info("{0} initializing...", name);

            var path = GetExecutablePath();

            if (path == null)
            {
                Log.Error("Could not find Hellgate: London executable.");
                return 1;
            }

            var chanName = IpcChannel.Create();

            Log.Info("Setting up IPC channel '{0}'...", chanName);

            var chan = IpcChannel.Connect(chanName);

            chan.Ping();

            Log.Info("Launching '{0}'...", path);

            var location = asm.Location;

            RemoteHooking.CreateAndInject(path, Configuration.Instance.GameArguments, 0, location, location, out var pid, chanName);

            using var proc = Process.GetProcessById(pid);

            if (proc == null)
            {
                Log.Error("Could not locate Hellgate: London process ({0}).", pid);
                return 1;
            }

            while (chan.Wait(Configuration.Instance.IpcTimeout))
            {
                if (chan.ExitCode is int c)
                {
                    Log.Important("Exiting...");

                    if (chan.KillRequested)
                        proc.Kill();

                    return c;
                }

                chan.Reset();
            }

            Log.Line();
            Log.Error("Lost connection to the game. Exiting...");
            Console.ReadLine();

            return 1;
        }

        static string GetExecutablePath()
        {
            var cfg = Configuration.Instance.GamePath;

            if (!string.IsNullOrWhiteSpace(cfg))
                return cfg;

            string steam = null;

            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                steam = (string)key?.GetValue("InstallPath");

            if (steam == null)
                return null;

            var steamApps = Path.Combine(steam, "steamapps");
            var libraryFolders = Path.Combine(steamApps, "libraryfolders.vdf");
            var libraryDirs = new List<string>
            {
                steamApps,
            };

            if (File.Exists(libraryFolders))
            {
                foreach (var line in File.ReadAllLines(libraryFolders))
                {
                    var match = Regex.Match(line, "^\\s*\"\\d*\"\\s*\"(?<path>\\S*)\"");

                    if (match.Success)
                        libraryDirs.Add(match.Groups["path"].Value);
                }
            }

            foreach (var library in libraryDirs)
            {
                var manifest = Path.Combine(library, "appmanifest_939520.acf");

                if (!File.Exists(manifest))
                    continue;

                foreach (var line in File.ReadAllLines(manifest))
                {
                    var match = Regex.Match(line, "^\\s*\"installdir\"\\s*\"(?<path>\\S*)\"");

                    if (match.Success)
                    {
                        var path = Path.Combine(library, "common", match.Groups["path"].Value, "bin", "Hellgate_sp_x86.exe");

                        if (File.Exists(path))
                            return path;
                    }
                }
            }

            return null;
        }
    }
}
