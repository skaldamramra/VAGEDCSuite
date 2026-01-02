using System;
using System.Data;
using System.Drawing;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Manages 3D and 2D chart rendering using Nevron Chart
    /// </summary>
    public class ChartService : IChartService
    {
        private double now_realMaxValue = double.MinValue;
        private double now_realMinValue = double.MaxValue;

        public void Configure3DChart(object chartControl, MapViewerState state)
        {
            // This method would configure the 3D chart control
            // The actual implementation depends on the specific chart control type
            // For Nevron NChartControl, configuration includes:
            // - Setting up projection (PerspectiveTilted)
            // - Configuring axes (X, Y, Z with labels)
            // - Setting up mesh surface series
        }

        public void Update3DChartData(object surfaceSeries, DataTable data, ViewConfiguration config)
        {
            // TODO: Implement OpenTK mesh data update
        }

        public void Configure2DChart(object chartControl, MapMetadata metadata)
        {
            // This method would configure the 2D line chart
            // For Nevron NChartControl:
            // - Setting up smooth line series
            // - Configuring linear scale with grid styles
            // - Setting up axis labels and markers
        }

        public void Update2DChartSlice(object chartControl, byte[] data, int sliceIndex, MapViewerState state)
        {
            if (chartControl == null || data == null) return;

            try
            {
                DataTable chartdt = new DataTable();
                chartdt.Columns.Add("X", typeof(double));
                chartdt.Columns.Add("Y", typeof(double));
                
                double valcount = 0;
                int offsetinmap = sliceIndex;
                int numberofrows = data.Length / state.Data.TableWidth;
                bool isSixteenBit = state.Data.IsSixteenBit;

                if (isSixteenBit)
                {
                    numberofrows /= 2;
                    offsetinmap *= 2;
                }

                for (int t = numberofrows - 1; t >= 0; t--)
                {
                    double yval = valcount;
                    double value;
                    
                    if (isSixteenBit)
                    {
                        int byteIndex1 = offsetinmap + (t * (state.Data.TableWidth * 2));
                        int byteValue1 = Convert.ToInt32(data.GetValue(byteIndex1));
                        value = Convert.ToDouble(byteValue1) * 256.0;
                        int byteIndex2 = byteIndex1 + 1;
                        int byteValue2 = Convert.ToInt32(data.GetValue(byteIndex2));
                        value += Convert.ToDouble(byteValue2);
                        if (value > 32000)
                        {
                            value = 65536 - value;
                            value = -value;
                        }
                    }
                    else
                    {
                        value = Convert.ToDouble(data.GetValue(offsetinmap + (t * state.Data.TableWidth)));
                    }
                    
                    value *= state.Configuration.CorrectionFactor;
                    value += state.Configuration.CorrectionOffset;
                    
                    if (state.Axes.YAxisValues.Length > valcount)
                    {
                        yval = Convert.ToDouble(state.Axes.YAxisValues.GetValue((int)valcount));
                    }

                    chartdt.Rows.Add(yval, value);
                    valcount++;
                }

                // TODO: Implement ZedGraph data update
            }
            catch (Exception E)
            {
                Console.WriteLine("Update2DChartSlice: " + E.Message);
            }
        }

    }
}
