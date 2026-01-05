namespace VAGSuite
{
    partial class CompareResults
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gridControl1 = new Zuby.ADGV.AdvancedDataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showDifferenceMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveLayoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gridColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridColumn14 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gcMissingInOriFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gcMissingInCompareFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.AllowUserToAddRows = false;
            this.gridControl1.AllowUserToDeleteRows = false;
            this.gridControl1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridControl1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gridColumn1,
            this.gridColumn2,
            this.gridColumn3,
            this.gridColumn4,
            this.gridColumn5,
            this.gridColumn6,
            this.gridColumn7,
            this.gridColumn8,
            this.gridColumn9,
            this.gridColumn10,
            this.gridColumn11,
            this.gridColumn12,
            this.gridColumn13,
            this.gridColumn14,
            this.gcMissingInOriFile,
            this.gcMissingInCompareFile});
            this.gridControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.ReadOnly = true;
            this.gridControl1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridControl1.Size = new System.Drawing.Size(728, 487);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridControl1_CellDoubleClick);
            this.gridControl1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridControl1_CellFormatting);
            this.gridControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridControl1_KeyDown);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDifferenceMapToolStripMenuItem,
            this.exportToExcelToolStripMenuItem,
            this.saveLayoutToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(192, 70);
            // 
            // showDifferenceMapToolStripMenuItem
            // 
            this.showDifferenceMapToolStripMenuItem.Name = "showDifferenceMapToolStripMenuItem";
            this.showDifferenceMapToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.showDifferenceMapToolStripMenuItem.Text = "Show differences map";
            this.showDifferenceMapToolStripMenuItem.Click += new System.EventHandler(this.showDifferenceMapToolStripMenuItem_Click);
            // 
            // exportToExcelToolStripMenuItem
            // 
            this.exportToExcelToolStripMenuItem.Enabled = false;
            this.exportToExcelToolStripMenuItem.Name = "exportToExcelToolStripMenuItem";
            this.exportToExcelToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.exportToExcelToolStripMenuItem.Text = "Export to Excel";
            this.exportToExcelToolStripMenuItem.Click += new System.EventHandler(this.exportToExcelToolStripMenuItem_Click);
            // 
            // saveLayoutToolStripMenuItem
            // 
            this.saveLayoutToolStripMenuItem.Enabled = false;
            this.saveLayoutToolStripMenuItem.Name = "saveLayoutToolStripMenuItem";
            this.saveLayoutToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.saveLayoutToolStripMenuItem.Text = "Save layout";
            this.saveLayoutToolStripMenuItem.Click += new System.EventHandler(this.saveLayoutToolStripMenuItem_Click);
            // 
            // gridColumn1
            // 
            this.gridColumn1.HeaderText = "Symbol";
            this.gridColumn1.Name = "gridColumn1";
            this.gridColumn1.ReadOnly = true;
            this.gridColumn1.DataPropertyName = "SYMBOLNAME";
            this.gridColumn1.Width = 300;
            // 
            // gridColumn2
            // 
            this.gridColumn2.HeaderText = "SRAM address";
            this.gridColumn2.Name = "gridColumn2";
            this.gridColumn2.ReadOnly = true;
            this.gridColumn2.DataPropertyName = "SRAMADDRESS";
            // 
            // gridColumn3
            // 
            this.gridColumn3.HeaderText = "Flash address";
            this.gridColumn3.Name = "gridColumn3";
            this.gridColumn3.ReadOnly = true;
            this.gridColumn3.DataPropertyName = "FLASHADDRESS";
            // 
            // gridColumn4
            // 
            this.gridColumn4.HeaderText = "Length (bytes)";
            this.gridColumn4.Name = "gridColumn4";
            this.gridColumn4.ReadOnly = true;
            this.gridColumn4.DataPropertyName = "LENGTHBYTES";
            // 
            // gridColumn5
            // 
            this.gridColumn5.HeaderText = "Length (values)";
            this.gridColumn5.Name = "gridColumn5";
            this.gridColumn5.ReadOnly = true;
            this.gridColumn5.DataPropertyName = "LENGTHVALUES";
            // 
            // gridColumn6
            // 
            this.gridColumn6.HeaderText = "Description";
            this.gridColumn6.Name = "gridColumn6";
            this.gridColumn6.ReadOnly = true;
            this.gridColumn6.DataPropertyName = "DESCRIPTION";
            this.gridColumn6.Width = 200;
            // 
            // gridColumn7
            // 
            this.gridColumn7.HeaderText = "% Different";
            this.gridColumn7.Name = "gridColumn7";
            this.gridColumn7.ReadOnly = true;
            this.gridColumn7.DataPropertyName = "DIFFPERCENTAGE";
            // 
            // gridColumn8
            // 
            this.gridColumn8.HeaderText = "Values Different";
            this.gridColumn8.Name = "gridColumn8";
            this.gridColumn8.ReadOnly = true;
            this.gridColumn8.DataPropertyName = "DIFFABSOLUTE";
            // 
            // gridColumn9
            // 
            this.gridColumn9.HeaderText = "Avg Difference";
            this.gridColumn9.Name = "gridColumn9";
            this.gridColumn9.ReadOnly = true;
            this.gridColumn9.DataPropertyName = "DIFFAVERAGE";
            // 
            // gridColumn10
            // 
            this.gridColumn10.HeaderText = "Category";
            this.gridColumn10.Name = "gridColumn10";
            this.gridColumn10.ReadOnly = true;
            this.gridColumn10.DataPropertyName = "CATEGORYNAME";
            // 
            // gridColumn11
            // 
            this.gridColumn11.HeaderText = "Subcategory";
            this.gridColumn11.Name = "gridColumn11";
            this.gridColumn11.ReadOnly = true;
            this.gridColumn11.DataPropertyName = "SUBCATEGORYNAME";
            // 
            // gridColumn12
            // 
            this.gridColumn12.HeaderText = "Symbol #1";
            this.gridColumn12.Name = "gridColumn12";
            this.gridColumn12.ReadOnly = true;
            this.gridColumn12.DataPropertyName = "SymbolNumber1";
            // 
            // gridColumn13
            // 
            this.gridColumn13.HeaderText = "Symbol #2";
            this.gridColumn13.Name = "gridColumn13";
            this.gridColumn13.ReadOnly = true;
            this.gridColumn13.DataPropertyName = "SymbolNumber2";
            // 
            // gridColumn14
            // 
            this.gridColumn14.HeaderText = "User Description";
            this.gridColumn14.Name = "gridColumn14";
            this.gridColumn14.ReadOnly = true;
            this.gridColumn14.DataPropertyName = "Userdescription";
            // 
            // gcMissingInOriFile
            // 
            this.gcMissingInOriFile.HeaderText = "Missing in Ori";
            this.gcMissingInOriFile.Name = "gcMissingInOriFile";
            this.gcMissingInOriFile.ReadOnly = true;
            this.gcMissingInOriFile.DataPropertyName = "MissingInOriFile";
            this.gcMissingInOriFile.Visible = false;
            // 
            // gcMissingInCompareFile
            // 
            this.gcMissingInCompareFile.HeaderText = "Missing in Compare";
            this.gcMissingInCompareFile.Name = "gcMissingInCompareFile";
            this.gcMissingInCompareFile.ReadOnly = true;
            this.gcMissingInCompareFile.DataPropertyName = "MissingInCompareFile";
            this.gcMissingInCompareFile.Visible = false;
            // 
            // CompareResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl1);
            this.Name = "CompareResults";
            this.Size = new System.Drawing.Size(728, 487);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public Zuby.ADGV.AdvancedDataGridView gridControl1;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn8;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn9;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem showDifferenceMapToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn10;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn11;
        private System.Windows.Forms.ToolStripMenuItem exportToExcelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveLayoutToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn12;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn13;
        private System.Windows.Forms.DataGridViewTextBoxColumn gridColumn14;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcMissingInOriFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcMissingInCompareFile;
    }
}
