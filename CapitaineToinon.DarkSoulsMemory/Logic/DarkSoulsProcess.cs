using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;

namespace CapitaineToinon.DarkSoulsMemory
{
    internal class DarkSoulsProcess
    {
        #region Variables
        public event EventHandler OnGameProgressUpdated;
        private GameProgress progress;

        private Process darksouls;
        private SafeProcessHandle safeHandle;
        private IntPtr getflagfunmem;
        private GameVersion gameVersion;
        private HookingStates state;
        #endregion

        #region CTORS & DEST
        public DarkSoulsProcess()
        {
            this.state = HookingStates.Start;
            this.safeHandle = null;

            this.progress = new GameProgress(new List<Requirement>()
            {
                { new Requirement("Treasure Locations", 0.2, UpdatePickedUpItems) },
                { new Requirement("Bosses", 0.25, UpdateDefetedBosses) },
                { new Requirement("Non-respawning Enemies", 0.15, UpdateKilledNonRespawningEnemies) },
                { new Requirement("NPC Questlines", 0.2, UpdateCompletedQuestlines) },
                { new Requirement("Shortcuts / Locked Doors", 0.1, UpdateUnlockedShortcutsAndLockedDoors) },
                { new Requirement("Illusory Walls", 0.025, ReleavedIllusoryWalls) },
                { new Requirement("Foggates", 0.025, UpdateDissolvedFoggates) },
                { new Requirement("Kindled Bonfires", 0.05, UpdateFullyKindledBonfires) },
            });
        }

        ~DarkSoulsProcess()
        {
            
        }
        #endregion

        #region Event callbacks
        private void Progress_OnGameProgressUpdated(object sender, EventArgs e)
        {
            // The progress was updated, we send the info to the view
            OnGameProgressUpdated?.Invoke(sender, e);
        }
        #endregion

        #region Main Mehtods
        public void Next()
        {
            switch (state)
            {
                // At the start, we force and progress update to start the UI / whatever is 
                // hooked to progress.OnGameProgressUpdated
                case HookingStates.Start:
                    progress.OnGameProgressUpdated += Progress_OnGameProgressUpdated;
                    progress.Reset();
                    state = HookingStates.Unhooked;
                    break;

                // Will loop here untils it finds a valid DARKSOULS.exe (Release or Debug)
                case HookingStates.Unhooked:
                    if (Hook())
                    {
                        // Game hooked! We can move on
                        state = HookingStates.CheckingProcess;
                    }
                    break;
                
                // Before updating the progress, we verify that the game is running up and running
                // Deallocates the asm memory if it's not the case and switch back to Unhooked
                // If the game is still present, this will bounce back and forth with Hooked
                case HookingStates.CheckingProcess:
                    if (!IsGameRunning())
                    {
                        // Process is gone
                        Deallocate();
                        state = HookingStates.Unhooked;
                    }
                    else
                    {
                        // Everything's fine, we can update
                        state = HookingStates.Hooked;
                    }
                    break;

                // If we're here, then we can finally update all the requirements...
                case HookingStates.Hooked:
                    try
                    {
                        Update();
                        state = HookingStates.CheckingProcess;
                    }
                    // Unless we get an Exception wil reading the memory, which means the game 
                    // got closed / while we were reading. Switch back to CheckingProcess
                    catch (Exception e)
                    when (e is Win32Exception || e is ArgumentNullException || e is ArgumentException)
                    {
                        state = HookingStates.CheckingProcess;
                        return;
                    }
                    break;

                // Cause you always need a default in a switch amarite
                default:
                    state = HookingStates.Unhooked;
                    break;
            }
        }

        private void Update()
        {
            if (IsUpdatable())
            {
                // Anonymous function that will be called after all the requirements are updated
                // to check if the game is still running / updatable before finally updating
                // (avoids getting wrong requirements when the game crashed / player quiouts while
                // we were updating the requirements)
                progress.UpdatePercentage(() => { return (IsUpdatable() && IsGameRunning()); });
            }
        }

        private bool IsUpdatable()
        {
            // if the game is still running
            // if the player is't in a loading screen or in the main menu, 
            // if the player didn't already reach 100%, 
            // if the player didn't already reach NG+, 
            // if the player isn't in someone else's world (for example Lautrec)
            return (IsGameRunning() && !progress.Completed && !FinishedNG() && IsPlayerLoaded() && IsPlayerInOwnWorld());
        }

