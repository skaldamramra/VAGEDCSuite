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
            
            if (data == null || data.Content == null)
                return dataTable;
                
            int xvalues = 0;
            int yvalues = 0;
            
            if (config.ViewType == ViewType.Easy)
            {
                xvalues = 1;
                yvalues = data.Length;
            }
            else
            {
                xvalues = data.TableWidth;
                if (xvalues > 0)
                    yvalues = data.Length / xvalues;
                else
                    yvalues = data.Length;
            }
            
            for (int i = 0; i < xvalues; i++)
            {
                var column = new DataColumn("col" + i.ToString());
                dataTable.Columns.Add(column);
            }
            
            int cellcount = 0;
            for (int t = 0; t < yvalues; t++)
            {
                DataRow row = dataTable.NewRow();
                for (int i = 0; i < xvalues; i++)
                {
                    if (cellcount < data.Content.Length)
                    {
                        int cellvalue = 0;
                        if (data.IsSixteenBit)
                        {
                            if ((cellcount + 1) < data.Content.Length)
                            {
                                cellvalue = data.Content[cellcount] * 256 + data.Content[cellcount + 1];
                            }
                            else
                            {
                                cellvalue = data.Content[cellcount];
                            }
                        }
                        else
                        {
                            cellvalue = data.Content[cellcount];
                        }
                        
                        string cellValueString = FormatValue(cellvalue, config.ViewType, data.IsSixteenBit);
                        row[i] = cellValueString;
                    }
                    cellcount++;
                }
                dataTable.Rows.Add(row);
            }
            
            return dataTable;
        }
        
        public byte[] ConvertFromDataTable(DataTable table, MapData data, ViewConfiguration config)
        {
            var result = new byte[data.Length];
            int cellcount = 0;
            
            foreach (DataRow row in table.Rows)
            {
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
                                    result[cellcount] = (byte)((value >> 8) & 0xFF);
                                    result[cellcount + 1] = (byte)(value & 0xFF);
                                }
                                else
                                {
                                    result[cellcount] = (byte)(value & 0xFF);
                                }
                            }
                            else
                            {
                                result[cellcount] = (byte)(value & 0xFF);
                            }
                        }
                    }
                    cellcount++;
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
                    double val = Convert.ToDouble(value);
                    val = val / 100;
                    return val.ToString(CultureInfo.InvariantCulture);
                    
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
                        double val = double.Parse(value, CultureInfo.InvariantCulture);
                        val = val * 100;
                        return (int)val;
                        
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
    }
}
