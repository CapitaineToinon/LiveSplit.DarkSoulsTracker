using System;
using System.Collections.Generic;

namespace Livesplit.DarkSoulsTracker
{
    class Game
    {
        GameMemory gameMemory;

        public int GetTreasureLocationsCleared
        {
            get { return itemsPickedUp; }
        }

        public int GetTotalTreasureLocationsCount
        {
            get { return totalTreasureLocationsCount; }
        }

        public int GetBossesKilled
        {
            get { return bossesKilled; }
        }

        public int GetTotalBossCount
        {
            get { return Flags.TotalBossFlags.Length; }
        }

        public int GetIllusoryWallsRevealed
        {
            get { return illusoryWallsRevealed; }
        }

        public int GetTotalIllusoryWallsCount
        {
            get { return Flags.TotalIllusoryWallsFlags.Length; }
        }

        public int GetShortcutsAndLockedDoorsUnlocked
        {
            get { return shortcutsLockedDoorsUnlocked; }
        }

        public int GetTotalShortcutsAndLockedDoorsCount
        {
            get { return Flags.TotalShortcutsLockedDoorsFlags.Length; }
        }

        public int GetNPCQuestlinesCompleted
        {
            get { return npcQuestlinesCompleted; }
        }

        public int GetTotalNPCQuestlinesCount
        {
            get { return Flags.TotalNPCQuestlineFlags.Length; }
        }

        public int GetNonRespawningEnemiesKilled
        {
            get { return nonRespawningEnemiesKilled; }
        }

        public int GetTotalNonRespawningEnemiesCount
        {
            get { return totalNonRespawningEnemiesCount; }
        }

        public int GetFoggatesDissolved
        {
            get { return foggatesDissolved; }
        }

        public int GetTotalFoggatesCount
        {
            get { return Flags.TotalFoggatesFlags.Length; }
        }

        public int GetBonfiresFullyKindled
        {
            get { return kindledBonfires; }
        }

        public int GetTotalBonfiresCount
        {
            get { return Flags.TotalBonfireFlags.Length; }
        }

        public double GetTotalCompletionPercentage
        {
            get { return totalCompletionPercentage; }
        }

        public bool Completed
        {
            get { return ((GetTotalCompletionPercentage % 100.0) == 0); }
        }

        int bossesKilled;
        int nonRespawningEnemiesKilled;
        int npcQuestlinesCompleted;
        int itemsPickedUp;
        int shortcutsLockedDoorsUnlocked;
        int illusoryWallsRevealed;
        int foggatesDissolved;

        int kindledBonfires;

        int totalTreasureLocationsCount;
        int totalNonRespawningEnemiesCount;

        double totalCompletionPercentage;

        public Game(GameMemory gameMemory)
        {
            this.gameMemory = gameMemory;
        }

        public void updateAllEventFlags()
        {
            updateTreasureLocationsCount();
            updateDissolvedFoggatesCount();
            updateDefeatedBossesCount();
            updateRevealedIllusoryWallsCount();
            updateUnlockedShortcutsAndLockedDoorsCount();
            updateCompletedQuestlinesCount();
            updateKilledNonRespawningEnemiesCount();
            updateFullyKindledBonfires();

            updateCompletionPercentage();
        }

        public void updateFullyKindledBonfires()
        {
            IntPtr ptr = (gameMemory.exeVER == "Debug") ? (IntPtr)gameMemory.RInt32((IntPtr)0x13823C4) : (IntPtr)gameMemory.RInt32((IntPtr)0x137E204);
            ptr = (IntPtr)gameMemory.RInt32(IntPtr.Add(ptr, 0xB48));
            ptr = (IntPtr)gameMemory.RInt32(IntPtr.Add(ptr, 0x24));
            ptr = (IntPtr)gameMemory.RInt32(ptr);
            kindledBonfires = 0;
            //  'Bonfires accessible in this way are only the ones the player has been able to access at some point
            //  'Once it reaches the end of the list, the bonfireID is 0 and then it loops back around
            //  'So reaching bonfireID = 0 means the loop has to end
            for (int i = 0; i < Flags.TotalBonfireFlags.Length; i++)
            {
                IntPtr bonfirePtr = (IntPtr)gameMemory.RInt32(ptr + 8);
                int bonfireID = gameMemory.RInt32(bonfirePtr + 4);

                if (bonfireID == 0)
                {
                    return;
                }

                int kindledState = gameMemory.RInt32(bonfirePtr + 8);
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
                ptr = (IntPtr)gameMemory.RInt32(ptr); // Go one step deeper in the struct
            }
        }

