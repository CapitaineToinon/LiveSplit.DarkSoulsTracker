using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Livesplit.DarkSoulsTracker
{
    public static class Constants
    {
        public const int PROCESS_VM_READ = 0x10;
        public const int TH32CS_SNAPPROCESS = 0x2;
        public const int MEM_COMMIT = 4096;
        public const int MEM_RELEASE = 0x8000;
        public const int PAGE_READWRITE = 4;
        public const int PAGE_EXECUTE_READWRITE = 0x40;
        public const int PROCESS_CREATE_THREAD = (0x2);
        public const int PROCESS_VM_OPERATION = (0x8);
        public const int PROCESS_VM_WRITE = (0x20);
        public const int PROCESS_ALL_ACCESS = 0x1F0FFF;
    }

    class GameMemory
    {
        public Game Game
        {
            get
            {
                return _game;
            }
            private set
            {
                _game = value;
            }
        }
        private Game _game;
        public string exeVER = "";
        IntPtr hook1mem;
        public IntPtr getflagfuncmem;

        Dictionary<string, IntPtr> hooks;
        Dictionary<string, IntPtr> dbgHooks = new Dictionary<string, IntPtr>();
        Dictionary<string, IntPtr> rlsHooks = new Dictionary<string, IntPtr>();

        Process _targetProcess = null;
        public IntPtr _targetProcessHandle = IntPtr.Zero;

        private Thread _thread; // Main thread
        private CancellationTokenSource _cancelSource; // used to cancel the main thread

        public event EventHandler UpdatePercentage; // Even to update the UI
        public event EventHandler UpdateDebug; // Even to update the UI

        public void StartReading()
        {
            _cancelSource = new CancellationTokenSource();
            _thread = new Thread(MaintThread)
            {
                IsBackground = true
            };
            _thread.Start();
        }

        public void Stop()
        {
            if (_thread != null)
            {
                while (_thread.IsAlive)
                {
                    _cancelSource.Cancel();
                }
                Thread unhookThread = new Thread(unhook)
                {
                    IsBackground = true
                };
                unhookThread.Start();
            }
        }

        private void MaintThread()
        {
            bool hooked = false;
            bool needsRehooking = false;
            bool rehookedAndWaiting = false;
            bool completed = false;
            Game = new Game(this);

            // Initialize the hooks for debug and release
            initHooks();

            // Until the thread is canceled...
            while (!_cancelSource.IsCancellationRequested)
            {
                // Unhook the game if rehooking is needed
                if (hooked && needsRehooking)
                {
                    unhook();
                    hooked = needsRehooking = false;
                    rehookedAndWaiting = true;
                }

                // Hooks the game if needed
                if (!hooked)
                {
                    if (ScanForProcess("DARK SOULS", true))
                    {
                        CheckDarkSoulsVersion();
                        if (!(exeVER == "Debug" || exeVER == "Release"))
                        {
                            // exe found, but it's not the correct one for some reason, cancel the thread
                            MessageBox.Show("Dark Souls exe found but invalid EXE type. (Not Release or Debug, maybe using Steam Beta?)");
                            _cancelSource.Cancel();
                            return;
                        }

                        // Set the correct hooks if Release of Debug 
                        hooks = (exeVER == "Release") ? rlsHooks : dbgHooks;

                        // DarkSoulsFlagsInjector shenanigans :thinking:
                        SetDarkSoulsThreadSuspend(true);
                        initFlagHook();
                        initGetFlagFunc();
                        SetDarkSoulsThreadSuspend(false);

                        hooked = true;
                    }
                    else
                    {
                        // exe not found
                        _cancelSource.Cancel();
                        hooked = false;
                        return;
                    }
                }

                if (hooked)
                {
                    try
                    {
                        // Do nothing when at the main menu, if 100% is achieved or NG+ triggered
                        if (Game.GetIngameTimeInMilliseconds() != 0 && !completed)
                        {
                            int CurrentIGT = Game.GetIngameTimeInMilliseconds();
                            Thread.Sleep(50);
                            int NextIGT = Game.GetIngameTimeInMilliseconds();

                            // Everytime the player enters a loadscreen, the hook gets disconnected and reconnected
                            // If IGT returns 0, the player is in the main menu. Disconnecting in the main menu may lead to the game freezing
                            if ((CurrentIGT == NextIGT) && !rehookedAndWaiting)
                            {
                                needsRehooking = true;
                            } 
                            // If the player is in his own world or not at the main menu
                            else if (Game.isPlayerInOwnWorld() && Game.IsPlayerLoaded() && Game.GetIngameTimeInMilliseconds() != 0)
                            {
                                rehookedAndWaiting = true;
                                // Updates all the flags and calls the event to update the UI
                                Game.updateAllEventFlags();
                                this.UpdatePercentage(Game.GetTotalCompletionPercentage, EventArgs.Empty);

                                // Check if 100% achieved or NG finished
                                completed = (Game.GetTotalCompletionPercentage == 100);
                                completed = (Game.GetClearCount() > 0);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong
                        Trace.WriteLine(ex.ToString());
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        public void initFlagHook()
        {
            hook1mem = Kernel.VirtualAllocEx(_targetProcessHandle, (IntPtr)0, (IntPtr)0x8000, Constants.MEM_COMMIT, Constants.PAGE_EXECUTE_READWRITE);
            Kernel.VirtualProtectEx(_targetProcessHandle, hook1mem, (UIntPtr)0x8000, Constants.PAGE_EXECUTE_READWRITE, out uint oldProtectionOut);

            // Using DarkSoulsFlagsInjector.dll, the VB code memes by Wulf :thinking:
            CodeToAssembly a = new CodeToAssembly();

            a.AddVar("hook", hooks["hook1"]);
            a.AddVar("newmem", hook1mem);
            a.AddVar("vardump", hook1mem + 0x400);
            a.AddVar("hookreturn", hooks["hook1return"]);
            a.AddVar("startloop", 0);
            a.AddVar("exitloop", 0);

            a.pos = (int)(hook1mem);
            a.Asm("pushad");
            a.Asm("mov eax, vardump");

            a.Asm("startloop:");
            a.Asm("mov ecx, [eax]");
            a.Asm("cmp ecx, 0");
            a.Asm("je exitloop");

            a.Asm("add eax, 0x8");
            a.Asm("jmp startloop");

            a.Asm("exitloop:");
            a.Asm("mov [eax], edx");
            a.Asm("mov edx, [esp+0x24]");
            a.Asm("mov [eax+4], edx");
            a.Asm("popad");
            a.Asm($"call 0x" + hooks["hook1seteventflag"].ToString("X" /* X = Hexadecimal */));
            a.Asm("jmp hookreturn");

            WriteCodeAndFlushCache(_targetProcessHandle, hook1mem, a.Bytes, a.Bytes.Length, 0);
            
            a.Clear();
            a.AddVar("newmem", hook1mem);
            a.pos = hooks["hook1"].ToInt32();
            a.Asm("jmp newmem");

            WriteCodeAndFlushCache(_targetProcessHandle, hooks["hook1"], a.Bytes, a.Bytes.Length, 0);
        }

        private void initGetFlagFunc()
        {
            getflagfuncmem = Kernel.VirtualAllocEx(_targetProcessHandle, (IntPtr)0, (IntPtr)0x8000, Constants.MEM_COMMIT, Constants.PAGE_EXECUTE_READWRITE);
            Kernel.VirtualProtectEx(_targetProcessHandle, getflagfuncmem, (UIntPtr)0x8000, Constants.PAGE_EXECUTE_READWRITE, out uint oldProtectionOut);

            // Using DarkSoulsFlagsInjector.dll, the VB code memes by Wulf :thinking:
            // DarkSoulsFlagsInjector.DarkSoulsFlagsInjector a = new DarkSoulsFlagsInjector.DarkSoulsFlagsInjector();
            CodeToAssembly a = new CodeToAssembly();

            a.AddVar("newmem", getflagfuncmem);
            a.AddVar("vardump", getflagfuncmem + 0x400);

            a.pos = (int)(getflagfuncmem);
            a.Asm("pushad");
            a.Asm("mov eax, vardump");
            a.Asm("mov eax, [eax]");
            a.Asm("push eax");
            a.Asm("call 0x" + hooks["geteventflagvalue"].ToString("X" /* X = Hexadecimal */));
            a.Asm("mov ecx, vardump");
            a.Asm("add ecx, 4");
            a.Asm("mov [ecx], eax");
            a.Asm("add ecx, 4");
            a.Asm("mov eax, 1");
            a.Asm("mov [ecx], eax");
            a.Asm("popad");
            a.Asm("ret");

            WriteCodeAndFlushCache(_targetProcessHandle, getflagfuncmem, a.Bytes, a.Bytes.Length, 0);
        }

        private bool WriteCodeAndFlushCache(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten)
        {
            bool result = Kernel.WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, ref lpNumberOfBytesWritten);
            if (!Kernel.FlushInstructionCache(hProcess, lpBaseAddress, (UIntPtr)iSize))
            {
                unhook();
                throw new Exception("Flush Instruction Cache Failed");
            }
            return result;
        }

        public void initHooks()
        {
            rlsHooks.Clear();
            rlsHooks.Add("geteventflagvalue", new IntPtr(0xD60340));
            rlsHooks.Add("hook1", new IntPtr(0xBC1CEA));
            rlsHooks.Add("hook1return", new IntPtr(0xBC1CEF));
            rlsHooks.Add("hook1seteventflag", new IntPtr(0xD38CB0));

            dbgHooks.Clear();
            dbgHooks.Add("geteventflagvalue", new IntPtr(0xD618D0));
            dbgHooks.Add("hook1", new IntPtr(0xBC23CA));
            dbgHooks.Add("hook1return", new IntPtr(0xBC23CF));
            dbgHooks.Add("hook1seteventflag", new IntPtr(0xD3A240));
        }

        public bool ScanForProcess(string windowTitle, bool automatic = false)
        {
            Process[] _allProcesses = Process.GetProcesses();
            Process selectedProcess = null;
            foreach (Process pp in _allProcesses)
            {
                if (pp.MainWindowTitle.ToLower() == windowTitle.ToLower())
                    selectedProcess = pp;
                else
                    pp.Dispose();
            }

            if (selectedProcess != null)
                return TryAttachToProcess(selectedProcess, automatic);
            else
                return false;
        }

        public bool TryAttachToProcess(Process proc, bool automatic = false)
        {
            DetachFromProcess();

            _targetProcess = proc;
            _targetProcessHandle = Kernel.OpenProcess(Constants.PROCESS_ALL_ACCESS, false, _targetProcess.Id);

            if (_targetProcessHandle == IntPtr.Zero)
            {
                if (!automatic)
                    MessageBox.Show("Failed to attach to process. Please rerun the application with administrative privileges.");
                return false;
            }
            else
            {
                // 'if we get here, all connected and ready to use ReadProcessMemory()
                return true;
                // 'MessageBox.Show("OpenProcess() OK")
            }
        }

        public void DetachFromProcess()
        {
            if (_targetProcessHandle != IntPtr.Zero)
            {
                _targetProcess.Dispose();
                _targetProcess = null;

                try
                {
                    Kernel.CloseHandle(_targetProcessHandle);
                    _targetProcessHandle = IntPtr.Zero;
                    //'MessageBox.Show("MemReader::Detach() OK")
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Warning: {0} error " + Environment.NewLine, ex.Message));
                }
            }
        }

        private void CheckDarkSoulsVersion()
        {
            UInt32 versionFlag = RUInt32((IntPtr)0x400080);
            switch (versionFlag)
            {
                case unchecked(0xCE9634B4):
                    exeVER = "Debug";
                    break;
                case unchecked(0xE91B11E2):
                    exeVER = "Beta";
                    break;
                case unchecked(0xFC293654):
                    exeVER = "Release";
                    break;
                default:
                    exeVER = "Unknown";
                    break;
            }
        }

        public void SetDarkSoulsThreadSuspend(bool suspend)
        {
            if (_targetProcess == null)
            {
                return;
            }

            foreach (ProcessThread pthread in _targetProcess.Threads)
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

        private void unhook()
        {
            SetDarkSoulsThreadSuspend(true);

            Kernel.VirtualFreeEx(_targetProcessHandle, hook1mem, 0, Constants.MEM_RELEASE);
            Kernel.VirtualFreeEx(_targetProcessHandle, getflagfuncmem, 0, Constants.MEM_RELEASE);

            Byte[] tmpbytes;

            if (exeVER == "Release")
            {
                tmpbytes = new Byte[] { 0xE8, 0xC1, 0x6F, 0x17, 0 };
                WriteCodeAndFlushCache(_targetProcessHandle, hooks["hook1"], tmpbytes, 5, 0);
            }
            if (exeVER == "Debug")
            {
                tmpbytes = new Byte[] { 0xE8, 0x71, 0x7E, 0x17, 0 };
                WriteCodeAndFlushCache(_targetProcessHandle, hooks["hook1"], tmpbytes, 5, 0);
            }

            SetDarkSoulsThreadSuspend(false);
            DetachFromProcess();
        }

        #region Convert Memes
        public SByte RInt8(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[1];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return Convert.ToSByte(_rtnBytes);
        }

        public Int16 RInt16(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[2];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt16(_rtnBytes, 0);
        }

        public Int32 RInt32(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt32(_rtnBytes, 0);
        }

        public Int64 RInt64(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[8];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToInt64(_rtnBytes, 0);
        }

        public UInt16 RUInt16(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[2];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt16(_rtnBytes, 0);
        }

        public UInt32 RUInt32(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt32(_rtnBytes, 0);
        }

        public UInt64 RUInt64(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[8];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToUInt64(_rtnBytes, 0);
        }

        public float RSingle(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToSingle(_rtnBytes, 0);
        }

        public double RDouble(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[8];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, _rtnBytes.Length, ref bytesRead);
            return BitConverter.ToDouble(_rtnBytes, 0);
        }

        public IntPtr RIntPtr(IntPtr addr)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, IntPtr.Size, ref bytesRead);
            if ((IntPtr.Size == 4))
                return new IntPtr(BitConverter.ToInt32(_rtnBytes, 0));
            else
                return new IntPtr(BitConverter.ToInt64(_rtnBytes, 0));

        }

        public byte[] RBytes(IntPtr addr, Int32 size)
        {
            int bytesRead = 0;
            byte[] _rtnBytes = new byte[4];
            Kernel.ReadProcessMemory(_targetProcessHandle, addr, _rtnBytes, size, ref bytesRead);
            return _rtnBytes;
        }

        public void WInt32(IntPtr addr, Int32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(_targetProcessHandle, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public void WUInt32(IntPtr addr, UInt32 val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(_targetProcessHandle, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public void WSingle(IntPtr addr, Single val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(_targetProcessHandle, addr, BitConverter.GetBytes(val), 4, ref bytesRead);
        }

        public void WBytes(IntPtr addr, byte[] val)
        {
            int bytesRead = 0;
            Kernel.WriteProcessMemory(_targetProcessHandle, addr, val, val.Length, ref bytesRead);
        }
        #endregion
    }
}
