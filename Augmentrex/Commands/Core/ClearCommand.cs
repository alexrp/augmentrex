using System;
using System.Collections.Generic;

namespace Augmentrex.Commands.Core
{
    sealed class ClearCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "clear" };

        public override string Description =>
            "Clears the console buffer, including the scrollback buffer.";

        public override int? Run(AugmentrexContext context, string[] args)
        {
            context.Ipc.Channel.Clear();
            Console.Clear();

            return null;
        }
    }
}
