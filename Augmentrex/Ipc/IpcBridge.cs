using System;
using System.Threading;
using System.Threading.Tasks;

namespace Augmentrex.Ipc
{
    sealed class IpcBridge : IDisposable
    {
        public string ChannelName { get; }

        public IpcChannel Channel { get; }

        public Configuration Configuration { get; }

        readonly CancellationTokenSource _cts;

        readonly Task _task;

        bool _disposed;

        internal IpcBridge(string channelName, bool shallow)
        {
            ChannelName = channelName;
            Channel = IpcChannel.Connect(channelName);

            Channel.Ping();

            Configuration = Channel.Configuration;

            if (shallow)
                return;

            Channel.KeepAlive();

            _cts = new CancellationTokenSource();
            _task = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    Channel.KeepAlive();

                    await Task.Delay(Configuration.IpcKeepAlive);
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        ~IpcBridge()
        {
            RealDispose();
        }

        public void Dispose()
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
            _task?.Wait();
            _task?.Dispose();
        }
    }
}
