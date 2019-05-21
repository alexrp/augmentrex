using Augmentrex.Memory;
using CommandLine;
using SharpDisasm;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Augmentrex.Commands.Core
{
    sealed class DisassembleCommand : Command
    {
        sealed class DisassembleOptions
        {
            [Option('a')]
            public bool Absolute { get; set; }

            [Value(0, Required = true)]
            public string Offset { get; set; }

            [Value(1, Required = true)]
            public uint Length { get; set; }
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "disassemble", "disasm" };

        public override string Description =>
            "Disassembles instructions from a given memory range based on the main module.";

        public override string Syntax =>
            "[-a] <offset> <length>";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If '-a' is given, 'offset' is interpreted as an absolute address instead.",
            };

        public override int? Run(AugmentrexContext context, string[] args)
        {
            var opts = Parse<DisassembleOptions>(context, args);

            if (opts == null)
                return null;

            var parsed = (int)TypeDescriptor.GetConverter(typeof(int)).ConvertFromString(opts.Offset);
            var address = opts.Absolute ? (MemoryAddress)parsed : context.Memory.ToAddress((MemoryOffset)parsed);

            using var disasm = new Disassembler(address, (int)opts.Length, ArchitectureMode.x86_32, (uint)address, true);
            var insns = new List<Instruction>();
            Instruction insn;
            string error = null;

            while ((insn = disasm.NextInstruction()) != null)
            {
                if (insn.Error)
                {
                    error = insn.ErrorMessage;
                    break;
                }

                insns.Add(insn);
            }

            var length = insns.Max(x => x.Bytes.Length);

            foreach (var i in insns)
                i.PrintColored(context, length);

            if (error != null)
            {
                if (insns.Count != 0)
                    context.Line();

                context.WarningLine("Could not disassemble further: {0}", error);
            }

            return null;
        }
    }
}
