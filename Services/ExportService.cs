using System;
using System.IO;
using System.Windows.Forms;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    public class ExportService
    {
        private AppSettings _appSettings;

        public ExportService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Starts Excel export for selected symbol
        /// </summary>
        public void StartExcelExport(string currentFile, SymbolCollection symbols, string mapName, int address, int length, byte[] mapdata, int columns, int rows, int[] xaxis, int[] yaxis, SymbolHelper sh)
        {
            ExcelInterface excelInterface = new ExcelInterface();
            if (xaxis != null && yaxis != null)
            {
                string cleanMapName = mapName.Replace(",", "");
                cleanMapName = cleanMapName.Replace("[", "");
                cleanMapName = cleanMapName.Replace("]", "");

                excelInterface.ExportToExcel(cleanMapName, address, length, mapdata, columns, rows, true, xaxis, yaxis, _appSettings.ShowTablesUpsideDown, sh.X_axis_descr, sh.Y_axis_descr, sh.Z_axis_descr);
            }
        }

        /// <summary>
        /// Starts CSV export for selected symbol
        /// </summary>
        public void StartCSVExport(string currentFile, SymbolCollection symbols, string mapName, int address, int length, byte[] mapdata, int columns, int rows, int[] xaxis, int[] yaxis, SymbolHelper sh)
        {
            ExcelInterface excelInterface = new ExcelInterface();
            if (xaxis != null && yaxis != null)
            {
                string cleanMapName = mapName.Replace(",", "");
                cleanMapName = cleanMapName.Replace("[", "");
                cleanMapName = cleanMapName.Replace("]", "");

                excelInterface.ExportToCSV(cleanMapName, address, length, mapdata, columns, rows, true, xaxis, yaxis, _appSettings.ShowTablesUpsideDown, sh.X_axis_descr, sh.Y_axis_descr, sh.Z_axis_descr);
            }
        }

        /// <summary>
        /// Starts XML export for selected symbol
        /// </summary>
        public void StartXMLExport(string currentFile, SymbolCollection symbols, string mapName, int address, int length, byte[] mapdata, int columns, int rows, int[] xaxis, int[] yaxis, SymbolHelper sh)
        {
            ExcelInterface excelInterface = new ExcelInterface();
            if (xaxis != null && yaxis != null)
            {
                string cleanMapName = mapName.Replace(",", "");
                cleanMapName = cleanMapName.Replace("[", "");
                cleanMapName = cleanMapName.Replace("]", "");

                excelInterface.ExportToXML(cleanMapName, address, length, mapdata, columns, rows, true, xaxis, yaxis, _appSettings.ShowTablesUpsideDown, sh.X_axis_descr, sh.Y_axis_descr, sh.Z_axis_descr);
            }
        }

        /// <summary>
        /// Imports file in Excel format
        /// </summary>
        public void ImportFileInExcelFormat(string currentFile, SymbolCollection symbols)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog();
            openFileDialog2.Multiselect = false;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string mapname = string.Empty;
                    string realmapname = string.Empty;
                    int tildeindex = openFileDialog2.FileName.LastIndexOf("~");
                    bool symbolfound = false;
                    
                    if (tildeindex > 0)
                    {
                        tildeindex++;
                        mapname = openFileDialog2.FileName.Substring(tildeindex, openFileDialog2.FileName.Length - tildeindex);
                        mapname = mapname.Replace(".xls", "");
                        mapname = mapname.Replace(".XLS", "");
                        mapname = mapname.Replace(".Xls", "");

                        // Look if it is a valid symbolname
                        foreach (SymbolHelper sh in symbols)
                        {
                            if (sh.Varname.Replace(",", "").Replace("[","").Replace("]","") == mapname || sh.Userdescription.Replace(",", "") == mapname)
                            {
                                symbolfound = true;
                                realmapname = sh.Varname;
                                break;
                            }
                        }
                        if (!symbolfound)
                        {
                            // Ask user for symbol designation
                            frmSymbolSelect frmselect = new frmSymbolSelect(symbols);
                            if (frmselect.ShowDialog() == DialogResult.OK)
                            {
                                mapname = frmselect.SelectedSymbol;
                                realmapname = frmselect.SelectedSymbol;
                            }
                        }
                    }
                    
                    if (realmapname != string.Empty)
                    {
                        ImportExcelSymbol(realmapname, openFileDialog2.FileName, currentFile, symbols);
                    }
                }
                catch (Exception E)
                {
                    frmInfoBox info = new frmInfoBox("Failed to import map from excel: " + E.Message);
                }
            }
        }

        /// <summary>
        /// Imports a symbol from Excel file
        /// </summary>
        public void ImportExcelSymbol(string symbolname, string filename, string currentFile, SymbolCollection symbols)
        {
            ExcelInterface excelInterface = new ExcelInterface();
            bool issixteenbit = true;
            System.Data.DataTable dt = excelInterface.getDataFromXLS(filename);
            int symbollength = SymbolQueryHelper.GetSymbolLength(symbols, symbolname);
            int datalength = symbollength;
            if (issixteenbit) datalength /= 2;
            int[] buffer = new int[datalength];
            int bcount = 0;

            for (int rtel = dt.Rows.Count; rtel >= 1; rtel--)
            {
                try
                {
                    int idx = 0;
                    foreach (object o in dt.Rows[rtel - 1].ItemArray)
                    {
                        if (idx > 0)
                        {
                            if (o != null && o != DBNull.Value && bcount < buffer.Length)
                            {
                                buffer.SetValue(Convert.ToInt32(o), bcount++);
                            }
                        }
                        idx++;
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine("ImportExcelSymbol: " + E.Message);
                }
            }

            if (bcount >= datalength)
            {
                byte[] data = new byte[symbollength];
                int cellcount = 0;
                if (issixteenbit)
                {
                    for (int dcnt = 0; dcnt < buffer.Length; dcnt++)
                    {
                        string bstr1 = "0";
                        string bstr2 = "0";
                        int cellvalue = Convert.ToInt32(buffer.GetValue(dcnt));
                        string svalue = cellvalue.ToString("X4");

                        bstr1 = svalue.Substring(svalue.Length - 4, 2);
                        bstr2 = svalue.Substring(svalue.Length - 2, 2);
                        data.SetValue(Convert.ToByte(bstr1, 16), cellcount++);
                        data.SetValue(Convert.ToByte(bstr2, 16), cellcount++);
                    }
                }
                else
                {
                    for (int dcnt = 0; dcnt < buffer.Length; dcnt++)
                    {
                        int cellvalue = Convert.ToInt32(buffer.GetValue(dcnt));
                        data.SetValue(Convert.ToByte(cellvalue), cellcount++);
                    }
                }
                
                Tools.Instance.savedatatobinary((int)SymbolQueryHelper.GetSymbolAddress(symbols, symbolname), symbollength, data, currentFile, true, Tools.Instance.m_currentFileType);
                Tools.Instance.UpdateChecksum(currentFile, false);
            }
        }

        /// <summary>
        /// Exports to XDF format
        /// </summary>
        public void StartXDFExport(string currentFile, SymbolCollection symbols, long currentFileLength)
        {
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();
            saveFileDialog2.Filter = "XDF files|*.xdf";
            
            if (saveFileDialog2.ShowDialog() == DialogResult.OK)
            {
                XDFWriter xdf = new XDFWriter();
                string filename = saveFileDialog2.FileName;
                
                xdf.CreateXDF(filename, currentFile, (int)currentFileLength, (int)currentFileLength);
                foreach (SymbolHelper sh in symbols)
                {
                    if (sh.Flash_start_address != 0)
                    {
                        int fileoffset = (int)sh.Flash_start_address;
                        while (fileoffset > (int)currentFileLength) fileoffset -= (int)currentFileLength;

                        int xaxisaddress = sh.X_axis_address;
                        int yaxisaddress = sh.Y_axis_address;
                        bool isxaxissixteenbit = true;
                        bool isyaxissixteenbit = true;
                        int columns = sh.X_axis_length;
                        int rows = sh.Y_axis_length;

                        xdf.AddTable(sh.Varname, sh.Description, XDFCategories.Fuel, sh.X_axis_descr, sh.Y_axis_descr, sh.Z_axis_descr, columns, rows, (int)fileoffset, true, xaxisaddress, yaxisaddress, isxaxissixteenbit, isyaxissixteenbit, 1.0F, 1.0F, 1.0F);
                    }
                }
                xdf.CloseFile();
            }
        }
    }
}