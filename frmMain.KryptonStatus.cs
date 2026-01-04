using System;
using System.Drawing;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace VAGSuite
{
    public partial class frmMain
    {
        // Field kryptonStatusStrip1 is already defined in frmMain.Designer.cs as System.Windows.Forms.StatusStrip
        private ToolStripStatusLabel statusFilename;
        private ToolStripStatusLabel statusChecksum;
        private ToolStripStatusLabel statusPartNumber;
        private ToolStripStatusLabel statusSymCount;
        private ToolStripStatusLabel statusReadOnly;
        private ToolStripStatusLabel statusCodeBlocks;
        private ToolStripStatusLabel statusUpdate;
        private ToolStripStatusLabel statusSpring;
        private ToolStripButton statusMapDescriptions;

        private void InitializeKryptonStatusBar()
        {
            // Use existing field from designer if available
            if (this.kryptonStatusStrip1 == null)
            {
                this.kryptonStatusStrip1 = new System.Windows.Forms.StatusStrip();
            }
            
            if (!this.Controls.Contains(this.kryptonStatusStrip1))
            {
                this.Controls.Add(this.kryptonStatusStrip1);
            }
            
            // Ensure it's at the bottom and visible
            // CRITICAL: Do NOT call BringToFront() here - let the initialization order handle Z-order
            this.kryptonStatusStrip1.Visible = true;
            this.kryptonStatusStrip1.Dock = DockStyle.Bottom;

            this.statusFilename = new ToolStripStatusLabel();
            this.statusChecksum = new ToolStripStatusLabel();
            this.statusPartNumber = new ToolStripStatusLabel();
            this.statusSymCount = new ToolStripStatusLabel();
            this.statusReadOnly = new ToolStripStatusLabel();
            this.statusCodeBlocks = new ToolStripStatusLabel();
            this.statusUpdate = new ToolStripStatusLabel();
            this.statusSpring = new ToolStripStatusLabel();
            this.statusMapDescriptions = new ToolStripButton();

            Color textColor = Color.FromArgb(220, 220, 220);

            // 
            // kryptonStatusStrip1
            // 
            this.kryptonStatusStrip1.Font = new Font("Segoe UI", 9F);
            this.kryptonStatusStrip1.Items.Clear();
            this.kryptonStatusStrip1.Items.AddRange(new ToolStripItem[] {
                this.statusFilename,
                this.statusChecksum,
                this.statusPartNumber,
                this.statusSymCount,
                this.statusReadOnly,
                this.statusCodeBlocks,
                this.statusUpdate,
                this.statusSpring,
                this.statusMapDescriptions
            });
            this.kryptonStatusStrip1.Dock = DockStyle.Bottom;
            this.kryptonStatusStrip1.Name = "kryptonStatusStrip1";
            this.kryptonStatusStrip1.TabIndex = 5;
            this.kryptonStatusStrip1.Text = "kryptonStatusStrip1";
            
            // Theming: Use ManagerRenderMode to allow KryptonManager/Palette to control rendering.
            // This is required for AllowStatusStripMerge to work correctly on KryptonForm.
            this.kryptonStatusStrip1.RenderMode = ToolStripRenderMode.ManagerRenderMode;
            this.kryptonStatusStrip1.BackColor = Color.FromArgb(30, 30, 30);
            this.kryptonStatusStrip1.ForeColor = textColor;
            this.kryptonStatusStrip1.SizingGrip = false;

            // Force Krypton Manager to apply toolstrip rendering
            if (this.kryptonManager1 != null)
            {
                this.kryptonManager1.GlobalApplyToolstrips = true;
            }

            // Diagnostic logging
            Console.WriteLine("DEBUG: InitializeKryptonStatusBar called");
            Console.WriteLine("DEBUG: StatusStrip RenderMode: " + this.kryptonStatusStrip1.RenderMode);
            Console.WriteLine("DEBUG: StatusStrip BackColor: " + this.kryptonStatusStrip1.BackColor);
            Console.WriteLine("DEBUG: StatusStrip Parent: " + (this.kryptonStatusStrip1.Parent != null ? this.kryptonStatusStrip1.Parent.GetType().Name : "NULL"));
            Console.WriteLine("DEBUG: StatusStrip Item Count: " + this.kryptonStatusStrip1.Items.Count);

            // 
            // statusFilename
            // 
            this.statusFilename.Name = "statusFilename";
            this.statusFilename.ForeColor = textColor;
            this.statusFilename.Text = "Ready";
            this.statusFilename.Spring = true;
            this.statusFilename.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // statusChecksum
            // 
            this.statusChecksum.Name = "statusChecksum";
            this.statusChecksum.ForeColor = textColor;
            this.statusChecksum.Text = "Checksum: N/A";
            this.statusChecksum.BorderSides = ToolStripStatusLabelBorderSides.Left;

            // 
            // statusPartNumber
            // 
            this.statusPartNumber.Name = "statusPartNumber";
            this.statusPartNumber.ForeColor = textColor;
            this.statusPartNumber.Text = "Part: N/A";
            this.statusPartNumber.BorderSides = ToolStripStatusLabelBorderSides.Left;

            // 
            // statusSymCount
            // 
            this.statusSymCount.Name = "statusSymCount";
            this.statusSymCount.ForeColor = textColor;
            this.statusSymCount.Text = "Symbols: 0";
            this.statusSymCount.BorderSides = ToolStripStatusLabelBorderSides.Left;

            // 
            // statusReadOnly
            // 
            this.statusReadOnly.Name = "statusReadOnly";
            this.statusReadOnly.ForeColor = textColor;
            this.statusReadOnly.Text = "Read-Write";
            this.statusReadOnly.BorderSides = ToolStripStatusLabelBorderSides.Left;

            // 
            // statusCodeBlocks
            // 
            this.statusCodeBlocks.Name = "statusCodeBlocks";
            this.statusCodeBlocks.ForeColor = textColor;
            this.statusCodeBlocks.Text = "Blocks: 0";
            this.statusCodeBlocks.BorderSides = ToolStripStatusLabelBorderSides.Left;

            //
            // statusUpdate
            //
            this.statusUpdate.Name = "statusUpdate";
            this.statusUpdate.ForeColor = Color.Yellow;

            //
            // statusSpring
            //
            this.statusSpring.Name = "statusSpring";
            this.statusSpring.Spring = true;
            this.statusSpring.Text = "";

            //
            // statusMapDescriptions
            //
            this.statusMapDescriptions.Name = "statusMapDescriptions";
            this.statusMapDescriptions.ForeColor = textColor;
            this.statusMapDescriptions.Text = "Map Descriptions";
            this.statusMapDescriptions.DisplayStyle = ToolStripItemDisplayStyle.Text;
            this.statusMapDescriptions.CheckOnClick = true;
            this.statusMapDescriptions.Click += new System.EventHandler(this.statusMapDescriptions_Click);
        }

        private void statusMapDescriptions_Click(object sender, EventArgs e)
        {
            Console.WriteLine("DEBUG: statusMapDescriptions_Click triggered. Checked: " + this.statusMapDescriptions.Checked);
            
            // Shim to the original DevExpress handler logic
            // We call the existing method in frmMain.cs
            btnToggleMapDescriptions_ItemClick(this.btnToggleMapDescriptions, null);
            
            // Ensure the Krypton UI reflects the new state
            UpdateMapDescriptionsButtonAppearance();
        }
    }
}