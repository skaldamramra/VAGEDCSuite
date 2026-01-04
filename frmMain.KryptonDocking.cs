using System;
using System.Drawing;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Navigator;
using ComponentFactory.Krypton.Workspace;
using ComponentFactory.Krypton.Docking;
using VAGSuite.Theming;

namespace VAGSuite
{
    partial class frmMain
    {
        private KryptonDockingManager kryptonDockingManager1;
        private KryptonDockableWorkspace kryptonDockableWorkspace1;
        private DragManager dragManager1;
        private KryptonPanel kryptonDockingPanel;
        private KryptonPage pageSymbols;

        private void InitializeKryptonDocking()
        {
            this.kryptonDockingManager1 = new KryptonDockingManager();
            this.kryptonDockableWorkspace1 = new KryptonDockableWorkspace();
            this.dragManager1 = new DragManager();
            this.kryptonDockingPanel = new KryptonPanel();
            
            // Configure the central workspace
            this.kryptonDockableWorkspace1.Dock = DockStyle.Fill;
            this.kryptonDockableWorkspace1.Name = "kryptonDockableWorkspace1";
            this.kryptonDockableWorkspace1.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            this.kryptonDockableWorkspace1.PaletteMode = PaletteMode.Custom;
            this.kryptonDockableWorkspace1.ShowMaximizeButton = true;
            this.kryptonDockableWorkspace1.AllowPageDrag = true;
            
            // Enable tab dragging and docking features
            this.kryptonDockableWorkspace1.WorkspaceCellAdding += (s, e) => {
                e.Cell.Button.CloseButtonAction = CloseButtonAction.RemovePageAndDispose;
                e.Cell.Button.CloseButtonDisplay = ButtonDisplay.ShowEnabled;
                e.Cell.AllowTabFocus = true;
                // Use a mode that provides a clear header/handle
                e.Cell.NavigatorMode = NavigatorMode.HeaderGroupTab;
                e.Cell.Header.HeaderStylePrimary = HeaderStyle.DockActive;
                
                // Ensure the cell is not forced to fill if we want to see multiple
                e.Cell.MinimumSize = new Size(200, 200);
            };

            // Configure the docking panel (container for everything below the ribbon)
            this.kryptonDockingPanel.Controls.Add(this.kryptonDockableWorkspace1);
            this.kryptonDockingPanel.Dock = DockStyle.Fill;
            this.kryptonDockingPanel.Name = "kryptonDockingPanel";
            this.kryptonDockingPanel.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            this.kryptonDockingPanel.PaletteMode = PaletteMode.Custom;

            // Create the Symbols Page
            pageSymbols = new KryptonPage();
            pageSymbols.Text = "Symbols";
            pageSymbols.TextTitle = "Symbols";
            pageSymbols.UniqueName = "SymbolsPage";
            pageSymbols.ImageSmall = GetResourceImage("vagedc.ico");
            
            // Note: The new AdvancedDataGridView (adgvSymbols) is added to this page
            // during InitializeSymbolGrid() in frmMain.SymbolList.cs

            // CRITICAL: Add the docking panel AFTER Ribbon and StatusStrip so it's behind them in Z-order
            // The Ribbon should already be in Controls (added by InitializeKryptonRibbon)
            // The StatusStrip should already be in Controls (added by InitializeKryptonStatusBar)
            // We add the docking panel last so it goes behind them
            this.Controls.Add(this.kryptonDockingPanel);
            
            // Ensure Z-order: DockingPanel at back, Ribbon at top, Status at bottom
            // In WinForms, controls added later are on top, so we need to send the panel to back
            this.kryptonDockingPanel.SendToBack();
            
            // Ensure the workspace doesn't force a maximized layout that hides headers
            this.kryptonDockableWorkspace1.ShowMaximizeButton = true;
            this.kryptonDockableWorkspace1.AllowPageDrag = true;
        }

        private void SetupDockingHierarchy()
        {
            // As per docking.pdf: "You should always wait until the load event occurs before executing any docking code."
            
            
            // Link the workspace to the docking manager for dragging support
            // (Derived from page_dragging.pdf p. 72)
            this.dragManager1.DragTargetProviders.Add(this.kryptonDockableWorkspace1);
            this.kryptonDockableWorkspace1.DragPageNotify = this.dragManager1;

            // 1. Manage the central workspace
            KryptonDockingWorkspace w = kryptonDockingManager1.ManageWorkspace("Workspace", kryptonDockableWorkspace1);
            
            // 2. Manage the panel for docked/auto-hidden content
            kryptonDockingManager1.ManageControl("Control", kryptonDockingPanel, w);
            
            // 3. Manage floating windows
            kryptonDockingManager1.ManageFloating("Floating", this);

            // 4. Add the Symbols page to the left side (replacing dockSymbols)
            kryptonDockingManager1.AddDockspace("Control", DockingEdge.Left, new KryptonPage[] { pageSymbols });
        }

        /// <summary>
        /// Helper to add a control to the docking system.
        /// </summary>
        public void AddControlToDocking(Control control, string title, string uniqueName, DockingEdge edge = DockingEdge.Right)
        {
            KryptonPage page = new KryptonPage();
            page.Text = title;
            page.TextTitle = title;
            page.UniqueName = uniqueName + Guid.NewGuid().ToString("N"); // Ensure uniqueness
            page.ImageSmall = GetResourceImage("vagedc.ico");
            
            // Set flags to ensure docking behavior
            page.Flags = (int)(KryptonPageFlags.DockingAllowDocked |
                              KryptonPageFlags.DockingAllowFloating |
                              KryptonPageFlags.DockingAllowAutoHidden |
                              KryptonPageFlags.DockingAllowClose);
            
            control.Dock = DockStyle.Fill;
            page.Controls.Add(control);

            // If it's a map viewer or hex viewer, we usually want it in the central workspace
            if (uniqueName.Contains("Viewer") || uniqueName.Contains("Hex"))
            {
                kryptonDockingManager1.AddToWorkspace("Workspace", new KryptonPage[] { page });
            }
            else
            {
                kryptonDockingManager1.AddDockspace("Control", edge, new KryptonPage[] { page });
            }
        }
    }
}