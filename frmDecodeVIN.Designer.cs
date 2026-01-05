namespace VAGSuite
{
    partial class frmDecodeVIN
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
            this.textEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblCarMake = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblCarModel = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblEngineType = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblMakeyear = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl6 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl7 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl8 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl9 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl10 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblPlant = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl12 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.lblChassis = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.SuspendLayout();
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(100, 12);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(200, 23);
            this.textEdit1.TabIndex = 0;
            this.textEdit1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textEdit1_KeyDown);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(306, 12);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Values.Text = "Decode";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 12);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(82, 20);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Values.Text = "VIN Number:";
            // 
            // lblCarMake
            // 
            this.lblCarMake.Location = new System.Drawing.Point(150, 50);
            this.lblCarMake.Name = "lblCarMake";
            this.lblCarMake.Size = new System.Drawing.Size(24, 20);
            this.lblCarMake.TabIndex = 3;
            this.lblCarMake.Values.Text = "---";
            // 
            // lblCarModel
            // 
            this.lblCarModel.Location = new System.Drawing.Point(150, 75);
            this.lblCarModel.Name = "lblCarModel";
            this.lblCarModel.Size = new System.Drawing.Size(24, 20);
            this.lblCarModel.TabIndex = 4;
            this.lblCarModel.Values.Text = "---";
            // 
            // lblEngineType
            // 
            this.lblEngineType.Location = new System.Drawing.Point(150, 100);
            this.lblEngineType.Name = "lblEngineType";
            this.lblEngineType.Size = new System.Drawing.Size(24, 20);
            this.lblEngineType.TabIndex = 5;
            this.lblEngineType.Values.Text = "---";
            // 
            // lblMakeyear
            // 
            this.lblMakeyear.Location = new System.Drawing.Point(150, 125);
            this.lblMakeyear.Name = "lblMakeyear";
            this.lblMakeyear.Size = new System.Drawing.Size(24, 20);
            this.lblMakeyear.TabIndex = 6;
            this.lblMakeyear.Values.Text = "---";
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(12, 50);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(43, 20);
            this.labelControl6.TabIndex = 7;
            this.labelControl6.Values.Text = "Make:";
            // 
            // labelControl7
            // 
            this.labelControl7.Location = new System.Drawing.Point(12, 75);
            this.labelControl7.Name = "labelControl7";
            this.labelControl7.Size = new System.Drawing.Size(48, 20);
            this.labelControl7.TabIndex = 8;
            this.labelControl7.Values.Text = "Model:";
            // 
            // labelControl8
            // 
            this.labelControl8.Location = new System.Drawing.Point(12, 100);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(79, 20);
            this.labelControl8.TabIndex = 9;
            this.labelControl8.Values.Text = "Engine type:";
            // 
            // labelControl9
            // 
            this.labelControl9.Location = new System.Drawing.Point(12, 125);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(68, 20);
            this.labelControl9.TabIndex = 10;
            this.labelControl9.Values.Text = "Make year:";
            // 
            // labelControl10
            // 
            this.labelControl10.Location = new System.Drawing.Point(12, 150);
            this.labelControl10.Name = "labelControl10";
            this.labelControl10.Size = new System.Drawing.Size(43, 20);
            this.labelControl10.TabIndex = 11;
            this.labelControl10.Values.Text = "Plant:";
            // 
            // lblPlant
            // 
            this.lblPlant.Location = new System.Drawing.Point(150, 150);
            this.lblPlant.Name = "lblPlant";
            this.lblPlant.Size = new System.Drawing.Size(24, 20);
            this.lblPlant.TabIndex = 12;
            this.lblPlant.Values.Text = "---";
            // 
            // labelControl12
            // 
            this.labelControl12.Location = new System.Drawing.Point(12, 175);
            this.labelControl12.Name = "labelControl12";
            this.labelControl12.Size = new System.Drawing.Size(53, 20);
            this.labelControl12.TabIndex = 13;
            this.labelControl12.Values.Text = "Chassis:";
            // 
            // lblChassis
            // 
            this.lblChassis.Location = new System.Drawing.Point(150, 175);
            this.lblChassis.Name = "lblChassis";
            this.lblChassis.Size = new System.Drawing.Size(24, 20);
            this.lblChassis.TabIndex = 14;
            this.lblChassis.Values.Text = "---";
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(306, 210);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 15;
            this.simpleButton2.Values.Text = "Close";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // frmDecodeVIN
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 251);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.lblChassis);
            this.Controls.Add(this.labelControl12);
            this.Controls.Add(this.lblPlant);
            this.Controls.Add(this.labelControl10);
            this.Controls.Add(this.labelControl9);
            this.Controls.Add(this.labelControl8);
            this.Controls.Add(this.labelControl7);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.lblMakeyear);
            this.Controls.Add(this.lblEngineType);
            this.Controls.Add(this.lblCarModel);
            this.Controls.Add(this.lblCarMake);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.textEdit1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDecodeVIN";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Decode VIN Number";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblCarMake;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblCarModel;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblEngineType;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblMakeyear;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl6;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl7;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl8;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl9;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl10;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblPlant;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl12;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel lblChassis;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
    }
}