namespace VAGSuite
{
    partial class ctrlReleaseNotes
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gridControl1 = new ComponentFactory.Krypton.Toolkit.KryptonDataGridView();
            this.gcVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gcDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gcTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gcDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gcLink = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gridControl1
            // 
            this.gridControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(587, 384);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gcVersion,
            this.gcDescription,
            this.gcTitle,
            this.gcDate,
            this.gcLink});
            //
            // gcVersion
            //
            this.gcVersion.HeaderText = "Version";
            this.gcVersion.DataPropertyName = "version";
            this.gcVersion.Name = "gcVersion";
            //
            // gcDescription
            //
            this.gcDescription.HeaderText = "Description";
            this.gcDescription.DataPropertyName = "description";
            this.gcDescription.Name = "gcDescription";
            this.gcDescription.Width = 138;
            //
            // gcTitle
            //
            this.gcTitle.HeaderText = "Title";
            this.gcTitle.DataPropertyName = "title";
            this.gcTitle.Name = "gcTitle";
            this.gcTitle.Width = 347;
            //
            // gcDate
            //
            this.gcDate.HeaderText = "Date";
            this.gcDate.DataPropertyName = "Date";
            this.gcDate.Name = "gcDate";
            this.gcDate.Width = 81;
            //
            // gcLink
            //
            this.gcLink.HeaderText = "Link";
            this.gcLink.DataPropertyName = "link";
            this.gcLink.Name = "gcLink";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.printPreviewToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 48);
            // 
            // printPreviewToolStripMenuItem
            // 
            this.printPreviewToolStripMenuItem.Name = "printPreviewToolStripMenuItem";
            this.printPreviewToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.printPreviewToolStripMenuItem.Text = "Print preview";
            this.printPreviewToolStripMenuItem.Click += new System.EventHandler(this.printPreviewToolStripMenuItem_Click);
            // 
            // ctrlReleaseNotes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl1);
            this.Name = "ctrlReleaseNotes";
            this.Size = new System.Drawing.Size(587, 384);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView gridControl1;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcLink;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
    }
}
