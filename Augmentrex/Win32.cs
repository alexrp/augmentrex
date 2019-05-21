using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Augmentrex
{
    public static class Win32
    {
        public static string GetLastError()
        {
            return new Win32Exception(Marshal.GetLastWin32Error()).Message;
        }
    }
}
