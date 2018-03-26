using Microsoft.Win32.SafeHandles;
using System;

namespace CapitaineToinon.DarkSoulsMemory
{
    internal static class MemoryTools
    {
        // READ
        public static byte[] RBytes(SafeProcessHandle HANDLE, IntPtr addr, Int32 size)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, size, ref bytesRead);
            return _rtnBytes;
        }

        public static UInt32 RUInt32(SafeProcessHandle HANDLE, IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt32(_rtnBytes, 0);
        }

        public static Int32 RInt32(SafeProcessHandle HANDLE, IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt32(_rtnBytes, 0);
        }

        // WRITE
        public static void WUInt32(SafeProcessHandle HANDLE, IntPtr addr, UInt32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(HANDLE, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public static void WInt32(SafeProcessHandle HANDLE, IntPtr addr, Int32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(HANDLE, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }
    }
}
