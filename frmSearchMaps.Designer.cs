namespace VAGSuite
{
    partial class frmSearchMaps
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
            this.kryptonNumericUpDown1 = new ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown();
            this.kryptonGroupBox1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.kryptonTextBox1 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.kryptonCheckBox4 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.kryptonCheckBox3 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.kryptonCheckBox2 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.kryptonCheckBox1 = new ComponentFactory.Krypton.Toolkit.KryptonCheckBox();
            this.kryptonButtonOk = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.kryptonButtonCancel = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1.Panel)).BeginInit();
            this.kryptonGroupBox1.Panel.SuspendLayout();
            this.kryptonGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // kryptonNumericUpDown1
            // 
            this.kryptonNumericUpDown1.Location = new System.Drawing.Point(174, 40);
            this.kryptonNumericUpDown1.Name = "kryptonNumericUpDown1";
            this.kryptonNumericUpDown1.Size = new System.Drawing.Size(207, 22);
            this.kryptonNumericUpDown1.TabIndex = 0;
            // 
            // kryptonGroupBox1
            // 
            this.kryptonGroupBox1.Location = new System.Drawing.Point(12, 12);
            this.kryptonGroupBox1.Name = "kryptonGroupBox1";
            // 
            // kryptonGroupBox1.Panel
            // 
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonTextBox1);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonCheckBox4);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonCheckBox3);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonCheckBox2);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonCheckBox1);
            this.kryptonGroupBox1.Panel.Controls.Add(this.kryptonNumericUpDown1);
            this.kryptonGroupBox1.Size = new System.Drawing.Size(407, 187);
            this.kryptonGroupBox1.TabIndex = 2;
            this.kryptonGroupBox1.Values.Heading = "Search options";
            // 
            // kryptonTextBox1
            // 
            this.kryptonTextBox1.Location = new System.Drawing.Point(174, 75);
            this.kryptonTextBox1.Name = "kryptonTextBox1";
            this.kryptonTextBox1.Size = new System.Drawing.Size(207, 23);
            this.kryptonTextBox1.TabIndex = 6;
            // 
            // kryptonCheckBox4
            // 
            this.kryptonCheckBox4.Location = new System.Drawing.Point(15, 75);
            this.kryptonCheckBox4.Name = "kryptonCheckBox4";
            this.kryptonCheckBox4.Size = new System.Drawing.Size(148, 20);
            this.kryptonCheckBox4.TabIndex = 5;
            this.kryptonCheckBox4.Values.Text = "Search for string value";
            // 
            // kryptonCheckBox3
            // 
            this.kryptonCheckBox3.Checked = true;
            this.kryptonCheckBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.kryptonCheckBox3.Location = new System.Drawing.Point(15, 40);
            this.kryptonCheckBox3.Name = "kryptonCheckBox3";
            this.kryptonCheckBox3.Size = new System.Drawing.Size(161, 20);
            this.kryptonCheckBox3.TabIndex = 4;
            this.kryptonCheckBox3.Values.Text = "Search for numeric value";
            // 
            // kryptonCheckBox2
            // 
            this.kryptonCheckBox2.Location = new System.Drawing.Point(15, 148);
            this.kryptonCheckBox2.Name = "kryptonCheckBox2";
            this.kryptonCheckBox2.Size = new System.Drawing.Size(175, 20);
            this.kryptonCheckBox2.TabIndex = 3;
            this.kryptonCheckBox2.Values.Text = "Include symbol descriptions";
            // 
            // kryptonCheckBox1
            // 
            this.kryptonCheckBox1.Location = new System.Drawing.Point(15, 118);
            this.kryptonCheckBox1.Name = "kryptonCheckBox1";
            this.kryptonCheckBox1.Size = new System.Drawing.Size(145, 20);
            this.kryptonCheckBox1.TabIndex = 2;
            this.kryptonCheckBox1.Values.Text = "Include symbol names";
            // 
            // kryptonButtonOk
            // 
            this.kryptonButtonOk.Location = new System.Drawing.Point(345, 205);
            this.kryptonButtonOk.Name = "kryptonButtonOk";
            this.kryptonButtonOk.Size = new System.Drawing.Size(75, 25);
            this.kryptonButtonOk.TabIndex = 3;
            this.kryptonButtonOk.Values.Text = "Ok";
            this.kryptonButtonOk.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // kryptonButtonCancel
            // 
            this.kryptonButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.kryptonButtonCancel.Location = new System.Drawing.Point(264, 205);
            this.kryptonButtonCancel.Name = "kryptonButtonCancel";
            this.kryptonButtonCancel.Size = new System.Drawing.Size(75, 25);
            this.kryptonButtonCancel.TabIndex = 4;
            this.kryptonButtonCancel.Values.Text = "Cancel";
            this.kryptonButtonCancel.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // frmSearchMaps
            // 
            this.AcceptButton = this.kryptonButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.kryptonButtonCancel;
            this.ClientSize = new System.Drawing.Size(432, 238);
            this.Controls.Add(this.kryptonButtonCancel);
            this.Controls.Add(this.kryptonButtonOk);
            this.Controls.Add(this.kryptonGroupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSearchMaps";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Search maps for value...";
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1.Panel)).EndInit();
            this.kryptonGroupBox1.Panel.ResumeLayout(false);
            this.kryptonGroupBox1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kryptonGroupBox1)).EndInit();
            this.kryptonGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonNumericUpDown kryptonNumericUpDown1;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox kryptonGroupBox1;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox kryptonCheckBox2;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox kryptonCheckBox1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButtonOk;
        private ComponentFactory.Krypton.Toolkit.KryptonButton kryptonButtonCancel;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox kryptonTextBox1;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox kryptonCheckBox4;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckBox kryptonCheckBox3;
    }
}