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
            DevExpress.UserSkins.OfficeSkins.Register();
            DevExpress.UserSkins.BonusSkins.Register();
            
            // Initialize VAGEDC Dark Theme for Krypton
            var themeManager = VAGEDCThemeManager.Instance;
            
            frmMain mainForm = new frmMain();
            themeManager.ApplyThemeToForm(mainForm);
            
            Application.Run(mainForm);
        }
    }
}
