using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Augmentrex
{
    static class Launcher
    {
        public static string ExecutableName { get; } = "Hellgate_sp_x86.exe";

        public static Process Process { get; set; }

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
                        var path = Path.Combine(library, "common", match.Groups["path"].Value, "bin", ExecutableName);

                        if (File.Exists(path))
                            return path;
                    }
                }
            }

            return null;
        }

        public static Process AttachOrLaunch()
        {
            var process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ExecutableName)).FirstOrDefault();

            if (process != null)
            {
                Log.Info("Found Hellgate: London process '{0}' ({1}).", process.ProcessName, process.Id);

                var file = Path.GetFileName(process.MainModule.FileName);

                if (file != ExecutableName)
                    Log.Warning("Process '{0}' ({1}) has unexpected main module '{2}'.", process.ProcessName, process.Id, file);

                return process;
            }

            Log.Warning("Could not find Hellgate: London process. Looking for the game directory...");

            var path = GetExecutablePath();

            if (path == null)
                return null;

            Log.Info("Attempting to launch '{0}'...", path);

            process = new Process()
            {
                StartInfo = new ProcessStartInfo(path)
                {
                    Arguments = Configuration.Instance.GameArguments,
                    UseShellExecute = false,
                }
            };

            process.Start();
            process.WaitForInputIdle();

            return Process = process;
        }

        public static void Dispose()
        {
            Process?.Dispose();
        }
    }
}
