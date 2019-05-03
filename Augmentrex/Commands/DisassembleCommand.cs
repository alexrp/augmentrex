using Augmentrex.Memory;
using CommandLine;
using SharpDisasm;
using SharpDisasm.Translators;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Augmentrex.Commands
{
    public sealed class DisassembleCommand : Command
    {
        enum DisassembleSyntax
        {
            Intel,
            ATT,
        }

        sealed class DisassembleOptions
        {
            [Option('a')]
            public bool Absolute { get; set; }

            [Option('s', Default = DisassembleSyntax.Intel)]
            public DisassembleSyntax Syntax { get; set; }

            [Value(0, Required = true)]
            public string Offset { get; set; }

            [Value(1, Required = true)]
            public uint Length { get; set; }
        }

        static readonly IntelTranslator _intel = new IntelTranslator();

        static readonly ATTTranslator _att = new ATTTranslator();

        public override IReadOnlyList<string> Names { get; } =
            new[] { "disassemble", "disasm" };

        public override string Description =>
            "Disassembles instructions from a given memory range based on the main module.";

        public override string Syntax =>
            "[-a] [-s <syntax>] <offset> <length>";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If '-a' is given, 'offset' is interpreted as an absolute address instead.",
                "If '-s' is given, the disassembly output will use the given syntax mode, which must be one of " +
                string.Join(", ", typeof(DisassembleSyntax).GetEnumNames().Select(x => "'" + x.ToLower() + "'")) +
                " (defaults to 'intel').",
            };

        public override int? Run(CommandContext context, string[] args)
        {
            var opts = Parse<DisassembleOptions>(context, args);

            if (opts == null)
                return null;

            var parsed = (int)TypeDescriptor.GetConverter(typeof(int)).ConvertFromString(opts.Offset);

            switch (opts.Syntax)
            {
                case DisassembleSyntax.Intel:
                    Disassembler.Translator = _intel;
                    break;
                case DisassembleSyntax.ATT:
                    Disassembler.Translator = _att;
                    break;
            }

            var address = opts.Absolute ? (MemoryAddress)parsed : context.Memory.ToAbsolute((MemoryOffset)parsed);

            using (var disasm = new Disassembler(address, (int)opts.Length, ArchitectureMode.x86_32))
            {
                var first = true;

                while (true)
                {
                    var insn = disasm.NextInstruction();

                    if (insn == null || insn.Error)
                        break;

                    if (first)
                    {
                        context.Line();
                        first = false;
                    }

                    context.Info("  {0}", insn.ToString());
                }

                if (!first)
                    context.Line();
            }

            return null;
        }
    }
}
