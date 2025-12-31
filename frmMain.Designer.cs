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
            
            // Step 3: Initialize status bar items (needed for AddButtonsToRibbon)
            InitializeStatusBar();
            
            // Step 4: Create and configure all buttons
            InitializeRibbonButtons();
            
            // Step 5: Initialize docking controls
            InitializeDockingControls();
            
            // Step 6: Initialize grid controls
            InitializeGridControls();
            
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
        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribFile;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbpgGeneralFile;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar1;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribActions;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribpgGeneralActions;
        private DevExpress.XtraBars.BarButtonItem btnBinaryCompare;
        private DevExpress.XtraBars.BarButtonItem btnOpenFile;
        private DevExpress.XtraBars.Docking.DockManager dockManager1;
        private DevExpress.XtraBars.Docking.DockPanel dockSymbols;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridViewSymbols;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolName;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolAddress;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolLength;
        private DevExpress.XtraBars.BarButtonItem btnCompareFiles;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolXLen;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolYLen;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolCategory;
        private DevExpress.XtraBars.BarStaticItem barPartnumber;
        private DevExpress.XtraBars.BarStaticItem barAdditionalInfo;
        private DevExpress.XtraBars.BarStaticItem barCodeBlock1;
        private DevExpress.XtraBars.BarStaticItem barCodeBlock2;
        private DevExpress.XtraBars.BarStaticItem barCodeBlock3;
        private DevExpress.XtraBars.BarStaticItem barSymCount;
        private DevExpress.XtraBars.BarButtonItem btnTestFiles;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolCodeblock;
        private DevExpress.XtraBars.BarButtonItem btnAppSettings;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnSettings;
        private DevExpress.XtraBars.Ribbon.RibbonPage rbnPageSkins;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageGroupSkins;
        private DevExpress.LookAndFeel.DefaultLookAndFeel defaultLookAndFeel1;
        private DevExpress.XtraBars.BarButtonItem btnCheckForUpdates;
        private DevExpress.XtraBars.Ribbon.RibbonPage rbnPageHelp;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageGroupUpdate;
        private DevExpress.XtraBars.BarStaticItem barUpdateText;
        private DevExpress.XtraBars.BarButtonItem btnReleaseNotes;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageDocumentation;
        private DevExpress.XtraBars.BarButtonItem btnEDC15PDocumentation;
        private DevExpress.XtraBars.BarButtonItem btnAbout;
        private DevExpress.XtraBars.Ribbon.RibbonPage rbnPageTuning;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageInjection;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageTurbo;
        private DevExpress.XtraBars.BarButtonItem btnChecksum;
        private DevExpress.XtraBars.BarButtonItem btnFirmwareInformation;
        private DevExpress.XtraBars.BarButtonItem btnVINDecoder;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageGroupInformation;
        private DevExpress.XtraBars.BarButtonItem btnViewFileInHex;
        private DevExpress.XtraBars.BarButtonItem btnSearchMaps;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageGroupTools;
        private DevExpress.XtraBars.BarButtonItem btnSaveAs;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rbnPageGourpProjects;
        private DevExpress.XtraBars.BarButtonItem btnCreateProject;
        private DevExpress.XtraBars.BarButtonItem btnCreateAProject;
        private DevExpress.XtraBars.BarButtonItem btnOpenProject;
        private DevExpress.XtraBars.BarButtonItem btnCloseProject;
        private DevExpress.XtraBars.BarButtonItem btnShowTransactionLog;
        private DevExpress.XtraBars.BarButtonItem btnRollback;
        private DevExpress.XtraBars.BarButtonItem btnRollforward;
        private DevExpress.XtraBars.BarButtonItem btnRebuildFile;
        private DevExpress.XtraBars.BarButtonItem btnEditProject;
        private DevExpress.XtraBars.BarButtonItem btnAddNoteToProject;
        private DevExpress.XtraBars.BarButtonItem btnShowProjectLogbook;
        private DevExpress.XtraBars.BarButtonItem btnProduceLatestBinary;
        private DevExpress.XtraBars.BarStaticItem barFilenameText;
        private DevExpress.XtraBars.BarButtonItem btnCreateBackup;
        private DevExpress.XtraBars.BarButtonItem btnLookupPartnumber;
        private DevExpress.XtraBars.BarButtonItem btnDriverWish;
        private DevExpress.XtraBars.BarButtonItem btnTorqueLimiter;
        private DevExpress.XtraBars.BarButtonItem btnSmokeLimiter;
        private DevExpress.XtraBars.BarButtonItem btnTargetBoost;
        private DevExpress.XtraBars.BarButtonItem btnBoostPressureLimiter;
        private DevExpress.XtraBars.BarButtonItem btnBoostPressureLimitSVBL;
        private DevExpress.XtraBars.BarButtonItem btnN75Map;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolDescription;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editXAxisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editYAxisToolStripMenuItem;
        private DevExpress.XtraBars.BarButtonItem btnEGRMap;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolSubCategory;
        private DevExpress.XtraBars.BarButtonItem btnAirmassResult;
        private DevExpress.XtraBars.BarStaticItem barReadOnly;
        private DevExpress.XtraBars.BarButtonItem btnExportXDF;
        private DevExpress.XtraBars.BarButtonItem btnActivateLaunchControl;
        private DevExpress.XtraBars.BarButtonItem btnEditEEProm;
        private DevExpress.XtraBars.BarAndDockingController barAndDockingController1;
        private DevExpress.XtraBars.BarStaticItem barChecksum;
        private DevExpress.XtraBars.BarButtonItem btnMergeFiles;
        private DevExpress.XtraBars.BarButtonItem btnSplitFiles;
        private DevExpress.XtraBars.BarButtonItem btnBuildLibrary;
        private DevExpress.XtraBars.BarButtonItem btnUserManual;
        private DevExpress.XtraBars.BarSubItem barSubItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.BarButtonItem barButtonItem3;
        private DevExpress.XtraBars.BarButtonItem barButtonItem4;
        private DevExpress.XtraBars.BarButtonItem barButtonItem5;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolUserdescription;
        private DevExpress.XtraBars.BarButtonItem btnActivateSmokeLimiters;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolXID;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolYID;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolXDescr;
        private DevExpress.XtraGrid.Columns.GridColumn gcSymbolYDescr;
        private DevExpress.XtraBars.BarButtonItem btnExportToExcel;
        private DevExpress.XtraBars.BarButtonItem btnExcelImport;
        private System.Windows.Forms.ToolStripMenuItem exportToExcelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToXMLToolStripMenuItem;
        private DevExpress.XtraBars.BarButtonItem btnIQByMap;
        private DevExpress.XtraBars.BarButtonItem btnIQByMAF;
        private DevExpress.XtraBars.BarButtonItem btnSOILimiter;
        private DevExpress.XtraBars.BarButtonItem btnStartOfInjection;
        private DevExpress.XtraBars.BarButtonItem btnInjectorDuration;
        private DevExpress.XtraBars.BarButtonItem btnStartIQ;
        private DevExpress.XtraBars.BarButtonItem btnBIPBasicCharacteristic;
        private DevExpress.XtraBars.BarButtonItem btnPIDMapP;
        private DevExpress.XtraBars.BarButtonItem btnPIDMapI;
        private DevExpress.XtraBars.BarButtonItem btnPIDMapD;
        private DevExpress.XtraBars.BarButtonItem btnDurationLimiter;
        private DevExpress.XtraBars.BarButtonItem btnMAFLimiter;
        private DevExpress.XtraBars.BarButtonItem btnMAPLimiter;
        private DevExpress.XtraBars.BarButtonItem btnMAFLinearization;
        private DevExpress.XtraBars.BarButtonItem btnMAPLinearization;
        private DevExpress.XtraBars.BarSubItem barSubItemVCDSDiag;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit1;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit2;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit3;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit4;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit5;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit6;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit7;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit8;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit9;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQLimit10;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAFLimit1;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAFLimit2;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAPLimit1;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAPLimit2;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAPLimit3;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticTorqueLimit;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticTorqueOffset;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAFOffset;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticMAPOffset;
        private DevExpress.XtraBars.BarButtonItem btnVCDSDiagnosticIQOffset;
    }
}
