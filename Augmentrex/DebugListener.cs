using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Augmentrex
{
    sealed class DebugListener : IDisposable
    {
        readonly CancellationTokenSource _cts;

        readonly Task _task;

        bool _disposed;

        public DebugListener(AugmentrexContext context)
        {
            context.InfoLine("Starting debug message listener...");

            _cts = new CancellationTokenSource();
            _task = Task.Factory.StartNew(() =>
            {
                const int BufferSize = 4096;

                using var mmf = MemoryMappedFile.CreateNew("DBWIN_BUFFER", BufferSize);
                using var bufReady = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_BUFFER_READY");
                using var dataReady = new EventWaitHandle(false, EventResetMode.AutoReset, "DBWIN_DATA_READY");

                while (true)
                {
                    _cts.Token.ThrowIfCancellationRequested();

                    bufReady.Set();

                    if (!dataReady.WaitOne(context.Configuration.DebugListenerInterval))
                        continue;

                    using var stream = mmf.CreateViewStream(0, BufferSize, MemoryMappedFileAccess.Read);
                    using var reader = new BinaryReader(stream);

                    if (reader.ReadUInt32() != context.Game.Id)
                        continue;

                    var bytes = reader.ReadBytes(BufferSize - sizeof(int));
                    var str = Encoding.GetEncoding("euc-kr").GetString(bytes, 0, Array.IndexOf<byte>(bytes, 0));

                    Log.DebugLine("{0}", str.Substring(0, str.Length - 1));
                }
            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        ~DebugListener()
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
