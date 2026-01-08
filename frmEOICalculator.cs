using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Components;
using VAGSuite.Helpers;
using VAGSuite.Models;
using VAGSuite.Services;
using VAGSuite.Theming;
using Zuby.ADGV;

namespace VAGSuite
{
    public partial class frmEOICalculator : KryptonForm
    {
        private List<SymbolHelper> _allSOIMaps;
        private List<SymbolHelper> _allDurationMaps;
        private SymbolHelper _allSelectorMap;
        private List<int> _codeBanks;
        
        private int _selectedCodeBank = -1;
        private double _selectedTemperature = 90.0;
        private List<double> _availableTemperatures;
        
        private EOICalculationResult _currentResult;
        private int _lastTooltipRow = -1;
        private int _lastTooltipCol = -1;
        private Timer _tooltipTimer;
        private Point _lastMousePos;
        private bool _showTooltips = true;
        
        // 3D Chart Component
        private Chart3DComponent _chart3DComponent;
        
        public IEOICalculatorService CalculatorService { get; set; }
        
        public frmEOICalculator()
        {
            InitializeComponent();
            ApplyTheme();
            
            // Initialize tooltip button appearance
            UpdateTooltipButtonAppearance();
            
            // Initialize tooltip timer
            _tooltipTimer = new Timer();
            _tooltipTimer.Interval = 300;
            _tooltipTimer.Tick += TooltipTimer_Tick;
            
            // Wire up tooltip events
            dgvEOI.MouseMove += dgvEOI_MouseMove;
            dgvEOI.MouseLeave += (s, e) => {
                _tooltipTimer.Stop();
                _lastTooltipRow = -1;
                _lastTooltipCol = -1;
                TooltipService.Hide("EOI");
            };
            
            // Initialize 3D Chart Component
            Initialize3DChart();
        }
        
        private void Initialize3DChart()
        {
            try
            {
                Console.WriteLine("frmEOICalculator: Initializing 3D Chart Component...");
                
                // Create and configure the Chart3DComponent
                _chart3DComponent = new Chart3DComponent();
                _chart3DComponent.SetChartControl(glControl3D);
                
                // Wire up navigation button events
                btnZoomIn.Click += (s, e) => {
                    if (_chart3DComponent != null)
                    {
                        float rotation, elevation, zoom;
                        _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                        zoom *= 1.15f;
                        zoom = Math.Max(0.5f, Math.Min(5.0f, zoom));
                        _chart3DComponent.SetView(rotation, elevation, zoom);
                    }
                };
                
                btnZoomOut.Click += (s, e) => {
                    if (_chart3DComponent != null)
                    {
                        float rotation, elevation, zoom;
                        _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                        zoom *= 0.85f;
                        zoom = Math.Max(0.5f, Math.Min(5.0f, zoom));
                        _chart3DComponent.SetView(rotation, elevation, zoom);
                    }
                };
                
                btnRotateLeft.Click += (s, e) => {
                    if (_chart3DComponent != null)
                    {
                        float rotation, elevation, zoom;
                        _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                        rotation -= 15f;
                        _chart3DComponent.SetView(rotation, elevation, zoom);
                    }
                };
                
                btnRotateRight.Click += (s, e) => {
                    if (_chart3DComponent != null)
                    {
                        float rotation, elevation, zoom;
                        _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                        rotation += 15f;
                        _chart3DComponent.SetView(rotation, elevation, zoom);
                    }
                };
                
                btnToggleWireframe.Click += (s, e) => {
                    if (_chart3DComponent != null)
                    {
                        _chart3DComponent.ToggleRenderMode();
                        UpdateWireframeButtonState();
                    }
                };
                
                btnToggleTooltips3D.Click += (s, e) => {
                    if (_chart3DComponent != null)
                    {
                        _chart3DComponent.ToggleTooltips();
                        UpdateTooltips3DButtonState();
                    }
                };
                
                // Set initial button states
                UpdateWireframeButtonState();
                UpdateTooltips3DButtonState();
                
                Console.WriteLine("frmEOICalculator: 3D Chart Component initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"frmEOICalculator: Error initializing 3D chart: {ex.Message}");
            }
        }
        
        private void UpdateWireframeButtonState()
        {
            if (_chart3DComponent != null)
            {
                bool isWireframe = _chart3DComponent.IsWireframeMode;
                Color activeColor = Color.FromArgb(0, 122, 204);
                Color inactiveColor = Color.FromArgb(60, 60, 60);
                btnToggleWireframe.StateNormal.Back.Color1 = isWireframe ? activeColor : inactiveColor;
                btnToggleWireframe.Refresh();
            }
        }
        
        private void UpdateTooltips3DButtonState()
        {
            if (_chart3DComponent != null)
            {
                bool isTooltips = _chart3DComponent.IsTooltipsEnabled;
                Color activeColor = Color.FromArgb(0, 122, 204);
                Color inactiveColor = Color.FromArgb(60, 60, 60);
                btnToggleTooltips3D.StateNormal.Back.Color1 = isTooltips ? activeColor : inactiveColor;
                btnToggleTooltips3D.Refresh();
            }
        }
        
        private void ApplyTheme()
        {
            this.PaletteMode = PaletteMode.Custom;
            this.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
            
            // Apply Source Sans Pro to all controls
            Font customFont = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);
            this.Font = customFont;
            foreach (Control c in this.Controls)
            {
                c.Font = customFont;
            }
            
            // Specific theme for ADGV to fix white headers
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            dgvEOI.BackgroundColor = theme.GridBackground;
            dgvEOI.GridColor = theme.GridBorder;
            dgvEOI.DefaultCellStyle.BackColor = theme.GridBackground;
            dgvEOI.DefaultCellStyle.ForeColor = theme.TextPrimary;
            dgvEOI.DefaultCellStyle.SelectionBackColor = theme.GridSelection;
            dgvEOI.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvEOI.DefaultCellStyle.Font = customFont;

            dgvEOI.ColumnHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
            dgvEOI.ColumnHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
            dgvEOI.ColumnHeadersDefaultCellStyle.Font = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Bold);
            dgvEOI.EnableHeadersVisualStyles = false;

