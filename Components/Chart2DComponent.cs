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

            // Styling
            pane.Fill = new ZedGraph.Fill(Color.White);
            pane.Chart.Fill = new ZedGraph.Fill(Color.White);
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;
            pane.XAxis.MajorGrid.DashOff = 0;
            pane.YAxis.MajorGrid.DashOff = 0;

            // Create point list and curve
            _pointList = new ZedGraph.PointPairList();
            ZedGraph.LineItem curve = pane.AddCurve(_mapName, _pointList, Color.Blue, ZedGraph.SymbolType.Circle);

            // Configure curve (Verified: IsSmooth=true for ZedGraph 5.1.7)
            curve.Line.IsSmooth = true;
            curve.Line.SmoothTension = 0.5f;
            curve.Line.Width = 2.0f;
            curve.Symbol.Size = 7.0f;
            curve.Symbol.Fill = new ZedGraph.Fill(Color.Blue);

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
                        valCount++;
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

        #endregion
    }
}
