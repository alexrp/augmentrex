using Augmentrex.Memory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Augmentrex.Commands.PatchCCAgent
{
    public sealed class PatchCCAgentCommand : Command
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
        delegate void ProcessCollisionFunc(IntPtr self, IntPtr body1, IntPtr body2, IntPtr input, IntPtr output);

        FunctionHook<ProcessCollisionFunc> _hook;

        public override int? Run(AugmentrexContext context, string[] args)
        {
            if (_hook == null)
            {
                static void ProcessCollisionHook(IntPtr self, IntPtr body1, IntPtr body2, IntPtr input, IntPtr output)
                {
                }

                context.Info("Searching for function... ");

                _hook = context.CreateHook<ProcessCollisionFunc>(new byte?[]
                {
                    0x55,                               // push ebp
                    0x8b, 0xec,                         // mov ebp, esp
                    0x83, 0xe4, 0xf0,                   // and esp, 0FFFFFFF0h
                    0xa1, null, null, null, null,       // mov eax, <global>
                    0x81, 0xec, 0xf4, 0x00, 0x00, 0x00, // sub esp, 0F4h
                    0x53,                               // push ebx
                    0x56,                               // push esi
                    0x57,                               // push edi
                    0x50,                               // push eax
                    0x8b, 0xd9,                         // mov ebx, ecx
                    0xff, 0x15, null, null, null, null, // call ds:TlsGetValue
                    0x8b, 0xc8,                         // mov ecx, eax
                    0x8b, 0x71, 0x04,                   // mov esi, [ecx+4]
                    0x3b, 0x71, 0x0c,                   // cmp esi, [ecx+0Ch]
                    0x73, null,                         // jnb <offset>
                    0xc7, 0x06, null, null, null, null, // mov dword ptr [esi], "TtCapsCaps"
                    0x0f, 0x31,                         // rdtsc
                    0x89, 0x44, 0x24, 0x14,             // mov [esp+100h+var_EC], eax
                    0x8b, 0x54, 0x24, 0x14,             // mov edx, [esp+100h+var_EC]
                    0x89, 0x56, 0x04,                   // mov [esi+4], edx
                }, "hkCapsuleCapsuleAgent::processCollision", ProcessCollisionHook);

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
