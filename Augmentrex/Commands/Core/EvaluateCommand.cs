using Augmentrex.Keyboard;
using Augmentrex.Memory;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Augmentrex.Commands.Core
{
    public sealed class EvaluateCommand : Command
    {
        public sealed class Globals
        {
#pragma warning disable IDE1006 // Naming Styles
            public AugmentrexContext ctx { get; }

            public Process host { get; }

            public Process game { get; }

            public MemoryWindow mem { get; }

            public Configuration cfg { get; }

            public HotKeyRegistrar keys { get; }
#pragma warning restore IDE1006 // Naming Styles

            internal Globals(AugmentrexContext context)
            {
                ctx = context;
                host = context.Host;
                game = context.Game;
                mem = context.Memory;
                cfg = context.Configuration;
                keys = context.HotKeys;
            }
        }

        sealed class EvaluateOptions
        {
            [Option('c')]
            public bool Clear { get; set; }

            [Value(0)]
            public IEnumerable<string> Fragments { get; set; }
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "evaluate", "print" };

        public override string Description =>
            "Evaluates a given C# expression.";

        public override string Syntax =>
            "[-c] [expr]";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If '-c' is given, the REPL session will be reset.",
                "The global variables " +
                string.Join(", ", typeof(Globals).GetTypeInfo().DeclaredProperties.Select(x => $"'{x.Name}'")) +
                " are implicitly defined for convenience.",
            };

        Script _script;

        internal EvaluateCommand()
        {
        }

        public override int? Run(AugmentrexContext context, string[] args)
        {
            var opts = Parse<EvaluateOptions>(context, args);

            if (opts == null)
                return null;

            if (_script == null || opts.Clear)
                _script = CSharpScript.Create(string.Empty,
                    ScriptOptions.Default.WithFilePath("<hgl>")
                        .WithImports(ScriptOptions.Default.Imports.AddRange(
                            Assembly.GetExecutingAssembly().DefinedTypes.Select(x => x.Namespace).Where(x => x != null).Distinct()))
                        .WithLanguageVersion(LanguageVersion.CSharp8)
                        .WithReferences(Assembly.GetExecutingAssembly()),
                    typeof(Globals));

            var code = string.Join(" ", opts.Fragments);

            if (string.IsNullOrWhiteSpace(code))
                return null;

            Script script = _script.ContinueWith(code);

            Task<ScriptState> task;

            try
            {
                task = script.RunAsync(new Globals(context));
            }
            catch (CompilationErrorException ex)
            {
                foreach (var diag in ex.Diagnostics)
                    context.ErrorLine("{0}", diag.ToString());

                return null;
            }

            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                context.ErrorLine("{0}", ex.InnerException);

                return null;
            }

            _script = script;

            var result = task.Result.ReturnValue;

            context.InfoLine("{0} it = {1}", result?.GetType() ?? typeof(object), result ?? "null");

            return null;
        }
    }
}
