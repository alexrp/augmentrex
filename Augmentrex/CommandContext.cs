using Augmentrex.Memory;
using System.Diagnostics;

namespace Augmentrex
{
    public sealed class CommandContext
    {
        public Process Process { get; }

        internal IpcChannel Channel { get; }

        public MemoryWindow Memory { get; }

        public Configuration Configuration => Channel.Configuration;

        internal CommandContext(Process process, IpcChannel channel)
        {
            Process = process;
            Channel = channel;

            var mod = process.MainModule;

            Memory = new MemoryWindow(mod.BaseAddress, (uint)mod.ModuleMemorySize);
        }

        public void Important(string format, params object[] args)
        {
            Channel.Important(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Channel.Info(format, args);
        }

        public void Warning(string format, params object[] args)
        {
            Channel.Warning(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Channel.Error(format, args);
        }

        public void Line()
        {
            Channel.Line();
        }
    }
}
