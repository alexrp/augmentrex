using Augmentrex.Memory;
using CommandLine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Augmentrex.Commands.Core
{
    sealed class WriteCommand : Command
    {
        enum WriteType : byte
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

        sealed class WriteOptions
        {
            [Option('a')]
            public bool Absolute { get; set; }

            [Option('t', Default = WriteType.I32)]
            public WriteType Type { get; set; }

            [Value(0, Required = true)]
            public string Offset { get; set; }

            [Value(1, Required = true)]
            public string Value { get; set; }
        }

        public override IReadOnlyList<string> Names { get; } =
            new[] { "write" };

        public override string Description =>
            "Writes data to a given memory offset based on the main module.";

        public override string Syntax =>
            "[-a] [-t <type>] <offset> <value>";

        public override IReadOnlyList<string> Details { get; } =
            new[]
            {
                "If '-a' is given, 'offset' is interpreted as an absolute address instead.",
                "If '-t' is given, the value will be written as the given 'type', which must be one of " +
                string.Join(", ", typeof(WriteType).GetEnumNames().Select(x => $"'{x.ToLower()}'")) +
                " (defaults to 'i32').",
            };

        public override int? Run(AugmentrexContext context, string[] args)
        {
            var opts = Parse<WriteOptions>(context, args);

            if (opts == null)
                return null;

            var parsed = (int)TypeDescriptor.GetConverter(typeof(int)).ConvertFromString(opts.Offset);

            long ivalue = 0;
            ulong uvalue = 0;
            var fvalue = 0.0f;
            var dvalue = 0.0d;
            var ovalue = default(MemoryOffset);

            switch (opts.Type)
            {
                case WriteType.I8:
                case WriteType.I16:
                case WriteType.I32:
                case WriteType.I64:
                    ivalue = (long)TypeDescriptor.GetConverter(typeof(long)).ConvertFromString(opts.Value);
                    break;
                case WriteType.U8:
                case WriteType.U16:
                case WriteType.U32:
                case WriteType.U64:
                    uvalue = (ulong)TypeDescriptor.GetConverter(typeof(ulong)).ConvertFromString(opts.Value);
                    break;
                case WriteType.F32:
                    fvalue = (float)TypeDescriptor.GetConverter(typeof(float)).ConvertFromString(opts.Value);
                    break;
                case WriteType.F64:
                    dvalue = (double)TypeDescriptor.GetConverter(typeof(double)).ConvertFromString(opts.Value);
                    break;
                case WriteType.Ptr:
                    ovalue = (MemoryOffset)(int)TypeDescriptor.GetConverter(typeof(int)).ConvertFromString(opts.Value);
                    break;
            }

            var memory = context.Memory;
            var offset = opts.Absolute ? memory.ToOffset((MemoryAddress)parsed) : (MemoryOffset)parsed;

            switch (opts.Type)
            {
                case WriteType.I8:
                    memory.Write(offset, (sbyte)ivalue);
                    break;
                case WriteType.U8:
                    memory.Write(offset, (byte)uvalue);
                    break;
                case WriteType.I16:
                    memory.Write(offset, (short)ivalue);
                    break;
                case WriteType.U16:
                    memory.Write(offset, (ushort)uvalue);
                    break;
                case WriteType.I32:
                    memory.Write(offset, (int)ivalue);
                    break;
                case WriteType.U32:
                    memory.Write(offset, (uint)uvalue);
                    break;
                case WriteType.I64:
                    memory.Write(offset, ivalue);
                    break;
                case WriteType.U64:
                    memory.Write(offset, uvalue);
                    break;
                case WriteType.F32:
                    memory.Write(offset, fvalue);
                    break;
                case WriteType.F64:
                    memory.Write(offset, dvalue);
                    break;
                case WriteType.Ptr:
                    memory.WriteOffset(offset, ovalue);
                    break;
            }

            return null;
        }
    }
}
