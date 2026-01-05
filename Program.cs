using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using VAGSuite.Theming;

namespace VAGSuite
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // DevExpress skins removed for Krypton migration
            
            // Initialize VAGEDC Dark Theme for Krypton
            var themeManager = VAGEDCThemeManager.Instance;
            
            frmMain mainForm = new frmMain();
            themeManager.ApplyThemeToForm(mainForm);
            
            Application.Run(mainForm);
        }
    }
}
