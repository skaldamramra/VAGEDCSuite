using System;
using Nevron.Chart.WinForm;
using VAGSuite.MapViewerEventArgs;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Nevron.Chart;
using Nevron.GraphicsCore;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Components
{
    /// <summary>
    /// Encapsulates the 3D surface chart for map visualization.
    /// </summary>
    public class Chart3DComponent : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private readonly IChartService _chartService;
        
        // Reference to the external chart control (provided by MapViewerEx designer)
        private Nevron.Chart.WinForm.NChartControl _externalChartControl;
        
        // State references
        private int _tableWidth;
        private bool _isSixteenBit;
        private ViewType _viewType;
        private string _mapName;
        private string _xAxisName;
        private string _yAxisName;
        private string _zAxisName;
        private byte[] _mapContent;
        private byte[] _originalContent;
        private byte[] _compareContent;
        private int[] _xAxisValues;
        private int[] _yAxisValues;
        private double _correctionFactor;
        private double _correctionOffset;
        private bool _isCompareViewer;
        private bool _onlineMode;
        private bool _overlayVisible;
        private bool _isUpsideDown;

        #endregion

        #region Events

        public event EventHandler<SurfaceGraphViewChangedEventArgsEx> ViewChanged;
        public event EventHandler RefreshRequested;

        #endregion

        #region Constructors

        public Chart3DComponent()
        {
            _chartService = new ChartService();
        }

        public Chart3DComponent(IChartService chartService)
        {
            _chartService = chartService ?? throw new ArgumentNullException("chartService");
        }

        /// <summary>
        /// Sets the external NChartControl to use instead of creating a new one.
        /// This allows the component to share the designer's chart control.
        /// </summary>
        public void SetChartControl(Nevron.Chart.WinForm.NChartControl externalChart)
        {
            _externalChartControl = externalChart;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads data into the 3D chart
        /// </summary>
        public void LoadData(MapViewerState state)
        {
            _tableWidth = state.Data.TableWidth;
            _isSixteenBit = state.Data.IsSixteenBit;
            _viewType = state.Configuration.ViewType;
            _mapName = state.Metadata.Name;
            _xAxisName = state.Metadata.XAxisName;
            _yAxisName = state.Metadata.YAxisName;
            _zAxisName = state.Metadata.ZAxisName;
            _mapContent = state.Data.Content;
            _originalContent = state.Data.OriginalContent;
            _compareContent = state.Data.CompareContent;
            _xAxisValues = state.Axes.XAxisValues;
            _yAxisValues = state.Axes.YAxisValues;
            _correctionFactor = state.Configuration.CorrectionFactor;
            _correctionOffset = state.Configuration.CorrectionOffset;
            _isCompareViewer = state.IsCompareMode;
            _onlineMode = state.IsOnlineMode;
            _overlayVisible = true;
            _isUpsideDown = state.Configuration.IsUpsideDown; // Load IsUpsideDown from state

            ConfigureChart();
        }

        /// <summary>
        /// Initializes the 3D chart with basic settings and series.
        /// </summary>
        public void InitializeChart3D()
        {
            var chartControl = GetChartControl();
            if (chartControl == null) return;

            chartControl.Legends.Clear();

            NChart chart = chartControl.Charts[0];

            // Configure for 3D
            chart.Enable3D = true;
            chart.Width = 60.0f;
            chart.Depth = 60.0f;
            chart.Height = 35.0f;
            chart.Projection.SetPredefinedProjection(PredefinedProjection.PerspectiveTilted);
            chart.LightModel.SetPredefinedLightModel(PredefinedLightModel.ShinyTopLeft);

            // Set title
            NLabel title = chartControl.Labels.AddHeader(_mapName);
            title.TextStyle.FontStyle = new NFontStyle("Times New Roman", 18, FontStyle.Italic);
            title.TextStyle.FillStyle = new NColorFillStyle(Color.FromArgb(68, 90, 108));

            // Configure axes
            ConfigureAxis(chart);

            // Add main surface series
            NMeshSurfaceSeries surface = null;
            if (chart.Series.Count == 0)
            {
                surface = (NMeshSurfaceSeries)chart.Series.Add(SeriesType.MeshSurface);
            }
            else
            {
                surface = (NMeshSurfaceSeries)chart.Series[0];
            }
            ConfigureSurface(surface);

            // Add original map overlay if available and NOT in Easy view
            // Easy view scaling makes the original content (unscaled) appear as a ghost mesh at the bottom
            if (_originalContent != null && _viewType != ViewType.Easy)
            {
                NMeshSurfaceSeries surface2 = null;
                if (chart.Series.Count == 1)
                {
                    surface2 = (NMeshSurfaceSeries)chart.Series.Add(SeriesType.MeshSurface);
                }
                else
                {
                    surface2 = (NMeshSurfaceSeries)chart.Series[1];
                }
                ConfigureOverlay(surface2, Color.YellowGreen, _originalContent);

                // Add compare overlay if available
                if (_compareContent != null)
                {
                    NMeshSurfaceSeries surface3 = null;
                    if (chart.Series.Count == 2)
                    {
                        surface3 = (NMeshSurfaceSeries)chart.Series.Add(SeriesType.MeshSurface);
                    }
                    else
                    {
                        surface3 = (NMeshSurfaceSeries)chart.Series[2];
                    }
                    ConfigureOverlay(surface3, Color.BlueViolet, _compareContent);
                }
            }
            else
            {
                // Clear overlays if in Easy view or no content
                while (chart.Series.Count > 1)
                {
                    chart.Series.RemoveAt(1);
                }
            }

            // Ensure all series are initialized but hidden if not needed
            while (chart.Series.Count < 3)
            {
                chart.Series.Add(SeriesType.MeshSurface);
            }

            // Hide walls
            chart.Wall(ChartWallType.Back).Visible = false;
            chart.Wall(ChartWallType.Left).Visible = false;
            chart.Wall(ChartWallType.Right).Visible = false;
            chart.Wall(ChartWallType.Floor).Visible = false;

            // Configure tools
            chartControl.Settings.ShapeRenderingMode = ShapeRenderingMode.HighSpeed;
            chartControl.Controller.Tools.Add(new NSelectorTool());
            chartControl.Controller.Tools.Add(new NTrackballTool());
        }

        /// <summary>
        /// Refreshes the chart with current data
        /// </summary>
        public void RefreshChart()
        {
            RefreshMeshGraph();
        }

        /// <summary>
        /// Sets the view rotation, elevation, and zoom
        /// </summary>
        public void SetView(float rotation, float elevation, float zoom)
        {
            var chartControl = GetChartControl();
            if (chartControl != null && chartControl.Charts.Count > 0)
            {
                chartControl.Charts[0].Projection.Rotation = rotation;
                chartControl.Charts[0].Projection.Elevation = elevation;
                chartControl.Charts[0].Projection.Zoom = zoom;
                chartControl.Refresh();
            }
        }

        /// <summary>
        /// Gets the current view parameters
        /// </summary>
        public void GetView(out float rotation, out float elevation, out float zoom)
        {
            rotation = 0;
            elevation = 0;
            zoom = 0;

            var chartControl = GetChartControl();
            if (chartControl != null && chartControl.Charts.Count > 0)
            {
                rotation = chartControl.Charts[0].Projection.Rotation;
                elevation = chartControl.Charts[0].Projection.Elevation;
                zoom = chartControl.Charts[0].Projection.Zoom;
            }
        }

        /// <summary>
        /// Shows or hides the overlay (original map comparison)
        /// </summary>
        public void SetOverlayVisible(bool visible)
        {
            _overlayVisible = visible;
            RefreshMeshGraph();
        }

        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            // Don't create a new chart control - use the external one if provided
            // This component acts as a wrapper around an existing chart control
        }

        /// <summary>
        /// Gets the chart control to use (external or null)
        /// </summary>
        private Nevron.Chart.WinForm.NChartControl GetChartControl()
        {
            return _externalChartControl;
        }

        private void ConfigureChart()
        {
            var chartControl = GetChartControl();
            if (chartControl == null || chartControl.Charts.Count == 0) return;

            NChart chart = chartControl.Charts[0];
            
            // Configure for 3D
            chart.Enable3D = true;
            chart.Width = 60.0f;
            chart.Depth = 60.0f;
            chart.Height = 35.0f;
            chart.Projection.SetPredefinedProjection(PredefinedProjection.PerspectiveTilted);
            chart.LightModel.SetPredefinedLightModel(PredefinedLightModel.ShinyTopLeft);

            // Set title
            NLabel title = chartControl.Labels.AddHeader(_mapName);
            title.TextStyle.FontStyle = new NFontStyle("Times New Roman", 18, FontStyle.Italic);
            title.TextStyle.FillStyle = new NColorFillStyle(Color.FromArgb(68, 90, 108));

            // Configure axes
            ConfigureAxis(chart);

            // Add surface series
            NMeshSurfaceSeries surface = null;
            if (chart.Series.Count == 0)
            {
                surface = (NMeshSurfaceSeries)chart.Series.Add(SeriesType.MeshSurface);
            }
            else
            {
                surface = (NMeshSurfaceSeries)chart.Series[0];
            }

            ConfigureSurface(surface);

            // Add original map overlay if available and NOT in Easy view
            if (_originalContent != null && _viewType != ViewType.Easy)
            {
                NMeshSurfaceSeries surface2 = null;
                if (chart.Series.Count == 1)
                {
                    surface2 = (NMeshSurfaceSeries)chart.Series.Add(SeriesType.MeshSurface);
                }
                else
                {
                    surface2 = (NMeshSurfaceSeries)chart.Series[1];
                }
                ConfigureOverlay(surface2, Color.YellowGreen, _originalContent);

                // Add compare overlay if available
                if (_compareContent != null)
                {
                    NMeshSurfaceSeries surface3 = null;
                    if (chart.Series.Count == 2)
                    {
                        surface3 = (NMeshSurfaceSeries)chart.Series.Add(SeriesType.MeshSurface);
                    }
                    else
                    {
                        surface3 = (NMeshSurfaceSeries)chart.Series[2];
                    }
                    ConfigureOverlay(surface3, Color.BlueViolet, _compareContent);
                }
            }
            else
            {
                // Clear overlays if in Easy view or no content
                while (chart.Series.Count > 1)
                {
                    chart.Series.RemoveAt(1);
                }
            }

            // Hide walls
            chart.Wall(ChartWallType.Back).Visible = false;
            chart.Wall(ChartWallType.Left).Visible = false;
            chart.Wall(ChartWallType.Right).Visible = false;
            chart.Wall(ChartWallType.Floor).Visible = false;

            // Configure tools
            chartControl.Settings.ShapeRenderingMode = ShapeRenderingMode.HighSpeed;
            chartControl.Controller.Tools.Add(new NSelectorTool());
            chartControl.Controller.Tools.Add(new NTrackballTool());
        }

        private void ConfigureAxis(NChart chart)
        {
            // Configure X axis (X-axis values)
            NStandardScaleConfigurator scaleConfiguratorX = (NStandardScaleConfigurator)chart.Axis(StandardAxis.PrimaryX).ScaleConfigurator;
            scaleConfiguratorX.MajorTickMode = MajorTickMode.AutoMaxCount;
            
            NScaleTitleStyle titleStyleX = (NScaleTitleStyle)scaleConfiguratorX.Title;
            titleStyleX.Text = _xAxisName;
            scaleConfiguratorX.AutoLabels = false;

            for (int t = 0; t < _xAxisValues.Length; t++)
            {
                string xvalue = ConvertXAxisValue(_xAxisValues[t].ToString());
                scaleConfiguratorX.Labels.Add(xvalue);
            }

            // Configure Y axis (Y-axis values)
            NStandardScaleConfigurator scaleConfiguratorY = (NStandardScaleConfigurator)chart.Axis(StandardAxis.Depth).ScaleConfigurator;
            scaleConfiguratorY.MajorTickMode = MajorTickMode.AutoMaxCount;
            
            NScaleTitleStyle titleStyleY = (NScaleTitleStyle)scaleConfiguratorY.Title;
            titleStyleY.Text = _yAxisName;
            scaleConfiguratorY.AutoLabels = false;

            for (int t = (_isUpsideDown ? 0 : _yAxisValues.Length - 1);
                 (_isUpsideDown ? t < _yAxisValues.Length : t >= 0);
                 t += (_isUpsideDown ? 1 : -1))
            {
                string yvalue = ConvertYAxisValue(_yAxisValues[t].ToString());
                scaleConfiguratorY.Labels.Add(yvalue);
            }

            // Configure Z axis
            NStandardScaleConfigurator scaleConfiguratorZ = (NStandardScaleConfigurator)chart.Axis(StandardAxis.PrimaryY).ScaleConfigurator;
            NScaleTitleStyle titleStyleZ = (NScaleTitleStyle)scaleConfiguratorZ.Title;
            titleStyleZ.Text = _zAxisName;
        }

        private void ConfigureSurface(NMeshSurfaceSeries surface)
        {
            surface.Name = "Surface";
            surface.PositionValue = 10.0;

            if (_isSixteenBit)
            {
                surface.Data.SetGridSize((_mapContent.Length / 2) / _tableWidth, _tableWidth);
            }
            else
            {
                surface.Data.SetGridSize(_mapContent.Length / _tableWidth, _tableWidth);
            }

            surface.ValueFormatter.FormatSpecifier = "0.00";
            surface.FillMode = SurfaceFillMode.Zone;
            surface.SmoothPalette = true;
            surface.FrameColorMode = SurfaceFrameColorMode.Uniform;
            surface.FrameMode = SurfaceFrameMode.MeshContour;
            surface.FillStyle.SetTransparencyPercent(25);
        }

        private void ConfigureOverlay(NMeshSurfaceSeries surface, Color overlayColor, byte[] content)
        {
            surface.PositionValue = 10.0;
            surface.Name = "Overlay";
            
            // Use the provided content array for dimension calculation
            if (content != null)
            {
                if (_isSixteenBit)
                {
                    surface.Data.SetGridSize((content.Length / 2) / _tableWidth, _tableWidth);
                }
                else
                {
                    surface.Data.SetGridSize(content.Length / _tableWidth, _tableWidth);
                }
            }

            surface.ValueFormatter.FormatSpecifier = "0.00";
            surface.FillMode = SurfaceFillMode.Zone;
            surface.FillStyle.SetTransparencyPercent(50);
            surface.SmoothPalette = true;
            surface.FrameColorMode = SurfaceFrameColorMode.Zone;
            surface.FrameMode = SurfaceFrameMode.MeshContour;

            // Set palette color
            surface.Palette.Clear();
            surface.Palette.Add(-255, overlayColor);
            surface.Palette.Add(255, overlayColor);
            surface.AutomaticPalette = false;
        }

        private void RefreshMeshGraph()
        {
            try
            {
                var chartControl = GetChartControl();
                if (chartControl == null || chartControl.Charts.Count == 0) return;

                NChart chart = chartControl.Charts[0];
                
                // Get main surface
                NMeshSurfaceSeries surface = null;
                if (chart.Series.Count > 0)
                {
                    surface = (NMeshSurfaceSeries)chart.Series[0];
                }
                else
                {
                    return;
                }

                FillData(surface);

                // Handle overlay surfaces - Use Visibility instead of adding/removing series to prevent crashes
                bool showOverlays = _overlayVisible && _viewType != ViewType.Easy;
                
                if (chart.Series.Count > 1)
                {
                    NMeshSurfaceSeries surface2 = (NMeshSurfaceSeries)chart.Series[1];
                    if (surface2 != null)
                    {
                        surface2.Visible = showOverlays && _originalContent != null;
                        if (surface2.Visible)
                        {
                            ConfigureOverlay(surface2, Color.YellowGreen, _originalContent);
                            FillDataOriginal(surface2);
                        }
                    }
                }

                if (chart.Series.Count > 2)
                {
                    NMeshSurfaceSeries surface3 = (NMeshSurfaceSeries)chart.Series[2];
                    if (surface3 != null)
                    {
                        surface3.Visible = showOverlays && _compareContent != null;
                        if (surface3.Visible)
                        {
                            ConfigureOverlay(surface3, Color.BlueViolet, _compareContent);
                            FillDataCompare(surface3);
                        }
                    }
                }

                // Refresh the chart
                chartControl.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: RefreshMeshGraph error: " + ex.Message);
            }
        }

        private void FillData(NMeshSurfaceSeries surface)
        {
            try
            {
                int rowCount = _isSixteenBit ? (_mapContent.Length / 2) / _tableWidth : _mapContent.Length / _tableWidth;
                
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < _tableWidth; col++)
                    {
                        double value = GetValueFromContent(row, col);
                        // Conditionally flip Y axis if _isUpsideDown is true
                        int yIndex = _isUpsideDown ? (rowCount - 1) - row : row;
                        surface.Data.SetValue(yIndex, col, value, yIndex, col);
                    }
                }

                // Set palette
                SetPalette(surface);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: FillData error: " + ex.Message);
            }
        }

        private void FillDataOriginal(NMeshSurfaceSeries surface)
        {
            try
            {
                // Calculate row count from original content, not map content
                int rowCount = _isSixteenBit ? (_originalContent.Length / 2) / _tableWidth : _originalContent.Length / _tableWidth;
                
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < _tableWidth; col++)
                    {
                        double value = GetOriginalValueFromContent(row, col);
                        int yIndex = _isUpsideDown ? (rowCount - 1) - row : row;
                        surface.Data.SetValue(yIndex, col, value, yIndex, col);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: FillDataOriginal error: " + ex.Message);
            }
        }

        private void FillDataCompare(NMeshSurfaceSeries surface)
        {
            try
            {
                int rowCount = _isSixteenBit ? (_compareContent.Length / 2) / _tableWidth : _compareContent.Length / _tableWidth;
                
                for (int row = 0; row < rowCount; row++)
                {
                    // Note: Original code iterates col from _tableWidth - 1 down to 0
                    for (int col = _tableWidth - 1; col >= 0; col--)
                    {
                        double value = GetCompareValueFromContent(row, col);
                        int yIndex = _isUpsideDown ? (rowCount - 1) - row : row;
                        surface.Data.SetValue(yIndex, col, value, yIndex, col);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart3DComponent: FillDataCompare error: " + ex.Message);
            }
        }

        private double GetValueFromContent(int row, int col)
        {
            int index = (row * _tableWidth + col) * (_isSixteenBit ? 2 : 1);
            if (index >= _mapContent.Length) return 0;

            double value;
            if (_isSixteenBit)
            {
                value = _mapContent[index] * 256.0 + _mapContent[index + 1];
                if (value > 32000)
                {
                    value = 65536 - value;
                    value = -value;
                }
            }
            else
            {
                value = _mapContent[index];
            }

            if (_viewType != ViewType.Decimal && _viewType != ViewType.Hexadecimal && _viewType != ViewType.ASCII)
            {
                value *= _correctionFactor;
                if (!_isCompareViewer) value += _correctionOffset;
            }

            return value;
        }

        private double GetOriginalValueFromContent(int row, int col)
        {
            int index = (row * _tableWidth + col) * (_isSixteenBit ? 2 : 1);
            if (index >= _originalContent.Length) return 0;

            double value;
            if (_isSixteenBit)
            {
                value = _originalContent[index] * 256.0 + _originalContent[index + 1];
                if (value > 32000)
                {
                    value = 65536 - value;
                    value = -value;
                }
            }
            else
            {
                value = _originalContent[index];
            }

            if (_viewType != ViewType.Decimal && _viewType != ViewType.Hexadecimal && _viewType != ViewType.ASCII)
            {
                value *= _correctionFactor;
                value += _correctionOffset;
            }

            return value;
        }

        private double GetCompareValueFromContent(int row, int col)
        {
            int index = (row * _tableWidth + col) * (_isSixteenBit ? 2 : 1);
            if (index >= _compareContent.Length) return 0;

            double value;
            if (_isSixteenBit)
            {
                value = _compareContent[index] * 256.0 + _compareContent[index + 1];
                if (value > 32000)
                {
                    value = 65536 - value;
                    value = -value;
                }
            }
            else
            {
                value = _compareContent[index];
            }

            if (_viewType != ViewType.Decimal && _viewType != ViewType.Hexadecimal && _viewType != ViewType.ASCII)
            {
                value *= _correctionFactor;
                value += _correctionOffset;
            }

            return value;
        }

        private void SetPalette(NMeshSurfaceSeries surface)
        {
            // Calculate min/max values
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            int rowCount = _isSixteenBit ? (_mapContent.Length / 2) / _tableWidth : _mapContent.Length / _tableWidth;
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < _tableWidth; col++)
                {
                    double value = GetValueFromContent(row, col);
                    if (value < minValue) minValue = value;
                    if (value > maxValue) maxValue = value;
                }
            }

            if (minValue == double.MaxValue || maxValue == double.MinValue) return;

            surface.Palette.Clear();
            double diff = maxValue - minValue;

            if (_onlineMode)
            {
                surface.Palette.Add(minValue, Color.Wheat);
                surface.Palette.Add(minValue + 0.25 * diff, Color.LightBlue);
                surface.Palette.Add(minValue + 0.50 * diff, Color.SteelBlue);
                surface.Palette.Add(minValue + 0.75 * diff, Color.Blue);
                surface.Palette.Add(minValue + diff, Color.DarkBlue);
            }
            else
            {
                // VAGEDC Dark Skin: Blue→Green→Yellow→Orange→Red gradient
                surface.Palette.Add(minValue, Color.FromArgb(30, 58, 138));  // Navy Blue
                surface.Palette.Add(minValue + 0.20 * diff, Color.FromArgb(16, 185, 129));  // Emerald Green
                surface.Palette.Add(minValue + 0.40 * diff, Color.Yellow);
                surface.Palette.Add(minValue + 0.60 * diff, Color.Orange);
                surface.Palette.Add(minValue + 0.80 * diff, Color.OrangeRed);
                surface.Palette.Add(minValue + diff, Color.Red);
            }

            surface.PaletteSteps = 5;
            surface.AutomaticPalette = false;
        }

        private string ConvertYAxisValue(string currValue)
        {
            if (_yAxisValues == null || _yAxisValues.Length == 0) return currValue;
            
            // Apply correction factor
            try
            {
                float temp = (float)Convert.ToDouble(currValue);
                temp *= (float)_correctionFactor;
                temp += (float)_correctionOffset;
                return temp.ToString("F1");
            }
            catch
            {
                return currValue;
            }
        }

        private string ConvertXAxisValue(string currValue)
        {
            if (_xAxisValues == null || _xAxisValues.Length == 0) return currValue;
            
            // Apply correction factor
            try
            {
                float temp = (float)Convert.ToDouble(currValue);
                temp *= (float)_correctionFactor;
                temp += (float)_correctionOffset;
                return temp.ToString("F2");
            }
            catch
            {
                return currValue;
            }
        }

        private void NChartControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chartControl = GetChartControl();
            if (chartControl == null || chartControl.Charts.Count > 0) return;
            
            if (e.Delta > 0)
            {
                chartControl.Charts[0].Projection.Zoom += 5;
            }
            else
            {
                chartControl.Charts[0].Projection.Zoom -= 5;
            }
            chartControl.Refresh();
            RaiseViewChanged();
        }

        private void NChartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            var chartControl = GetChartControl();
            if (chartControl == null) return;
            
            if (e.Button == MouseButtons.Right)
            {
                chartControl.Controller.Tools.Clear();
                NOffsetTool dragTool = new NOffsetTool();
                chartControl.Controller.Tools.Add(dragTool);
            }
        }

        private void NChartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            var chartControl = GetChartControl();
            if (chartControl == null) return;
            
            if (e.Button == MouseButtons.Right)
            {
                chartControl.Controller.Tools.Clear();
                NTrackballTool dragTool = new NTrackballTool();
                chartControl.Controller.Tools.Add(dragTool);
            }

            RaiseViewChanged();
        }

        private void RaiseViewChanged()
        {
            var chartControl = GetChartControl();
            if (chartControl == null || chartControl.Charts.Count == 0) return;
            
            ViewChanged?.Invoke(this, new SurfaceGraphViewChangedEventArgsEx(
                chartControl.Charts[0].Projection.XDepth,
                chartControl.Charts[0].Projection.YDepth,
                chartControl.Charts[0].Projection.Zoom,
                chartControl.Charts[0].Projection.Rotation,
                chartControl.Charts[0].Projection.Elevation,
                _mapName));
        }

        #endregion
    }
}