        private bool IsGameRunning()
        {
            // Returns false is the game was closed / crashed
            return !(safeHandle.IsClosed || safeHandle.IsInvalid || darksouls.HasExited);
        }
        #endregion

        #region Hooking Methods
        private bool Hook()
        {
            Process tmp = GetGame();
            if (tmp != null && gameVersion != GameVersion.Unknown)
            {
                // Dark Souls found! Save the process, safeHandle and allocates the ASM memory
                darksouls = tmp;
                safeHandle = darksouls.SafeHandle;
                getflagfunmem = GetFlagsInit();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Deallocate()
        {
            // If the game is already closed, then the memory is already deallocated
            if (darksouls != null && !darksouls.HasExited)
            {
                Kernel.VirtualFreeEx(safeHandle, getflagfunmem, 0, Constants.MEM_RELEASE);
                darksouls.Dispose();
            }
            darksouls = null;
            safeHandle = null;
            getflagfunmem = IntPtr.Zero;
        }

        public void Quit()
        {
            Deallocate();
            state = HookingStates.Start;
            progress.Reset();
            progress.OnGameProgressUpdated -= Progress_OnGameProgressUpdated;
        }

        private Process GetGame()
        {
            Process[] candidates = Process.GetProcessesByName(Constants.PROCESS_NAME);
            gameVersion = GameVersion.Unknown;

            foreach (Process candidate in candidates)
            {
                SafeProcessHandle c = candidate.SafeHandle;
                UInt32 value = MemoryTools.RUInt32(c, (IntPtr)Pointers.VERSION_CHECK);
                switch (value)
                {
                    case Pointers.VERSION_RELEASE:
                        {
                            gameVersion = GameVersion.Release;
                            return candidate;
                        }
                    case Pointers.VERSION_DEBUG:
                        {
                            gameVersion = GameVersion.Debug;
                            return candidate;
                        }
                    // No test for the Beta version because who gives a shit
                    default:
                        candidate.Dispose();
                        break;
                }
            }

            return null;
        }
        #endregion

        #region Flags Methods
        /// <summary>
        /// Returns true if a Flag is set
        /// </summary>
        /// <param name="eventID"></param>
        /// <returns></returns>
        private bool GetEventFlagState(int eventID)
        {
            // Write the flag ID in the ASM memory
            MemoryTools.WInt32(safeHandle, getflagfunmem + 0x400, eventID);

            // Execute the ASM memory
            IntPtr newThreadHook = (IntPtr)Kernel.CreateRemoteThread(safeHandle, 0, 0, (int)getflagfunmem, 0, 0, 0);
            Kernel.WaitForSingleObject(newThreadHook, 0xFFFFFFFFU);
            Kernel.CloseHandle(newThreadHook);

            // Read the returned value and check if the highest bit is set
            int a = MemoryTools.RInt32(safeHandle, getflagfunmem + 0x404);
            return (a >> 7) == 1;
        }

        /// <summary>
        /// Allocates a memory region and puts the required assembly code
        /// to read Event Flags. Returns the memory allocated.
        /// </summary>
        /// <returns></returns>
        private IntPtr GetFlagsInit()
        {
            IntPtr memory = Kernel.VirtualAllocEx(safeHandle, (IntPtr)0, (IntPtr)0x8000, Constants.MEM_COMMIT, Constants.PAGE_EXECUTE_READWRITE);
            Kernel.VirtualProtectEx(safeHandle, memory, (UIntPtr)0x8000, Constants.PAGE_EXECUTE_READWRITE, out uint oldProtectionOutFunc);

            // Using DarkSoulsFlagsInjector.dll, the VB code memes by Wulf :thinking:
            AssemblyGenerator a = new AssemblyGenerator();

            a.AddVar("newmem", memory);
            a.AddVar("vardump", memory + 0x400);

            a.pos = (int)(memory);
            a.Asm("pushad");
            a.Asm("mov eax, vardump");
            a.Asm("mov eax, [eax]");
            a.Asm("push eax");
            a.Asm("call 0x" + Pointers.GET_EVENT_FLAG.ToString("X"));
            a.Asm("mov ecx, vardump");
            a.Asm("add ecx, 4");
            a.Asm("mov [ecx], eax");
            a.Asm("add ecx, 4");
            a.Asm("mov eax, 1");
            a.Asm("mov [ecx], eax");
            a.Asm("popad");
            a.Asm("ret");

            WriteCodeAndFlushCache(safeHandle, memory, a.Bytes, a.Bytes.Length, 0);

            return memory;
        }

        bool WriteCodeAndFlushCache(SafeProcessHandle hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten)
        {
            bool result = Kernel.WriteProcessMemory(hProcess, lpBaseAddress, lpBuffer, iSize, ref lpNumberOfBytesWritten);
            if (!Kernel.FlushInstructionCache(hProcess, lpBaseAddress, (UIntPtr)iSize))
            {
                throw new Exception("Flush Instruction Cache Failed");
            }
            return result;
        }

        private void Execute(SafeProcessHandle Handle, IntPtr Address)
        {
            IntPtr newThreadHook = (IntPtr)Kernel.CreateRemoteThread(Handle, 0, 0, (int)Address, 0, 0, 0);
            Kernel.WaitForSingleObject(newThreadHook, 0xFFFFFFFFU);
            Kernel.CloseHandle(newThreadHook);
        }
        #endregion

        #region Other Memory Methods
        private bool IsPlayerInOwnWorld()
        {
            PlayerCharacterType t = GetPlayerCharacterType();
            return (t == PlayerCharacterType.Hollow || t == PlayerCharacterType.Human);
        }

        private bool IsPlayerLoaded()
        {
            return MemoryTools.RInt32(safeHandle, (IntPtr)Pointers.CharData1Ptr) != 0;
        }

        private PlayerStartingClass GetPlayerStartingClass()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetPlayerStartingClass];
            ptr = (IntPtr)MemoryTools.RUInt32(safeHandle, ptr);

            if (ptr == IntPtr.Zero)
            {
                return PlayerStartingClass.None;
            }
            else
            {
                ptr = (IntPtr)MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 8));
                int t = MemoryTools.RBytes(safeHandle, IntPtr.Add(ptr, 0xC6), 1)[0];
                return (Enum.IsDefined(typeof(PlayerStartingClass), t)) ? (PlayerStartingClass)t : PlayerStartingClass.None;
            }
        }

        private int GetIngameTimeInMilliseconds()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetIngameTimeInMilliseconds];
            ptr = (IntPtr)MemoryTools.RInt32(safeHandle, ptr);
            return MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 0x68));

        }

        private bool FinishedNG()
        {
            return GetClearCount() > 0;
        }

        private int GetClearCount()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetClearCount];
            ptr = (IntPtr)MemoryTools.RInt32(safeHandle, ptr);
            if (ptr == IntPtr.Zero)
                return -1;
            else
                return MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 0x3C));
        }

        private PlayerCharacterType GetPlayerCharacterType()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetPlayerCharacterType];
            if (ptr == IntPtr.Zero)
            {
                return PlayerCharacterType.None;
            }
            else
            {
                int t = MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 0xA28));
                return (Enum.IsDefined(typeof(PlayerCharacterType), t)) ? (PlayerCharacterType)t : PlayerCharacterType.None;
            }
        }
        #endregion

        #region Requirements Methods
        // Items
        private int[] UpdatePickedUpItems()
        {
            int totalTreasureLocationsCount = Flags.TotalItemFlags.Length;
            int _itemsPickedUp = 0;

            // Check all treasure locations
            foreach (int item in Flags.TotalItemFlags)
            {
                // If the treasure location has multiple items, 
                // check if the last item has been picked up instead to confirm all items have been picked up
                int itemToCheck = item;
                if (Dictionaries.SharedTreasureLocationItems.ContainsKey(item))
                {
                    int[] values = Dictionaries.SharedTreasureLocationItems[item];
                    itemToCheck = values[values.Length - 1];
                }

                if (GetEventFlagState(item))
                    _itemsPickedUp++;

            }
            // Check which starting items the player had and whether he picked them up

            int[] startingItemFlags = Dictionaries.StartingClassItems[GetPlayerStartingClass()];

            foreach (int item in startingItemFlags)
            {
                if (GetEventFlagState(item))
                    _itemsPickedUp++;
            }

            totalTreasureLocationsCount += startingItemFlags.Length;

            // Check for killed NPCs. If one is killed, add their drops to the required item total and check if they have been picked up
            foreach (KeyValuePair<NPC, int[]> pair in Dictionaries.NpcDroppedItems)
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

            return new int[]
            {
                _itemsPickedUp, totalTreasureLocationsCount
            };
        }

        // Bosses
        private int[] UpdateDefetedBosses()
        {
            int bossesKilled = 0;
            foreach (int item in Flags.TotalBossFlags)
            {
                if (GetEventFlagState(item))
                    bossesKilled++;
            }
            return new int[] { bossesKilled, Flags.TotalBossFlags.Length };
        }

        // Non respawnable ennemies
        private int[] UpdateKilledNonRespawningEnemies()
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

            foreach (int[] npc in Dictionaries.NpcHostileDeadFlags)
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
            return new int[]
            {
                nonRespawningEnemiesKilled, totalNonRespawningEnemiesCount
            };
        }

        // Illusory Walls
        private int[] ReleavedIllusoryWalls()
        {
            int illusoryWallsRevealed = 0;
            foreach (int item in Flags.TotalIllusoryWallsFlags)
            {
                if (GetEventFlagState(item))
                    illusoryWallsRevealed++;
            }
            return new int[]
            {
                illusoryWallsRevealed, Flags.TotalIllusoryWallsFlags.Length
            };
        }

        // Foggates
        private int[] UpdateDissolvedFoggates()
        {
            int foggatesDissolved = 0;
            foreach (int item in Flags.TotalFoggatesFlags)
            {
                if (GetEventFlagState(item))
                    foggatesDissolved++;
            }
            return new int[] { foggatesDissolved, Flags.TotalFoggatesFlags.Length };
        }

        // shortcuts and doors
        private int[] UpdateUnlockedShortcutsAndLockedDoors()
        {
            int shortcutsLockedDoorsUnlocked = 0;
            foreach (int item in Flags.TotalShortcutsLockedDoorsFlags)
            {
                if (GetEventFlagState(item))
                    shortcutsLockedDoorsUnlocked++;
            }
            return new int[]
            {
                shortcutsLockedDoorsUnlocked, Flags.TotalShortcutsLockedDoorsFlags.Length
            };
        }

        // NPC questlines
        private int[] UpdateCompletedQuestlines()
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
            return new int[]
            {
                npcQuestlinesCompleted, Flags.TotalNPCQuestlineFlags.Length
            };
        }

        // Fully kindled bonfires
        private int[] UpdateFullyKindledBonfires()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.updateFullyKindledBonfires];

            ptr = (IntPtr)MemoryTools.RInt32(safeHandle, ptr);
            ptr = (IntPtr)MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 0xB48));
            ptr = (IntPtr)MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 0x24));
            ptr = (IntPtr)MemoryTools.RInt32(safeHandle, ptr);

            int kindledBonfires = 0;
            //  'Bonfires accessible in this way are only the ones the player has been able to access at some point
            //  'Once it reaches the end of the list, the bonfireID is 0 and then it loops back around
            //  'So reaching bonfirePtr = IntPtr.Zero means the loop has to end
            int bonfireID = 0;
            IntPtr bonfirePtr = (IntPtr)MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 8));

            while (bonfirePtr != IntPtr.Zero)
            {
                bonfireID = MemoryTools.RInt32(safeHandle, IntPtr.Add(bonfirePtr, 4));
                int kindledState = MemoryTools.RInt32(safeHandle, IntPtr.Add(bonfirePtr, 8));

                if (kindledState == Constants.BONFIRE_FULLY_KINDLED)
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

                ptr = (IntPtr)MemoryTools.RInt32(safeHandle, ptr);
                bonfirePtr = (IntPtr)MemoryTools.RInt32(safeHandle, IntPtr.Add(ptr, 8));
            }

            // We use Flags.TotalBonfireFlags.Length to get the total amount of bonfires because
            // the memory we're looking at in the while loop only contains the bonfires from the
            // areas we've already visited
            return new int[]
            {
                kindledBonfires, Flags.TotalBonfireFlags.Length
            };
        }
        #endregion
    }
}
