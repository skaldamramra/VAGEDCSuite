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
        
        private Nevron.Chart.WinForm.NChartControl nChartControl1;
        
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

        #endregion

        #region Events

        public event EventHandler<SurfaceGraphViewChangedEventArgsEx> ViewChanged;
        public event EventHandler RefreshRequested;

        #endregion

        #region Constructors

        public Chart3DComponent()
        {
            _chartService = new ChartService();
            InitializeComponent();
        }

        public Chart3DComponent(IChartService chartService)
        {
            _chartService = chartService ?? throw new ArgumentNullException("chartService");
            InitializeComponent();
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

            ConfigureChart();
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
            if (nChartControl1 != null && nChartControl1.Charts.Count > 0)
            {
                nChartControl1.Charts[0].Projection.Rotation = rotation;
                nChartControl1.Charts[0].Projection.Elevation = elevation;
                nChartControl1.Charts[0].Projection.Zoom = zoom;
                nChartControl1.Refresh();
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

            if (nChartControl1 != null && nChartControl1.Charts.Count > 0)
            {
                rotation = nChartControl1.Charts[0].Projection.Rotation;
                elevation = nChartControl1.Charts[0].Projection.Elevation;
                zoom = nChartControl1.Charts[0].Projection.Zoom;
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
            // Create chart control
            nChartControl1 = new Nevron.Chart.WinForm.NChartControl();
            
            // 
            // nChartControl1
            // 
            nChartControl1.Dock = DockStyle.Fill;
            nChartControl1.Location = new Point(0, 0);
            nChartControl1.Name = "nChartControl1";
            nChartControl1.Size = new Size(400, 300);
            nChartControl1.TabIndex = 0;
            nChartControl1.Visible = true;

            // Wire up mouse events
            nChartControl1.MouseWheel += NChartControl1_MouseWheel;
            nChartControl1.MouseDown += NChartControl1_MouseDown;
            nChartControl1.MouseUp += NChartControl1_MouseUp;

            this.Controls.Add(nChartControl1);
            this.Dock = DockStyle.Fill;
            this.Size = new Size(400, 300);
        }

        private void ConfigureChart()
        {
            if (nChartControl1.Charts.Count == 0) return;

            NChart chart = nChartControl1.Charts[0];
            
            // Configure for 3D
            chart.Enable3D = true;
            chart.Width = 60.0f;
            chart.Depth = 60.0f;
            chart.Height = 35.0f;
            chart.Projection.SetPredefinedProjection(PredefinedProjection.PerspectiveTilted);
            chart.LightModel.SetPredefinedLightModel(PredefinedLightModel.ShinyTopLeft);

            // Set title
            NLabel title = nChartControl1.Labels.AddHeader(_mapName);
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

            // Add original map overlay if available
            if (_originalContent != null)
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
                ConfigureOverlay(surface2, Color.YellowGreen);

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
                    ConfigureOverlay(surface3, Color.BlueViolet);
                }
            }

            // Hide walls
            chart.Wall(ChartWallType.Back).Visible = false;
            chart.Wall(ChartWallType.Left).Visible = false;
            chart.Wall(ChartWallType.Right).Visible = false;
            chart.Wall(ChartWallType.Floor).Visible = false;

            // Configure tools
            nChartControl1.Settings.ShapeRenderingMode = ShapeRenderingMode.HighSpeed;
            nChartControl1.Controller.Tools.Add(new NSelectorTool());
            nChartControl1.Controller.Tools.Add(new NTrackballTool());

            RefreshMeshGraph();
        }

        private void ConfigureAxis(NChart chart)
        {
            // Configure X axis (Y-axis values)
            NStandardScaleConfigurator scaleConfiguratorX = (NStandardScaleConfigurator)chart.Axis(StandardAxis.PrimaryX).ScaleConfigurator;
            scaleConfiguratorX.MajorTickMode = MajorTickMode.AutoMaxCount;
            
            NScaleTitleStyle titleStyleX = (NScaleTitleStyle)scaleConfiguratorX.Title;
            titleStyleX.Text = _yAxisName;
            scaleConfiguratorX.AutoLabels = false;

            for (int t = _yAxisValues.Length - 1; t >= 0; t--)
            {
                string yvalue = ConvertYAxisValue(_yAxisValues[t].ToString());
                scaleConfiguratorX.Labels.Add(yvalue);
            }

            // Configure Y axis (X-axis values)
            NStandardScaleConfigurator scaleConfiguratorY = (NStandardScaleConfigurator)chart.Axis(StandardAxis.Depth).ScaleConfigurator;
            scaleConfiguratorY.MajorTickMode = MajorTickMode.AutoMaxCount;
            
            NScaleTitleStyle titleStyleY = (NScaleTitleStyle)scaleConfiguratorY.Title;
            titleStyleY.Text = _xAxisName;
            scaleConfiguratorY.AutoLabels = false;

            for (int t = 0; t < _xAxisValues.Length; t++)
            {
                string xvalue = ConvertXAxisValue(_xAxisValues[t].ToString());
                scaleConfiguratorY.Labels.Add(xvalue);
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

        private void ConfigureOverlay(NMeshSurfaceSeries surface, Color overlayColor)
        {
            surface.PositionValue = 10.0;
            surface.Name = "Overlay";
            
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
                if (nChartControl1.Charts.Count == 0) return;

                NChart chart = nChartControl1.Charts[0];
                
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

                // Handle overlay surfaces
                if (_overlayVisible && _originalContent != null)
                {
                    NMeshSurfaceSeries surface2 = (NMeshSurfaceSeries)chart.Series[1];
                    if (surface2 != null)
                    {
                        surface2.Visible = true;
                        FillDataOriginal(surface2);
                    }

                    if (_compareContent != null && chart.Series.Count > 2)
                    {
                        NMeshSurfaceSeries surface3 = (NMeshSurfaceSeries)chart.Series[2];
                        if (surface3 != null)
                        {
                            surface3.Visible = true;
                            FillDataCompare(surface3);
                        }
                    }
                }

                nChartControl1.Refresh();
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
                        surface.Data.SetValue(row, col, value, row, col);
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
                int rowCount = _isSixteenBit ? (_originalContent.Length / 2) / _tableWidth : _originalContent.Length / _tableWidth;
                
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < _tableWidth; col++)
                    {
                        double value = GetOriginalValueFromContent(row, col);
                        surface.Data.SetValue((rowCount - 1) - row, col, value, (rowCount - 1) - row, col);
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
                    for (int col = _tableWidth - 1; col >= 0; col--)
                    {
                        double value = GetCompareValueFromContent(row, col);
                        surface.Data.SetValue((rowCount - 1) - row, col, value, (rowCount - 1) - row, col);
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
            if (nChartControl1.Charts.Count > 0)
            {
                if (e.Delta > 0)
                {
                    nChartControl1.Charts[0].Projection.Zoom += 5;
                }
                else
                {
                    nChartControl1.Charts[0].Projection.Zoom -= 5;
                }
                nChartControl1.Refresh();
                RaiseViewChanged();
            }
        }

        private void NChartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                nChartControl1.Controller.Tools.Clear();
                NOffsetTool dragTool = new NOffsetTool();
                nChartControl1.Controller.Tools.Add(dragTool);
            }
        }

        private void NChartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                nChartControl1.Controller.Tools.Clear();
                NTrackballTool dragTool = new NTrackballTool();
                nChartControl1.Controller.Tools.Add(dragTool);
            }

            RaiseViewChanged();
        }

        private void RaiseViewChanged()
        {
            if (nChartControl1.Charts.Count > 0)
            {
                ViewChanged?.Invoke(this, new SurfaceGraphViewChangedEventArgsEx(
                    nChartControl1.Charts[0].Projection.XDepth,
                    nChartControl1.Charts[0].Projection.YDepth,
                    nChartControl1.Charts[0].Projection.Zoom,
                    nChartControl1.Charts[0].Projection.Rotation,
                    nChartControl1.Charts[0].Projection.Elevation,
                    _mapName));
            }
        }

        #endregion
    }
}
