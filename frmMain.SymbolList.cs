using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Services;
using VAGSuite.Theming;

namespace VAGSuite
{
    public partial class frmMain
    {
        private KryptonTreeView tvSymbols;

        private void InitializeSymbolGrid()
        {
            Console.WriteLine("🧑🔬 [DEBUG] InitializeSymbolGrid: Starting KryptonTreeView initialization...");
            this.tvSymbols = new KryptonTreeView();
            this.tvSymbols.Dock = DockStyle.Fill;
            
            // Apply VAGEDC Dark Theme to TreeView
            this.tvSymbols.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            this.tvSymbols.PaletteMode = PaletteMode.Custom;
            this.tvSymbols.StateCommon.Back.Color1 = Color.FromArgb(30, 30, 30);
            this.tvSymbols.StateCommon.Node.Content.ShortText.Color1 = Color.FromArgb(220, 220, 220);
            this.tvSymbols.StateCommon.Node.Content.ShortText.Color2 = Color.FromArgb(220, 220, 220);
            
            // Events
            this.tvSymbols.NodeMouseDoubleClick += tvSymbols_NodeMouseDoubleClick;
            this.tvSymbols.KeyDown += tvSymbols_KeyDown;
            this.tvSymbols.MouseMove += tvSymbols_MouseMove;
            this.tvSymbols.MouseLeave += tvSymbols_MouseLeave;
            this.hoverTimer = new Timer();
            this.hoverTimer.Interval = 300;
            this.hoverTimer.Tick += hoverTimer_Tick;

            // Tracking timer polls the cursor position so tooltips can be shown even when the form is not active.
            this.trackingTimer = new Timer();
            this.trackingTimer.Interval = 100;
            this.trackingTimer.Tick += trackingTimer_Tick;
            this.trackingTimer.Start();
            
            Console.WriteLine("🧑🔬 [DEBUG] InitializeSymbolGrid: Adding TreeView to pageSymbols container.");
            this.pageSymbols.Controls.Clear();
            this.pageSymbols.Controls.Add(this.tvSymbols);
            this.tvSymbols.BringToFront();
        }

        /// <summary>
        /// Suspends the tracking timer to improve performance during docking/floating operations.
        /// Call this before starting any operation that may cause layout recalculations.
        /// </summary>
        public void SuspendTrackingTimer()
        {
            if (trackingTimer != null && trackingTimer.Enabled)
            {
                trackingTimer.Stop();
            }
        }

        /// <summary>
        /// Resumes the tracking timer after docking/floating operations complete.
        /// </summary>
        public void ResumeTrackingTimer()
        {
            if (trackingTimer != null && !trackingTimer.Enabled)
            {
                trackingTimer.Start();
            }
        }

        public void UpdateSymbolList(System.Collections.IEnumerable symbols)
        {
            Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: Building hierarchical tree...");
            if (symbols == null)
            {
                Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: symbols collection is null.");
                return;
            }

            if (tvSymbols == null)
            {
                Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: tvSymbols is null. Initializing now...");
                InitializeSymbolGrid();
            }

            try
            {
                // Suspend tracking timer during tree update to avoid interference
                SuspendTrackingTimer();

                tvSymbols.BeginUpdate();
                Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: Clearing existing nodes...");
                tvSymbols.Nodes.Clear();

                // Pre-compute format string outside the loop for better performance
                string addressFormat = m_appSettings.ShowAddressesInHex ? "X6" : string.Empty;

                // First pass: collect all symbols grouped by category and subcategory
                var categorySubcatMap = new Dictionary<string, Dictionary<string, List<SymbolHelper>>>();
                int symbolCount = 0;

                foreach (SymbolHelper sh in symbols)
                {
                    symbolCount++;
                    try
                    {
                        string catName = string.IsNullOrEmpty(sh.Category) ? "Undocumented" : sh.Category;
                        string subCatName = string.IsNullOrEmpty(sh.Subcategory) ? "General" : sh.Subcategory;

                        Dictionary<string, List<SymbolHelper>> subcatMap;
                        if (!categorySubcatMap.TryGetValue(catName, out subcatMap))
                        {
                            subcatMap = new Dictionary<string, List<SymbolHelper>>();
                            categorySubcatMap[catName] = subcatMap;
                        }

                        List<SymbolHelper> symbolList;
                        if (!subcatMap.TryGetValue(subCatName, out symbolList))
                        {
                            symbolList = new List<SymbolHelper>();
                            subcatMap[subCatName] = symbolList;
                        }

                        symbolList.Add(sh);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("🧑🔬 [DEBUG] UpdateSymbolList: Error processing symbol {0}: {1}", symbolCount, ex.Message));
                    }
                }

                // Sort categories: "Detected maps" first, then others alphabetically
                var sortedCategories = new List<string>(categorySubcatMap.Keys);
                sortedCategories.Sort((a, b) =>
                {
                    if (a == "Detected maps") return -1;
                    if (b == "Detected maps") return 1;
                    return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
                });

                // Pre-create bold font once
                Font boldFont = new Font(tvSymbols.Font, FontStyle.Bold);

                // Second pass: build tree nodes in sorted category order
                foreach (string catName in sortedCategories)
                {
                    var catNode = new TreeNode(catName);
                    catNode.NodeFont = boldFont;
                    tvSymbols.Nodes.Add(catNode);

                    // Sort subcategories alphabetically
                    var sortedSubcats = new List<string>(categorySubcatMap[catName].Keys);
                    sortedSubcats.Sort(StringComparer.OrdinalIgnoreCase);

                    foreach (string subCatName in sortedSubcats)
                    {
                        var subNode = new TreeNode(subCatName);
                        catNode.Nodes.Add(subNode);

                        foreach (SymbolHelper sh in categorySubcatMap[catName][subCatName])
                        {
                            // Use cached format string for address formatting
                            string addressString = !string.IsNullOrEmpty(addressFormat)
                                ? sh.Flash_start_address.ToString(addressFormat)
                                : sh.Flash_start_address.ToString();
                            var symbolNode = new TreeNode(sh.Varname + " [" + addressString + "]");
                            symbolNode.Tag = sh;
                            subNode.Nodes.Add(symbolNode);
                        }
                    }
                }

                Console.WriteLine(string.Format("🧑🔬 [DEBUG] UpdateSymbolList: Processed {0} symbols. Expanding tree...", symbolCount));
                
                // OPTIMIZATION: Only expand categories, not all subcategories
                // This significantly reduces layout calculations while still showing structure
                foreach (TreeNode catNode in tvSymbols.Nodes)
                {
                    catNode.Expand();
                }
                
                Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: Ending update...");
                tvSymbols.EndUpdate();

                // Resume tracking timer after tree update is complete
                ResumeTrackingTimer();
                
                Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: Tree built successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: CRITICAL ERROR: " + ex.Message);
                Console.WriteLine("🧑🔬 [DEBUG] StackTrace: " + ex.StackTrace);
                
                // Ensure timer is resumed even on error
                ResumeTrackingTimer();
            }
        }

