using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace Augmentrex.Commands.Core
{
    sealed class HelpCommand : Command
    {
        sealed class HelpOptions
        {
            [Value(0)]
            public IEnumerable<string> Commands { get; set; }
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "help", "?" };

        public override string Description =>
            "Requests help for given commands, or lists all commands.";

        public override string Syntax =>
            "[command ...]";

        public override int? Run(AugmentrexContext context, string[] args)
        {
            var opts = Parse<HelpOptions>(context, args);

            if (opts == null)
                return null;

            if (opts.Commands.Any())
            {
                var cmds = opts.Commands.Select(x => (x, CommandInterpreter.GetCommand(x)));
                var unknown = false;

                foreach (var (name, cmd) in cmds)
                {
                    if (cmd == null)
                    {
                        unknown = true;
                        context.ErrorLine("Unknown command '{0}'.", name);
                    }
                }

                if (unknown)
                    return null;

                context.Line();

                foreach (var (_, cmd) in cmds)
                {
                    PrintHelp(context, cmd, true);
                    context.Line();
                }
            }
            else
            {
                context.Line();
                context.InfoLine("Available commands:");
                context.Line();

                foreach (var cmd in CommandInterpreter.Commands)
                {
                    PrintHelp(context, cmd, false);
                    context.Line();
                }
            }

            return null;
        }
    }
}
