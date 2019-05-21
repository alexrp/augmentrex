using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;

namespace Augmentrex.Commands
{
    public abstract class Command
    {
        public abstract IReadOnlyList<string> Names { get; }

        public abstract string Description { get; }

        public virtual string Syntax => string.Empty;

        public virtual IReadOnlyList<string> Details => Array.Empty<string>();

        protected static void PrintHelp(AugmentrexContext context, Command command, bool details)
        {
            context.SuccessLine("  {0} {1}", string.Join("|", command.Names), command.Syntax);
            context.InfoLine("    {0}", command.Description);

            if (details && command.Details.Count != 0)
            {
                foreach (var detail in command.Details)
                {
                    context.Line();
                    context.InfoLine("    {0}", detail);
                }
            }
        }

        public abstract int? Run(AugmentrexContext context, string[] args);

        protected T Parse<T>(AugmentrexContext context, string[] args)
            where T : class
        {
            return new Parser(opts =>
            {
                opts.AutoHelp = false;
                opts.AutoVersion = false;
                opts.CaseInsensitiveEnumValues = true;
                opts.CaseSensitive = false;
            }).ParseArguments<T>(args).MapResult(
                result => result,
                errors =>
                {
                    var builder = SentenceBuilder.Create();

                    foreach (var error in errors)
                        context.ErrorLine("{0}", builder.FormatError(error));

                    context.Line();
                    PrintHelp(context, this, true);
                    context.Line();

                    return null;
                });
        }
    }
}