        private void OpenSelectedSymbol()
        {
            if (tvSymbols.SelectedNode != null && tvSymbols.SelectedNode.Tag is SymbolHelper)
            {
                SymbolHelper sh = (SymbolHelper)tvSymbols.SelectedNode.Tag;
                StartTableViewer(sh.Varname, sh.CodeBlock);
            }
        }

        private void tvSymbols_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            OpenSelectedSymbol();
        }

        private void tvSymbols_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OpenSelectedSymbol();
                e.Handled = true;
            }
        }

        private Timer hoverTimer;
        private Timer trackingTimer;
        private Point lastMousePos;
        private TreeNode activeNode;
        private bool isTooltipActive;

        private void tvSymbols_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_appSettings == null || !m_appSettings.ShowMapDescriptions) return;

            // Update last known mouse position for possible UI-driven updates.
            lastMousePos = e.Location;

            // If the tooltip is currently active for the node, update its screen position so it "travels".
            if (isTooltipActive && activeNode != null && activeNode.Tag is SymbolHelper sh)
            {
                string description = VAGSuite.Services.MapDescriptionService.Instance.GetDescription(sh.Varname);
                if (!string.IsNullOrEmpty(description))
                {
                    // Use the WinForms-based TooltipService to update the shown tooltip position/text.
                    // ShowForControl expects a client coordinate relative to the owner control; we have e.Location.
                    TooltipService.ShowForControl(tvSymbols, e.Location, sh.Varname, description, "MapDescription");
                }
            }
        }

        private void tvSymbols_MouseLeave(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            TooltipService.Hide("MapDescription");
            activeNode = null;
            isTooltipActive = false;
        }

        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            if (m_appSettings == null || !m_appSettings.ShowMapDescriptions) return;
    
            if (activeNode != null && activeNode.Tag is SymbolHelper sh)
            {
                string description = VAGSuite.Services.MapDescriptionService.Instance.GetDescription(sh.Varname);
                if (!string.IsNullOrEmpty(description))
                {
                    isTooltipActive = true;
                    // Use TooltipService (WinForms.ToolTip wrapper) to show the hint.
                    TooltipService.ShowForControl(tvSymbols, tvSymbols.PointToClient(System.Windows.Forms.Cursor.Position), sh.Varname, description, "MapDescription");
                }
            }
        }

        /// <summary>
        /// Polls the cursor position and updates activeNode even when the form is not active.
        /// This allows the hover timer to start while the window does not have input focus.
        /// </summary>
        private void trackingTimer_Tick(object sender, EventArgs e)
        {
            if (m_appSettings == null || !m_appSettings.ShowMapDescriptions)
            {
                // Ensure we hide any active tooltip when descriptions are disabled.
                hoverTimer.Stop();
                TooltipService.Hide("MapDescription");
                activeNode = null;
                isTooltipActive = false;
                return;
            }

            if (tvSymbols == null || tvSymbols.IsDisposed) return;

            // Determine if the cursor is within the TreeView's screen rectangle.
            Rectangle tvScreenRect = tvSymbols.RectangleToScreen(tvSymbols.ClientRectangle);
            Point cursorPos = System.Windows.Forms.Cursor.Position;

            if (!tvScreenRect.Contains(cursorPos))
            {
                // Cursor outside treeview: hide tooltip if necessary.
                if (activeNode != null || isTooltipActive)
                {
                    activeNode = null;
                    isTooltipActive = false;
                    hoverTimer.Stop();
                    TooltipService.Hide("MapDescription");
                }
                return;
            }

            // Convert to control client coordinates and perform hit test.
            Point clientPt = tvSymbols.PointToClient(cursorPos);
            TreeViewHitTestInfo hit = tvSymbols.HitTest(clientPt);
            TreeNode node = null;
            if (hit.Node != null)
            {
                if ((hit.Location & (TreeViewHitTestLocations.Label |
                                      TreeViewHitTestLocations.Image |
                                      TreeViewHitTestLocations.StateImage |
                                      TreeViewHitTestLocations.RightOfLabel)) != 0)
                {
                    if (hit.Node.Tag is SymbolHelper) node = hit.Node;
                }
            }

            if (node != activeNode)
            {
                // Node changed: reset tooltip state and start hover timer for the new node.
                activeNode = node;
                isTooltipActive = false;
                try { TooltipService.Hide("MapDescription"); } catch { }
                hoverTimer.Stop();
    
                if (node != null)
                {
                    hoverTimer.Start();
                }
            }
        }
    }
}