            dgvEOI.RowHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
            dgvEOI.RowHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
            dgvEOI.RowHeadersDefaultCellStyle.Font = customFont;
        }
        
        /// <summary>
        /// Original method for backward compatibility
        /// </summary>
        public void SetMaps(
            List<SymbolHelper> soiMaps,
            List<SymbolHelper> durationMaps,
            SymbolHelper selectorMap)
        {
            _allSOIMaps = soiMaps;
            _allDurationMaps = durationMaps;
            _allSelectorMap = selectorMap;
            _codeBanks = new List<int>();
            
            // Initialize UI
            InitializeUI();
        }
        
        /// <summary>
        /// New method with codebank support
        /// </summary>
        public void SetMapsWithCodeBankSupport(
            List<SymbolHelper> soiMaps,
            List<SymbolHelper> durationMaps,
            SymbolHelper selectorMap,
            List<int> codeBanks)
        {
            _allSOIMaps = soiMaps;
            _allDurationMaps = durationMaps;
            _allSelectorMap = selectorMap;
            _codeBanks = codeBanks;
            
            // Initialize UI with codebank support
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            // Populate codebank dropdown if multiple codebanks exist
            cmbCodeBank.Items.Clear();
            if (_codeBanks != null && _codeBanks.Count > 0)
            {
                foreach (int cb in _codeBanks)
                {
                    cmbCodeBank.Items.Add("Codebank " + cb);
                }
                
                if (_codeBanks.Count > 0)
                {
                    // Set selected index but don't trigger update yet if we are initializing
                    _selectedCodeBank = _codeBanks[0];
                    cmbCodeBank.SelectedIndex = 0;
                }
            }
            else
            {
                // No codebanks found via pattern - add a default one
                cmbCodeBank.Items.Add("Default Codebank");
                cmbCodeBank.SelectedIndex = 0;
                _selectedCodeBank = 0;
                cmbCodeBank.Visible = true;
                lblCodeBank.Visible = true;
            }
            
            // Update temperature slider based on selected codebank
            UpdateTemperatureSlider();
        }
        
