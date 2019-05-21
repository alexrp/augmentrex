using Augmentrex.Plugins;
using System;
using System.Collections.Generic;

namespace Augmentrex.Commands.Core
{
    sealed class ReloadCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "reload" };

        public override string Description =>
            "Unloads all active plugins and reloads all plugins from disk.";

        public override int? Run(AugmentrexContext context, string[] args)
        {
            PluginManager.UnloadPlugins();

            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();

            PluginManager.LoadPlugins(context);

            return null;
        }
    }
}
