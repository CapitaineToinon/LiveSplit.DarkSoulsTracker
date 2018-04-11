using Microsoft.Win32.SafeHandles;
using System;

namespace CapitaineToinon.DarkSoulsMemory
{
    internal static class MemoryTools
    {
        // READ normal handle
        public static UInt32 RUInt32(IntPtr HANDLE, IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt32(_rtnBytes, 0);
        }

        // READ
        public static byte[] RBytes(IntPtr HANDLE, IntPtr addr, Int32 size)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, size, ref bytesRead);
            return _rtnBytes;
        }

        public static Int32 RInt32(IntPtr HANDLE, IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt32(_rtnBytes, 0);
        }

        // WRITE
        public static void WUInt32(IntPtr HANDLE, IntPtr addr, UInt32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(HANDLE, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public static void WInt32(IntPtr HANDLE, IntPtr addr, Int32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(HANDLE, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }
    }
}
