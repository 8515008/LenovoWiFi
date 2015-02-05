using System;
using System.Runtime.InteropServices;

namespace Lenovo.WiFi.Client.Native
{
    internal class NativeMethods
    {
        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern ModuleSafeHandle LoadLibrary(string lpFileName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(ModuleSafeHandle hModule, string lpProcName);

        [DllImport("Kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);
    }
}
