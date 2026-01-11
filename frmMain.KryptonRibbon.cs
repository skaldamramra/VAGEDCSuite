using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Ribbon;
using VAGSuite.Theming;

namespace VAGSuite
{
    partial class frmMain
    {
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonGeneralFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonGeneralFileTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonOpenFile;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonSaveAs;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonBinaryCompare;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonProjects;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonProjectsTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonOpenProject;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonCloseProject;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonCreateProject;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonTransactionLog;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonRollback;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonRollforward;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonTurboAnalysis;

        // Tuning Tab Components
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonInjection;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonTurbo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonLimiters;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonLinearization;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonPID;

        // Help Tab Components
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonHelp;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonHelpAbout;

        // Skins Tab Components
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonSkins;

        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonGeneralActions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonGeneralActionsTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonSearchMaps;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonViewHex;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonChecksum;
        
        // File Tools Group
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonFileTools;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonFileToolsTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonLookupPart;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonMergeFiles;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonSplitFiles;

        // Export Group
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonExport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonExportTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonExcelExport;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonXDFExport;

        // Transactions Group
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonTransactions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonTransactionsTriple;

        // Extra Tools Group (EEPROM, EOI, Settings)
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonExtraTools;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonExtraToolsTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonEditEEProm;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonEOICalculator;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonAppSettings;

        private void InitializeKryptonRibbon()
        {
            this.kryptonRibbon1 = new ComponentFactory.Krypton.Ribbon.KryptonRibbon();
            this.kryptonRibbonTabFile = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.rbpgKryptonGeneralFile = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonGeneralFileTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.btnKryptonOpenFile = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.btnKryptonSaveAs = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.btnKryptonBinaryCompare = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            
            this.rbpgKryptonProjects = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonProjectsTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.btnKryptonOpenProject = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.btnKryptonCloseProject = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.btnKryptonCreateProject = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();

            this.kryptonRibbonTabActions = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.rbpgKryptonGeneralActions = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonGeneralActionsTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.btnKryptonSearchMaps = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.btnKryptonViewHex = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.btnKryptonChecksum = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton();
            this.kryptonRibbonTabTuning = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.kryptonRibbonTabSkins = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();
            this.kryptonRibbonTabHelp = new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab();

            ((System.ComponentModel.ISupportInitialize)(this.kryptonRibbon1)).BeginInit();

            // 
            // kryptonRibbon1
            // 
            this.kryptonRibbon1.Name = "kryptonRibbon1";
            this.kryptonRibbon1.RibbonTabs.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonTab[] {
                this.kryptonRibbonTabFile,
                this.kryptonRibbonTabActions,
                this.kryptonRibbonTabTuning,
                this.kryptonRibbonTabSkins,
                this.kryptonRibbonTabHelp
            });
            this.kryptonRibbon1.SelectedTab = this.kryptonRibbonTabFile;
            this.kryptonRibbon1.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            this.kryptonRibbon1.PaletteMode = PaletteMode.Custom;
            this.kryptonRibbon1.AllowFormIntegrate = false;
            this.kryptonRibbon1.Visible = true;

