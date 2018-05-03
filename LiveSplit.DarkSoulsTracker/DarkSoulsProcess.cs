using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveSplit.DarkSoulsTracker.Tools;

namespace LiveSplit.DarkSoulsTracker
{
    public class DarkSoulsProcess
    {
        Process darksouls;
        GameVersion gameVersion;
        Memory memory;

        DarkSoulsProgress soulsProgress;

        internal GameVersion GameVersion => gameVersion;
        internal Memory Memory => memory;
        internal bool IsTracking => (darksouls != null);

        public DarkSoulsProgress DarkSoulsProgress => soulsProgress;

        public DarkSoulsProcess()
        {
            soulsProgress = new DarkSoulsProgress(this);
        }

        public void Start()
        {
            Hook();
        }

        public void Stop()
        {
            Unhook();
        }

        #region [Flags Methods]
        private bool GetEventFlagAddress(int ID, out int address, out uint mask)
        {
            string idString = ID.ToString("D8");
            if (idString.Length == 8 && darksouls != null)
            {
                string group = idString.Substring(0, 1);
                string area = idString.Substring(1, 3);
                int section = Int32.Parse(idString.Substring(4, 1));
                int number = Int32.Parse(idString.Substring(5, 3));

                if (Dictionaries.EventFlagGroups.ContainsKey(group) && Dictionaries.EventFlagAreas.ContainsKey(area))
                {
                    int offset = Dictionaries.EventFlagGroups[group];
                    offset += Dictionaries.EventFlagAreas[area] * 0x500;
                    offset += section * 128;
                    offset += (number - (number % 32)) / 8;

                    address = memory.RInt32((IntPtr)Pointers.EventFlagPtr[gameVersion]);
                    address = memory.RInt32((IntPtr)address);
                    address += offset;

                    mask = 0x80000000 >> (number % 32);
                    return true;
                }
            }

            address = 0;
            mask = 0;
            return false;
        }

        public bool GetEventFlagState(int ID)
        {
            if (GetEventFlagAddress(ID, out int address, out uint mask))
            {
                uint flags = (uint)memory.RInt32((IntPtr)address);
                return (flags & mask) != 0;
            }
            else
                return false;
        }
        #endregion

        #region [Other Memory Methods]
        internal bool IsPlayerInOwnWorld()
        {
            PlayerCharacterType t = GetPlayerCharacterType();
            return (t == PlayerCharacterType.Hollow || t == PlayerCharacterType.Human);
        }

        internal bool IsPlayerLoaded()
        {
            return memory.RInt32((IntPtr)Pointers.CharData1Ptr) != 0;
        }

        internal PlayerStartingClass GetPlayerStartingClass()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetPlayerStartingClass];
            ptr = (IntPtr)memory.RUInt32(ptr);

            if (ptr == IntPtr.Zero)
            {
                return PlayerStartingClass.None;
            }
            else
            {
                ptr = (IntPtr)memory.RInt32(IntPtr.Add(ptr, 8));
                int t = memory.RBytes(IntPtr.Add(ptr, 0xC6), 1)[0];
                return (Enum.IsDefined(typeof(PlayerStartingClass), t)) ? (PlayerStartingClass)t : PlayerStartingClass.None;
            }
        }

        internal int GetIngameTimeInMilliseconds()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetIngameTimeInMilliseconds];
            ptr = (IntPtr)memory.RInt32(ptr);
            return memory.RInt32(IntPtr.Add(ptr, 0x68));

        }

        internal bool FinishedNG()
        {
            return GetClearCount() > 0;
        }

        internal int GetClearCount()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetClearCount];
            ptr = (IntPtr)memory.RInt32(ptr);
            if (ptr == IntPtr.Zero)
                return -1;
            else
                return memory.RInt32(IntPtr.Add(ptr, 0x3C));
        }

        internal PlayerCharacterType GetPlayerCharacterType()
        {
            IntPtr ptr = Pointers.PointersTypes[gameVersion][PointerType.GetPlayerCharacterType];
            if (ptr == IntPtr.Zero)
            {
                return PlayerCharacterType.None;
            }
            else
            {
                int t = memory.RInt32(IntPtr.Add(ptr, 0xA28));
                return (Enum.IsDefined(typeof(PlayerCharacterType), t)) ? (PlayerCharacterType)t : PlayerCharacterType.None;
            }
        }
        #endregion

        #region [Hooking & Unhooking]
        Process GetGame(out GameVersion gameVersion)
        {
            gameVersion = GameVersion.Unknown;
            Process[] candidates = Process.GetProcessesByName(Constants.PROCESS_NAME);
            foreach (Process c in candidates)
            {
                if (IsValidDarkSoulsProcess(c, out gameVersion))
                {
                    // Game found!
                    c.EnableRaisingEvents = true;
                    c.Exited += DarkSouls_Exited;
                    this.memory = new Memory(c);
                    return c;
                }
            }

            // Game not found
            return null;
        }

        private void DarkSouls_Exited(object sender, EventArgs e)
        {
            Unhook();
        }

        bool Hook()
        {
            darksouls = GetGame(out gameVersion);
            return (darksouls != null);
        }

        void Unhook()
        {
            darksouls = null;
            memory = null;
            gameVersion = GameVersion.Unknown;
        }

        bool IsValidDarkSoulsProcess(Process candidate, out GameVersion gameVersion)
        {
            Memory TmpMemory = new Memory(candidate);
            uint GameType = TmpMemory.RUInt32((IntPtr)Pointers.VERSION_CHECK);
            switch (GameType)
            {
                case Pointers.VERSION_RELEASE:
                    gameVersion = GameVersion.Release;
                    return true;
                case Pointers.VERSION_DEBUG:
                    gameVersion = GameVersion.Debug;
                    return true;
                default:
                    gameVersion = GameVersion.Unknown;
                    return false;                     
            }
        }
        #endregion


    }
}
