using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Livesplit.DarkSouls100Tracker.Logic
{
    public partial class Game
    {
        private enum GameState
        {
            Unhooked,
            CheckIfUpdatable,
            NeedsUnhookAndClean
        }
        private GameState gameState;
        private MemoryTools memoryTools;
        private GameProgress gameProgress;
        public event EventHandler OnGameProgressUpdated;

        private Thread mainThread;
        private CancellationTokenSource cancellationTokenSource;

        public Game()
        {
            memoryTools = new MemoryTools("DARK SOULS");
            gameState = GameState.Unhooked;

            mainThread = new Thread(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(Constants.Thread_Frequency);
                    Next();
                }
            })
            {
                IsBackground = true
            };

            gameProgress = new GameProgress(new List<Requirement>()
            {
                { new Requirement("Treasure Locations", 0.2, UpdatePickedUpItems) },
                { new Requirement("Bosses", 0.25, UpdateDefetedBosses) },
                { new Requirement("Non-respawning Enemies", 0.15, UpdateKilledNonRespawningEnemies) },
                { new Requirement("NPC Questlines", 0.2, UpdateCompletedQuestlines) },
                { new Requirement("Shortcuts / Locked Doors", 0.1, UpdateUnlockedShortcutsAndLockedDoors) },
                { new Requirement("Illusory Walls", 0.025, ReleavedIllusoryWalls) },
                { new Requirement("Foggates", 025, UpdateDissolvedFoggates) },
                { new Requirement("Kindled Bonfires", 0.05, UpdateFullyKindledBonfires) },
            });
        }

        public void Start()
        {
            if (!mainThread.IsAlive)
            {
                cancellationTokenSource = new CancellationTokenSource();
                mainThread.Start();
            }
        }

        public void Stop()
        {
            if (mainThread.IsAlive && cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        private void Next()
        {
            // MessageBox.Show(gameState.ToString());
            switch (gameState)
            {
                case GameState.Unhooked:
                    // Hooks the game and switch to CheckIfUpdatable if successful
                    if (memoryTools.Hook())
                    {
                        gameState = GameState.CheckIfUpdatable;
                    }
                    break;

                case GameState.CheckIfUpdatable:

                    // Checks IGT
                    int IGT = GetIngameTimeInMilliseconds();
                    bool InOwnWorld = IsPlayerInOwnWorld();

                    // If the player is in a loading screen, unhooks
                    if (IGT != 0 && !InOwnWorld)
                    {
                        gameState = GameState.NeedsUnhookAndClean;
                    }
                    // Do nothing if in the main menu
                    else if (IGT == 0 && !InOwnWorld)
                    {
                        // ...
                    }
                    // Otherwise we can test if updatable
                    else
                    {
                        UpdateAllRequirements();
                    }

                    break;

                case GameState.NeedsUnhookAndClean:
                    // Unhooks the game and clean the memory, switch to Unhooked if successful
                    if (memoryTools.UnHook())
                    {
                        gameState = GameState.Unhooked;
                    }
                    break;

                default:
                    // how the f did we get here
                    gameState = GameState.NeedsUnhookAndClean;
                    break;
            }
        }

        private bool IsPlayerInOwnWorld()
        {
            PlayerCharacterType t = GetPlayerCharacterType();
            return (t == PlayerCharacterType.Hollow || t == PlayerCharacterType.Human);
        }

        private bool IsPlayerLoaded()
        {
            IntPtr ptr = Dictionaries.PointersTypes[memoryTools.ExeType][PointerType.IsPlayerLoaded];
            if (ptr == IntPtr.Zero)
            {
                return false;
            }
            ptr = (IntPtr)memoryTools.RInt32(IntPtr.Add(ptr, 4));
            return (ptr != IntPtr.Zero);
        }

        private PlayerCharacterType GetPlayerCharacterType()
        {
            IntPtr ptr = Dictionaries.PointersTypes[memoryTools.ExeType][PointerType.GetPlayerCharacterType];
            if (ptr == IntPtr.Zero)
            {
                return PlayerCharacterType.None;
            }
            else
            {
                Int32 t = memoryTools.RInt32(ptr + 0xA28);
                return (Enum.IsDefined(typeof(PlayerCharacterType), t)) ? (PlayerCharacterType)t : PlayerCharacterType.None;
            }
        }

        private PlayerStartingClass GetPlayerStartingClass()
        {
            IntPtr ptr = Dictionaries.PointersTypes[memoryTools.ExeType][PointerType.GetPlayerStartingClass];
            ptr = (IntPtr)memoryTools.RInt32(ptr);
            if (ptr == IntPtr.Zero)
            {
                return PlayerStartingClass.None;
            }
            else
            {
                ptr = (IntPtr)memoryTools.RInt32(ptr + 8);
                if (ptr == IntPtr.Zero)
                    return PlayerStartingClass.None;
                else
                    return (PlayerStartingClass)memoryTools.RBytes(ptr + 0xC6, 1)[0];
            }
        }

        private int GetIngameTimeInMilliseconds()
        {
            IntPtr ptr = Dictionaries.PointersTypes[memoryTools.ExeType][PointerType.GetIngameTimeInMilliseconds];
            return memoryTools.RInt32((IntPtr)memoryTools.RInt32(ptr) + 0x68);
        }

        private int GetClearCount()
        {
            IntPtr ptr = Dictionaries.PointersTypes[memoryTools.ExeType][PointerType.GetClearCount];
            ptr = (IntPtr)memoryTools.RInt32(ptr);
            if (ptr == IntPtr.Zero)
                return -1;
            else
                return memoryTools.RInt32(IntPtr.Add(ptr, 0x3C));
        }
    }
}
