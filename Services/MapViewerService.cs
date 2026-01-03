using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using VAGSuite.Helpers;
using VAGSuite.Services;

namespace VAGSuite.Services
{
    public class MapViewerService
    {
        private AppSettings _appSettings;
        private DockManager _dockManager;

        public MapViewerService(DockManager dockManager, AppSettings appSettings)
        {
            _dockManager = dockManager;
            _appSettings = appSettings;
        }

        /// <summary>
        /// Starts a table viewer for the specified symbol
        /// </summary>
        public void StartTableViewer(SymbolHelper sh, string currentFile, SymbolCollection symbols)
        {
            if (sh == null) return;
            if (sh.Flash_start_address == 0 && sh.Start_address == 0) return;

            DockPanel dockPanel;
            bool pnlfound = false;
            pnlfound = CheckMapViewerActive(sh, currentFile);
            
            if (!pnlfound)
            {
                _dockManager.BeginUpdate();
                try
                {
                    MapViewerEx tabdet = new MapViewerEx();
                    tabdet.AutoUpdateIfSRAM = false;
                    tabdet.AutoUpdateInterval = 99999;
                    tabdet.SetViewSize(ViewSize.NormalView);
                    tabdet.Visible = false;
                    tabdet.Filename = currentFile;
                    tabdet.GraphVisible = true;
                    tabdet.Viewtype = _appSettings.DefaultViewType;
                    tabdet.DisableColors = _appSettings.DisableMapviewerColors;
                    tabdet.AutoSizeColumns = _appSettings.AutoSizeColumnsInWindows;
                    tabdet.GraphVisible = _appSettings.ShowGraphs;
                    tabdet.IsRedWhite = _appSettings.ShowRedWhite;
                    tabdet.SetViewSize(_appSettings.DefaultViewSize);
                    tabdet.Map_name = sh.Varname;
                    tabdet.Map_descr = tabdet.Map_name;
                    tabdet.Map_cat = XDFCategories.Undocumented;

                    // Set axis information
                    tabdet.X_axis_name = sh.X_axis_descr;
                    tabdet.Y_axis_name = sh.Y_axis_descr;
                    tabdet.Z_axis_name = sh.Z_axis_descr;
                    tabdet.XaxisUnits = sh.XaxisUnits;
                    tabdet.YaxisUnits = sh.YaxisUnits;
                    tabdet.X_axisAddress = sh.Y_axis_address;
                    tabdet.Y_axisAddress = sh.X_axis_address;
                    tabdet.Xaxiscorrectionfactor = sh.X_axis_correction;
                    tabdet.Yaxiscorrectionfactor = sh.Y_axis_correction;
                    tabdet.Xaxiscorrectionoffset = sh.X_axis_offset;
                    tabdet.Yaxiscorrectionoffset = sh.Y_axis_offset;

                    // Get axis values
                    int[] xAxisValues = SymbolQueryHelper.GetXAxisValues(currentFile, symbols, tabdet.Map_name);
                    int[] yAxisValues = SymbolQueryHelper.GetYAxisValues(currentFile, symbols, tabdet.Map_name);
                    tabdet.X_axisvalues = xAxisValues;
                    tabdet.Y_axisvalues = yAxisValues;

                    dockPanel = _dockManager.AddPanel(DockingStyle.Right);
                    int dw = 650;
                    if (xAxisValues.Length > 0)
                    {
                        dw = 30 + ((xAxisValues.Length + 1) * 45);
                    }
                    if (dw < 400) dw = 400;
                    if (dw > 800) dw = 800;
                    dockPanel.FloatSize = new Size(dw, 900);

                    dockPanel.Tag = currentFile;

                    int columns = 8;
                    int rows = 8;
                    SymbolQueryHelper.GetTableDimensions(symbols, tabdet.Map_name, out columns, out rows);
                    int address = Convert.ToInt32(sh.Flash_start_address);
                    int sramaddress = 0;

                    if (address != 0)
                    {
                        tabdet.Map_address = address;
                        tabdet.Map_sramaddress = sramaddress;
                        int length = Convert.ToInt32(sh.Length);
                        tabdet.Map_length = length;
                        byte[] mapdata = Tools.Instance.readdatafromfile(currentFile, address, length, Tools.Instance.m_currentFileType);
                        tabdet.Map_content = mapdata;
                        tabdet.Correction_factor = sh.Correction;
                        tabdet.Correction_offset = sh.Offset;
                        tabdet.IsUpsideDown = _appSettings.ShowTablesUpsideDown;
                        tabdet.ShowTable(columns, true);
                        tabdet.Dock = DockStyle.Fill;

                        // Subscribe to axis editor event - FIX: was missing after refactor
                        tabdet.onAxisEditorRequested += tabdet_onAxisEditorRequested;
                        dockPanel.Text = "Symbol: " + tabdet.Map_name + " [" + Path.GetFileName(currentFile) + "]";
                        
                        int width = 500;
                        if (xAxisValues.Length > 0)
                        {
                            width = 30 + ((xAxisValues.Length + 1) * 45);
                        }
                        if (width < 500) width = 500;
                        if (width > 800) width = 800;
                        dockPanel.Width = width;
                        // Verified: DevExpress DockPanel requires adding to ControlContainer
                        if (dockPanel.ControlContainer != null)
                        {
                            dockPanel.ControlContainer.Controls.Add(tabdet);
                            dockPanel.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
                            tabdet.Visible = true;
                            tabdet.BringToFront();
                        }
                    }
                    else
                    {
                        byte[] mapdata = new byte[sh.Length];
                        mapdata.Initialize();
                    }
                    tabdet.Visible = true;
                }
                catch (Exception newdockE)
                {
                    Console.WriteLine(newdockE.Message);
                }
                _dockManager.EndUpdate();
            }
        }