            //
            // kryptonRibbonTabFile
            //
            this.kryptonRibbonTabFile.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
                this.rbpgKryptonGeneralFile,
                this.rbpgKryptonProjects
            });
            this.kryptonRibbonTabFile.Text = "File";

            this.rbpgKryptonGeneralFile.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
                this.rbpgKryptonGeneralFileTriple
            });
            this.rbpgKryptonGeneralFile.TextLine1 = "General";
            this.rbpgKryptonGeneralFile.TextLine2 = "File";

            this.rbpgKryptonGeneralFileTriple.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
                this.btnKryptonOpenFile,
                this.btnKryptonSaveAs,
                this.btnKryptonBinaryCompare
            });
            this.rbpgKryptonGeneralFileTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonGeneralFileTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonOpenFile.TextLine1 = "Open";
            this.btnKryptonOpenFile.TextLine2 = "File";
            this.btnKryptonOpenFile.ImageLarge = GetResourceImage("Open.png");
            this.btnKryptonOpenFile.Click += new System.EventHandler(this.btnKryptonOpenFile_Click);

            this.btnKryptonSaveAs.TextLine1 = "Save";
            this.btnKryptonSaveAs.TextLine2 = "As...";
            this.btnKryptonSaveAs.ImageLarge = GetResourceImage("Save.png");
            this.btnKryptonSaveAs.Click += new System.EventHandler(this.btnKryptonSaveAs_Click);

            this.btnKryptonBinaryCompare.TextLine1 = "Binary";
            this.btnKryptonBinaryCompare.TextLine2 = "Compare";
            this.btnKryptonBinaryCompare.ImageLarge = GetResourceImage("BinaryCompare.png");
            this.btnKryptonBinaryCompare.Click += new System.EventHandler(this.btnKryptonBinaryCompare_Click);

            this.rbpgKryptonProjects.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
                this.rbpgKryptonProjectsTriple
            });
            this.rbpgKryptonProjects.TextLine1 = "Projects";

            this.rbpgKryptonProjectsTriple.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
                this.btnKryptonOpenProject,
                this.btnKryptonCloseProject,
                this.btnKryptonCreateProject
            });
            this.rbpgKryptonProjectsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonProjectsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonOpenProject.TextLine1 = "Open";
            this.btnKryptonOpenProject.TextLine2 = "Project";
            this.btnKryptonOpenProject.ImageLarge = GetResourceImage("OpenProject.png");
            this.btnKryptonOpenProject.Click += new System.EventHandler(this.btnKryptonOpenProject_Click);

            this.btnKryptonCloseProject.TextLine1 = "Close";
            this.btnKryptonCloseProject.TextLine2 = "Project";
            this.btnKryptonCloseProject.ImageLarge = GetResourceImage("CloseProject.png");
            this.btnKryptonCloseProject.Click += new System.EventHandler(this.btnKryptonCloseProject_Click);

            this.btnKryptonCreateProject.TextLine1 = "Create";
            this.btnKryptonCreateProject.TextLine2 = "Project";
            this.btnKryptonCreateProject.ImageLarge = GetResourceImage("CreateProject.png");
            this.btnKryptonCreateProject.Click += new System.EventHandler(this.btnKryptonCreateProject_Click);
            
            //
            // kryptonRibbonTabActions
            //
            this.kryptonRibbonTabActions.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
                this.rbpgKryptonGeneralActions
            });
            this.kryptonRibbonTabActions.Text = "Actions";

            this.rbpgKryptonGeneralActions.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
                this.rbpgKryptonGeneralActionsTriple
            });
            this.rbpgKryptonGeneralActions.TextLine1 = "General";
            this.rbpgKryptonGeneralActions.TextLine2 = "Actions";

            this.rbpgKryptonGeneralActionsTriple.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
                this.btnKryptonSearchMaps,
                this.btnKryptonViewHex,
                this.btnKryptonChecksum
            });
            this.rbpgKryptonGeneralActionsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonGeneralActionsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonSearchMaps.TextLine1 = "Search";
            this.btnKryptonSearchMaps.TextLine2 = "Maps";
            this.btnKryptonSearchMaps.ImageLarge = GetResourceImage("SearchMaps.png");
            this.btnKryptonSearchMaps.Click += new System.EventHandler(this.btnKryptonSearchMaps_Click);

            this.btnKryptonViewHex.TextLine1 = "View";
            this.btnKryptonViewHex.TextLine2 = "Hex";
            this.btnKryptonViewHex.ImageLarge = GetResourceImage("ViewHex.png");
            this.btnKryptonViewHex.Click += new System.EventHandler(this.btnKryptonViewHex_Click);

            this.btnKryptonChecksum.TextLine1 = "Verify";
            this.btnKryptonChecksum.TextLine2 = "Checksum";
            this.btnKryptonChecksum.ImageLarge = GetResourceImage("VerifyChecksum.png");
            this.btnKryptonChecksum.Click += new System.EventHandler(this.btnKryptonChecksum_Click);

            KryptonRibbonGroupTriple rbpgKryptonActionsTriple2 = new KryptonRibbonGroupTriple();
            rbpgKryptonActionsTriple2.MaximumSize = GroupItemSize.Large;
            rbpgKryptonActionsTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupTriple rbpgKryptonActionsTriple3 = new KryptonRibbonGroupTriple();
            rbpgKryptonActionsTriple3.MaximumSize = GroupItemSize.Large;
            rbpgKryptonActionsTriple3.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonFirmwareInfo = new KryptonRibbonGroupButton { TextLine1 = "Firmware", TextLine2 = "Info", ImageLarge = GetResourceImage("FirmwareInfo.png") };
            btnKryptonFirmwareInfo.Click += (s, e) => btnFirmwareInformation_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonVINDecoder = new KryptonRibbonGroupButton { TextLine1 = "VIN", TextLine2 = "Decoder", ImageLarge = GetResourceImage("VINDecoder.png") };
            btnKryptonVINDecoder.Click += (s, e) => btnVINDecoder_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonAirmass = new KryptonRibbonGroupButton { TextLine1 = "Airmass", TextLine2 = "Result", ImageLarge = GetResourceImage("AirmassResult.png") };
            btnKryptonAirmass.Click += (s, e) => btnAirmassResult_ItemClick(s, null);
            
            this.btnKryptonTurboAnalysis = new KryptonRibbonGroupButton();
            this.btnKryptonTurboAnalysis.TextLine1 = "Turbo";
            this.btnKryptonTurboAnalysis.TextLine2 = "Analysis";
            this.btnKryptonTurboAnalysis.ImageLarge = GetResourceImage("SearchMaps.png");
            this.btnKryptonTurboAnalysis.Click += new System.EventHandler(this.btnKryptonTurboAnalysis_Click);

            rbpgKryptonActionsTriple2.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonFirmwareInfo, btnKryptonVINDecoder, btnKryptonAirmass });
            rbpgKryptonActionsTriple3.Items.AddRange(new KryptonRibbonGroupItem[] { this.btnKryptonTurboAnalysis });
            
            this.rbpgKryptonGeneralActions.Items.Add(rbpgKryptonActionsTriple2);
            this.rbpgKryptonGeneralActions.Items.Add(rbpgKryptonActionsTriple3);

            // File Tools Group Initialization
            this.rbpgKryptonFileTools = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonFileToolsTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.rbpgKryptonFileToolsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonFileToolsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonLookupPart = new KryptonRibbonGroupButton { TextLine1 = "Part", TextLine2 = "Lookup", ImageLarge = GetResourceImage("LookupPart.png") };
            this.btnKryptonLookupPart.Click += (s, e) => btnLookupPartnumber_ItemClick(s, null);

            this.btnKryptonMergeFiles = new KryptonRibbonGroupButton { TextLine1 = "Merge", TextLine2 = "Files", ImageLarge = GetResourceImage("MergeFiles.png") };
            this.btnKryptonMergeFiles.Click += (s, e) => btnMergeFiles_ItemClick(s, null);

            this.btnKryptonSplitFiles = new KryptonRibbonGroupButton { TextLine1 = "Split", TextLine2 = "Files", ImageLarge = GetResourceImage("SplitFiles.png") };
            this.btnKryptonSplitFiles.Click += (s, e) => btnSplitFiles_ItemClick(s, null);

            this.rbpgKryptonFileTools.TextLine1 = "File Tools";
            this.rbpgKryptonFileToolsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonLookupPart, btnKryptonMergeFiles, btnKryptonSplitFiles });
            this.rbpgKryptonFileTools.Items.Add(this.rbpgKryptonFileToolsTriple);

            // Export Group Initialization
            this.rbpgKryptonExport = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonExportTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.rbpgKryptonExportTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonExportTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonExcelExport = new KryptonRibbonGroupButton { TextLine1 = "Export", TextLine2 = "Excel", ImageLarge = GetResourceImage("ExcelExport.png") };
            this.btnKryptonExcelExport.Click += (s, e) => btnExportToExcel_ItemClick(s, null);

            this.btnKryptonXDFExport = new KryptonRibbonGroupButton { TextLine1 = "Export", TextLine2 = "XDF", ImageLarge = GetResourceImage("ExportXDF.png") };
            this.btnKryptonXDFExport.Click += (s, e) => btnExportXDF_ItemClick(s, null);

            this.rbpgKryptonExport.TextLine1 = "Export";
            this.rbpgKryptonExportTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonExcelExport, btnKryptonXDFExport });
            this.rbpgKryptonExport.Items.Add(this.rbpgKryptonExportTriple);

            // Transactions Group Initialization
            this.rbpgKryptonTransactions = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonTransactionsTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.rbpgKryptonTransactionsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonTransactionsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonTransactionLog = new KryptonRibbonGroupButton { TextLine1 = "Transaction", TextLine2 = "Log", ImageLarge = GetResourceImage("TransactionLog.png") };
            this.btnKryptonTransactionLog.Click += (s, e) => btnShowTransactionLog_ItemClick(s, null);

            this.btnKryptonRollback = new KryptonRibbonGroupButton { TextLine1 = "Roll", TextLine2 = "Back", ImageLarge = GetResourceImage("Rollback.png") };
            this.btnKryptonRollback.Click += (s, e) => btnRollback_ItemClick(s, null);

            this.btnKryptonRollforward = new KryptonRibbonGroupButton { TextLine1 = "Roll", TextLine2 = "Forward", ImageLarge = GetResourceImage("RollForward.png") };
            this.btnKryptonRollforward.Click += (s, e) => btnRollforward_ItemClick(s, null);

            this.rbpgKryptonTransactions.TextLine1 = "Transactions";
            this.rbpgKryptonTransactionsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonTransactionLog, btnKryptonRollback, btnKryptonRollforward });
            this.rbpgKryptonTransactions.Items.Add(this.rbpgKryptonTransactionsTriple);

            // Extra Tools Group Initialization
            this.rbpgKryptonExtraTools = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup();
            this.rbpgKryptonExtraToolsTriple = new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple();
            this.rbpgKryptonExtraToolsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonExtraToolsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonEditEEProm = new KryptonRibbonGroupButton { TextLine1 = "Edit", TextLine2 = "EEPROM", ImageLarge = GetResourceImage("ViewHex.png") };
            this.btnKryptonEditEEProm.Click += (s, e) => btnEditEEProm_ItemClick(s, null);

            this.btnKryptonEOICalculator = new KryptonRibbonGroupButton { TextLine1 = "EOI", TextLine2 = "Calculator", ImageLarge = GetResourceImage("SearchMaps.png") };
            this.btnKryptonEOICalculator.Click += (s, e) => btnEOICalculator_ItemClick(s, null);

            this.btnKryptonAppSettings = new KryptonRibbonGroupButton { TextLine1 = "App", TextLine2 = "Settings", ImageLarge = GetResourceImage("AppSettings.png") };
            this.btnKryptonAppSettings.Click += (s, e) => btnAppSettings_ItemClick(s, null);

            this.rbpgKryptonExtraTools.TextLine1 = "Tools";
            this.rbpgKryptonExtraToolsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonEditEEProm, btnKryptonEOICalculator, btnKryptonAppSettings });
            this.rbpgKryptonExtraTools.Items.Add(this.rbpgKryptonExtraToolsTriple);

            // Add Groups to Actions Tab
            this.kryptonRibbonTabActions.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
                this.rbpgKryptonFileTools,
                this.rbpgKryptonExport,
                this.rbpgKryptonTransactions,
                this.rbpgKryptonExtraTools
            });

            this.kryptonRibbonTabTuning.Text = "Tuning";
            this.kryptonRibbonTabSkins.Text = "Skins";
            this.kryptonRibbonTabHelp.Text = "Help";

            ((System.ComponentModel.ISupportInitialize)(this.kryptonRibbon1)).EndInit();

            this.Controls.Add(this.kryptonRibbon1);
            this.kryptonRibbon1.Dock = DockStyle.Top;

            InitializeTuningTab();
            InitializeSkinsTab();
            InitializeHelpTab();
        }

        private void InitializeTuningTab()
        {
            // Injection Group
            this.rbpgKryptonInjection = new KryptonRibbonGroup { TextLine1 = "Injection" };
            KryptonRibbonGroupTriple injectionTriple1 = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };
            KryptonRibbonGroupTriple injectionTriple2 = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };
            KryptonRibbonGroupTriple injectionTriple3 = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnDriverWish = new KryptonRibbonGroupButton { TextLine1 = "Driver", TextLine2 = "Wish", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnDriverWish.Click += (s, e) => btnDriverWish_ItemClick(s, e);
            KryptonRibbonGroupButton btnIQByMap = new KryptonRibbonGroupButton { TextLine1 = "IQ by", TextLine2 = "MAP", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnIQByMap.Click += (s, e) => btnIQByMap_ItemClick(s, e);
            KryptonRibbonGroupButton btnIQByMAF = new KryptonRibbonGroupButton { TextLine1 = "IQ by", TextLine2 = "MAF", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnIQByMAF.Click += (s, e) => btnIQByMAF_ItemClick(s, e);

            KryptonRibbonGroupButton btnSOI = new KryptonRibbonGroupButton { TextLine1 = "Start of", TextLine2 = "Injection", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnSOI.Click += (s, e) => btnStartOfInjection_ItemClick(s, e);
            KryptonRibbonGroupButton btnSOILimiter = new KryptonRibbonGroupButton { TextLine1 = "SOI", TextLine2 = "Limiter", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnSOILimiter.Click += (s, e) => btnSOILimiter_ItemClick(s, e);
            KryptonRibbonGroupButton btnDuration = new KryptonRibbonGroupButton { TextLine1 = "Injector", TextLine2 = "Duration", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnDuration.Click += (s, e) => btnInjectorDuration_ItemClick(s, e);

            KryptonRibbonGroupButton btnStartIQ = new KryptonRibbonGroupButton { TextLine1 = "Start", TextLine2 = "IQ", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnStartIQ.Click += (s, e) => btnStartIQ_ItemClick(s, e);
            KryptonRibbonGroupButton btnBIP = new KryptonRibbonGroupButton { TextLine1 = "BIP", TextLine2 = "Char.", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnBIP.Click += (s, e) => btnBIPBasicCharacteristic_ItemClick(s, e);

            injectionTriple1.Items.AddRange(new KryptonRibbonGroupItem[] { btnDriverWish, btnIQByMap, btnIQByMAF });
            injectionTriple2.Items.AddRange(new KryptonRibbonGroupItem[] { btnSOI, btnSOILimiter, btnDuration });
            injectionTriple3.Items.AddRange(new KryptonRibbonGroupItem[] { btnStartIQ, btnBIP });
            this.rbpgKryptonInjection.Items.AddRange(new KryptonRibbonGroupContainer[] { injectionTriple1, injectionTriple2, injectionTriple3 });

            // Turbo Group
            this.rbpgKryptonTurbo = new KryptonRibbonGroup { TextLine1 = "Turbo" };
            KryptonRibbonGroupTriple turboTriple1 = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };
            KryptonRibbonGroupTriple turboTriple2 = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnTargetBoost = new KryptonRibbonGroupButton { TextLine1 = "Target", TextLine2 = "Boost", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnTargetBoost.Click += (s, e) => btnTargetBoost_ItemClick(s, e);
            KryptonRibbonGroupButton btnBoostLimiter = new KryptonRibbonGroupButton { TextLine1 = "Boost", TextLine2 = "Limiter", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnBoostLimiter.Click += (s, e) => btnBoostPressureLimiter_ItemClick(s, e);
            KryptonRibbonGroupButton btnSVBL = new KryptonRibbonGroupButton { TextLine1 = "Boost", TextLine2 = "SVBL", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnSVBL.Click += (s, e) => btnBoostPressureLimitSVBL_ItemClick(s, e);

            KryptonRibbonGroupButton btnN75 = new KryptonRibbonGroupButton { TextLine1 = "N75", TextLine2 = "Map", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnN75.Click += (s, e) => btnN75Map_ItemClick(s, e);
            turboTriple1.Items.AddRange(new KryptonRibbonGroupItem[] { btnTargetBoost, btnBoostLimiter, btnSVBL });
            turboTriple2.Items.AddRange(new KryptonRibbonGroupItem[] { btnN75 });
            this.rbpgKryptonTurbo.Items.AddRange(new KryptonRibbonGroupContainer[] { turboTriple1, turboTriple2 });

            // Limiters & Filters Group
            this.rbpgKryptonLimiters = new KryptonRibbonGroup { TextLine1 = "Limiters & Filters" };
            KryptonRibbonGroupTriple limitersTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnSmokeLimiter = new KryptonRibbonGroupButton { TextLine1 = "Smoke", TextLine2 = "Limiter", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnSmokeLimiter.Click += (s, e) => btnSmokeLimiter_ItemClick(s, e);
            KryptonRibbonGroupButton btnMAFLimiter = new KryptonRibbonGroupButton { TextLine1 = "MAF", TextLine2 = "Limiter", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnMAFLimiter.Click += (s, e) => btnMAFLimiter_ItemClick(s, e);
            KryptonRibbonGroupButton btnMAPLimiter = new KryptonRibbonGroupButton { TextLine1 = "MAP", TextLine2 = "Limiter", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnMAPLimiter.Click += (s, e) => btnMAPLimiter_ItemClick(s, e);
            KryptonRibbonGroupButton btnEGR = new KryptonRibbonGroupButton { TextLine1 = "EGR", TextLine2 = "Map", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnEGR.Click += (s, e) => btnEGRMap_ItemClick(s, e);

            limitersTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnSmokeLimiter, btnMAFLimiter, btnMAPLimiter });
            // Add EGR to a new line or same triple if space allows. Triple only allows 3.
            KryptonRibbonGroupTriple limitersTriple2 = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };
            limitersTriple2.Items.Add(btnEGR);

            this.rbpgKryptonLimiters.Items.AddRange(new KryptonRibbonGroupContainer[] { limitersTriple, limitersTriple2 });

            // Linearization Group
            this.rbpgKryptonLinearization = new KryptonRibbonGroup { TextLine1 = "Linearization" };
            KryptonRibbonGroupTriple linearTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnMAFLinear = new KryptonRibbonGroupButton { TextLine1 = "MAF", TextLine2 = "Linear.", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnMAFLinear.Click += (s, e) => btnMAFLinearization_ItemClick(s, e);
            KryptonRibbonGroupButton btnMAPLinear = new KryptonRibbonGroupButton { TextLine1 = "MAP", TextLine2 = "Linear.", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnMAPLinear.Click += (s, e) => btnMAPLinearization_ItemClick(s, e);

            linearTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnMAFLinear, btnMAPLinear });
            this.rbpgKryptonLinearization.Items.Add(linearTriple);

            // PID Group
            this.rbpgKryptonPID = new KryptonRibbonGroup { TextLine1 = "PID" };
            KryptonRibbonGroupTriple pidTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnPIDP = new KryptonRibbonGroupButton { TextLine1 = "PID", TextLine2 = "P Map", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnPIDP.Click += (s, e) => btnPIDMapP_ItemClick(s, e);
            KryptonRibbonGroupButton btnPIDI = new KryptonRibbonGroupButton { TextLine1 = "PID", TextLine2 = "I Map", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnPIDI.Click += (s, e) => btnPIDMapI_ItemClick(s, e);
            KryptonRibbonGroupButton btnPIDD = new KryptonRibbonGroupButton { TextLine1 = "PID", TextLine2 = "D Map", ImageLarge = GetResourceImage("SearchMaps.png") };
            btnPIDD.Click += (s, e) => btnPIDMapD_ItemClick(s, e);

            pidTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnPIDP, btnPIDI, btnPIDD });
            this.rbpgKryptonPID.Items.Add(pidTriple);

            // Special Actions Group
            KryptonRibbonGroup rbpgKryptonTuningActions = new KryptonRibbonGroup { TextLine1 = "Actions" };
            KryptonRibbonGroupTriple tuningActionsTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnLaunchControl = new KryptonRibbonGroupButton { TextLine1 = "Activate", TextLine2 = "Launch Ctrl", ImageLarge = GetResourceImage("VerifyChecksum.png") };
            btnLaunchControl.Click += (s, e) => btnActivateLaunchControl_ItemClick(s, e);
            KryptonRibbonGroupButton btnActivateSmoke = new KryptonRibbonGroupButton { TextLine1 = "Activate", TextLine2 = "Smoke Lim.", ImageLarge = GetResourceImage("VerifyChecksum.png") };
            btnActivateSmoke.Click += (s, e) => btnActivateSmokeLimiters_ItemClick(s, e);

            tuningActionsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnLaunchControl, btnActivateSmoke });
            rbpgKryptonTuningActions.Items.Add(tuningActionsTriple);

            this.kryptonRibbonTabTuning.Groups.AddRange(new KryptonRibbonGroup[] {
                rbpgKryptonInjection,
                rbpgKryptonTurbo,
                rbpgKryptonLimiters,
                rbpgKryptonLinearization,
                rbpgKryptonPID,
                rbpgKryptonTuningActions
            });
        }

        private void InitializeSkinsTab()
        {
            this.rbpgKryptonSkins = new KryptonRibbonGroup { TextLine1 = "Themes" };
            
            KryptonRibbonGroupTriple skinsTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnVAGEDCDark = new KryptonRibbonGroupButton { TextLine1 = "VAGEDC", TextLine2 = "Dark", ImageLarge = GetResourceImage("AppSettings.png"), Tag = "CUSTOM_VAGEDC_DARK" };
            btnVAGEDCDark.Click += OnKryptonSkinClick;

            skinsTriple.Items.Add(btnVAGEDCDark);

            this.rbpgKryptonSkins.Items.Add(skinsTriple);
            this.kryptonRibbonTabSkins.Groups.Add(rbpgKryptonSkins);
        }

        private void InitializeHelpTab()
        {
            this.rbpgKryptonHelp = new KryptonRibbonGroup { TextLine1 = "Help" };
            KryptonRibbonGroupTriple helpTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnCheckUpdates = new KryptonRibbonGroupButton { TextLine1 = "Check for", TextLine2 = "Updates", ImageLarge = GetResourceImage("CheckForUpdates.png") };
            btnCheckUpdates.Click += (s, e) => btnCheckForUpdates_ItemClick(s, e);
            KryptonRibbonGroupButton btnReleaseNotes = new KryptonRibbonGroupButton { TextLine1 = "Release", TextLine2 = "Notes", ImageLarge = GetResourceImage("ReleaseNotes.png") };
            btnReleaseNotes.Click += (s, e) => btnReleaseNotes_ItemClick(s, e);
            KryptonRibbonGroupButton btnManual = new KryptonRibbonGroupButton { TextLine1 = "User", TextLine2 = "Manual", ImageLarge = GetResourceImage("UserManual.png") };
            btnManual.Click += (s, e) => btnUserManual_ItemClick(s, e);

            helpTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnCheckUpdates, btnReleaseNotes, btnManual });
            this.rbpgKryptonHelp.Items.Add(helpTriple);

            this.rbpgKryptonHelpAbout = new KryptonRibbonGroup { TextLine1 = "About" };
            KryptonRibbonGroupTriple aboutTriple = new KryptonRibbonGroupTriple { MaximumSize = GroupItemSize.Large, MinimumSize = GroupItemSize.Medium };

            KryptonRibbonGroupButton btnDocs = new KryptonRibbonGroupButton { TextLine1 = "EDC15P", TextLine2 = "Docs", ImageLarge = GetResourceImage("UserManual.png") };
            btnDocs.Click += (s, e) => btnEDC15PDocumentation_ItemClick(s, e);
            KryptonRibbonGroupButton btnAbout = new KryptonRibbonGroupButton { TextLine1 = "About", TextLine2 = "VAGEDCSuite", ImageLarge = GetResourceImage("AboutVagSuite.png") };
            btnAbout.Click += (s, e) => btnAbout_ItemClick(s, e);

            aboutTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnDocs, btnAbout });
            this.rbpgKryptonHelpAbout.Items.Add(aboutTriple);

            this.kryptonRibbonTabHelp.Groups.AddRange(new KryptonRibbonGroup[] { rbpgKryptonHelp, rbpgKryptonHelpAbout });
        }

        #region Krypton Ribbon Event Shims
        
        private void btnKryptonOpenFile_Click(object sender, EventArgs e)
        {
            btnOpenFile_ItemClick(sender, null);
        }

        private void btnKryptonSaveAs_Click(object sender, EventArgs e)
        {
            btnSaveAs_ItemClick(sender, null);
        }

        private void btnKryptonBinaryCompare_Click(object sender, EventArgs e)
        {
            btnBinaryCompare_ItemClick(sender, null);
        }

        private void btnKryptonOpenProject_Click(object sender, EventArgs e)
        {
            btnOpenProject_ItemClick(sender, null);
        }

        private void btnKryptonCloseProject_Click(object sender, EventArgs e)
        {
            btnCloseProject_ItemClick(sender, null);
        }

        private void btnKryptonCreateProject_Click(object sender, EventArgs e)
        {
            btnCreateAProject_ItemClick(sender, null);
        }

        private void btnKryptonSearchMaps_Click(object sender, EventArgs e)
        {
            btnSearchMaps_ItemClick(sender, null);
        }

        private void btnKryptonViewHex_Click(object sender, EventArgs e)
        {
            btnViewFileInHex_ItemClick(sender, null);
        }

        private void btnKryptonChecksum_Click(object sender, EventArgs e)
        {
            btnChecksum_ItemClick(sender, null);
        }

        private void btnKryptonTurboAnalysis_Click(object sender, EventArgs e)
        {
            btnTurboAnalysis_ItemClick(sender, null);
        }

        private void OnKryptonSkinClick(object sender, EventArgs e)
        {
            if (sender is KryptonRibbonGroupButton btn && btn.Tag != null)
            {
                if (btn.Tag.ToString() == "CUSTOM_VAGEDC_DARK")
                {
                    VAGEDCThemeManager.Instance.ActivateVAGEDCDark(this);
                    m_appSettings.UseVAGEDCDarkTheme = true;
                }
                else if (btn.Tag is PaletteMode mode)
                {
                    this.kryptonManager1.GlobalPaletteMode = (PaletteModeManager)((int)mode);
                    m_appSettings.UseVAGEDCDarkTheme = false;
                    VAGEDCThemeManager.Instance.DeactivateCustomTheme();
                }
            }
        }

        private Image GetResourceImage(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return null;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string fullResourceName = null;
                foreach (string name in assembly.GetManifestResourceNames())
                {
                    if (name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase))
                    {
                        fullResourceName = name;
                        break;
                    }
                }

                if (fullResourceName != null)
                {
                    using (Stream stream = assembly.GetManifestResourceStream(fullResourceName))
                    {
                        if (stream != null)
                        {
                            return Image.FromStream(stream);
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        #endregion
    }
}
