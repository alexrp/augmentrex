using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Augmentrex
{
    sealed class CommandInterpreter
    {
        public sealed class CommandAutoCompletionHandler : IAutoCompleteHandler
        {
            public char[] Separators { get; set; } = new[] { ' ' };

            public string[] GetSuggestions(string text, int index)
            {
                return Commands.SelectMany(x => x.Names).Where(x => x.StartsWith(text, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            }
        }

        static readonly List<Command> _commands = new List<Command>();

        public static IReadOnlyCollection<Command> Commands => _commands;

        public CommandContext Context { get; }

        public CommandInterpreter(Process process, IpcChannel channel)
        {
            Context = new CommandContext(process, channel);
        }

        public static void LoadCommands()
        {
            var exe = Assembly.GetExecutingAssembly();
            var asms = Directory.EnumerateFiles(Path.GetDirectoryName(exe.Location), "augmentrex-*.dll", SearchOption.AllDirectories)
                .Select(x => Assembly.LoadFile(x))
                .Concat(new[] { exe });

            foreach (var asm in asms)
                LoadCommandsIn(asm);

            _commands.Sort((a, b) => a.GetType().Name.CompareTo(b.GetType().Name));
        }

        static void LoadCommandsIn(Assembly assembly)
        {
            foreach (var type in assembly.ExportedTypes.Where(x => x.BaseType == typeof(Command)))
                _commands.Add((Command)Activator.CreateInstance(type));
        }

        public static Command GetCommand(string name)
        {
            return Commands.Where(x => x.Names.Any(x => x.StartsWith(name, StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault();
        }

        public bool RunCommand(string command)
        {
            var chan = Context.Channel;

            if (string.IsNullOrWhiteSpace(command))
                return true;

            var args = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            var name = args[0];
            var cmd = GetCommand(name);

            if (cmd == null)
            {
                chan.Error("Unknown command '{0}'.", name);

                return true;
            }

            int? code = null;

            try
            {
                code = cmd.Run(Context, args.Skip(1).ToArray());
            }
            catch (Exception ex)
            {
                chan.Error("Command '{0}' failed: {1}", name, ex.ToString());
            }

            if (code is int c)
            {
                chan.ExitCode = c;
                chan.KeepAlive();

                return false;
            }

            return true;
        }

        public void RunLoop()
        {
            while (true)
            {
                var chan = Context.Channel;
                var str = chan.ReadPrompt();

                if (!RunCommand(str))
                    break;
            }
        }
    }
}
