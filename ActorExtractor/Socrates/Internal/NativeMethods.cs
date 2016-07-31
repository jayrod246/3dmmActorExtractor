using System;
using System.Runtime.InteropServices;


namespace Socrates.Internal
{
    internal class NativeMethods
    {
        internal const uint PAGE_EXECUTE_READWRITE = 0x40;
        internal const uint MEM_COMMIT = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize, uint flAllocationType, uint flProtect);

        internal static T GetDelegateFromBytes<T>(byte[] bytes) where T : class
        {
            var buf = VirtualAlloc(IntPtr.Zero, (UIntPtr)bytes.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            Marshal.Copy(bytes, 0, buf, bytes.Length);

            return Marshal.GetDelegateForFunctionPointer(buf, typeof(T)) as T;
        }
    }
}
