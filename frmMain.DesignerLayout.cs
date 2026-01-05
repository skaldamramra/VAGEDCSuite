using System.Drawing;
using System.Windows.Forms;
 
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
            // Krypton Integration: Enable status strip merging into the form chrome
            this.AllowStatusStripMerge = true;

            // Form settings
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(
                LayoutConstants.DefaultFormWidth,
                LayoutConstants.DefaultFormHeight);
            
            // Load icon directly from file to support multi-resolution .ico
            if (System.IO.File.Exists("vagedc.ico"))
            {
                this.Icon = new Icon("vagedc.ico");
            }
            
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VAGEDCSuite";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            // Add controls to form
            if (this.kryptonRibbon1 != null) this.Controls.Add(this.kryptonRibbon1);
            if (this.kryptonStatusStrip1 != null) this.Controls.Add(this.kryptonStatusStrip1);

            // Add Krypton docking panel
            if (this.kryptonDockingPanel != null && !this.Controls.Contains(this.kryptonDockingPanel))
            {
                this.Controls.Add(this.kryptonDockingPanel);
                this.kryptonDockingPanel.Dock = DockStyle.Fill;
            }

            // Enforce Z-Order for correct Docking behavior.
            if (this.kryptonRibbon1 != null) this.kryptonRibbon1.SendToBack();
            if (this.kryptonStatusStrip1 != null) this.kryptonStatusStrip1.SendToBack();
            if (this.kryptonDockingPanel != null) this.kryptonDockingPanel.BringToFront();
            
            // Form event handlers
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
        }

        /// <summary>
        /// Obsolete DevExpress initialization.
        /// </summary>
        private void ConfigureRibbonPages() { }
    }
}