        public void updateTreasureLocationsCount()
        {
            bool value;
            totalTreasureLocationsCount = Flags.TotalItemFlags.Length;
            itemsPickedUp = 0;

            // Check all treasure locations
            foreach (int item in Flags.TotalItemFlags)
            {
                // If the treasure location has multiple items, 
                // check if the last item has been picked up instead to confirm all items have been picked up
                int itemToCheck = item;
                if (Dictionaries.sharedTreasureLocationItems.ContainsKey(item))
                {
                    int[] values = Dictionaries.sharedTreasureLocationItems[item];
                    itemToCheck = values[values.Length - 1];
                }

                value = GetEventFlagState(item);

                if (value)
                {
                    itemsPickedUp++;
                }
            }
            // Check which starting items the player had and whether he picked them up
            int[] startingItemFlags = Dictionaries.startingClassItems[GetPlayerStartingClass()];

            foreach (int item in startingItemFlags)
            {
                value = GetEventFlagState(item);
                if (value)
                {
                    itemsPickedUp++;
                }
            }

            totalTreasureLocationsCount += startingItemFlags.Length;

            // Check for killed NPCs. If one is killed, add their drops to the required item total and check if they have been picked up
            foreach (KeyValuePair<int, int[]> pair in Dictionaries.npcDroppedItems)
            {
                // Check if NPC - dead flag is true
                value = GetEventFlagState(pair.Key);
                if (value)
                {
                    // If NPC is dead, add his treasure location to required total and check whether the last item has been picked up
                    totalTreasureLocationsCount += 1;
                    int item = pair.Value[pair.Value.Length - 1];
                    value = GetEventFlagState(item);
                    if (value)
                    {
                        itemsPickedUp++;
                    }
                }
            }
        }

        public void updateDissolvedFoggatesCount()
        {
            bool value;
            foggatesDissolved = 0;

            foreach (int item in Flags.TotalFoggatesFlags)
            {
                value = GetEventFlagState(item);
                if (value)
                    foggatesDissolved++;
            }
        }

        public void updateDefeatedBossesCount()
        {
            bool value;
            bossesKilled = 0;

            foreach (int item in Flags.TotalBossFlags)
            {
                value = GetEventFlagState(item);
                if (value)
                    bossesKilled++;
            }
        }

        public void updateRevealedIllusoryWallsCount()
        {
            bool value;
            illusoryWallsRevealed = 0;

            foreach (int item in Flags.TotalIllusoryWallsFlags)
            {
                value = GetEventFlagState(item);
                if (value)
                    illusoryWallsRevealed++;
            }
        }

        public void updateUnlockedShortcutsAndLockedDoorsCount()
        {
            bool value;
            shortcutsLockedDoorsUnlocked = 0;

            foreach (int item in Flags.TotalShortcutsLockedDoorsFlags)
            {
                value = GetEventFlagState(item);
                if (value)
                    shortcutsLockedDoorsUnlocked++;
            }
        }

        public void updateCompletedQuestlinesCount()
        {
            bool value;
            npcQuestlinesCompleted = 0;

            foreach (int item in Flags.TotalNPCQuestlineFlags)
            {
                value = GetEventFlagState(item);
                if (value)
                {
                    npcQuestlinesCompleted++;
                }
                else if (item == 1003) // Solaire has two outcomes: dead or rescued in Izalith
                {
                    value = GetEventFlagState(1011);
                    if (value)
                        npcQuestlinesCompleted++;
                }
                else if (item == 1862) // Ciaran can be disabled after giving her the soul, which uses another flag
                {
                    value = GetEventFlagState(1865);
                    if (value)
                        npcQuestlinesCompleted++;
                }
            }
        }

        public void updateKilledNonRespawningEnemiesCount()
        {
            bool value;
            totalNonRespawningEnemiesCount = Flags.TotalNonRespawningEnemiesFlags.Length;
            nonRespawningEnemiesKilled = 0;

            foreach (int item in Flags.TotalNonRespawningEnemiesFlags)
            {
                if (item == 11515080 || item == 11515081)
                {
                    value = GetEventFlagState(11510400); // Check for AL Gargoyles if it's Dark AL
                    if (value)
                        nonRespawningEnemiesKilled++;
                }

                value = GetEventFlagState(item);
                if (value)
                    nonRespawningEnemiesKilled++;
            }

            foreach (int[] npc in Dictionaries.npcHostileDeadFlags)
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
        }

