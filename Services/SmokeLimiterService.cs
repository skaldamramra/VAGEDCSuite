using System;
using System.Collections.Generic;
using System.IO;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    public class SmokeLimiterService
    {
        private readonly AppSettings _appSettings;

        public SmokeLimiterService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Activates smoke limiters by modifying the smoke limiter map.
        /// Returns true if activation was successful.
        /// </summary>
        public bool ActivateSmokeLimiters(string currentFile, SymbolCollection symbols)
        {
            if (string.IsNullOrEmpty(currentFile) || !File.Exists(currentFile))
            {
                return false;
            }

            // Find the smoke limiter control map (selector)
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname.StartsWith("Smoke limiter"))
                {
                    byte[] mapdata = new byte[sh.Length];
                    mapdata.Initialize();
                    mapdata = Tools.Instance.readdatafromfile(currentFile, (int)sh.Flash_start_address, sh.Length, Tools.Instance.m_currentFileType);

                    int selectorAddress = sh.MapSelector.StartAddress;
                    if (selectorAddress > 0)
                    {
                        byte[] mapIndexes = new byte[sh.MapSelector.MapIndexes.Length * 2];
                        int bIdx = 0;
                        for (int i = 0; i < sh.MapSelector.MapIndexes.Length; i++)
                        {
                            mapIndexes[bIdx++] = Convert.ToByte(i);
                            mapIndexes[bIdx++] = 0;
                        }
                        Tools.Instance.savedatatobinary(selectorAddress + mapIndexes.Length, mapIndexes.Length, mapIndexes, currentFile, false, Tools.Instance.m_currentFileType);
                    }
                    for (int i = 1; i < sh.MapSelector.MapIndexes.Length; i++)
                    {
                        // save the map data (copy)
                        int saveAddress = (int)sh.Flash_start_address + i * sh.Length;
                        Tools.Instance.savedatatobinary(saveAddress, sh.Length, mapdata, currentFile, false, Tools.Instance.m_currentFileType);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if smoke limiter is missing from the symbols collection.
        /// </summary>
        public bool IsSmokeLimiterMissing(SymbolCollection symbols)
        {
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname.StartsWith("Smoke limiter"))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the number of smoke limiter maps.
        /// </summary>
        public int GetSmokeLimiterCount(SymbolCollection symbols)
        {
            int count = 0;
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname.StartsWith("Smoke limiter"))
                {
                    count++;
                }
            }
            return count;
        }
    }
}