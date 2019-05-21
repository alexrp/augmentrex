using System;

namespace Augmentrex.Keyboard
{
    public sealed class HotKeyHandler : MarshalByRefObject
    {
        public Action<HotKeyInfo> Handler { get; }

        public HotKeyHandler(Action<HotKeyInfo> handler)
        {
            Handler = handler;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Invoke(HotKeyInfo info)
        {
            Handler(info);
        }
    }
}