        public void updateCompletionPercentage()
        {
            double itemPercentage = itemsPickedUp * (0.2 / totalTreasureLocationsCount);
            double bossPercentage = bossesKilled * (0.25 / Flags.TotalBossFlags.Length);
            double nonrespawningPercentage = nonRespawningEnemiesKilled * (0.15 / totalNonRespawningEnemiesCount);
            double questlinesPercentage = npcQuestlinesCompleted * (0.2 / Flags.TotalNPCQuestlineFlags.Length);
            double shortcutsLockedDoorsPercentage = shortcutsLockedDoorsUnlocked * (0.1 / Flags.TotalShortcutsLockedDoorsFlags.Length);
            double illusoryWallsPercentage = illusoryWallsRevealed * (0.025 / Flags.TotalIllusoryWallsFlags.Length);
            double foggatesPercentage = foggatesDissolved * (0.025 / Flags.TotalFoggatesFlags.Length);
            double bonfiresPercentage = kindledBonfires * (0.05 / Flags.TotalBonfireFlags.Length);

            totalCompletionPercentage = itemPercentage + bossPercentage + nonrespawningPercentage + questlinesPercentage + shortcutsLockedDoorsPercentage + illusoryWallsPercentage + foggatesPercentage + bonfiresPercentage;
            totalCompletionPercentage = Math.Floor(totalCompletionPercentage * 1000);
            totalCompletionPercentage /= 10;
        }
            
        public int GetClearCount()
        {
            IntPtr ptr = (gameMemory.exeVER == "Debug") ? (IntPtr)gameMemory.RInt32((IntPtr)0x137C8C0) : (IntPtr)gameMemory.RInt32((IntPtr)0x1378700);
            if (ptr == IntPtr.Zero)
                return -1;
            return gameMemory.RInt32(ptr + 0x3C);
        }


        public bool IsPlayerLoaded()
        {
            IntPtr ptr = (gameMemory.exeVER == "Debug") ? (IntPtr)gameMemory.RInt32((IntPtr)0x1381E30) : (IntPtr)gameMemory.RInt32((IntPtr)0x137DC70);
            if (ptr == IntPtr.Zero)
            {
                return false;
            }
            ptr = (IntPtr)gameMemory.RInt32(IntPtr.Add(ptr, 4));
            return (ptr != IntPtr.Zero);
        }

        public int GetIngameTimeInMilliseconds()
        {
            if (gameMemory.exeVER == "Debug")
                return gameMemory.RInt32((IntPtr)(gameMemory.RInt32((IntPtr)0x137C8C0) + 0x68));
            else
                return gameMemory.RInt32((IntPtr)(gameMemory.RInt32((IntPtr)0x1378700) + 0x68));
        }

        public bool isPlayerInOwnWorld()
        {
            PlayerCharacterType chrType = GetPlayerCharacterType();
            return (chrType == PlayerCharacterType.Hollow || chrType == PlayerCharacterType.Human);
        }

        public PlayerStartingClass GetPlayerStartingClass()
        {
            IntPtr ptr = (gameMemory.exeVER == "Debug") ? (IntPtr)gameMemory.RInt32((IntPtr)0x137C8C0) : (IntPtr)gameMemory.RInt32((IntPtr)0x1378700);
            if (ptr == IntPtr.Zero)
            {
                return PlayerStartingClass.None;
            }
            else
            {
                ptr = (IntPtr)gameMemory.RInt32(ptr + 8);
                if (ptr == IntPtr.Zero)
                    return PlayerStartingClass.None;
                else
                    return (PlayerStartingClass)gameMemory.RBytes(ptr + 0xC6, 1)[0];
            }
        }

        public PlayerCharacterType GetPlayerCharacterType()
        {
            IntPtr ptr = (gameMemory.exeVER == "Debug") ? (IntPtr)gameMemory.RInt32((IntPtr)0x13823C4) : (IntPtr)gameMemory.RInt32((IntPtr)0x137E204);
            if (ptr == IntPtr.Zero)
            {
                return PlayerCharacterType.None;
            }
            else
            {
                return (PlayerCharacterType)gameMemory.RInt32(ptr + 0xA28);
            }
        }

        public bool GetEventFlagState(int eventID)
        {
            gameMemory.WInt32(gameMemory.getflagfuncmem + 0x400, eventID);
            int target = (int)gameMemory._targetProcessHandle;
            int getFlag = (int)gameMemory.getflagfuncmem;
            int dummy = 0;
            IntPtr newThreadHook = (IntPtr)Kernel.CreateRemoteThread(target, 0, 0, getFlag, 0, 0, ref dummy);
            Kernel.WaitForSingleObject(newThreadHook, 0xFFFFFFFFU);
            Kernel.CloseHandle(newThreadHook);
            int a = gameMemory.RInt32(gameMemory.getflagfuncmem + 0x404);
            double b = Math.Pow(2.0, 7.0); // 128
            decimal result = Math.Floor((decimal)a / (decimal)(b));
            return (result == 1);
        }
    }
}
