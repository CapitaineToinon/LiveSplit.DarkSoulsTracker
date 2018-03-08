using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public partial class DetailedView : Form
    {
        public new event EventHandler OnClosed;

        private const int WindowHeaderHeight = 14;

        private int[] defeatedBossesCount = new int[] { 0, 1 };
        private int[] itemsPickedUp = new int[] { 0, 1 };
        private int[] dissolvedFoggatesCount = new int[] { 0, 1 };
        private int[] fullyKindledBonfires = new int[] { 0, 1 };
        private int[] revealedIllusoryWallsCount = new int[] { 0, 1 };
        private int[] unlockedShortcutsAndLockedDoorsCount = new int[] { 0, 1 };
        private int[] completedQuestlinesCount = new int[] { 0, 1 };
        private int[] killedNonRespawningEnemiesCount = new int[] { 0, 1 };
        private string stringpercentage = "-";
        private double percentage = 0;

        private bool showPercentage;
        private bool darkTheme;

        public int FormTop
        {
            get
            {
                return Top;
            }
            set
            {
                Top = value;
            }
        }

        public int FormLeft
        {
            get
            {
                return Left;
            }
            set
            {
                Left = value;
            }
        }

        public bool ShowPercentage
        {
            set
            {
                showPercentage = value;
                UpdateDataGridView();
            }
        }

        public bool DarkTheme
        {
            set
            {
                darkTheme = value;
                UpdateDataGridView();
            }
        }

        public DetailedView()
        {
            InitializeComponent();

            // Datagrid formatting
            TrackerDataGrid.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TrackerDataGrid.Columns["count"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TrackerDataGrid.Columns["count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            TrackerDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            TrackerDataGrid.Columns[0].DefaultCellStyle.Font = new Font("Calibri", 14.0f, FontStyle.Bold);
            TrackerDataGrid.Columns[1].DefaultCellStyle.Font = new Font("Segoe UI", 14.0f, FontStyle.Bold);

            UpdateDataGridView();
        }

        // Delegate pour update le textbox de la form
        delegate void UpdateGridView();

        private void UpdateDataGridView()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TrackerDataGrid.InvokeRequired)
            {
                UpdateGridView d = new UpdateGridView(UpdateDataGridView);
                this.Invoke(d, new object[] { });
            }
            else
            {
                // Style
                if (darkTheme)
                {
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionForeColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.ForeColor = Color.White;
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionBackColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.BackColor = Color.Black;
                }
                else
                {
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionForeColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.ForeColor = Color.Black;
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionBackColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.BackColor = Color.White;
                }

                // Content
                List<string[]> tmp = new List<string[]>();
                string[] row0 = { "Treasure Locations", FormatString(itemsPickedUp) };
                string[] row1 = { "Bosses", FormatString(defeatedBossesCount) };
                string[] row2 = { "Non-respawning Enemies", FormatString(killedNonRespawningEnemiesCount) };
                string[] row3 = { "NPC Questlines", FormatString(completedQuestlinesCount) };
                string[] row4 = { "Shortcuts / Locked Doors", FormatString(unlockedShortcutsAndLockedDoorsCount) };
                string[] row5 = { "Illusory Walls", FormatString(revealedIllusoryWallsCount) };
                string[] row6 = { "Foggates", FormatString(dissolvedFoggatesCount) };
                string[] row7 = { "Kindled Bonfires", FormatString(fullyKindledBonfires) };

                tmp.Add(row0);
                tmp.Add(row1);
                tmp.Add(row2);
                tmp.Add(row3);
                tmp.Add(row4);
                tmp.Add(row5);
                tmp.Add(row6);
                tmp.Add(row7);

                if (showPercentage)
                {
                    string[] row8 = { "Progression", stringpercentage };
                    tmp.Add(row8);
                }

                this.TrackerDataGrid.Rows.Clear();
                foreach (string[] row in tmp)
                {
                    TrackerDataGrid.Rows.Add(row);
                }

                TrackerDataGrid.Height = (TrackerDataGrid.Rows[0].Height * (TrackerDataGrid.Rows.Count + 1));
                this.Height = TrackerDataGrid.Size.Height + WindowHeaderHeight;

                // refresh
                TrackerDataGrid.Invalidate();
            }
        }

        private string FormatString(int[] val)
        {
            if (val == null)
                val = new int[] { 0, 1 };

            return string.Format("{0}/{1}", val[0], val[1]);
        }

        public int[] DefeatedBossesCount
        {
            get => defeatedBossesCount;
            set
            {
                defeatedBossesCount = value;
            }
        }
        public int[] ItemsPickedUp
        {
            get => itemsPickedUp;
            set
            {
                itemsPickedUp = value;
            }
        }
        public int[] DissolvedFoggatesCount
        {
            get => dissolvedFoggatesCount;
            set
            {
                dissolvedFoggatesCount = value;
            }
        }
        public int[] FullyKindledBonfires
        {
            get => fullyKindledBonfires;
            set
            {
                fullyKindledBonfires = value;
            }
        }
        public int[] RevealedIllusoryWallsCount
        {
            get => revealedIllusoryWallsCount;
            set
            {
                revealedIllusoryWallsCount = value;
            }
        }
        public int[] UnlockedShortcutsAndLockedDoorsCount
        {
            get => unlockedShortcutsAndLockedDoorsCount;
            set
            {
                unlockedShortcutsAndLockedDoorsCount = value;
            }
        }
        public int[] CompletedQuestlinesCount
        {
            get => completedQuestlinesCount;
            set
            {
                completedQuestlinesCount = value;
            }
        }
        public int[] KilledNonRespawningEnemiesCount
        {
            get => killedNonRespawningEnemiesCount;
            set
            {
                killedNonRespawningEnemiesCount = value;
            }
        }

        public string StringPercentage
        {
            get => stringpercentage;
            set
            {
                stringpercentage = value;
            }
        }

        public double Percentage
        {
            get => percentage;
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
