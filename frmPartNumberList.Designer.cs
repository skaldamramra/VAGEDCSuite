namespace VAGSuite
{
    partial class frmPartNumberList
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPartNumberList));
            this.gridControl1 = new Zuby.ADGV.AdvancedDataGridView();
            this.groupControl1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.labelControl2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).BeginInit();
            this.groupControl1.Panel.SuspendLayout();
            this.groupControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.AllowUserToAddRows = false;
            this.gridControl1.AllowUserToDeleteRows = false;
            this.gridControl1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridControl1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.FilterAndSortEnabled = true;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MultiSelect = false;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.ReadOnly = true;
            this.gridControl1.RowHeadersVisible = false;
            this.gridControl1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridControl1.Size = new System.Drawing.Size(760, 400);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridControl1_CellFormatting);
            this.gridControl1.DoubleClick += new System.EventHandler(this.gridView1_DoubleClick);
            // 
            // groupControl1
            // 
            this.groupControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupControl1.Location = new System.Drawing.Point(12, 12);
            this.groupControl1.Name = "groupControl1";
            // 
            // groupControl1.Panel
            // 
            this.groupControl1.Panel.Controls.Add(this.gridControl1);
            this.groupControl1.Size = new System.Drawing.Size(764, 424);
            this.groupControl1.TabIndex = 1;
            this.groupControl1.Values.Heading = "Partnumber list";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.Location = new System.Drawing.Point(701, 442);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 2;
            this.simpleButton1.Values.Text = "Ok";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // labelControl2
            // 
            this.labelControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelControl2.Location = new System.Drawing.Point(12, 442);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(280, 20);
            this.labelControl2.TabIndex = 3;
            this.labelControl2.Values.Text = "Green rows indicate files present in your library";
            // 
            // simpleButton2
            // 
            this.simpleButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton2.Location = new System.Drawing.Point(620, 442);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 4;
            this.simpleButton2.Values.Text = "Export";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // frmPartNumberList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 479);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.groupControl1);
            this.Name = "frmPartNumberList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Partnumber list";
            this.Load += new System.EventHandler(this.frmPartNumberList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).EndInit();
            this.groupControl1.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Zuby.ADGV.AdvancedDataGridView gridControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox groupControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
    }
}