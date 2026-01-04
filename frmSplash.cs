using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Theming;

namespace VAGSuite
{
    public partial class frmSplash : KryptonForm
    {
        private DateTime _startTime;

        public frmSplash()
        {
            InitializeComponent();
            VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
            _startTime = DateTime.Now;
            labelControl1.Text = "ver " + Application.ProductVersion.ToString();
            // Use a safe default size (9pt) for the bold font to avoid NullReferenceException
            labelControl1.StateCommon.ShortText.Font = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Bold);
            AdjustSizeToScreen();
        }

        public new void Close()
        {
            double elapsed = (DateTime.Now - _startTime).TotalMilliseconds;
            if (elapsed < 1000)
            {
                System.Threading.Thread.Sleep(1000 - (int)elapsed);
            }
            base.Close();
        }

        private void AdjustSizeToScreen()
        {
            // Get the working area of the primary screen
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            
            // If the splash is larger than 80% of the screen width or height, scale it down to 50%
            if (this.Width > workingArea.Width * 0.8 || this.Height > workingArea.Height * 0.8)
            {
                this.Width = 512;  // 1024 / 2
                this.Height = 463; // 925 / 2 (rounded up)
            }
            
            // Move label to bottom right
            labelControl1.AutoSize = false;
            labelControl1.StateCommon.ShortText.Color1 = Color.White;
            labelControl1.Size = new Size(200, 20); // Fixed size to ensure it's not 0 or too large
            labelControl1.StateCommon.ShortText.TextH = PaletteRelativeAlign.Far; // Right align text
            labelControl1.Location = new Point(this.Width - labelControl1.Width - 20, this.Height - labelControl1.Height - 20);
            labelControl1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            labelControl1.BringToFront();

            // Re-center after resize
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}