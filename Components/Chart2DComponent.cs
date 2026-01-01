using System;
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
    /// Encapsulates the 2D line chart for single-column map visualization.
    /// </summary>
    public class Chart2DComponent : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private readonly IChartService _chartService;
        
        private Nevron.Chart.WinForm.NChartControl _externalChartControl;
        
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
        public void SetChartControl(Nevron.Chart.WinForm.NChartControl externalChart)
        {
            _externalChartControl = externalChart;
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
        private Nevron.Chart.WinForm.NChartControl GetChartControl()
        {
            return _externalChartControl;
        }

        private void ConfigureChart(MapViewerState state)
        {
            var chartControl = GetChartControl();
            if (chartControl == null || chartControl.Charts.Count == 0) return;

            chartControl.Settings.ShapeRenderingMode = ShapeRenderingMode.HighSpeed;
            chartControl.Legends.Clear();

            NChart chart2d = chartControl.Charts[0];

            // Configure axis
            NLinearScaleConfigurator linearScale = (NLinearScaleConfigurator)chart2d.Axis(StandardAxis.PrimaryY).ScaleConfigurator;
            linearScale.MajorGridStyle.LineStyle.Pattern = LinePattern.Dot;
            linearScale.MajorGridStyle.SetShowAtWall(ChartWallType.Back, true);

            NScaleStripStyle stripStyle = new NScaleStripStyle(new NColorFillStyle(Color.Beige), null, true, 0, 0, 1, 1);
            stripStyle.Interlaced = true;
            stripStyle.SetShowAtWall(ChartWallType.Back, true);
            stripStyle.SetShowAtWall(ChartWallType.Left, true);
            linearScale.StripStyles.Add(stripStyle);

            // Add line series
            NSmoothLineSeries line = null;
            if (chart2d.Series.Count == 0)
            {
                line = (NSmoothLineSeries)chart2d.Series.Add(SeriesType.SmoothLine);
            }
            else
            {
                line = (NSmoothLineSeries)chart2d.Series[0];
            }

            line.Name = _mapName;
            line.Legend.Mode = SeriesLegendMode.Series;
            line.UseXValues = true;
            line.UseZValues = false;
            line.DataLabelStyle.Visible = true;
            line.MarkerStyle.Visible = true;
            line.MarkerStyle.PointShape = PointShape.Sphere;
            line.MarkerStyle.AutoDepth = true;
            line.MarkerStyle.Width = new NLength(1.4f, NRelativeUnit.ParentPercentage);
            line.MarkerStyle.Height = new NLength(1.4f, NRelativeUnit.ParentPercentage);
            line.MarkerStyle.Depth = new NLength(1.4f, NRelativeUnit.ParentPercentage);

            // Set X values from axis
            if (_yAxisValues != null)
            {
                for (int i = 0; i < _yAxisValues.Length; i++)
                {
                    line.XValues.Add(_yAxisValues[i]);
                }
            }

            // Apply style sheet
            NStyleSheet styleSheet = NStyleSheet.CreatePredefinedStyleSheet(PredefinedStyleSheet.Nevron);
            styleSheet.Apply(chartControl.Document);

            chart2d.BoundsMode = BoundsMode.Stretch;
        }

        /// <summary>
        /// Refreshes the 2D chart with current data.
        /// </summary>
        public void RefreshChart()
        {
            try
            {
                var chartControl = GetChartControl();
                if (chartControl == null || chartControl.Charts.Count == 0) return;

                NChart chart = chartControl.Charts[0];
                NSmoothLineSeries line = null;

                if (chart.Series.Count == 0)
                {
                    line = (NSmoothLineSeries)chart.Series.Add(SeriesType.SmoothLine);
                }
                else
                {
                    line = (NSmoothLineSeries)chart.Series[0];
                }

                // Clear existing data points
                line.ClearDataPoints();

                // Populate data table
                DataTable chartdt = new DataTable();
                chartdt.Columns.Add("X", Type.GetType("System.Double"));
                chartdt.Columns.Add("Y", Type.GetType("System.Double"));

                double valCount = 0;
                if (_isSixteenBit)
                {
                    for (int t = 0; t < _mapContent.Length; t += 2)
                    {
                        double yval = valCount;
                        double value = Convert.ToDouble(_mapContent.GetValue(t)) * 256;
                        value += Convert.ToDouble(_mapContent.GetValue(t + 1));
                        if (_yAxisValues.Length > valCount) yval = Convert.ToDouble((int)_yAxisValues.GetValue((int)valCount));
                        chartdt.Rows.Add(yval, value);
                        valCount++;
                    }
                }
                else
                {
                    for (int t = 0; t < _mapContent.Length; t++)
                    {
                        double yval = valCount;
                        double value = Convert.ToDouble(_mapContent.GetValue(t));
                        if (_yAxisValues.Length > valCount) yval = Convert.ToDouble((int)_yAxisValues.GetValue((int)valCount));
                        chartdt.Rows.Add(yval, value);
                        valCount++;
                    }
                }

                // Add new data points
                foreach (DataRow dr in chartdt.Rows)
                {
                    try
                    {
                        double xValue = Convert.ToDouble(dr["X"]);
                        double yValue = Convert.ToDouble(dr["Y"]);
                        line.AddDataPoint(new NDataPoint(xValue, yValue));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Chart2DComponent: Error adding data point: " + ex.Message);
                    }
                }

                chartControl.Refresh();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart2DComponent: RefreshChart error: " + ex.Message);
            }
        }

        #endregion
    }
}
