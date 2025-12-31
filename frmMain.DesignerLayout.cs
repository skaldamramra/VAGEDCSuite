namespace VAGSuite
{
    public partial class frmMain
    {
        // Resource manager for accessing localized resources
        private System.ComponentModel.ComponentResourceManager resources;

        /// <summary>
        /// Configures form layout
        /// </summary>
        private void ConfigureLayout()
        {
            // Form settings
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(
                LayoutConstants.DefaultFormWidth, 
                LayoutConstants.DefaultFormHeight);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VAGEDCSuite";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            // Add controls to form
            this.Controls.Add(this.dockSymbols);
            this.Controls.Add(this.ribbonStatusBar1);
            this.Controls.Add(this.ribbonControl1);

            // Form event handlers
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
        }

        /// <summary>
        /// Configures ribbon pages and groups
        /// </summary>
        private void ConfigureRibbonPages()
        {
            // Add pages to ribbon
            this.ribbonControl1.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
                this.ribFile,
                this.ribActions,
                this.rbnPageTuning,
                this.rbnPageSkins,
                this.rbnPageHelp
            });

            // Configure File page
            this.ribFile.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
                this.rbpgGeneralFile,
                this.rbnSettings,
                this.rbnPageGourpProjects
            });
            this.ribFile.Name = "ribFile";
            this.ribFile.Text = "File";

            // Configure File > General group
            this.rbpgGeneralFile.AllowTextClipping = false;
            this.rbpgGeneralFile.ItemLinks.Add(this.btnOpenFile);
            this.rbpgGeneralFile.ItemLinks.Add(this.btnTestFiles);
            this.rbpgGeneralFile.ItemLinks.Add(this.btnSaveAs);
            this.rbpgGeneralFile.ItemLinks.Add(this.btnCreateBackup);
            this.rbpgGeneralFile.ItemLinks.Add(this.btnExportXDF, true);
            this.rbpgGeneralFile.ItemLinks.Add(this.btnBuildLibrary);
            this.rbpgGeneralFile.ItemLinks.Add(this.barSubItem1);
            this.rbpgGeneralFile.Name = "rbpgGeneralFile";
            this.rbpgGeneralFile.Text = "General";

            // Configure File > Settings group
            this.rbnSettings.AllowTextClipping = false;
            this.rbnSettings.ItemLinks.Add(this.btnAppSettings);
            this.rbnSettings.ItemLinks.Add(this.btnLookupPartnumber);
            this.rbnSettings.Name = "rbnSettings";
            this.rbnSettings.Text = "Settings";

            // Configure File > Projects group
            this.rbnPageGourpProjects.AllowTextClipping = false;
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnCreateAProject);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnOpenProject);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnCloseProject);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnShowTransactionLog);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnRollback);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnRollforward);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnRebuildFile);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnEditProject);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnAddNoteToProject);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnShowProjectLogbook);
            this.rbnPageGourpProjects.ItemLinks.Add(this.btnProduceLatestBinary);
            this.rbnPageGourpProjects.Name = "rbnPageGourpProjects";
            this.rbnPageGourpProjects.Text = "Projects";

            // Configure Actions page
            this.ribActions.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
                this.ribpgGeneralActions,
                this.rbnPageGroupInformation,
                this.rbnPageGroupTools
            });
            this.ribActions.Name = "ribActions";
            this.ribActions.Text = "Actions";

            // Configure Actions > General group
            this.ribpgGeneralActions.AllowTextClipping = false;
            this.ribpgGeneralActions.ItemLinks.Add(this.btnChecksum);
            this.ribpgGeneralActions.ItemLinks.Add(this.btnBinaryCompare);
            this.ribpgGeneralActions.ItemLinks.Add(this.btnCompareFiles);
            this.ribpgGeneralActions.Name = "ribpgGeneralActions";
            this.ribpgGeneralActions.Text = "General";

            // Configure Actions > Information group
            this.rbnPageGroupInformation.AllowTextClipping = false;
            this.rbnPageGroupInformation.ItemLinks.Add(this.btnFirmwareInformation);
            this.rbnPageGroupInformation.ItemLinks.Add(this.btnVINDecoder);
            this.rbnPageGroupInformation.Name = "rbnPageGroupInformation";
            this.rbnPageGroupInformation.Text = "Information";

            // Configure Actions > Tools group
            this.rbnPageGroupTools.AllowTextClipping = false;
            this.rbnPageGroupTools.ItemLinks.Add(this.btnViewFileInHex);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnSearchMaps);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnAirmassResult);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnActivateLaunchControl);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnActivateSmokeLimiters);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnEditEEProm);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnMergeFiles, true);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnSplitFiles);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnExportToExcel, true);
            this.rbnPageGroupTools.ItemLinks.Add(this.btnExcelImport);
            this.rbnPageGroupTools.Name = "rbnPageGroupTools";
            this.rbnPageGroupTools.Text = "Tools";

            // Configure Tuning page
            this.rbnPageTuning.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
                this.rbnPageInjection,
                this.rbnPageTurbo
            });
            this.rbnPageTuning.Name = "rbnPageTuning";
            this.rbnPageTuning.Text = "Tuning";

            // Configure Tuning > Injection group
            this.rbnPageInjection.AllowTextClipping = false;
            this.rbnPageInjection.ItemLinks.Add(this.btnDriverWish);
            this.rbnPageInjection.ItemLinks.Add(this.btnTorqueLimiter);
            this.rbnPageInjection.ItemLinks.Add(this.btnSmokeLimiter);
            this.rbnPageInjection.ItemLinks.Add(this.btnIQByMap);
            this.rbnPageInjection.ItemLinks.Add(this.btnIQByMAF);
            this.rbnPageInjection.ItemLinks.Add(this.btnSOILimiter);
            this.rbnPageInjection.ItemLinks.Add(this.btnStartOfInjection);
            this.rbnPageInjection.ItemLinks.Add(this.btnInjectorDuration);
            this.rbnPageInjection.ItemLinks.Add(this.btnStartIQ);
            this.rbnPageInjection.ItemLinks.Add(this.btnBIPBasicCharacteristic, true);
            this.rbnPageInjection.ItemLinks.Add(this.btnPIDMapP);
            this.rbnPageInjection.ItemLinks.Add(this.btnPIDMapI);
            this.rbnPageInjection.ItemLinks.Add(this.btnPIDMapD);
            this.rbnPageInjection.ItemLinks.Add(this.btnDurationLimiter);
            this.rbnPageInjection.ItemLinks.Add(this.btnMAFLimiter);
            this.rbnPageInjection.ItemLinks.Add(this.btnMAPLimiter);
            this.rbnPageInjection.ItemLinks.Add(this.barSubItemVCDSDiag, true);
            this.rbnPageInjection.Name = "rbnPageInjection";
            this.rbnPageInjection.Text = "Injection quantity";

            // Configure Tuning > Turbo group
            this.rbnPageTurbo.AllowTextClipping = false;
            this.rbnPageTurbo.ItemLinks.Add(this.btnTargetBoost);
            this.rbnPageTurbo.ItemLinks.Add(this.btnBoostPressureLimiter);
            this.rbnPageTurbo.ItemLinks.Add(this.btnBoostPressureLimitSVBL);
            this.rbnPageTurbo.ItemLinks.Add(this.btnN75Map);
            this.rbnPageTurbo.ItemLinks.Add(this.btnEGRMap);
            this.rbnPageTurbo.ItemLinks.Add(this.btnMAFLinearization, true);
            this.rbnPageTurbo.ItemLinks.Add(this.btnMAPLinearization);
            this.rbnPageTurbo.Name = "rbnPageTurbo";
            this.rbnPageTurbo.Text = "Turbo";

            // Configure Skins page
            this.rbnPageSkins.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
                this.rbnPageGroupSkins
            });
            this.rbnPageSkins.Name = "rbnPageSkins";
            this.rbnPageSkins.Text = "Skins";

            this.rbnPageGroupSkins.AllowTextClipping = false;
            this.rbnPageGroupSkins.Name = "rbnPageGroupSkins";
            this.rbnPageGroupSkins.Text = "Available skins";

            // Configure Help page
            this.rbnPageHelp.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
                this.rbnPageGroupUpdate,
                this.rbnPageDocumentation
            });
            this.rbnPageHelp.Name = "rbnPageHelp";
            this.rbnPageHelp.Text = "Help";

            this.rbnPageGroupUpdate.AllowTextClipping = false;
            this.rbnPageGroupUpdate.ItemLinks.Add(this.btnCheckForUpdates);
            this.rbnPageGroupUpdate.ItemLinks.Add(this.btnReleaseNotes);
            this.rbnPageGroupUpdate.Name = "rbnPageGroupUpdate";
            this.rbnPageGroupUpdate.Text = "Updates";

            this.rbnPageDocumentation.AllowTextClipping = false;
            this.rbnPageDocumentation.ItemLinks.Add(this.btnEDC15PDocumentation);
            this.rbnPageDocumentation.ItemLinks.Add(this.btnUserManual);
            this.rbnPageDocumentation.ItemLinks.Add(this.btnAbout);
            this.rbnPageDocumentation.Name = "rbnPageDocumentation";
            this.rbnPageDocumentation.Text = "Documentation";
        }
    }
}