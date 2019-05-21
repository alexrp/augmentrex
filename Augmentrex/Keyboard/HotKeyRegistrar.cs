using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace Augmentrex.Keyboard
{
    public sealed class HotKeyRegistrar : MarshalByRefObject, IDisposable
    {
        sealed class MessageWindow : NativeWindow, IDisposable
        {
            readonly HotKeyRegistrar _registrar;

            bool _disposed;

            public MessageWindow(HotKeyRegistrar registrar)
            {
                _registrar = registrar;

                CreateHandle(new CreateParams());
            }

            ~MessageWindow()
            {
                RealDispose();
            }

            public void Dispose()
            {
                RealDispose();
            }

            void RealDispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                DestroyHandle();
            }

            protected override void WndProc(ref Message m)
            {
                const int HotKeyMessage = 0x312;

                if (m.Msg == HotKeyMessage)
                {
                    var key = (Keys)(((int)m.LParam >> 16) & 0xffff);
                    var mods = (User32.HotKeyModifiers)((int)m.LParam & 0xffff);

                    _registrar.Invoke(new HotKeyInfo(key,
                        mods.HasFlag(User32.HotKeyModifiers.MOD_ALT),
                        mods.HasFlag(User32.HotKeyModifiers.MOD_CONTROL),
                        mods.HasFlag(User32.HotKeyModifiers.MOD_SHIFT)));
                }

                base.WndProc(ref m);
            }
        }

        readonly CancellationTokenSource _cts;

        readonly Task _task;

        readonly ConcurrentQueue<HotKeyInfo> _newInfo = new ConcurrentQueue<HotKeyInfo>();

        readonly HashSet<HotKeyInfo> _registry = new HashSet<HotKeyInfo>();

        MessageWindow _window;

        bool _disposed;

        internal HotKeyRegistrar()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Factory.StartNew(() =>
            {
                _window = new MessageWindow(this);

                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    Application.Run();

                    while (_newInfo.TryDequeue(out var info))
                    {
                        var mods = User32.HotKeyModifiers.MOD_NOREPEAT;

                        if (info.Alt)
                            mods |= User32.HotKeyModifiers.MOD_ALT;

                        if (info.Control)
                            mods |= User32.HotKeyModifiers.MOD_CONTROL;

                        if (info.Shift)
                            mods |= User32.HotKeyModifiers.MOD_SHIFT;

                        User32.RegisterHotKey(_window.Handle, info.Id, mods, (uint)info.Key);
                    }
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        ~HotKeyRegistrar()
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

            _cts?.Cancel();
            Application.Exit();
            _task?.Wait();
            _task?.Dispose();

            if (_window != null)
            {
                if (_registry != null)
                    foreach (var info in _registry)
                        User32.UnregisterHotKey(_window.Handle, info.Id);

                _window.Dispose();
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public bool Add(HotKeyInfo info, HotKeyHandler handler)
        {
            if (!_registry.TryGetValue(info, out var existing))
            {
                info.Id = Kernel32.GlobalAddAtom(info.ToString());

                _registry.Add(info);
                _newInfo.Enqueue(info);
                Application.Exit();
            }
            else
                info = existing;

            return info.Handlers.Add(handler);
        }

        public bool Remove(HotKeyInfo info, HotKeyHandler handler)
        {
            return _registry.TryGetValue(info, out info) ? info.Handlers.Remove(handler) : false;
        }

        void Invoke(HotKeyInfo info)
        {
            if (_registry.TryGetValue(info, out info))
                foreach (var handler in info.Handlers)
                    handler.Invoke(info);
        }
    }
}
