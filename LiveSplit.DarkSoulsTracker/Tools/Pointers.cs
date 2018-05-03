using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.DarkSoulsTracker.Tools
{
    internal static class Pointers
    {
        #region GameVersion
        public const uint VERSION_CHECK = 0x400080;
        public const uint VERSION_RELEASE = 0xFC293654;
        public const uint VERSION_DEBUG = 0xCE9634B4;
        #endregion

        #region Flags
        public const int GET_EVENT_FLAG = 0xD60340;
        #endregion

        #region CharData
        public const int CharData1Ptr = 0x137DC70;
        public const int CharData1Ptr2 = 0x4;
        public const int CharData1Ptr3 = 0x0;
        #endregion

        public static Dictionary<GameVersion, int> EventFlagPtr
        {
            get
            {
                return new Dictionary<GameVersion, int>
                {
                    { GameVersion.Release, 0x137D7D4 },
                    { GameVersion.Debug, 0x1381994 },
                };
            }
        }

        public static Dictionary<GameVersion, IntPtr> EventFlagValues
        {
            get
            {
                return new Dictionary<GameVersion, IntPtr>
                {
                    { GameVersion.Release, new IntPtr(0xD60340) },
                    { GameVersion.Debug, new IntPtr(0xD618D0) },
                };
            }
        }

        // PointerType that needs a pointer  
        public static Dictionary<GameVersion, Dictionary<PointerType, IntPtr>> PointersTypes
        {
            get
            {
                return new Dictionary<GameVersion, Dictionary<PointerType, IntPtr>>
                {
                    { GameVersion.Release, new Dictionary<PointerType, IntPtr>
                        {
                            { PointerType.updateFullyKindledBonfires,   (IntPtr)0x137E204 },
                            { PointerType.GetClearCount,                (IntPtr)0x1378700 },
                            { PointerType.IsPlayerLoaded,               (IntPtr)0x137DC70 },
                            { PointerType.GetPlayerStartingClass,       (IntPtr)0x1378700 },
                            { PointerType.GetPlayerCharacterType,       (IntPtr)0x137E204 },
                            { PointerType.GetIngameTimeInMilliseconds,  (IntPtr)0x1378700 },
                        }
                    },
                    { GameVersion.Debug, new Dictionary<PointerType, IntPtr>
                        {
                            { PointerType.updateFullyKindledBonfires,   (IntPtr)0x13823C4 },
                            { PointerType.GetClearCount,                (IntPtr)0x137C8C0 },
                            { PointerType.IsPlayerLoaded,               (IntPtr)0x1381E30 },
                            { PointerType.GetPlayerStartingClass,       (IntPtr)0x137C8C0 },
                            { PointerType.GetPlayerCharacterType,       (IntPtr)0x13823C4 },
                            { PointerType.GetIngameTimeInMilliseconds,  (IntPtr)0x137C8C0 },
                        }
                    },
                };
            }
        }
    }
}
