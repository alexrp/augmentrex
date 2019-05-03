using System;
using System.Runtime.InteropServices;

namespace Augmentrex.Memory
{
    static class MemoryWin32
    {
        [DllImport("kernel32")]
        public static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);
    }
}
