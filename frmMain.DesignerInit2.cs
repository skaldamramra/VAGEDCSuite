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
            // Dock manager
            this.dockManager1.Controller = this.barAndDockingController1;
            this.dockManager1.Form = this;
            this.dockManager1.LayoutVersion = "1.1.8";
            this.dockManager1.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
                this.dockSymbols
            });
            this.dockManager1.TopZIndexControls.AddRange(new string[] {
                "DevExpress.XtraBars.BarDockControl",
                "DevExpress.XtraBars.StandaloneBarDockControl",
                "System.Windows.Forms.StatusBar",
                "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
                "DevExpress.XtraBars.Ribbon.RibbonControl"
            });

            // Symbols dock panel
            this.dockSymbols.Appearance.BackColor = System.Drawing.Color.FromArgb(235, 236, 239);
            this.dockSymbols.Appearance.Options.UseBackColor = true;
            this.dockSymbols.Controls.Add(this.dockPanel1_Container);
            this.dockSymbols.Dock = DevExpress.XtraBars.Docking.DockingStyle.Left;
            this.dockSymbols.ID = new System.Guid("fef7db83-4a1b-495e-96c7-e0c2d4294391");
            this.dockSymbols.Location = new System.Drawing.Point(0, LayoutConstants.RibbonHeight);
            this.dockSymbols.Name = "dockSymbols";
            this.dockSymbols.Options.AllowFloating = false;
            this.dockSymbols.Options.ShowCloseButton = false;
            this.dockSymbols.OriginalSize = new System.Drawing.Size(
                LayoutConstants.SymbolDockPanelWidth, 
                LayoutConstants.SymbolDockPanelHeight);
            this.dockSymbols.Size = new System.Drawing.Size(
                LayoutConstants.SymbolDockPanelWidth, 
                347);
            this.dockSymbols.Text = "Maps";

            // Container
            this.dockPanel1_Container.Controls.Add(this.gridControl1);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 24);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(519, 319);
            this.dockPanel1_Container.TabIndex = 0;
        }

        /// <summary>
        /// Initializes grid controls (symbol grid)
        /// </summary>
        private void InitializeGridControls()
        {
            // Grid control
            this.gridControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 0);
            this.gridControl1.MainView = this.gridViewSymbols;
            this.gridControl1.MenuManager = this.ribbonControl1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(519, 319);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
                this.gridViewSymbols
            });
            this.gridControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.gridControl1_MouseMove);

            // Configure grid view and columns
            ConfigureGridView();
            ConfigureGridColumns();
        }

        /// <summary>
        /// Configures grid view settings
        /// </summary>
        private void ConfigureGridView()
        {
            this.gridViewSymbols.CustomizationFormBounds = new System.Drawing.Rectangle(627, 480, 208, 170);
            this.gridViewSymbols.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFocus;
            this.gridViewSymbols.GridControl = this.gridControl1;
            this.gridViewSymbols.GroupCount = 2;
            this.gridViewSymbols.GroupSummary.AddRange(new DevExpress.XtraGrid.GridSummaryItem[] {
                new DevExpress.XtraGrid.GridGroupSummaryItem(DevExpress.Data.SummaryItemType.Count, "Category", null, "({0})")
            });
            this.gridViewSymbols.Name = "gridViewSymbols";
            this.gridViewSymbols.OptionsBehavior.AllowIncrementalSearch = true;
            this.gridViewSymbols.OptionsFilter.UseNewCustomFilterDialog = true;
            this.gridViewSymbols.OptionsSelection.EnableAppearanceFocusedCell = false;
            this.gridViewSymbols.OptionsView.ShowGroupPanel = false;
            this.gridViewSymbols.OptionsView.ShowIndicator = false;
            this.gridViewSymbols.PreviewFieldName = "Description";

            // Sort info
            this.gridViewSymbols.SortInfo.AddRange(new DevExpress.XtraGrid.Columns.GridColumnSortInfo[] {
                new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.gcSymbolCategory, DevExpress.Data.ColumnSortOrder.Ascending),
                new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.gcSymbolSubCategory, DevExpress.Data.ColumnSortOrder.Ascending),
                new DevExpress.XtraGrid.Columns.GridColumnSortInfo(this.gcSymbolName, DevExpress.Data.ColumnSortOrder.Ascending)
            });

            // Event handlers
            this.gridViewSymbols.CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(this.gridViewSymbols_CellValueChanged);
            this.gridViewSymbols.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridViewSymbols_KeyDown);
            this.gridViewSymbols.CustomDrawCell += new DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventHandler(this.gridView1_CustomDrawCell);
            this.gridViewSymbols.DoubleClick += new System.EventHandler(this.gridView1_DoubleClick);
        }

        /// <summary>
        /// Configures grid columns
        /// </summary>
        private void ConfigureGridColumns()
        {
            // Create columns using helper
            this.gcSymbolName = CreateGridColumn(
                "gcSymbolName", "Name", "Varname", 
                visible: true, width: LayoutConstants.SymbolNameColumnWidth);

            this.gcSymbolAddress = CreateGridColumn(
                "gcSymbolAddress", "Address", "Flash_start_address", 
                visible: true, width: LayoutConstants.SymbolAddressColumnWidth);

            this.gcSymbolLength = CreateGridColumn(
                "gcSymbolLength", "Length", "Length", 
                visible: false);

            this.gcSymbolXLen = CreateGridColumn(
                "gcSymbolXLen", "Width", "X_axis_length", 
                visible: false);

            this.gcSymbolYLen = CreateGridColumn(
                "gcSymbolYLen", "Height", "Y_axis_length", 
                visible: false);

            this.gcSymbolCategory = CreateGridColumn(
                "gcSymbolCategory", "Category", "Category", 
                visible: false);
            this.gcSymbolCategory.GroupFormat.FormatString = "{0}: [#image]{1} {2}";

            this.gcSymbolCodeblock = CreateGridColumn(
                "gcSymbolCodeblock", "Codeblock", "CodeBlock", 
                visible: false);

            this.gcSymbolDescription = CreateGridColumn(
                "gcSymbolDescription", "Description", "Description", 
                visible: false, width: LayoutConstants.SymbolDescriptionColumnWidth);

            this.gcSymbolSubCategory = CreateGridColumn(
                "gcSymbolSubCategory", "Subcategory", "Subcategory", 
                visible: false);
            this.gcSymbolSubCategory.GroupFormat.FormatString = "{0}: [#image]{1} {2}";

            this.gcSymbolUserdescription = CreateGridColumn(
                "gcSymbolUserdescription", "Userdescription", "Userdescription", 
                visible: true, width: LayoutConstants.SymbolUserDescriptionWidth, readOnly: false);

            this.gcSymbolXID = CreateGridColumn(
                "gcSymbolXID", "Y axis ID", "X_axis_ID", 
                visible: false);

            this.gcSymbolYID = CreateGridColumn(
                "gcSymbolYID", "X axis ID", "Y_axis_ID", 
                visible: false);

            this.gcSymbolXDescr = CreateGridColumn(
                "gcSymbolXDescr", "Y axis descr", "Y_axis_descr", 
                visible: false);

            this.gcSymbolYDescr = CreateGridColumn(
                "gcSymbolYDescr", "X axis descr", "X_axis_descr", 
                visible: false);

            // Add columns to grid view
            this.gridViewSymbols.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
                this.gcSymbolName, this.gcSymbolAddress, this.gcSymbolLength,
                this.gcSymbolXLen, this.gcSymbolYLen, this.gcSymbolCategory,
                this.gcSymbolCodeblock, this.gcSymbolDescription, this.gcSymbolSubCategory,
                this.gcSymbolUserdescription, this.gcSymbolXID, this.gcSymbolYID,
                this.gcSymbolXDescr, this.gcSymbolYDescr
            });
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