        private void UpdateTemperatureSlider()
        {
            if (_selectedCodeBank < 0 || CalculatorService == null)
            {
                // Use default temperatures
                _availableTemperatures = new List<double> { -20, -10, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110 };
            }
            else
            {
                // Get actual temperatures from SOI maps
                _availableTemperatures = CalculatorService.GetSOITemperaturesForCodeBank(_selectedCodeBank);
                
                if (_availableTemperatures.Count == 0)
                {
                    _availableTemperatures = new List<double> { -20, -10, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110 };
                }
            }
            
            // Configure slider
            if (_availableTemperatures.Count > 1)
            {
                trackTemperature.Minimum = 0;
                trackTemperature.Maximum = _availableTemperatures.Count - 1;
                trackTemperature.Value = FindNearestTemperatureIndex(_selectedTemperature);
                trackTemperature.TickFrequency = 1;
                trackTemperature.Enabled = true;
                
                // Set large change to 1 to ensure we snap to discrete temperatures
                trackTemperature.LargeChange = 1;
                trackTemperature.SmallChange = 1;
                
                UpdateTemperatureLabel();
            }
            else
            {
                // Only one temperature available
                trackTemperature.Enabled = false;
                lblTemperature.Text = $"Temperature: {_availableTemperatures.FirstOrDefault():F0} °C";
            }
        }
        
        private int FindNearestTemperatureIndex(double targetTemp)
        {
            if (_availableTemperatures == null || _availableTemperatures.Count == 0)
                return 0;
            
            int nearestIndex = 0;
            double minDiff = Math.Abs(_availableTemperatures[0] - targetTemp);
            
            for (int i = 1; i < _availableTemperatures.Count; i++)
            {
                double diff = Math.Abs(_availableTemperatures[i] - targetTemp);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestIndex = i;
                }
            }
            
