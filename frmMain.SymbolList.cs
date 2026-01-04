using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Services;

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

        public void UpdateSymbolList(System.Collections.IEnumerable symbols)
        {
            Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: Building hierarchical tree...");
            if (symbols == null) return;

            tvSymbols.BeginUpdate();
            tvSymbols.Nodes.Clear();

            var categories = new Dictionary<string, TreeNode>();

            foreach (SymbolHelper sh in symbols)
            {
                string catName = string.IsNullOrEmpty(sh.Category) ? "Undocumented" : sh.Category;
                string subCatName = string.IsNullOrEmpty(sh.Subcategory) ? "General" : sh.Subcategory;

                if (!categories.ContainsKey(catName))
                {
                    var catNode = new TreeNode(catName);
                    catNode.NodeFont = new Font(tvSymbols.Font, FontStyle.Bold);
                    tvSymbols.Nodes.Add(catNode);
                    categories.Add(catName, catNode);
                }

                TreeNode parentNode = categories[catName];
                TreeNode subNode = null;

                foreach (TreeNode node in parentNode.Nodes)
                {
                    if (node.Text == subCatName)
                    {
                        subNode = node;
                        break;
                    }
                }

                if (subNode == null)
                {
                    subNode = new TreeNode(subCatName);
                    parentNode.Nodes.Add(subNode);
                }

                string addressString = m_appSettings.ShowAddressesInHex ? sh.Flash_start_address.ToString("X6") : sh.Flash_start_address.ToString();
                var symbolNode = new TreeNode(string.Format("{0} [{1}]", sh.Varname, addressString));
                symbolNode.Tag = sh;
                subNode.Nodes.Add(symbolNode);
            }

            tvSymbols.ExpandAll();
            tvSymbols.EndUpdate();
            Console.WriteLine("🧑🔬 [DEBUG] UpdateSymbolList: Tree built successfully.");
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
                    TooltipService.ShowForControl(tvSymbols, e.Location, sh.Varname, description);
                }
            }
        }

        private void tvSymbols_MouseLeave(object sender, EventArgs e)
        {
            hoverTimer.Stop();
            TooltipService.Hide();
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
                    TooltipService.ShowForControl(tvSymbols, tvSymbols.PointToClient(System.Windows.Forms.Cursor.Position), sh.Varname, description);
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
                TooltipService.Hide();
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
                    TooltipService.Hide();
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
                try { TooltipService.Hide(); } catch { }
                hoverTimer.Stop();
    
                if (node != null)
                {
                    hoverTimer.Start();
                }
            }
        }
    }
}