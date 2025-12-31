using System;
using System.Data;
using System.Drawing;
using Nevron.Chart;
using Nevron.GraphicsCore;
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
            if (surfaceSeries == null || data == null) return;

            try
            {
                NMeshSurfaceSeries surface = (NMeshSurfaceSeries)surfaceSeries;
                int rowcount = 0;
                now_realMaxValue = double.MinValue;
                now_realMinValue = double.MaxValue;

                foreach (DataRow dr in data.Rows)
                {
                    for (int t = 0; t < data.Columns.Count; t++)
                    {
                        double value = 0;
                        if (config.ViewType == ViewType.Easy || config.ViewType == ViewType.Decimal)
                        {
                            value = Convert.ToInt32(dr[t]);
                        }
                        else if (config.ViewType == ViewType.Hexadecimal)
                        {
                            value = Convert.ToInt32(dr[t].ToString(), 16);
                        }

                        if (config.ViewType != ViewType.Decimal && config.ViewType != ViewType.Hexadecimal && config.ViewType != ViewType.ASCII)
                        {
                            value *= config.CorrectionFactor;
                            // Note: IsCompareViewer should be passed separately or stored in state
                        }
                        
                        surface.Data.SetValue(rowcount, t, value, rowcount, t);
                        
                        if (value > now_realMaxValue) now_realMaxValue = value;
                        if (value < now_realMinValue) now_realMinValue = value;
                    }
                    rowcount++;
                }

                // Apply color palette
                if (now_realMaxValue != double.MinValue)
                {
                    surface.Palette.Clear();
                    double diff = now_realMaxValue - now_realMinValue;
                    
                    // VAGEDC Dark Skin: Blue→Green→Yellow→Orange→Red gradient
                    surface.Palette.Add(now_realMinValue, Color.FromArgb(30, 58, 138));  // Navy Blue for low values
                    surface.Palette.Add(now_realMinValue + 0.20 * diff, Color.FromArgb(16, 185, 129));  // Emerald Green
                    surface.Palette.Add(now_realMinValue + 0.40 * diff, Color.Yellow);
                    surface.Palette.Add(now_realMinValue + 0.60 * diff, Color.Orange);
                    surface.Palette.Add(now_realMinValue + 0.80 * diff, Color.OrangeRed);
                    surface.Palette.Add(now_realMaxValue, Color.Red);
                    
                    surface.PaletteSteps = 5;
                    surface.AutomaticPalette = false;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Update3DChartData: " + E.Message);
            }
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

                // Update the chart series
                NChart chart = (NChart)chartControl;
                if (chart.Series.Count > 0)
                {
                    NSmoothLineSeries line = (NSmoothLineSeries)chart.Series[0];
                    line.ClearDataPoints();
                    
                    foreach (DataRow dr in chartdt.Rows)
                    {
                        line.AddDataPoint(new NDataPoint(Convert.ToDouble(dr["X"]), Convert.ToDouble(dr["Y"])));
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Update2DChartSlice: " + E.Message);
            }
        }

        /// <summary>
        /// Fills 3D surface data from DataTable
        /// </summary>
        public void FillSurfaceData(NMeshSurfaceSeries surface, DataTable dt, ViewType viewType, double correctionFactor, double correctionOffset, bool isCompareViewer)
        {
            try
            {
                int rowcount = 0;
                now_realMaxValue = double.MinValue;
                now_realMinValue = double.MaxValue;

                foreach (DataRow dr in dt.Rows)
                {
                    for (int t = 0; t < dt.Columns.Count; t++)
                    {
                        double value = 0;
                        if (viewType == ViewType.Easy || viewType == ViewType.Decimal)
                        {
                            value = Convert.ToInt32(dr[t]);
                        }
                        else if (viewType == ViewType.Hexadecimal)
                        {
                            value = Convert.ToInt32(dr[t].ToString(), 16);
                        }

                        if (viewType != ViewType.Decimal && viewType != ViewType.Hexadecimal && viewType != ViewType.ASCII)
                        {
                            value *= correctionFactor;
                            if (!isCompareViewer) value += correctionOffset;
                        }
                        
                        surface.Data.SetValue(rowcount, t, value, rowcount, t);
                        
                        if (value > now_realMaxValue) now_realMaxValue = value;
                        if (value < now_realMinValue) now_realMinValue = value;
                    }
                    rowcount++;
                }

                // Apply palette
                if (now_realMaxValue != double.MinValue)
                {
                    surface.Palette.Clear();
                    double diff = now_realMaxValue - now_realMinValue;
                    
                    surface.Palette.Add(now_realMinValue, Color.FromArgb(30, 58, 138));
                    surface.Palette.Add(now_realMinValue + 0.20 * diff, Color.FromArgb(16, 185, 129));
                    surface.Palette.Add(now_realMinValue + 0.40 * diff, Color.Yellow);
                    surface.Palette.Add(now_realMinValue + 0.60 * diff, Color.Orange);
                    surface.Palette.Add(now_realMinValue + 0.80 * diff, Color.OrangeRed);
                    surface.Palette.Add(now_realMaxValue, Color.Red);
                    
                    surface.PaletteSteps = 5;
                    surface.AutomaticPalette = false;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("FillSurfaceData: " + E.Message);
            }
        }

        /// <summary>
        /// Fills original map data to surface series
        /// </summary>
        public void FillOriginalData(NMeshSurfaceSeries surface, DataTable dt, byte[] originalContent, bool isSixteenBit, ViewType viewType, double correctionFactor, double correctionOffset)
        {
            try
            {
                int rowcount = dt.Rows.Count;
                int colcount = dt.Columns.Count;

                for (int row = 0; row < rowcount; row++)
                {
                    for (int col = 0; col < colcount; col++)
                    {
                        try
                        {
                            double value = 0;
                            
                            if (isSixteenBit)
                            {
                                int indexinmap = ((row * colcount) + col) * 2;
                                Int32 ivalue = Convert.ToInt32(originalContent[indexinmap]) * 256;
                                ivalue += Convert.ToInt32(originalContent[indexinmap + 1]);

                                if (ivalue > 32000)
                                {
                                    ivalue = 65536 - ivalue;
                                    ivalue = -ivalue;
                                }

                                value = ivalue;
                            }
                            else
                            {
                                int indexinmap = ((row * colcount) + col);
                                Int32 ivalue = Convert.ToInt32(originalContent[indexinmap]);
                                value = ivalue;
                            }

                            if (viewType != ViewType.Decimal && viewType != ViewType.Hexadecimal && viewType != ViewType.ASCII)
                            {
                                value *= correctionFactor;
                                value += correctionOffset;
                            }
                            
                            surface.Data.SetValue((rowcount - 1) - row, col, value, (rowcount - 1) - row, col);
                        }
                        catch (Exception E)
                        {
                            Console.WriteLine("FillOriginalData cell: " + E.Message);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("FillOriginalData: " + E.Message);
            }
        }

        /// <summary>
        /// Fills compare data to surface series
        /// </summary>
        public void FillCompareData(NMeshSurfaceSeries surface, DataTable dt, byte[] compareContent, bool isSixteenBit, ViewType viewType, double correctionFactor, double correctionOffset)
        {
            try
            {
                int rowcount = dt.Rows.Count;
                int colcount = dt.Columns.Count;

                for (int row = 0; row < rowcount; row++)
                {
                    for (int col = colcount - 1; col >= 0; col--)
                    {
                        if (isSixteenBit)
                        {
                            int indexinmap = ((row * colcount) + col) * 2;
                            Int32 ivalue = Convert.ToInt32(compareContent[indexinmap]) * 256;
                            ivalue += Convert.ToInt32(compareContent[indexinmap + 1]);

                            if (ivalue > 32000)
                            {
                                ivalue = 65536 - ivalue;
                                ivalue = -ivalue;
                            }

                            double value = ivalue;
                            
                            if (viewType != ViewType.Decimal && viewType != ViewType.Hexadecimal && viewType != ViewType.ASCII)
                            {
                                value *= correctionFactor;
                                value += correctionOffset;
                            }
                            
                            surface.Data.SetValue((rowcount - 1) - row, col, value, (rowcount - 1) - row, col);
                        }
                        else
                        {
                            int indexinmap = ((row * colcount) + col);
                            Int32 ivalue = Convert.ToInt32(compareContent[indexinmap]);
                            
                            double value = ivalue;
                            
                            if (viewType != ViewType.Decimal && viewType != ViewType.Hexadecimal && viewType != ViewType.ASCII)
                            {
                                value *= correctionFactor;
                                value += correctionOffset;
                            }
                            
                            surface.Data.SetValue((rowcount - 1) - row, col, value, (rowcount - 1) - row, col);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("FillCompareData: " + E.Message);
            }
        }
    }
}
