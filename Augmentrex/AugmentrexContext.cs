using Augmentrex.Commands;
using Augmentrex.Ipc;
using Augmentrex.Keyboard;
using Augmentrex.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Augmentrex
{
    public sealed class AugmentrexContext : IDisposable
    {
        public Process Host { get; }

        public Process Game { get; }

        public MemoryWindow Memory { get; }

        internal IpcBridge Ipc { get; }

        public Configuration Configuration => Ipc.Configuration;

        public HotKeyRegistrar HotKeys { get; }

        internal CommandInterpreter Interpreter { get; }

        readonly ConsoleWindow _console;

        readonly DebugListener _debug;

        readonly HashSet<IDisposable> _hooks = new HashSet<IDisposable>();

        readonly bool _shallow;

        bool _disposed;

        internal AugmentrexContext(bool shallow, Process host, Process game, string channelName, HotKeyRegistrar hotKeys)
        {
            Host = host;
            Game = game;

            var mod = game.MainModule;

            Memory = new MemoryWindow(mod.BaseAddress, (uint)mod.ModuleMemorySize);
            Ipc = new IpcBridge(channelName, shallow);
            HotKeys = hotKeys ?? new HotKeyRegistrar();

            if (!shallow)
            {
                Interpreter = new CommandInterpreter(this);
                _console = Configuration.GameConsoleEnabled ? new ConsoleWindow(this) : null;
                _debug = Configuration.DebugListenerEnabled ? new DebugListener(this) : null;
            }

            _shallow = shallow;
        }

        ~AugmentrexContext()
        {
            RealDispose();
        }

        void IDisposable.Dispose()
        {
            RealDispose();
            GC.SuppressFinalize(this);
        }

        void RealDispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_hooks != null)
                foreach (var hook in _hooks)
                    hook.Dispose();

            _debug?.Dispose();
            _console?.Dispose();

            if (!_shallow)
                ((IDisposable)HotKeys)?.Dispose();

            Ipc?.Dispose();
            Game?.Dispose();
            Host?.Dispose();
        }

        public void Info(string format, params object[] args)
        {
            Ipc.Channel.Info(format, args);
        }

        public void InfoLine(string format, params object[] args)
        {
            Ipc.Channel.InfoLine(format, args);
        }

        public void Warning(string format, params object[] args)
        {
            Ipc.Channel.Warning(format, args);
        }

        public void WarningLine(string format, params object[] args)
        {
            Ipc.Channel.WarningLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Ipc.Channel.Error(format, args);
        }

        public void ErrorLine(string format, params object[] args)
        {
            Ipc.Channel.ErrorLine(format, args);
        }

        public void Success(string format, params object[] args)
        {
            Ipc.Channel.Success(format, args);
        }

        public void SuccessLine(string format, params object[] args)
        {
            Ipc.Channel.SuccessLine(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Ipc.Channel.Debug(format, args);
        }

        public void DebugLine(string format, params object[] args)
        {
            Ipc.Channel.DebugLine(format, args);
        }

        public void Color(ConsoleColor color, string format, params object[] args)
        {
            Ipc.Channel.Color(color, format, args);
        }

        public void ColorLine(ConsoleColor color, string format, params object[] args)
        {
            Ipc.Channel.ColorLine(color, format, args);
        }

        public void Line()
        {
            Ipc.Channel.Line();
        }

        public FunctionHook<T> CreateHook<T>(MemoryOffset offset, string name, T hook)
            where T : Delegate
        {
            var address = Memory.ToAddress(offset);
            var original = Marshal.GetDelegateForFunctionPointer<T>(address);
            var fh = new FunctionHook<T>(address, name, original, hook);

            _hooks.Add(fh);

            return fh;
        }

        public FunctionHook<T> CreateHook<T>(byte?[] pattern, string name, T hook)
            where T : Delegate
        {
            return Memory.Search(pattern).Cast<MemoryOffset?>().FirstOrDefault() is MemoryOffset o ? CreateHook(o, name, hook) : null;
        }

        public void DeleteHook<T>(FunctionHook<T> hook)
            where T : Delegate
        {
            _hooks.Remove(hook);
            ((IDisposable)hook).Dispose();
        }
    }
}
