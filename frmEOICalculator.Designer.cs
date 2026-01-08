namespace VAGSuite
{
    partial class frmEOICalculator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEOICalculator));
            
            this.kryptonPanel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.btnClose = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnExportCSV = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            
            // 3D Chart Panel
            this.chart3DPanel = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.glControl3D = new OpenTK.GLControl();
            this.btnZoomIn = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnZoomOut = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnRotateLeft = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnRotateRight = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnToggleWireframe = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnToggleTooltips3D = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.dgvEOI = new Zuby.ADGV.AdvancedDataGridView();
            this.kryptonGroupBox2 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.cmbCodeBank = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.lblCodeBank = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.trackTemperature = new System.Windows.Forms.TrackBar();
            this.lblTemperature = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.btnCalculate = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnToggleTooltips = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
            this.chart3DPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2)).BeginInit();
            this.kryptonGroupBox2.Panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEOI)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackTemperature)).BeginInit();
            this.SuspendLayout();
            //
            // kryptonPanel1
            //
            this.kryptonPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.kryptonPanel1.Location = new System.Drawing.Point(0, 550);
            this.kryptonPanel1.Name = "kryptonPanel1";
            this.kryptonPanel1.Size = new System.Drawing.Size(900, 50);
            this.kryptonPanel1.TabIndex = 0;
            //
            // btnClose
            //
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(820, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(70, 30);
            this.btnClose.TabIndex = 0;
            this.btnClose.Values.Text = "Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //
            // btnExportCSV
            //
            this.btnExportCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportCSV.Location = new System.Drawing.Point(740, 10);
            this.btnExportCSV.Name = "btnExportCSV";
            this.btnExportCSV.Size = new System.Drawing.Size(70, 30);
            this.btnExportCSV.TabIndex = 1;
            this.btnExportCSV.Values.Text = "Export CSV";
            this.btnExportCSV.Click += new System.EventHandler(this.btnExportCSV_Click);
            
            //
            // chart3DPanel (replaces rtfDetails area)
            //
            this.chart3DPanel.Controls.Add(this.btnZoomIn);
            this.chart3DPanel.Controls.Add(this.btnZoomOut);
            this.chart3DPanel.Controls.Add(this.btnRotateLeft);
            this.chart3DPanel.Controls.Add(this.btnRotateRight);
            this.chart3DPanel.Controls.Add(this.btnToggleWireframe);
            this.chart3DPanel.Controls.Add(this.btnToggleTooltips3D);
            this.chart3DPanel.Controls.Add(this.glControl3D);
            this.chart3DPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart3DPanel.Location = new System.Drawing.Point(0, 0);
            this.chart3DPanel.Name = "chart3DPanel";
            this.chart3DPanel.Size = new System.Drawing.Size(900, 250);
            this.chart3DPanel.TabIndex = 1;
            
            //
            // glControl3D
            //
            this.glControl3D.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.glControl3D.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl3D.Location = new System.Drawing.Point(0, 0);
            this.glControl3D.Name = "glControl3D";
            this.glControl3D.Size = new System.Drawing.Size(900, 250);
            this.glControl3D.TabIndex = 0;
            this.glControl3D.VSync = false;
            
            // Navigation Buttons (anchored top-right)
            this.btnZoomIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoomIn.Location = new System.Drawing.Point(866, 10);
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(24, 24);
            this.btnZoomIn.TabIndex = 1;
            this.btnZoomIn.Values.Text = "+";
            
            this.btnZoomOut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnZoomOut.Location = new System.Drawing.Point(866, 40);
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(24, 24);
            this.btnZoomOut.TabIndex = 2;
            this.btnZoomOut.Values.Text = "-";
            
            this.btnRotateLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRotateLeft.Location = new System.Drawing.Point(866, 70);
            this.btnRotateLeft.Name = "btnRotateLeft";
            this.btnRotateLeft.Size = new System.Drawing.Size(24, 24);
            this.btnRotateLeft.TabIndex = 3;
            this.btnRotateLeft.Values.Text = "<";
            
            this.btnRotateRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRotateRight.Location = new System.Drawing.Point(866, 100);
            this.btnRotateRight.Name = "btnRotateRight";
            this.btnRotateRight.Size = new System.Drawing.Size(24, 24);
            this.btnRotateRight.TabIndex = 4;
            this.btnRotateRight.Values.Text = ">";
            
            this.btnToggleWireframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleWireframe.Location = new System.Drawing.Point(866, 130);
            this.btnToggleWireframe.Name = "btnToggleWireframe";
            this.btnToggleWireframe.Size = new System.Drawing.Size(24, 24);
            this.btnToggleWireframe.TabIndex = 5;
            this.btnToggleWireframe.Values.Text = "W";
            
            this.btnToggleTooltips3D.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleTooltips3D.Location = new System.Drawing.Point(866, 160);
            this.btnToggleTooltips3D.Name = "btnToggleTooltips3D";
            this.btnToggleTooltips3D.Size = new System.Drawing.Size(24, 24);
            this.btnToggleTooltips3D.TabIndex = 6;
            this.btnToggleTooltips3D.Values.Text = "T";
            
            // dgvEOI
            //
            this.dgvEOI.AllowUserToAddRows = false;
            this.dgvEOI.AllowUserToDeleteRows = false;
            this.dgvEOI.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvEOI.ColumnHeadersHeight = 30;
            this.dgvEOI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEOI.FilterAndSortEnabled = false;
            this.dgvEOI.Location = new System.Drawing.Point(0, 0);
            this.dgvEOI.Name = "dgvEOI";
            this.dgvEOI.ReadOnly = true;
            this.dgvEOI.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dgvEOI.RowHeadersWidth = 80;
            this.dgvEOI.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvEOI.Size = new System.Drawing.Size(900, 250);
            this.dgvEOI.TabIndex = 2;

            //
            // mainSplitContainer
            //
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 65);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // mainSplitContainer.Panel1
            //
            this.mainSplitContainer.Panel1.Controls.Add(this.dgvEOI);
            //
            // mainSplitContainer.Panel2
            //
            this.mainSplitContainer.Panel2.Controls.Add(this.chart3DPanel);
            this.mainSplitContainer.Size = new System.Drawing.Size(900, 455);
            this.mainSplitContainer.SplitterDistance = 220;
            this.mainSplitContainer.TabIndex = 4;
            // 
            // kryptonGroupBox2
            // 
            this.kryptonGroupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.kryptonGroupBox2.Location = new System.Drawing.Point(0, 0);
            this.kryptonGroupBox2.Name = "kryptonGroupBox2";
            this.kryptonGroupBox2.Panel.Controls.Add(this.cmbCodeBank);
            this.kryptonGroupBox2.Panel.Controls.Add(this.lblCodeBank);
            this.kryptonGroupBox2.Panel.Controls.Add(this.trackTemperature);
            this.kryptonGroupBox2.Panel.Controls.Add(this.lblTemperature);
            this.kryptonGroupBox2.Panel.Controls.Add(this.btnCalculate);
            this.kryptonGroupBox2.Panel.Controls.Add(this.btnToggleTooltips);
            this.kryptonGroupBox2.Size = new System.Drawing.Size(900, 65);
            this.kryptonGroupBox2.TabIndex = 3;
            this.kryptonGroupBox2.Values.Heading = "Configuration";
            //
            // cmbCodeBank
            //
            this.cmbCodeBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCodeBank.Location = new System.Drawing.Point(10, 18);
            this.cmbCodeBank.Name = "cmbCodeBank";
            this.cmbCodeBank.Size = new System.Drawing.Size(150, 21);
            this.cmbCodeBank.TabIndex = 0;
            this.cmbCodeBank.SelectedIndexChanged += new System.EventHandler(this.cmbCodeBank_SelectedIndexChanged);
            //
            // lblCodeBank
            //
            this.lblCodeBank.Location = new System.Drawing.Point(10, 2);
            this.lblCodeBank.Name = "lblCodeBank";
            this.lblCodeBank.Size = new System.Drawing.Size(150, 20);
            this.lblCodeBank.TabIndex = 1;
            this.lblCodeBank.Values.Text = "Codebank:";
            //
            // trackTemperature
            //
            this.trackTemperature.AutoSize = false;
            this.trackTemperature.Location = new System.Drawing.Point(180, 18);
            this.trackTemperature.Maximum = 100;
            this.trackTemperature.Minimum = 0;
            this.trackTemperature.Name = "trackTemperature";
            this.trackTemperature.Size = new System.Drawing.Size(300, 25);
            this.trackTemperature.TabIndex = 2;
            this.trackTemperature.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackTemperature.Scroll += new System.EventHandler(this.trackTemperature_Scroll);
            //
            // lblTemperature
            //
            this.lblTemperature.Location = new System.Drawing.Point(180, 2);
            this.lblTemperature.Name = "lblTemperature";
            this.lblTemperature.Size = new System.Drawing.Size(300, 20);
            this.lblTemperature.TabIndex = 3;
            this.lblTemperature.Values.Text = "Temperature: 90 °C";
            //
            // btnCalculate
            //
            this.btnCalculate.Location = new System.Drawing.Point(500, 12);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(120, 30);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Values.Text = "Calculate EOI";
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            //
            // btnToggleTooltips
            //
            this.btnToggleTooltips.Location = new System.Drawing.Point(640, 12);
            this.btnToggleTooltips.Name = "btnToggleTooltips";
            this.btnToggleTooltips.Size = new System.Drawing.Size(120, 30);
            this.btnToggleTooltips.TabIndex = 5;
            this.btnToggleTooltips.Values.Text = "Tooltips: ON";
            this.btnToggleTooltips.Click += new System.EventHandler(this.btnToggleTooltips_Click);
            //
            // frmEOICalculator
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.kryptonGroupBox2);
            this.Controls.Add(this.btnExportCSV);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.Name = "frmEOICalculator";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EOI Calculator";
            this.chart3DPanel.ResumeLayout(false);
            this.chart3DPanel.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.kryptonGroupBox2.Panel.ResumeLayout(false);
            this.kryptonGroupBox2.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEOI)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackTemperature)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel kryptonPanel1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnClose;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnExportCSV;
        
        // 3D Chart Components
        private ComponentFactory.Krypton.Toolkit.KryptonPanel chart3DPanel;
        private OpenTK.GLControl glControl3D;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnZoomIn;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnZoomOut;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnRotateLeft;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnRotateRight;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnToggleWireframe;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnToggleTooltips3D;
        
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private Zuby.ADGV.AdvancedDataGridView dgvEOI;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox kryptonGroupBox2;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox cmbCodeBank;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblCodeBank;
        private System.Windows.Forms.TrackBar trackTemperature;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblTemperature;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnCalculate;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnToggleTooltips;
    }
}
