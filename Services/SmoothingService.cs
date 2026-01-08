using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Provides smoothing algorithms for map data
    /// </summary>
    public class SmoothingService : ISmoothingService
    {
        private readonly IDataConversionService _conversionService;
        
        // Blending factor for neighbor influence (0.0 = no neighbor influence, 1.0 = full neighbor)
        private const double NeighborBlendFactor = 0.25;

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
                
                // Parse cell positions from DataGridViewCell objects
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i] is DataGridViewCell gc)
                    {
                        int colindex = gc.ColumnIndex;
                        int rowhandle = gc.RowIndex;
                        
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
                        if (cells[q] is DataGridViewCell gc && yAxis != null)
                        {
                            int rowHandle = gc.RowIndex;
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
                        if (cells[q] is DataGridViewCell gc && xAxis != null)
                        {
                            int colIndex = gc.ColumnIndex;
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
                                if (cells[i] is DataGridViewCell gc)
                                {
                                    if (gc.RowIndex == min_row + tely && gc.ColumnIndex == min_column + telx)
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

        /// <summary>
        /// Smooths selected cells using interpolation between 4 corners.
        /// Compatible with MapViewerEx GridCell objects.
        /// </summary>
        public void SmoothInterpolated(object[] cells, object gridView, int[] xAxis, int[] yAxis)
        {
            if (cells == null || cells.Length <= 2) return;

            try
            {
                // Get boundaries for this selection
                int max_column = 0;
                int min_column = 0xFFFF;
                int max_row = 0;
                int min_row = 0xFFFF;

                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i] is DataGridViewCell gc)
                    {
                        if (gc.ColumnIndex > max_column) max_column = gc.ColumnIndex;
                        if (gc.ColumnIndex < min_column) min_column = gc.ColumnIndex;
                        if (gc.RowIndex > max_row) max_row = gc.RowIndex;
                        if (gc.RowIndex < min_row) min_row = gc.RowIndex;
                    }
                }

                if (max_column == min_column)
                {
                    // one column selected only
                    int top_value = ParseCellValue(cells[0], gridView);
                    int bottom_value = ParseCellValue(cells[cells.Length - 1], gridView);
                    double diffvalue = (top_value - bottom_value);
                    double[] yaxisvalues = new double[cells.Length];
                    for (int q = 0; q < cells.Length; q++)
                    {
                        yaxisvalues[q] = Convert.ToDouble(yAxis.GetValue(yAxis.Length - min_row - q - 1));
                    }
                    double yaxisdiff = yaxisvalues[0] - yaxisvalues[cells.Length - 1];

                    for (int t = 1; t < cells.Length - 1; t++)
                    {
                        double newvalue = bottom_value + (diffvalue * ((yaxisvalues[0] - yaxisvalues[t]) / (yaxisdiff != 0 ? yaxisdiff : 1)));
                        newvalue = Math.Round(newvalue, 0);
                        SetCellValue(cells[t], (int)newvalue, gridView);
                    }
                }
                else if (max_row == min_row)
                {
                    // one row selected only
                    int top_value = ParseCellValue(cells[0], gridView);
                    int bottom_value = ParseCellValue(cells[cells.Length - 1], gridView);
                    double diffvalue = (top_value - bottom_value);
                    double[] xaxisvalues = new double[cells.Length];
                    for (int q = 0; q < cells.Length; q++)
                    {
                        xaxisvalues[q] = Convert.ToDouble(xAxis.GetValue(xAxis.Length - min_column - q - 1));
                    }
                    double xaxisdiff = xaxisvalues[0] - xaxisvalues[cells.Length - 1];
                    for (int t = 1; t < cells.Length - 1; t++)
                    {
                        double newvalue = bottom_value + (diffvalue * ((xaxisvalues[0] - xaxisvalues[t]) / (xaxisdiff != 0 ? xaxisdiff : 1)));
                        newvalue = Math.Round(newvalue, 0);
                        SetCellValue(cells[t], (int)newvalue, gridView);
                    }
                }
                else
                {
                    // block selected
                    int top_leftvalue = ParseCellValue(cells[0], gridView);
                    int top_rightvalue = ParseCellValue(cells[1], gridView);
                    int bottom_leftvalue = ParseCellValue(cells[cells.Length - 2], gridView);
                    int bottom_rightvalue = ParseCellValue(cells[cells.Length - 1], gridView);
                    double[] xaxisvalues = new double[max_column - min_column + 1];
                    double[] yaxisvalues = new double[max_row - min_row + 1];

                    for (int q = 0; q <= (max_column - min_column); q++)
                    {
                        xaxisvalues[q] = Convert.ToDouble(xAxis.GetValue(xAxis.Length - min_column - q - 1));
                    }
                    for (int q = 0; q <= (max_row - min_row); q++)
                    {
                        yaxisvalues[q] = Convert.ToDouble(yAxis.GetValue(yAxis.Length - min_row - q - 1));
                    }
                    double xaxisdiff = xaxisvalues[0] - xaxisvalues[max_column - min_column];
                    double yaxisdiff = yaxisvalues[0] - yaxisvalues[max_row - min_row];
                    int xvaluediff = ((top_rightvalue - top_leftvalue) + (bottom_rightvalue - bottom_leftvalue)) / 2;
                    int yvaluediff = ((top_leftvalue - bottom_leftvalue) + (top_rightvalue - bottom_rightvalue)) / 2;

                    for (int tely = 0; tely <= max_row - min_row; tely++)
                    {
                        for (int telx = 0; telx <= max_column - min_column; telx++)
                        {
                            double xportion = (xaxisvalues[max_column - min_column - telx] - xaxisvalues[max_column - min_column]) / (xaxisdiff != 0 ? xaxisdiff : 1);
                            double yportion = (yaxisvalues[tely] - yaxisvalues[max_row - min_row]) / (yaxisdiff != 0 ? yaxisdiff : 1);

                            float newvalue = (float)(bottom_leftvalue + ((xvaluediff * xportion) + (yvaluediff * yportion)));
                            
                            // Find the cell at (min_row + tely, min_column + telx)
                            for (int i = 0; i < cells.Length; i++)
                            {
                                if (cells[i] is DataGridViewCell gc && gc.RowIndex == min_row + tely && gc.ColumnIndex == min_column + telx)
                                {
                                    SetCellValue(cells[i], (int)Math.Round(newvalue), gridView);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("SmoothInterpolated: " + E.Message);
            }
        }

        /// <summary>
        /// Smooths selected cells using linear interpolation with neighbor awareness.
        /// Provides smoother transitions by blending with surrounding cells.
        /// </summary>
        public void SmoothLinearWithNeighbors(object[] cells, object gridView, int[] xAxis, int[] yAxis, bool isUpsideDown = false)
        {
            if (cells == null || cells.Length < 2 || !(gridView is DataGridView gv))
                return;

            var analyzer = new SelectionAnalyzer(cells, gv);

            try
            {
                if (analyzer.IsSingleColumn)
                {
                    // Single column - smooth along Y axis
                    int topValue = analyzer.GetCornerValue(Corner.TopLeft);
                    int bottomValue = analyzer.GetCornerValue(Corner.BottomLeft);
                    double step = (double)(bottomValue - topValue) / (analyzer.RowCount - 1);

                    for (int row = analyzer.MinRow + 1; row < analyzer.MaxRow; row++)
                    {
                        int interpolatedValue = topValue + (int)(step * (row - analyzer.MinRow));
                        
                        // Blend with neighbor average
                        double neighborAvg = analyzer.GetNeighborAverage(row, analyzer.MinColumn);
                        if (neighborAvg > 0)
                        {
                            interpolatedValue = (int)((interpolatedValue * (1 - NeighborBlendFactor)) +
                                                       (neighborAvg * NeighborBlendFactor));
                        }

                        SetCellValue(analyzer.FindCell(row, analyzer.MinColumn), interpolatedValue, gv);
                    }
                }
                else if (analyzer.IsSingleRow)
                {
                    // Single row - smooth along X axis
                    int leftValue = analyzer.GetCornerValue(Corner.TopLeft);
                    int rightValue = analyzer.GetCornerValue(Corner.TopRight);
                    double step = (double)(rightValue - leftValue) / (analyzer.ColumnCount - 1);

                    for (int col = analyzer.MinColumn + 1; col < analyzer.MaxColumn; col++)
                    {
                        int interpolatedValue = leftValue + (int)(step * (col - analyzer.MinColumn));
                        
                        // Blend with neighbor average
                        double neighborAvg = analyzer.GetNeighborAverage(analyzer.MinRow, col);
                        if (neighborAvg > 0)
                        {
                            interpolatedValue = (int)((interpolatedValue * (1 - NeighborBlendFactor)) +
                                                       (neighborAvg * NeighborBlendFactor));
                        }

                        SetCellValue(analyzer.FindCell(analyzer.MinRow, col), interpolatedValue, gv);
                    }
                }
                else
                {
                    // Block selected - apply linear smoothing per row with neighbor blending
                    for (int row = analyzer.MinRow; row <= analyzer.MaxRow; row++)
                    {
                        int leftValue = analyzer.GetCellValue(row, analyzer.MinColumn);
                        int rightValue = analyzer.GetCellValue(row, analyzer.MaxColumn);
                        double step = (double)(rightValue - leftValue) / (analyzer.ColumnCount - 1);

                        for (int col = analyzer.MinColumn + 1; col < analyzer.MaxColumn; col++)
                        {
                            int interpolatedValue = leftValue + (int)(step * (col - analyzer.MinColumn));
                            
                            // Blend with neighbor average
                            double neighborAvg = analyzer.GetNeighborAverage(row, col);
                            if (neighborAvg > 0)
                            {
                                interpolatedValue = (int)((interpolatedValue * (1 - NeighborBlendFactor)) +
                                                           (neighborAvg * NeighborBlendFactor));
                            }

                            SetCellValue(analyzer.FindCell(row, col), interpolatedValue, gv);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("SmoothLinearWithNeighbors: " + E.Message);
            }
        }

        /// <summary>
        /// Smooths selected cells proportionally with actual corner detection and neighbor blending.
        /// Uses axis values for interpolation, respecting non-linear axis scaling.
        /// </summary>
        public void SmoothProportionalWithNeighbors(object[] cells, object gridView, int[] xAxis, int[] yAxis, bool isUpsideDown = false)
        {
            if (cells == null || cells.Length < 2 || !(gridView is DataGridView gv))
                return;

            var analyzer = new SelectionAnalyzer(cells, gv);

            try
            {
                if (analyzer.IsSingleColumn)
                {
                    // Single column - smooth along Y axis using proportional interpolation
                    int topValue = analyzer.GetCornerValue(Corner.TopLeft);
                    int bottomValue = analyzer.GetCornerValue(Corner.BottomLeft);
                    double diffvalue = topValue - bottomValue;

                    for (int row = analyzer.MinRow + 1; row < analyzer.MaxRow; row++)
                    {
                        // Calculate position along Y axis
                        double yPos = SelectionAnalyzer.GetAxisValue(yAxis, row, isUpsideDown);
                        double yStart = SelectionAnalyzer.GetAxisValue(yAxis, analyzer.MinRow, isUpsideDown);
                        double yEnd = SelectionAnalyzer.GetAxisValue(yAxis, analyzer.MaxRow, isUpsideDown);
                        
                        if (yEnd != yStart)
                        {
                            double t = (yPos - yStart) / (yEnd - yStart);
                            double newvalue = bottomValue + (diffvalue * t);
                            newvalue = Math.Round(newvalue, 0);

                            // Blend with neighbor average
                            double neighborAvg = analyzer.GetNeighborAverage(row, analyzer.MinColumn);
                            if (neighborAvg > 0)
                            {
                                newvalue = ((newvalue * (1 - NeighborBlendFactor)) +
                                            (neighborAvg * NeighborBlendFactor));
                            }

                            SetCellValue(analyzer.FindCell(row, analyzer.MinColumn), (int)newvalue, gv);
                        }
                    }
                }
                else if (analyzer.IsSingleRow)
                {
                    // Single row - smooth along X axis using proportional interpolation
                    int leftValue = analyzer.GetCornerValue(Corner.TopLeft);
                    int rightValue = analyzer.GetCornerValue(Corner.TopRight);
                    double diffvalue = rightValue - leftValue;

                    for (int col = analyzer.MinColumn + 1; col < analyzer.MaxColumn; col++)
                    {
                        // Calculate position along X axis
                        double xPos = SelectionAnalyzer.GetAxisValue(xAxis, col, isUpsideDown);
                        double xStart = SelectionAnalyzer.GetAxisValue(xAxis, analyzer.MinColumn, isUpsideDown);
                        double xEnd = SelectionAnalyzer.GetAxisValue(xAxis, analyzer.MaxColumn, isUpsideDown);

                        if (xEnd != xStart)
                        {
                            double t = (xPos - xStart) / (xEnd - xStart);
                            double newvalue = leftValue + (diffvalue * t);
                            newvalue = Math.Round(newvalue, 0);

                            // Blend with neighbor average
                            double neighborAvg = analyzer.GetNeighborAverage(analyzer.MinRow, col);
                            if (neighborAvg > 0)
                            {
                                newvalue = ((newvalue * (1 - NeighborBlendFactor)) +
                                            (neighborAvg * NeighborBlendFactor));
                            }

                            SetCellValue(analyzer.FindCell(analyzer.MinRow, col), (int)newvalue, gv);
                        }
                    }
                }
                else
                {
                    // Block selected - use actual corner values for 2D proportional interpolation
                    int tl = analyzer.GetCornerValue(Corner.TopLeft);
                    int tr = analyzer.GetCornerValue(Corner.TopRight);
                    int bl = analyzer.GetCornerValue(Corner.BottomLeft);
                    int br = analyzer.GetCornerValue(Corner.BottomRight);

                    for (int row = analyzer.MinRow; row <= analyzer.MaxRow; row++)
                    {
                        double yPos = SelectionAnalyzer.GetAxisValue(yAxis, row, isUpsideDown);
                        double yStart = SelectionAnalyzer.GetAxisValue(yAxis, analyzer.MinRow, isUpsideDown);
                        double yEnd = SelectionAnalyzer.GetAxisValue(yAxis, analyzer.MaxRow, isUpsideDown);

                        for (int col = analyzer.MinColumn; col <= analyzer.MaxColumn; col++)
                        {
                            if (col == analyzer.MinColumn || col == analyzer.MaxColumn ||
                                row == analyzer.MinRow || row == analyzer.MaxRow)
                                continue; // Skip corners

                            double xPos = SelectionAnalyzer.GetAxisValue(xAxis, col, isUpsideDown);
                            double xStart = SelectionAnalyzer.GetAxisValue(xAxis, analyzer.MinColumn, isUpsideDown);
                            double xEnd = SelectionAnalyzer.GetAxisValue(xAxis, analyzer.MaxColumn, isUpsideDown);

                            if (xEnd != xStart && yEnd != yStart)
                            {
                                double tx = (xPos - xStart) / (xEnd - xStart);
                                double ty = (yPos - yStart) / (yEnd - yStart);

                                // Bilinear interpolation
                                double topRow = tr * tx + tl * (1 - tx);
                                double bottomRow = br * tx + bl * (1 - tx);
                                double newvalue = bottomRow * ty + topRow * (1 - ty);
                                newvalue = Math.Round(newvalue, 0);

                                // Blend with neighbor average
                                double neighborAvg = analyzer.GetNeighborAverage(row, col);
                                if (neighborAvg > 0)
                                {
                                    newvalue = ((newvalue * (1 - NeighborBlendFactor)) +
                                                (neighborAvg * NeighborBlendFactor));
                                }

                                SetCellValue(analyzer.FindCell(row, col), (int)newvalue, gv);
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("SmoothProportionalWithNeighbors: " + E.Message);
            }
        }

        /// <summary>
        /// Applies true bilinear interpolation for 2D blocks using axis values.
        /// Provides smooth transitions that respect both X and Y axis scaling.
        /// </summary>
        public void SmoothBilinear(object[] cells, object gridView, int[] xAxis, int[] yAxis, bool isUpsideDown = false)
        {
            if (cells == null || cells.Length < 4 || !(gridView is DataGridView gv))
                return;

            var analyzer = new SelectionAnalyzer(cells, gv);

            try
            {
                // Get actual corner values
                int tl = analyzer.GetCornerValue(Corner.TopLeft);
                int tr = analyzer.GetCornerValue(Corner.TopRight);
                int bl = analyzer.GetCornerValue(Corner.BottomLeft);
                int br = analyzer.GetCornerValue(Corner.BottomRight);

                // Get axis ranges
                double xStart = SelectionAnalyzer.GetAxisValue(xAxis, analyzer.MinColumn, isUpsideDown);
                double xEnd = SelectionAnalyzer.GetAxisValue(xAxis, analyzer.MaxColumn, isUpsideDown);
                double yStart = SelectionAnalyzer.GetAxisValue(yAxis, analyzer.MinRow, isUpsideDown);
                double yEnd = SelectionAnalyzer.GetAxisValue(yAxis, analyzer.MaxRow, isUpsideDown);

                double xRange = xEnd - xStart;
                double yRange = yEnd - yStart;

                if (xRange == 0 || yRange == 0)
                    return;

                // Apply bilinear interpolation to all cells in selection
                for (int row = analyzer.MinRow; row <= analyzer.MaxRow; row++)
                {
                    double yPos = SelectionAnalyzer.GetAxisValue(yAxis, row, isUpsideDown);
                    double ty = (yPos - yStart) / yRange;

                    for (int col = analyzer.MinColumn; col <= analyzer.MaxColumn; col++)
                    {
                        // Skip corners (already have values)
                        if ((row == analyzer.MinRow && col == analyzer.MinColumn) ||
                            (row == analyzer.MinRow && col == analyzer.MaxColumn) ||
                            (row == analyzer.MaxRow && col == analyzer.MinColumn) ||
                            (row == analyzer.MaxRow && col == analyzer.MaxColumn))
                            continue;

                        double xPos = SelectionAnalyzer.GetAxisValue(xAxis, col, isUpsideDown);
                        double tx = (xPos - xStart) / xRange;

                        // Bilinear interpolation formula:
                        // V(x,y) = V00*(1-tx)*(1-ty) + V10*tx*(1-ty) + V01*(1-tx)*ty + V11*tx*ty
                        double interpolatedValue =
                            tl * (1 - tx) * (1 - ty) +
                            tr * tx * (1 - ty) +
                            bl * (1 - tx) * ty +
                            br * tx * ty;

                        // Blend with neighbor average for smoother transitions at boundaries
                        if (analyzer.RowCount > 2 || analyzer.ColumnCount > 2)
                        {
                            double neighborAvg = analyzer.GetNeighborAverage(row, col);
                            if (neighborAvg > 0)
                            {
                                interpolatedValue = (interpolatedValue * (1 - NeighborBlendFactor)) +
                                                   (neighborAvg * NeighborBlendFactor);
                            }
                        }

                        int finalValue = (int)Math.Round(interpolatedValue);
                        SetCellValue(analyzer.FindCell(row, col), finalValue, gv);
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("SmoothBilinear: " + E.Message);
            }
        }

        private int ParseCellValue(object cell, object gridView)
        {
            if (cell == null)
                return 0;
                
            try
            {
                // Handle DataGridViewCell objects from MapViewerEx
                if (cell is DataGridViewCell gc && gridView is DataGridView gv)
                {
                    object value = gv.Rows[gc.RowIndex].Cells[gc.ColumnIndex].Value;
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
                // Handle DataGridViewCell objects from MapViewerEx
                if (cell is DataGridViewCell gc && gridView is DataGridView gv)
                {
                    // Determine format based on view type - use simple string format for now
                    string formattedValue = value.ToString();
                    
                    gv.Rows[gc.RowIndex].Cells[gc.ColumnIndex].Value = formattedValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("SetCellValue: " + ex.Message);
            }
        }
    }
}