        /// <summary>
        /// Starts a compare map viewer for the specified symbol
        /// </summary>
        public void StartCompareMapViewer(string symbolName, string filename, int symbolAddress, int symbolLength, SymbolCollection curSymbols, int symbolnumber)
        {
            try
            {
                SymbolHelper sh = SymbolQueryHelper.FindSymbol(curSymbols, symbolName);

                DockPanel dockPanel;
                bool pnlfound = false;
                foreach (DockPanel pnl in _dockManager.Panels)
                {
                    if (pnl.Text == "Symbol: " + symbolName + " [" + Path.GetFileName(filename) + "]")
                    {
                        if (pnl.Tag.ToString() == filename)
                        {
                            dockPanel = pnl;
                            pnlfound = true;
                            dockPanel.Show();
                        }
                    }
                }
                if (!pnlfound)
                {
                    _dockManager.BeginUpdate();
                    try
                    {
                        dockPanel = _dockManager.AddPanel(new Point(-500, -500));
                        dockPanel.Tag = filename;
                        MapViewerEx tabdet = new MapViewerEx();

                        tabdet.AutoUpdateIfSRAM = false;
                        tabdet.AutoUpdateInterval = 99999;
                        tabdet.SetViewSize(ViewSize.NormalView);
                        tabdet.Viewtype = _appSettings.DefaultViewType;
                        tabdet.DisableColors = _appSettings.DisableMapviewerColors;
                        tabdet.AutoSizeColumns = _appSettings.AutoSizeColumnsInWindows;
                        tabdet.GraphVisible = _appSettings.ShowGraphs;
                        tabdet.IsRedWhite = _appSettings.ShowRedWhite;
                        tabdet.SetViewSize(_appSettings.DefaultViewSize);
                        tabdet.Filename = filename;
                        tabdet.Map_name = symbolName;
                        tabdet.Map_descr = tabdet.Map_name;
                        tabdet.Map_cat = XDFCategories.Undocumented;

                        int[] xAxisValues = SymbolQueryHelper.GetXAxisValues(filename, curSymbols, tabdet.Map_name);
                        int[] yAxisValues = SymbolQueryHelper.GetYAxisValues(filename, curSymbols, tabdet.Map_name);
                        tabdet.X_axisvalues = xAxisValues;
                        tabdet.Y_axisvalues = yAxisValues;

                        tabdet.X_axis_name = sh.X_axis_descr;
                        tabdet.Y_axis_name = sh.Y_axis_descr;
                        tabdet.Z_axis_name = sh.Z_axis_descr;
                        tabdet.XaxisUnits = sh.XaxisUnits;
                        tabdet.YaxisUnits = sh.YaxisUnits;
                        tabdet.X_axisAddress = sh.Y_axis_address;
                        tabdet.Y_axisAddress = sh.X_axis_address;
                        tabdet.Xaxiscorrectionfactor = sh.X_axis_correction;
                        tabdet.Yaxiscorrectionfactor = sh.Y_axis_correction;

                        int columns = 8;
                        int rows = 8;
                        SymbolQueryHelper.GetTableDimensions(curSymbols, tabdet.Map_name, out columns, out rows);
                        int address = Convert.ToInt32(symbolAddress);
                        if (address != 0)
                        {
                            tabdet.Map_address = address;
                            int length = symbolLength;
                            tabdet.Map_length = length;
                            byte[] mapdata = Tools.Instance.readdatafromfile(filename, address, length, Tools.Instance.m_currentFileType);
                            tabdet.Map_content = mapdata;
                            tabdet.Correction_factor = sh.Correction;
                            tabdet.Correction_offset = sh.Offset;
                            tabdet.IsUpsideDown = _appSettings.ShowTablesUpsideDown;
                            tabdet.ShowTable(columns, true);
                            tabdet.Dock = DockStyle.Fill;

                            // Subscribe to axis editor event - FIX: was missing after refactor
                            tabdet.onAxisEditorRequested += tabdet_onAxisEditorRequested;

                            dockPanel.Text = "Symbol: " + symbolName + " [" + Path.GetFileName(filename) + "]";
                            dockPanel.DockTo(_dockManager, DockingStyle.Right, 1);

                            bool isDocked = false;
                            foreach (DockPanel pnl in _dockManager.Panels)
                            {
                                if (pnl.Text.StartsWith("Symbol: " + symbolName) && pnl != dockPanel && (pnl.Visibility == DockVisibility.Visible))
                                {
                                    dockPanel.DockAsTab(pnl, 0);
                                    isDocked = true;
                                    break;
                                }
                            }
                            if (!isDocked)
                            {
                                int width = 500;
                                if (xAxisValues.Length > 0)
                                {
                                    width = 30 + ((xAxisValues.Length + 1) * 45);
                                }
                                if (width < 500) width = 500;
                                if (width > 800) width = 800;
                                dockPanel.Width = width;
                            }
                            // Verified: DevExpress DockPanel requires adding to ControlContainer
                            dockPanel.ControlContainer.Controls.Add(tabdet);
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.Message);
                    }
                    _dockManager.EndUpdate();
                    Application.DoEvents();
                }
            }
            catch (Exception startnewcompareE)
            {
                Console.WriteLine(startnewcompareE.Message);
            }
        }

