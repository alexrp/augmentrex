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

        public string[] RunCommands { get; set; }

        Configuration(NameValueCollection cfg)
        {
            GamePath = cfg["gamePath"];
            GameArguments = cfg["gameArguments"];
            IpcTimeout = TimeSpan.FromMilliseconds(int.Parse(cfg["ipcTimeout"]));
            IpcKeepAlive = TimeSpan.FromMilliseconds(int.Parse(cfg["ipcKeepAlive"]));
            RunCommands = Split(cfg["runCommands"], ';');
        }

        static string[] Split(string value, params char[] separators)
        {
            return value.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }
    }
}
