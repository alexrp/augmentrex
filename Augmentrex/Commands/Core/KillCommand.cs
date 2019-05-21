using System.Collections.Generic;

namespace Augmentrex.Commands.Core
{
    sealed class KillCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "kill" };

        public override string Description =>
            "Detaches Augmentrex from the game, kills the game process, then exits.";

        public override int? Run(AugmentrexContext context, string[] args)
        {
            context.Ipc.Channel.KillRequested = true;

            return 0;
        }
    }
}
