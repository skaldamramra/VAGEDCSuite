namespace VAGSuite
{
    partial class frmTransactionLog
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTransactionLog));
            this.groupControl1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.gridControl1 = new Zuby.ADGV.AdvancedDataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rolllBackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rollForwardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton3 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton4 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).BeginInit();
            this.groupControl1.Panel.SuspendLayout();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            //
            // groupControl1
            //
            this.groupControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            //
            // groupControl1.Panel
            //
            this.groupControl1.Panel.Controls.Add(this.gridControl1);
            this.groupControl1.Location = new System.Drawing.Point(13, 10);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(519, 361);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Values.Heading = "Logged transactions";
            //
            // gridControl1
            //
            this.gridControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(515, 338);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ReadOnly = true;
            this.gridControl1.AllowUserToAddRows = false;
            this.gridControl1.AllowUserToDeleteRows = false;
            this.gridControl1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridControl1.MultiSelect = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rolllBackToolStripMenuItem,
            this.rollForwardToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 70);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // rolllBackToolStripMenuItem
            // 
            this.rolllBackToolStripMenuItem.Name = "rolllBackToolStripMenuItem";
            this.rolllBackToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rolllBackToolStripMenuItem.Text = "Roll back";
            this.rolllBackToolStripMenuItem.Click += new System.EventHandler(this.rolllBackToolStripMenuItem_Click);
            // 
            // rollForwardToolStripMenuItem
            // 
            this.rollForwardToolStripMenuItem.Name = "rollForwardToolStripMenuItem";
            this.rollForwardToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rollForwardToolStripMenuItem.Text = "Roll forward";
            this.rollForwardToolStripMenuItem.Click += new System.EventHandler(this.rollForwardToolStripMenuItem_Click);
            this.gridControl1.SelectionChanged += new System.EventHandler(this.gridControl1_SelectionChanged);
            this.gridControl1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridControl1_CellValueChanged);
            this.gridControl1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.gridControl1_CellPainting);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.Location = new System.Drawing.Point(455, 377);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 23);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Values.Text = "Ok";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            //
            // simpleButton2
            //
            this.simpleButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton2.Location = new System.Drawing.Point(374, 377);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 23);
            this.simpleButton2.TabIndex = 2;
            this.simpleButton2.Values.Text = "Details";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            //
            // simpleButton3
            //
            this.simpleButton3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton3.Location = new System.Drawing.Point(13, 377);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(85, 23);
            this.simpleButton3.TabIndex = 3;
            this.simpleButton3.Values.Text = "Roll back";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            //
            // simpleButton4
            //
            this.simpleButton4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton4.Location = new System.Drawing.Point(104, 377);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(85, 23);
            this.simpleButton4.TabIndex = 4;
            this.simpleButton4.Values.Text = "Roll forward";
            this.simpleButton4.Click += new System.EventHandler(this.simpleButton4_Click);
            // 
            // frmTransactionLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 412);
            this.Controls.Add(this.simpleButton4);
            this.Controls.Add(this.simpleButton3);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.groupControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmTransactionLog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Transaction log";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.frmTransactionLog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).EndInit();
            this.groupControl1.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox groupControl1;
        private Zuby.ADGV.AdvancedDataGridView gridControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rolllBackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rollForwardToolStripMenuItem;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton3;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton4;
    }
}