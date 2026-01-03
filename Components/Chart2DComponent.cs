using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Components
{
    /// <summary>
    /// Encapsulates the 2D line chart for single-column map visualization.
    /// </summary>
    public class Chart2DComponent : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private readonly IChartService _chartService;
        
        private ZedGraph.ZedGraphControl _externalChartControl;
        private ZedGraph.PointPairList _pointList;
        
        // State references
        private int _tableWidth;
        private bool _isSixteenBit;
        private ViewType _viewType;
        private string _mapName;
        private string _xAxisName;
        private byte[] _mapContent;
        private int[] _yAxisValues;
        private double _correctionFactor;
        private double _correctionOffset;
        private bool _isUpsideDown;

        #endregion

        #region Events

        public event EventHandler DataChanged;

        #endregion

        #region Constructors

        public Chart2DComponent()
        {
            _chartService = new ChartService();
        }

        public Chart2DComponent(IChartService chartService)
        {
            _chartService = chartService ?? throw new ArgumentNullException("chartService");
        }

        /// <summary>
        /// Sets the external NChartControl to use instead of creating a new one.
        /// This allows the component to share the designer's chart control.
        /// </summary>
        public void SetChartControl(ZedGraph.ZedGraphControl externalChart)
        {
            _externalChartControl = externalChart;
            if (_externalChartControl != null)
            {
                _pointList = new ZedGraph.PointPairList();
                
                // Wire up navigation events
                _externalChartControl.KeyDown += OnChartKeyDown;
                _externalChartControl.MouseWheel += OnChartMouseWheel;
                _externalChartControl.MouseDown += (s, e) => _externalChartControl.Focus();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads data into the 2D chart
        /// </summary>
        public void LoadData(byte[] content, int mapLength, MapViewerState state)
        {
            _tableWidth = state.Data.TableWidth;
            _isSixteenBit = state.Data.IsSixteenBit;
            _viewType = state.Configuration.ViewType;
            _mapName = state.Metadata.Name;
            _xAxisName = state.Metadata.XAxisName;
            _mapContent = content;
            _yAxisValues = state.Axes.YAxisValues;
            _correctionFactor = state.Configuration.CorrectionFactor;
            _correctionOffset = state.Configuration.CorrectionOffset;
            _isUpsideDown = state.Configuration.IsUpsideDown;

            ConfigureChart(state);
        }

        /// <summary>
        /// Updates the chart slice at specified position
        /// </summary>
        public void UpdateSlice(byte[] data, int sliceIndex)
        {
            if (data == null || data.Length == 0) return;

            DataTable chartdt = new DataTable();
            chartdt.Columns.Add("X", Type.GetType("System.Double"));
            chartdt.Columns.Add("Y", Type.GetType("System.Double"));

            double valCount = 0;
            int offsetInMap = sliceIndex;

            int numberOfRows = data.Length / _tableWidth;
            if (_isSixteenBit)
            {
                numberOfRows /= 2;
                offsetInMap *= 2;
            }

            if (_isSixteenBit)
            {
                for (int t = numberOfRows - 1; t >= 0; t--)
                {
                    double yVal = valCount;
                    double value = Convert.ToDouble(data.GetValue(offsetInMap + (t * _tableWidth * 2))) * 256.0;
                    value += Convert.ToDouble(data.GetValue(offsetInMap + (t * (_tableWidth * 2)) + 1));

                    if (value > 32000)
                    {
                        value = 65536 - value;
                        value = -value;
                    }

                    value *= _correctionFactor;
                    value += _correctionOffset;

                    if (_yAxisValues.Length > valCount)
                    {
                        yVal = Convert.ToDouble(_yAxisValues[(int)valCount]);
                    }

                    chartdt.Rows.Add(yVal, value);
                    valCount++;
                }
            }
            else
            {
                for (int t = numberOfRows - 1; t >= 0; t--)
                {
                    double yVal = valCount;
                    double value = Convert.ToDouble(data.GetValue(offsetInMap + (t * _tableWidth)));

                    value *= _correctionFactor;
                    value += _correctionOffset;

                    if (_yAxisValues.Length > valCount)
                    {
                        yVal = Convert.ToDouble(_yAxisValues[(int)valCount]);
                    }

                    chartdt.Rows.Add(yVal, value);
                    valCount++;
                }
            }

            RefreshChart();
        }

        /// <summary>
        /// Gets the current slice index
        /// </summary>
        public int CurrentSliceIndex { get; private set; }

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
        private ZedGraph.ZedGraphControl GetChartControl()
        {
            return _externalChartControl;
        }

        private void ConfigureChart(MapViewerState state)
        {
            var chartControl = GetChartControl();
            if (chartControl == null) return;

            ZedGraph.GraphPane pane = chartControl.GraphPane;
            pane.CurveList.Clear();
            pane.GraphObjList.Clear();

            // Configure pane
            pane.Title.Text = _mapName;
            pane.XAxis.Title.Text = _xAxisName;
            pane.YAxis.Title.Text = state.Metadata.ZAxisName;

            // Modern Dark Styling (Verified in Phase 2 to match Chart3DComponent)
            Color darkBg = Color.FromArgb(40, 40, 40);
            Color offWhite = Color.FromArgb(224, 224, 224);
            Color gridColor = Color.FromArgb(100, 100, 100); // Subtle gray
            Color pastelBlue = ColorTranslator.FromHtml("#5B8FF9"); // From 3D gradient

            pane.Fill = new ZedGraph.Fill(darkBg);
            pane.Chart.Fill = new ZedGraph.Fill(darkBg);
            pane.Chart.Border.Color = gridColor;
            pane.Legend.IsVisible = false; // Minimalist look

            // Title and Axis Label Styling
            pane.Title.FontSpec.FontColor = offWhite;
            pane.Title.FontSpec.Family = "Segoe UI";
            pane.Title.FontSpec.IsBold = true;

            // X-Axis Styling
            pane.XAxis.Color = gridColor;
            pane.XAxis.Title.FontSpec.FontColor = offWhite;
            pane.XAxis.Title.FontSpec.Family = "Segoe UI";
            pane.XAxis.Scale.FontSpec.FontColor = offWhite;
            pane.XAxis.Scale.FontSpec.Family = "Segoe UI";
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.XAxis.MajorGrid.Color = Color.FromArgb(50, gridColor); // Semi-transparent
            pane.XAxis.MajorGrid.DashOff = 0;

            // Y-Axis Styling
            pane.YAxis.Color = gridColor;
            pane.YAxis.Title.FontSpec.FontColor = offWhite;
            pane.YAxis.Title.FontSpec.Family = "Segoe UI";
            pane.YAxis.Scale.FontSpec.FontColor = offWhite;
            pane.YAxis.Scale.FontSpec.Family = "Segoe UI";
            pane.YAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.Color = Color.FromArgb(50, gridColor); // Semi-transparent
            pane.YAxis.MajorGrid.DashOff = 0;

            // Create point list and curve
            _pointList = new ZedGraph.PointPairList();
            ZedGraph.LineItem curve = pane.AddCurve(_mapName, _pointList, Color.FromArgb(150, offWhite), ZedGraph.SymbolType.Circle);

            // Configure curve (Verified: IsSmooth=true for ZedGraph 5.1.7)
            curve.Line.IsSmooth = true;
            curve.Line.SmoothTension = 0.5f;
            curve.Line.Width = 1.5f; // Slimmer line as requested
            
            // Gradient Symbol Styling (Verified in Phase 2)
            // Sequence: Pastel Dark Blue -> Dark Green -> Light Green -> Yellow -> Orange -> Red
            Color[] gradientStops = new Color[] {
                ColorTranslator.FromHtml("#5B8FF9"),
                ColorTranslator.FromHtml("#61DDAA"),
                ColorTranslator.FromHtml("#91CC75"),
                ColorTranslator.FromHtml("#FAC858"),
                ColorTranslator.FromHtml("#FC8452"),
                ColorTranslator.FromHtml("#EE6666")
            };
            
            curve.Symbol.Type = ZedGraph.SymbolType.Circle;
            curve.Symbol.Size = 11.0f; // Bigger points as requested
            curve.Symbol.Fill = new ZedGraph.Fill(gradientStops);
            curve.Symbol.Fill.Type = ZedGraph.FillType.GradientByY;
            curve.Symbol.Border.Color = darkBg;
            curve.Symbol.Border.Width = 1.5f;

            chartControl.AxisChange();
        }

        /// <summary>
        /// Refreshes the 2D chart with current data.
        /// </summary>
        public void RefreshChart()
        {
            RefreshChart(null);
        }

        /// <summary>
        /// Refreshes the 2D chart with current data.
        /// </summary>
        public void RefreshChart(MapViewerState state)
        {
            try
            {
                var chartControl = GetChartControl();
                if (chartControl == null || _pointList == null) return;

                // Clear existing data points
                _pointList.Clear();

                double valCount = 0;
                int numberOfRows = _mapContent.Length;
                if (_isSixteenBit) numberOfRows /= 2;

                double minY = double.MaxValue;
                double maxY = double.MinValue;

                if (_isSixteenBit)
                {
                    for (int t = 0; t < numberOfRows; t++)
                    {
                        int actualRow = t;
                        double xval = valCount;
                        int offset = actualRow * 2;
                        double value = Convert.ToDouble(_mapContent.GetValue(offset)) * 256;
                        value += Convert.ToDouble(_mapContent.GetValue(offset + 1));

                        if (value > 32000)
                        {
                            value = 65536 - value;
                            value = -value;
                        }

                        value *= _correctionFactor;
                        value += _correctionOffset;

                        if (_yAxisValues != null && _yAxisValues.Length > valCount)
                            xval = Convert.ToDouble(_yAxisValues[(int)valCount]);

                        _pointList.Add(xval, value);
                        if (value < minY) minY = value;
                        if (value > maxY) maxY = value;
                        valCount++;
                    }
                }
                else
                {
                    for (int t = 0; t < numberOfRows; t++)
                    {
                        int actualRow = t;
                        double xval = valCount;
                        double value = Convert.ToDouble(_mapContent.GetValue(actualRow));

                        value *= _correctionFactor;
                        value += _correctionOffset;

                        if (_yAxisValues != null && _yAxisValues.Length > valCount)
                            xval = Convert.ToDouble(_yAxisValues[(int)valCount]);

                        _pointList.Add(xval, value);
                        if (value < minY) minY = value;
                        if (value > maxY) maxY = value;
                        valCount++;
                    }
                }

                // Update Gradient Range (Verified in Phase 2)
                if (chartControl.GraphPane.CurveList.Count > 0)
                {
                    var curve = chartControl.GraphPane.CurveList[0] as ZedGraph.LineItem;
                    if (curve != null && curve.Symbol.Fill.Type == ZedGraph.FillType.GradientByY)
                    {
                        curve.Symbol.Fill.RangeMin = minY;
                        curve.Symbol.Fill.RangeMax = maxY;
                    }
                }

                chartControl.AxisChange();
                chartControl.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart2DComponent: RefreshChart error: " + ex.Message);
            }
        }

        private void OnChartKeyDown(object sender, KeyEventArgs e)
        {
            var chartControl = GetChartControl();
            if (chartControl == null) return;

            var pane = chartControl.GraphPane;
            double xSpan = pane.XAxis.Scale.Max - pane.XAxis.Scale.Min;
            double ySpan = pane.YAxis.Scale.Max - pane.YAxis.Scale.Min;
            double panStep = 0.1; // 10% of current span

            switch (e.KeyCode)
            {
                case Keys.W: // Pan Up
                    pane.YAxis.Scale.Min += ySpan * panStep;
                    pane.YAxis.Scale.Max += ySpan * panStep;
                    break;
                case Keys.S: // Pan Down
                    pane.YAxis.Scale.Min -= ySpan * panStep;
                    pane.YAxis.Scale.Max -= ySpan * panStep;
                    break;
                case Keys.A: // Pan Left
                    pane.XAxis.Scale.Min -= xSpan * panStep;
                    pane.XAxis.Scale.Max -= xSpan * panStep;
                    break;
                case Keys.D: // Pan Right
                    pane.XAxis.Scale.Min += xSpan * panStep;
                    pane.XAxis.Scale.Max += xSpan * panStep;
                    break;
                case Keys.Add:
                case Keys.Oemplus: // Zoom In
                    ZoomChart(0.9);
                    break;
                case Keys.Subtract:
                case Keys.OemMinus: // Zoom Out
                    ZoomChart(1.1);
                    break;
                case Keys.R: // Reset View
                    chartControl.RestoreScale(pane);
                    break;
            }

            chartControl.AxisChange();
            chartControl.Invalidate();
        }

        private void OnChartMouseWheel(object sender, MouseEventArgs e)
        {
            // Zoom based on wheel direction
            double zoomFactor = e.Delta > 0 ? 0.9 : 1.1;
            ZoomChart(zoomFactor);
        }

        private void ZoomChart(double factor)
        {
            var chartControl = GetChartControl();
            if (chartControl == null) return;

            var pane = chartControl.GraphPane;
            
            double xMid = (pane.XAxis.Scale.Max + pane.XAxis.Scale.Min) / 2.0;
            double yMid = (pane.YAxis.Scale.Max + pane.YAxis.Scale.Min) / 2.0;
            double xHalfSpan = (pane.XAxis.Scale.Max - pane.XAxis.Scale.Min) * factor / 2.0;
            double yHalfSpan = (pane.YAxis.Scale.Max - pane.YAxis.Scale.Min) * factor / 2.0;

            pane.XAxis.Scale.Min = xMid - xHalfSpan;
            pane.XAxis.Scale.Max = xMid + xHalfSpan;
            pane.YAxis.Scale.Min = yMid - yHalfSpan;
            pane.YAxis.Scale.Max = yMid + yHalfSpan;

            chartControl.AxisChange();
            chartControl.Invalidate();
        }

        #endregion
    }
}
