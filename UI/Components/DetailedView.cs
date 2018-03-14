using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Livesplit.DarkSouls100Tracker.Logic;

namespace LiveSplit.UI.Components
{
    public partial class DetailedView : Form
    {
        public new event EventHandler OnClosed;
        public new event EventHandler OnLocationChanged;

        // Delegate to update the data grid view from a thread
        delegate void UpdateGridView();

        private const int WindowHeaderHeight = 14;

        private GameProgress gameProgress;
        public GameProgress GameProgress
        {
            get { return gameProgress; }
            set
            {
                if (gameProgress.Percentage != value.Percentage)
                {
                    MessageBox.Show("Updated : " + value.PercentageString);
                    gameProgress = value;
                    UpdateDataGridView();
                }
            }
        }

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
            gameProgress = new GameProgress();

            // Datagrid formatting
            TrackerDataGrid.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TrackerDataGrid.Columns["count"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TrackerDataGrid.Columns["count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            TrackerDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            TrackerDataGrid.Columns[0].DefaultCellStyle.Font = new Font("Calibri", 14.0f, FontStyle.Bold);
            TrackerDataGrid.Columns[1].DefaultCellStyle.Font = new Font("Segoe UI", 14.0f, FontStyle.Bold);

            UpdateDataGridView();
        }

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
                this.TrackerDataGrid.Rows.Clear();
                foreach (Requirement r in GameProgress.Requirements)
                {
                    TrackerDataGrid.Rows.Add(new string[] { r.Name, FormatString(r.Progression) });
                }

                if (showPercentage)
                {
                    string[] row8 = { "Progression", GameProgress.PercentageString };
                    TrackerDataGrid.Rows.Add(row8);
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

        private void DetailedView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (OnClosed != null)
                this.OnClosed(this, EventArgs.Empty);
        }

        private void DetailedView_LocationChanged(object sender, EventArgs e)
        {
            if (OnLocationChanged != null)
                this.OnLocationChanged(this, EventArgs.Empty);
        }
    }
}
