namespace VAGSuite
{
    partial class frmTurboCompressorAnalysis
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTurboCompressorAnalysis));
            this.kryptonPanel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.cmbBoostMap = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.kryptonLabelBoostMap = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.cmbCompressorMap = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.kryptonLabel1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.kryptonGroupBox1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.txtCylinders = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.txtDisplacement = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.kryptonLabel2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblMapName = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.kryptonGroupBox2 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.numVE = new ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown();
            this.kryptonLabel6 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.numTemp = new ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown();
            this.kryptonLabel5 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.numAltitude = new ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown();
            this.kryptonLabel4 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.zedMap = new ZedGraph.ZedGraphControl();
            this.btnCalculate = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnExport = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.chkTooltips = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
            this.kryptonPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCompressorMap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1.Panel)).BeginInit();
            this.kryptonGroupBox1.Panel.SuspendLayout();
            this.kryptonGroupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2.Panel)).BeginInit();
            this.kryptonGroupBox2.Panel.SuspendLayout();
            this.kryptonGroupBox2.SuspendLayout();
            this.SuspendLayout();
            
            // kryptonPanel1
            this.kryptonPanel1.Controls.Add(this.chkTooltips);
            this.kryptonPanel1.Controls.Add(this.btnExport);
            this.kryptonPanel1.Controls.Add(this.btnCalculate);
            this.kryptonPanel1.Controls.Add(this.zedMap);
            this.kryptonPanel1.Controls.Add(this.kryptonGroupBox2);
            this.kryptonPanel1.Controls.Add(this.lblMapName);
            this.kryptonPanel1.Controls.Add(this.cmbBoostMap);
            this.kryptonPanel1.Controls.Add(this.kryptonLabelBoostMap);
            this.kryptonPanel1.Controls.Add(this.kryptonGroupBox1);
            this.kryptonPanel1.Controls.Add(this.kryptonLabel1);
            this.kryptonPanel1.Controls.Add(this.cmbCompressorMap);
            this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kryptonPanel1.Location = new System.Drawing.Point(0, 0);
            this.kryptonPanel1.Name = "kryptonPanel1";
            this.kryptonPanel1.Size = new System.Drawing.Size(1008, 729);
            this.kryptonPanel1.TabIndex = 0;
            
            // cmbCompressorMap
            this.cmbCompressorMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCompressorMap.Location = new System.Drawing.Point(140, 12);
            this.cmbCompressorMap.Name = "cmbCompressorMap";
            this.cmbCompressorMap.Size = new System.Drawing.Size(200, 21);
            this.cmbCompressorMap.TabIndex = 0;
            this.cmbCompressorMap.SelectedIndexChanged += new System.EventHandler(this.cmbCompressorMap_SelectedIndexChanged);
            
            // kryptonLabel1
            this.kryptonLabel1.Location = new System.Drawing.Point(12, 12);
            this.kryptonLabel1.Name = "kryptonLabel1";
            this.kryptonLabel1.Size = new System.Drawing.Size(102, 20);
            this.kryptonLabel1.TabIndex = 1;
            this.kryptonLabel1.Values.Text = "Compressor Map";
            
            // kryptonGroupBox1
            this.kryptonGroupBox1.Location = new System.Drawing.Point(12, 50);
            this.kryptonGroupBox1.Name = "kryptonGroupBox1";
            
            // Panel
            this.kryptonGroupBox1.Panel.Controls.Add(this.txtCylinders);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonLabel3);
            this.kryptonGroupBox1.Panel.Controls.Add(this.txtDisplacement);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonLabel2);
            this.kryptonGroupBox1.Size = new System.Drawing.Size(328, 100);
            this.kryptonGroupBox1.TabIndex = 2;
            this.kryptonGroupBox1.Values.Heading = "Engine Parameters (extracted)";
            
            // txtCylinders
            this.txtCylinders.Location = new System.Drawing.Point(120, 40);
            this.txtCylinders.Name = "txtCylinders";
            this.txtCylinders.ReadOnly = true;
            this.txtCylinders.Size = new System.Drawing.Size(60, 23);
            this.txtCylinders.TabIndex = 3;
            
            // kryptonLabel3
            this.kryptonLabel3.Location = new System.Drawing.Point(10, 40);
            this.kryptonLabel3.Name = "kryptonLabel3";
            this.kryptonLabel3.Size = new System.Drawing.Size(60, 20);
            this.kryptonLabel3.TabIndex = 2;
            this.kryptonLabel3.Values.Text = "Cylinders";
            
            // txtDisplacement
            this.txtDisplacement.Location = new System.Drawing.Point(120, 10);
            this.txtDisplacement.Name = "txtDisplacement";
            this.txtDisplacement.Size = new System.Drawing.Size(60, 23);
            this.txtDisplacement.TabIndex = 1;
            
            // kryptonLabel2
            this.kryptonLabel2.Location = new System.Drawing.Point(10, 10);
            this.kryptonLabel2.Name = "kryptonLabel2";
            this.kryptonLabel2.Size = new System.Drawing.Size(84, 20);
            this.kryptonLabel2.TabIndex = 0;
            this.kryptonLabel2.Values.Text = "Displacement";
            
            // lblMapName
            this.lblMapName.Location = new System.Drawing.Point(12, 155);
            this.lblMapName.Name = "lblMapName";
            this.lblMapName.Size = new System.Drawing.Size(70, 20);
            this.lblMapName.TabIndex = 3;
            this.lblMapName.Values.Text = "Boost Map:";

            // kryptonLabelBoostMap
            this.kryptonLabelBoostMap.Location = new System.Drawing.Point(12, 155);
            this.kryptonLabelBoostMap.Name = "kryptonLabelBoostMap";
            this.kryptonLabelBoostMap.Size = new System.Drawing.Size(70, 20);
            this.kryptonLabelBoostMap.TabIndex = 10;
            this.kryptonLabelBoostMap.Values.Text = "Boost Map:";

            // cmbBoostMap
            this.cmbBoostMap.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoostMap.Location = new System.Drawing.Point(90, 153);
            this.cmbBoostMap.Name = "cmbBoostMap";
            this.cmbBoostMap.Size = new System.Drawing.Size(250, 21);
            this.cmbBoostMap.TabIndex = 11;
            this.cmbBoostMap.SelectedIndexChanged += new System.EventHandler(this.cmbBoostMap_SelectedIndexChanged);
            
            // kryptonGroupBox2
            this.kryptonGroupBox2.Location = new System.Drawing.Point(12, 180);
            this.kryptonGroupBox2.Name = "kryptonGroupBox2";
            
            // Panel
            this.kryptonGroupBox2.Panel.Controls.Add(this.numVE);
            this.kryptonGroupBox2.Panel.Controls.Add(this.kryptonLabel6);
            this.kryptonGroupBox2.Panel.Controls.Add(this.numTemp);
            this.kryptonGroupBox2.Panel.Controls.Add(this.kryptonLabel5);
            this.kryptonGroupBox2.Panel.Controls.Add(this.numAltitude);
            this.kryptonGroupBox2.Panel.Controls.Add(this.kryptonLabel4);
            this.kryptonGroupBox2.Size = new System.Drawing.Size(328, 120);
            this.kryptonGroupBox2.TabIndex = 4;
            this.kryptonGroupBox2.Values.Heading = "Environment / Tuning";
            
            // numVE
            this.numVE.Location = new System.Drawing.Point(120, 65);
            this.numVE.Name = "numVE";
            this.numVE.Size = new System.Drawing.Size(60, 22);
            this.numVE.TabIndex = 5;
            this.numVE.Value = new decimal(new int[] { 85, 0, 0, 0 });
            
            // kryptonLabel6
            this.kryptonLabel6.Location = new System.Drawing.Point(10, 65);
            this.kryptonLabel6.Name = "kryptonLabel6";
            this.kryptonLabel6.Size = new System.Drawing.Size(43, 20);
            this.kryptonLabel6.TabIndex = 4;
            this.kryptonLabel6.Values.Text = "VE (%)";
            
            // numTemp
            this.numTemp.Location = new System.Drawing.Point(120, 37);
            this.numTemp.Name = "numTemp";
            this.numTemp.Size = new System.Drawing.Size(60, 22);
            this.numTemp.TabIndex = 3;
            this.numTemp.Value = new decimal(new int[] { 20, 0, 0, 0 });
            
            // kryptonLabel5
            this.kryptonLabel5.Location = new System.Drawing.Point(10, 37);
            this.kryptonLabel5.Name = "kryptonLabel5";
            this.kryptonLabel5.Size = new System.Drawing.Size(61, 20);
            this.kryptonLabel5.TabIndex = 2;
            this.kryptonLabel5.Values.Text = "Temp (C)";
            
            // numAltitude
            this.numAltitude.Location = new System.Drawing.Point(120, 10);
            this.numAltitude.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            this.numAltitude.Name = "numAltitude";
            this.numAltitude.Size = new System.Drawing.Size(60, 22);
            this.numAltitude.TabIndex = 1;
            
            // kryptonLabel4
            this.kryptonLabel4.Location = new System.Drawing.Point(10, 10);
            this.kryptonLabel4.Name = "kryptonLabel4";
            this.kryptonLabel4.Size = new System.Drawing.Size(73, 20);
            this.kryptonLabel4.TabIndex = 0;
            this.kryptonLabel4.Values.Text = "Altitude (m)";
            
            // zedMap
            //
            this.zedMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.zedMap.Location = new System.Drawing.Point(350, 12);
            this.zedMap.Name = "zedMap";
            this.zedMap.ScrollGrace = 0D;
            this.zedMap.ScrollMaxX = 0D;
            this.zedMap.ScrollMaxY = 0D;
            this.zedMap.ScrollMaxY2 = 0D;
            this.zedMap.ScrollMinX = 0D;
            this.zedMap.ScrollMinY = 0D;
            this.zedMap.ScrollMinY2 = 0D;
            this.zedMap.Size = new System.Drawing.Size(646, 705);
            this.zedMap.TabIndex = 5;
            
            // btnCalculate
            this.btnCalculate.Location = new System.Drawing.Point(12, 310);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(150, 30);
            this.btnCalculate.TabIndex = 6;
            this.btnCalculate.Values.Text = "Update Plot";
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            
            // btnExport
            this.btnExport.Location = new System.Drawing.Point(190, 310);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(150, 30);
            this.btnExport.TabIndex = 7;
            this.btnExport.Values.Text = "Export Image";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            
            // chkTooltips
            this.chkTooltips.Location = new System.Drawing.Point(12, 350);
            this.chkTooltips.Name = "chkTooltips";
            this.chkTooltips.Size = new System.Drawing.Size(100, 20);
            this.chkTooltips.TabIndex = 12;
            this.chkTooltips.Values.Text = "Show Tooltips";
            this.chkTooltips.Checked = true;
            this.chkTooltips.CheckedChanged += new System.EventHandler(this.chkTooltips_CheckedChanged);

            // frmTurboCompressorAnalysis
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.kryptonPanel1);
            this.Name = "frmTurboCompressorAnalysis";
            this.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Custom;
            this.Text = "Turbo Compressor Analysis Helper";
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).EndInit();
            this.kryptonPanel1.ResumeLayout(false);
            this.kryptonPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cmbCompressorMap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1.Panel)).EndInit();
            this.kryptonGroupBox1.Panel.ResumeLayout(false);
            this.kryptonGroupBox1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1)).EndInit();
            this.kryptonGroupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2.Panel)).EndInit();
            this.kryptonGroupBox2.Panel.ResumeLayout(false);
            this.kryptonGroupBox2.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2)).EndInit();
            this.kryptonGroupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private ComponentFactory.Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox cmbCompressorMap;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox cmbBoostMap;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabelBoostMap;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel1;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox kryptonGroupBox1;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox txtCylinders;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel3;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox txtDisplacement;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblMapName;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox kryptonGroupBox2;
        private ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown numVE;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel6;
        private ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown numTemp;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel5;
        private ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown numAltitude;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel kryptonLabel4;
        private ZedGraph.ZedGraphControl zedMap;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnCalculate;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnExport;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox chkTooltips;
    }
}
