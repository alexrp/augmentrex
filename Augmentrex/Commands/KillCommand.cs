using System.Collections.Generic;

namespace Augmentrex.Commands
{
    public sealed class KillCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "kill" };

        public override string Description =>
            "Detaches Augmentrex from the game, kills the game process, then exits.";

        public override int? Run(CommandContext context, string[] args)
        {
            context.Channel.KillRequested = true;

            return 0;
        }
    }
}
