using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using ComponentFactory.Krypton.Docking;
using ComponentFactory.Krypton.Navigator;
using VAGSuite;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service for handling file comparison operations
    /// Extracted from frmMain.cs to improve maintainability
    /// </summary>
    public class FileComparisonService
    {
        private AppSettings _appSettings;
        private KryptonDockingManager _kryptonDockingManager;
        
        public FileComparisonService(KryptonDockingManager kryptonDockingManager, AppSettings appSettings)
        {
            _kryptonDockingManager = kryptonDockingManager;
            _appSettings = appSettings;
        }

        #region Compare To File

        /// <summary>
        /// Compares the current file with another file
        /// </summary>
        /// <param name="filename">Path to the file to compare</param>
        /// <param name="currentFile">Path to the current file</param>
        /// <param name="symbols">Current symbol collection</param>
        /// <param name="codeBlocks">Current code blocks</param>
        /// <param name="dockManager">Dock manager for creating comparison panels</param>
        /// <param name="onSymbolSelect">Callback when symbol is selected</param>
        /// <returns>DataTable with comparison results</returns>
        public DataTable CompareToFile(string filename, string currentFile, SymbolCollection symbols, 
            List<CodeBlock> codeBlocks, DockManager dockManager, 
            CompareResults.NotifySelectSymbol onSymbolSelect)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SYMBOLNAME");
            dt.Columns.Add("SRAMADDRESS", typeof(System.Int32));
            dt.Columns.Add("FLASHADDRESS", typeof(System.Int32));
            dt.Columns.Add("LENGTHBYTES", typeof(System.Int32));
            dt.Columns.Add("LENGTHVALUES", typeof(System.Int32));
            dt.Columns.Add("DESCRIPTION");
            dt.Columns.Add("ISCHANGED", typeof(System.Boolean));
            dt.Columns.Add("CATEGORY", typeof(System.Int32));
            dt.Columns.Add("DIFFPERCENTAGE", typeof(System.Double));
            dt.Columns.Add("DIFFABSOLUTE", typeof(System.Int32));
            dt.Columns.Add("DIFFAVERAGE", typeof(System.Double));
            dt.Columns.Add("CATEGORYNAME", typeof(string));
            dt.Columns.Add("SUBCATEGORYNAME", typeof(string));
            dt.Columns.Add("SymbolNumber1", typeof(System.Int32));
            dt.Columns.Add("SymbolNumber2", typeof(System.Int32));
            dt.Columns.Add("Userdescription", typeof(string));
            dt.Columns.Add("MissingInOriFile", typeof(System.Boolean));
            dt.Columns.Add("MissingInCompareFile", typeof(System.Boolean));
            dt.Columns.Add("CodeBlock1", typeof(System.Int32));
            dt.Columns.Add("CodeBlock2", typeof(System.Int32));

            if (symbols.Count > 0)
            {
                // Compare each symbol from the current file with the comparison file
                foreach (SymbolHelper sh in symbols)
                {
                    double diffperc = 0;
                    int diffabs = 0;
                    double diffavg = 0;
                    
                    if (!CompareSymbolToCurrentFile(sh.Varname, 
                        (int)sh.Flash_start_address, sh.Length, filename, 
                        out diffperc, out diffabs, out diffavg, sh.Correction))
                    {
                        string category = "";
                        
                        dt.Rows.Add(sh.Varname, sh.Start_address, 
                            sh.Flash_start_address, sh.Length, sh.Length, 
                            sh.Varname, false, 0, diffperc, diffabs, diffavg, 
                            category, "", sh.Symbol_number, sh.Symbol_number, 
                            "", false, false, sh.CodeBlock, sh.CodeBlock);
                    }
                }
            }
            
            return dt;
        }

        #endregion

        #region Compare Symbol to Current File

        /// <summary>
        /// Compares a single symbol between the current file and a comparison file
        /// </summary>
        /// <returns>True if symbols match, False if they differ</returns>
        public bool CompareSymbolToCurrentFile(string symbolname, int address, int length, string filename, 
            out double diffperc, out int diffabs, out double diffavg, double correction)
        {
            diffperc = 0;
            diffabs = 0;
            diffavg = 0;
            
            bool retval = true;
            
            if (address > 0)
            {
                // Read current file data
                byte[] curdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, address, length, Tools.Instance.m_currentFileType);
                byte[] compdata = Tools.Instance.readdatafromfile(filename, address, length, Tools.Instance.m_currentFileType);
                
                if (curdata.Length != compdata.Length)
                {
                    Console.WriteLine("Lengths didn't match: " + symbolname);
                    diffabs = length;
                    return false;
                }
                
                double totalvalue1 = 0;
                double totalvalue2 = 0;
                
                for (int offset = 0; offset < curdata.Length; offset += 2)
                {
                    int ival1 = Convert.ToInt32(curdata.GetValue(offset)) * 256 + Convert.ToInt32(curdata.GetValue(offset + 1));
                    int ival2 = Convert.ToInt32(compdata.GetValue(offset)) * 256 + Convert.ToInt32(compdata.GetValue(offset + 1));
                    if (ival1 != ival2)
                    {
                        retval = false;
                        diffabs++;
                    }
                    totalvalue1 += Convert.ToDouble(ival1);
                    totalvalue2 += Convert.ToDouble(ival2);
                }
                
                if (curdata.Length > 0)
                {
                    totalvalue1 /= (curdata.Length / 2);
                    totalvalue2 /= (compdata.Length / 2);
                }
                
                diffavg = Math.Abs(totalvalue1 - totalvalue2) * correction;
                diffperc = (diffabs * 100) / (length / 2);
            }
            
            return retval;
        }

        #endregion

        #region Start Compare Map Viewer

        /// <summary>
        /// Starts a map viewer for comparing a symbol from the comparison file
        /// </summary>
        public void StartCompareMapViewer(string SymbolName, string Filename, int SymbolAddress, 
            int SymbolLength, SymbolCollection curSymbols, int symbolnumber, 
            DockManager dockManager, AppSettings appSettings, 
            MapViewerEx.NotifySaveSymbol onSymbolSave, 
            MapViewerEx.NotifyReadSymbol onSymbolRead, 
            MapViewerEx.ViewerClose onClose,
            MapViewerEx.NotifySliderMove onSliderMove,
            MapViewerEx.SplitterMoved onSplitterMoved,
            MapViewerEx.SelectionChanged onSelectionChanged,
            MapViewerEx.SurfaceGraphViewChangedEx onSurfaceGraphViewChangedEx,
            MapViewerEx.NotifyAxisLock onAxisLock,
            MapViewerEx.ViewTypeChanged onViewTypeChanged)
        {
            // This is a simplified implementation
            // Full implementation would create a MapViewerEx instance for the comparison symbol
            // and set it up with the comparison file data
            
            // Placeholder for actual implementation
        }

        #endregion

        #region Start Compare Difference Viewer

        /// <summary>
        /// Starts a viewer showing the difference between two maps
        /// </summary>
        public void StartCompareDifferenceViewer(SymbolHelper sh, string Filename, int SymbolAddress, 
            string currentFile, SymbolCollection symbols, DockManager dockManager, 
            AppSettings appSettings, MapViewerEx.ViewerClose onClose)
        {
            // This is a simplified implementation
            // Full implementation would create a MapViewerEx instance showing the difference
            // between the current file and comparison file data
            
            // Placeholder for actual implementation
        }

        #endregion

        #region Dump Dock Windows

        /// <summary>
        /// Logs all dock panel names to console (for debugging)
        /// </summary>
        public void DumpDockWindows(DockManager dockManager)
        {
            foreach(KryptonPage page in _kryptonDockingManager.Pages)
            {
                Console.WriteLine(page.Text);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets table dimensions for a symbol
        /// </summary>
        public void GetTableDimensions(SymbolCollection symbols, string symbolname, out int columns, out int rows)
        {
            columns = 8;
            rows = 8;
            
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname == symbolname)
                {
                    columns = sh.X_axis_length;
                    rows = sh.Y_axis_length;
                    break;
                }
            }
        }

        /// <summary>
        /// Gets X-axis values for a symbol
        /// </summary>
        public int[] GetXaxisValues(string filename, SymbolCollection symbols, string symbolname)
        {
            return SymbolQueryHelper.GetXAxisValues(filename, symbols, symbolname);
        }

        /// <summary>
        /// Gets Y-axis values for a symbol
        /// </summary>
        public int[] GetYaxisValues(string filename, SymbolCollection symbols, string symbolname)
        {
            return SymbolQueryHelper.GetYAxisValues(filename, symbols, symbolname);
        }

        /// <summary>
        /// Gets symbol address
        /// </summary>
        public Int64 GetSymbolAddress(SymbolCollection symbols, string symbolname)
        {
            return SymbolQueryHelper.GetSymbolAddress(symbols, symbolname);
        }

        /// <summary>
        /// Gets symbol length
        /// </summary>
        public int GetSymbolLength(SymbolCollection symbols, string symbolname)
        {
            return SymbolQueryHelper.GetSymbolLength(symbols, symbolname);
        }

        #endregion
    }
}
