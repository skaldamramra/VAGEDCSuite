namespace VAGSuite
{
    partial class frmAbout
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
            this.panelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.labelControl1 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.labelControl2 = new System.Windows.Forms.Label();
            this.labelControl3 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl5 = new System.Windows.Forms.Label();
            this.labelControl6 = new System.Windows.Forms.Label();
            this.hyperLinkEdit1 = new ComponentFactory.Krypton.Toolkit.KryptonLinkLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).BeginInit();
            this.panelControl1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            //
            // panelControl1
            //
            this.panelControl1.Controls.Add(this.labelControl1);
            this.panelControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelControl1.Location = new System.Drawing.Point(0, 0);
            this.panelControl1.Name = "panelControl1";
            this.panelControl1.Size = new System.Drawing.Size(450, 70);
            this.panelControl1.TabIndex = 0;
            //
            // labelControl1
            //
            this.labelControl1.AutoSize = false;
            this.labelControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl1.Location = new System.Drawing.Point(0, 0);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(450, 70);
            this.labelControl1.StateCommon.ShortText.Font = new System.Drawing.Font("Source Sans Pro", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl1.StateCommon.ShortText.TextH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.labelControl1.StateCommon.ShortText.TextV = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Center;
            this.labelControl1.TabIndex = 0;
            this.labelControl1.Values.Text = "VAGEDCSuite";
            this.labelControl1.DoubleClick += new System.EventHandler(this.labelControl1_DoubleClick);
            //
            // simpleButton1
            //
            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.Location = new System.Drawing.Point(340, 310);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(90, 32);
            this.simpleButton1.TabIndex = 1;
            this.simpleButton1.Values.Text = "OK";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.labelControl2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelControl3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.hyperLinkEdit1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelControl6, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelControl5, 0, 4);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 80);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(426, 220);
            this.tableLayoutPanel1.TabIndex = 8;
            //
            // labelControl2
            //
            this.labelControl2.AutoSize = false;
            this.labelControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl2.Location = new System.Drawing.Point(3, 3);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(420, 40);
            this.labelControl2.TabIndex = 2;
            this.labelControl2.Text = "VAGEDCSuite was created with the help of lots of people on ecuconnections.com and chiptuners.org";
            this.labelControl2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelControl2.DoubleClick += new System.EventHandler(this.labelControl2_DoubleClick);
            //
            // labelControl3
            //
            this.labelControl3.AutoSize = true;
            this.labelControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl3.Location = new System.Drawing.Point(3, 49);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(420, 20);
            this.labelControl3.TabIndex = 3;
            this.labelControl3.Values.Text = "Special thanks go out to:";
            this.labelControl3.DoubleClick += new System.EventHandler(this.labelControl3_DoubleClick);
            //
            // hyperLinkEdit1
            //
            this.hyperLinkEdit1.AutoSize = true;
            this.hyperLinkEdit1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hyperLinkEdit1.Location = new System.Drawing.Point(3, 75);
            this.hyperLinkEdit1.Name = "hyperLinkEdit1";
            this.hyperLinkEdit1.Size = new System.Drawing.Size(420, 20);
            this.hyperLinkEdit1.TabIndex = 7;
            this.hyperLinkEdit1.Values.Text = "http://www.mtx-electronics.com";
            //
            // labelControl6
            //
            this.labelControl6.AutoSize = false;
            this.labelControl6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl6.Location = new System.Drawing.Point(3, 101);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(420, 40);
            this.labelControl6.TabIndex = 6;
            this.labelControl6.Text = "rkam, Pixis5, othmar77, dieseljohnny, bazare, bondiblu, Relic, Macadam, dilemma";
            this.labelControl6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // labelControl5
            //
            this.labelControl5.AutoSize = false;
            this.labelControl5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelControl5.Location = new System.Drawing.Point(3, 147);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(420, 40);
            this.labelControl5.TabIndex = 5;
            this.labelControl5.Text = "You can send an email for support to skaldamramra@yahoo.com";
            this.labelControl5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelControl5.DoubleClick += new System.EventHandler(this.labelControl5_DoubleClick);
            //
            // frmAbout
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 355);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.simpleButton1);
            this.Controls.Add(this.panelControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmAbout";
            this.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Custom;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About...";
            ((System.ComponentModel.ISupportInitialize)(this.panelControl1)).EndInit();
            this.panelControl1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel panelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private System.Windows.Forms.Label labelControl2;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl3;
        private System.Windows.Forms.Label labelControl5;
        private System.Windows.Forms.Label labelControl6;
        private ComponentFactory.Krypton.Toolkit.KryptonLinkLabel hyperLinkEdit1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}