using LiveSplit.UI;
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

        private int[] defeatedBossesCount = new int[] { 0, 1 };
        private int[] itemsPickedUp = new int[] { 0, 1 };
        private int[] dissolvedFoggatesCount = new int[] { 0, 1 };
        private int[] fullyKindledBonfires = new int[] { 0, 1 };
        private int[] revealedIllusoryWallsCount = new int[] { 0, 1 };
        private int[] unlockedShortcutsAndLockedDoorsCount = new int[] { 0, 1 };
        private int[] completedQuestlinesCount = new int[] { 0, 1 };
        private int[] killedNonRespawningEnemiesCount = new int[] { 0, 1 };
        private string percentage = "-";

        public Color BackgroundColor
        {
            set
            {
                if (TrackerDataGrid != null)
                {
                    TrackerDataGrid.BackgroundColor = value;
                    TrackerDataGrid.RowsDefaultCellStyle.BackColor = value;
                    TrackerDataGrid.RowsDefaultCellStyle.SelectionBackColor = value;
                }
            }
        }

        public SimpleLabel TextFont
        {
            set
            {
                if (TrackerDataGrid != null)
                {
                    TrackerDataGrid.Columns["name"].DefaultCellStyle.Font = value.Font;
                    TrackerDataGrid.Columns["name"].DefaultCellStyle.ForeColor = value.ForeColor;
                }
            }
        }

        public SimpleLabel TimesFont
        {
            set
            {
                if (TrackerDataGrid != null)
                {
                    TrackerDataGrid.Columns["count"].DefaultCellStyle.Font = value.Font;
                    TrackerDataGrid.Columns["count"].DefaultCellStyle.ForeColor = value.ForeColor;

                }
            }
        }

        public DetailedView()
        {
            InitializeComponent();

            // Datagrid formatting
            TrackerDataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TrackerDataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TrackerDataGrid.Columns["count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            TrackerDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            UpdateDataGridView();

            this.Height = TrackerDataGrid.Height;
        }

        private void UpdateDataGridView()
        {
            TrackerDataGrid.Rows.Clear();
            string[] row0 = { "Treasure Locations", FormatString(itemsPickedUp) };
            string[] row1 = { "Bosses", FormatString(defeatedBossesCount) };
            string[] row2 = { "Non-respawning Enemies", FormatString(killedNonRespawningEnemiesCount) };
            string[] row3 = { "NPC Questlines", FormatString(completedQuestlinesCount) };
            string[] row4 = { "Shortcuts / Locked Doors", FormatString(unlockedShortcutsAndLockedDoorsCount) };
            string[] row5 = { "Illusory Walls", FormatString(revealedIllusoryWallsCount) };
            string[] row6 = { "Foggates", FormatString(dissolvedFoggatesCount) };
            string[] row7 = { "Kindled Bonfires", FormatString(fullyKindledBonfires) };
            string[] row8 = { "Progression", percentage };

            TrackerDataGrid.Rows.Add(row0);
            TrackerDataGrid.Rows.Add(row1);
            TrackerDataGrid.Rows.Add(row2);
            TrackerDataGrid.Rows.Add(row3);
            TrackerDataGrid.Rows.Add(row4);
            TrackerDataGrid.Rows.Add(row5);
            TrackerDataGrid.Rows.Add(row6);
            TrackerDataGrid.Rows.Add(row7);
            TrackerDataGrid.Rows.Add(row8);
        }

        private string FormatString(int[] val)
        {
            if (val == null)
                val = new int[] { 0, 1 };

            return string.Format("{0}/{1}", val[0], val[1]);
        }

        public int[] DefeatedBossesCount { get => defeatedBossesCount;
            set
            {
                 defeatedBossesCount = value;
            }
        }
        public int[] ItemsPickedUp { get => itemsPickedUp;
            set
            {
                itemsPickedUp = value;
            }
        }
        public int[] DissolvedFoggatesCount { get => dissolvedFoggatesCount;
            set
            {
                dissolvedFoggatesCount = value;
            }
        }
        public int[] FullyKindledBonfires { get => fullyKindledBonfires;
            set
            {
                fullyKindledBonfires = value;
            }
        }
        public int[] RevealedIllusoryWallsCount { get => revealedIllusoryWallsCount;
            set
            {
                revealedIllusoryWallsCount = value;
            }
        }
        public int[] UnlockedShortcutsAndLockedDoorsCount { get => unlockedShortcutsAndLockedDoorsCount;
            set
            {
                unlockedShortcutsAndLockedDoorsCount = value;
            }
        }
        public int[] CompletedQuestlinesCount { get => completedQuestlinesCount;
            set
            {
                completedQuestlinesCount = value;
            }
        }
        public int[] KilledNonRespawningEnemiesCount { get => killedNonRespawningEnemiesCount;
            set
            {
                killedNonRespawningEnemiesCount = value;
            }
        }

        public string Percentage { get => percentage;
            set
            {
                // Only updates the UI if the percentage changed, to avoid flickering
                if (value != percentage)
                {
                    percentage = value;
                    UpdateDataGridView();
                }
            }
        }

        private void DetailedView_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.OnClosed(this, EventArgs.Empty);
        }
    }
}
