using EasyHook;
using System;
using System.Runtime.Remoting;
using System.Threading;

namespace Augmentrex
{
    sealed class IpcChannel : MarshalByRefObject
    {
        readonly ManualResetEventSlim _keepAlive = new ManualResetEventSlim();

        int _counter;

        public Configuration Configuration { get; }

        public int? ExitCode { get; set; }

        public bool KillRequested { get; set; }

        public IpcChannel()
        {
            Configuration = Configuration.Instance;

            CommandInterpreter.LoadCommands();
            ReadLine.AutoCompletionHandler = new CommandInterpreter.CommandAutoCompletionHandler();
        }

        public static string Create()
        {
            string name = null;

            RemoteHooking.IpcCreateServer<IpcChannel>(ref name, WellKnownObjectMode.Singleton);

            return name;
        }

        public static IpcChannel Connect(string name)
        {
            return RemoteHooking.IpcConnectClient<IpcChannel>(name);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Ping()
        {
        }

        public bool Wait(TimeSpan timeout)
        {
            return _keepAlive.Wait(timeout);
        }

        public void KeepAlive()
        {
            _keepAlive.Set();
        }

        public void Reset()
        {
            _keepAlive.Reset();
        }

        public void Important(string format, params object[] args)
        {
            Log.Important(format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log.Info(format, args);
        }

        public void Warning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log.Error(format, args);
        }

        public void Line()
        {
            Log.Line();
        }

        public string ReadPrompt()
        {
            var str = ReadLine.Read($"hgl({++_counter})> ");

            ReadLine.AddHistory(str);

            return str;
        }

        public void Clear()
        {
            Console.Clear();
        }
    }
}
