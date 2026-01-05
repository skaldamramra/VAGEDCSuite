namespace VAGSuite
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            
            this.SuspendLayout();

            // Step 1: Create all component instances (ribbon, dock, grid, etc.)
            InitializeComponentInstances();
            
            // Step 2: Configure ribbon infrastructure
            InitializeRibbonInfrastructure();
            InitializeKryptonRibbon();
            
            // Step 3: Initialize status bar items (needed for AddButtonsToRibbon)
            InitializeStatusBar();
            InitializeKryptonStatusBar();
            
            // Step 4: Create and configure all buttons
            InitializeRibbonButtons();
            
            // Step 5: Initialize docking controls
            InitializeDockingControls();
            InitializeKryptonDocking();
            
            // Step 6: Initialize grid controls
            InitializeGridControls();
            InitializeSymbolGrid();
            
            // Step 7: Initialize context menus
            InitializeContextMenus();
            
            // Step 8: Configure form layout
            ConfigureLayout();
            
            // Step 9: Configure ribbon pages and groups
            ConfigureRibbonPages();
            
            // Step 10: Wire event handlers for all controls
            FinalizeInitialization();
            
            // Step 11: End initialization (EndInit calls)
            EndComponentInitialization();

            this.ResumeLayout(false);
        }

        #endregion

        // Field declarations - all UI control fields
        private ComponentFactory.Krypton.Toolkit.KryptonManager kryptonManager1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbon kryptonRibbon1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab kryptonRibbonTabFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab kryptonRibbonTabActions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab kryptonRibbonTabTuning;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab kryptonRibbonTabSkins;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonTab kryptonRibbonTabHelp;
        private System.Windows.Forms.StatusStrip kryptonStatusStrip1;

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editXAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editYAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToExcelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToXMLToolStripMenuItem;
    }
}
