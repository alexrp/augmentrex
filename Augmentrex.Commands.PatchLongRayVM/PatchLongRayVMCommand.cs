using Augmentrex.Memory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

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

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        delegate void QueryRayOnTreeFunc(IntPtr self, IntPtr qint, IntPtr code, IntPtr qfloat);

        FunctionHook<QueryRayOnTreeFunc> _hook;

        public override int? Run(AugmentrexContext context, string[] args)
        {
            if (_hook == null)
            {
                static void QueryRayOnTreeHook(IntPtr self, IntPtr qint, IntPtr code, IntPtr qfloat)
                {
                }

                context.Info("Searching for function... ");

                _hook = context.CreateHook<QueryRayOnTreeFunc>(new byte?[]
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
                    0x89, 0x84, 0x24, 0xe0, 0x04, 0x00, 0x00, // mov [esp+4F4h+var_14], eax
                    0x53,                                     // push ebx
                    0x56,                                     // push esi
                    0x57,                                     // push edi
                    0xa1, null, null, null, null,             // mov eax, <global>
                    0x33, 0xc4,                               // xor eax, esp
                    0x50,                                     // push eax
                    0x8d, 0x84, 0x24, 0xf8, 0x04, 0x00, 0x00, // lea eax, [esp+504h+var_C]
                    0x64, 0xa3, 0x00, 0x00, 0x00, 0x00,       // mov large fs:0, eax
                    0x80, 0x3d, null, null, null, null, 0x00, // cmp _HK_flyingcolors_mopp, 0
                }, "hkMoppLongRayVirtualMachine::queryRayOnTree", QueryRayOnTreeHook);

                if (_hook == null)
                {
                    context.ErrorLine("No matches. Was the game updated?");

                    return null;
                }
                else
                {
                    var memory = context.Memory;

                    context.SuccessLine("{0} at {1}{2}={3}.", _hook.Name, memory.Address, memory.ToOffset(_hook.Address), _hook.Address);
                }
            }

            _hook.Toggle();

            context.InfoLine("Patch {0}installed successfully.", !_hook.IsInstalled ? "un" : string.Empty);

            return null;
        }
    }
}
