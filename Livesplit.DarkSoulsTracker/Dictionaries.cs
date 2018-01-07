using System.Collections.Generic;

namespace Livesplit.DarkSoulsTracker
{
    public enum PlayerStartingClass
    {
        None = -1,
        Warrior = 0,
        Knight = 1,
        Wanderer = 2,
        Thief = 3,
        Bandit = 4,
        Hunter = 5,
        Sorcerer = 6,
        Pyromancer = 7,
        Cleric = 8,
        Deprived = 9
    }

    public enum PlayerCharacterType
    {
        None = -1,
        Human = 0,
        Summon = 1,
        Invader = 2,
        Hollow = 8
    }

    static class Dictionaries
    {
        // Dictionary for the starting items in Asylum. Key is the starting class, values are the starting item flags
        public static Dictionary<PlayerStartingClass, int[]> startingClassItems = new Dictionary<PlayerStartingClass, int[]>
            {
                { PlayerStartingClass.Warrior, new int[] { 51810110, 51810100 } },
                { PlayerStartingClass.Knight, new int[] { 51810130, 51810120 } },
                { PlayerStartingClass.Wanderer, new int[] { 51810150, 51810140 } },
                { PlayerStartingClass.Thief, new int[] { 51810170, 51810160 } },
                { PlayerStartingClass.Bandit, new int[] { 51810190, 51810180 } },
                { PlayerStartingClass.Hunter, new int[] { 51810210, 51810200, 51810220 } },
                { PlayerStartingClass.Sorcerer, new int[] { 51810240, 51810230, 51810250 } },
                { PlayerStartingClass.Pyromancer, new int[] { 51810270, 51810260, 51810280 } },
                { PlayerStartingClass.Cleric, new int[] { 51810300, 51810290, 51810310 } },
                { PlayerStartingClass.Deprived, new int[] { 51810330, 51810320 } }
            };

        // Dictionary for the dropped item flags of each NPC. Key is the dead flag for each NPC, values are the dropped item flags
        public static Dictionary<int, int[]> npcDroppedItems = new Dictionary<int, int[]>
            {
                // Permanent dropped items from NPCs
                { 1322, new int[] { 51010990 } }, // Andre
                { 1362, new int[] { 51510940 } }, // 'Giant Blacksmith
                { 1342, new int[] { 51300990, 51300991 } },// 'Vamos
                { 1097, new int[] { 50007031, 50007030 } },// 'Logan
                { 1284, new int[] { 51400980 } }, // 'Eingyi
                { 1872, new int[] { 50000520 } }, // 'Elizabeth
                { 1115, new int[] { 50006041, 50006040 } }, // 'Griggs
                { 1823, new int[] { 50000510, 50000511 } },// 'Gough
                { 1034, new int[] { 50006010 } }, // 'Darkling Lady
                { 1842, new int[] { 51210990 } }, // 'Chester
                { 1702, new int[] { 11607020, 50006371 } }, // 'Oswald
                { 1628, new int[] { 50006320, 50006321 } }, // 'Patches
                { 1198, new int[] { 50006080, 50006081 } }, // 'Petrus
                { 1295, new int[] { 50000300 } }, // 'Quelana
                { 1177, new int[] { 50006070, 50006072 } }, // 'Rhea
                { 1604, new int[] { 50006310, 50006311 } },// 'Shiva
                { 1764, new int[] { 50006420, 50006421 } }, // 'Shiva's Bodyguard
                { 1402, new int[] { 51010960 } }, // 'Undead Merchant (Male)
                { 1272, new int[] { 51400990 } },// 'Fair Lady
                { 1864, new int[] { 50000501, 50000500 } }, // Ciaran
                { 1513, new int[] { 50006280, 50000070 } },//  'Siegmeyer
                { 11200818, new int[] { 50008000, 50008001 } }, // 'Pharis
                { 11210021, new int[] { 51210910 } }, //'Sif rescued in DLC
                { 11210681, new int[] { 51210921 } }, //'Carving Mimic in DLC
                { 11210680, new int[] { 51210981 } }, //'Crest Key Mimic in DLC
                { 800, new int[] { 51410990 } }, //'Sunlight Maggot Chaos Firebug
                { 1062, new int[] { 50007020 } } //'Hollowed Oscar
            };

        // Dictionary for the hostile flags of each NPC that isn't tied to a questline. Key is just an index, values are the hostile and dead flags
        public static List<int[]> npcHostileDeadFlags = new List<int[]>
            {
                // NPCs not tied to questlines
                new int[] { 1321, 1322 }, // 'Andre
                new int[] { 1361, 1362 }, // 'Giant Blacksmith
                new int[] { 1341, 1342 }, // 'Vamos
                new int[] { 1283, 1284 }, // 'Eingyi
                new int[] { 1822, 1823 }, // 'Gough
                new int[] { 1841, 1842 }, // 'Chester
                new int[] { 1701, 1702 }, // 'Oswald
                new int[] { 1197, 1198 }, // 'Petrus
                new int[] { 1294, 1295 }, // 'Quelana
                new int[] { 1763, 1764 }, // 'Shiva's Bodyguard
                new int[] { 1401, 1402 }, // 'Undead Merchant (Male)
                new int[] { 1434, 1435 } // 'Domhnall
            };

        // Dictionary for treasure locations that have multiple pickups/event flags associated with it. Key is the first flag, values are the remaining flags
        public static Dictionary<int, int[]> sharedTreasureLocationItems = new Dictionary<int, int[]>
            {
                { 50001030, new int[] { 50001031 } }, //  'Dingy Set + Black Eye Orb
                { 51010380, new int[] { 51010381 } }, //  'Thief Set + Target Shield
                { 51010510, new int[] { 51010511 } }, //  'Sorcerer Set + Sorcerer Catalyst
                { 51200020, new int[] { 51200021 } }, //  'Hunter Set + Longbow
                { 51200140, new int[] { 51200141, 51200142 } }, // 'Divine Ember + Watchtower Basement Key + Homeward Bone
                { 51300190, new int[] { 51300191 } }, //  'Cleric Set + Mace
                { 51400190, new int[] { 51400191 } }, //  'Crimson Set + Tin Banishment Catalyst
                { 51400270, new int[] { 51000271 } }, //  'Poison Mist + Pyromancer Set
                { 51400310, new int[] { 51400311 } }, //  'Wanderer Set + Falchion
                { 51500060, new int[] { 51500061 } }, //  'Black Sorcerer Set + Hush
                { 51510070, new int[] { 51510071 } }, //  'Black Iron Set + Greatsword/Black Iron Greatshield
                { 51600220, new int[] { 51600221 } }, //  'Bandit Set + Spider Shield
                { 51600360, new int[] { 51600361 } }, //  'Witch Set + Beatrice's Catalyst
                { 51700070, new int[] { 51700071 } }, //  'Maiden Set + White Seance Ring
                { 51700640, new int[] { 51700641 } } //  'Sage Set + Logan's Catalyst
            };
    }
}
