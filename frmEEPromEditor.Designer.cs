namespace VAGSuite
{
    partial class frmEEPromEditor
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
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.textEdit2 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.labelControl3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.textEdit3 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.checkEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.labelControl4 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.textEdit4 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.SuspendLayout();
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(100, 12);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(200, 23);
            this.textEdit1.TabIndex = 0;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 12);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(45, 20);
            this.labelControl1.TabIndex = 1;
            this.labelControl1.Values.Text = "IMMO:";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(12, 42);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(56, 20);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Values.Text = "Mileage:";
            // 
            // textEdit2
            // 
            this.textEdit2.Location = new System.Drawing.Point(100, 42);
            this.textEdit2.Name = "textEdit2";
            this.textEdit2.Size = new System.Drawing.Size(200, 23);
            this.textEdit2.TabIndex = 3;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(12, 72);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(33, 20);
            this.labelControl3.TabIndex = 4;
            this.labelControl3.Values.Text = "VIN:";
            // 
            // textEdit3
            // 
            this.textEdit3.Location = new System.Drawing.Point(100, 72);
            this.textEdit3.Name = "textEdit3";
            this.textEdit3.Size = new System.Drawing.Size(200, 23);
            this.textEdit3.TabIndex = 5;
            // 
            // checkEdit1
            // 
            this.checkEdit1.Location = new System.Drawing.Point(100, 132);
            this.checkEdit1.Name = "checkEdit1";
            this.checkEdit1.Size = new System.Drawing.Size(93, 20);
            this.checkEdit1.TabIndex = 6;
            this.checkEdit1.Values.Text = "IMMO Active";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(144, 170);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 7;
            this.simpleButton1.Values.Text = "Save";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(225, 170);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 8;
            this.simpleButton2.Values.Text = "Cancel";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(12, 102);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(33, 20);
            this.labelControl4.TabIndex = 9;
            this.labelControl4.Values.Text = "Key:";
            // 
            // textEdit4
            // 
            this.textEdit4.Location = new System.Drawing.Point(100, 102);
            this.textEdit4.Name = "textEdit4";
            this.textEdit4.Size = new System.Drawing.Size(200, 23);
            this.textEdit4.TabIndex = 10;
            // 
            // frmEEPromEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(312, 210);
            this.Controls.Add(this.textEdit4);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.checkEdit1);
            this.Controls.Add(this.textEdit3);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.textEdit2);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.textEdit1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmEEPromEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EEPROM Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl3;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit3;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox checkEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl4;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit4;
    }
}