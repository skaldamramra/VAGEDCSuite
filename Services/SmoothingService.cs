using System;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
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
        
        /// <summary>
        /// Smooths selected cells using linear interpolation between first and last value.
        /// Compatible with MapViewerEx GridCell objects.
        /// </summary>
        public void SmoothLinear(object[] cells, object gridView)
        {
            if (cells == null || cells.Length < 2)
                return;
                
            // Linear smoothing: interpolate between first and last value
            if (cells.Length == 2)
            {
                return;
            }
            
            int firstValue = ParseCellValue(cells[0], gridView);
            int lastValue = ParseCellValue(cells[cells.Length - 1], gridView);
            
            double step = (double)(lastValue - firstValue) / (cells.Length - 1);
            
            for (int i = 1; i < cells.Length - 1; i++)
            {
                int interpolatedValue = firstValue + (int)(step * i);
                SetCellValue(cells[i], interpolatedValue, gridView);
            }
        }
        
        /// <summary>
        /// Smooths selected cells proportionally based on axis values.
        /// Compatible with MapViewerEx GridCell objects.
        /// </summary>
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
                
                // Parse cell positions from GridCell objects (cells are DevExpress.XtraGrid.Views.Base.GridCell)
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i] is GridCell)
                    {
                        GridCell gc = (GridCell)cells[i];
                        int colindex = gc.Column.AbsoluteIndex;
                        int rowhandle = gc.RowHandle;
                        
                        if (colindex > max_column) max_column = colindex;
                        if (colindex < min_column) min_column = colindex;
                        if (rowhandle > max_row) max_row = rowhandle;
                        if (rowhandle < min_row) min_row = rowhandle;
                    }
                }
                
                if (max_column == min_column)
                {
                    // One column selected only - smooth along Y axis
                    int topValue = ParseCellValue(cells[0], gridView);
                    int bottomValue = ParseCellValue(cells[cells.Length - 1], gridView);
                    
                    double diffvalue = topValue - bottomValue;
                    double[] yaxisvalues = new double[cells.Length];
                    
                    for (int q = 0; q < yaxisvalues.Length; q++)
                    {
                        if (cells[q] is GridCell && yAxis != null)
                        {
                            GridCell gc = (GridCell)cells[q];
                            int rowHandle = gc.RowHandle;
                            if (rowHandle < yAxis.Length)
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
                            
                            SetCellValue(cells[t], (int)newvalue, gridView);
                        }
                    }
                }
                else if (max_row == min_row)
                {
                    // One row selected only - smooth along X axis
                    int topValue = ParseCellValue(cells[0], gridView);
                    int bottomValue = ParseCellValue(cells[cells.Length - 1], gridView);
                    
                    double diffvalue = topValue - bottomValue;
                    double[] xaxisvalues = new double[cells.Length];
                    
                    for (int q = 0; q < xaxisvalues.Length; q++)
                    {
                        if (cells[q] is GridCell && xAxis != null)
                        {
                            GridCell gc = (GridCell)cells[q];
                            int colIndex = gc.Column.AbsoluteIndex;
                            if (colIndex < xAxis.Length)
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
                            
                            SetCellValue(cells[t], (int)newvalue, gridView);
                        }
                    }
                }
                else
                {
                    // Block selected - 2D interpolation on 4 corners
                    int topLeftValue = ParseCellValue(cells[0], gridView);
                    int topRightValue = ParseCellValue(cells[1], gridView);
                    int bottomLeftValue = ParseCellValue(cells[cells.Length - 2], gridView);
                    int bottomRightValue = ParseCellValue(cells[cells.Length - 1], gridView);
                    
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
                            
                            // Find the cell at (min_row + tely, min_column + telx)
                            for (int i = 0; i < cells.Length; i++)
                            {
                                if (cells[i] is GridCell)
                                {
                                    GridCell gc = (GridCell)cells[i];
                                    if (gc.RowHandle == min_row + tely && gc.Column.AbsoluteIndex == min_column + telx)
                                    {
                                        SetCellValue(cells[i], (int)Math.Round(newvalue), gridView);
                                        break;
                                    }
                                }
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
        
        private int ParseCellValue(object cell, object gridView)
        {
            if (cell == null)
                return 0;
                
            try
            {
                // Handle GridCell objects from MapViewerEx
                if (cell is GridCell && gridView is GridView)
                {
                    GridCell gc = (GridCell)cell;
                    GridView gv = (GridView)gridView;
                    
                    object value = gv.GetRowCellValue(gc.RowHandle, gc.Column);
                    return int.Parse(value.ToString());
                }
                return int.Parse(cell.ToString());
            }
            catch
            {
                return 0;
            }
        }
        
        private void SetCellValue(object cell, int value, object gridView)
        {
            try
            {
                // Handle GridCell objects from MapViewerEx
                if (cell is GridCell && gridView is GridView)
                {
                    GridCell gc = (GridCell)cell;
                    GridView gv = (GridView)gridView;
                    
                    // Determine format based on view type - use simple string format for now
                    string formattedValue = value.ToString();
                    
                    gv.SetRowCellValue(gc.RowHandle, gc.Column, formattedValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetCellValue: " + ex.Message);
            }
        }
    }
}
