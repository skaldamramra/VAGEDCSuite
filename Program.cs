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
            
            // Initialize VAGEDC Dark Theme for Krypton
            var themeManager = VAGEDCThemeManager.Instance;
            
            // Activate the dark theme before applying to forms
            // This sets _isCustomThemeActive = true so ApplyThemeToForm works
            frmMain mainForm = new frmMain();
            themeManager.ActivateVAGEDCDark(mainForm);
            
            Application.Run(mainForm);
        }
    }
}
