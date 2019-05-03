using System.Collections.Generic;

namespace Augmentrex.Commands
{
    public sealed class ClearCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "clear" };

        public override string Description =>
            "Clears the console buffer, including the scrollback buffer.";

        public override int? Run(CommandContext context, string[] args)
        {
            context.Channel.Clear();

            return null;
        }
    }
}
