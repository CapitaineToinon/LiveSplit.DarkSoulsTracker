namespace LiveSplit.UI.Components
{
    partial class DetailedView
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetailedView));
            this.TrackerDataGrid = new System.Windows.Forms.DataGridView();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.TrackerDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // TrackerDataGrid
            // 
            this.TrackerDataGrid.AllowUserToAddRows = false;
            this.TrackerDataGrid.AllowUserToDeleteRows = false;
            this.TrackerDataGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TrackerDataGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.TrackerDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TrackerDataGrid.ColumnHeadersVisible = false;
            this.TrackerDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.count});
            this.TrackerDataGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.TrackerDataGrid.Location = new System.Drawing.Point(0, 0);
            this.TrackerDataGrid.MultiSelect = false;
            this.TrackerDataGrid.Name = "TrackerDataGrid";
            this.TrackerDataGrid.ReadOnly = true;
            this.TrackerDataGrid.RowHeadersVisible = false;
            this.TrackerDataGrid.RowTemplate.DefaultCellStyle.BackColor = System.Drawing.Color.White;
            this.TrackerDataGrid.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            this.TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.TrackerDataGrid.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.TrackerDataGrid.RowTemplate.Height = 25;
            this.TrackerDataGrid.RowTemplate.ReadOnly = true;
            this.TrackerDataGrid.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.TrackerDataGrid.Size = new System.Drawing.Size(357, 150);
            this.TrackerDataGrid.TabIndex = 0;
            // 
            // name
            // 
            this.name.HeaderText = "Column1";
            this.name.Name = "name";
            this.name.ReadOnly = true;
            // 
            // count
            // 
            this.count.HeaderText = "Column1";
            this.count.Name = "count";
            this.count.ReadOnly = true;
            // 
            // DetailedView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 150);
            this.Controls.Add(this.TrackerDataGrid);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DetailedView";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Detailed Tracker";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DetailedView_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.TrackerDataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView TrackerDataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn count;
    }
}

