namespace VAGSuite
{
    partial class frmBinmerger
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBinmerger));
            this.buttonEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.btnBrowse1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.buttonEdit2 = new ComponentFactory.Krypton.Toolkit.KryptonTextBox();
            this.btnBrowse2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.groupControl1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.labelControl2 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).BeginInit();
            this.groupControl1.Panel.SuspendLayout();
            this.groupControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonEdit1
            // 
            this.buttonEdit1.Location = new System.Drawing.Point(100, 15);
            this.buttonEdit1.Name = "buttonEdit1";
            this.buttonEdit1.Size = new System.Drawing.Size(250, 23);
            this.buttonEdit1.TabIndex = 0;
            // 
            // btnBrowse1
            // 
            this.btnBrowse1.Location = new System.Drawing.Point(355, 15);
            this.btnBrowse1.Name = "btnBrowse1";
            this.btnBrowse1.Size = new System.Drawing.Size(30, 23);
            this.btnBrowse1.TabIndex = 1;
            this.btnBrowse1.Values.Text = "...";
            this.btnBrowse1.Click += new System.EventHandler(this.btnBrowse1_Click);
            // 
            // buttonEdit2
            // 
            this.buttonEdit2.Location = new System.Drawing.Point(100, 45);
            this.buttonEdit2.Name = "buttonEdit2";
            this.buttonEdit2.Size = new System.Drawing.Size(250, 23);
            this.buttonEdit2.TabIndex = 2;
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Location = new System.Drawing.Point(355, 45);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(30, 23);
            this.btnBrowse2.TabIndex = 3;
            this.btnBrowse2.Values.Text = "...";
            this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(230, 120);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(80, 25);
            this.simpleButton1.TabIndex = 4;
            this.simpleButton1.Values.Text = "Merge";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(315, 120);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(80, 25);
            this.simpleButton2.TabIndex = 5;
            this.simpleButton2.Values.Text = "Close";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // groupControl1
            // 
            this.groupControl1.Location = new System.Drawing.Point(10, 10);
            this.groupControl1.Name = "groupControl1";
            // 
            // groupControl1.Panel
            // 
            this.groupControl1.Panel.Controls.Add(this.labelControl2);
            this.groupControl1.Panel.Controls.Add(this.labelControl1);
            this.groupControl1.Panel.Controls.Add(this.buttonEdit1);
            this.groupControl1.Panel.Controls.Add(this.btnBrowse1);
            this.groupControl1.Panel.Controls.Add(this.buttonEdit2);
            this.groupControl1.Panel.Controls.Add(this.btnBrowse2);
            this.groupControl1.Size = new System.Drawing.Size(395, 100);
            this.groupControl1.TabIndex = 6;
            this.groupControl1.Values.Heading = "Files to merge";
            // 
            // labelControl2
            // 
            this.labelControl2.Location = new System.Drawing.Point(10, 45);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(80, 20);
            this.labelControl2.TabIndex = 7;
            this.labelControl2.Values.Text = "Second file:";
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(10, 15);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(80, 20);
            this.labelControl1.TabIndex = 8;
            this.labelControl1.Values.Text = "First file:";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "bin";
            this.saveFileDialog1.Filter = "Binary files|*.bin|All files|*.*";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Binary files|*.bin|All files|*.*";
            // 
            // frmBinmerger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 155);
            this.Controls.Add(this.groupControl1);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmBinmerger";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Binary Merger";
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).EndInit();
            this.groupControl1.Panel.ResumeLayout(false);
            this.groupControl1.Panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonTextBox buttonEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnBrowse1;
        private ComponentFactory.Krypton.Toolkit.KryptonTextBox buttonEdit2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnBrowse2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox groupControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}