using System;
using System.Diagnostics;

namespace LiveSplit.DarkSoulsTracker
{
    internal class Memory
    {
        Process process;

        public Memory(Process process)
        {
            this.process = process;
        }

        // READ normal process.Handle
        public UInt32 RUInt32(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(process.Handle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt32(_rtnBytes, 0);
        }

        // READ
        public byte[] RBytes(IntPtr addr, Int32 size)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(process.Handle, addr, _rtnBytes, size, ref bytesRead);
            return _rtnBytes;
        }

        public Int32 RInt32(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(process.Handle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt32(_rtnBytes, 0);
        }

        // WRITE
        public void WUInt32(IntPtr addr, UInt32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(process.Handle, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public void WInt32(IntPtr addr, Int32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(process.Handle, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }
    }
}
