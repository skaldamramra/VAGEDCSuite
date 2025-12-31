using System.Data;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles conversion between different data representations
    /// </summary>
    public interface IDataConversionService
    {
        /// <summary>
        /// Converts byte array to DataTable for grid display
        /// </summary>
        DataTable ConvertToDataTable(MapData data, ViewConfiguration config);
        
        /// <summary>
        /// Converts DataTable back to byte array
        /// </summary>
        byte[] ConvertFromDataTable(DataTable table, MapData data, ViewConfiguration config);
        
        /// <summary>
        /// Converts a single value based on view type
        /// </summary>
        string FormatValue(int value, ViewType viewType, bool isSixteenBit);
        
        /// <summary>
        /// Parses a string value back to integer
        /// </summary>
        int ParseValue(string value, ViewType viewType);
        
        /// <summary>
        /// Applies correction factor and offset
        /// </summary>
        double ApplyCorrection(int rawValue, double factor, double offset);
    }
}
