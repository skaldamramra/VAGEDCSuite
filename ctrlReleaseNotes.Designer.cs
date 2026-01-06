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
            this.gcLink = new System.Windows.Forms.DataGridViewLinkColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.printPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnViewOnGitHub = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.panelTop = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            //
            // gridControl1
            //
            this.gridControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 40);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(587, 344);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gcVersion,
            this.gcDescription,
            this.gcTitle,
            this.gcDate,
            this.gcLink});
            this.gridControl1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridControl1_CellContentClick);
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
            this.gcLink.VisitedLinkColor = System.Drawing.Color.Blue;
            this.gcLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
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
            // btnViewOnGitHub
            //
            this.btnViewOnGitHub.Location = new System.Drawing.Point(5, 5);
            this.btnViewOnGitHub.Name = "btnViewOnGitHub";
            this.btnViewOnGitHub.Size = new System.Drawing.Size(120, 30);
            this.btnViewOnGitHub.TabIndex = 1;
            this.btnViewOnGitHub.Values.Text = "View on GitHub";
            this.btnViewOnGitHub.Click += new System.EventHandler(this.btnViewOnGitHub_Click);
            //
            // panelTop
            //
            this.panelTop.Controls.Add(this.btnViewOnGitHub);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(587, 40);
            this.panelTop.TabIndex = 1;
            //
            // ctrlReleaseNotes
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.panelTop);
            this.Name = "ctrlReleaseNotes";
            this.Size = new System.Drawing.Size(587, 384);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonDataGridView gridControl1;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn gcDate;
        private System.Windows.Forms.DataGridViewLinkColumn gcLink;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem printPreviewToolStripMenuItem;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnViewOnGitHub;
        private System.Windows.Forms.Panel panelTop;
    }
}
