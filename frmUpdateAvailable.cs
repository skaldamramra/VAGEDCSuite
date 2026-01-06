using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using System.IO;


namespace VAGSuite
{
    public partial class frmUpdateAvailable : KryptonForm
    {
        public frmUpdateAvailable()
        {
            InitializeComponent();
            VAGSuite.Theming.VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public void SetVersionNumber(string version)
        {
            labelControl2.Text = "Available version: " + version;
        }

        
        private void AddDebugLog(string line)
        {
            Console.WriteLine(line);
            if (Directory.Exists("C:\\debug"))
            {
                using (StreamWriter sw = new StreamWriter("c:\\debug\\vagedcsuite.log", true))
                {
                    sw.WriteLine(line);
                }
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            AddDebugLog("Opening GitHub releases page");
            System.Diagnostics.Process.Start("https://github.com/skaldamramra/VAGEDCSuite/releases");
        }


        private void frmUpdateAvailable_Load(object sender, EventArgs e)
        {

        }
    }
}