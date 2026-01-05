using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ComponentFactory.Krypton.Docking;
using ComponentFactory.Krypton.Navigator;
using VAGSuite.Helpers;
using VAGSuite.MapViewerEventArgs;

namespace VAGSuite.Services
{
    /// <summary>
    /// Coordinates creation, display, and management of map viewer panels.
    /// Extracted from frmMain to separate map viewer logic from main form.
    /// </summary>
    public class MapViewerCoordinator
    {
        private readonly KryptonDockingManager _dockingManager;
        private readonly AppSettings _appSettings;

        public MapViewerCoordinator(KryptonDockingManager dockingManager, AppSettings appSettings)
        {
            _dockingManager = dockingManager;
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
        public KryptonPage ShowMapViewer(MapViewerParams parameters)
        {
            if (parameters.Symbol == null)
                throw new ArgumentNullException(nameof(parameters.Symbol));

            if (parameters.Symbol.Flash_start_address == 0 && parameters.Symbol.Start_address == 0)
                return null;

            // Check if viewer already exists
            if (CheckMapViewerActive(parameters.Symbol, parameters.Filename))
                return null;

            KryptonPage page = null;

            try
            {
                MapViewerEx viewer = CreateMapViewer(parameters);
                page = CreateDockPage(parameters, viewer);
                
                // Subscribe to axis editor event - FIX: was missing after refactor
                viewer.onAxisEditorRequested += tabdet_onAxisEditorRequested;

                page.Controls.Add(viewer);
                viewer.Visible = true;

                if (!_appSettings.NewPanelsFloating)
                {
                    _dockingManager.AddToWorkspace("Workspace", new KryptonPage[] { page });
                }
                else
                {
                    _dockingManager.AddFloatingWindow("Floating", new KryptonPage[] { page });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating map viewer: {ex.Message}");
            }

            return page;
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
        /// Creates and configures the dock page for the viewer.
        /// </summary>
        private KryptonPage CreateDockPage(MapViewerParams parameters, MapViewerEx viewer)
        {
            KryptonPage page = new KryptonPage();
            page.Tag = parameters.Filename;
            page.Text = $"Symbol: {viewer.Map_name} [{Path.GetFileName(parameters.Filename)}]";
            page.UniqueName = "SymbolViewer_" + viewer.Map_name + "_" + Guid.NewGuid().ToString("N");

            // Set width based on columns
            int width = CalculatePanelWidth(viewer.X_axisvalues.Length);
            // KryptonPage doesn't have Width/FloatSize directly, these are managed by the docking system
            
            viewer.Dock = DockStyle.Fill;

            return page;
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
                foreach (KryptonPage page in _dockingManager.Pages)
                {
                    if (page.Text == $"Symbol: {symbol.Varname} [{Path.GetFileName(filename)}]")
                    {
                        if (page.Tag?.ToString() == filename)
                        {
                            if (IsSymbolDisplaySameAddress(symbol, page))
                            {
                                // In Krypton, we'd need to find the cell and select the page
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
        /// Checks if a page displays a symbol at the same address.
        /// </summary>
        private bool IsSymbolDisplaySameAddress(SymbolHelper symbol, KryptonPage page)
        {
            try
            {
                if (page.Text.StartsWith("Symbol: "))
                {
                    foreach (Control c in page.Controls)
                    {
                        if (c is MapViewerEx viewer)
                        {
                            if (viewer.Map_address == symbol.Flash_start_address)
                                return true;
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
                foreach (KryptonPage page in _dockingManager.Pages)
                {
                    if (page.Text.StartsWith("Symbol: "))
                    {
                        UpdateViewersInPage(page, filename);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating viewers: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates viewers within a dock page.
        /// </summary>
        private void UpdateViewersInPage(KryptonPage page, string filename)
        {
            foreach (Control c in page.Controls)
            {
                if (c is MapViewerEx viewer && (viewer.Filename == filename || filename == string.Empty))
                {
                    UpdateViewer(viewer);
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

            foreach (KryptonPage page in _dockingManager.Pages)
            {
                if (page.Text == panelName || page.Text == panelName2)
                {
                    page.Dispose();
                    break;
                }
            }
        }

        /// <summary>
        /// Event handler for axis editor requests from MapViewerEx
        /// </summary>
        private void tabdet_onAxisEditorRequested(object sender, AxisEditorRequestedEventArgs e)
        {
            // start axis editor
            foreach (SymbolHelper sh in Tools.Instance.m_symbols)
            {
                if (sh.Varname == e.Mapname)
                {
                    if (e.Axisident == AxisIdent.X_Axis)
                        StartAxisViewer(sh, Axis.XAxis);
                    else if (e.Axisident == AxisIdent.Y_Axis)
                        StartAxisViewer(sh, Axis.YAxis);

                    break;
                }
            }
        }

        /// <summary>
        /// Starts an axis viewer for editing axis values
        /// </summary>
        private void StartAxisViewer(SymbolHelper symbol, Axis axisToShow)
        {
            // This method delegates to MapViewerService.StartAxisViewer
            // The service instance is not available here, so we use the event-based approach
            // or the caller should handle this. For now, we'll throw an event that can be handled externally.
            OnAxisEditorRequested?.Invoke(this, new AxisEditorRequestEventArgs
            {
                Symbol = symbol,
                AxisToShow = axisToShow,
                Filename = Tools.Instance.m_currentfile,
                Symbols = Tools.Instance.m_symbols
            });
        }

        /// <summary>
        /// Event raised when axis editor is requested
        /// </summary>
        public event EventHandler<AxisEditorRequestEventArgs> OnAxisEditorRequested;

        /// <summary>
        /// Enum for axis type
        /// </summary>
        public enum Axis
        {
            XAxis,
            YAxis
        }
    }

    /// <summary>
    /// Event arguments for axis editor request
    /// </summary>
    public class AxisEditorRequestEventArgs : EventArgs
    {
        public SymbolHelper Symbol { get; set; }
        public MapViewerCoordinator.Axis AxisToShow { get; set; }
        public string Filename { get; set; }
        public SymbolCollection Symbols { get; set; }
    }
}
