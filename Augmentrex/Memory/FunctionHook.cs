using EasyHook;
using System;

namespace Augmentrex.Memory
{
    public sealed class FunctionHook<T> : IDisposable
        where T : Delegate
    {
        public MemoryAddress Address { get; }

        public string Name { get; }

        public T Original { get; }

        public T Hook { get; }

        public bool IsInstalled { get; private set; }

        readonly LocalHook _hook;

        bool _disposed;

        internal FunctionHook(MemoryAddress address, string name, T original, T hook)
        {
            Address = address;
            Name = name;
            Original = original;
            Hook = hook;

            _hook = LocalHook.Create(address, hook, this);
        }

        ~FunctionHook()
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

            _hook?.Dispose();
        }

        public void Install()
        {
            _hook.ThreadACL.SetExclusiveACL(null);
            IsInstalled = true;
        }

        public void Uninstall()
        {
            _hook.ThreadACL.SetInclusiveACL(null);
            IsInstalled = false;
        }

        public void Toggle()
        {
            if (!IsInstalled)
                Install();
            else
                Uninstall();
        }
    }
}
