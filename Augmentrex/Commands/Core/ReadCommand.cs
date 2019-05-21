using Augmentrex.Memory;
using CommandLine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Augmentrex.Commands.Core
{
    sealed class ReadCommand : Command
    {
        enum ReadType : byte
        {
            I8,
            U8,
            I16,
            U16,
            I32,
            U32,
            I64,
            U64,
            F32,
            F64,
            Ptr,
        }

        sealed class ReadOptions
        {
            [Option('a')]
            public bool Absolute { get; set; }

            [Option('t', Default = ReadType.I32)]
            public ReadType Type { get; set; }

            [Value(0, Required = true)]
            public string Offset { get; set; }
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "read" };

        public override string Description =>
            "Reads data from a given memory offset based on the main module.";

        public override string Syntax =>
            "[-a] [-t <type>] <offset>";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If '-a' is given, 'offset' is interpreted as an absolute address instead.",
                "If '-t' is given, the value will be read as the given 'type', which must be one of " +
                string.Join(", ", typeof(ReadType).GetEnumNames().Select(x => $"'{x.ToLower()}'")) +
                " (defaults to 'i32').",
            };

        public unsafe override int? Run(AugmentrexContext context, string[] args)
        {
            var opts = Parse<ReadOptions>(context, args);

            if (opts == null)
                return null;

            var parsed = (int)TypeDescriptor.GetConverter(typeof(int)).ConvertFromString(opts.Offset);
            var memory = context.Memory;
            var offset = opts.Absolute ? memory.ToOffset((MemoryAddress)parsed) : (MemoryOffset)parsed;

            switch (opts.Type)
            {
                case ReadType.I8:
                    var i8 = memory.Read<sbyte>(offset);
                    context.InfoLine("{0} (0x{0:X})", i8);
                    break;
                case ReadType.U8:
                    var u8 = memory.Read<byte>(offset);
                    context.InfoLine("{0} (0x{0:X})", u8);
                    break;
                case ReadType.I16:
                    var i16 = memory.Read<short>(offset);
                    context.InfoLine("{0} (0x{0:X})", i16);
                    break;
                case ReadType.U16:
                    var u16 = memory.Read<ushort>(offset);
                    context.InfoLine("{0} (0x{0:X})", u16);
                    break;
                case ReadType.I32:
                    var i32 = memory.Read<int>(offset);
                    context.InfoLine("{0} (0x{0:X})", i32);
                    break;
                case ReadType.U32:
                    var u32 = memory.Read<uint>(offset);
                    context.InfoLine("{0} (0x{0:X})", u32);
                    break;
                case ReadType.I64:
                    var i64 = memory.Read<long>(offset);
                    context.InfoLine("{0} (0x{0:X})", i64);
                    break;
                case ReadType.U64:
                    var u64 = memory.Read<ulong>(offset);
                    context.InfoLine("{0} (0x{0:X})", u64);
                    break;
                case ReadType.F32:
                    var f32 = memory.Read<float>(offset);
                    context.InfoLine("{0} (0x{0:X})", f32, Unsafe.Read<uint>(&f32));
                    break;
                case ReadType.F64:
                    var f64 = memory.Read<double>(offset);
                    context.InfoLine("{0} (0x{0:X})", f64, Unsafe.Read<uint>(&f64));
                    break;
                case ReadType.Ptr:
                    var ptr = memory.ReadOffset(offset);
                    context.InfoLine("{0}{1}={2}", memory.Address, ptr, memory.Address + ptr);
                    break;
            }

            return null;
        }
    }
}
