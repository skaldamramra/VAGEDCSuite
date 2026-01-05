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
        
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonGeneralActions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonGeneralActionsTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonSearchMaps;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonViewHex;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonChecksum;
        
        // Tuning Tab Components
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonInjection;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonInjectionTriple1;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonInjectionTriple2;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonTurbo;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonTurboTriple1;
        
        // Help Tab Components
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonHelpDocs;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonHelpDocsTriple;

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
            
            // Enable the new Ribbon
            this.kryptonRibbon1.Visible = true;


            //
            // kryptonRibbonTabFile
            //
            this.kryptonRibbonTabFile.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
                this.rbpgKryptonGeneralFile,
                this.rbpgKryptonProjects
            });
            this.kryptonRibbonTabFile.Text = "File";

            //
            // rbpgKryptonGeneralFile
            //
            this.rbpgKryptonGeneralFile.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
                this.rbpgKryptonGeneralFileTriple
            });
            this.rbpgKryptonGeneralFile.TextLine1 = "General";
            this.rbpgKryptonGeneralFile.TextLine2 = "File";

            //
            // rbpgKryptonGeneralFileTriple
            //
            this.rbpgKryptonGeneralFileTriple.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
                this.btnKryptonOpenFile,
                this.btnKryptonSaveAs,
                this.btnKryptonBinaryCompare
            });
            this.rbpgKryptonGeneralFileTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonGeneralFileTriple.MinimumSize = GroupItemSize.Large;

            //
            // btnKryptonOpenFile
            //
            this.btnKryptonOpenFile.TextLine1 = "Open";
            this.btnKryptonOpenFile.TextLine2 = "File";
            this.btnKryptonOpenFile.ImageLarge = GetResourceImage("vagedc.ico");
            this.btnKryptonOpenFile.Click += new System.EventHandler(this.btnKryptonOpenFile_Click);

            //
            // btnKryptonSaveAs
            //
            this.btnKryptonSaveAs.TextLine1 = "Save";
            this.btnKryptonSaveAs.TextLine2 = "As...";
            this.btnKryptonSaveAs.ImageLarge = GetResourceImage("vagedc.ico");
            this.btnKryptonSaveAs.Click += new System.EventHandler(this.btnKryptonSaveAs_Click);

            //
            // btnKryptonBinaryCompare
            //
            this.btnKryptonBinaryCompare.TextLine1 = "Binary";
            this.btnKryptonBinaryCompare.TextLine2 = "Compare";
            this.btnKryptonBinaryCompare.ImageLarge = GetResourceImage("vagedc.ico");
            this.btnKryptonBinaryCompare.Click += new System.EventHandler(this.btnKryptonBinaryCompare_Click);

            KryptonRibbonGroupTriple rbpgKryptonGeneralFileTriple2 = new KryptonRibbonGroupTriple();
            rbpgKryptonGeneralFileTriple2.MaximumSize = GroupItemSize.Large;
            rbpgKryptonGeneralFileTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonProduceBinary = new KryptonRibbonGroupButton { TextLine1 = "Produce", TextLine2 = "Binary", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonProduceBinary.Click += (s, e) => btnProduceLatestBinary_ItemClick(s, null);
            
            KryptonRibbonGroupButton btnKryptonCompareFiles = new KryptonRibbonGroupButton { TextLine1 = "Compare", TextLine2 = "Files", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonCompareFiles.Click += (s, e) => btnCompareFiles_ItemClick(s, null);

            rbpgKryptonGeneralFileTriple2.Items.Add(btnKryptonProduceBinary);
            rbpgKryptonGeneralFileTriple2.Items.Add(btnKryptonCompareFiles);
            this.rbpgKryptonGeneralFile.Items.Add(rbpgKryptonGeneralFileTriple2);

            //
            // rbpgKryptonProjects
            //
            this.rbpgKryptonProjects.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
                this.rbpgKryptonProjectsTriple
            });
            this.rbpgKryptonProjects.TextLine1 = "Projects";

            //
            // rbpgKryptonProjectsTriple
            //
            this.rbpgKryptonProjectsTriple.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
                this.btnKryptonOpenProject,
                this.btnKryptonCloseProject,
                this.btnKryptonCreateProject
            });
            this.rbpgKryptonProjectsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonProjectsTriple.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupTriple rbpgKryptonProjectsTriple2 = new KryptonRibbonGroupTriple();
            rbpgKryptonProjectsTriple2.MaximumSize = GroupItemSize.Large;
            rbpgKryptonProjectsTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonBackup = new KryptonRibbonGroupButton { TextLine1 = "Create", TextLine2 = "Backup", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonBackup.Click += (s, e) => btnCreateBackup_ItemClick(s, null);
            rbpgKryptonProjectsTriple2.Items.Add(btnKryptonBackup);
            this.rbpgKryptonProjects.Items.Add(rbpgKryptonProjectsTriple2);

            //
            // btnKryptonOpenProject
            //
            this.btnKryptonOpenProject.TextLine1 = "Open";
            this.btnKryptonOpenProject.TextLine2 = "Project";
            this.btnKryptonOpenProject.ImageLarge = GetResourceImage("vagedc.ico");
            this.btnKryptonOpenProject.Click += new System.EventHandler(this.btnKryptonOpenProject_Click);

            //
            // btnKryptonCloseProject
            //
            this.btnKryptonCloseProject.TextLine1 = "Close";
            this.btnKryptonCloseProject.TextLine2 = "Project";
            this.btnKryptonCloseProject.ImageLarge = GetResourceImage("vagedc.ico");
            this.btnKryptonCloseProject.Click += new System.EventHandler(this.btnKryptonCloseProject_Click);

            //
            // btnKryptonCreateProject
            //
            this.btnKryptonCreateProject.TextLine1 = "Create";
            this.btnKryptonCreateProject.TextLine2 = "Project";
            this.btnKryptonCreateProject.ImageLarge = GetResourceImage("vagedc.ico");
            this.btnKryptonCreateProject.Click += new System.EventHandler(this.btnKryptonCreateProject_Click);
            
            //
            // kryptonRibbonTabActions
            //
            this.kryptonRibbonTabActions.Groups.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup[] {
                this.rbpgKryptonGeneralActions
            });
            this.kryptonRibbonTabActions.Text = "Actions";

            //
            // rbpgKryptonGeneralActions
            //
            this.rbpgKryptonGeneralActions.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupContainer[] {
                this.rbpgKryptonGeneralActionsTriple
            });
            this.rbpgKryptonGeneralActions.TextLine1 = "General";
            this.rbpgKryptonGeneralActions.TextLine2 = "Actions";

            //
            // rbpgKryptonGeneralActionsTriple
            //
            this.rbpgKryptonGeneralActionsTriple.Items.AddRange(new ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupItem[] {
                this.btnKryptonSearchMaps,
                this.btnKryptonViewHex,
                this.btnKryptonChecksum
            });
            this.rbpgKryptonGeneralActionsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonGeneralActionsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonSearchMaps.TextLine1 = "Search";
            this.btnKryptonSearchMaps.TextLine2 = "Maps";
            this.btnKryptonSearchMaps.ImageLarge = GetResourceImage("vagedc.ico");

            this.btnKryptonViewHex.TextLine1 = "View";
            this.btnKryptonViewHex.TextLine2 = "Hex";
            this.btnKryptonViewHex.ImageLarge = GetResourceImage("vagedc.ico");

            this.btnKryptonChecksum.TextLine1 = "Verify";
            this.btnKryptonChecksum.TextLine2 = "Checksum";
            this.btnKryptonChecksum.ImageLarge = GetResourceImage("vagedc.ico");

            KryptonRibbonGroupTriple rbpgKryptonActionsTriple2 = new KryptonRibbonGroupTriple();
            rbpgKryptonActionsTriple2.MaximumSize = GroupItemSize.Large;
            rbpgKryptonActionsTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonFirmwareInfo = new KryptonRibbonGroupButton { TextLine1 = "Firmware", TextLine2 = "Info", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonFirmwareInfo.Click += (s, e) => btnFirmwareInformation_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonVINDecoder = new KryptonRibbonGroupButton { TextLine1 = "VIN", TextLine2 = "Decoder", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonVINDecoder.Click += (s, e) => btnVINDecoder_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonAirmass = new KryptonRibbonGroupButton { TextLine1 = "Airmass", TextLine2 = "Result", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonAirmass.Click += (s, e) => btnAirmassResult_ItemClick(s, null);

            rbpgKryptonActionsTriple2.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonFirmwareInfo, btnKryptonVINDecoder, btnKryptonAirmass });
            this.rbpgKryptonGeneralActions.Items.Add(rbpgKryptonActionsTriple2);

            // Project Actions Triple
            KryptonRibbonGroupTriple rbpgKryptonProjectActionsTriple = new KryptonRibbonGroupTriple();
            rbpgKryptonProjectActionsTriple.MaximumSize = GroupItemSize.Large;
            rbpgKryptonProjectActionsTriple.MinimumSize = GroupItemSize.Large;

            this.btnKryptonTransactionLog = new KryptonRibbonGroupButton { TextLine1 = "Transaction", TextLine2 = "Log", ImageLarge = GetResourceImage("vagedc.ico") };
            this.btnKryptonTransactionLog.Click += (s, e) => btnShowTransactionLog_ItemClick(s, null);
            this.btnKryptonRollback = new KryptonRibbonGroupButton { TextLine1 = "Rollback", ImageLarge = GetResourceImage("vagedc.ico") };
            this.btnKryptonRollback.Click += (s, e) => btnRollback_ItemClick(s, null);
            this.btnKryptonRollforward = new KryptonRibbonGroupButton { TextLine1 = "Roll", TextLine2 = "Forward", ImageLarge = GetResourceImage("vagedc.ico") };
            this.btnKryptonRollforward.Click += (s, e) => btnRollforward_ItemClick(s, null);

            rbpgKryptonProjectActionsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { this.btnKryptonTransactionLog, this.btnKryptonRollback, this.btnKryptonRollforward });
            this.rbpgKryptonGeneralActions.Items.Add(rbpgKryptonProjectActionsTriple);

            // Tools Group
            KryptonRibbonGroup rbpgKryptonTools = new KryptonRibbonGroup { TextLine1 = "Tools" };
            KryptonRibbonGroupTriple rbpgKryptonToolsTriple = new KryptonRibbonGroupTriple();
            rbpgKryptonToolsTriple.MaximumSize = GroupItemSize.Large;
            rbpgKryptonToolsTriple.MinimumSize = GroupItemSize.Large;
            
            KryptonRibbonGroupButton btnKryptonLookupPart = new KryptonRibbonGroupButton { TextLine1 = "Lookup", TextLine2 = "Part", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonLookupPart.Click += (s, e) => btnLookupPartnumber_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonExportXDF = new KryptonRibbonGroupButton { TextLine1 = "Export", TextLine2 = "XDF", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonExportXDF.Click += (s, e) => btnExportXDF_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonEditEEProm = new KryptonRibbonGroupButton { TextLine1 = "Edit", TextLine2 = "EEProm", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonEditEEProm.Click += (s, e) => btnEditEEProm_ItemClick(s, null);
            
            rbpgKryptonToolsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonLookupPart, btnKryptonExportXDF, btnKryptonEditEEProm });
            rbpgKryptonTools.Items.Add(rbpgKryptonToolsTriple);

            KryptonRibbonGroupTriple rbpgKryptonToolsTriple2 = new KryptonRibbonGroupTriple();
            rbpgKryptonToolsTriple2.MaximumSize = GroupItemSize.Large;
            rbpgKryptonToolsTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonMerge = new KryptonRibbonGroupButton { TextLine1 = "Merge", TextLine2 = "Files", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonMerge.Click += (s, e) => btnMergeFiles_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonSplit = new KryptonRibbonGroupButton { TextLine1 = "Split", TextLine2 = "Files", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonSplit.Click += (s, e) => btnSplitFiles_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonExcelExp = new KryptonRibbonGroupButton { TextLine1 = "Excel", TextLine2 = "Export", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonExcelExp.Click += (s, e) => btnExportToExcel_ItemClick(s, null);

            rbpgKryptonToolsTriple2.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonMerge, btnKryptonSplit, btnKryptonExcelExp });
            rbpgKryptonTools.Items.Add(rbpgKryptonToolsTriple2);

            // Settings Group
            KryptonRibbonGroup rbpgKryptonSettings = new KryptonRibbonGroup { TextLine1 = "Settings" };
            KryptonRibbonGroupTriple rbpgKryptonSettingsTriple = new KryptonRibbonGroupTriple();
            rbpgKryptonSettingsTriple.MaximumSize = GroupItemSize.Large;
            rbpgKryptonSettingsTriple.MinimumSize = GroupItemSize.Large;
            KryptonRibbonGroupButton btnKryptonSettings = new KryptonRibbonGroupButton { TextLine1 = "App", TextLine2 = "Settings", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonSettings.Click += (s, e) => btnAppSettings_ItemClick(s, null);
            rbpgKryptonSettingsTriple.Items.Add(btnKryptonSettings);
            rbpgKryptonSettings.Items.Add(rbpgKryptonSettingsTriple);

            this.kryptonRibbonTabActions.Groups.AddRange(new KryptonRibbonGroup[] { rbpgKryptonTools, rbpgKryptonSettings });

            //
            // btnKryptonSearchMaps
            //
            this.btnKryptonSearchMaps.TextLine1 = "Search Maps";
            this.btnKryptonSearchMaps.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonSearchMaps.Click += new System.EventHandler(this.btnKryptonSearchMaps_Click);

            //
            // btnKryptonViewHex
            //
            this.btnKryptonViewHex.TextLine1 = "View Hex";
            this.btnKryptonViewHex.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonViewHex.Click += new System.EventHandler(this.btnKryptonViewHex_Click);

            //
            // btnKryptonChecksum
            //
            this.btnKryptonChecksum.TextLine1 = "Verify Checksum";
            this.btnKryptonChecksum.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonChecksum.Click += new System.EventHandler(this.btnKryptonChecksum_Click);

            //
            // kryptonRibbonTabTuning
            //
            this.kryptonRibbonTabTuning.Text = "Tuning";
            this.rbpgKryptonInjection = new KryptonRibbonGroup();
            this.rbpgKryptonInjection.TextLine1 = "Injection";
            this.rbpgKryptonInjectionTriple1 = new KryptonRibbonGroupTriple();
            this.rbpgKryptonInjectionTriple1.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonInjectionTriple1.MinimumSize = GroupItemSize.Large;
            
            KryptonRibbonGroupButton btnKryptonDriverWish = new KryptonRibbonGroupButton { TextLine1 = "Driver", TextLine2 = "Wish", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonDriverWish.Click += (s, e) => btnDriverWish_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonTorqueLimiter = new KryptonRibbonGroupButton { TextLine1 = "Torque", TextLine2 = "Limiter", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonTorqueLimiter.Click += (s, e) => btnTorqueLimiter_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonSmokeLimiter = new KryptonRibbonGroupButton { TextLine1 = "Smoke", TextLine2 = "Limiter", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonSmokeLimiter.Click += (s, e) => btnSmokeLimiter_ItemClick(s, null);
            
            this.rbpgKryptonInjectionTriple1.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonDriverWish, btnKryptonTorqueLimiter, btnKryptonSmokeLimiter });
            this.rbpgKryptonInjection.Items.Add(this.rbpgKryptonInjectionTriple1);

            this.rbpgKryptonInjectionTriple2 = new KryptonRibbonGroupTriple();
            this.rbpgKryptonInjectionTriple2.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonInjectionTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonSOI = new KryptonRibbonGroupButton { TextLine1 = "Start of", TextLine2 = "Injection", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonSOI.Click += (s, e) => btnStartOfInjection_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonDuration = new KryptonRibbonGroupButton { TextLine1 = "Injector", TextLine2 = "Duration", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonDuration.Click += (s, e) => btnInjectorDuration_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonEGR = new KryptonRibbonGroupButton { TextLine1 = "EGR", TextLine2 = "Map", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonEGR.Click += (s, e) => btnEGRMap_ItemClick(s, null);

            this.rbpgKryptonInjectionTriple2.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonSOI, btnKryptonDuration, btnKryptonEGR });
            this.rbpgKryptonInjection.Items.Add(this.rbpgKryptonInjectionTriple2);
            
            this.rbpgKryptonTurbo = new KryptonRibbonGroup();
            this.rbpgKryptonTurbo.TextLine1 = "Turbo";
            this.rbpgKryptonTurboTriple1 = new KryptonRibbonGroupTriple();
            this.rbpgKryptonTurboTriple1.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonTurboTriple1.MinimumSize = GroupItemSize.Large;
            
            KryptonRibbonGroupButton btnKryptonTargetBoost = new KryptonRibbonGroupButton { TextLine1 = "Target", TextLine2 = "Boost", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonTargetBoost.Click += (s, e) => btnTargetBoost_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonBoostLimiter = new KryptonRibbonGroupButton { TextLine1 = "Boost", TextLine2 = "Limiter", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonBoostLimiter.Click += (s, e) => btnBoostPressureLimiter_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonN75 = new KryptonRibbonGroupButton { TextLine1 = "N75", TextLine2 = "Map", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonN75.Click += (s, e) => btnN75Map_ItemClick(s, null);
            
            this.rbpgKryptonTurboTriple1.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonTargetBoost, btnKryptonBoostLimiter, btnKryptonN75 });
            this.rbpgKryptonTurbo.Items.Add(this.rbpgKryptonTurboTriple1);

            // Turbo Triple 2
            KryptonRibbonGroupTriple rbpgKryptonTurboTriple2 = new KryptonRibbonGroupTriple();
            rbpgKryptonTurboTriple2.MaximumSize = GroupItemSize.Large;
            rbpgKryptonTurboTriple2.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonBoostSVBL = new KryptonRibbonGroupButton { TextLine1 = "Boost", TextLine2 = "SVBL", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonBoostSVBL.Click += (s, e) => btnBoostPressureLimitSVBL_ItemClick(s, null);
            
            rbpgKryptonTurboTriple2.Items.Add(btnKryptonBoostSVBL);
            this.rbpgKryptonTurbo.Items.Add(rbpgKryptonTurboTriple2);

            // Injection Triple 3
            KryptonRibbonGroupTriple rbpgKryptonInjectionTriple3 = new KryptonRibbonGroupTriple();
            rbpgKryptonInjectionTriple3.MaximumSize = GroupItemSize.Large;
            rbpgKryptonInjectionTriple3.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonSOILimiter = new KryptonRibbonGroupButton { TextLine1 = "SOI", TextLine2 = "Limiter", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonSOILimiter.Click += (s, e) => btnSOILimiter_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonStartIQ = new KryptonRibbonGroupButton { TextLine1 = "Start", TextLine2 = "IQ", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonStartIQ.Click += (s, e) => btnStartIQ_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonBIP = new KryptonRibbonGroupButton { TextLine1 = "BIP", TextLine2 = "Basic", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonBIP.Click += (s, e) => btnBIPBasicCharacteristic_ItemClick(s, null);

            rbpgKryptonInjectionTriple3.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonSOILimiter, btnKryptonStartIQ, btnKryptonBIP });
            this.rbpgKryptonInjection.Items.Add(rbpgKryptonInjectionTriple3);

            // Injection Triple 4
            KryptonRibbonGroupTriple rbpgKryptonInjectionTriple4 = new KryptonRibbonGroupTriple();
            rbpgKryptonInjectionTriple4.MaximumSize = GroupItemSize.Large;
            rbpgKryptonInjectionTriple4.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonPIDP = new KryptonRibbonGroupButton { TextLine1 = "PID", TextLine2 = "P", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonPIDP.Click += (s, e) => btnPIDMapP_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonDurationLimiter = new KryptonRibbonGroupButton { TextLine1 = "Duration", TextLine2 = "Limiter", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonDurationLimiter.Click += (s, e) => btnDurationLimiter_ItemClick(s, null);

            rbpgKryptonInjectionTriple4.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonPIDP, btnKryptonDurationLimiter });
            this.rbpgKryptonInjection.Items.Add(rbpgKryptonInjectionTriple4);

            // Injection Triple 5
            KryptonRibbonGroupTriple rbpgKryptonInjectionTriple5 = new KryptonRibbonGroupTriple();
            rbpgKryptonInjectionTriple5.MaximumSize = GroupItemSize.Large;
            rbpgKryptonInjectionTriple5.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonMAFLimiter = new KryptonRibbonGroupButton { TextLine1 = "MAF", TextLine2 = "Limiter", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonMAFLimiter.Click += (s, e) => btnMAFLimiter_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonMAPLinearization = new KryptonRibbonGroupButton { TextLine1 = "MAP", TextLine2 = "Linear", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonMAPLinearization.Click += (s, e) => btnMAPLinearization_ItemClick(s, null);

            rbpgKryptonInjectionTriple5.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonMAFLimiter, btnKryptonMAPLinearization });
            this.rbpgKryptonInjection.Items.Add(rbpgKryptonInjectionTriple5);

            KryptonRibbonGroup rbpgKryptonSpecial = new KryptonRibbonGroup { TextLine1 = "Special" };
            KryptonRibbonGroupTriple rbpgKryptonSpecialTriple = new KryptonRibbonGroupTriple();
            rbpgKryptonSpecialTriple.MaximumSize = GroupItemSize.Large;
            rbpgKryptonSpecialTriple.MinimumSize = GroupItemSize.Large;

            KryptonRibbonGroupButton btnKryptonLaunch = new KryptonRibbonGroupButton { TextLine1 = "Launch", TextLine2 = "Control", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonLaunch.Click += (s, e) => btnActivateLaunchControl_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonSmokeLim = new KryptonRibbonGroupButton { TextLine1 = "Smoke", TextLine2 = "Limiters", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonSmokeLim.Click += (s, e) => btnActivateSmokeLimiters_ItemClick(s, null);

            rbpgKryptonSpecialTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonLaunch, btnKryptonSmokeLim });
            rbpgKryptonSpecial.Items.Add(rbpgKryptonSpecialTriple);
            
            this.kryptonRibbonTabTuning.Groups.AddRange(new KryptonRibbonGroup[] { this.rbpgKryptonInjection, this.rbpgKryptonTurbo, rbpgKryptonSpecial });

            //
            // kryptonRibbonTabSkins
            //
            this.kryptonRibbonTabSkins.Text = "Skins";
            KryptonRibbonGroup rbpgSkins = new KryptonRibbonGroup();
            rbpgSkins.TextLine1 = "Application Skins";
            KryptonRibbonGroupTriple rbpgSkinsTriple = new KryptonRibbonGroupTriple();
            
            KryptonRibbonGroupButton btnSkinVAGEDCDark = new KryptonRibbonGroupButton();
            btnSkinVAGEDCDark.TextLine1 = "VAGEDC Dark";
            btnSkinVAGEDCDark.Tag = "CUSTOM_VAGEDC_DARK";
            btnSkinVAGEDCDark.Click += new EventHandler(OnKryptonSkinClick);
            
            KryptonRibbonGroupButton btnSkinOffice2010 = new KryptonRibbonGroupButton();
            btnSkinOffice2010.TextLine1 = "Office 2010";
            btnSkinOffice2010.Tag = PaletteMode.Office2010Blue;
            btnSkinOffice2010.Click += new EventHandler(OnKryptonSkinClick);

            KryptonRibbonGroupButton btnSkinSparkle = new KryptonRibbonGroupButton();
            btnSkinSparkle.TextLine1 = "Sparkle";
            btnSkinSparkle.Tag = PaletteMode.SparkleBlue;
            btnSkinSparkle.Click += new EventHandler(OnKryptonSkinClick);

            rbpgSkinsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnSkinVAGEDCDark, btnSkinOffice2010, btnSkinSparkle });
            rbpgSkins.Items.Add(rbpgSkinsTriple);
            this.kryptonRibbonTabSkins.Groups.Add(rbpgSkins);

            //
            // kryptonRibbonTabHelp
            //
            this.kryptonRibbonTabHelp.Text = "Help";
            this.rbpgKryptonHelpDocs = new KryptonRibbonGroup();
            this.rbpgKryptonHelpDocs.TextLine1 = "Documentation";
            this.rbpgKryptonHelpDocsTriple = new KryptonRibbonGroupTriple();
            this.rbpgKryptonHelpDocsTriple.MaximumSize = GroupItemSize.Large;
            this.rbpgKryptonHelpDocsTriple.MinimumSize = GroupItemSize.Large;
            
            KryptonRibbonGroupButton btnKryptonUserManual = new KryptonRibbonGroupButton { TextLine1 = "User", TextLine2 = "Manual", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonUserManual.Click += (s, e) => btnUserManual_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonReleaseNotes = new KryptonRibbonGroupButton { TextLine1 = "Release", TextLine2 = "Notes", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonReleaseNotes.Click += (s, e) => btnReleaseNotes_ItemClick(s, null);
            KryptonRibbonGroupButton btnKryptonAbout = new KryptonRibbonGroupButton { TextLine1 = "About", TextLine2 = "VAGSuite", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonAbout.Click += (s, e) => btnAbout_ItemClick(s, null);
            
            this.rbpgKryptonHelpDocsTriple.Items.AddRange(new KryptonRibbonGroupItem[] { btnKryptonUserManual, btnKryptonReleaseNotes, btnKryptonAbout });
            this.rbpgKryptonHelpDocs.Items.Add(this.rbpgKryptonHelpDocsTriple);

            KryptonRibbonGroup rbpgKryptonUpdate = new KryptonRibbonGroup { TextLine1 = "Update" };
            KryptonRibbonGroupTriple rbpgKryptonUpdateTriple = new KryptonRibbonGroupTriple();
            rbpgKryptonUpdateTriple.MaximumSize = GroupItemSize.Large;
            rbpgKryptonUpdateTriple.MinimumSize = GroupItemSize.Large;
            KryptonRibbonGroupButton btnKryptonUpdate = new KryptonRibbonGroupButton { TextLine1 = "Check for", TextLine2 = "Updates", ImageLarge = GetResourceImage("vagedc.ico") };
            btnKryptonUpdate.Click += (s, e) => btnCheckForUpdates_ItemClick(s, null);
            rbpgKryptonUpdateTriple.Items.Add(btnKryptonUpdate);
            rbpgKryptonUpdate.Items.Add(rbpgKryptonUpdateTriple);

            this.kryptonRibbonTabHelp.Groups.AddRange(new KryptonRibbonGroup[] { this.rbpgKryptonHelpDocs, rbpgKryptonUpdate });

            ((System.ComponentModel.ISupportInitialize)(this.kryptonRibbon1)).EndInit();

            // Add to form controls and set docking
            // CRITICAL: Do NOT call BringToFront() here - let the initialization order handle Z-order
            this.Controls.Add(this.kryptonRibbon1);
            this.kryptonRibbon1.Dock = DockStyle.Top;
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

        private void OnKryptonSkinClick(object sender, EventArgs e)
        {
            if (sender is KryptonRibbonGroupButton btn && btn.Tag != null)
            {
                if (btn.Tag.ToString() == "CUSTOM_VAGEDC_DARK")
                {
                    VAGEDCThemeManager.Instance.ActivateVAGEDCDark(this);
                    m_appSettings.UseVAGEDCDarkTheme = true;
                    m_appSettings.Skinname = "VAGEDC Dark";
                }
                else if (btn.Tag is PaletteMode mode)
                {
                    // Convert PaletteMode to PaletteModeManager for the manager component
                    this.kryptonManager1.GlobalPaletteMode = (PaletteModeManager)((int)mode);
                    m_appSettings.UseVAGEDCDarkTheme = false;
                    m_appSettings.Skinname = mode.ToString();
                    VAGEDCThemeManager.Instance.DeactivateCustomTheme();
                }
            }
        }

        private Image GetResourceImage(string resourceName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                // Try to find the resource by name (case-insensitive)
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
                            if (resourceName.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                            {
                                return new Icon(stream).ToBitmap();
                            }
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