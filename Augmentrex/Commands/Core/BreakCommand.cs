using System.Collections.Generic;
using System.Diagnostics;

namespace Augmentrex.Commands.Core
{
    sealed class BreakCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { "break" };

        public override string Description =>
            "Signals a breakpoint to an attached debugger (attaches one if not present).";

        public override int? Run(AugmentrexContext context, string[] args)
        {
            if (!Debugger.IsAttached)
                Debugger.Launch();

            Debugger.Break();

            return null;
        }
    }
}
