using System;
using System.Collections.Generic;
using System.IO;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    public class LaunchControlService
    {
        private readonly AppSettings _appSettings;

        public LaunchControlService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Activates launch control by modifying the launch control map.
        /// Returns true if activation was successful.
        /// </summary>
        public bool ActivateLaunchControl(string currentFile, SymbolCollection symbols)
        {
            if (string.IsNullOrEmpty(currentFile) || !File.Exists(currentFile))
            {
                return false;
            }

            byte[] allBytes = File.ReadAllBytes(currentFile);
            bool found = true;
            int offset = 0;

            while (found)
            {
                // Search for the launch control sequence pattern
                int LCAddress = Tools.Instance.findSequence(allBytes, offset,
                    new byte[16] { 0xFF, 0xFF, 0x02, 0x00, 0x80, 0x00, 0x00, 0x0A, 0xFF, 0xFF, 0x02, 0x00, 0x00, 0x00, 0x70, 0x17 },
                    new byte[16] { 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1 });

                if (LCAddress > 0)
                {
                    // Create the new launch control map values
                    byte[] saveByte = new byte[(0x0E * 2) + 2];
                    int i = 0;
                    
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(0x0E), i++);
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(0), i++); // 1st value = 0 km/h
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(20), i++); // 2nd value = 6 km/h

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(40), i++);

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(60), i++);

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(80), i++);

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(100), i++);
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(120), i++);

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(140), i++);
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(160), i++);

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(180), i++);
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(200), i++);

                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(220), i++);
                    saveByte.SetValue(Convert.ToByte(0), i++);
                    saveByte.SetValue(Convert.ToByte(240), i++);

                    saveByte.SetValue(Convert.ToByte(1), i++);
                    saveByte.SetValue(Convert.ToByte(4), i++);

                    // Save the modified data
                    Tools.Instance.savedatatobinary(LCAddress + 2, saveByte.Length, saveByte, currentFile, false, Tools.Instance.m_currentFileType);
                    
                    offset = LCAddress + 1;
                }
                else
                {
                    found = false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if launch control map is missing from the symbols collection.
        /// </summary>
        public bool IsLaunchControlMapMissing(SymbolCollection symbols)
        {
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname.StartsWith("Launch control map"))
                {
                    return false;
                }
            }
            return true;
        }
    }
}