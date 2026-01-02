using System;
using System.Data;
using System.Globalization;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles conversion between different data representations
    /// </summary>
    public class DataConversionService : IDataConversionService
    {
        public DataTable ConvertToDataTable(MapData data, ViewConfiguration config)
        {
            var dataTable = new DataTable();

            if (data == null || data.Content == null || data.Content.Length == 0)
                return dataTable;

            int tableWidth = data.TableWidth;
            if (tableWidth <= 0)
                tableWidth = 1;

            // Create columns
            for (int c = 0; c < tableWidth; c++)
            {
                dataTable.Columns.Add(c.ToString());
            }

            int numberRows = data.Content.Length / tableWidth;
            if (data.IsSixteenBit)
                numberRows /= 2;

            int mapOffset = 0;

            // Populate rows
            if (data.IsSixteenBit)
            {
                for (int i = 0; i < numberRows; i++)
                {
                    object[] objarr = new object[tableWidth];
                    for (int j = 0; j < tableWidth; j++)
                    {
                        int b = data.Content[mapOffset++];
                        b *= 256;
                        b += data.Content[mapOffset++];

                        if (b > 0xF000)
                        {
                            b = 0x10000 - b;
                            b = -b;
                        }

                        string formattedValue = FormatValue(b, config.ViewType, true);
                        objarr[j] = formattedValue;
                    }

                    if (config.IsUpsideDown)
                    {
                        System.Data.DataRow r = dataTable.NewRow();
                        r.ItemArray = objarr;
                        dataTable.Rows.InsertAt(r, 0);
                    }
                    else
                    {
                        dataTable.Rows.Add(objarr);
                    }
                }

                // Handle remaining bytes
                if (mapOffset < data.Content.Length)
                {
                    object[] objarr = new object[tableWidth];
                    int sicnt = 0;
                    for (int v = mapOffset; v < data.Content.Length - 1; v++)
                    {
                        if (mapOffset <= data.Content.Length - 1)
                        {
                            int b = data.Content[mapOffset++];
                            b *= 256;
                            b += data.Content[mapOffset++];

                            if (b > 0xF000)
                            {
                                b = 0x10000 - b;
                                b = -b;
                            }

                            string formattedValue = FormatValue(b, config.ViewType, true);
                            objarr[sicnt] = formattedValue;
                            sicnt++;
                        }
                    }

                    if (config.IsUpsideDown)
                    {
                        System.Data.DataRow r = dataTable.NewRow();
                        r.ItemArray = objarr;
                        dataTable.Rows.InsertAt(r, 0);
                    }
                    else
                    {
                        dataTable.Rows.Add(objarr);
                    }
                }
            }
            else
            {
                for (int i = 0; i < numberRows; i++)
                {
                    object[] objarr = new object[tableWidth];
                    for (int j = 0; j < tableWidth; j++)
                    {
                        int b = data.Content[mapOffset++];
                        string formattedValue = FormatValue(b, config.ViewType, false);
                        objarr[j] = formattedValue;
                    }

                    if (config.IsUpsideDown)
                    {
                        System.Data.DataRow r = dataTable.NewRow();
                        r.ItemArray = objarr;
                        dataTable.Rows.InsertAt(r, 0);
                    }
                    else
                    {
                        dataTable.Rows.Add(objarr);
                    }
                }

                // Handle remaining bytes
                if (mapOffset < data.Content.Length)
                {
                    object[] objarr = new object[tableWidth];
                    int sicnt = 0;
                    for (int v = mapOffset; v < data.Content.Length; v++)
                    {
                        int b = data.Content[mapOffset++];
                        string formattedValue = FormatValue(b, config.ViewType, false);
                        objarr[sicnt] = formattedValue;
                        sicnt++;
                    }

                    if (config.IsUpsideDown)
                    {
                        System.Data.DataRow r = dataTable.NewRow();
                        r.ItemArray = objarr;
                        dataTable.Rows.InsertAt(r, 0);
                    }
                    else
                    {
                        dataTable.Rows.Add(objarr);
                    }
                }
            }

            return dataTable;
        }
        
        public byte[] ConvertFromDataTable(DataTable table, MapData data, ViewConfiguration config, bool upsidedown = false)
        {
            if (table == null || table.Rows.Count == 0 || data == null || data.Length == 0)
                return new byte[0];

            var result = new byte[data.Length];
            int cellcount = 0;

            // Process rows in the appropriate order
            for (int rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
            {
                int actualRowIndex = rowIndex;
                if (upsidedown)
                {
                    actualRowIndex = table.Rows.Count - 1 - rowIndex;
                }

                DataRow row = table.Rows[actualRowIndex];
                foreach (object o in row.ItemArray)
                {
                    if (o != null && o != DBNull.Value)
                    {
                        if (cellcount < result.Length)
                        {
                            int value = ParseValue(o.ToString(), config.ViewType);

                            if (data.IsSixteenBit)
                            {
                                if ((cellcount + 1) < result.Length)
                                {
                                    // Store as big-endian (high byte first)
                                    result[cellcount] = (byte)((value >> 8) & 0xFF);
                                    result[cellcount + 1] = (byte)(value & 0xFF);
                                    cellcount += 2;
                                }
                                else
                                {
                                    result[cellcount] = (byte)(value & 0xFF);
                                    cellcount++;
                                }
                            }
                            else
                            {
                                result[cellcount] = (byte)(value & 0xFF);
                                cellcount++;
                            }
                        }
                    }
                }
            }

            return result;
        }
        
        public string FormatValue(int value, ViewType viewType, bool isSixteenBit)
        {
            switch (viewType)
            {
                case ViewType.Hexadecimal:
                    if (isSixteenBit)
                        return value.ToString("X4");
                    else
                        return value.ToString("X2");
                        
                case ViewType.Decimal:
                    return value.ToString(CultureInfo.InvariantCulture);
                    
                case ViewType.Easy:
                    return value.ToString(CultureInfo.InvariantCulture);
                    
                case ViewType.ASCII:
                    return ((char)value).ToString();
                    
                default:
                    return value.ToString(CultureInfo.InvariantCulture);
            }
        }
        
        public int ParseValue(string value, ViewType viewType)
        {
            try
            {
                switch (viewType)
                {
                    case ViewType.Hexadecimal:
                        return Convert.ToInt32(value, 16);
                        
                    case ViewType.Decimal:
                        return int.Parse(value, CultureInfo.InvariantCulture);
                        
                    case ViewType.Easy:
                        // Easy view values in the DataTable are raw integers (as strings)
                        // The scaling is applied only during rendering in CustomDrawCell.
                        double val = double.Parse(value, CultureInfo.InvariantCulture);
                        return (int)Math.Round(val);
                        
                    case ViewType.ASCII:
                        if (!string.IsNullOrEmpty(value))
                            return (int)value[0];
                        return 0;
                        
                    default:
                        return int.Parse(value, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return 0;
            }
        }
        
        public double ApplyCorrection(int rawValue, double factor, double offset)
        {
            if (factor == 1.0 && offset == 0.0)
                return rawValue;
                
            double result = rawValue * factor + offset;
            return result;
        }

        public void CalculateStatistics(DataTable dt, ViewType viewType, double factor, double offset, bool isCompare, out int maxVal, out double realMin, out double realMax)
        {
            maxVal = 0;
            realMin = double.MaxValue;
            realMax = double.MinValue;

            if (dt == null) return;

            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    if (item == null || item == DBNull.Value) continue;

                    int val = ParseValue(item.ToString(), viewType);
                    if (val > maxVal) maxVal = val;

                    double realVal = ApplyCorrection(val, factor, isCompare ? 0 : offset);
                    if (realVal > realMax) realMax = realVal;
                    if (realVal < realMin) realMin = realVal;
                }
            }

            if (realMin == double.MaxValue) realMin = 0;
            if (realMax == double.MinValue) realMax = 0;
        }
    }
}
