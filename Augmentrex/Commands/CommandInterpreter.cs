using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Augmentrex.Commands
{
    sealed class CommandInterpreter
    {
        public sealed class CommandAutoCompletionHandler : IAutoCompleteHandler
        {
            public char[] Separators { get; set; } = new[] { ' ' };

            public string[] GetSuggestions(string text, int index)
            {
                return Commands.SelectMany(x => x.Names).Where(y => MatchCommand(y, text)).ToArray();
            }
        }

        static readonly List<Command> _commands = new List<Command>();

        public static IReadOnlyCollection<Command> Commands => _commands;

        readonly object _lock = new object();

        public AugmentrexContext Context { get; }

        public CommandInterpreter(AugmentrexContext context)
        {
            Context = context;
        }

        public static void LoadCommands(bool informational)
        {
            var exe = Assembly.GetExecutingAssembly();
            var asms = Directory.EnumerateFiles(Path.GetDirectoryName(exe.Location), "augmentrex-command-*.dll")
                .Select(x => Assembly.UnsafeLoadFrom(x))
                .Concat(new[] { exe });

            static Command InstantiateCommand(Type type, bool informational)
            {
                const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                var ctor1 = type.GetConstructor(Flags, null, new[] { typeof(bool) }, null);
                var ctor2 = type.GetConstructor(Flags, null, Type.EmptyTypes, null);

                return (Command)(ctor1 != null ? ctor1.Invoke(new object[] { informational }) : ctor2.Invoke(null));
            }

            foreach (var asm in asms)
                foreach (var type in asm.DefinedTypes.Where(x => x.BaseType == typeof(Command)))
                    _commands.Add(InstantiateCommand(type, informational));

            _commands.Sort((a, b) => string.Compare(a.GetType().Name, b.GetType().Name, StringComparison.InvariantCulture));
        }

        public static Command GetCommand(string name)
        {
            return Commands.Where(x => x.Names.Any(y => MatchCommand(y, name))).FirstOrDefault();
        }

        public bool RunCommand(string command, bool async)
        {
            if (string.IsNullOrWhiteSpace(command))
                return true;

            var args = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            var name = args[0];
            var cmd = GetCommand(name);

            lock (_lock)
            {
                var chan = Context.Ipc.Channel;

                if (async)
                {
                    Context.Line();
                    chan.PrintPrompt();
                    chan.ColorLine(Console.ForegroundColor, "{0}", command);
                }

                using var _ = async ? new Finally(() => chan.PrintPrompt()) : default;

                if (cmd == null)
                {
                    Context.ErrorLine("Unknown command '{0}'.", name);

                    return true;
                }

                int? code = null;

                try
                {
                    code = cmd.Run(Context, args.Skip(1).ToArray());
                }
                catch (Exception ex)
                {
                    Context.ErrorLine("Command '{0}' failed: {1}", name, ex);
                }

                if (code is int c)
                {
                    chan.ExitCode = c;
                    chan.KeepAlive();

                    return false;
                }

                return true;
            }
        }

        public void RunLoop()
        {
            while (true)
            {
                var chan = Context.Ipc.Channel;
                var str = chan.ReadPrompt();

                if (str == null || chan.ExitCode != null || !RunCommand(str, false))
                    break;
            }
        }

        static bool MatchCommand(string command, string prefix)
        {
            return command.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
