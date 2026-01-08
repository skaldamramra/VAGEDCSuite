using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace VAGSuite.Services
{
    /// <summary>
    /// Analyzes cell selections and provides information about selection boundaries,
    /// corner cells, and neighbor data for smoothing operations.
    /// </summary>
    public class SelectionAnalyzer
    {
        private readonly object[] _cells;
        private readonly DataGridView _gridView;
        
        public int MinColumn { get; private set; }
        public int MaxColumn { get; private set; }
        public int MinRow { get; private set; }
        public int MaxRow { get; private set; }
        public int ColumnCount => MaxColumn - MinColumn + 1;
        public int RowCount => MaxRow - MinRow + 1;
        public bool IsSingleColumn => MaxColumn == MinColumn;
        public bool IsSingleRow => MaxRow == MinRow;
        public bool IsBlock => !IsSingleColumn && !IsSingleRow;

        public SelectionAnalyzer(object[] cells, DataGridView gridView)
        {
            _cells = cells ?? throw new ArgumentNullException(nameof(cells));
            _gridView = gridView ?? throw new ArgumentNullException(nameof(gridView));
            AnalyzeSelection();
        }

        private void AnalyzeSelection()
        {
            MinColumn = int.MaxValue;
            MaxColumn = 0;
            MinRow = int.MaxValue;
            MaxRow = 0;

            foreach (var cell in _cells)
            {
                if (cell is DataGridViewCell gc)
                {
                    MinColumn = Math.Min(MinColumn, gc.ColumnIndex);
                    MaxColumn = Math.Max(MaxColumn, gc.ColumnIndex);
                    MinRow = Math.Min(MinRow, gc.RowIndex);
                    MaxRow = Math.Max(MaxRow, gc.RowIndex);
                }
            }

            // Handle case where no valid cells found
            if (MinColumn == int.MaxValue)
            {
                MinColumn = MaxColumn = 0;
                MinRow = MaxRow = 0;
            }
        }

        /// <summary>
        /// Gets the value at a specific corner of the selection.
        /// </summary>
        public int GetCornerValue(Corner corner)
        {
            int targetCol, targetRow;

            switch (corner)
            {
                case Corner.TopLeft:
                    targetCol = MinColumn;
                    targetRow = MinRow;
                    break;
                case Corner.TopRight:
                    targetCol = MaxColumn;
                    targetRow = MinRow;
                    break;
                case Corner.BottomLeft:
                    targetCol = MinColumn;
                    targetRow = MaxRow;
                    break;
                case Corner.BottomRight:
                    targetCol = MaxColumn;
                    targetRow = MaxRow;
                    break;
                default:
                    return 0;
            }

            return GetCellValue(targetRow, targetCol);
        }

        /// <summary>
        /// Gets the value at a specific position, checking neighbors if within selection bounds.
        /// </summary>
        public int GetCellValue(int row, int col)
        {
            // First try to get from selection
            foreach (var cell in _cells)
            {
                if (cell is DataGridViewCell gc && gc.RowIndex == row && gc.ColumnIndex == col)
                {
                    return ParseCellValue(gc);
                }
            }

            // If not in selection, check grid neighbors (for boundary cells)
            if (_gridView != null && _gridView.Rows.Count > row && 
                _gridView.Columns.Count > col && col >= 0 && row >= 0)
            {
                return ParseCellValue(_gridView.Rows[row].Cells[col]);
            }

            return 0;
        }

        /// <summary>
        /// Gets the neighbor value at a specific position (outside selection).
        /// Returns 0 if out of bounds.
        /// </summary>
        public int GetNeighborValue(int row, int col)
        {
            if (_gridView == null) return 0;
            if (row < 0 || row >= _gridView.Rows.Count) return 0;
            if (col < 0 || col >= _gridView.Columns.Count) return 0;

            // Don't use values from within the selection
            foreach (var cell in _cells)
            {
                if (cell is DataGridViewCell gc && gc.RowIndex == row && gc.ColumnIndex == col)
                {
                    return 0; // Skip cells in selection
                }
            }

            return ParseCellValue(_gridView.Rows[row].Cells[col]);
        }

        /// <summary>
        /// Gets the average value of surrounding neighbors for a cell position.
        /// </summary>
        public double GetNeighborAverage(int row, int col)
        {
            var neighborValues = new List<int>();
            
            // Check 8 surrounding cells
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    
                    int neighborRow = row + dr;
                    int neighborCol = col + dc;
                    
                    // Only consider neighbors outside the selection
                    if (neighborRow < MinRow || neighborRow > MaxRow ||
                        neighborCol < MinColumn || neighborCol > MaxColumn)
                    {
                        int value = GetNeighborValue(neighborRow, neighborCol);
                        if (value != 0 || _gridView != null)
                        {
                            neighborValues.Add(value);
                        }
                    }
                }
            }

            if (neighborValues.Count == 0) return 0;
            return neighborValues.Average();
        }

        /// <summary>
        /// Gets the axis value at a specific index, handling upside-down mode.
        /// </summary>
        public static double GetAxisValue(int[] axisValues, int index, bool isUpsideDown = false)
        {
            if (axisValues == null || axisValues.Length == 0) return 0;
            
            int actualIndex = isUpsideDown ? (axisValues.Length - 1 - index) : index;
            if (actualIndex < 0 || actualIndex >= axisValues.Length) return 0;
            
            return Convert.ToDouble(axisValues[actualIndex]);
        }

        /// <summary>
        /// Gets all cell values in the selection as a 2D array.
        /// </summary>
        public int[,] GetSelectionValues()
        {
            var values = new int[RowCount, ColumnCount];
            
            for (int row = MinRow; row <= MaxRow; row++)
            {
                for (int col = MinColumn; col <= MaxColumn; col++)
                {
                    values[row - MinRow, col - MinColumn] = GetCellValue(row, col);
                }
            }
            
            return values;
        }

        private int ParseCellValue(DataGridViewCell cell)
        {
            if (cell?.Value == null) return 0;
            
            try
            {
                return int.Parse(cell.Value.ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Finds the cell at a specific position within the selection.
        /// </summary>
        public DataGridViewCell FindCell(int row, int col)
        {
            foreach (var cell in _cells)
            {
                if (cell is DataGridViewCell gc && gc.RowIndex == row && gc.ColumnIndex == col)
                {
                    return gc;
                }
            }
            return null;
        }
    }

    public enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}