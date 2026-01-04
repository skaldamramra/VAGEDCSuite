using System;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    public enum OperationType
    {
        Addition,
        Multiplication,
        Division,
        Fill
    }

    public interface IMapOperationService
    {
        /// <summary>
        /// Applies a mathematical operation to a set of cells.
        /// </summary>
        /// <param name="state">Current viewer state</param>
        /// <param name="type">Operation type (Add, Mult, etc)</param>
        /// <param name="operand">The value to apply</param>
        /// <param name="selectedCells">Collection of cells to modify</param>
        /// <param name="getCellValue">Callback to get raw cell value</param>
        /// <param name="updateCell">Callback to update cell with formatted value</param>
        void ApplyOperation(
            MapViewerState state,
            OperationType type,
            double operand,
            object[] selectedCells,
            Func<int, object, object> getCellValue,
            Action<int, object, string> updateCell);
    }
}