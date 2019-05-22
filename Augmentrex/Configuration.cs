using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace Augmentrex
{
    [Serializable]
    public sealed class Configuration
    {
        static readonly Lazy<Configuration> _lazy =
            new Lazy<Configuration>(() => new Configuration(ConfigurationManager.AppSettings));

        internal static Configuration Instance => _lazy.Value;

        public string GamePath { get; set; }

        public string GameArguments { get; set; }

        public TimeSpan IpcTimeout { get; set; }

        public TimeSpan IpcKeepAlive { get; set; }

        public bool GameConsoleEnabled { get; set; }

        public bool DebugListenerEnabled { get; set; }

        public TimeSpan DebugListenerInterval { get; set; }

        public int HotKeyBeepFrequency { get; set; }

        public TimeSpan HotKeyBeepDuration { get; set; }

        public string[] DisabledPlugins { get; set; }

        public string[] RunCommands { get; set; }

        Configuration(NameValueCollection cfg)
        {
            GamePath = cfg["gamePath"];
            GameArguments = cfg["gameArguments"];
            IpcTimeout = TimeSpan.FromMilliseconds(int.Parse(cfg["ipcTimeout"]));
            IpcKeepAlive = TimeSpan.FromMilliseconds(int.Parse(cfg["ipcKeepAlive"]));
            DebugListenerInterval = TimeSpan.FromMilliseconds(int.Parse(cfg["debugListenerInterval"]));
            GameConsoleEnabled = bool.Parse(cfg["gameConsoleEnabled"]);
            DebugListenerEnabled = bool.Parse(cfg["debugListenerEnabled"]);
            HotKeyBeepFrequency = int.Parse(cfg["hotKeyBeepFrequency"]);
            HotKeyBeepDuration = TimeSpan.FromMilliseconds(int.Parse(cfg["HotKeyBeepDuration"]));
            DisabledPlugins = Split(cfg["disabledPlugins"], ',');
            RunCommands = Split(cfg["runCommands"], ';');
        }

        static string[] Split(string value, params char[] separators)
        {
            return value.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
    }
}
