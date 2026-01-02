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
            DevExpress.XtraGrid.Views.Base.GridCell[] selectedCells,
            Func<int, DevExpress.XtraGrid.Columns.GridColumn, object> getCellValue,
            Action<int, DevExpress.XtraGrid.Columns.GridColumn, string> updateCell)
        {
            if (selectedCells == null || selectedCells.Length == 0) return;

            foreach (var cell in selectedCells)
            {
                try
                {
                    object cellVal = getCellValue(cell.RowHandle, cell.Column);
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

                    updateCell(cell.RowHandle, cell.Column, formattedValue);
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