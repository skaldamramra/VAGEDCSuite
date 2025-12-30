using System;
using System.IO;

namespace VAGSuite.Services
{
    public class LayoutService
    {
        private AppSettings _appSettings;

        public LayoutService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Gets the application data path for storing layout files
        /// </summary>
        public string GetAppDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        /// <summary>
        /// Loads layout settings from XML file
        /// </summary>
        public void LoadLayoutFiles(string layoutFileName, Action restoreLayout)
        {
            try
            {
                string layoutPath = Path.Combine(GetAppDataPath(), layoutFileName);
                if (File.Exists(layoutPath))
                {
                    restoreLayout();
                }
            }
            catch (Exception E1)
            {
                Console.WriteLine("LoadLayoutFiles error: " + E1.Message);
            }
        }

        /// <summary>
        /// Saves layout settings to XML file
        /// </summary>
        public void SaveLayoutFiles(string layoutFileName, Action saveLayout)
        {
            try
            {
                _appSettings.SymbolDockWidth = Math.Max(2, _appSettings.SymbolDockWidth);
                saveLayout();
            }
            catch (Exception E)
            {
                Console.WriteLine("SaveLayoutFiles error: " + E.Message);
            }
        }

        /// <summary>
        /// Gets the symbol dock width setting
        /// </summary>
        public int GetSymbolDockWidth()
        {
            return _appSettings.SymbolDockWidth;
        }

        /// <summary>
        /// Sets the symbol dock width setting
        /// </summary>
        public void SetSymbolDockWidth(int width)
        {
            _appSettings.SymbolDockWidth = width;
        }
    }
}