namespace VAGSuite
{
    partial class ctrlAirmassResult
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
            this.xtraTabControl1 = new ComponentFactory.Krypton.Navigator.KryptonNavigator();
            this.xtraTabPage1 = new ComponentFactory.Krypton.Navigator.KryptonPage();
            this.gridControl1 = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            this.xtraTabPage2 = new ComponentFactory.Krypton.Navigator.KryptonPage();
            this.checkEdit9 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.checkEdit8 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.chartControl1 = new ZedGraph.ZedGraphControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupControl2 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.comboBoxEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.checkEdit6 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.checkEdit5 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.labelControl13 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl14 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl10 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl9 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.spinEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown();
            this.labelControl7 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl8 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl4 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl6 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.comboBoxEdit2 = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.checkEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.simpleButton3 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).BeginInit();
            this.xtraTabControl1.SuspendLayout();
            this.xtraTabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.xtraTabPage2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2.Panel)).BeginInit();
            this.groupControl2.Panel.SuspendLayout();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit2)).BeginInit();
            // spinEdit1 (KryptonNumericUpDown) does not support ISupportInitialize
            this.SuspendLayout();
            //
            // xtraTabControl1
            //
            this.xtraTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.xtraTabControl1.Location = new System.Drawing.Point(0, 0);
            this.xtraTabControl1.Name = "xtraTabControl1";
            this.xtraTabControl1.Pages.AddRange(new ComponentFactory.Krypton.Navigator.KryptonPage[] {
            this.xtraTabPage1,
            this.xtraTabPage2});
            this.xtraTabControl1.SelectedIndex = 0;
            this.xtraTabControl1.Size = new System.Drawing.Size(815, 426);
            this.xtraTabControl1.TabIndex = 2;
            this.xtraTabControl1.SelectedPageChanged += new System.EventHandler(this.xtraTabControl1_SelectedPageChanged);
            //
            // xtraTabPage1
            //
            this.xtraTabPage1.Controls.Add(this.gridControl1);
            this.xtraTabPage1.Name = "xtraTabPage1";
            this.xtraTabPage1.Size = new System.Drawing.Size(797, 353);
            this.xtraTabPage1.Text = "Table view";
            //
            // gridControl1
            //
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(813, 399);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridControl1_CellFormatting);
            this.gridControl1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.gridControl1_CellPainting);
            //
            // xtraTabPage2
            //
            this.xtraTabPage2.Controls.Add(this.checkEdit9);
            this.xtraTabPage2.Controls.Add(this.checkEdit8);
            this.xtraTabPage2.Controls.Add(this.chartControl1);
            this.xtraTabPage2.Name = "xtraTabPage2";
            this.xtraTabPage2.Size = new System.Drawing.Size(793, 350);
            this.xtraTabPage2.Text = "Dyno graph view";
            //
            // checkEdit9
            //
            this.checkEdit9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkEdit9.Checked = true;
            this.checkEdit9.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEdit9.Location = new System.Drawing.Point(133, 324);
            this.checkEdit9.Name = "checkEdit9";
            this.checkEdit9.Size = new System.Drawing.Size(124, 19);
            this.checkEdit9.TabIndex = 2;
            this.checkEdit9.Values.Text = "Show torque curve";
            this.checkEdit9.CheckedChanged += new System.EventHandler(this.checkEdit8_CheckedChanged);
            //
            // checkEdit8
            //
            this.checkEdit8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkEdit8.Checked = true;
            this.checkEdit8.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkEdit8.Location = new System.Drawing.Point(3, 324);
            this.checkEdit8.Name = "checkEdit8";
            this.checkEdit8.Size = new System.Drawing.Size(124, 19);
            this.checkEdit8.TabIndex = 1;
            this.checkEdit8.Values.Text = "Show power curve";
            this.checkEdit8.CheckedChanged += new System.EventHandler(this.checkEdit8_CheckedChanged);
            //
            // chartControl1
            //
            this.chartControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chartControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.chartControl1.Location = new System.Drawing.Point(0, 0);
            this.chartControl1.Name = "chartControl1";
            this.chartControl1.Size = new System.Drawing.Size(789, 318);
            this.chartControl1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem,
            this.printToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(139, 48);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.saveAsToolStripMenuItem.Text = "Save as ...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.printToolStripMenuItem.Text = "Print";
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
            //
            // groupControl2
            //
            this.groupControl2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupControl2.Location = new System.Drawing.Point(0, 426);
            this.groupControl2.Name = "groupControl2";
            //
            // groupControl2.Panel
            //
            this.groupControl2.Panel.Controls.Add(this.comboBoxEdit1);
            this.groupControl2.Panel.Controls.Add(this.checkEdit6);
            this.groupControl2.Panel.Controls.Add(this.checkEdit5);
            this.groupControl2.Panel.Controls.Add(this.labelControl13);
            this.groupControl2.Panel.Controls.Add(this.labelControl14);
            this.groupControl2.Panel.Controls.Add(this.labelControl10);
            this.groupControl2.Panel.Controls.Add(this.labelControl9);
            this.groupControl2.Panel.Controls.Add(this.spinEdit1);
            this.groupControl2.Panel.Controls.Add(this.labelControl7);
            this.groupControl2.Panel.Controls.Add(this.labelControl8);
            this.groupControl2.Panel.Controls.Add(this.labelControl4);
            this.groupControl2.Panel.Controls.Add(this.labelControl6);
            this.groupControl2.Panel.Controls.Add(this.labelControl3);
            this.groupControl2.Panel.Controls.Add(this.labelControl1);
            this.groupControl2.Panel.Controls.Add(this.comboBoxEdit2);
            this.groupControl2.Panel.Controls.Add(this.checkEdit1);
            this.groupControl2.Panel.StateCommon.Color1 = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.groupControl2.Size = new System.Drawing.Size(815, 156);
            this.groupControl2.TabIndex = 3;
            this.groupControl2.Values.Heading = "Options";
            //
            // comboBoxEdit1
            //
            this.comboBoxEdit1.Items.AddRange(new object[] {
            "Automatic banks selection"});
            this.comboBoxEdit1.Location = new System.Drawing.Point(16, 113);
            this.comboBoxEdit1.Name = "comboBoxEdit1";
            this.comboBoxEdit1.Size = new System.Drawing.Size(195, 21);
            this.comboBoxEdit1.TabIndex = 26;
            this.comboBoxEdit1.Text = "Automatic bank selection";
            this.comboBoxEdit1.SelectedIndexChanged += new System.EventHandler(this.comboBoxEdit1_SelectedIndexChanged_1);
            //
            // checkEdit6
            //
            this.checkEdit6.Location = new System.Drawing.Point(125, 88);
            this.checkEdit6.Name = "checkEdit6";
            this.checkEdit6.Size = new System.Drawing.Size(95, 20);
            this.checkEdit6.TabIndex = 25;
            this.checkEdit6.Values.Text = "torque in lbft";
            this.checkEdit6.CheckedChanged += new System.EventHandler(this.checkEdit6_CheckedChanged);
            //
            // checkEdit5
            //
            this.checkEdit5.Location = new System.Drawing.Point(14, 88);
            this.checkEdit5.Name = "checkEdit5";
            this.checkEdit5.Size = new System.Drawing.Size(92, 20);
            this.checkEdit5.TabIndex = 24;
            this.checkEdit5.Values.Text = "power in kW";
            this.checkEdit5.CheckedChanged += new System.EventHandler(this.checkEdit5_CheckedChanged);
            //
            // labelControl13
            //
            this.labelControl13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl13.Location = new System.Drawing.Point(775, 79);
            this.labelControl13.Name = "labelControl13";
            this.labelControl13.Size = new System.Drawing.Size(15, 20);
            this.labelControl13.StateCommon.ShortText.Color1 = System.Drawing.Color.Black;
            this.labelControl13.StateCommon.ShortText.ImageStyle = ComponentFactory.Krypton.Toolkit.PaletteImageStyle.Inherit;
            this.labelControl13.StateCommon.ShortText.Trim = ComponentFactory.Krypton.Toolkit.PaletteTextTrim.Inherit;
            this.labelControl13.TabIndex = 19;
            this.labelControl13.Values.Text = "     ";
            this.labelControl13.Visible = false;
            this.labelControl13.Click += new System.EventHandler(this.labelControl13_Click);
            //
            // labelControl14
            //
            this.labelControl14.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl14.Location = new System.Drawing.Point(703, 80);
            this.labelControl14.Name = "labelControl14";
            this.labelControl14.Size = new System.Drawing.Size(88, 20);
            this.labelControl14.TabIndex = 18;
            this.labelControl14.Values.Text = "Fuelcut limiter";
            this.labelControl14.Visible = false;
            this.labelControl14.DoubleClick += new System.EventHandler(this.labelControl14_DoubleClick);
            this.labelControl14.Click += new System.EventHandler(this.labelControl14_Click);
            //
            // labelControl10
            //
            this.labelControl10.Location = new System.Drawing.Point(217, 41);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(30, 20);
            this.labelControl10.TabIndex = 15;
            this.labelControl10.Values.Text = "kPa";
            //
            // labelControl9
            //
            this.labelControl9.Location = new System.Drawing.Point(16, 41);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(108, 20);
            this.labelControl9.TabIndex = 14;
            this.labelControl9.Values.Text = "Ambient pressure";
            //
            // spinEdit1
            //
            this.spinEdit1.Location = new System.Drawing.Point(146, 36);
            this.spinEdit1.Maximum = new decimal(new int[] {
            150,
            0,
            0,
            0});
            this.spinEdit1.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.spinEdit1.Name = "spinEdit1";
            this.spinEdit1.Size = new System.Drawing.Size(65, 22);
            this.spinEdit1.TabIndex = 13;
            this.spinEdit1.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.spinEdit1.ValueChanged += new System.EventHandler(this.spinEdit1_EditValueChanged);
            //
            // labelControl7
            //
            this.labelControl7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl7.Location = new System.Drawing.Point(775, 22);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(15, 20);
            this.labelControl7.TabIndex = 12;
            this.labelControl7.Values.Text = "     ";
            this.labelControl7.Visible = false;
            //
            // labelControl8
            //
            this.labelControl8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl8.Location = new System.Drawing.Point(681, 23);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(114, 20);
            this.labelControl8.TabIndex = 11;
            this.labelControl8.Values.Text = "Turbospeed limiter";
            this.labelControl8.Visible = false;
            this.labelControl8.DoubleClick += new System.EventHandler(this.labelControl8_DoubleClick);
            //
            // labelControl4
            //
            this.labelControl4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl4.Location = new System.Drawing.Point(775, 60);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(15, 20);
            this.labelControl4.TabIndex = 10;
            this.labelControl4.Values.Text = "     ";
            //
            // labelControl6
            //
            this.labelControl6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl6.Location = new System.Drawing.Point(775, 41);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(15, 20);
            this.labelControl6.TabIndex = 8;
            this.labelControl6.Values.Text = "     ";
            this.labelControl6.Visible = false;
            //
            // labelControl3
            //
            this.labelControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl3.Location = new System.Drawing.Point(704, 61);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(86, 20);
            this.labelControl3.TabIndex = 7;
            this.labelControl3.Values.Text = "Torque limiter";
            this.labelControl3.DoubleClick += new System.EventHandler(this.labelControl3_DoubleClick);
            //
            // labelControl1
            //
            this.labelControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl1.Location = new System.Drawing.Point(701, 42);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(91, 20);
            this.labelControl1.TabIndex = 5;
            this.labelControl1.Values.Text = "Airmass limiter";
            this.labelControl1.Visible = false;
            this.labelControl1.DoubleClick += new System.EventHandler(this.labelControl1_DoubleClick);
            //
            // comboBoxEdit2
            //
            this.comboBoxEdit2.Items.AddRange(new object[] {
            "Show IQ",
            "Show estimated torque",
            "Show estimated horsepower"});
            this.comboBoxEdit2.Location = new System.Drawing.Point(16, 62);
            this.comboBoxEdit2.Name = "comboBoxEdit2";
            this.comboBoxEdit2.Size = new System.Drawing.Size(195, 21);
            this.comboBoxEdit2.TabIndex = 4;
            this.comboBoxEdit2.Text = "Show IQ";
            this.comboBoxEdit2.SelectedIndexChanged += new System.EventHandler(this.comboBoxEdit2_SelectedIndexChanged);
            //
            // checkEdit1
            //
            this.checkEdit1.Location = new System.Drawing.Point(425, 87);
            this.checkEdit1.Name = "checkEdit1";
            this.checkEdit1.Size = new System.Drawing.Size(177, 20);
            this.checkEdit1.TabIndex = 0;
            this.checkEdit1.Values.Text = "Car has automatic gearbox";
            this.checkEdit1.Visible = false;
            this.checkEdit1.CheckedChanged += new System.EventHandler(this.checkEdit1_CheckedChanged);
            //
            // simpleButton3
            //
            this.simpleButton3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton3.Enabled = false;
            this.simpleButton3.Location = new System.Drawing.Point(8, 120);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(147, 25);
            this.simpleButton3.TabIndex = 7;
            this.simpleButton3.Values.Text = "Compare to another file";
            this.simpleButton3.Visible = false;
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            //
            // simpleButton2
            //
            this.simpleButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton2.Location = new System.Drawing.Point(651, 120);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 6;
            this.simpleButton2.Values.Text = "Refresh";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            //
            // simpleButton1
            //
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.Location = new System.Drawing.Point(732, 120);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 5;
            this.simpleButton1.Values.Text = "Close";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // ctrlAirmassResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.groupControl2.Panel.Controls.Add(this.simpleButton3);
            this.groupControl2.Panel.Controls.Add(this.simpleButton2);
            this.groupControl2.Panel.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.xtraTabControl1);
            this.Controls.Add(this.groupControl2);
            this.Name = "ctrlAirmassResult";
            this.Size = new System.Drawing.Size(815, 582);
            ((System.ComponentModel.ISupportInitialize)(this.xtraTabControl1)).EndInit();
            this.xtraTabControl1.ResumeLayout(false);
            this.xtraTabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.xtraTabPage2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.groupControl2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxEdit2)).EndInit();
            // spinEdit1 (KryptonNumericUpDown) does not support ISupportInitialize
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Navigator.KryptonNavigator xtraTabControl1;
        private ComponentFactory.Krypton.Navigator.KryptonPage xtraTabPage1;
        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView gridControl1;
        private ComponentFactory.Krypton.Navigator.KryptonPage xtraTabPage2;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox checkEdit9;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox checkEdit8;
        private ZedGraph.ZedGraphControl chartControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox groupControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox checkEdit6;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox checkEdit5;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl13;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl14;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl10;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl9;
        private ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown spinEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl7;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl8;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl6;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl3;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox comboBoxEdit2;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox checkEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton3;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox comboBoxEdit1;
    }
}
