using Augmentrex.Commands;
using EasyHook;
using System;
using System.Runtime.Remoting;
using System.Threading;

namespace Augmentrex.Ipc
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

            CommandInterpreter.LoadCommands(true);
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

        public void Info(string format, params object[] args)
        {
            Log.Info(format, args);
        }

        public void InfoLine(string format, params object[] args)
        {
            Log.InfoLine(format, args);
        }

        public void Warning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        public void WarningLine(string format, params object[] args)
        {
            Log.WarningLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log.Error(format, args);
        }

        public void ErrorLine(string format, params object[] args)
        {
            Log.ErrorLine(format, args);
        }

        public void Success(string format, params object[] args)
        {
            Log.Success(format, args);
        }

        public void SuccessLine(string format, params object[] args)
        {
            Log.SuccessLine(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Log.Debug(format, args);
        }

        public void DebugLine(string format, params object[] args)
        {
            Log.DebugLine(format, args);
        }

        public void Color(ConsoleColor color, string format, params object[] args)
        {
            Log.Color(color, format, args);
        }

        public void ColorLine(ConsoleColor color, string format, params object[] args)
        {
            Log.ColorLine(color, format, args);
        }

        public void Line()
        {
            Log.Line();
        }

        public void PrintPrompt()
        {
            _counter++;

            Log.Color(ConsoleColor.Cyan, "hgl({0})> ", _counter);
        }

        public string ReadPrompt()
        {
            PrintPrompt();

            string str;

            try
            {
                str = ReadLine.Read();
            }
            catch (Exception)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(str))
                ReadLine.AddHistory(str);

            return str;
        }

        public void Clear()
        {
            Console.Clear();
        }
    }
}
