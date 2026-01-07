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
            this.kryptonPanel1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.btnClose = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnExportCSV = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.lblDetails = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.kryptonGroupBox1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.dgvEOI = new Zuby.ADGV.AdvancedDataGridView();
            this.kryptonGroupBox2 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.cmbCodeBank = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.lblCodeBank = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.trackTemperature = new System.Windows.Forms.TrackBar();
            this.lblTemperature = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.btnCalculate = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonPanel1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1)).BeginInit();
            this.kryptonGroupBox1.Panel.SuspendLayout();
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
            // lblDetails
            // 
            this.lblDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDetails.Location = new System.Drawing.Point(5, 5);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new System.Drawing.Size(890, 90);
            this.lblDetails.TabIndex = 0;
            this.lblDetails.Values.Text = "Calculation details will appear here...";
            // 
            // kryptonGroupBox1
            // 
            this.kryptonGroupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.kryptonGroupBox1.Location = new System.Drawing.Point(0, 460);
            this.kryptonGroupBox1.Name = "kryptonGroupBox1";
            this.kryptonGroupBox1.Panel.Controls.Add(this.lblDetails);
            this.kryptonGroupBox1.Size = new System.Drawing.Size(900, 100);
            this.kryptonGroupBox1.TabIndex = 1;
            this.kryptonGroupBox1.Values.Heading = "Calculation Details";
            // 
            //
            // dgvEOI
            //
            this.dgvEOI.AllowUserToAddRows = false;
            this.dgvEOI.AllowUserToDeleteRows = false;
            this.dgvEOI.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvEOI.ColumnHeadersHeight = 30;
            this.dgvEOI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvEOI.FilterAndSortEnabled = false;
            this.dgvEOI.Location = new System.Drawing.Point(0, 150);
            this.dgvEOI.Name = "dgvEOI";
            this.dgvEOI.ReadOnly = true;
            this.dgvEOI.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dgvEOI.RowHeadersWidth = 80;
            this.dgvEOI.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvEOI.Size = new System.Drawing.Size(900, 310);
            this.dgvEOI.TabIndex = 2;
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
            this.kryptonGroupBox2.Size = new System.Drawing.Size(900, 150);
            this.kryptonGroupBox2.TabIndex = 3;
            this.kryptonGroupBox2.Values.Heading = "Configuration";
            // 
            // cmbCodeBank
            // 
            this.cmbCodeBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCodeBank.Location = new System.Drawing.Point(10, 25);
            this.cmbCodeBank.Name = "cmbCodeBank";
            this.cmbCodeBank.Size = new System.Drawing.Size(150, 21);
            this.cmbCodeBank.TabIndex = 0;
            this.cmbCodeBank.SelectedIndexChanged += new System.EventHandler(this.cmbCodeBank_SelectedIndexChanged);
            // 
            // lblCodeBank
            // 
            this.lblCodeBank.Location = new System.Drawing.Point(10, 10);
            this.lblCodeBank.Name = "lblCodeBank";
            this.lblCodeBank.Size = new System.Drawing.Size(150, 20);
            this.lblCodeBank.TabIndex = 1;
            this.lblCodeBank.Values.Text = "Codebank:";
            //
            // trackTemperature
            //
            this.trackTemperature.Location = new System.Drawing.Point(180, 25);
            this.trackTemperature.Maximum = 100;
            this.trackTemperature.Minimum = 0;
            this.trackTemperature.Name = "trackTemperature";
            this.trackTemperature.Size = new System.Drawing.Size(300, 45);
            this.trackTemperature.TabIndex = 2;
            this.trackTemperature.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.trackTemperature.Scroll += new System.EventHandler(this.trackTemperature_Scroll);
            // 
            // lblTemperature
            // 
            this.lblTemperature.Location = new System.Drawing.Point(180, 10);
            this.lblTemperature.Name = "lblTemperature";
            this.lblTemperature.Size = new System.Drawing.Size(300, 20);
            this.lblTemperature.TabIndex = 3;
            this.lblTemperature.Values.Text = "Temperature: 90 °C";
            // 
            // btnCalculate
            // 
            this.btnCalculate.Location = new System.Drawing.Point(500, 25);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(120, 30);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Values.Text = "Calculate EOI";
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);
            // 
            // frmEOICalculator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 600);
            this.Controls.Add(this.dgvEOI);
            this.Controls.Add(this.kryptonGroupBox2);
            this.Controls.Add(this.kryptonGroupBox1);
            this.Controls.Add(this.btnExportCSV);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.Name = "frmEOICalculator";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EOI Calculator";
            this.kryptonGroupBox1.Panel.ResumeLayout(false);
            this.kryptonGroupBox1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1)).EndInit();
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
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblDetails;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox kryptonGroupBox1;
        private Zuby.ADGV.AdvancedDataGridView dgvEOI;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox kryptonGroupBox2;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox cmbCodeBank;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblCodeBank;
        private System.Windows.Forms.TrackBar trackTemperature;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblTemperature;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnCalculate;
    }
}
