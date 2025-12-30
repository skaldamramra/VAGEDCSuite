using System;
using System.Collections.Generic;
using System.IO;

namespace VAGSuite.Services
{
    /// <summary>
    /// Manages all file-related operations including opening, detecting maps, and saving data.
    /// Extracted from frmMain to separate business logic from UI concerns.
    /// </summary>
    public class FileOperationsManager
    {
        private readonly AppSettings _appSettings;

        public FileOperationsManager(AppSettings appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        /// <summary>
        /// Result of opening a file operation
        /// </summary>
        public class OpenFileResult
        {
            public bool Success { get; set; }
            public string FileName { get; set; }
            public int FileLength { get; set; }
            public bool IsReadOnly { get; set; }
            public SymbolCollection Symbols { get; set; }
            public List<CodeBlock> CodeBlocks { get; set; }
            public List<AxisHelper> AxisList { get; set; }
            public string PartNumber { get; set; }
            public string AdditionalInfo { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// Opens a binary file and detects all maps within it.
        /// </summary>
        public OpenFileResult OpenFile(string fileName, bool showMessage)
        {
            var result = new OpenFileResult
            {
                FileName = fileName,
                Success = false
            };

            try
            {
                if (!File.Exists(fileName))
                {
                    result.ErrorMessage = "File does not exist";
                    return result;
                }

                FileInfo fi = new FileInfo(fileName);
                result.FileLength = (int)fi.Length;
                result.IsReadOnly = fi.IsReadOnly;

                // Store in Tools singleton for backward compatibility
                Tools.Instance.m_currentfile = fileName;
                Tools.Instance.m_currentfilelength = result.FileLength;

                // Detect maps in the file
                Tools.Instance.m_symbols = new SymbolCollection();
                Tools.Instance.codeBlockList = new List<CodeBlock>();

                List<CodeBlock> tempCodeBlocks;
                List<AxisHelper> tempAxisList;
                result.Symbols = DetectMaps(fileName, out tempCodeBlocks, out tempAxisList, showMessage, true);
                result.CodeBlocks = tempCodeBlocks;
                result.AxisList = tempAxisList;
                result.Success = true;

                // Update Tools singleton for backward compatibility
                Tools.Instance.m_symbols = result.Symbols;
                Tools.Instance.codeBlockList = result.CodeBlocks;
                Tools.Instance.AxisList = result.AxisList;

                // Store last opened file
                _appSettings.Lastfilename = fileName;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// Detects all maps/symbols in the binary file.
        /// </summary>
        public SymbolCollection DetectMaps(string filename, out List<CodeBlock> newCodeBlocks, 
            out List<AxisHelper> newAxisHelpers, bool showMessage, bool isPrimaryFile)
        {
            IEDCFileParser parser = Tools.Instance.GetParserForFile(filename, isPrimaryFile);
            newCodeBlocks = new List<CodeBlock>();
            newAxisHelpers = new List<AxisHelper>();
            SymbolCollection newSymbols = new SymbolCollection();

            if (parser != null)
            {
                byte[] allBytes = File.ReadAllBytes(filename);
                string boschnumber = parser.ExtractBoschPartnumber(allBytes);
                string softwareNumber = parser.ExtractSoftwareNumber(allBytes);
                partNumberConverter pnc = new partNumberConverter();
                ECUInfo info = pnc.ConvertPartnumber(boschnumber, allBytes.Length);

                if (!info.EcuType.StartsWith("EDC15P") && !info.EcuType.StartsWith("EDC15VM") && 
                    info.EcuType != string.Empty && showMessage)
                {
                    // Note: UI message handling should be done by caller
                    Console.WriteLine($"Warning: Non-EDC15P/VM file [{info.EcuType}] {Path.GetFileName(filename)}");
                }

                if (info.EcuType == string.Empty)
                {
                    Console.WriteLine("partnumber " + info.PartNumber + " unknown " + filename);
                }

                if (isPrimaryFile)
                {
                    string partNo = parser.ExtractPartnumber(allBytes);
                    partNo = Tools.Instance.StripNonAscii(partNo);
                    softwareNumber = Tools.Instance.StripNonAscii(softwareNumber);
                    // These will be used by UI to update status bar
                }

                newSymbols = parser.parseFile(filename, out newCodeBlocks, out newAxisHelpers);
                newSymbols.SortColumn = "Flash_start_address";
                newSymbols.SortingOrder = GenericComparer.SortOrder.Ascending;
                newSymbols.Sort();
            }

            return newSymbols;
        }

        /// <summary>
        /// Saves data to binary file with optional codeblock synchronization.
        /// </summary>
        public void SaveDataWithSync(string fileName, string varName, int address, int length, 
            byte[] data, bool useNote, string note, SymbolCollection symbols, List<CodeBlock> codeBlocks)
        {
            Tools.Instance.savedatatobinary(address, length, data, fileName, useNote, note, Tools.Instance.m_currentFileType);

            if (_appSettings.CodeBlockSyncActive)
            {
                SynchronizeCodeBlocks(fileName, varName, address, length, data, useNote, note, symbols, codeBlocks);
            }
        }

        /// <summary>
        /// Synchronizes data across multiple codeblocks (for multi-transmission ECUs).
        /// </summary>
        private void SynchronizeCodeBlocks(string fileName, string varName, int address, int length, 
            byte[] data, bool useNote, string note, SymbolCollection symbols, List<CodeBlock> codeBlocks)
        {
            if (fileName != Tools.Instance.m_currentfile)
                return;

            int codeBlockOffset = -1;

            // Find the codeblock offset for the current symbol
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Flash_start_address == address && sh.Length == length)
                {
                    if (sh.CodeBlock > 0)
                    {
                        foreach (CodeBlock cb in codeBlocks)
                        {
                            if (cb.CodeID == sh.CodeBlock)
                            {
                                codeBlockOffset = address - cb.StartAddress;
                                break;
                            }
                        }
                    }
                    break;
                }
            }

            // Synchronize to other codeblocks
            foreach (SymbolHelper sh in symbols)
            {
                bool shSaved = false;
                if (sh.Length == length)
                {
                    if (sh.Flash_start_address != address)
                    {
                        // Check if symbols have matching lower 16 bits of address
                        if ((sh.Flash_start_address & 0x0FFFF) == (address & 0x0FFFF))
                        {
                            Tools.Instance.savedatatobinary((int)sh.Flash_start_address, length, data, 
                                fileName, useNote, note, Tools.Instance.m_currentFileType);
                            shSaved = true;
                        }
                    }
                }

                // Also check using codeblock offset
                if (!shSaved && codeBlockOffset >= 0)
                {
                    if (sh.Length == length && sh.Flash_start_address != address)
                    {
                        if (sh.CodeBlock > 0)
                        {
                            foreach (CodeBlock cb in codeBlocks)
                            {
                                if (cb.CodeID == sh.CodeBlock)
                                {
                                    int thiscodeBlockOffset = (int)sh.Flash_start_address - cb.StartAddress;
                                    if (thiscodeBlockOffset == codeBlockOffset)
                                    {
                                        Tools.Instance.savedatatobinary((int)sh.Flash_start_address, length, 
                                            data, fileName, useNote, note, Tools.Instance.m_currentFileType);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves axis data with codeblock synchronization.
        /// </summary>
        public void SaveAxisDataWithSync(int address, int length, byte[] data, string fileName, 
            bool useNote, string note, SymbolCollection symbols)
        {
            Tools.Instance.savedatatobinary(address, length, data, fileName, useNote, note, Tools.Instance.m_currentFileType);

            if (_appSettings.CodeBlockSyncActive)
            {
                SynchronizeAxisData(address, length, data, fileName, useNote, note, symbols);
            }
        }

        /// <summary>
        /// Synchronizes axis data across codeblocks.
        /// </summary>
        private void SynchronizeAxisData(int address, int length, byte[] data, string fileName, 
            bool useNote, string note, SymbolCollection symbols)
        {
            if (fileName != Tools.Instance.m_currentfile)
                return;

            foreach (SymbolHelper sh in symbols)
            {
                if (sh.X_axis_address != address)
                {
                    if ((sh.X_axis_address & 0x0FFFF) == (address & 0x0FFFF))
                    {
                        if (sh.X_axis_length * 2 == length)
                        {
                            Tools.Instance.savedatatobinary(sh.X_axis_address, length, data, fileName, 
                                useNote, note, Tools.Instance.m_currentFileType);
                        }
                    }
                }
                else if (sh.Y_axis_address != address)
                {
                    if ((sh.Y_axis_address & 0x0FFFF) == (address & 0x0FFFF))
                    {
                        if (sh.Y_axis_length * 2 == length)
                        {
                            Tools.Instance.savedatatobinary(sh.Y_axis_address, length, data, fileName, 
                                useNote, note, Tools.Instance.m_currentFileType);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if critical maps are missing from the symbol collection.
        /// </summary>
        public List<string> GetMissingCriticalMaps(SymbolCollection symbols, IEDCFileParser parser)
        {
            var missingMaps = new List<string>();

            if (!(parser is EDC15PFileParser || parser is EDC15P6FileParser))
                return missingMaps;

            if (MapsWithNameMissing("EGR", symbols)) missingMaps.Add("EGR maps");
            if (MapsWithNameMissing("SVBL", symbols)) missingMaps.Add("SVBL");
            if (MapsWithNameMissing("Torque limiter", symbols)) missingMaps.Add("Torque limiter");
            if (MapsWithNameMissing("Smoke limiter", symbols)) missingMaps.Add("Smoke limiter");
            if (MapsWithNameMissing("Injector duration", symbols)) missingMaps.Add("Injector duration maps");
            if (MapsWithNameMissing("Start of injection", symbols)) missingMaps.Add("Start of injection maps");
            if (MapsWithNameMissing("N75 duty cycle", symbols)) missingMaps.Add("N75 duty cycle map");
            if (MapsWithNameMissing("Inverse driver wish", symbols)) missingMaps.Add("Inverse driver wish map");
            if (MapsWithNameMissing("Boost target map", symbols)) missingMaps.Add("Boost target map");
            if (MapsWithNameMissing("SOI limiter", symbols)) missingMaps.Add("SOI limiter");
            if (MapsWithNameMissing("Driver wish", symbols)) missingMaps.Add("Driver wish map");
            if (MapsWithNameMissing("Boost limit map", symbols)) missingMaps.Add("Boost limit map");
            if (MapsWithNameMissing("MAF correction", symbols)) missingMaps.Add("MAF correction map");
            if (MapsWithNameMissing("MAF linearization", symbols)) missingMaps.Add("MAF linearization map");
            if (MapsWithNameMissing("MAP linearization", symbols)) missingMaps.Add("MAP linearization map");

            return missingMaps;
        }

        private bool MapsWithNameMissing(string varName, SymbolCollection symbols)
        {
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname.StartsWith(varName)) return false;
            }
            return true;
        }

        /// <summary>
        /// Counts how many maps match a specific name pattern.
        /// </summary>
        public int GetMapCount(string varName, SymbolCollection symbols)
        {
            int mapCount = 0;
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname.StartsWith(varName)) mapCount++;
            }
            return mapCount;
        }
    }
}
