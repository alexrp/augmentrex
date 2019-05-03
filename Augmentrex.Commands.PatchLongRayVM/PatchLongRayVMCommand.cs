using Augmentrex.Memory;
using System.Collections.Generic;
using System.Reflection;

namespace Augmentrex.Commands.PatchLongRayVM
{
    public sealed class PatchLongRayVMCommand : Command
    {
        public override IReadOnlyList<string> Names { get; } =
            new[] { Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title };

        public override string Description { get; } =
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>().Description;

        public override IReadOnlyList<string> Details =>
            new[]
            {
                "Run the command again to undo the patch.",
            };

        static readonly byte?[] _pattern = new byte?[]
        {
            0x55,                                     // push ebp
            0x8b, 0xec,                               // mov ebp, esp
            0x83, 0xe4, 0xf0,                         // and esp, 0FFFFFFF0h
            0x6a, 0xff,                               // push 0FFFFFFFFh
            0x68, null, null, null, null,             // push <offset>
            0x64, 0xa1, 0x00, 0x00, 0x00, 0x00,       // mov eax, large fs:0
            0x50,                                     // push eax
            0x81, 0xec, 0xe8, 0x04, 0x00, 0x00,       // sub esp, 4E8h
            0xa1, null, null, null, null,             // mov eax, <global>
            0x33, 0xc4,                               // xor eax, esp
            0x89, 0x84, 0x24, 0xe0, 0x04, 0x00, 0x00, // mov [esp + 4F4h + var_14], eax
            0x53,                                     // push ebx
            0x56,                                     // push esi
            0x57,                                     // push edi
            0xa1, null, null, null, null,             // mov eax, <global>
            0x33, 0xc4,                               // xor eax, esp
            0x50,                                     // push eax
            0x8d, 0x84, 0x24, 0xf8, 0x04, 0x00, 0x00, // lea eax, [esp + 504h + var_C]
            0x64, 0xa3, 0x00, 0x00, 0x00, 0x00,       // mov large fs:0, eax
            0x80, 0x3d, null, null, null, null, 0x00, // cmp _HK_flyingcolors_mopp, 0

        };

        MemoryOffset? _offset;

        uint? _backup;

        public override int? Run(CommandContext context, string[] args)
        {
            context.Info("Locating function 'hkpMoppLongRayVirtualMachine::queryRayOnTree()'...");

            var memory = context.Memory;

            if (_offset == null)
            {
                var results = memory.Search(_pattern);

                if (results.Length == 0)
                {
                    context.Error("Could not locate function. Was the game updated?");

                    return null;
                }

                foreach (var offs in results)
                    context.Info("Located at: {0}{1}={2}", memory.Address, offs, memory.Address + offs);

                if (results.Length != 1)
                {
                    context.Error("Found multiple matches. Was the game updated?");

                    return null;
                }

                _offset = results[0];
            }

            var offset = (MemoryOffset)_offset;

            if (_backup is uint b)
            {
                context.Info("Patch already applied; reverting...");

                memory.Write(offset, b);
                _backup = null;
            }
            else
            {
                _backup = memory.Read<uint>(offset);

                memory.WriteBytes(offset, new byte[] { 0xc2, 0x0c, 0x00 }); // retn 0Ch
                memory.Write<byte>(offset + 3, 0x90);                       // nop
            }

            context.Info("Successfully patched.");

            return null;
        }
    }
}
