using ExposedObject;
using SharpDisasm;
using SharpDisasm.Translators;
using SharpDisasm.Udis86;
using System;
using System.Linq;

namespace Augmentrex.Memory
{
    static class InstructionExtensions
    {
        sealed class HackyTranslator : Translator
        {
            protected override void TranslateInstruction(Instruction insn)
            {
            }

            public string GetRegisterName(ud_type type)
            {
                return RegisterForType(type);
            }

            public ulong GetRelativeJumpTarget(Instruction instruction, Operand operand)
            {
                return ud_syn_rel_target(instruction, operand);
            }
        }

        public static void PrintColored(this Instruction instruction, AugmentrexContext context, int length)
        {
            const ConsoleColor TriviaColor = ConsoleColor.DarkGray;
            const ConsoleColor AddressColor = ConsoleColor.White;
            const ConsoleColor BinaryColor = ConsoleColor.DarkCyan;
            const ConsoleColor MnemonicColor = ConsoleColor.DarkYellow;
            const ConsoleColor CastColor = ConsoleColor.DarkMagenta;
            const ConsoleColor RegisterColor = ConsoleColor.DarkGreen;
            const ConsoleColor LiteralColor = ConsoleColor.DarkRed;

            context.Color(AddressColor, "{0:X8} ", instruction.Offset);
            context.Color(BinaryColor, $"{{0,-{length * 2 + length - 1}}} ",
                string.Join(" ", instruction.Bytes.Select(x => x.ToString("X2"))));

            var dynIns = Exposed.From(instruction);

            string GetCast(Operand operand)
            {
                var value = dynIns.br_far != 0 ? "far " : string.Empty;

                switch (operand.Size)
                {
                    case 8:
                        value += "byte ";
                        break;
                    case 16:
                        value += "word ";
                        break;
                    case 32:
                        value += "dword ";
                        break;
                    case 64:
                        value += "qword ";
                        break;
                    case 80:
                        value += "tword ";
                        break;
                    case 128:
                        value += "oword ";
                        break;
                    case 256:
                        value += "yword ";
                        break;
                    case 512:
                        value += "zword ";
                        break;
                }

                return value;
            }

            var trans = new HackyTranslator();

            void HandleOperand(Operand operand, int index)
            {
                var cast = false;

                switch (index)
                {
                    case 0:
                        if (operand.Type == ud_type.UD_OP_MEM)
                        {
                            var next = instruction.Operands.Skip(1).FirstOrDefault();

                            if (next != null && next.Type != ud_type.UD_OP_IMM &&
                                next.Type != ud_type.UD_OP_CONST && next.Size == operand.Size)
                            {
                                if (next.Type == ud_type.UD_OP_REG && next.Base == ud_type.UD_R_CL)
                                {
                                    switch (instruction.Mnemonic)
                                    {
                                        case ud_mnemonic_code.UD_Ircl:
                                        case ud_mnemonic_code.UD_Irol:
                                        case ud_mnemonic_code.UD_Iror:
                                        case ud_mnemonic_code.UD_Ircr:
                                        case ud_mnemonic_code.UD_Ishl:
                                        case ud_mnemonic_code.UD_Ishr:
                                        case ud_mnemonic_code.UD_Isar:
                                            cast = true;
                                            break;
                                    }
                                }
                            }
                            else
                                cast = true;
                        }
                        break;
                    case 1:
                        if (operand.Type == ud_type.UD_OP_MEM &&
                            operand.Size != instruction.Operands[0].Size)
                            cast = true;
                        break;
                    case 2:
                        if (operand.Type == ud_type.UD_OP_MEM &&
                            operand.Size != instruction.Operands[1].Size)
                            cast = true;
                        break;
                    case 3:
                        break;
                }

                switch (operand.Type)
                {
                    case ud_type.UD_OP_REG:
                        context.Color(RegisterColor, trans.GetRegisterName(operand.Base));
                        break;
                    case ud_type.UD_OP_MEM:
                        if (cast)
                            context.Color(CastColor, GetCast(operand));

                        context.Color(TriviaColor, "[");

                        if (dynIns.pfx_seg != 0)
                        {
                            context.Color(RegisterColor, trans.GetRegisterName((ud_type)dynIns.pfx_seg));
                            context.Color(TriviaColor, ":");
                        }

                        if (operand.Base != ud_type.UD_NONE)
                            context.Color(RegisterColor, trans.GetRegisterName(operand.Base));

                        if (operand.Index != ud_type.UD_NONE)
                        {
                            if (operand.Base != ud_type.UD_NONE)
                                context.Color(TriviaColor, "+");

                            context.Color(RegisterColor, trans.GetRegisterName(operand.Index));

                            if (operand.Scale != 0)
                            {
                                context.Color(TriviaColor, "*");
                                context.Color(RegisterColor, "{0}", operand.Scale);
                            }
                        }

                        if (operand.Offset != 0)
                        {
                            if (operand.Base != ud_type.UD_NONE || operand.Index != ud_type.UD_NONE)
                            {
                                var value = operand.LvalSQWord;

                                if (value > 0)
                                    context.Color(TriviaColor, "+");
                                else
                                    context.Color(TriviaColor, "-");

                                context.Color(LiteralColor, "0x{0:X}", value);
                            }
                            else
                                context.Color(LiteralColor, "0x{0:X}", operand.LvalUQWord);
                        }

                        context.Color(TriviaColor, "]");
                        break;
                    case ud_type.UD_OP_PTR:
                        if (operand.Size == 32)
                        {
                            context.Color(CastColor, "word ");
                            context.Color(LiteralColor, "0x{0:X}:0x{1:X}", operand.PtrSegment, operand.PtrOffset & 0xffff);
                        }
                        else if (operand.Size == 48)
                        {
                            context.Color(CastColor, "dword ");
                            context.Color(LiteralColor, "0x{0:X}:0x{1:X}", operand.PtrSegment, operand.PtrOffset);
                        }
                        break;
                    case ud_type.UD_OP_IMM:
                        var imm = operand.LvalUQWord;

                        if (operand.Opcode == ud_operand_code.OP_sI &&
                            operand.Size != dynIns.opr_mode &&
                            dynIns.opr_mode < 64)
                            imm &= (1ul << dynIns.opr_mode) - 1;

                        context.Color(LiteralColor, "0x{0:X}", imm);
                        break;
                    case ud_type.UD_OP_JIMM:
                        context.Color(LiteralColor, "0x{0:X}", trans.GetRelativeJumpTarget(instruction, operand));
                        break;
                    case ud_type.UD_OP_CONST:
                        if (cast)
                            context.Color(CastColor, GetCast(operand));

                        context.Color(LiteralColor, "{0}", operand.LvalUDWord);
                        break;
                }
            }

            var dynBitOps = Exposed.From(typeof(Disassembler).Assembly.GetType("SharpDisasm.Udis86.BitOps"));

            if (dynBitOps.P_OSO((uint)dynIns.itab_entry.Prefix) == 0 && dynIns.pfx_opr != 0)
            {
                switch (instruction.dis_mode)
                {
                    case ArchitectureMode.x86_16:
                        context.Color(MnemonicColor, "o32 ");
                        break;
                    case ArchitectureMode.x86_64:
                        context.Color(MnemonicColor, "o16 ");
                        break;
                }
            }

            if (dynBitOps.P_ASO((uint)dynIns.itab_entry.Prefix) == 0 && dynIns.pfx_opr != 0)
            {
                switch (instruction.dis_mode)
                {
                    case ArchitectureMode.x86_16:
                        context.Color(MnemonicColor, "a32 ");
                        break;
                    case ArchitectureMode.x86_32:
                        context.Color(MnemonicColor, "a16 ");
                        break;
                    case ArchitectureMode.x86_64:
                        context.Color(MnemonicColor, "a32 ");
                        break;
                }
            }

            if (dynIns.pfx_seg != 0 && instruction.Operands.Length >= 2 &&
                instruction.Operands[0].Type != ud_type.UD_OP_MEM &&
                instruction.Operands[1].Type != ud_type.UD_OP_MEM)
                context.Color(RegisterColor, "{0} ", trans.GetRegisterName((ud_type)dynIns.pfx_seg));

            if (dynIns.pfx_lock != 0)
                context.Color(MnemonicColor, "lock ");

            if (dynIns.pfx_rep != 0)
                context.Color(MnemonicColor, "rep ");
            else if (dynIns.pfx_repe != 0)
                context.Color(MnemonicColor, "repe ");
            else if (dynIns.pfx_repne != 0)
                context.Color(MnemonicColor, "repne ");

            context.Color(MnemonicColor, "{0}", udis86.ud_lookup_mnemonic(instruction.Mnemonic));

            for (var i = 0; i < instruction.Operands.Length; i++)
            {
                var op = instruction.Operands[i];

                if (i != 0)
                    context.Color(TriviaColor, ",");

                context.Info(" ");

                HandleOperand(op, i);
            }

            context.Line();
        }
    }
}
