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
            // Wipe logs at start as requested
            try
            {
                if (System.IO.File.Exists("debug_output.log")) System.IO.File.Delete("debug_output.log");
                if (System.IO.File.Exists("debug_error.log")) System.IO.File.Delete("debug_error.log");
            }
            catch { }

            // Redirect Debug and Console to files for better troubleshooting
            // Use a shared stream to avoid locking issues if multiple listeners are added
            try
            {
                System.Diagnostics.TextWriterTraceListener debugListener = new System.Diagnostics.TextWriterTraceListener("debug_output.log");
                System.Diagnostics.Debug.Listeners.Add(debugListener);
                System.Diagnostics.Debug.AutoFlush = true;
                
                // Also redirect Console.Out for legacy code
                System.IO.StreamWriter sw = new System.IO.StreamWriter("debug_output.log", true);
                sw.AutoFlush = true;
                Console.SetOut(sw);
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Initialize VAGEDC Dark Theme for Krypton
            var themeManager = VAGEDCThemeManager.Instance;
            
            // Activate the dark theme before applying to forms
            // This sets _isCustomThemeActive = true so ApplyThemeToForm works
            frmMain mainForm = new frmMain();
            themeManager.ActivateVAGEDCDark(mainForm);
            
            try
            {
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText("debug_error.log", ex.ToString()); } catch { }
                throw;
            }
        }
    }
}
