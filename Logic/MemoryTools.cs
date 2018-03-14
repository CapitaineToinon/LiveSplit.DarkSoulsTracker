using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Livesplit.DarkSouls100Tracker.Logic
{
    public class MemoryTools
    {
        private string WindowName;
        private IntPtr EventFlagValueMemory;
        public ExeTypes ExeType { get; set; }
        private Process DARKSOULS { get; set; }
        private IntPtr HANDLE { get; set; }

        public bool IsAlive
        {
            get
            {
                return (GetProcess(WindowName) != null);
            }
        }

        public MemoryTools(string ProcessName)
        {
            this.WindowName = ProcessName;
            this.DARKSOULS = null;
            this.ExeType = ExeTypes.Unknown;
            this.EventFlagValueMemory = IntPtr.Zero;
        }

        public bool Hook()
        {
            if (Attach(WindowName))
            {
                ExeTypes t;
                UInt32 versionFlag = RUInt32(Dictionaries.GameVersion);
                if (versionFlag == Dictionaries.Release)
                    t = ExeTypes.Release;
                else if (versionFlag == Dictionaries.Debug)
                    t = ExeTypes.Debug;
                else
                    t = ExeTypes.Unknown;

                if (t == ExeTypes.Unknown)
                {
                    // Wrong exe type
                    UnHook();
                    return false;
                }
                else
                {
                    // Hooked !
                    ExeType = t;
                    SetDarkSoulsThreadSuspend(true);
                    EventFlagValueMemory = CreateReadingFlagMemory();
                    SetDarkSoulsThreadSuspend(false);
                    return true;
                }
            }
            else
            {
                // Game not found
                UnHook();
                return false;
            }
        }

        public bool UnHook()
        {
            bool unhooked = false;
            // Unhooks the game
            if (DARKSOULS != null)
            {
                if (HANDLE != IntPtr.Zero)
                {
                    DARKSOULS.Dispose();
                    DARKSOULS = null;

                    try
                    {
                        Kernel.CloseHandle(HANDLE);
                        // Succesfuly detached
                        unhooked = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Warning: {0} error " + Environment.NewLine, ex.Message));
                        // Couldn't detach
                        unhooked = false;
                    }
                }
                else
                {
                    // Game is already detached
                    unhooked = true;
                }
            }

            ClearMemory();
            return unhooked;
        }

        public void ClearMemory()
        {
            // Frees the memory
            if (EventFlagValueMemory != IntPtr.Zero)
            {
                Kernel.VirtualFreeEx(HANDLE, EventFlagValueMemory, 0, Constants.MEM_RELEASE);
                EventFlagValueMemory = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Finds a process by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Process GetProcess(string name)
        {
            Process[] _allProcesses = Process.GetProcesses();
            Process selectedProcess = null;
            foreach (Process pp in _allProcesses)
            {
                if (pp.MainWindowTitle.ToLower() == name.ToLower())
                    selectedProcess = pp;
                else
                    pp.Dispose();
            }

            return selectedProcess;
        }

        /// <summary>
        /// Attachs DARKSOULS process
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool Attach(string name)
        {
            UnHook();
            DARKSOULS = GetProcess(WindowName);
            if (DARKSOULS != null)
            {
                HANDLE = Kernel.OpenProcess(Constants.PROCESS_ALL_ACCESS, false, DARKSOULS.Id);
                if (HANDLE != IntPtr.Zero)
                {
                    return true;
                }
                else
                {
                    DARKSOULS = null;
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Function that actually reads and returns the value of a Flag
        /// </summary>
        /// <param name="FlagMemory"></param>
        /// <param name="eventID"></param>
        /// <returns></returns>
        public bool GetEventFlagState(int eventID)
        {
            // Gets the Flag ID and write it to the memory we created
            // with our own assembly code
            WInt32(EventFlagValueMemory + 0x400, eventID);
            int target = (int)HANDLE;
            int getFlag = (int)EventFlagValueMemory;
            int dummy = 0;

            // Starts the thread and wait for it to be over
            IntPtr newThreadHook = (IntPtr)Kernel.CreateRemoteThread(target, 0, 0, getFlag, 0, 0, ref dummy);
            Kernel.WaitForSingleObject(newThreadHook, 0xFFFFFFFFU);
            Kernel.CloseHandle(newThreadHook);

            // Get the value returned by our assembly code and check (by diving it by 128)
            // if the 7th bit is set, which means the flag is set. Because reasons xD
            int a = RInt32(EventFlagValueMemory + 0x404);
            double b = Math.Pow(2.0, 7.0);
            decimal result = Math.Floor((decimal)a / (decimal)(b));
            return (result == 1);
        }

        /// <summary>
        /// This is where the magic happens
        /// Wulf you're a god, I think 🤔
        /// </summary>
        private IntPtr CreateReadingFlagMemory()
        {
            IntPtr EventFlagValue = Dictionaries.EventFlagValues[ExeType];

            IntPtr getflagfuncmem = Kernel.VirtualAllocEx(
                HANDLE, 
                (IntPtr)0, 
                (IntPtr)0x8000, 
                Constants.MEM_COMMIT, 
                Constants.PAGE_EXECUTE_READWRITE
            );

            Kernel.VirtualProtectEx(
                HANDLE, 
                getflagfuncmem, 
                (UIntPtr)0x8000, 
                Constants.PAGE_EXECUTE_READWRITE, 
                out uint oldProtectionOutFunc
            );

            // Using DarkSoulsFlagsInjector.dll, the VB code memes by Wulf :thinking:
            // Basically creates a memory with some of our assembly code in it
            // Put the ID of the Flag you wanna test in getflagfuncmem + 0x400
            // And the result of that will be in getflagfuncmem + 0x404
            // Then checks if the 7th bit is set to know if flag is set
            AssemblyGenerator a = new AssemblyGenerator();

            a.AddVar("newmem", getflagfuncmem);
            a.AddVar("vardump", getflagfuncmem + 0x400);

            a.pos = (int)(getflagfuncmem);
            a.Asm("pushad");
            a.Asm("mov eax, vardump");
            a.Asm("mov eax, [eax]");
            a.Asm("push eax");
            a.Asm("call 0x" + EventFlagValue.ToString("X"));
            a.Asm("mov ecx, vardump");
            a.Asm("add ecx, 4");
            a.Asm("mov [ecx], eax");
            a.Asm("add ecx, 4");
            a.Asm("mov eax, 1");
            a.Asm("mov [ecx], eax");
            a.Asm("popad");
            a.Asm("ret");

            WriteCodeAndFlushCache(HANDLE, getflagfuncmem, a.Bytes, a.Bytes.Length, 0);

            return getflagfuncmem;
        }

        private bool WriteCodeAndFlushCache(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten)
        {
            bool result = Kernel.WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, ref lpNumberOfBytesWritten);
            if (!Kernel.FlushInstructionCache(hProcess, lpBaseAddress, (UIntPtr)iSize))
            {
                throw new Exception("Flush Instruction Cache Failed");
            }
            return result;
        }

        /// <summary>
        /// Suspends DARKSOULS 
        /// </summary>
        /// <param name="suspend"></param>
        public void SetDarkSoulsThreadSuspend(bool suspend)
        {
            if (DARKSOULS == null)
            {
                return;
            }

            foreach (ProcessThread pthread in DARKSOULS.Threads)
            {
                IntPtr pOpenThread = Kernel.OpenThread(0x2, false, (UInt32)pthread.Id);
                if (pOpenThread != IntPtr.Zero)
                {
                    if (suspend)
                    {
                        Kernel.SuspendThread(pOpenThread);
                    }
                    else
                    {
                        UInt32 suspendCount = 0;
                        do
                        {
                            suspendCount = Kernel.ResumeThread(pOpenThread);
                        } while (suspendCount > 0);
                    }

                    Kernel.CloseHandle(pOpenThread);
                }
            }
        }

        #region Functions to read and write to the Process Memory
        public byte[] RBytes(IntPtr addr, Int32 size)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, size, ref bytesRead);
            return _rtnBytes;
        }

        public UInt32 RUInt32(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt32(_rtnBytes, 0);
        }

        public Int32 RInt32(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(HANDLE, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt32(_rtnBytes, 0);
        }

        public void WUInt32(IntPtr addr, UInt32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(HANDLE, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public void WInt32(IntPtr addr, Int32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(HANDLE, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }
        #endregion
    }
}
