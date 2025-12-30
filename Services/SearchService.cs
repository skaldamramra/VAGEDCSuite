using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    public class SearchService
    {
        private AppSettings _appSettings;
        private DockManager _dockManager;

        public SearchService(DockManager dockManager, AppSettings appSettings)
        {
            _dockManager = dockManager;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Performs a search for symbols and map data matching the specified criteria
        /// </summary>
        public DataTable PerformSearch(string currentFile, SymbolCollection symbols, long currentFileLength,
            frmSearchMaps searchOptions, ref SymbolCollection resultCollection)
        {
            DataTable dt = new DataTable();
            
            if (searchOptions.ShowDialog() == DialogResult.OK)
            {
                frmProgress progress = new frmProgress();
                progress.SetProgress("Start searching data...");
                progress.SetProgressPercentage(0);
                progress.Show();
                Application.DoEvents();

                int cnt = 0;
                foreach (SymbolHelper sh in symbols)
                {
                    progress.SetProgress("Searching " + sh.Varname);
                    progress.SetProgressPercentage((cnt * 100) / symbols.Count);
                    bool hit_found = false;

                    // Check symbol name
                    if (searchOptions.IncludeSymbolNames)
                    {
                        if (searchOptions.SearchForNumericValues)
                        {
                            if (sh.Varname.Contains(searchOptions.NumericValueToSearchFor.ToString()))
                            {
                                hit_found = true;
                            }
                        }
                        if (searchOptions.SearchForStringValues)
                        {
                            if (!string.IsNullOrEmpty(searchOptions.StringValueToSearchFor))
                            {
                                if (sh.Varname.Contains(searchOptions.StringValueToSearchFor))
                                {
                                    hit_found = true;
                                }
                            }
                        }
                    }

                    // Check symbol description
                    if (searchOptions.IncludeSymbolDescription)
                    {
                        if (searchOptions.SearchForNumericValues)
                        {
                            if (sh.Description.Contains(searchOptions.NumericValueToSearchFor.ToString()))
                            {
                                hit_found = true;
                            }
                        }
                        if (searchOptions.SearchForStringValues)
                        {
                            if (!string.IsNullOrEmpty(searchOptions.StringValueToSearchFor))
                            {
                                if (sh.Description.Contains(searchOptions.StringValueToSearchFor))
                                {
                                    hit_found = true;
                                }
                            }
                        }
                    }

                    // Search symbol data
                    if (sh.Flash_start_address < currentFileLength)
                    {
                        byte[] symboldata = Tools.Instance.readdatafromfile(currentFile, (int)sh.Flash_start_address, sh.Length, Tools.Instance.m_currentFileType);
                        if (searchOptions.SearchForNumericValues)
                        {
                            for (int i = 0; i < symboldata.Length / 2; i += 2)
                            {
                                float value = Convert.ToInt32(symboldata.GetValue(i)) * 256;
                                value += Convert.ToInt32(symboldata.GetValue(i + 1));
                                value *= (float)GetMapCorrectionFactor(sh.Varname);
                                value += (float)GetMapCorrectionOffset(sh.Varname);
                                if (value == (float)searchOptions.NumericValueToSearchFor)
                                {
                                    hit_found = true;
                                }
                            }
                        }
                        if (searchOptions.SearchForStringValues)
                        {
                            if (searchOptions.StringValueToSearchFor.Length > symboldata.Length)
                            {
                                string symboldataasstring = Encoding.ASCII.GetString(symboldata);
                                if (symboldataasstring.Contains(searchOptions.StringValueToSearchFor))
                                {
                                    hit_found = true;
                                }
                            }
                        }
                    }

                    if (hit_found)
                    {
                        resultCollection.Add(sh);
                    }
                    cnt++;
                }
                progress.Close();

                // Create results table
                dt.Columns.Add("SYMBOLNAME");
                dt.Columns.Add("SRAMADDRESS", typeof(System.Int32));
                dt.Columns.Add("FLASHADDRESS", typeof(System.Int32));
                dt.Columns.Add("LENGTHBYTES", typeof(System.Int32));
                dt.Columns.Add("LENGTHVALUES", typeof(System.Int32));
                dt.Columns.Add("DESCRIPTION");
                dt.Columns.Add("ISCHANGED", typeof(System.Boolean));
                dt.Columns.Add("CATEGORY"); //0
                dt.Columns.Add("DIFFPERCENTAGE", typeof(System.Double));
                dt.Columns.Add("DIFFABSOLUTE", typeof(System.Int32));
                dt.Columns.Add("DIFFAVERAGE", typeof(System.Double));
                dt.Columns.Add("CATEGORYNAME");
                dt.Columns.Add("SUBCATEGORYNAME");
                dt.Columns.Add("SymbolNumber1", typeof(System.Int32));
                dt.Columns.Add("SymbolNumber2", typeof(System.Int32));
                dt.Columns.Add("CodeBlock1", typeof(System.Int32));
                dt.Columns.Add("CodeBlock2", typeof(System.Int32));

                SymbolTranslator st = new SymbolTranslator();
                foreach (SymbolHelper shfound in resultCollection)
                {
                    string helptext = st.TranslateSymbolToHelpText(shfound.Varname);
                    if (shfound.Varname.Contains("."))
                    {
                        try
                        {
                            shfound.Category = shfound.Varname.Substring(0, shfound.Varname.IndexOf("."));
                        }
                        catch (Exception cE)
                        {
                            Console.WriteLine("Failed to assign category to symbol: " + shfound.Varname + " err: " + cE.Message);
                        }
                    }
                    dt.Rows.Add(shfound.Varname, shfound.Start_address, shfound.Flash_start_address, shfound.Length, shfound.Length, helptext, false, 0, 0, 0, 0, shfound.Category, "", shfound.Symbol_number, shfound.Symbol_number, shfound.CodeBlock, shfound.CodeBlock);
                }
            }

            return dt;
        }

        /// <summary>
        /// Creates a dock panel for displaying search results
        /// </summary>
        public DockPanel CreateSearchResultsPanel(string currentFile, SymbolCollection resultCollection, DataTable dt)
        {
            _dockManager.BeginUpdate();
            try
            {
                DockPanel dockPanel = _dockManager.AddPanel(new Point(-500, -500));
                CompareResults tabdet = new CompareResults();
                tabdet.ShowAddressesInHex = _appSettings.ShowAddressesInHex;
                tabdet.SetFilterMode(_appSettings.ShowAddressesInHex);
                tabdet.Dock = DockStyle.Fill;
                tabdet.UseForFind = true;
                tabdet.Filename = currentFile;
                dockPanel.Controls.Add(tabdet);
                dockPanel.Text = "Search results: " + Path.GetFileName(currentFile);
                dockPanel.DockTo(_dockManager, DockingStyle.Left, 1);
                dockPanel.Width = 500;

                tabdet.CompareSymbolCollection = resultCollection;
                tabdet.OpenGridViewGroups(tabdet.gridControl1, 1);
                tabdet.gridControl1.DataSource = dt.Copy();

                return dockPanel;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
                return null;
            }
            finally
            {
                _dockManager.EndUpdate();
            }
        }

        private double GetMapCorrectionFactor(string mapname)
        {
            return 1;
        }

        private double GetMapCorrectionOffset(string mapname)
        {
            return 0;
        }
    }
}