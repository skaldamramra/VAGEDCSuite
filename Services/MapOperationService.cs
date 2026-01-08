using System;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    public class MapOperationService : IMapOperationService
    {
        private readonly IDataConversionService _dataConversionService;

        public MapOperationService(IDataConversionService dataConversionService)
        {
            _dataConversionService = dataConversionService ?? throw new ArgumentNullException(nameof(dataConversionService));
        }

        public void ApplyOperation(
            MapViewerState state,
            OperationType type,
            double operand,
            object[] selectedCells,
            Func<int, object, object> getCellValue,
            Action<int, object, string> updateCell)
        {
            if (selectedCells == null || selectedCells.Length == 0) return;

            foreach (var cellObj in selectedCells)
            {
                try
                {
                    int rowHandle = 0;
                    object column = null;

                    if (cellObj is System.Windows.Forms.DataGridViewCell)
                    {
                        var dgCell = (System.Windows.Forms.DataGridViewCell)cellObj;
                        rowHandle = dgCell.RowIndex;
                        column = dgCell.ColumnIndex;
                    }
                    else
                    {
                        // Fallback for other types if necessary
                        continue;
                    }

                    object cellVal = getCellValue(rowHandle, column);
                    if (cellVal == null) continue;

                    int rawValue = _dataConversionService.ParseValue(cellVal.ToString(), state.Configuration.ViewType);
                    double workingValue = rawValue;

                    // If in Easy view, we apply the operation to the "real world" value
                    if (state.Configuration.ViewType == ViewType.Easy)
                    {
                        workingValue = _dataConversionService.ApplyCorrection(
                            rawValue, 
                            state.Configuration.CorrectionFactor, 
                            state.Configuration.CorrectionOffset);
                    }

                    switch (type)
                    {
                        case OperationType.Addition:
                            workingValue += operand;
                            break;
                        case OperationType.Multiplication:
                            workingValue *= operand;
                            break;
                        case OperationType.Division:
                            if (operand != 0) workingValue /= operand;
                            break;
                        case OperationType.Fill:
                            workingValue = operand;
                            break;
                        case OperationType.Percentage:
                            // Convert percentage (e.g. 5) to multiplier (e.g. 1.05)
                            double factor = 1.0 + (operand / 100.0);
                            workingValue *= factor;
                            break;
                    }

                    int finalRawValue;
                    if (state.Configuration.ViewType == ViewType.Easy)
                    {
                        // Convert back to raw value
                        double factor = state.Configuration.CorrectionFactor != 0 ? state.Configuration.CorrectionFactor : 1;
                        finalRawValue = (int)Math.Round((workingValue - state.Configuration.CorrectionOffset) / factor);
                    }
                    else
                    {
                        finalRawValue = (int)Math.Round(workingValue);
                    }

                    // Clamp based on bit width
                    int max = state.Data.IsSixteenBit ? 0xFFFF : 0xFF;
                    if (finalRawValue > max) finalRawValue = max;
                    if (finalRawValue < 0) finalRawValue = 0;

                    string formattedValue = _dataConversionService.FormatValue(
                        finalRawValue,
                        state.Configuration.ViewType,
                        state.Data.IsSixteenBit);

                    updateCell(rowHandle, column, formattedValue);
                }
                catch (Exception ex)
                {
                    // Log error for specific cell but continue with others
                    Console.WriteLine($"MapOperationService.ApplyOperation cell error: {ex.Message}");
                }
            }
        }
    }
}