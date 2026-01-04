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
        
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroup rbpgKryptonGeneralActions;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupTriple rbpgKryptonGeneralActionsTriple;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonSearchMaps;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonViewHex;
        private ComponentFactory.Krypton.Ribbon.KryptonRibbonGroupButton btnKryptonChecksum;

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
            
            // Enable the new Ribbon and hide the old DevExpress one
            this.kryptonRibbon1.Visible = true;
            this.ribbonControl1.Visible = false;


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

            //
            // btnKryptonOpenFile
            //
            this.btnKryptonOpenFile.TextLine1 = "Open File";
            this.btnKryptonOpenFile.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonOpenFile.Click += new System.EventHandler(this.btnKryptonOpenFile_Click);

            //
            // btnKryptonSaveAs
            //
            this.btnKryptonSaveAs.TextLine1 = "Save As...";
            this.btnKryptonSaveAs.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonSaveAs.Click += new System.EventHandler(this.btnKryptonSaveAs_Click);

            //
            // btnKryptonBinaryCompare
            //
            this.btnKryptonBinaryCompare.TextLine1 = "Compare";
            this.btnKryptonBinaryCompare.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonBinaryCompare.Click += new System.EventHandler(this.btnKryptonBinaryCompare_Click);

            //
            // rbpgKryptonGeneralFileTriple
            //
            this.rbpgKryptonGeneralFileTriple.MaximumSize = GroupItemSize.Medium;
            this.rbpgKryptonGeneralFileTriple.MinimumSize = GroupItemSize.Medium;

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

            //
            // rbpgKryptonProjectsTriple
            //
            this.rbpgKryptonProjectsTriple.MaximumSize = GroupItemSize.Medium;
            this.rbpgKryptonProjectsTriple.MinimumSize = GroupItemSize.Medium;

            //
            // btnKryptonOpenProject
            //
            this.btnKryptonOpenProject.TextLine1 = "Open Project";
            this.btnKryptonOpenProject.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonOpenProject.Click += new System.EventHandler(this.btnKryptonOpenProject_Click);

            //
            // btnKryptonCloseProject
            //
            this.btnKryptonCloseProject.TextLine1 = "Close Project";
            this.btnKryptonCloseProject.ImageSmall = GetResourceImage("vagedc.ico");
            this.btnKryptonCloseProject.Click += new System.EventHandler(this.btnKryptonCloseProject_Click);

            //
            // btnKryptonCreateProject
            //
            this.btnKryptonCreateProject.TextLine1 = "Create Project";
            this.btnKryptonCreateProject.ImageSmall = GetResourceImage("vagedc.ico");
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

            //
            // rbpgKryptonGeneralActionsTriple
            //
            this.rbpgKryptonGeneralActionsTriple.MaximumSize = GroupItemSize.Medium;
            this.rbpgKryptonGeneralActionsTriple.MinimumSize = GroupItemSize.Medium;

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

            ((System.ComponentModel.ISupportInitialize)(this.kryptonRibbon1)).EndInit();

            // Add to form controls and set docking at the VERY END
            this.Controls.Add(this.kryptonRibbon1);
            this.kryptonRibbon1.Dock = DockStyle.Top;
            this.kryptonRibbon1.BringToFront();

            // Ensure DevExpress dock panels don't push the ribbon aside
            foreach (Control c in this.Controls)
            {
                if (c != null && c.GetType().FullName.Contains("DevExpress.XtraBars.Docking.DockPanel"))
                {
                    c.SendToBack();
                }
            }
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
                    ApplyVAGEDCDarkTheme();
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