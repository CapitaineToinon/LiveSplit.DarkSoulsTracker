using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Livesplit.DarkSouls100PercentTracker
{
    public enum ExeTypes
    {
        Release,
        Debug,
        Beta,
        Unknown,
    }

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

        // 2 frames at 30 FPS
        public const int Thread_Frequency = 33;
    }

    class Tracker
    {
        #region Variables and properties
        private bool IsAlreadyRunning = false;

        public event EventHandler OnPercentageUpdated;
        private CancellationTokenSource MainThreadToken;
        string DARKSOULSName = "DARK SOULS";
        private Process DARKSOULS = null;
        private IntPtr DARKSOULSHandle = IntPtr.Zero;
        private IntPtr hook1mem;
        private IntPtr getflagfuncmem;
        private Dictionaries dictionaries;
        public bool IsHooked
        {
            get { return (DARKSOULSHandle != IntPtr.Zero); }
        }

        // Returns the type of EXE is currently running
        public ExeTypes ExeType
        {
            get
            {
                if (IsHooked)
                {
                    UInt32 versionFlag = MemoryTools.RUInt32(DARKSOULSHandle, Dictionaries.GameVersion);
                    if (versionFlag == Dictionaries.Release)
                        return ExeTypes.Release;
                    else if (versionFlag == Dictionaries.Debug)
                        return ExeTypes.Debug;
                    else if (versionFlag == Dictionaries.Beta)
                        return ExeTypes.Beta;
                    else
                        return ExeTypes.Unknown;
                }
                else
                {
                    return ExeTypes.Unknown;
                }
            }
        }
        #endregion

        #region Percentage and things
        private int[] defeatedBossesCount = new int[] { 0, 1 };
        private int[] itemsPickedUp = new int[] { 0, 1 };
        private int[] dissolvedFoggatesCount = new int[] { 0, 1 };
        private int[] revealedIllusoryWallsCount = new int[] { 0, 1 };
        private int[] unlockedShortcutsAndLockedDoorsCount = new int[] { 0, 1 };
        private int[] completedQuestlinesCount = new int[] { 0, 1 };
        private int[] killedNonRespawningEnemiesCount = new int[] { 0, 1 };
        private int[] fullyKindledBonfires = new int[] { 0, 1 };
        private double totalPercentage = 0.0;

        private void update_defeatedBossesCount()
        {
            int bossesKilled = 0;
            foreach (int item in Flags.TotalBossFlags)
            {
                if (GetEventFlagState(item))
                    bossesKilled++;
            }
            defeatedBossesCount = new int[]
            {
                bossesKilled, Flags.TotalBossFlags.Length
            };
        }
        private void update_itemsPickedUp()
        {
            int totalTreasureLocationsCount = Flags.TotalItemFlags.Length;
            int _itemsPickedUp = 0;

            // Check all treasure locations
            foreach (int item in Flags.TotalItemFlags)
            {
                // If the treasure location has multiple items, 
                // check if the last item has been picked up instead to confirm all items have been picked up
                int itemToCheck = item;
                if (dictionaries.SharedTreasureLocationItems.ContainsKey(item))
                {
                    int[] values = dictionaries.SharedTreasureLocationItems[item];
                    itemToCheck = values[values.Length - 1];
                }

                if (GetEventFlagState(item))
                    _itemsPickedUp++;

            }
            // Check which starting items the player had and whether he picked them up
            int[] startingItemFlags = dictionaries.StartingClassItems[PlayerStartingClass];

            foreach (int item in startingItemFlags)
            {
                if (GetEventFlagState(item))
                    _itemsPickedUp++;
            }

            totalTreasureLocationsCount += startingItemFlags.Length;

            // Check for killed NPCs. If one is killed, add their drops to the required item total and check if they have been picked up
            foreach (KeyValuePair<NPC, int[]> pair in dictionaries.NpcDroppedItems)
            {
                // Check if NPC - dead flag is true
                if (GetEventFlagState((int)pair.Key))
                {
                    // If NPC is dead, add his treasure location to required total and check whether the last item has been picked up
                    totalTreasureLocationsCount += 1;
                    int item = pair.Value[pair.Value.Length - 1];
                    if (GetEventFlagState(item))
                        _itemsPickedUp++;
                }
            }

            itemsPickedUp = new int[]
            {
                _itemsPickedUp, totalTreasureLocationsCount
            };
        }
        private void update_dissolvedFoggatesCount()
        {
            int foggatesDissolved = 0;
            foreach (int item in Flags.TotalFoggatesFlags)
            {
                if (GetEventFlagState(item))
                    foggatesDissolved++;
            }
            dissolvedFoggatesCount = new int[]
            {
                foggatesDissolved, Flags.TotalFoggatesFlags.Length
            };
        }
        private void update_revealedIllusoryWallsCount()
        {
            int illusoryWallsRevealed = 0;
            foreach (int item in Flags.TotalIllusoryWallsFlags)
            {
                if (GetEventFlagState(item))
                    illusoryWallsRevealed++;
            }
            revealedIllusoryWallsCount = new int[]
            {
                illusoryWallsRevealed, Flags.TotalIllusoryWallsFlags.Length
            };
        }
        private void update_unlockedShortcutsAndLockedDoorsCount()
        {
            int shortcutsLockedDoorsUnlocked = 0;
            foreach (int item in Flags.TotalShortcutsLockedDoorsFlags)
            {
                if (GetEventFlagState(item))
                    shortcutsLockedDoorsUnlocked++;
            }
            unlockedShortcutsAndLockedDoorsCount = new int[]
            {
                shortcutsLockedDoorsUnlocked, Flags.TotalShortcutsLockedDoorsFlags.Length
            };
        }
        private void update_completedQuestlinesCount()
        {
            int npcQuestlinesCompleted = 0;
            foreach (int item in Flags.TotalNPCQuestlineFlags)
            {
                if (GetEventFlagState(item))
                {
                    npcQuestlinesCompleted++;
                }
                else if (item == 1003) // Solaire has two outcomes: dead or rescued in Izalith
                {
                    if (GetEventFlagState(1011))
                        npcQuestlinesCompleted++;
                }
                else if (item == 1862) // Ciaran can be disabled after giving her the soul, which uses another flag
                {
                    if (GetEventFlagState(1865))
                        npcQuestlinesCompleted++;
                }
            }
            completedQuestlinesCount = new int[]
            {
                    npcQuestlinesCompleted, Flags.TotalNPCQuestlineFlags.Length
            };
        }
        private void update_killedNonRespawningEnemiesCount()
        {
            int totalNonRespawningEnemiesCount = Flags.TotalNonRespawningEnemiesFlags.Length;
            int nonRespawningEnemiesKilled = 0;
            foreach (int item in Flags.TotalNonRespawningEnemiesFlags)
            {
                if (item == 11515080 || item == 11515081)
                {
                    if (GetEventFlagState(11510400)) // Check for AL Gargoyles if it's Dark AL
                        nonRespawningEnemiesKilled++;
                }

                if (GetEventFlagState(item))
                    nonRespawningEnemiesKilled++;
            }

            foreach (int[] npc in dictionaries.NpcHostileDeadFlags)
            {
                if (GetEventFlagState(npc[0]))
                {
                    totalNonRespawningEnemiesCount++;
                }
                else if (GetEventFlagState(npc[1]))
                {
                    // If NPC is dead, add him to total non-respawning enemies required and mark him as killed
                    totalNonRespawningEnemiesCount++;
                    nonRespawningEnemiesKilled++;
                }
            }
            killedNonRespawningEnemiesCount = new int[]
            {
                nonRespawningEnemiesKilled, totalNonRespawningEnemiesCount
            };
        }
        private void update_fullyKindledBonfires()
        {
            IntPtr ptr = dictionaries.PointersTypes[PointerType.updateFullyKindledBonfires];
            ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr);
            ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, IntPtr.Add(ptr, 0xB48));
            ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, IntPtr.Add(ptr, 0x24));
            ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr);


            int kindledBonfires = 0;
            //  'Bonfires accessible in this way are only the ones the player has been able to access at some point
            //  'Once it reaches the end of the list, the bonfireID is 0 and then it loops back around
            //  'So reaching bonfireID = 0 means the loop has to end
            for (int i = 0; i < Flags.TotalBonfireFlags.Length; i++)
            {
                IntPtr bonfirePtr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr + 8);
                int bonfireID = MemoryTools.RInt32(DARKSOULSHandle, bonfirePtr + 4);

                if (bonfireID == 0)
                {
                    return;
                }

                int kindledState = MemoryTools.RInt32(DARKSOULSHandle, bonfirePtr + 8);
                if (kindledState == 40)
                {
                    kindledBonfires++;
                }
                else
                {
                    // If bonfire is not fully kindled, check whether it's the AL or DoC bonfire
                    // If yes, check whether the respective Firekeeper is dead. If yes, treat the bonfire as fully kindled
                    if (bonfireID == 1511960 && GetEventFlagState(1034))
                    {
                        kindledBonfires++;
                    }
                    if (bonfireID == 1401960 && GetEventFlagState(1272))
                    {
                        kindledBonfires++;
                    }
                }
                ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr); // Go one step deeper in the struct
            }
            fullyKindledBonfires = new int[]
            {
                kindledBonfires, Flags.TotalBonfireFlags.Length
            };
        }
        private void update_totalPercentage()
        {
            double totalCompletionPercentage;

            double itemPercentage = itemsPickedUp[0] * (0.2 / itemsPickedUp[1]);
            double bossPercentage = defeatedBossesCount[0] * (0.25 / defeatedBossesCount[1]);
            double nonrespawningPercentage = killedNonRespawningEnemiesCount[0] * (0.15 / killedNonRespawningEnemiesCount[1]);
            double questlinesPercentage = completedQuestlinesCount[0] * (0.2 / completedQuestlinesCount[1]);
            double shortcutsLockedDoorsPercentage = unlockedShortcutsAndLockedDoorsCount[0] * (0.1 / unlockedShortcutsAndLockedDoorsCount[1]);
            double illusoryWallsPercentage = revealedIllusoryWallsCount[0] * (0.025 / revealedIllusoryWallsCount[1]);
            double foggatesPercentage = dissolvedFoggatesCount[0] * (0.025 / dissolvedFoggatesCount[1]);
            double bonfiresPercentage = fullyKindledBonfires[0] * (0.05 / fullyKindledBonfires[1]);

            totalCompletionPercentage = itemPercentage + bossPercentage + nonrespawningPercentage + questlinesPercentage + shortcutsLockedDoorsPercentage + illusoryWallsPercentage + foggatesPercentage + bonfiresPercentage;
            totalCompletionPercentage *= 100;

            totalPercentage = totalCompletionPercentage;
        }
        public void Update()
        {
            update_defeatedBossesCount();
            update_itemsPickedUp();
            update_dissolvedFoggatesCount();
            update_revealedIllusoryWallsCount();
            update_unlockedShortcutsAndLockedDoorsCount();
            update_completedQuestlinesCount();
            update_killedNonRespawningEnemiesCount();
            update_fullyKindledBonfires();

            update_totalPercentage();
        }

        public int[] DefeatedBossesCount { get => defeatedBossesCount; }
        public int[] ItemsPickedUp { get => itemsPickedUp; }
        public int[] DissolvedFoggatesCount { get => dissolvedFoggatesCount; }
        public int[] RevealedIllusoryWallsCount { get => revealedIllusoryWallsCount; }
        public int[] UnlockedShortcutsAndLockedDoorsCount { get => unlockedShortcutsAndLockedDoorsCount; }
        public int[] CompletedQuestlinesCount { get => completedQuestlinesCount; }
        public int[] KilledNonRespawningEnemiesCount { get => killedNonRespawningEnemiesCount; }
        public int[] FullyKindledBonfires { get => fullyKindledBonfires; }
        public double TotalPercentage { get => totalPercentage; }
        #endregion

        #region Game memory properties
        private bool IsPlayerInOwnWorld
        {
            get
            {
                return (PlayerCharacterType == PlayerCharacterType.Hollow || PlayerCharacterType == PlayerCharacterType.Human);
            }
        }
        private bool IsPlayerLoaded
        {
            get
            {
                IntPtr ptr = dictionaries.PointersTypes[PointerType.IsPlayerLoaded];
                if (ptr == IntPtr.Zero)
                {
                    return false;
                }
                ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, IntPtr.Add(ptr, 4));
                return (ptr != IntPtr.Zero);
            }
        }

        private PlayerStartingClass PlayerStartingClass
        {
            get
            {
                IntPtr ptr = dictionaries.PointersTypes[PointerType.GetPlayerStartingClass];
                ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr);
                if (ptr == IntPtr.Zero)
                {
                    return PlayerStartingClass.None;
                }
                else
                {
                    ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr + 8);
                    if (ptr == IntPtr.Zero)
                        return PlayerStartingClass.None;
                    else
                        return (PlayerStartingClass)MemoryTools.RBytes(DARKSOULSHandle, ptr + 0xC6, 1)[0];
                }
            }
        }

        private PlayerCharacterType PlayerCharacterType
        {
            get
            {
                IntPtr ptr = dictionaries.PointersTypes[PointerType.GetPlayerCharacterType];
                if (ptr == IntPtr.Zero)
                {
                    return PlayerCharacterType.None;
                }
                else
                {
                    return (PlayerCharacterType)MemoryTools.RInt32(DARKSOULSHandle, ptr + 0xA28);
                }
            }
        }

        private int GetIngameTimeInMilliseconds()
        {
            IntPtr ptr = dictionaries.PointersTypes[PointerType.GetIngameTimeInMilliseconds];
            return MemoryTools.RInt32(DARKSOULSHandle, (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr) + 0x68);
        }

        public int GetClearCount()
        {
            IntPtr ptr = dictionaries.PointersTypes[PointerType.GetClearCount];
            ptr = (IntPtr)MemoryTools.RInt32(DARKSOULSHandle, ptr);
            if (ptr == IntPtr.Zero)
                return -1;
            else
                return MemoryTools.RInt32(DARKSOULSHandle, ptr + 0x32);
        }

        private bool GetEventFlagState(int eventID)
        {
            if (IsHooked)
            {
                MemoryTools.WInt32(DARKSOULSHandle, getflagfuncmem + 0x400, eventID);
                int target = (int)DARKSOULSHandle;
                int getFlag = (int)getflagfuncmem;
                int dummy = 0;
                IntPtr newThreadHook = (IntPtr)Kernel.CreateRemoteThread(target, 0, 0, getFlag, 0, 0, ref dummy);
                Kernel.WaitForSingleObject(newThreadHook, 0xFFFFFFFFU);
                Kernel.CloseHandle(newThreadHook);
                int a = MemoryTools.RInt32(DARKSOULSHandle, getflagfuncmem + 0x404);
                double b = Math.Pow(2.0, 7.0); // 128
                decimal result = Math.Floor((decimal)a / (decimal)(b));
                return (result == 1);
            }
            else
            {
                return false;
            }
        }
        #endregion

        public Tracker()
        {

        }

        #region Hooking and stuff functions
        /// <summary>
        /// Hooks
        /// </summary>
        private void Hook()
        {
            if (Attach(DARKSOULSName))
            {
                ExeTypes type = ExeType;
                if (type == ExeTypes.Unknown || type == ExeTypes.Beta)
                {
                    //Console.WriteLine("Invalid EXE type.");
                    UnHook();
                    return;
                }
                else
                {
                    //Console.WriteLine("Hooked ! Game's version is : " + ExeType.ToString());
                    return;
                }
            }
            else
            {
                //Console.WriteLine("Couldn't find the Dark Souls process!");
                UnHook();
                return;
            }
        }

        /// <summary>
        /// UnHooks DARKSOULS 
        /// Sets DARKSOULS to null
        /// Sets DARKSOULSHandle to IntPtr.Zero
        /// </summary>
        /// <returns></returns>
        private bool UnHook()
        {
            if (DARKSOULSHandle != IntPtr.Zero)
            {
                DARKSOULS.Dispose();
                DARKSOULS = null;

                try
                {
                    Kernel.CloseHandle(DARKSOULSHandle);
                    DARKSOULSHandle = IntPtr.Zero;
                    // Succesfuly detached
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Warning: {0} error " + Environment.NewLine, ex.Message));
                    // Couldn't detach
                    return false;
                }

            }
            else
            {
                // Game is already detached
                return true;
            }
        }

        /// <summary>
        /// Attachs DARKSOULS process
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool Attach(string name)
        {
            UnHook();
            DARKSOULS = GetProcess(name);
            if (DARKSOULS != null)
            {
                DARKSOULSHandle = Kernel.OpenProcess(Constants.PROCESS_ALL_ACCESS, false, DARKSOULS.Id);
                if (DARKSOULSHandle != IntPtr.Zero)
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
        /// This is where the magic happens
        /// Wulf you're a god, I think 🤔
        /// </summary>
        private void InitFlagsAndFunctions()
        {
            bool WriteCodeAndFlushCache(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten)
            {
                bool result = Kernel.WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, ref lpNumberOfBytesWritten);
                if (!Kernel.FlushInstructionCache(hProcess, lpBaseAddress, (UIntPtr)iSize))
                {
                    UnHook();
                    throw new Exception("Flush Instruction Cache Failed");
                }
                return result;
            }

            // HOOKS
            Dictionary<string, IntPtr> hooks = Dictionaries.GetHooks(ExeType);

            // *********************** FLAGS *********************** //
            hook1mem = Kernel.VirtualAllocEx(DARKSOULSHandle, (IntPtr)0, (IntPtr)0x8000, Constants.MEM_COMMIT, Constants.PAGE_EXECUTE_READWRITE);
            Kernel.VirtualProtectEx(DARKSOULSHandle, hook1mem, (UIntPtr)0x8000, Constants.PAGE_EXECUTE_READWRITE, out uint oldProtectionOutFlag);

            // Using DarkSoulsFlagsInjector.dll, the VB code memes by Wulf :thinking:
            AssemblyGenerator a = new AssemblyGenerator();

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

            WriteCodeAndFlushCache(DARKSOULSHandle, hook1mem, a.Bytes, a.Bytes.Length, 0);

            a.Clear();
            a.AddVar("newmem", hook1mem);
            a.pos = hooks["hook1"].ToInt32();
            a.Asm("jmp newmem");

            WriteCodeAndFlushCache(DARKSOULSHandle, hooks["hook1"], a.Bytes, a.Bytes.Length, 0);

            // *********************** FUNC *********************** //
            getflagfuncmem = Kernel.VirtualAllocEx(DARKSOULSHandle, (IntPtr)0, (IntPtr)0x8000, Constants.MEM_COMMIT, Constants.PAGE_EXECUTE_READWRITE);
            Kernel.VirtualProtectEx(DARKSOULSHandle, getflagfuncmem, (UIntPtr)0x8000, Constants.PAGE_EXECUTE_READWRITE, out uint oldProtectionOutFunc);

            // Using DarkSoulsFlagsInjector.dll, the VB code memes by Wulf :thinking:
            // DarkSoulsFlagsInjector.DarkSoulsFlagsInjector a = new DarkSoulsFlagsInjector.DarkSoulsFlagsInjector();
            a = new AssemblyGenerator();

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

            WriteCodeAndFlushCache(DARKSOULSHandle, getflagfuncmem, a.Bytes, a.Bytes.Length, 0);
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

        #endregion

        #region Threads 
        public async void Start()
        {
            MainThreadToken = new CancellationTokenSource();
            if (!IsAlreadyRunning)
            {
                IsAlreadyRunning = true;
                await Task.Factory.StartNew(() =>
                {
                    bool needsRehooking = false;
                    bool completed = false;

                    // Until the thread is canceled...
                    while (!MainThreadToken.IsCancellationRequested)
                    {
                        // Unhook the game if rehooking is needed
                        if (IsHooked && needsRehooking)
                        {
                            UnHook();
                            needsRehooking = false;
                        }

                        // Hooks the game if needed
                        if (!IsHooked)
                        {
                            // Hook the game...
                            Hook();
                            if (IsHooked)
                            {
                                if (ExeType == ExeTypes.Release || ExeType == ExeTypes.Debug)
                                {
                                    //Console.WriteLine("Game is hooked, ExeType : " + ExeType.ToString());

                                    dictionaries = new Dictionaries(ExeType);

                                    // DarkSoulsFlagsInjector shenanigans :thinking:
                                    // This is where the magic happens
                                    SetDarkSoulsThreadSuspend(true);
                                    InitFlagsAndFunctions();
                                    SetDarkSoulsThreadSuspend(false);
                                }
                                else
                                {
                                    MainThreadToken.Cancel();
                                    //Console.WriteLine("Wrong ExeType. Exit.");
                                    return;
                                }
                            }
                            else
                            {
                                // Game not found
                                MainThreadToken.Cancel();
                                //Console.WriteLine("Game not found, thread canceled");
                                return;
                            }
                        }
                        else
                        {
                            try
                            {
                                // Do nothing when at the main menu, if 100% is achieved or NG+ triggered
                                if (GetIngameTimeInMilliseconds() != 0 && !completed)
                                {
                                    int CurrentIGT = GetIngameTimeInMilliseconds();
                                    Thread.Sleep(Constants.Thread_Frequency);
                                    int NextIGT = GetIngameTimeInMilliseconds();

                                    // Everytime the player enters a loadscreen, the hook gets disconnected and reconnected
                                    // If IGT returns 0, the player is in the main menu. Disconnecting in the main menu may lead to the game freezing
                                    if ((CurrentIGT == NextIGT))
                                    {
                                        needsRehooking = true;
                                    }
                                    // If the player is in his own world or not at the main menu
                                    else if (IsPlayerInOwnWorld && IsPlayerLoaded && GetIngameTimeInMilliseconds() != 0)
                                    {
                                        // Updates all the flags and calls the event to update the UI
                                        Update();
                                        this.OnPercentageUpdated(this, EventArgs.Empty);

                                        // Check if completed
                                        completed = (TotalPercentage == 100);
                                        completed = (GetClearCount() > 0);
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

                        // tick
                        // Console.WriteLine("Tick");
                        Thread.Sleep(1000);
                    }

                    // Thread has been canceled, unhooks before leaving
                    IsAlreadyRunning = false;
                    if (IsHooked)
                    {
                        //Console.WriteLine("Thread stopped.");
                        UnHook();
                    }
                });
            }
            else
            {
                //Console.WriteLine("Thread already running");
            }
        }

        public async void Stop()
        {
            await Task.Factory.StartNew(() =>
            {
                if (MainThreadToken != null)
                {
                    //Console.WriteLine("Thread cancel requested...");
                    MainThreadToken.Cancel();
                }
            });
        }
        #endregion
    }
}
