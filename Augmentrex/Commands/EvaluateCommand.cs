using Augmentrex.Memory;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Augmentrex.Commands
{
    public sealed class EvaluateCommand : Command
    {
        public sealed class Globals
        {
#pragma warning disable IDE1006 // Naming Styles

            public CommandContext ctx { get; }

            public Process proc { get; }

            public MemoryWindow mem { get; }

            public Configuration cfg { get; }

#pragma warning restore IDE1006 // Naming Styles

            internal Globals(CommandContext context)
            {
                ctx = context;
                proc = context.Process;
                mem = context.Memory;
                cfg = context.Configuration;
            }
        }

        sealed class EvaluateOptions
        {
            [Option('c')]
            public bool Clear { get; set; }

            [Value(0, Required = true)]
            public IEnumerable<string> Fragments { get; set; }
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "evaluate" };

        public override string Description =>
            "Evaluates a given C# expression.";

        public override string Syntax =>
            "[-c] <expr>";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If '-c' is given, the REPL session will be reset.",
                "The global variables " +
                string.Join(", ", typeof(Globals).GetTypeInfo().DeclaredProperties.Select(x => $"'{x.Name}'")) +
                " are implicitly defined for convenience.",
            };

        Script _script;

        public override int? Run(CommandContext context, string[] args)
        {
            var opts = Parse<EvaluateOptions>(context, args);

            if (opts == null)
                return null;

            if (_script == null || opts.Clear)
            {
                var imports = ScriptOptions.Default.Imports.AddRange(new[]
                {
                    nameof(Augmentrex),
                    $"{nameof(Augmentrex)}.{nameof(Memory)}",
                });
                var options = ScriptOptions.Default
                    .WithFilePath("<hgl>")
                    .WithImports(imports)
                    .WithLanguageVersion(LanguageVersion.Preview)
                    .WithReferences(Assembly.GetExecutingAssembly());

                _script = CSharpScript.Create(string.Empty, options, typeof(Globals));
            }

            Script script = _script.ContinueWith(string.Join(" ", opts.Fragments));

            Task<ScriptState> task;

            try
            {
                task = script.RunAsync(new Globals(context));
            }
            catch (CompilationErrorException ex)
            {
                foreach (var diag in ex.Diagnostics)
                    context.Error("{0}", diag.ToString());

                return null;
            }

            _script = script;

            context.Info("{0}", task.Result.ReturnValue);

            return null;
        }
    }
}
