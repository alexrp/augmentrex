using System.Collections.Generic;

namespace Augmentrex.Commands.Core
{
    sealed class ExitCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "exit", "quit" };

        public override string Description =>
            "Detaches Augmentrex from the game and exits, leaving the game open.";

        public override int? Run(AugmentrexContext context, string[] args)
        {
            return 0;
        }
    }
}
