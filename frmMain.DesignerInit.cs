namespace VAGSuite
{
    public partial class frmMain
    {
        /// <summary>
        /// Initializes all component instances
        /// </summary>
        private void InitializeComponentInstances()
        {
            // Core components
            this.ribbonControl1 = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barAndDockingController1 = new DevExpress.XtraBars.BarAndDockingController(this.components);
            this.dockManager1 = new DevExpress.XtraBars.Docking.DockManager(this.components);

            // Ribbon pages
            this.ribFile = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribActions = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rbnPageTuning = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rbnPageSkins = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rbnPageHelp = new DevExpress.XtraBars.Ribbon.RibbonPage();

            // Ribbon page groups
            this.rbpgGeneralFile = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnSettings = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageGourpProjects = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribpgGeneralActions = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageGroupInformation = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageGroupTools = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageInjection = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageTurbo = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageGroupSkins = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageGroupUpdate = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rbnPageDocumentation = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();

            // Status bar
            this.ribbonStatusBar1 = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();

            // Docking
            this.dockSymbols = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();

            // Grid
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridViewSymbols = new DevExpress.XtraGrid.Views.Grid.GridView();

            // Context menu
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);

            // Default look and feel
            this.defaultLookAndFeel1 = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);

            // Begin initialization
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dockManager1)).BeginInit();
            this.dockSymbols.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridViewSymbols)).BeginInit();
        }

        /// <summary>
        /// Initializes ribbon control infrastructure
        /// </summary>
        private void InitializeRibbonInfrastructure()
        {
            // Ribbon control
            this.ribbonControl1.Controller = this.barAndDockingController1;
            this.ribbonControl1.ExpandCollapseItem.Id = 0;
            this.ribbonControl1.ExpandCollapseItem.Name = "";
            this.ribbonControl1.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl1.MaxItemId = 100;
            this.ribbonControl1.Name = "ribbonControl1";
            this.ribbonControl1.Size = new System.Drawing.Size(
                LayoutConstants.DefaultFormWidth, 
                LayoutConstants.RibbonHeight);
            this.ribbonControl1.StatusBar = this.ribbonStatusBar1;

            // Bar and docking controller
            this.barAndDockingController1.PropertiesBar.AllowLinkLighting = false;

            // Look and feel
            this.defaultLookAndFeel1.LookAndFeel.SkinName = "Metropolis";
        }

        /// <summary>
        /// Initializes all ribbon buttons
        /// </summary>
        private void InitializeRibbonButtons()
        {
            InitializeFileButtons();
            InitializeProjectButtons();
            InitializeActionButtons();
            InitializeTuningButtons();
            InitializeToolButtons();
            InitializeSettingsButtons();
            InitializeHelpButtons();

            // Add all buttons to ribbon
            AddButtonsToRibbon();
        }

        /// <summary>
        /// Initializes file operation buttons
        /// </summary>
        private void InitializeFileButtons()
        {
            this.btnBinaryCompare = CreateBarButton("btnBinaryCompare", "Binary compare files", ControlIds.BinaryCompare);
            this.btnOpenFile = CreateBarButton("btnOpenFile", "Open file", ControlIds.OpenFile);
            this.btnCompareFiles = CreateBarButton("btnCompareFiles", "Compare to another binary", ControlIds.CompareFiles);
            this.btnSaveAs = CreateBarButton("btnSaveAs", "Save as...", ControlIds.SaveAs);
            this.btnCreateBackup = CreateBarButton("btnCreateBackup", "Create a backup", ControlIds.CreateBackup);
            this.btnExportXDF = CreateBarButton("btnExportXDF", "Export XDF", ControlIds.ExportXDF);
            this.btnBuildLibrary = CreateBarButton("btnBuildLibrary", "Build library", 53);
            this.btnTestFiles = CreateBarButton("btnTestFiles", "Test files", 10, enabled: false);
            this.btnTestFiles.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;

            // Import submenu buttons
            this.barButtonItem1 = CreateBarButton("barButtonItem1", "XML descriptor", 56);
            this.barButtonItem2 = CreateBarButton("barButtonItem2", "A2L descriptor", 57, enabled: false);
            this.barButtonItem3 = CreateBarButton("barButtonItem3", "CSV descriptor", 58);
            this.barButtonItem4 = CreateBarButton("barButtonItem4", "AS2 descriptor", 59);
            this.barButtonItem5 = CreateBarButton("barButtonItem5", "DAMOS definition", 60, enabled: false);

            // Import submenu
            this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
            this.barSubItem1.Caption = "Import ...";
            this.barSubItem1.Id = 55;
            this.barSubItem1.Name = "barSubItem1";
            this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
                new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem1),
                new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem2),
                new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem3),
                new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem4),
                new DevExpress.XtraBars.LinkPersistInfo(this.barButtonItem5)
            });
        }

        /// <summary>
        /// Initializes project operation buttons
        /// </summary>
        private void InitializeProjectButtons()
        {
            this.btnCreateAProject = CreateBarButton("btnCreateAProject", "Create a project", ControlIds.CreateProject);
            this.btnOpenProject = CreateBarButton("btnOpenProject", "Open a project", ControlIds.OpenProject);
            this.btnCloseProject = CreateBarButton("btnCloseProject", "Close project", ControlIds.CloseProject, enabled: false);
            this.btnShowTransactionLog = CreateBarButton("btnShowTransactionLog", "Show transaction log", 26, enabled: false);
            this.btnRollback = CreateBarButton("btnRollback", "Roll back/undo", 27, enabled: false);
            this.btnRollforward = CreateBarButton("btnRollforward", "Roll forward/redo", 28, enabled: false);
            this.btnRebuildFile = CreateBarButton("btnRebuildFile", "Rebuild file", 29, enabled: false);
            this.btnEditProject = CreateBarButton("btnEditProject", "Edit project properties", 30, enabled: false);
            this.btnAddNoteToProject = CreateBarButton("btnAddNoteToProject", "Add note to project log", 31, enabled: false);
            this.btnShowProjectLogbook = CreateBarButton("btnShowProjectLogbook", "Show project logbook", 32, enabled: false);
            this.btnProduceLatestBinary = CreateBarButton("btnProduceLatestBinary", "Produce latest binary", 33, enabled: false);
        }

        /// <summary>
        /// Initializes action buttons
        /// </summary>
        private void InitializeActionButtons()
        {
            this.btnChecksum = CreateBarButton("btnChecksum", "Verify checksums", ControlIds.Checksum);
            this.btnFirmwareInformation = CreateBarButton("btnFirmwareInformation", "Firmware information", 18);
            this.btnVINDecoder = CreateBarButton("btnVINDecoder", "VIN decoder", 19);
        }

        /// <summary>
        /// Initializes tuning buttons
        /// </summary>
        private void InitializeTuningButtons()
        {
            InitializeInjectionButtons();
            InitializeTurboButtons();
        }

        /// <summary>
        /// Initializes injection tuning buttons
        /// </summary>
        private void InitializeInjectionButtons()
        {
            this.btnDriverWish = CreateBarButton("btnDriverWish", "Driver wish", ControlIds.DriverWish);
            this.btnTorqueLimiter = CreateBarButton("btnTorqueLimiter", "Torque limiter", ControlIds.TorqueLimiter);
            this.btnSmokeLimiter = CreateBarButton("btnSmokeLimiter", "Smoke limiter", ControlIds.SmokeLimiter);
            this.btnIQByMap = CreateBarButton("btnIQByMap", "IQ by MAP limiter", ControlIds.IQByMap);
            this.btnIQByMAF = CreateBarButton("btnIQByMAF", "IQ by MAF limiter", ControlIds.IQByMAF);
            this.btnSOILimiter = CreateBarButton("btnSOILimiter", "SOI limiter", ControlIds.SOILimiter);
            this.btnStartOfInjection = CreateBarButton("btnStartOfInjection", "Start of injection", ControlIds.StartOfInjection);
            this.btnInjectorDuration = CreateBarButton("btnInjectorDuration", "Injection duration", ControlIds.InjectorDuration);
            this.btnStartIQ = CreateBarButton("btnStartIQ", "Start IQ", ControlIds.StartIQ);
            this.btnBIPBasicCharacteristic = CreateBarButton("btnBIPBasicCharacteristic", "BIP Basic characteristic", ControlIds.BIPBasicCharacteristic);
            this.btnPIDMapP = CreateBarButton("btnPIDMapP", "Pid Map P", ControlIds.PIDMapP);
            this.btnPIDMapI = CreateBarButton("btnPIDMapI", "Pid Map I", ControlIds.PIDMapI);
            this.btnPIDMapD = CreateBarButton("btnPIDMapD", "Pid Map D", ControlIds.PIDMapD);
            this.btnDurationLimiter = CreateBarButton("btnDurationLimiter", "Duration limiter", ControlIds.DurationLimiter);
            this.btnMAFLimiter = CreateBarButton("btnMAFLimiter", "MAF limiter", ControlIds.MAFLimiter);
            this.btnMAPLimiter = CreateBarButton("btnMAPLimiter", "MAP limiter", ControlIds.MAPLimiter);

            InitializeVCDSDiagnosticButtons();
        }

        /// <summary>
        /// Initializes VCDS diagnostic buttons
        /// </summary>
        private void InitializeVCDSDiagnosticButtons()
        {
            this.btnVCDSDiagnosticIQLimit1 = CreateBarButton("btnVCDSDiagnosticIQLimit1", "VCDS IQ Limit 1", ControlIds.VCDSDiagStart);
            this.btnVCDSDiagnosticIQLimit2 = CreateBarButton("btnVCDSDiagnosticIQLimit2", "VCDS IQ Limit 2", ControlIds.VCDSDiagStart + 1);
            this.btnVCDSDiagnosticIQLimit3 = CreateBarButton("btnVCDSDiagnosticIQLimit3", "VCDS IQ Limit 3", ControlIds.VCDSDiagStart + 2);
            this.btnVCDSDiagnosticIQLimit4 = CreateBarButton("btnVCDSDiagnosticIQLimit4", "VCDS IQ Limit 4", ControlIds.VCDSDiagStart + 3);
            this.btnVCDSDiagnosticIQLimit5 = CreateBarButton("btnVCDSDiagnosticIQLimit5", "VCDS IQ Limit 5", ControlIds.VCDSDiagStart + 4);
            this.btnVCDSDiagnosticIQLimit6 = CreateBarButton("btnVCDSDiagnosticIQLimit6", "VCDS IQ Limit 6", ControlIds.VCDSDiagStart + 5);
            this.btnVCDSDiagnosticIQLimit7 = CreateBarButton("btnVCDSDiagnosticIQLimit7", "VCDS IQ Limit 7", ControlIds.VCDSDiagStart + 6);
            this.btnVCDSDiagnosticIQLimit8 = CreateBarButton("btnVCDSDiagnosticIQLimit8", "VCDS IQ Limit 8", ControlIds.VCDSDiagStart + 7);
            this.btnVCDSDiagnosticIQLimit9 = CreateBarButton("btnVCDSDiagnosticIQLimit9", "VCDS IQ Limit 9", ControlIds.VCDSDiagStart + 8);
            this.btnVCDSDiagnosticIQLimit10 = CreateBarButton("btnVCDSDiagnosticIQLimit10", "VCDS IQ Limit 10", ControlIds.VCDSDiagStart + 9);
            this.btnVCDSDiagnosticMAFLimit1 = CreateBarButton("btnVCDSDiagnosticMAFLimit1", "VCDS MAF Limit 1", ControlIds.VCDSDiagStart + 10);
            this.btnVCDSDiagnosticMAFLimit2 = CreateBarButton("btnVCDSDiagnosticMAFLimit2", "VCDS MAF Limit 2", ControlIds.VCDSDiagStart + 11);
            this.btnVCDSDiagnosticMAPLimit1 = CreateBarButton("btnVCDSDiagnosticMAPLimit1", "VCDS MAP Limit 1", ControlIds.VCDSDiagStart + 12);
            this.btnVCDSDiagnosticMAPLimit2 = CreateBarButton("btnVCDSDiagnosticMAPLimit2", "VCDS MAP Limit 2", ControlIds.VCDSDiagStart + 13);
            this.btnVCDSDiagnosticMAPLimit3 = CreateBarButton("btnVCDSDiagnosticMAPLimit3", "VCDS MAP Limit 3", ControlIds.VCDSDiagStart + 14);
            this.btnVCDSDiagnosticTorqueLimit = CreateBarButton("btnVCDSDiagnosticTorqueLimit", "VCDS Torque Limit", ControlIds.VCDSDiagStart + 15);
            this.btnVCDSDiagnosticTorqueOffset = CreateBarButton("btnVCDSDiagnosticTorqueOffset", "VCDS Torque Offset", ControlIds.VCDSDiagStart + 16);
            this.btnVCDSDiagnosticMAFOffset = CreateBarButton("btnVCDSDiagnosticMAFOffset", "VCDS MAF Offset", ControlIds.VCDSDiagStart + 17);
            this.btnVCDSDiagnosticMAPOffset = CreateBarButton("btnVCDSDiagnosticMAPOffset", "VCDS MAP Offset", ControlIds.VCDSDiagStart + 18);
            this.btnVCDSDiagnosticIQOffset = CreateBarButton("btnVCDSDiagnosticIQOffset", "VCDS IQ Offset", ControlIds.VCDSDiagStart + 19);

            // Create submenu
            this.barSubItemVCDSDiag = new DevExpress.XtraBars.BarSubItem();
            this.barSubItemVCDSDiag.Caption = "VCDS Diagnostic Limits";
            this.barSubItemVCDSDiag.Id = ControlIds.VCDSDiagStart - 1;
            this.barSubItemVCDSDiag.Name = "barSubItemVCDSDiag";
            this.barSubItemVCDSDiag.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit1),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit2),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit3),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit4),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit5),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit6),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit7),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit8),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit9),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQLimit10),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAFLimit1),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAFLimit2),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAPLimit1),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAPLimit2),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAPLimit3),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticTorqueLimit),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticTorqueOffset),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAFOffset),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticMAPOffset),
                new DevExpress.XtraBars.LinkPersistInfo(this.btnVCDSDiagnosticIQOffset)
            });
        }

        /// <summary>
        /// Initializes turbo tuning buttons
        /// </summary>
        private void InitializeTurboButtons()
        {
            this.btnTargetBoost = CreateBarButton("btnTargetBoost", "Target boost", ControlIds.TargetBoost);
            this.btnBoostPressureLimiter = CreateBarButton("btnBoostPressureLimiter", "Boost pressure limiter (atm)", ControlIds.BoostPressureLimiter);
            this.btnBoostPressureLimitSVBL = CreateBarButton("btnBoostPressureLimitSVBL", "Boost pressure guard (SVBL)", ControlIds.BoostPressureLimitSVBL);
            this.btnN75Map = CreateBarButton("btnN75Map", "N75 duty cycle", ControlIds.N75Map);
            this.btnEGRMap = CreateBarButton("btnEGRMap", "EGR", ControlIds.EGRMap);
            this.btnMAFLinearization = CreateBarButton("btnMAFLinearization", "MAF linearization", ControlIds.MAFLinearization);
            this.btnMAPLinearization = CreateBarButton("btnMAPLinearization", "MAP linearization", ControlIds.MAPLinearization);
        }

        /// <summary>
        /// Initializes tool buttons
        /// </summary>
        private void InitializeToolButtons()
        {
            this.btnViewFileInHex = CreateBarButton("btnViewFileInHex", "View file in hex", ControlIds.ViewFileInHex);
            this.btnSearchMaps = CreateBarButton("btnSearchMaps", "Search map content", ControlIds.SearchMaps);
            this.btnAirmassResult = CreateBarButton("btnAirmassResult", "View performance", ControlIds.AirmassResult);
            this.btnActivateLaunchControl = CreateBarButton(
                "btnActivateLaunchControl", 
                "Activate Launch Control", 
                ControlIds.ActivateLaunchControl,
                enabled: false,
                hint: "Tries to activate the launch control maps in the file.");
            this.btnActivateSmokeLimiters = CreateBarButton(
                "btnActivateSmokeLimiters", 
                "Activate multi smoke limiters", 
                ControlIds.ActivateSmokeLimiters,
                enabled: false,
                hint: "Tries to activate the coolant dependant smoke limiters.");
            this.btnEditEEProm = CreateBarButton("btnEditEEProm", "Edit EEProm data", ControlIds.EditEEProm);
            this.btnMergeFiles = CreateBarButton("btnMergeFiles", "Merge two files", ControlIds.MergeFiles);
            this.btnSplitFiles = CreateBarButton("btnSplitFiles", "Split file in two parts", ControlIds.SplitFiles);
            this.btnExportToExcel = CreateBarButton("btnExportToExcel", "Export map to Excel", ControlIds.ExportToExcel);
            this.btnExcelImport = CreateBarButton("btnExcelImport", "Import map from Excel", ControlIds.ExcelImport);
        }

        /// <summary>
        /// Initializes settings buttons
        /// </summary>
        private void InitializeSettingsButtons()
        {
            this.btnAppSettings = CreateBarButton("btnAppSettings", "Application settings", ControlIds.AppSettings);
            this.btnLookupPartnumber = CreateBarButton("btnLookupPartnumber", "Lookup partnumber", ControlIds.LookupPartnumber);
        }

        /// <summary>
        /// Initializes help buttons
        /// </summary>
        private void InitializeHelpButtons()
        {
            this.btnCheckForUpdates = CreateBarButton("btnCheckForUpdates", "Check for updates", ControlIds.CheckForUpdates);
            this.btnReleaseNotes = CreateBarButton("btnReleaseNotes", "Read release notes", ControlIds.ReleaseNotes);
            this.btnEDC15PDocumentation = CreateBarButton("btnEDC15PDocumentation", "EDC15P documentation", ControlIds.EDC15PDocumentation);
            this.btnUserManual = CreateBarButton("btnUserManual", "EDC15P Suite user manual", ControlIds.UserManual);
            this.btnAbout = CreateBarButton("btnAbout", "About VAGEDCSuite", ControlIds.About);
        }

        /// <summary>
        /// Adds all buttons to ribbon control
        /// </summary>
        private void AddButtonsToRibbon()
        {
            this.ribbonControl1.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
                this.ribbonControl1.ExpandCollapseItem,
                // File buttons
                this.btnBinaryCompare, this.btnOpenFile, this.btnCompareFiles,
                this.btnSaveAs, this.btnCreateBackup, this.btnExportXDF,
                this.btnBuildLibrary, this.btnTestFiles, this.barSubItem1,
                this.barButtonItem1, this.barButtonItem2, this.barButtonItem3,
                this.barButtonItem4, this.barButtonItem5,
                // Project buttons
                this.btnCreateAProject, this.btnOpenProject, this.btnCloseProject,
                this.btnShowTransactionLog, this.btnRollback, this.btnRollforward,
                this.btnRebuildFile, this.btnEditProject, this.btnAddNoteToProject,
                this.btnShowProjectLogbook, this.btnProduceLatestBinary,
                // Action buttons
                this.btnChecksum, this.btnFirmwareInformation, this.btnVINDecoder,
                // Tool buttons
                this.btnViewFileInHex, this.btnSearchMaps, this.btnAirmassResult,
                this.btnActivateLaunchControl, this.btnActivateSmokeLimiters,
                this.btnEditEEProm, this.btnMergeFiles, this.btnSplitFiles,
                this.btnExportToExcel, this.btnExcelImport,
                // Tuning buttons
                this.btnDriverWish, this.btnTorqueLimiter, this.btnSmokeLimiter,
                this.btnIQByMap, this.btnIQByMAF, this.btnSOILimiter,
                this.btnStartOfInjection, this.btnInjectorDuration, this.btnStartIQ,
                this.btnBIPBasicCharacteristic, this.btnPIDMapP, this.btnPIDMapI,
                this.btnPIDMapD, this.btnDurationLimiter, this.btnMAFLimiter,
                this.btnMAPLimiter, this.barSubItemVCDSDiag,
                this.btnVCDSDiagnosticIQLimit1, this.btnVCDSDiagnosticIQLimit2,
                this.btnVCDSDiagnosticIQLimit3, this.btnVCDSDiagnosticIQLimit4,
                this.btnVCDSDiagnosticIQLimit5, this.btnVCDSDiagnosticIQLimit6,
                this.btnVCDSDiagnosticIQLimit7, this.btnVCDSDiagnosticIQLimit8,
                this.btnVCDSDiagnosticIQLimit9, this.btnVCDSDiagnosticIQLimit10,
                this.btnVCDSDiagnosticMAFLimit1, this.btnVCDSDiagnosticMAFLimit2,
                this.btnVCDSDiagnosticMAPLimit1, this.btnVCDSDiagnosticMAPLimit2,
                this.btnVCDSDiagnosticMAPLimit3, this.btnVCDSDiagnosticTorqueLimit,
                this.btnVCDSDiagnosticTorqueOffset, this.btnVCDSDiagnosticMAFOffset,
                this.btnVCDSDiagnosticMAPOffset, this.btnVCDSDiagnosticIQOffset,
                this.btnTargetBoost, this.btnBoostPressureLimiter,
                this.btnBoostPressureLimitSVBL, this.btnN75Map, this.btnEGRMap,
                this.btnMAFLinearization, this.btnMAPLinearization,
                // Settings buttons
                this.btnAppSettings, this.btnLookupPartnumber,
                // Help buttons
                this.btnCheckForUpdates, this.btnReleaseNotes,
                this.btnEDC15PDocumentation, this.btnUserManual, this.btnAbout,
                // Status bar items
                this.barFilenameText, this.barChecksum, this.barReadOnly,
                this.barPartnumber, this.barAdditionalInfo,
                this.barCodeBlock1, this.barCodeBlock2, this.barCodeBlock3,
                this.barSymCount, this.barUpdateText
            });
        }
    }
}