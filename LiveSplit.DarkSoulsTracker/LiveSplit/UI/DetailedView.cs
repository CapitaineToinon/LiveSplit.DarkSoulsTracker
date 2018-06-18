using System;
using System.Drawing;
using System.Windows.Forms;
using LiveSplit.TimeFormatters;

namespace LiveSplit.DarkSoulsTracker
{
    public partial class DetailedView : Form
    {
        // Things to make the form draggable
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public new event EventHandler OnClosed;
        public new event EventHandler OnLocationChanged;

        // Delegate to update the data grid view from a thread
        delegate void UpdateGridView(DarkSoulsProgress progress);

        private const int WindowHeaderHeight = 14;

        private DarkSoulsProgress gameProgress;
        public DarkSoulsProgress Progress
        {
            get { return gameProgress; }
            set
            {
                gameProgress = value;
                UpdateDataGridView(gameProgress);
            }
        }

        private bool showPercentage;
        private bool darkTheme;
        private TimeAccuracy accuracy;

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
                UpdateDataGridView(Progress);
            }
        }

        public bool DarkTheme
        {
            set
            {
                darkTheme = value;
                UpdateDataGridView(Progress);
            }
        }

        public TimeAccuracy Accuracy
        {
            set
            {
                accuracy = value;
                UpdateDataGridView(Progress);
            }
        }

        public DetailedView()
        {
            InitializeComponent();
            KeyPreview = true;
            //gameProgress = new DarkSoulsProgress();

            // Datagrid formatting
            TrackerDataGrid.Columns["name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            TrackerDataGrid.Columns["count"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            TrackerDataGrid.Columns["count"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            TrackerDataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            TrackerDataGrid.Columns[0].DefaultCellStyle.Font = new Font("Calibri", 14.0f, FontStyle.Bold);
            TrackerDataGrid.Columns[0].DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
            TrackerDataGrid.Columns[1].DefaultCellStyle.Font = new Font("Segoe UI", 14.0f, FontStyle.Bold);
            TrackerDataGrid.Columns[1].DefaultCellStyle.Padding = new Padding(0, 0, 2, 0);

            //UpdateDataGridView(GameProgress);
        }

        private void UpdateDataGridView(DarkSoulsProgress progress)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.TrackerDataGrid.InvokeRequired)
            {
                UpdateGridView d = new UpdateGridView(UpdateDataGridView);
                this.Invoke(d, new object[] { progress });
            }
            else
            {
                // Style
                if (darkTheme)
                {
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionForeColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.ForeColor = Color.White;
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionBackColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.BackColor =
                    TrackerDataGrid.BackgroundColor = Color.Black;
                }
                else
                {
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionForeColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.ForeColor = Color.Black;
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionBackColor =
                    TrackerDataGrid.RowTemplate.DefaultCellStyle.BackColor =
                    TrackerDataGrid.BackgroundColor = Color.White;
                }

                // Content
                this.TrackerDataGrid.Rows.Clear();
                foreach (Requirement r in progress.Requirements)
                {
                    TrackerDataGrid.Rows.Add(new string[] { r.Name, r.ToString() });
                }

                if (showPercentage)
                {
                    string[] row8 = { "Progression", PercentageFormatter.Format(progress.Percentage, accuracy) };
                    TrackerDataGrid.Rows.Add(row8);
                }

                if (TrackerDataGrid.Rows.Count >= 2)
                {
                    TrackerDataGrid.Height = (TrackerDataGrid.Rows[0].Height * (TrackerDataGrid.Rows.Count + 1));
                    this.Height = TrackerDataGrid.Size.Height + WindowHeaderHeight + 5;
                }

                // refresh
                TrackerDataGrid.Invalidate();
            }
        }

        private void DetailedView_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.OnClosed?.Invoke(this, EventArgs.Empty);
        }

        private void DetailedView_LocationChanged(object sender, EventArgs e)
        {
            this.OnLocationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void TrackerDataGrid_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}