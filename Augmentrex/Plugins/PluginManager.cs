using Augmentrex.Keyboard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Augmentrex.Plugins
{
    sealed class PluginManager : MarshalByRefObject
    {
        const string AssemblyFormat = "augmentrex-plugin-{0}.dll";

        static readonly List<(AppDomain, PluginManager)> _managers = new List<(AppDomain, PluginManager)>();

        readonly List<Plugin> _plugins = new List<Plugin>();

        AugmentrexContext _context;

        public static void LoadPlugins(AugmentrexContext context)
        {
            var exe = Assembly.GetExecutingAssembly();
            var loc = exe.Location;
            var asms = Directory.EnumerateFiles(Path.GetDirectoryName(loc), string.Format(AssemblyFormat, "*"));

            foreach (var asm in asms)
            {
                if (context.Configuration.DisabledPlugins.Any(x =>
                    Path.GetFileName(asm).Equals(string.Format(AssemblyFormat, x), StringComparison.CurrentCultureIgnoreCase)))
                    continue;

                var path = Path.GetDirectoryName(loc);
                var domain = AppDomain.CreateDomain(string.Empty, null, new AppDomainSetup
                {
                    ApplicationBase = path,
                    LoaderOptimization = LoaderOptimization.MultiDomainHost,
                    ShadowCopyFiles = "true",
                });
                var mgr = (PluginManager)domain.CreateInstanceAndUnwrap(exe.GetName().FullName, typeof(PluginManager).FullName);

                mgr.StartPlugins(asm, context.Host.Id, context.Game.Id, context.Ipc.ChannelName, context.HotKeys);
                _managers.Add((domain, mgr));
            }
        }

        public static void UnloadPlugins()
        {
            foreach (var (domain, mgr) in _managers)
            {
                mgr.StopPlugins();
                AppDomain.Unload(domain);
            }

            _managers.Clear();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal void StartPlugins(string path, int hostId, int gameId, string channelName, HotKeyRegistrar hotKeys)
        {
            var host = Process.GetProcessById(hostId);
            var game = Process.GetProcessById(gameId);

            _context = new AugmentrexContext(true, host, game, channelName, hotKeys);

            foreach (var type in Assembly.UnsafeLoadFrom(path).DefinedTypes.Where(x => x.BaseType == typeof(Plugin)))
            {
                var plugin = (Plugin)Activator.CreateInstance(type);

                _context.InfoLine("Starting plugin '{0}'...", plugin.Name);

                plugin.Start(_context);
                _plugins.Add(plugin);
            }
        }

        internal void StopPlugins()
        {
            foreach (var plugin in _plugins)
            {
                _context.InfoLine("Stopping plugin '{0}'...", plugin.Name);

                plugin.Stop(_context);
            }

            ((IDisposable)_context).Dispose();
        }
    }
}
