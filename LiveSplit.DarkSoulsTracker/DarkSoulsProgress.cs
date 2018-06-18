using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiveSplit.DarkSoulsTracker
{
    public class DarkSoulsProgress
    {
        private List<Requirement> requirements;
        private ConcurrentDictionary<int, bool> AllFlagsStates;
        private DarkSoulsProcess darkSoulsProcess;

        public double Percentage
        {
            get
            {
                double percentage = 0;

                if (darkSoulsProcess.IsTracking)
                {
                    UpdateAllFlagsStates();
                    Parallel.ForEach(requirements, r =>
                    {
                        r.Progress = r.Callback();
                        double tmpPercentage = r.Progress[0] * (r.Weight / r.Progress[1]);
                        percentage += tmpPercentage;
                    });
                    percentage *= 100;
                }

                return percentage;
            }
        }

        public List<Requirement> Requirements { get => requirements; }

        public DarkSoulsProgress(DarkSoulsProcess darkSoulsProcess)
        {
            this.darkSoulsProcess = darkSoulsProcess;
            requirements = new List<Requirement>()
            {
                { new Requirement("Bosses", 0.25, Flags.TotalBossFlags, UpdateDefetedBosses) },
                { new Requirement("Treasure Locations", 0.2, Flags.TotalItemFlags, UpdatePickedUpItems) },
                { new Requirement("Non-respawning Enemies", 0.15, Flags.TotalNonRespawningEnemiesFlags, UpdateKilledNonRespawningEnemies) },
                { new Requirement("Illusory Walls", 0.025, Flags.TotalIllusoryWallsFlags, UpdateReleavedIllusoryWalls) },
                { new Requirement("Foggates", 0.025, Flags.TotalFoggatesFlags, UpdateDissolvedFoggates) },
                { new Requirement("Shortcuts / Locked Doors", 0.1, Flags.TotalShortcutsLockedDoorsFlags, UpdateUnlockedShortcutsAndLockedDoors) },
                { new Requirement("NPC Questlines", 0.2, Flags.TotalNPCQuestlineFlags, UpdateCompletedQuestlines) },
                { new Requirement("Kindled Bonfires", 0.05, Flags.TotalBonfireFlags, UpdateFullyKindledBonfires) },
            };

            AllFlagsStates = new ConcurrentDictionary<int, bool>();
            requirements.ForEach(r =>
            {
                r.Flags.ForEach(f =>
                {
                    AllFlagsStates[f] = false;
                });
            });

            // Misc flags
            Flags.MiscFlags.ForEach(f =>
            {
                AllFlagsStates[f] = false;
            });
        }

        private void UpdateAllFlagsStates()
        {
            // Updates all the flags at once
            Parallel.ForEach(AllFlagsStates, FlagState =>
            {
                AllFlagsStates[FlagState.Key] = darkSoulsProcess.GetEventFlagState(FlagState.Key);
            });
        }

        public void Reset()
        {
            // Resets all the requirements. Sets -1 to be sure the percentage
            // will also be updated
            requirements.ForEach(r => r.Progress = new int[] { 0, 1 });
        }

        #region [Flags Methods]
        private bool GetEventFlagState(int ID)
        {
            return (AllFlagsStates.Keys.Contains(ID)) ? AllFlagsStates[ID] : false;
        }
        #endregion

        #region [Requirements Methods]
        /// <summary>
        /// Bosses
        /// </summary>
        /// <returns></returns>
        private int[] UpdateDefetedBosses()
        {
            int bossesKilled = 0;
            foreach (int flag in Flags.TotalBossFlags)
            {
                if (GetEventFlagState(flag))
                    bossesKilled++;
            }
            return new int[] { bossesKilled, Flags.TotalBossFlags.Count };
        }

        /// <summary>
        /// Treasure Locations
        /// </summary>
        /// <returns></returns>
        private int[] UpdatePickedUpItems()
        {
            int totalTreasureLocationsCount = Flags.TotalItemFlags.Count;
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
            int[] startingItemFlags = Dictionaries.StartingClassItems[darkSoulsProcess.GetPlayerStartingClass()];

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


        /// <summary>
        /// Non respawnable ennemies
        /// </summary>
        /// <returns></returns>
        private int[] UpdateKilledNonRespawningEnemies()
        {
            int totalNonRespawningEnemiesCount = Flags.TotalNonRespawningEnemiesFlags.Count;
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

        /// <summary>
        /// Illusory Walls
        /// </summary>
        /// <returns></returns>
        private int[] UpdateReleavedIllusoryWalls()
        {
            int illusoryWallsRevealed = 0;
            foreach (int item in Flags.TotalIllusoryWallsFlags)
            {
                if (GetEventFlagState(item))
                    illusoryWallsRevealed++;
            }
            return new int[]
            {
                illusoryWallsRevealed, Flags.TotalIllusoryWallsFlags.Count
            };
        }

        /// <summary>
        /// Foggates
        /// </summary>
        /// <returns></returns>        
        private int[] UpdateDissolvedFoggates()
        {
            int foggatesDissolved = 0;
            foreach (int item in Flags.TotalFoggatesFlags)
            {
                if (GetEventFlagState(item))
                    foggatesDissolved++;
            }
            return new int[] { foggatesDissolved, Flags.TotalFoggatesFlags.Count };
        }

        /// <summary>
        /// shortcuts and doors
        /// </summary>
        /// <returns></returns>
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
                shortcutsLockedDoorsUnlocked, Flags.TotalShortcutsLockedDoorsFlags.Count
            };
        }

        /// <summary>
        /// NPC questlines
        /// </summary>
        /// <returns></returns>
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
                npcQuestlinesCompleted, Flags.TotalNPCQuestlineFlags.Count
            };
        }

        /// <summary>
        /// Fully kindled bonfires
        /// </summary>
        /// <returns></returns>
        private int[] UpdateFullyKindledBonfires()
        {
            IntPtr ptr = Pointers.PointersTypes[darkSoulsProcess.GameVersion][PointerType.updateFullyKindledBonfires];

            ptr = (IntPtr)darkSoulsProcess.Memory.RInt32(ptr);
            ptr = (IntPtr)darkSoulsProcess.Memory.RInt32(IntPtr.Add(ptr, 0xB48));
            ptr = (IntPtr)darkSoulsProcess.Memory.RInt32(IntPtr.Add(ptr, 0x24));
            ptr = (IntPtr)darkSoulsProcess.Memory.RInt32(ptr);

            int kindledBonfires = 0;
            //  'Bonfires accessible in this way are only the ones the player has been able to access at some point
            //  'Once it reaches the end of the list, the bonfireID is 0 and then it loops back around
            //  'So reaching bonfirePtr = IntPtr.Zero means the loop has to end
            int bonfireID = 0;
            IntPtr bonfirePtr = (IntPtr)darkSoulsProcess.Memory.RInt32(IntPtr.Add(ptr, 8));

            while (bonfirePtr != IntPtr.Zero)
            {
                bonfireID = darkSoulsProcess.Memory.RInt32(IntPtr.Add(bonfirePtr, 4));
                int kindledState = darkSoulsProcess.Memory.RInt32(IntPtr.Add(bonfirePtr, 8));

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

                ptr = (IntPtr)darkSoulsProcess.Memory.RInt32(ptr);
                bonfirePtr = (IntPtr)darkSoulsProcess.Memory.RInt32(IntPtr.Add(ptr, 8));
            }

            // We use Flags.TotalBonfireFlags.Length to get the total amount of bonfires because
            // the memory we're looking at in the while loop only contains the bonfires from the
            // areas we've already visited
            return new int[]
            {
                kindledBonfires, Flags.TotalBonfireFlags.Count
            };
        }
        #endregion

    }


}