            return nearestIndex;
        }
        
        private void UpdateTemperatureLabel()
        {
            if (_availableTemperatures != null && trackTemperature.Value >= 0 && trackTemperature.Value < _availableTemperatures.Count)
            {
                _selectedTemperature = _availableTemperatures[trackTemperature.Value];
                lblTemperature.Text = $"Temperature: {_selectedTemperature:F0} °C";
            }
            else if (_availableTemperatures != null && _availableTemperatures.Count > 0)
            {
                _selectedTemperature = _availableTemperatures[0];
                lblTemperature.Text = $"Temperature: {_selectedTemperature:F0} °C";
            }
        }
        
        private SymbolHelper FindSOIMapForCurrentSettings()
        {
            if (_allSOIMaps == null)
            {
                return null;
            }
            
            // Filter by codebank first
            var mapsInBank = _allSOIMaps.Where(m => {
                int cb = EOIMapFinder.ExtractCodeBankStatic(m.Varname);
                if (cb < 0) cb = m.CodeBlock;
                return cb == _selectedCodeBank || _selectedCodeBank < 0;
            }).ToList();

            if (mapsInBank.Count == 0) return null;

            // Find exact temperature match
            foreach (var map in mapsInBank)
            {
                double temp = EOIMapFinder.ExtractTemperatureStatic(map.Varname);
                if (Math.Abs(temp - _selectedTemperature) < 0.1)
                {
                    return map;
                }
            }
            
            // Find closest temperature
            SymbolHelper closestMap = null;
            double minTempDiff = double.MaxValue;
            
            foreach (var map in mapsInBank)
            {
                double temp = EOIMapFinder.ExtractTemperatureStatic(map.Varname);
                double diff = Math.Abs(temp - _selectedTemperature);
                if (diff < minTempDiff)
                {
                    minTempDiff = diff;
                    closestMap = map;
                }
            }
            
            return closestMap;
        }
        
        private List<SymbolHelper> GetDurationMapsForCurrentSettings()
        {
            if (_allDurationMaps == null) return new List<SymbolHelper>();
            
            // If we have codebanks, filter by codebank
            if (_selectedCodeBank >= 0)
            {
                var result = new List<SymbolHelper>();
                foreach (var map in _allDurationMaps)
                {
                    int cb = EOIMapFinder.ExtractCodeBankStatic(map.Varname);
                    if (cb < 0) cb = map.CodeBlock; // Fallback to CodeBlock property
                    
                    if (cb == _selectedCodeBank)
                    {
                        result.Add(map);
                    }
                }
                return result;
            }
            
            return _allDurationMaps;
        }
        
        private SymbolHelper GetSelectorMapForCurrentSettings()
        {
            // If we have codebanks, try to find selector for the selected codebank
            if (_selectedCodeBank >= 0 && CalculatorService != null)
            {
                var selector = CalculatorService.GetSelectorMapForCodeBank(_selectedCodeBank);
                if (selector != null)
                {
                    return selector;
                }
            }
            
            // Fallback to the global selector if no codebank-specific one found
            return _allSelectorMap;
        }
        
        private void btnCalculate_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                // Ensure temperature is updated from slider before calculation
                UpdateTemperatureLabel();

                // Find maps for current settings
                var soiSymbol = FindSOIMapForCurrentSettings();
                if (soiSymbol == null)
                {
                    MessageBox.Show("No SOI map found for the selected temperature.", "No Map Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Load SOI map
                // IMPORTANT: EDC15P SOI maps have swapped axes compared to Duration maps!
                // SOI map in ECU: X-axis = IQ (mg/st), Y-axis = RPM
                // Duration map in ECU: X-axis = RPM, Y-axis = IQ (mg/st)
                //
                // To match Duration maps (RPM on X, IQ on Y), we need to:
                // - Read raw X-axis (IQ) and apply IQ correction -> this becomes our Y-axis
                // - Read raw Y-axis (RPM) and apply RPM correction -> this becomes our X-axis
                // - Transpose the values matrix so [iq_index, rpm_index] becomes [rpm_index, iq_index]
                // Load SOI map
                // Based on debug analysis:
                // X_axis_address points to RPM data (16 values) -> Use identity scaling
                // Y_axis_address points to IQ data (14 values) -> Use 0.01 scaling
                // Resulting map has X=RPM, Y=IQ, which matches Duration maps.
                // No transposition needed.
                var soiMap = LoadEOIMap(
                    soiSymbol,
                    val => val * -0.023437 + 78.0, // Values
                    val => val,                    // X-Axis raw = RPM
                    val => val * 0.01              // Y-Axis raw = IQ
                );
                
                Console.WriteLine("--- SOI Map (Used for Calc) ---");
                DumpMapToLog(soiMap);
                
                // Load duration maps
                var durationMapSet = new DurationMapSet();
                var durationSymbols = GetDurationMapsForCurrentSettings();
                
                if (durationSymbols != null)
                {
                    foreach (var durSymbol in durationSymbols)
                    {
                        // Duration maps:
                        // Based on debug analysis:
                        // X_axis_address points to IQ data (10 values) -> Use 0.01 scaling
                        // Y_axis_address points to RPM data (10 values) -> Use identity scaling
                        // Resulting map has X=IQ, Y=RPM.
                        // We MUST transpose to get X=RPM, Y=IQ.
                        var durMap = LoadEOIMap(
                            durSymbol,
                            val => val * 0.023437, // Values
                            val => val * 0.01,     // X-Axis raw = IQ
                            val => val             // Y-Axis raw = RPM
                        );
                        
                        if (durMap != null)
                        {
                            Console.WriteLine($"--- Raw Duration Map {durationMapSet.Count} (Before Transpose) ---");
                            DumpMapToLog(durMap);
                            
                            // Transpose to match X=RPM, Y=IQ convention
                            durMap = TransposeMap(durMap);
                            
                            Console.WriteLine($"--- Transposed Duration Map {durationMapSet.Count} (Used for Calc) ---");
                            DumpMapToLog(durMap);
                            
                            durationMapSet.AddMap(durMap);
                        }
                    }
                }
                
                // Load selector if available
                EOIMap selectorMap = null;
                var selectorSymbol = GetSelectorMapForCurrentSettings();
                if (selectorSymbol != null)
                {
                    // Selector map:
                    // X-Axis = SOI thresholds (needs SOI correction)
                    // Values = Map indices (needs division by 256 for EDC15)
                    selectorMap = LoadEOIMap(
                        selectorSymbol,
                        val => val / 256.0,           // Values are indices (stored as index * 256)
                        val => val * -0.023437 + 78.0, // X-Axis are SOI thresholds
                        val => val                    // Y-Axis (usually unused)
                    );
                    durationMapSet.Selector = selectorMap;
                }
                
                // Calculate EOI
                _currentResult = CalculatorService.CalculateEOI(
                    soiMap, durationMapSet, selectorMap);
                
                // Display results
                DisplayResults();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error calculating EOI:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Calculation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        
        /// <summary>
        /// Transposes an EOIMap to swap X and Y axes.
        /// Used to ensure all maps have consistent axes (RPM on X, IQ on Y).
        /// </summary>
        private EOIMap TransposeMap(EOIMap inputMap)
        {
            if (inputMap == null) return null;
            
            var transposedMap = new EOIMap
            {
                SourceSymbol = inputMap.SourceSymbol,
                // Swap axes: X becomes Y, Y becomes X
                XAxis = inputMap.YAxis,
                YAxis = inputMap.XAxis,
                Values = new double[inputMap.Values.GetLength(1), inputMap.Values.GetLength(0)]
            };
            
            // Transpose the values matrix (swap rows and columns)
            for (int i = 0; i < inputMap.Values.GetLength(0); i++)
            {
                for (int j = 0; j < inputMap.Values.GetLength(1); j++)
                {
                    transposedMap.Values[j, i] = inputMap.Values[i, j];
                }
            }
            
            return transposedMap;
        }
        
        private EOIMap LoadEOIMap(
            SymbolHelper symbol,
            Func<double, double> correctionFunc,
            Func<double, double> xAxisCorrectionFunc = null,
            Func<double, double> yAxisCorrectionFunc = null)
        {
            if (symbol == null) return null;
            
            // Use main correction func for axis if no specific one provided
            if (xAxisCorrectionFunc == null) xAxisCorrectionFunc = correctionFunc;
            if (yAxisCorrectionFunc == null) yAxisCorrectionFunc = correctionFunc;

            // Read map data from binary
            byte[] mapData;
            try
            {
                mapData = Tools.Instance.readdatafromfile(
                    Tools.Instance.m_currentfile,
                    (int)symbol.Flash_start_address,
                    symbol.Length,
                    Tools.Instance.m_currentFileType);
            }
            catch (Exception ex)
            {
                throw new IndexOutOfRangeException($"Error reading map data for {symbol.Varname} at address {symbol.Flash_start_address:X}: {ex.Message}", ex);
            }
            
            // Get axis values directly from addresses to avoid SymbolQueryHelper confusion
            int[] xAxisRaw;
            try
            {
                if (symbol.X_axis_address > 0)
                {
                    int len = symbol.X_axis_length;
                    // Handle 1D maps where length might be 1 but actual data is longer
                    if (len <= 1 && symbol.Length > 2 && symbol.Y_axis_length <= 1)
                    {
                        len = symbol.Length / 2;
                    }

                    xAxisRaw = Tools.Instance.readdatafromfileasint(
                        Tools.Instance.m_currentfile,
                        symbol.X_axis_address,
                        len,
                        Tools.Instance.m_currentFileType);
                }
                else
                {
                    // Fallback for maps without explicit axis address (rare in EDC15)
                    xAxisRaw = new int[Math.Max(1, symbol.X_axis_length)];
                }
            }
            catch (Exception ex)
            {
                throw new IndexOutOfRangeException($"Error reading X-axis for {symbol.Varname} at address {symbol.X_axis_address:X}: {ex.Message}", ex);
            }

            int[] yAxisRaw = null;
            try
            {
                if (symbol.Y_axis_length > 1 && symbol.Y_axis_address > 0)
                {
                    yAxisRaw = Tools.Instance.readdatafromfileasint(
                        Tools.Instance.m_currentfile,
                        symbol.Y_axis_address,
                        symbol.Y_axis_length,
                        Tools.Instance.m_currentFileType);
                }
            }
            catch (Exception ex)
            {
                throw new IndexOutOfRangeException($"Error reading Y-axis for {symbol.Varname} at address {symbol.Y_axis_address:X}: {ex.Message}", ex);
            }
            
            // Convert to double arrays
            double[] xAxis = new double[xAxisRaw.Length];
            for (int i = 0; i < xAxisRaw.Length; i++)
            {
                xAxis[i] = xAxisCorrectionFunc(xAxisRaw[i]);
            }
            
            double[] yAxis;
            if (yAxisRaw != null)
            {
                yAxis = new double[yAxisRaw.Length];
                for (int i = 0; i < yAxisRaw.Length; i++)
                {
                    yAxis[i] = yAxisCorrectionFunc(yAxisRaw[i]);
                }
            }
            else
            {
                yAxis = new double[] { 0 };
            }
            
            // Create values array
            int xCount = Math.Max(1, symbol.X_axis_length);
            int yCount = Math.Max(1, symbol.Y_axis_length);
            
            // For 1D maps (like the selector), ensure we use the correct dimension
            if (xCount == 1 && yCount == 1 && symbol.Length > 2)
            {
                xCount = symbol.Length / 2;
            }

            double[,] values = new double[xCount, yCount];
            
            // Parse map data (LoHi byte order for EDC15)
            int dataIndex = 0;
            for (int i = 0; i < xCount; i++)
            {
                for (int j = 0; j < yCount; j++)
                {
                    if (dataIndex + 1 < mapData.Length)
                    {
                        // EDC15 uses HiLo (Big Endian) for map data
                        int rawValue = (mapData[dataIndex] << 8) | mapData[dataIndex + 1];
                        
                        // Handle invalid/filler values (e.g., 0xFFFF, 0xFFEE)
                        // Values above 0xFB00 (64256) are typically filler in EDC15
                        if (rawValue > 0xFB00)
                        {
                            // If we have a previous valid value in this row, use it
                            if (j > 0) values[i, j] = values[i, j - 1];
                            else if (i > 0) values[i, j] = values[i - 1, j];
                            else values[i, j] = 0;
                        }
                        else
                        {
                            values[i, j] = correctionFunc(rawValue);
                        }
                        dataIndex += 2;
                    }
                    else
                    {
                        // Safety fallback if data is shorter than expected
                        values[i, j] = 0;
                    }
                }
            }

            // If the axis was read as 1x1 but we have more data, try to fix the axis
            if (xAxis.Length == 1 && xCount > 1)
            {
                xAxis = new double[xCount];
                for (int i = 0; i < xCount; i++) xAxis[i] = xAxisCorrectionFunc(0); // Fallback
            }
            
            return EOIMap.Create(xAxis, yAxis, values, symbol);
        }

        private void DumpMapToLog(EOIMap map)
        {
            if (map == null) return;
            
            Console.WriteLine($"Map: {map.SourceSymbol?.Varname ?? "Unknown"}");
            Console.WriteLine($"Size: {map.XCount}x{map.YCount}");
            
            Console.Write("X-Axis: ");
            if (map.XAxis != null)
                foreach (var val in map.XAxis) Console.Write($"{val:F2} ");
            Console.WriteLine();

            Console.Write("Y-Axis: ");
            if (map.YAxis != null)
                foreach (var val in map.YAxis) Console.Write($"{val:F2} ");
            Console.WriteLine();

            Console.WriteLine("Values (Top-Left 5x5):");
            int xLimit = Math.Min(5, map.XCount);
            int yLimit = Math.Min(5, map.YCount);
            
            for (int y = 0; y < yLimit; y++)
            {
                for (int x = 0; x < xLimit; x++)
                {
                    Console.Write($"{map.Values[x, y]:F2}\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine("----------------------------------------");
        }
        
        private void DisplayResults()
        {
            if (_currentResult == null) return;
            
            // Populate data grid
            PopulateDataGrid();
            
            // Update 3D visualization
            Update3DVisualization();
        }
        
        private void Update3DVisualization()
        {
            if (_chart3DComponent == null || _currentResult == null || _currentResult.EOIMap == null)
            {
                Console.WriteLine("frmEOICalculator: Cannot update 3D visualization - chart or data is null");
                return;
            }
            
            try
            {
                Console.WriteLine($"frmEOICalculator: Updating 3D visualization with EOI map...");
                
                // Create metadata for the EOI map
                var metadata = new MapMetadata
                {
                    Name = $"EOI Map ({_selectedTemperature:F0}°C)",
                    Description = "End of Injection calculation result",
                    XAxisName = "RPM",
                    YAxisName = "IQ (mg/st)",
                    ZAxisName = "EOI (degrees)"
                };
                
                // Load EOI data into the 3D chart
                _chart3DComponent.LoadEOIData(_currentResult.EOIMap, metadata);
                
                Console.WriteLine("frmEOICalculator: 3D visualization updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"frmEOICalculator: Error updating 3D visualization: {ex.Message}");
            }
        }
        
        private void PopulateDataGrid()
        {
            dgvEOI.Rows.Clear();
            dgvEOI.Columns.Clear();
            
            var eoiMap = _currentResult.EOIMap;
            if (eoiMap == null) return;
            
            // Add columns for each RPM value
            for (int i = 0; i < eoiMap.XAxis.Length; i++)
            {
                var col = new DataGridViewTextBoxColumn();
                col.Name = "col" + i;
                col.HeaderText = eoiMap.XAxis[i].ToString("F0");
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvEOI.Columns.Add(col);
            }
            
            // Add rows for each IQ value
            for (int j = eoiMap.YAxis.Length - 1; j >= 0; j--)
            {
                var row = new DataGridViewRow();
                row.HeaderCell.Value = eoiMap.YAxis[j].ToString("F1");
                
                for (int i = 0; i < eoiMap.XAxis.Length; i++)
                {
                    var cell = new DataGridViewTextBoxCell();
                    
                    // Safety check for values array bounds
                    if (i < eoiMap.Values.GetLength(0) && j < eoiMap.Values.GetLength(1))
                    {
                        cell.Value = eoiMap.Values[i, j].ToString("F2");
                        
                        // Color code based on EOI value
                        double eoiValue = eoiMap.Values[i, j];
                        cell.Style.BackColor = GetEOIColor(eoiValue);
                        cell.Style.ForeColor = Color.White;
                    }
                    else
                    {
                        cell.Value = "N/A";
                    }
                    
                    row.Cells.Add(cell);
                }
                
                dgvEOI.Rows.Add(row);
            }

            // Ensure row headers are also sized correctly
            dgvEOI.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }
        
        private Color GetEOIColor(double eoiValue)
        {
            if (eoiValue > 5.0)
                return Color.FromArgb(0, 100, 0);      // Dark green
            else if (eoiValue > 0.0)
                return Color.FromArgb(0, 150, 0);      // Green
            else if (eoiValue > -5.0)
                return Color.FromArgb(150, 150, 0);    // Yellow
            else if (eoiValue > -10.0)
                return Color.FromArgb(200, 100, 0);    // Orange
            else
                return Color.FromArgb(150, 0, 0);      // Red
        }
        
        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            if (_currentResult == null || _currentResult.EOIMap == null)
            {
                MessageBox.Show("Please calculate EOI first.", "No Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                sfd.FileName = $"EOI_Map_{_selectedTemperature:F0}C.csv";
                
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var eoiMap = _currentResult.EOIMap;
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                        {
                            // Write header with metadata
                            sw.WriteLine($"# EOI Map - Temperature: {_selectedTemperature:F0} °C");
                            if (_selectedCodeBank >= 0)
                            {
                                sw.WriteLine($"# Codebank: {_selectedCodeBank}");
                            }
                            sw.Write("RPM\\IQ");
                            
                            for (int j = 0; j < eoiMap.YAxis.Length; j++)
                            {
                                sw.Write("," + eoiMap.YAxis[j].ToString("F1"));
                            }
                            sw.WriteLine();
                            
                            // Write data
                            for (int i = 0; i < eoiMap.XAxis.Length; i++)
                            {
                                sw.Write(eoiMap.XAxis[i].ToString("F0"));
                                for (int j = 0; j < eoiMap.YAxis.Length; j++)
                                {
                                    sw.Write("," + eoiMap.Values[i, j].ToString("F2"));
                                }
                                sw.WriteLine();
                            }
                        }
                        
                        MessageBox.Show("Export completed successfully.", "Export",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting: " + ex.Message, "Export Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void cmbCodeBank_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCodeBank.SelectedIndex >= 0 && _codeBanks != null && cmbCodeBank.SelectedIndex < _codeBanks.Count)
            {
                _selectedCodeBank = _codeBanks[cmbCodeBank.SelectedIndex];
                
                // Update temperature slider for new codebank
                UpdateTemperatureSlider();
                
                // Clear current results
                _currentResult = null;
                dgvEOI.Rows.Clear();
            }
        }
        
        private void trackTemperature_Scroll(object sender, EventArgs e)
        {
            UpdateTemperatureLabel();
            
            // Optionally auto-calculate when temperature changes
            // btnCalculate_Click(sender, e);
        }

        private void dgvEOI_MouseMove(object sender, MouseEventArgs e)
        {
            if (_currentResult == null || _currentResult.EOIMap == null) return;

            var hit = dgvEOI.HitTest(e.X, e.Y);
            if (hit.Type == DataGridViewHitTestType.Cell && hit.RowIndex >= 0 && hit.ColumnIndex >= 0)
            {
                if (hit.RowIndex != _lastTooltipRow || hit.ColumnIndex != _lastTooltipCol)
                {
                    // Cell changed, restart timer
                    _tooltipTimer.Stop();
                    TooltipService.Hide("EOI");
                    
                    _lastTooltipRow = hit.RowIndex;
                    _lastTooltipCol = hit.ColumnIndex;
                    
                    // Offset tooltip position to prevent it from appearing directly under the cursor
                    // Using a larger offset (30px) ensures the tooltip doesn't trigger MouseLeave on the grid
                    _lastMousePos = new Point(e.X + 30, e.Y + 30);
                    _tooltipTimer.Start();
                }
            }
            else
            {
                if (_lastTooltipRow != -1)
                {
                    _tooltipTimer.Stop();
                    _lastTooltipRow = -1;
                    _lastTooltipCol = -1;
                    TooltipService.Hide("EOI");
                }
            }
        }

        private void btnToggleTooltips_Click(object sender, EventArgs e)
        {
            _showTooltips = !_showTooltips;
            UpdateTooltipButtonAppearance();
            
            // Hide tooltip immediately if tooltips are disabled
            if (!_showTooltips)
            {
                TooltipService.Hide("EOI");
            }
        }
        
        private void UpdateTooltipButtonAppearance()
        {
            Color activeBackColor = Color.FromArgb(14, 99, 156); // VAGEDC Dark blue
            Color inactiveBackColor = Color.FromArgb(60, 60, 60); // Krypton group box background
            Color textColor = Color.FromArgb(220, 220, 220);
            
            if (_showTooltips)
            {
                btnToggleTooltips.BackColor = activeBackColor;
                btnToggleTooltips.ForeColor = Color.White;
                btnToggleTooltips.Values.Text = "Tooltips: ON";
            }
            else
            {
                btnToggleTooltips.BackColor = inactiveBackColor;
                btnToggleTooltips.ForeColor = textColor;
                btnToggleTooltips.Values.Text = "Tooltips: OFF";
            }
            
            btnToggleTooltips.Invalidate();
        }
        
        private void TooltipTimer_Tick(object sender, EventArgs e)
        {
            _tooltipTimer.Stop();
            
            // Check if tooltips are enabled
            if (!_showTooltips) return;
            
            if (_currentResult == null || _currentResult.EOIMap == null) return;
            if (_lastTooltipRow < 0 || _lastTooltipCol < 0) return;

            int colIndex = _lastTooltipCol;
            int rowIndex = _lastTooltipRow;
            
            // Map grid row index to Y-axis index (rows are reversed in PopulateDataGrid)
            int yCount = _currentResult.EOIMap.YCount;
            int yIndex = (yCount - 1) - rowIndex;
            int xIndex = colIndex;

            // Safety check: Ensure indices are within the bounds of the Values array for all maps
            if (xIndex >= 0 && xIndex < _currentResult.EOIMap.Values.GetLength(0) &&
                yIndex >= 0 && yIndex < _currentResult.EOIMap.Values.GetLength(1) &&
                xIndex < _currentResult.SOIMap.Values.GetLength(0) &&
                yIndex < _currentResult.SOIMap.Values.GetLength(1) &&
                xIndex < _currentResult.ActualDurationMap.Values.GetLength(0) &&
                yIndex < _currentResult.ActualDurationMap.Values.GetLength(1))
            {
                double eoi = _currentResult.EOIMap.Values[xIndex, yIndex];
                double soi = _currentResult.SOIMap.Values[xIndex, yIndex];
                double duration = _currentResult.ActualDurationMap.Values[xIndex, yIndex];
                
                string soiMapName = _currentResult.SOIMap.SourceSymbol?.Varname ?? "Unknown SOI Map";
                string durationUsage = _currentResult.GetDurationMapUsageAt(xIndex, yIndex);
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"SOI: {soi:F2}° ({soiMapName})");
                sb.AppendLine($"Duration Formula: {durationUsage}");
                sb.AppendLine($"Duration Result: {duration:F2}°");
                sb.AppendLine($"Formula: EOI = SOI - Duration");
                sb.AppendLine($"Result: {eoi:F2}°");

                TooltipService.ShowForControl(dgvEOI, _lastMousePos, "Calculation Details", sb.ToString(), "EOI");
            }
        }
    }
}
