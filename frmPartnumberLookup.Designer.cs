namespace VAGSuite
{
    partial class frmPartnumberLookup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmPartnumberLookup));
            this.kryptonTextBoxPartNumber = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.kryptonButtonBrowse = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.groupControl1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.lblSoftwareID = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl10 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblRating = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl8 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblCarType = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl5 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblFuel = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblECUType = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblCarModel = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl7 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton3 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton4 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).BeginInit();
            this.groupControl1.Panel.SuspendLayout();
            this.groupControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonTextBoxPartNumber
            // 
            this.kryptonTextBoxPartNumber.Location = new System.Drawing.Point(12, 30);
            this.kryptonTextBoxPartNumber.Name = "kryptonTextBoxPartNumber";
            this.kryptonTextBoxPartNumber.Size = new System.Drawing.Size(340, 23);
            this.kryptonTextBoxPartNumber.TabIndex = 0;
            this.kryptonTextBoxPartNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.buttonEdit1_KeyDown);
            // 
            // kryptonButtonBrowse
            // 
            this.kryptonButtonBrowse.Location = new System.Drawing.Point(358, 29);
            this.kryptonButtonBrowse.Name = "kryptonButtonBrowse";
            this.kryptonButtonBrowse.Size = new System.Drawing.Size(30, 25);
            this.kryptonButtonBrowse.TabIndex = 1;
            this.kryptonButtonBrowse.Values.Text = "...";
            this.kryptonButtonBrowse.Click += new System.EventHandler(this.buttonEdit1_ButtonClick);
            // 
            // groupControl1
            // 
            this.groupControl1.Location = new System.Drawing.Point(12, 65);
            this.groupControl1.Name = "groupControl1";
            // 
            // groupControl1.Panel
            // 
            this.groupControl1.Panel.Controls.Add(this.lblSoftwareID);
            this.groupControl1.Panel.Controls.Add(this.labelControl10);
            this.groupControl1.Panel.Controls.Add(this.lblRating);
            this.groupControl1.Panel.Controls.Add(this.labelControl8);
            this.groupControl1.Panel.Controls.Add(this.lblCarType);
            this.groupControl1.Panel.Controls.Add(this.labelControl5);
            this.groupControl1.Panel.Controls.Add(this.lblFuel);
            this.groupControl1.Panel.Controls.Add(this.lblECUType);
            this.groupControl1.Panel.Controls.Add(this.lblCarModel);
            this.groupControl1.Panel.Controls.Add(this.labelControl7);
            this.groupControl1.Panel.Controls.Add(this.labelControl3);
            this.groupControl1.Panel.Controls.Add(this.labelControl2);
            this.groupControl1.Size = new System.Drawing.Size(376, 180);
            this.groupControl1.TabIndex = 2;
            this.groupControl1.Values.Heading = "ECU Information";
            // 
            // lblSoftwareID
            // 
            this.lblSoftwareID.Location = new System.Drawing.Point(120, 130);
            this.lblSoftwareID.Name = "lblSoftwareID";
            this.lblSoftwareID.Size = new System.Drawing.Size(27, 20);
            this.lblSoftwareID.TabIndex = 11;
            this.lblSoftwareID.Values.Text = "---";
            // 
            // labelControl10
            // 
            this.labelControl10.Location = new System.Drawing.Point(10, 130);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(71, 20);
            this.labelControl10.TabIndex = 10;
            this.labelControl10.Values.Text = "Software ID";
            // 
            // lblRating
            // 
            this.lblRating.Location = new System.Drawing.Point(120, 105);
            this.lblRating.Name = "lblRating";
            this.lblRating.Size = new System.Drawing.Size(27, 20);
            this.lblRating.TabIndex = 9;
            this.lblRating.Values.Text = "---";
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(10, 105);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(45, 20);
            this.labelControl8.TabIndex = 8;
            this.labelControl8.Values.Text = "Rating";
            // 
            // lblCarType
            // 
            this.lblCarType.Location = new System.Drawing.Point(120, 80);
            this.lblCarType.Name = "lblCarType";
            this.lblCarType.Size = new System.Drawing.Size(27, 20);
            this.lblCarType.TabIndex = 7;
            this.lblCarType.Values.Text = "---";
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(10, 80);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(53, 20);
            this.labelControl5.TabIndex = 6;
            this.labelControl5.Values.Text = "Car type";
            // 
            // lblFuel
            // 
            this.lblFuel.Location = new System.Drawing.Point(120, 55);
            this.lblFuel.Name = "lblFuel";
            this.lblFuel.Size = new System.Drawing.Size(27, 20);
            this.lblFuel.TabIndex = 5;
            this.lblFuel.Values.Text = "---";
            // 
            // lblECUType
            // 
            this.lblECUType.Location = new System.Drawing.Point(120, 30);
            this.lblECUType.Name = "lblECUType";
            this.lblECUType.Size = new System.Drawing.Size(27, 20);
            this.lblECUType.TabIndex = 4;
            this.lblECUType.Values.Text = "---";
            // 
            // lblCarModel
            // 
            this.lblCarModel.Location = new System.Drawing.Point(120, 5);
            this.lblCarModel.Name = "lblCarModel";
            this.lblCarModel.Size = new System.Drawing.Size(27, 20);
            this.lblCarModel.TabIndex = 3;
            this.lblCarModel.Values.Text = "---";
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(10, 55);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(32, 20);
            this.labelControl7.TabIndex = 2;
            this.labelControl7.Values.Text = "Fuel";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(10, 30);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(58, 20);
            this.labelControl3.TabIndex = 1;
            this.labelControl3.Values.Text = "ECU type";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(10, 5);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(64, 20);
            this.labelControl2.TabIndex = 0;
            this.labelControl2.Values.Text = "Car model";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 10);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(125, 20);
            this.labelControl1.TabIndex = 3;
            this.labelControl1.Values.Text = "Enter Bosch partnumber";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(313, 285);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 4;
            this.simpleButton1.Values.Text = "Close";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Enabled = false;
            this.simpleButton2.Location = new System.Drawing.Point(12, 255);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(120, 25);
            this.simpleButton2.TabIndex = 5;
            this.simpleButton2.Values.Text = "Open original";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // simpleButton3
            // 
            this.simpleButton3.Enabled = false;
            this.simpleButton3.Location = new System.Drawing.Point(138, 255);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(120, 25);
            this.simpleButton3.TabIndex = 6;
            this.simpleButton3.Values.Text = "Compare to original";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);
            // 
            // simpleButton4
            // 
            this.simpleButton4.Enabled = false;
            this.simpleButton4.Location = new System.Drawing.Point(264, 255);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(124, 25);
            this.simpleButton4.TabIndex = 7;
            this.simpleButton4.Values.Text = "Create new from original";
            this.simpleButton4.Click += new System.EventHandler(this.simpleButton4_Click);
            // 
            // frmPartnumberLookup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 320);
            this.Controls.Add(this.simpleButton4);
            this.Controls.Add(this.simpleButton3);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.kryptonButtonBrowse);
            this.Controls.Add(this.kryptonTextBoxPartNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmPartnumberLookup";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Partnumber lookup";
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).EndInit();
            this.groupControl1.Panel.ResumeLayout(false);
            this.groupControl1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTextBox kryptonTextBoxPartNumber;
        private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButtonBrowse;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox groupControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl3;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl7;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblFuel;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblECUType;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblCarModel;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton3;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblSoftwareID;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl10;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblRating;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl8;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblCarType;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl5;
    }
}