        /// <summary>
        /// Starts a difference viewer for comparing symbols
        /// </summary>
        public void StartCompareDifferenceViewer(SymbolHelper sh, string filename, int symbolAddress)
        {
            DockPanel dockPanel;
            bool pnlfound = false;
            foreach (DockPanel pnl in _dockManager.Panels)
            {
                if (pnl.Text == "Symbol difference: " + sh.Varname + " [" + Path.GetFileName(Tools.Instance.m_currentfile) + "]")
                {
                    dockPanel = pnl;
                    pnlfound = true;
                    dockPanel.Show();
                }
            }
            if (!pnlfound)
            {
                _dockManager.BeginUpdate();
                try
                {
                    dockPanel = _dockManager.AddPanel(new Point(-500, -500));
                    dockPanel.Tag = Tools.Instance.m_currentfile;
                    MapViewerEx tabdet = new MapViewerEx();
                    tabdet.Map_name = sh.Varname;
                    tabdet.IsDifferenceViewer = true;
                    tabdet.AutoUpdateIfSRAM = false;
                    tabdet.AutoUpdateInterval = 999999;
                    tabdet.Viewtype = _appSettings.DefaultViewType;
                    tabdet.DisableColors = _appSettings.DisableMapviewerColors;
                    tabdet.AutoSizeColumns = _appSettings.AutoSizeColumnsInWindows;
                    tabdet.GraphVisible = _appSettings.ShowGraphs;
                    tabdet.IsRedWhite = _appSettings.ShowRedWhite;
                    tabdet.SetViewSize(_appSettings.DefaultViewSize);
                    tabdet.Filename = filename;
                    tabdet.Map_descr = tabdet.Map_name;
                    tabdet.Map_cat = XDFCategories.Undocumented;

                    int[] xAxisValues = SymbolQueryHelper.GetXAxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, tabdet.Map_name);
                    int[] yAxisValues = SymbolQueryHelper.GetYAxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, tabdet.Map_name);
                    tabdet.X_axisvalues = xAxisValues;
                    tabdet.Y_axisvalues = yAxisValues;

                    tabdet.X_axis_name = sh.X_axis_descr;
                    tabdet.Y_axis_name = sh.Y_axis_descr;
                    tabdet.Z_axis_name = sh.Z_axis_descr;
                    tabdet.XaxisUnits = sh.XaxisUnits;
                    tabdet.YaxisUnits = sh.YaxisUnits;
                    tabdet.X_axisAddress = sh.Y_axis_address;
                    tabdet.Y_axisAddress = sh.X_axis_address;
                    tabdet.Xaxiscorrectionfactor = sh.X_axis_correction;
                    tabdet.Yaxiscorrectionfactor = sh.Y_axis_correction;

                    int columns = 8;
                    int rows = 8;
                    SymbolQueryHelper.GetTableDimensions(Tools.Instance.m_symbols, tabdet.Map_name, out columns, out rows);
                    int address = Convert.ToInt32(symbolAddress);
                    if (address != 0)
                    {
                        tabdet.Map_address = address;
                        int length = sh.Length;
                        tabdet.Map_length = length;
                        byte[] mapdata = Tools.Instance.readdatafromfile(filename, address, length, Tools.Instance.m_currentFileType);
                        byte[] mapdataorig = Tools.Instance.readdatafromfile(filename, address, length, Tools.Instance.m_currentFileType);
                        byte[] mapdata2 = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, (int)SymbolQueryHelper.GetSymbolAddress(Tools.Instance.m_symbols, sh.Varname), SymbolQueryHelper.GetSymbolLength(Tools.Instance.m_symbols, sh.Varname), Tools.Instance.m_currentFileType);

                        tabdet.Map_original_content = mapdataorig;
                        tabdet.Map_compare_content = mapdata2;

                        if (mapdata.Length == mapdata2.Length)
                        {
                            for (int bt = 0; bt < mapdata2.Length; bt += 2)
                            {
                                int value1 = Convert.ToInt16(mapdata.GetValue(bt)) * 256 + Convert.ToInt16(mapdata.GetValue(bt + 1));
                                int value2 = Convert.ToInt16(mapdata2.GetValue(bt)) * 256 + Convert.ToInt16(mapdata2.GetValue(bt + 1));
                                value1 = Math.Abs((int)value1 - (int)value2);
                                byte v1 = (byte)(value1 / 256);
                                byte v2 = (byte)(value1 - (int)v1 * 256);
                                mapdata.SetValue(v1, bt);
                                mapdata.SetValue(v2, bt + 1);
                            }

                            tabdet.Map_content = mapdata;
                            tabdet.UseNewCompare = true;
                            tabdet.Correction_factor = sh.Correction;
                            tabdet.Correction_offset = sh.Offset;
                            tabdet.IsUpsideDown = _appSettings.ShowTablesUpsideDown;
                            tabdet.ShowTable(columns, true);
                            tabdet.Dock = DockStyle.Fill;

                            // Subscribe to axis editor event - FIX: was missing after refactor
                            tabdet.onAxisEditorRequested += tabdet_onAxisEditorRequested;

                            dockPanel.Text = "Symbol difference: " + sh.Varname + " [" + Path.GetFileName(filename) + "]";

                            if (_appSettings.AutoSizeNewWindows)
                            {
                                if (xAxisValues.Length > 0)
                                {
                                    dockPanel.Width = 30 + ((xAxisValues.Length + 1) * 45);
                                }
                            }
                            if (dockPanel.Width < 400) dockPanel.Width = 400;
                            // Verified: DevExpress DockPanel requires adding to ControlContainer
                            dockPanel.ControlContainer.Controls.Add(tabdet);
                        }
                        else
                        {
                            frmInfoBox info = new frmInfoBox("Map lengths don't match...");
                        }
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine(E.Message);
                }
                _dockManager.EndUpdate();
            }
        }

        /// <summary>
        /// Starts an axis viewer for editing axis values
        /// </summary>
        public void StartAxisViewer(SymbolHelper symbol, Axis axisToShow, string currentFile, SymbolCollection symbols)
        {
            DockPanel dockPanel;
            _dockManager.BeginUpdate();
            try
            {
                dockPanel = _dockManager.AddPanel(DockingStyle.Right);
                int dw = 650;
                dockPanel.FloatSize = new Size(dw, 900);
                dockPanel.Width = dw;
                dockPanel.Tag = currentFile;
                ctrlAxisEditor tabdet = new ctrlAxisEditor();
                tabdet.FileName = currentFile;

                if (axisToShow == Axis.XAxis)
                {
                    tabdet.AxisID = symbol.Y_axis_ID;
                    tabdet.AxisAddress = symbol.Y_axis_address;
                    tabdet.Map_name = symbol.X_axis_descr + " (" + symbol.Y_axis_address.ToString("X8") + ")";
                    int[] values = SymbolQueryHelper.GetXAxisValues(currentFile, symbols, symbol.Varname);
                    float[] dataValues = new float[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        float fValue = (float)Convert.ToDouble(values.GetValue(i)) * (float)symbol.X_axis_correction;
                        dataValues.SetValue(fValue, i);
                    }
                    tabdet.CorrectionFactor = (float)symbol.X_axis_correction;
                    tabdet.SetData(dataValues);
                    dockPanel.Text = "Axis: (X) " + tabdet.Map_name + " [" + Path.GetFileName(currentFile) + "]";
                }
                else if (axisToShow == Axis.YAxis)
                {
                    tabdet.AxisID = symbol.X_axis_ID;
                    tabdet.AxisAddress = symbol.X_axis_address;
                    tabdet.Map_name = symbol.Y_axis_descr + " (" + symbol.X_axis_address.ToString("X8") + ")";
                    int[] values = SymbolQueryHelper.GetYAxisValues(currentFile, symbols, symbol.Varname);
                    float[] dataValues = new float[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        float fValue = (float)Convert.ToDouble(values.GetValue(i)) * (float)symbol.Y_axis_correction;
                        dataValues.SetValue(fValue, i);
                    }
                    tabdet.CorrectionFactor = (float)symbol.Y_axis_correction;
                    tabdet.SetData(dataValues);
                    dockPanel.Text = "Axis: (Y) " + tabdet.Map_name + " [" + Path.GetFileName(currentFile) + "]";
                }

                // Subscribe to save event - FIX: was missing after refactor
                tabdet.onSave += tabdet_onSave;
                tabdet.onClose += tabdet_onClose;

                tabdet.Dock = DockStyle.Fill;
                dockPanel.Controls.Add(tabdet);
            }
            catch (Exception newdockE)
            {
                Console.WriteLine(newdockE.Message);
            }
            _dockManager.EndUpdate();
            Application.DoEvents();
        }

        /// <summary>
        /// Event handler for axis editor save
        /// </summary>
        private void tabdet_onSave(object sender, EventArgs e)
        {
            if (sender is ctrlAxisEditor)
            {
                ctrlAxisEditor editor = (ctrlAxisEditor)sender;
                // recalculate the values back and store it in the file at the correct location
                float[] newvalues = editor.GetData();
                int[] iValues = new int[newvalues.Length];
                // calculate back to integer values
                for (int i = 0; i < newvalues.Length; i++)
                {
                    int iValue = Convert.ToInt32(Convert.ToDouble(newvalues.GetValue(i)) / editor.CorrectionFactor);
                    iValues.SetValue(iValue, i);
                }
                byte[] barr = new byte[iValues.Length * 2];
                int bCount = 0;
                for (int i = 0; i < iValues.Length; i++)
                {
                    int iVal = (int)iValues.GetValue(i);
                    byte b1 = (byte)((iVal & 0x00FF00) / 256);
                    byte b2 = (byte)(iVal & 0x0000FF);
                    barr[bCount++] = b1;
                    barr[bCount++] = b2;
                }
                
                // Use the existing pattern from frmMain.cs - delegate to frmMain via event or use FileOperationsManager directly
                // For now, we'll raise an event that frmMain can handle
                OnAxisSaveRequested?.Invoke(this, new AxisSaveRequestedEventArgs
                {
                    AxisAddress = editor.AxisAddress,
                    Data = barr,
                    Filename = editor.FileName
                });
            }
        }

        /// <summary>
        /// Event handler for axis editor close
        /// </summary>
        private void tabdet_onClose(object sender, EventArgs e)
        {
            if (sender is ctrlAxisEditor)
            {
                ctrlAxisEditor editor = (ctrlAxisEditor)sender;
                string dockpanelname = "Axis: (X) " + editor.Map_name + " [" + Path.GetFileName(editor.FileName) + "]";
                string dockpanelname2 = "Axis: (Y) " + editor.Map_name + " [" + Path.GetFileName(editor.FileName) + "]";
                
                foreach (DockPanel dp in _dockManager.Panels)
                {
                    if (dp.Text == dockpanelname || dp.Text == dockpanelname2)
                    {
                        _dockManager.RemovePanel(dp);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a viewer for the specified symbol is already active
        /// </summary>
        public bool CheckMapViewerActive(SymbolHelper sh, string currentFile)
        {
            bool retval = false;
            try
            {
                foreach (DockPanel pnl in _dockManager.Panels)
                {
                    if (pnl.Text == "Symbol: " + sh.Varname + " [" + Path.GetFileName(currentFile) + "]")
                    {
                        if (pnl.Tag.ToString() == currentFile)
                        {
                            if (IsSymbolDisplaySameAddress(sh, pnl))
                            {
                                retval = true;
                                pnl.Show();
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            return retval;
        }

        /// <summary>
        /// Checks if the symbol display is at the same address
        /// </summary>
        public bool IsSymbolDisplaySameAddress(SymbolHelper sh, DockPanel pnl)
        {
            bool retval = false;
            try
            {
                if (pnl.Text.StartsWith("Symbol: "))
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is MapViewerEx)
                        {
                            MapViewerEx vwr = (MapViewerEx)c;
                            if (vwr.Map_address == sh.Flash_start_address) retval = true;
                        }
                        else if (c is DockPanel)
                        {
                            DockPanel tpnl = (DockPanel)c;
                            foreach (Control c2 in tpnl.Controls)
                            {
                                if (c2 is MapViewerEx)
                                {
                                    MapViewerEx vwr2 = (MapViewerEx)c2;
                                    if (vwr2.Map_address == sh.Flash_start_address) retval = true;
                                }
                            }
                        }
                        else if (c is ControlContainer)
                        {
                            ControlContainer cntr = (ControlContainer)c;
                            foreach (Control c3 in cntr.Controls)
                            {
                                if (c3 is MapViewerEx)
                                {
                                    MapViewerEx vwr3 = (MapViewerEx)c3;
                                    if (vwr3.Map_address == sh.Flash_start_address) retval = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("IsSymbolDisplaySameAddress error: " + E.Message);
            }
            return retval;
        }

        /// <summary>
        /// Updates all open viewers for the specified file
        /// </summary>
        public void UpdateOpenViewers(string filename, SymbolCollection symbols)
        {
            try
            {
                foreach (DockPanel pnl in _dockManager.Panels)
                {
                    if (pnl.Text.StartsWith("Symbol: "))
                    {
                        foreach (Control c in pnl.Controls)
                        {
                            if (c is MapViewerEx)
                            {
                                MapViewerEx vwr = (MapViewerEx)c;
                                if (vwr.Filename == filename || filename == string.Empty)
                                {
                                    UpdateViewer(vwr, symbols);
                                }
                            }
                            else if (c is DockPanel)
                            {
                                DockPanel tpnl = (DockPanel)c;
                                foreach (Control c2 in tpnl.Controls)
                                {
                                    if (c2 is MapViewerEx)
                                    {
                                        MapViewerEx vwr2 = (MapViewerEx)c2;
                                        if (vwr2.Filename == filename || filename == string.Empty)
                                        {
                                            UpdateViewer(vwr2, symbols);
                                        }
                                    }
                                }
                            }
                            else if (c is ControlContainer)
                            {
                                ControlContainer cntr = (ControlContainer)c;
                                foreach (Control c3 in cntr.Controls)
                                {
                                    if (c3 is MapViewerEx)
                                    {
                                        MapViewerEx vwr3 = (MapViewerEx)c3;
                                        if (vwr3.Filename == filename || filename == string.Empty)
                                        {
                                            UpdateViewer(vwr3, symbols);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Refresh viewer error: " + E.Message);
            }
        }

        /// <summary>
        /// Updates a single viewer with fresh data
        /// </summary>
        public void UpdateViewer(MapViewerEx tabdet, SymbolCollection symbols)
        {
            string mapname = tabdet.Map_name;
            if (tabdet.Filename == Tools.Instance.m_currentfile)
            {
                foreach (SymbolHelper sh in symbols)
                {
                    if (sh.Varname == mapname)
                    {
                        tabdet.X_axis_name = sh.X_axis_descr;
                        tabdet.Y_axis_name = sh.Y_axis_descr;
                        tabdet.Z_axis_name = sh.Z_axis_descr;
                        tabdet.X_axisAddress = sh.Y_axis_address;
                        tabdet.Y_axisAddress = sh.X_axis_address;
                        tabdet.Xaxiscorrectionfactor = sh.X_axis_correction;
                        tabdet.Yaxiscorrectionfactor = sh.Y_axis_correction;
                        tabdet.Xaxiscorrectionoffset = sh.X_axis_offset;
                        tabdet.Yaxiscorrectionoffset = sh.Y_axis_offset;
                        tabdet.X_axisvalues = SymbolQueryHelper.GetXAxisValues(Tools.Instance.m_currentfile, symbols, tabdet.Map_name);
                        tabdet.Y_axisvalues = SymbolQueryHelper.GetYAxisValues(Tools.Instance.m_currentfile, symbols, tabdet.Map_name);
                        int columns = 8;
                        int rows = 8;
                        SymbolQueryHelper.GetTableDimensions(symbols, tabdet.Map_name, out columns, out rows);
                        int address = Convert.ToInt32(sh.Flash_start_address);
                        tabdet.ShowTable(columns, true);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Closes a viewer panel by name
        /// </summary>
        public void CloseViewer(string mapName, string filename)
        {
            string dockpanelname = "Symbol: " + mapName + " [" + Path.GetFileName(filename) + "]";
            string dockpanelname3 = "Symbol difference: " + mapName + " [" + Path.GetFileName(filename) + "]";
            foreach (DockPanel dp in _dockManager.Panels)
            {
                if (dp.Text == dockpanelname)
                {
                    _dockManager.RemovePanel(dp);
                    break;
                }
                else if (dp.Text == dockpanelname3)
                {
                    _dockManager.RemovePanel(dp);
                    break;
                }
            }
        }

        /// <summary>
        /// Closes the airmass result viewer
        /// </summary>
        public void CloseAirmassResultViewer(string currentFile)
        {
            string dockpanelname = "Airmass result viewer: " + Path.GetFileName(currentFile);
            foreach (DockPanel dp in _dockManager.Panels)
            {
                if (dp.Text == dockpanelname)
                {
                    _dockManager.RemovePanel(dp);
                    break;
                }
            }
        }

        /// <summary>
        /// Starts the airmass result viewer
        /// </summary>
        public void StartAirmassResult(string currentFile, SymbolCollection symbols, long currentfileSize)
        {
            DockPanel dockPanel;
            _dockManager.BeginUpdate();
            try
            {
                ctrlAirmassResult airmassResult = new ctrlAirmassResult();
                airmassResult.Dock = DockStyle.Fill;
                dockPanel = _dockManager.AddPanel(DockingStyle.Right);
                dockPanel.Tag = currentFile;
                dockPanel.Text = "Airmass result viewer: " + Path.GetFileName(currentFile);
                dockPanel.Width = 800;

                IEDCFileParser parser = Tools.Instance.GetParserForFile(currentFile, false);
                byte[] allBytes = File.ReadAllBytes(currentFile);
                string additionalInfo = parser.ExtractInfo(allBytes);
                string bpn = parser.ExtractBoschPartnumber(allBytes);
                partNumberConverter pnc = new partNumberConverter();
                ECUInfo info = pnc.ConvertPartnumber(bpn, allBytes.Length);
                
                airmassResult.NumberCylinders = pnc.GetNumberOfCylinders(info.EngineType, additionalInfo);
                airmassResult.ECUType = info.EcuType;
                airmassResult.Currentfile = currentFile;
                airmassResult.Symbols = symbols;
                airmassResult.Currentfile_size = (int)currentfileSize;
                airmassResult.Calculate(currentFile, symbols);
                dockPanel.Controls.Add(airmassResult);
            }
            catch (Exception newdockE)
            {
                Console.WriteLine(newdockE.Message);
            }
            _dockManager.EndUpdate();
        }

        /// <summary>
        /// Starts the hex viewer
        /// </summary>
        public void StartHexViewer(string currentFile, SymbolCollection symbols)
        {
            if (currentFile != "")
            {
                _dockManager.BeginUpdate();
                try
                {
                    DockPanel dockPanel;
                    if (!_appSettings.NewPanelsFloating)
                    {
                        dockPanel = _dockManager.AddPanel(DockingStyle.Right);
                    }
                    else
                    {
                        Point floatpoint = new Point(0, 0); // Default position
                        dockPanel = _dockManager.AddPanel(floatpoint);
                    }

                    dockPanel.Text = "Hexviewer: " + Path.GetFileName(currentFile);
                    HexViewer hv = new HexViewer();
                    hv.Issramviewer = false;
                    hv.Dock = DockStyle.Fill;
                    dockPanel.Width = 580;
                    hv.LoadDataFromFile(currentFile, symbols);
                    dockPanel.Controls.Add(hv);
                }
                catch (Exception E)
                {
                    Console.WriteLine(E.Message);
                }
                _dockManager.EndUpdate();
            }
        }

        /// <summary>
        /// Enum for axis type
        /// </summary>
        public enum Axis
        {
            XAxis,
            YAxis
        }

        /// <summary>
        /// Event handler for axis editor requests from MapViewerEx
        /// </summary>
        private void tabdet_onAxisEditorRequested(object sender, MapViewerEventArgs.AxisEditorRequestedEventArgs e)
        {
            // start axis editor
            foreach (SymbolHelper sh in Tools.Instance.m_symbols)
            {
                if (sh.Varname == e.Mapname)
                {
                    if (e.Axisident == AxisIdent.X_Axis)
                        StartAxisViewer(sh, Axis.XAxis, Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
                    else if (e.Axisident == AxisIdent.Y_Axis)
                        StartAxisViewer(sh, Axis.YAxis, Tools.Instance.m_currentfile, Tools.Instance.m_symbols);

                    break;
                }
            }
        }

        /// <summary>
        /// Event raised when axis data needs to be saved
        /// </summary>
        public event EventHandler<AxisSaveRequestedEventArgs> OnAxisSaveRequested;
    }

    /// <summary>
    /// Event arguments for axis save request
    /// </summary>
    public class AxisSaveRequestedEventArgs : EventArgs
    {
        public int AxisAddress { get; set; }
        public byte[] Data { get; set; }
        public string Filename { get; set; }
    }
}