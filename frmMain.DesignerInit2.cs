namespace VAGSuite
{
    public partial class frmMain
    {
        /// <summary>
        /// Initializes status bar items
        /// </summary>
        private void InitializeStatusBar()
        {
            // Create status items
            this.barFilenameText = CreateBarStaticItem(
                "barFilenameText", 
                "---", 
                ControlIds.FilenameText,
                hint: "Shows the currently opened file");

            this.barChecksum = CreateBarStaticItem(
                "barChecksum", 
                "---", 
                ControlIds.ChecksumStatus);

            this.barReadOnly = CreateBarStaticItem(
                "barReadOnly", 
                "---", 
                ControlIds.ReadOnly,
                hint: "Indicates whether access to the current file is ok or not");

            this.barPartnumber = CreateBarStaticItem(
                "barPartnumber", 
                "---", 
                ControlIds.Partnumber,
                hint: "Shows the partnumber of the opened file");

            this.barAdditionalInfo = CreateBarStaticItem(
                "barAdditionalInfo", 
                "---", 
                ControlIds.AdditionalInfo);
            this.barAdditionalInfo.Description = "Shows additional information about the opened file";

            this.barCodeBlock1 = CreateBarStaticItem(
                "barCodeBlock1", 
                "AUT", 
                ControlIds.CodeBlock1,
                hint: "Maps for automatic transmission present");
            this.barCodeBlock1.Enabled = false;

            this.barCodeBlock2 = CreateBarStaticItem(
                "barCodeBlock2", 
                "MAN", 
                ControlIds.CodeBlock2,
                hint: "Maps for manual transmission present");
            this.barCodeBlock2.Enabled = false;

            this.barCodeBlock3 = CreateBarStaticItem(
                "barCodeBlock3", 
                "4WD", 
                ControlIds.CodeBlock3,
                hint: "Maps for quattro cars present");
            this.barCodeBlock3.Enabled = false;

            this.barSymCount = CreateBarStaticItem(
                "barSymCount", 
                "0 symbols", 
                ControlIds.SymCount);
            this.barSymCount.Description = "Shows how many symbols where detected in the opened file";

            this.barUpdateText = CreateBarStaticItem(
                "barUpdateText", 
                "No updates", 
                ControlIds.UpdateText);
            this.barUpdateText.Description = "Shows the automatic updater status";

            // Configure status bar
            this.ribbonStatusBar1.ItemLinks.Add(this.barFilenameText);
            this.ribbonStatusBar1.ItemLinks.Add(this.barChecksum);
            this.ribbonStatusBar1.ItemLinks.Add(this.barReadOnly);
            this.ribbonStatusBar1.ItemLinks.Add(this.barPartnumber);
            this.ribbonStatusBar1.ItemLinks.Add(this.barAdditionalInfo);
            this.ribbonStatusBar1.ItemLinks.Add(this.barCodeBlock1);
            this.ribbonStatusBar1.ItemLinks.Add(this.barCodeBlock2);
            this.ribbonStatusBar1.ItemLinks.Add(this.barCodeBlock3);
            this.ribbonStatusBar1.ItemLinks.Add(this.barSymCount);
            this.ribbonStatusBar1.ItemLinks.Add(this.barUpdateText);
            this.ribbonStatusBar1.Location = new System.Drawing.Point(0, 489);
            this.ribbonStatusBar1.Name = "ribbonStatusBar1";
            this.ribbonStatusBar1.Ribbon = this.ribbonControl1;
            this.ribbonStatusBar1.Size = new System.Drawing.Size(
                LayoutConstants.DefaultFormWidth, 
                LayoutConstants.StatusBarHeight);
        }

        /// <summary>
        /// Initializes docking controls (symbol panel)
        /// </summary>
        private void InitializeDockingControls()
        {
            // DevExpress Docking is disabled in SetupDockingHierarchy
        }

        /// <summary>
        /// Initializes grid controls (symbol grid)
        /// </summary>
        private void InitializeGridControls()
        {
            // Grid initialization moved to frmMain.SymbolList.cs
        }

        /// <summary>
        /// Initializes context menus
        /// </summary>
        private void InitializeContextMenus()
        {
            // Create menu items
            this.editXAxisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editYAxisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToExcelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            // Configure context menu
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.editXAxisToolStripMenuItem,
                this.editYAxisToolStripMenuItem,
                this.exportToExcelToolStripMenuItem,
                this.exportToCSVToolStripMenuItem,
                this.exportToXMLToolStripMenuItem
            });
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(
                LayoutConstants.ContextMenuWidth, 
                LayoutConstants.ContextMenuHeight);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);

            // Edit X Axis
            this.editXAxisToolStripMenuItem.Name = "editXAxisToolStripMenuItem";
            this.editXAxisToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.editXAxisToolStripMenuItem.Text = "Edit x axis";
            this.editXAxisToolStripMenuItem.Click += new System.EventHandler(this.editXAxisToolStripMenuItem_Click);

            // Edit Y Axis
            this.editYAxisToolStripMenuItem.Name = "editYAxisToolStripMenuItem";
            this.editYAxisToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.editYAxisToolStripMenuItem.Text = "Edit y axis";
            this.editYAxisToolStripMenuItem.Click += new System.EventHandler(this.editYAxisToolStripMenuItem_Click);

            // Export to Excel
            this.exportToExcelToolStripMenuItem.Name = "exportToExcelToolStripMenuItem";
            this.exportToExcelToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.exportToExcelToolStripMenuItem.Text = "Export to Excel";
            this.exportToExcelToolStripMenuItem.Click += new System.EventHandler(this.exportToExcelToolStripMenuItem_Click);

            // Export to CSV
            this.exportToCSVToolStripMenuItem.Name = "exportToCSVToolStripMenuItem";
            this.exportToCSVToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.exportToCSVToolStripMenuItem.Text = "Export to CSV";
            this.exportToCSVToolStripMenuItem.Click += new System.EventHandler(this.exportToCSVToolStripMenuItem_Click);

            // Export to XML
            this.exportToXMLToolStripMenuItem.Name = "exportToXMLToolStripMenuItem";
            this.exportToXMLToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.exportToXMLToolStripMenuItem.Text = "Export to XML";
            this.exportToXMLToolStripMenuItem.Click += new System.EventHandler(this.exportToXMLToolStripMenuItem_Click);
        }
    }
}