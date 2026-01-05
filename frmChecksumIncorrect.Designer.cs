namespace VAGSuite
{
    partial class frmChecksumIncorrect
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
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.textEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.textEdit2 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.labelControl3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.textEdit3 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.labelControl4 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.textEdit4 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.labelControl5 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.labelControl6 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.SuspendLayout();
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 12);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(360, 40);
            this.labelControl1.StateCommon.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Values.Text = "The checksums in the file are incorrect.\r\nDo you want to update them now?";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(12, 65);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(95, 20);
            this.labelControl2.TabIndex = 1;
            this.labelControl2.Values.Text = "Checksum type:";
            // 
            // textEdit1
            // 
            this.textEdit1.Location = new System.Drawing.Point(150, 65);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.ReadOnly = true;
            this.textEdit1.Size = new System.Drawing.Size(222, 23);
            this.textEdit1.TabIndex = 2;
            // 
            // textEdit2
            // 
            this.textEdit2.Location = new System.Drawing.Point(150, 95);
            this.textEdit2.Name = "textEdit2";
            this.textEdit2.ReadOnly = true;
            this.textEdit2.Size = new System.Drawing.Size(222, 23);
            this.textEdit2.TabIndex = 4;
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(12, 95);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(132, 20);
            this.labelControl3.TabIndex = 3;
            this.labelControl3.Values.Text = "Number of checksums:";
            // 
            // textEdit3
            // 
            this.textEdit3.Location = new System.Drawing.Point(150, 125);
            this.textEdit3.Name = "textEdit3";
            this.textEdit3.ReadOnly = true;
            this.textEdit3.Size = new System.Drawing.Size(222, 23);
            this.textEdit3.TabIndex = 6;
            // 
            // labelControl4
            // 
            this.labelControl4.Location = new System.Drawing.Point(12, 125);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(114, 20);
            this.labelControl4.TabIndex = 5;
            this.labelControl4.Values.Text = "Checksums failed:";
            // 
            // textEdit4
            // 
            this.textEdit4.Location = new System.Drawing.Point(150, 155);
            this.textEdit4.Name = "textEdit4";
            this.textEdit4.ReadOnly = true;
            this.textEdit4.Size = new System.Drawing.Size(222, 23);
            this.textEdit4.TabIndex = 8;
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(12, 155);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(119, 20);
            this.labelControl5.TabIndex = 7;
            this.labelControl5.Values.Text = "Checksums passed:";
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(216, 200);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 9;
            this.simpleButton1.Values.Text = "Yes";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(297, 200);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 10;
            this.simpleButton2.Values.Text = "No";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // labelControl6
            // 
            this.labelControl6.Location = new System.Drawing.Point(12, 200);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(6, 2);
            this.labelControl6.TabIndex = 11;
            this.labelControl6.Values.Text = "";
            // 
            // frmChecksumIncorrect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 241);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.textEdit4);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.textEdit3);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.textEdit2);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.textEdit1);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmChecksumIncorrect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Checksum incorrect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl3;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit3;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl4;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox textEdit4;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl5;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl6;
    }
}