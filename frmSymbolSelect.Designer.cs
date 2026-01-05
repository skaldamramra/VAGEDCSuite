namespace VAGSuite
{
    partial class frmSymbolSelect
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
            this.groupControl1 = new ComponentFactory.Krypton.Toolkit.KryptonGroupBox();
            this.lookUpEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonComboBox();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).BeginInit();
            this.groupControl1.Panel.SuspendLayout();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEdit1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupControl1
            // 
            this.groupControl1.Location = new System.Drawing.Point(12, 12);
            this.groupControl1.Name = "groupControl1";
            // 
            // groupControl1.Panel
            // 
            this.groupControl1.Panel.Controls.Add(this.lookUpEdit1);
            this.groupControl1.Size = new System.Drawing.Size(360, 80);
            this.groupControl1.TabIndex = 0;
            this.groupControl1.Values.Heading = "Select symbol";
            // 
            // lookUpEdit1
            // 
            this.lookUpEdit1.DropDownWidth = 320;
            this.lookUpEdit1.Location = new System.Drawing.Point(10, 15);
            this.lookUpEdit1.Name = "lookUpEdit1";
            this.lookUpEdit1.Size = new System.Drawing.Size(320, 21);
            this.lookUpEdit1.TabIndex = 0;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(216, 105);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(75, 25);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Values.Text = "Ok";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // simpleButton2
            // 
            this.simpleButton2.Location = new System.Drawing.Point(297, 105);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 2;
            this.simpleButton2.Values.Text = "Cancel";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);
            // 
            // frmSymbolSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 145);
            this.Controls.Add(this.simpleButton2);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.groupControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSymbolSelect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Symbol Selection";
            this.Load += new System.EventHandler(this.frmSymbolSelect_Load);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1.Panel)).EndInit();
            this.groupControl1.Panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEdit1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonGroupBox groupControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonComboBox lookUpEdit1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
    }
}