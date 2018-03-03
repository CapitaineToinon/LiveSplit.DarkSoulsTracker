using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Livesplit.DarkSouls100PercentTracker
{
    public partial class DetailedView : Form
    {
        public new event EventHandler OnClosed;

        private int[] defeatedBossesCount;
        private int[] itemsPickedUp;
        private int[] dissolvedFoggatesCount;
        private int[] fullyKindledBonfires;
        private int[] revealedIllusoryWallsCount;
        private int[] unlockedShortcutsAndLockedDoorsCount;
        private int[] completedQuestlinesCount;
        private int[] killedNonRespawningEnemiesCount;
        private string percentage;

        public DetailedView()
        {
            InitializeComponent();
        }

        private void FormatLabel(Label l, int[] val)
        {
            l.Text = string.Format("{0}/{1}", val[0], val[1]);
        }

        public int[] DefeatedBossesCount { get => defeatedBossesCount;
            set
            {
                defeatedBossesCount = value;
                FormatLabel(bossesKilledValueLabel, defeatedBossesCount);
            }
        }
        public int[] ItemsPickedUp { get => itemsPickedUp;
            set
            {
                itemsPickedUp = value;
                FormatLabel(treasureLocationsValueLabel, itemsPickedUp);
            }
        }
        public int[] DissolvedFoggatesCount { get => dissolvedFoggatesCount;
            set
            {
                dissolvedFoggatesCount = value;
                FormatLabel(foggatesValueLabel, dissolvedFoggatesCount);
            }
        }
        public int[] FullyKindledBonfires { get => fullyKindledBonfires;
            set
            {
                fullyKindledBonfires = value;
                FormatLabel(bonfiresValueLabel, fullyKindledBonfires);
            }
        }
        public int[] RevealedIllusoryWallsCount { get => revealedIllusoryWallsCount;
            set
            {
                revealedIllusoryWallsCount = value;
                FormatLabel(illusoryWallsValueLabel, revealedIllusoryWallsCount);
            }
        }
        public int[] UnlockedShortcutsAndLockedDoorsCount { get => unlockedShortcutsAndLockedDoorsCount;
            set
            {
                unlockedShortcutsAndLockedDoorsCount = value;
                FormatLabel(shortcutsValueLabel, unlockedShortcutsAndLockedDoorsCount);
            }
        }
        public int[] CompletedQuestlinesCount { get => completedQuestlinesCount;
            set
            {
                completedQuestlinesCount = value;
                FormatLabel(npcQuestlinesValueLabel, completedQuestlinesCount);
            }
        }
        public int[] KilledNonRespawningEnemiesCount { get => killedNonRespawningEnemiesCount;
            set
            {
                killedNonRespawningEnemiesCount = value;
                FormatLabel(nonRespawningEnemiesValueLabel, killedNonRespawningEnemiesCount);
            }
        }

        public string Percentage { get => percentage;
            set
            {
                percentage = value;
                percentageLabel.Text = percentage;
            }
        }

        private void DetailedView_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.OnClosed(this, EventArgs.Empty);
        }
    }
}
