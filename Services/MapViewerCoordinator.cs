using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraBars.Docking;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    /// <summary>
    /// Coordinates creation, display, and management of map viewer panels.
    /// Extracted from frmMain to separate map viewer logic from main form.
    /// </summary>
    public class MapViewerCoordinator
    {
        private readonly DockManager _dockManager;
        private readonly AppSettings _appSettings;

        public MapViewerCoordinator(DockManager dockManager, AppSettings appSettings)
        {
            _dockManager = dockManager ?? throw new ArgumentNullException(nameof(dockManager));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        /// <summary>
        /// Parameters for creating a map viewer
        /// </summary>
        public class MapViewerParams
        {
            public SymbolHelper Symbol { get; set; }
            public string Filename { get; set; }
            public SymbolCollection Symbols { get; set; }
            public bool IsCompareMode { get; set; }
            public string CompareFilename { get; set; }
        }

        /// <summary>
        /// Shows a map viewer for the specified symbol.
        /// </summary>
        public DockPanel ShowMapViewer(MapViewerParams parameters)
        {
            if (parameters.Symbol == null)
                throw new ArgumentNullException(nameof(parameters.Symbol));

            if (parameters.Symbol.Flash_start_address == 0 && parameters.Symbol.Start_address == 0)
                return null;

            // Check if viewer already exists
            if (CheckMapViewerActive(parameters.Symbol, parameters.Filename))
                return null;

            _dockManager.BeginUpdate();
            DockPanel dockPanel = null;

            try
            {
                MapViewerEx viewer = CreateMapViewer(parameters);
                dockPanel = CreateDockPanel(parameters, viewer);
                
                dockPanel.Controls.Add(viewer);
                viewer.Visible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating map viewer: {ex.Message}");
            }
            finally
            {
                _dockManager.EndUpdate();
            }

            return dockPanel;
        }

        /// <summary>
        /// Creates the MapViewerEx control with all settings.
        /// </summary>
        private MapViewerEx CreateMapViewer(MapViewerParams parameters)
        {
            var viewer = new MapViewerEx
            {
                AutoUpdateIfSRAM = false,
                AutoUpdateInterval = 99999,
                Visible = false,
                Filename = parameters.Filename,
                GraphVisible = _appSettings.ShowGraphs,
                Viewtype = _appSettings.DefaultViewType,
                DisableColors = _appSettings.DisableMapviewerColors,
                AutoSizeColumns = _appSettings.AutoSizeColumnsInWindows,
                IsRedWhite = _appSettings.ShowRedWhite,
                IsUpsideDown = _appSettings.ShowTablesUpsideDown,
                Map_name = parameters.Symbol.Varname,
                Map_descr = parameters.Symbol.Varname,
                Map_cat = XDFCategories.Undocumented
            };

            viewer.SetViewSize(_appSettings.DefaultViewSize);

            // Set axis information
            SetAxisInformation(viewer, parameters.Symbol, parameters.Filename, parameters.Symbols);

            // Load map data
            LoadMapData(viewer, parameters.Symbol);

            return viewer;
        }

        /// <summary>
        /// Sets axis information for the map viewer.
        /// </summary>
        private void SetAxisInformation(MapViewerEx viewer, SymbolHelper symbol, string filename, SymbolCollection symbols)
        {
            viewer.X_axis_name = symbol.X_axis_descr;
            viewer.Y_axis_name = symbol.Y_axis_descr;
            viewer.Z_axis_name = symbol.Z_axis_descr;
            viewer.XaxisUnits = symbol.XaxisUnits;
            viewer.YaxisUnits = symbol.YaxisUnits;
            viewer.X_axisAddress = symbol.Y_axis_address;
            viewer.Y_axisAddress = symbol.X_axis_address;
            viewer.Xaxiscorrectionfactor = symbol.X_axis_correction;
            viewer.Yaxiscorrectionfactor = symbol.Y_axis_correction;
            viewer.Xaxiscorrectionoffset = symbol.X_axis_offset;
            viewer.Yaxiscorrectionoffset = symbol.Y_axis_offset;

            viewer.X_axisvalues = SymbolQueryHelper.GetXAxisValues(filename, symbols, viewer.Map_name);
            viewer.Y_axisvalues = SymbolQueryHelper.GetYAxisValues(filename, symbols, viewer.Map_name);
        }

        /// <summary>
        /// Loads map data into the viewer.
        /// </summary>
        private void LoadMapData(MapViewerEx viewer, SymbolHelper symbol)
        {
            int columns, rows;
            SymbolQueryHelper.GetTableDimensions(Tools.Instance.m_symbols, viewer.Map_name, out columns, out rows);

            int address = (int)symbol.Flash_start_address;
            if (address != 0)
            {
                viewer.Map_address = address;
                viewer.Map_sramaddress = 0;
                viewer.Map_length = symbol.Length;

                byte[] mapdata = Tools.Instance.readdatafromfile(
                    viewer.Filename, address, symbol.Length, Tools.Instance.m_currentFileType);

                viewer.Map_content = mapdata;
                viewer.Correction_factor = symbol.Correction;
                viewer.Correction_offset = symbol.Offset;
                viewer.ShowTable(columns, true);
            }
        }

        /// <summary>
        /// Creates and configures the dock panel for the viewer.
        /// </summary>
        private DockPanel CreateDockPanel(MapViewerParams parameters, MapViewerEx viewer)
        {
            DockPanel dockPanel;

            if (!_appSettings.NewPanelsFloating)
            {
                dockPanel = _dockManager.AddPanel(DockingStyle.Right);
            }
            else
            {
                dockPanel = _dockManager.AddPanel(new Point(-500, -500));
            }

            dockPanel.Tag = parameters.Filename;
            dockPanel.Text = $"Symbol: {viewer.Map_name} [{Path.GetFileName(parameters.Filename)}]";

            // Set width based on columns
            int width = CalculatePanelWidth(viewer.X_axisvalues.Length);
            dockPanel.Width = width;
            dockPanel.FloatSize = new Size(width, 900);

            viewer.Dock = DockStyle.Fill;

            return dockPanel;
        }

        /// <summary>
        /// Calculates appropriate panel width based on number of columns.
        /// </summary>
        private int CalculatePanelWidth(int columnCount)
        {
            int width = 500;
            
            if (columnCount > 0)
            {
                width = 30 + ((columnCount + 1) * 45);
            }
            
            if (width < 400) width = 400;
            if (width > 800) width = 800;
            
            return width;
        }

        /// <summary>
        /// Checks if a map viewer is already active for this symbol.
        /// </summary>
        public bool CheckMapViewerActive(SymbolHelper symbol, string filename)
        {
            try
            {
                foreach (DockPanel panel in _dockManager.Panels)
                {
                    if (panel.Text == $"Symbol: {symbol.Varname} [{Path.GetFileName(filename)}]")
                    {
                        if (panel.Tag?.ToString() == filename)
                        {
                            if (IsSymbolDisplaySameAddress(symbol, panel))
                            {
                                panel.Show();
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking map viewer: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Checks if a panel displays a symbol at the same address.
        /// </summary>
        private bool IsSymbolDisplaySameAddress(SymbolHelper symbol, DockPanel panel)
        {
            try
            {
                if (panel.Text.StartsWith("Symbol: "))
                {
                    foreach (Control c in panel.Controls)
                    {
                        if (c is MapViewerEx viewer)
                        {
                            if (viewer.Map_address == symbol.Flash_start_address)
                                return true;
                        }
                        else if (c is DockPanel childPanel)
                        {
                            foreach (Control c2 in childPanel.Controls)
                            {
                                if (c2 is MapViewerEx viewer2)
                                {
                                    if (viewer2.Map_address == symbol.Flash_start_address)
                                        return true;
                                }
                            }
                        }
                        else if (c is ControlContainer container)
                        {
                            foreach (Control c3 in container.Controls)
                            {
                                if (c3 is MapViewerEx viewer3)
                                {
                                    if (viewer3.Map_address == symbol.Flash_start_address)
                                        return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking symbol address: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Updates all open map viewers for a specific file.
        /// </summary>
        public void UpdateOpenViewers(string filename)
        {
            try
            {
                foreach (DockPanel panel in _dockManager.Panels)
                {
                    if (panel.Text.StartsWith("Symbol: "))
                    {
                        UpdateViewersInPanel(panel, filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating viewers: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates viewers within a dock panel.
        /// </summary>
        private void UpdateViewersInPanel(DockPanel panel, string filename)
        {
            foreach (Control c in panel.Controls)
            {
                if (c is MapViewerEx viewer && (viewer.Filename == filename || filename == string.Empty))
                {
                    UpdateViewer(viewer);
                }
                else if (c is DockPanel childPanel)
                {
                    UpdateViewersInPanel(childPanel, filename);
                }
                else if (c is ControlContainer container)
                {
                    foreach (Control c3 in container.Controls)
                    {
                        if (c3 is MapViewerEx viewer3 && (viewer3.Filename == filename || filename == string.Empty))
                        {
                            UpdateViewer(viewer3);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates a single map viewer with fresh data.
        /// </summary>
        private void UpdateViewer(MapViewerEx viewer)
        {
            string mapname = viewer.Map_name;
            
            if (viewer.Filename != Tools.Instance.m_currentfile)
                return;

            foreach (SymbolHelper sh in Tools.Instance.m_symbols)
            {
                if (sh.Varname == mapname)
                {
                    SetAxisInformation(viewer, sh, viewer.Filename, Tools.Instance.m_symbols);

                    int columns, rows;
                    SymbolQueryHelper.GetTableDimensions(Tools.Instance.m_symbols, mapname, out columns, out rows);
                    
                    viewer.ShowTable(columns, true);
                    break;
                }
            }
        }

        /// <summary>
        /// Closes a map viewer by symbol name.
        /// </summary>
        public void CloseMapViewer(string symbolName, string filename)
        {
            string panelName = $"Symbol: {symbolName} [{Path.GetFileName(filename)}]";
            string panelName2 = $"Symbol difference: {symbolName} [{Path.GetFileName(filename)}]";

            foreach (DockPanel panel in _dockManager.Panels)
            {
                if (panel.Text == panelName || panel.Text == panelName2)
                {
                    _dockManager.RemovePanel(panel);
                    break;
                }
            }
        }
    }
}
