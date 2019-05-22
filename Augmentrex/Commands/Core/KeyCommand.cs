using Augmentrex.Keyboard;
using CommandLine;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Augmentrex.Commands.Core
{
    sealed class KeyCommand : Command
    {
        sealed class KeyOptions
        {
            [Option("add", SetName = "operation")]
            public bool Add { get; set; }

            [Option("del", SetName = "operation")]
            public bool Delete { get; set; }

            [Option('a')]
            public bool Alt { get; set; }

            [Option('c')]
            public bool Control { get; set; }

            [Option('s')]
            public bool Shift { get; set; }

            [Value(0)]
            public Keys Key { get; set; }

            [Value(1)]
            public IEnumerable<string> Fragments { get; set; } = new string[0];
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "key" };

        public override string Description =>
            "Manipulates global hot key bindings.";

        public override string Syntax =>
            "[[-a] [-c] [-s] (--add <key> <command>|--del <key>)]";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If --add or --del is given, a key binding will be added or deleted, respectively. " +
                "If neither is given, all active key bindings are listed.",
                "The -a, -c, and -s options correspond to the Alt, Control, and Shift modifier keys, " +
                "respectively. They can be combined (or left unused) as needed.",
            };

        readonly Dictionary<HotKeyInfo, string> _bindings = new Dictionary<HotKeyInfo, string>();

        HotKeyHandler _handler;

        public override int? Run(AugmentrexContext context, string[] args)
        {
            var opts = Parse<KeyOptions>(context, args);

            if (opts == null)
                return null;

            if (!opts.Add && !opts.Delete)
            {
                foreach (var kvp in _bindings)
                    context.InfoLine("{0} = {1}", kvp.Key, kvp.Value);

                return null;
            }

            var info = new HotKeyInfo(opts.Key, opts.Alt, opts.Control, opts.Shift);

            if (_handler == null)
            {
                void KeyHandler(HotKeyInfo info)
                {
                    var freq = context.Configuration.HotKeyBeepFrequency;

                    if (freq != 0)
                        context.Ipc.Channel.Beep(freq, context.Configuration.HotKeyBeepDuration);

                    if (_bindings.TryGetValue(info, out var command))
                        context.Interpreter.RunCommand(command, true);
                }

                _handler = new HotKeyHandler(KeyHandler);
            }

            if (opts.Add)
            {
                var command = string.Join(" ", opts.Fragments);

                if (string.IsNullOrWhiteSpace(command))
                {
                    context.ErrorLine("No command given.");

                    return null;
                }

                if (_bindings.TryAdd(info, command))
                {
                    context.HotKeys.Add(info, _handler);

                    context.InfoLine("Added key binding: {0} = {1}", info, command);
                }
                else
                    context.ErrorLine("Key binding already exists: {0} = {1}", info, _bindings[info]);
            }

            if (opts.Delete)
            {
                if (_bindings.Remove(info, out var command))
                    context.InfoLine("Deleted key binding: {0} = {1}", info, command);
                else
                    context.ErrorLine("Key binding not found: {0}", info);
            }

            return null;
        }
    }
}
