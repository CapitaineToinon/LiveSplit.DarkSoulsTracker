using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace CapitaineToinon.DarkSoulsMemory
{
    internal static class Kernel
    {
        #region IntPtr
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr handle,UInt32 milliseconds);
        #endregion

        #region IntPtr
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(IntPtr dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, int flAllocationType, int flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int CreateRemoteThread(IntPtr hProcess, int lpThreadAttributes, int dwStackSize, int lpStartAddress, int lpParameter, int dwCreationFlags, int lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(IntPtr dwDesiredAccess,bool bInheritHandle,UInt32 dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern UInt32 SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern UInt32 ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern bool FlushInstructionCache(IntPtr hProcess,IntPtr lpBaseAddress,UIntPtr dwSize);
        #endregion
    }
}
