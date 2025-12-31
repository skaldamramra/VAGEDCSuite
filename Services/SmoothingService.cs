using System;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Provides smoothing algorithms for map data
    /// </summary>
    public class SmoothingService : ISmoothingService
    {
        private readonly IDataConversionService _conversionService;
        
        public SmoothingService(IDataConversionService conversionService)
        {
            _conversionService = conversionService;
        }
        
        public void SmoothLinear(object[] cells, object gridView)
        {
            if (cells == null || cells.Length < 2)
                return;
                
            // Linear smoothing: interpolate between first and last value
            if (cells.Length == 2)
            {
                return;
            }
            
            int firstValue = ParseCellValue(cells[0]);
            int lastValue = ParseCellValue(cells[cells.Length - 1]);
            
            double step = (double)(lastValue - firstValue) / (cells.Length - 1);
            
            for (int i = 1; i < cells.Length - 1; i++)
            {
                int interpolatedValue = firstValue + (int)(step * i);
                SetCellValue(cells[i], interpolatedValue);
            }
        }
        
        public void SmoothProportional(object[] cells, object gridView, int[] xAxis, int[] yAxis)
        {
            if (cells == null || cells.Length == 0)
                return;
                
            try
            {
                // Get boundaries for this selection
                int max_column = 0;
                int min_column = 0xFFFF;
                int max_row = 0;
                int min_row = 0xFFFF;
                
                // Parse cell positions from the cells array (assuming format: [rowHandle, colIndex, value, ...])
                for (int i = 0; i < cells.Length; i += 3)
                {
                    if (i + 1 >= cells.Length) break;
                    
                    int rowhandle = Convert.ToInt32(cells[i]);
                    int colindex = Convert.ToInt32(cells[i + 1]);
                    
                    if (colindex > max_column) max_column = colindex;
                    if (colindex < min_column) min_column = colindex;
                    if (rowhandle > max_row) max_row = rowhandle;
                    if (rowhandle < min_row) min_row = rowhandle;
                }
                
                if (max_column == min_column)
                {
                    // One column selected only - smooth along Y axis
                    int topValue = ParseCellValue(cells[2]); // First cell value
                    int bottomValue = ParseCellValue(cells[cells.Length - 1]);
                    
                    double diffvalue = topValue - bottomValue;
                    double[] yaxisvalues = new double[(cells.Length / 3)];
                    
                    for (int q = 0; q < yaxisvalues.Length; q++)
                    {
                        int cellIndex = q * 3;
                        if (cellIndex + 1 < cells.Length)
                        {
                            int rowHandle = Convert.ToInt32(cells[cellIndex]);
                            // Get corresponding Y axis value
                            if (yAxis != null && rowHandle < yAxis.Length)
                            {
                                yaxisvalues[q] = Convert.ToDouble(yAxis[yAxis.Length - min_row - q - 1]);
                            }
                        }
                    }
                    
                    double yaxisdiff = yaxisvalues[0] - yaxisvalues[yaxisvalues.Length - 1];
                    
                    for (int t = 1; t < yaxisvalues.Length - 1; t++)
                    {
                        if (yaxisdiff != 0)
                        {
                            double newvalue = bottomValue + (diffvalue * ((yaxisvalues[0] - yaxisvalues[t]) / yaxisdiff));
                            newvalue = Math.Round(newvalue, 0);
                            
                            // Set the cell value at position min_row + t
                            int targetIndex = t * 3;
                            if (targetIndex < cells.Length)
                            {
                                SetCellValue(cells[targetIndex], (int)newvalue);
                            }
                        }
                    }
                }
                else if (max_row == min_row)
                {
                    // One row selected only - smooth along X axis
                    int topValue = ParseCellValue(cells[2]);
                    int bottomValue = ParseCellValue(cells[1]);
                    
                    double diffvalue = topValue - bottomValue;
                    double[] xaxisvalues = new double[(cells.Length / 3)];
                    
                    for (int q = 0; q < xaxisvalues.Length; q++)
                    {
                        int cellIndex = q * 3;
                        if (cellIndex + 1 < cells.Length)
                        {
                            int colIndex = Convert.ToInt32(cells[cellIndex + 1]);
                            // Get corresponding X axis value
                            if (xAxis != null && colIndex < xAxis.Length)
                            {
                                xaxisvalues[q] = Convert.ToDouble(xAxis[xAxis.Length - min_column - q - 1]);
                            }
                        }
                    }
                    
                    double xaxisdiff = xaxisvalues[0] - xaxisvalues[xaxisvalues.Length - 1];
                    
                    for (int t = 1; t < xaxisvalues.Length - 1; t++)
                    {
                        if (xaxisdiff != 0)
                        {
                            double newvalue = bottomValue + (diffvalue * ((xaxisvalues[0] - xaxisvalues[t]) / xaxisdiff));
                            newvalue = Math.Round(newvalue, 0);
                            
                            // Set the cell value at position min_column + t
                            int targetIndex = t * 3;
                            if (targetIndex < cells.Length)
                            {
                                SetCellValue(cells[targetIndex], (int)newvalue);
                            }
                        }
                    }
                }
                else
                {
                    // Block selected - 2D interpolation on 4 corners
                    int topLeftValue = ParseCellValue(cells[0]);
                    int topRightValue = ParseCellValue(cells[1]);
                    int bottomLeftValue = ParseCellValue(cells[cells.Length - 2]);
                    int bottomRightValue = ParseCellValue(cells[cells.Length - 1]);
                    
                    double[] xaxisvalues = new double[max_column - min_column + 1];
                    double[] yaxisvalues = new double[max_row - min_row + 1];
                    
                    for (int q = 0; q <= (max_column - min_column); q++)
                    {
                        if (xAxis != null && q < xAxis.Length)
                        {
                            xaxisvalues[q] = Convert.ToDouble(xAxis[xAxis.Length - min_column - q - 1]);
                        }
                    }
                    
                    for (int q = 0; q <= (max_row - min_row); q++)
                    {
                        if (yAxis != null && q < yAxis.Length)
                        {
                            yaxisvalues[q] = Convert.ToDouble(yAxis[yAxis.Length - min_row - q - 1]);
                        }
                    }
                    
                    double xaxisdiff = xaxisvalues[0] - xaxisvalues[max_column - min_column];
                    double yaxisdiff = yaxisvalues[0] - yaxisvalues[max_row - min_row];
                    int xvaluediff = ((topRightValue - topLeftValue) + (bottomRightValue - bottomLeftValue)) / 2;
                    int yvaluediff = ((topLeftValue - bottomLeftValue) + (topRightValue - bottomRightValue)) / 2;
                    
                    for (int tely = 0; tely <= max_row - min_row; tely++)
                    {
                        for (int telx = 0; telx <= max_column - min_column; telx++)
                        {
                            double xportion = 0;
                            double yportion = 0;
                            
                            if (xaxisdiff != 0)
                            {
                                xportion = (xaxisvalues[max_column - min_column - telx] - xaxisvalues[max_column - min_column]) / xaxisdiff;
                            }
                            if (yaxisdiff != 0)
                            {
                                yportion = (yaxisvalues[tely] - yaxisvalues[max_row - min_row]) / yaxisdiff;
                            }
                            
                            double newvalue = bottomLeftValue + ((xvaluediff * xportion) + (yvaluediff * yportion));
                            
                            // Set the cell value at position (min_row + tely, min_column + telx)
                            int targetIndex = (tely * (max_column - min_column + 1) + telx) * 3;
                            if (targetIndex < cells.Length)
                            {
                                SetCellValue(cells[targetIndex], (int)Math.Round(newvalue));
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("SmoothProportional: " + E.Message);
            }
        }
        
        private int ParseCellValue(object cell)
        {
            if (cell == null)
                return 0;
                
            try
            {
                return int.Parse(cell.ToString());
            }
            catch
            {
                return 0;
            }
        }
        
        private void SetCellValue(object cell, int value)
        {
            // Placeholder - actual implementation would set the cell value through gridView API
            // For now, we just update the object if it's mutable
            try
            {
                if (cell != null && cell is System.Collections.IDictionary)
                {
                    // Could store in a dictionary for later retrieval
                }
            }
            catch
            {
            }
        }
    }
}
