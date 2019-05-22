using System;

namespace Augmentrex
{
    public struct Finally : IDisposable
    {
        public readonly Action Action;

        public Finally(Action action)
        {
            Action = action;
        }

        public void Dispose()
        {
            Action?.Invoke();
        }
    }
}
