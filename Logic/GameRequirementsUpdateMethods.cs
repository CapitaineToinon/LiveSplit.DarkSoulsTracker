using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Livesplit.DarkSouls100Tracker.Logic
{
    public partial class Game
    {
        private void UpdateAllRequirements()
        {
            // Updates all requirements
            foreach (Requirement r in gameProgress.Requirements)
            {
                r.Progression = r.Callback();
            }

            // Done
            gameProgress.UpdatePercentage();
            this.OnGameProgressUpdated(gameProgress, EventArgs.Empty);
        }

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

                if (memoryTools.GetEventFlagState(item))
                    _itemsPickedUp++;

            }
            // Check which starting items the player had and whether he picked them up
            int[] startingItemFlags = Dictionaries.StartingClassItems[GetPlayerStartingClass()];

            foreach (int item in startingItemFlags)
            {
                if (memoryTools.GetEventFlagState(item))
                    _itemsPickedUp++;
            }

            totalTreasureLocationsCount += startingItemFlags.Length;

            // Check for killed NPCs. If one is killed, add their drops to the required item total and check if they have been picked up
            foreach (KeyValuePair<NPC, int[]> pair in Dictionaries.NpcDroppedItems)
            {
                // Check if NPC - dead flag is true
                if (memoryTools.GetEventFlagState((int)pair.Key))
                {
                    // If NPC is dead, add his treasure location to required total and check whether the last item has been picked up
                    totalTreasureLocationsCount += 1;
                    int item = pair.Value[pair.Value.Length - 1];
                    if (memoryTools.GetEventFlagState(item))
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
                if (memoryTools.GetEventFlagState(item))
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
                    if (memoryTools.GetEventFlagState(11510400)) // Check for AL Gargoyles if it's Dark AL
                        nonRespawningEnemiesKilled++;
                }

                if (memoryTools.GetEventFlagState(item))
                    nonRespawningEnemiesKilled++;
            }

            foreach (int[] npc in Dictionaries.NpcHostileDeadFlags)
            {
                if (memoryTools.GetEventFlagState(npc[0]))
                {
                    totalNonRespawningEnemiesCount++;
                }
                else if (memoryTools.GetEventFlagState(npc[1]))
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
                if (memoryTools.GetEventFlagState(item))
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
                if (memoryTools.GetEventFlagState(item))
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
                if (memoryTools.GetEventFlagState(item))
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
                if (memoryTools.GetEventFlagState(item))
                {
                    npcQuestlinesCompleted++;
                }
                else if (item == 1003) // Solaire has two outcomes: dead or rescued in Izalith
                {
                    if (memoryTools.GetEventFlagState(1011))
                        npcQuestlinesCompleted++;
                }
                else if (item == 1862) // Ciaran can be disabled after giving her the soul, which uses another flag
                {
                    if (memoryTools.GetEventFlagState(1865))
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
            IntPtr ptr = Dictionaries.PointersTypes[memoryTools.ExeType][PointerType.updateFullyKindledBonfires];
            ptr = (IntPtr)memoryTools.RInt32(ptr);
            ptr = (IntPtr)memoryTools.RInt32(IntPtr.Add(ptr, 0xB48));
            ptr = (IntPtr)memoryTools.RInt32(IntPtr.Add(ptr, 0x24));
            ptr = (IntPtr)memoryTools.RInt32(ptr);


            int kindledBonfires = 0;
            //  'Bonfires accessible in this way are only the ones the player has been able to access at some point
            //  'Once it reaches the end of the list, the bonfireID is 0 and then it loops back around
            //  'So reaching bonfireID = 0 means the loop has to end
            int bonfireID = 0;
            do
            {
                IntPtr bonfirePtr = (IntPtr)memoryTools.RInt32(ptr + 8);
                bonfireID = memoryTools.RInt32(bonfirePtr + 4);

                int kindledState = memoryTools.RInt32(bonfirePtr + 8);
                if (kindledState == Constants.BONFIRE_FULLY_KINDLED)
                {
                    kindledBonfires++;
                }
                else
                {
                    // If bonfire is not fully kindled, check whether it's the AL or DoC bonfire
                    // If yes, check whether the respective Firekeeper is dead. If yes, treat the bonfire as fully kindled
                    if (bonfireID == 1511960 && memoryTools.GetEventFlagState(1034))
                    {
                        kindledBonfires++;
                    }
                    if (bonfireID == 1401960 && memoryTools.GetEventFlagState(1272))
                    {
                        kindledBonfires++;
                    }
                }

                ptr = (IntPtr)memoryTools.RInt32(ptr); // Go one step deeper in the struct
            } while (bonfireID != 0);

            // We use Flags.TotalBonfireFlags.Length to get the total amount of bonfires because
            // the memory we're looking at in the while loop only contains the bonfires from the
            // areas we've already visited
            return new int[]
            {
                kindledBonfires, Flags.TotalBonfireFlags.Length
            };
        }
    }
}
