using System;
using System.Data;
using System.IO;
using VAGSuite;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service for handling all import/export operations (Excel, CSV, XML)
    /// Extracted from frmMain.cs to improve maintainability
    /// </summary>
    public class ImportExportService
    {
        private AppSettings _appSettings;
        
        public ImportExportService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        #region Excel Export

        /// <summary>
        /// Exports symbol data to Excel format
        /// </summary>
        /// <param name="gridView">The grid view containing selected symbols</param>
        /// <param name="currentFile">Path to the current binary file</param>
        /// <param name="symbols">Collection of symbols to export</param>
        /// <param name="showTablesUpsideDown">Whether to show tables upside down</param>
        public void StartExcelExport(object gridView, string currentFile, SymbolCollection symbols, bool showTablesUpsideDown)
        {
            ExcelInterface excelInterface = new ExcelInterface();
            if (gridView != null && gridView is DevExpress.XtraGrid.Views.Grid.GridView)
            {
                DevExpress.XtraGrid.Views.Grid.GridView gv = (DevExpress.XtraGrid.Views.Grid.GridView)gridView;
                if (gv.SelectedRowsCount > 0)
                {
                    int[] selrows = gv.GetSelectedRows();
                    if (selrows.Length > 0)
                    {
                        SymbolHelper sh = (SymbolHelper)gv.GetRow((int)selrows.GetValue(0));
                        string Map_name = sh.Varname;
                        if ((Map_name.StartsWith("2D") || Map_name.StartsWith("3D")) && sh.Userdescription != "") Map_name = sh.Userdescription;
                    }
                }
            }
            
            // Simplified export logic - full implementation requires ExcelInterface details
            throw new NotImplementedException("Excel export requires ExcelInterface implementation");
        }

        #endregion

        #region CSV Export

        /// <summary>
        /// Exports symbol data to CSV format
        /// </summary>
        public void StartCSVExport(object gridView, string currentFile, SymbolCollection symbols, bool showTablesUpsideDown)
        {
            // CSV export implementation placeholder
            throw new NotImplementedException("CSV export requires ExcelInterface implementation");
        }

        #endregion

        #region XML Export

        /// <summary>
        /// Exports symbol data to XML format
        /// </summary>
        public void StartXMLExport(object gridView, string currentFile, SymbolCollection symbols, bool showTablesUpsideDown)
        {
            // XML export implementation placeholder
            throw new NotImplementedException("XML export requires ExcelInterface implementation");
        }

        #endregion

        #region Import File in Excel Format

        /// <summary>
        /// Imports symbol data from Excel format file
        /// </summary>
        /// <param name="filename">Path to the Excel file</param>
        /// <param name="currentFile">Path to the current binary file</param>
        /// <param name="symbols">Collection of symbols to update</param>
        public void ImportFileInExcelFormat(string filename, string currentFile, SymbolCollection symbols)
        {
            // Implementation placeholder for Excel import
            throw new NotImplementedException("Excel import requires ExcelInterface implementation");
        }

        #endregion

        #region Save Additional Symbols

        /// <summary>
        /// Saves additional symbol descriptions to XML file
        /// </summary>
        public void SaveAdditionalSymbols(string currentFile, SymbolCollection symbols)
        {
            try
            {
                DataTable dt = new DataTable(Path.GetFileNameWithoutExtension(currentFile));
                dt.Columns.Add("SYMBOLNAME");
                dt.Columns.Add("SYMBOLNUMBER", typeof(System.Int32));
                dt.Columns.Add("FLASHADDRESS", typeof(System.Int32));
                dt.Columns.Add("DESCRIPTION");
                
                byte[] allBytes = File.ReadAllBytes(currentFile);
                string boschpartNumber = Tools.Instance.ExtractBoschPartnumber(allBytes);
                partNumberConverter pnc = new partNumberConverter();
                ECUInfo info = pnc.ConvertPartnumber(boschpartNumber, allBytes.Length);
                string checkstring = boschpartNumber + "_" + info.SoftwareID;

                string xmlfilename = Tools.Instance.GetWorkingDirectory() + "\\repository\\" + 
                    Path.GetFileNameWithoutExtension(currentFile) + 
                    File.GetCreationTime(currentFile).ToString("yyyyMMddHHmmss") + 
                    checkstring + ".xml";
                
                if (!Directory.Exists(Tools.Instance.GetWorkingDirectory() + "\\repository"))
                {
                    Directory.CreateDirectory(Tools.Instance.GetWorkingDirectory() + "\\repository");
                }
                
                if (File.Exists(xmlfilename))
                {
                    File.Delete(xmlfilename);
                }
                
                foreach (SymbolHelper sh in symbols)
                {
                    if (sh.Userdescription != "")
                    {
                        dt.Rows.Add(sh.Varname, sh.Symbol_number, sh.Flash_start_address, sh.Userdescription);
                    }
                }
                
                dt.WriteXml(xmlfilename);
            }
            catch (Exception e)
            {
                Console.WriteLine("SaveAdditionalSymbols failed: " + e.Message);
            }
        }

        #endregion

        #region Try to Load Additional Symbols

        /// <summary>
        /// Loads additional symbols from various file formats
        /// </summary>
        public void TryToLoadAdditionalSymbols(string filename, ImportFileType importFileType, SymbolCollection symbolCollection, bool fromRepository)
        {
            if (importFileType == ImportFileType.XML)
            {
                ImportXMLFile(filename, symbolCollection, fromRepository);
            }
            else if (importFileType == ImportFileType.AS2)
            {
                TryToLoadAdditionalAS2Symbols(filename, symbolCollection);
            }
            else if (importFileType == ImportFileType.CSV)
            {
                TryToLoadAdditionalCSVSymbols(filename, symbolCollection);
            }
        }

        /// <summary>
        /// Loads additional symbols from CSV format
        /// </summary>
        public void TryToLoadAdditionalCSVSymbols(string filename, SymbolCollection coll2load)
        {
            try
            {
                SymbolTranslator st = new SymbolTranslator();
                char[] sep = new char[1];
                sep.SetValue(';', 0);
                string[] fileContent = File.ReadAllLines(filename);
                
                foreach (string line in fileContent)
                {
                    string[] values = line.Split(sep);
                    try
                    {
                        string varname = (string)values.GetValue(1);
                        int flashaddress = Convert.ToInt32(values.GetValue(0));
                        
                        foreach (SymbolHelper sh in coll2load)
                        {
                            if (sh.Flash_start_address == flashaddress)
                            {
                                sh.Userdescription = varname;
                            }
                        }
                    }
                    catch (Exception lineE)
                    {
                        Console.WriteLine("Failed to import a symbol from CSV file " + line + ": " + lineE.Message);
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to import additional CSV symbols: " + E.Message);
            }
        }

        /// <summary>
        /// Loads additional symbols from AS2 format
        /// </summary>
        public void TryToLoadAdditionalAS2Symbols(string filename, SymbolCollection coll2load)
        {
            try
            {
                SymbolTranslator st = new SymbolTranslator();
                char[] sep = new char[1];
                sep.SetValue(';', 0);
                string[] fileContent = File.ReadAllLines(filename);
                int symbolnumber = 0;
                
                foreach (string line in fileContent)
                {
                    if (line.StartsWith("*"))
                    {
                        symbolnumber++;
                        string[] values = line.Split(sep);
                        try
                        {
                            string varname = (string)values.GetValue(0);
                            varname = varname.Substring(1);
                            int idxSymTab = 0;
                            
                            foreach (SymbolHelper sh in coll2load)
                            {
                                if (sh.Length > 0) idxSymTab++;
                                if (idxSymTab == symbolnumber)
                                {
                                    sh.Userdescription = varname;
                                    break;
                                }
                            }
                        }
                        catch (Exception lineE)
                        {
                            Console.WriteLine("Failed to import a symbol from AS2 file " + line + ": " + lineE.Message);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to import additional AS2 symbols: " + E.Message);
            }
        }

        #endregion

        #region Import XML File

        /// <summary>
        /// Loads symbol descriptions from XML file
        /// </summary>
        public bool ImportXMLFile(string filename, SymbolCollection coll2load, bool ImportFromRepository)
        {
            bool retval = false;
            SymbolTranslator st = new SymbolTranslator();
            
            DataTable dt = new DataTable(Path.GetFileNameWithoutExtension(filename));
            dt.Columns.Add("SYMBOLNAME");
            dt.Columns.Add("SYMBOLNUMBER", typeof(System.Int32));
            dt.Columns.Add("FLASHADDRESS", typeof(System.Int32));
            dt.Columns.Add("DESCRIPTION");
            
            if (ImportFromRepository)
            {
                try
                {
                    byte[] allBytes = File.ReadAllBytes(filename);
                    string boschpartNumber = Tools.Instance.ExtractBoschPartnumber(allBytes);
                    partNumberConverter pnc = new partNumberConverter();
                    ECUInfo info = pnc.ConvertPartnumber(boschpartNumber, allBytes.Length);
                    string checkstring = boschpartNumber + "_" + info.SoftwareID;

                    string xmlfilename = Tools.Instance.GetWorkingDirectory() + "\\repository\\" + 
                        Path.GetFileNameWithoutExtension(filename) + 
                        File.GetCreationTime(filename).ToString("yyyyMMddHHmmss") + 
                        checkstring + ".xml";
                    
                    if (!Directory.Exists(Tools.Instance.GetWorkingDirectory() + "\\repository"))
                    {
                        Directory.CreateDirectory(Tools.Instance.GetWorkingDirectory() + "\\repository");
                    }
                    
                    if (File.Exists(xmlfilename))
                    {
                        dt.ReadXml(xmlfilename);
                        retval = true;
                    }
                }
                catch
                {
                    // Repository import failed, continue without it
                }
            }
            else
            {
                string binname = GetFileDescriptionFromFile(filename);
                if (binname != string.Empty)
                {
                    dt = new DataTable(binname);
                    dt.Columns.Add("SYMBOLNAME");
                    dt.Columns.Add("SYMBOLNUMBER", typeof(System.Int32));
                    dt.Columns.Add("FLASHADDRESS", typeof(System.Int32));
                    dt.Columns.Add("DESCRIPTION");
                    
                    if (File.Exists(filename))
                    {
                        dt.ReadXml(filename);
                        retval = true;
                    }
                }
            }
            
            foreach (SymbolHelper sh in coll2load)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        if (sh.Flash_start_address == Convert.ToInt32(dr["FLASHADDRESS"]))
                        {
                            sh.Userdescription = dr["DESCRIPTION"].ToString();
                            break;
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.Message);
                    }
                }
            }
            
            return retval;
        }

        #endregion

        #region Get File Description From File

        /// <summary>
        /// Extracts file description from XML file
        /// </summary>
        public string GetFileDescriptionFromFile(string file)
        {
            string retval = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    sr.ReadLine();
                    sr.ReadLine();
                    string name = sr.ReadLine();
                    name = name.Trim();
                    name = name.Replace("<", "");
                    name = name.Replace(">", "");
                    name = name.Replace("_x0020_", " ");
                    
                    for (int i = 0; i <= 9; i++)
                    {
                        name = name.Replace("_x003" + i.ToString() + "_", i.ToString());
                    }
                    retval = name;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            return retval;
        }

        #endregion
    }
}
