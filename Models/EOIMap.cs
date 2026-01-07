using System;

namespace VAGSuite.Models
{
    /// <summary>
    /// Represents a 2D or 3D calibration map.
    /// Translated from edcmap.py:Map class.
    /// </summary>
    public class EOIMap
    {
        /// <summary>
        /// X-axis values (e.g., RPM).
        /// </summary>
        public double[] XAxis { get; set; }
        
        /// <summary>
        /// Y-axis values (e.g., IQ/fuel quantity).
        /// Null for 2D maps.
        /// </summary>
        public double[] YAxis { get; set; }
        
        /// <summary>
        /// Map values as 2D array [x, y].
        /// For 2D maps, second dimension is 1.
        /// </summary>
        public double[,] Values { get; set; }
        
        /// <summary>
        /// Original symbol this map was loaded from.
        /// </summary>
        public SymbolHelper SourceSymbol { get; set; }
        
        /// <summary>
        /// Gets whether this is a 2D map (single Y value).
        /// </summary>
        public bool Is2D
        {
            get { return YAxis == null || YAxis.Length <= 1; }
        }
        
        /// <summary>
        /// Gets whether this is a 3D map (multiple Y values).
        /// </summary>
        public bool Is3D
        {
            get { return !Is2D; }
        }
        
        /// <summary>
        /// Creates an EOIMap from raw data arrays.
        /// </summary>
        public static EOIMap Create(
            double[] xAxis,
            double[] yAxis,
            double[,] values,
            SymbolHelper sourceSymbol = null)
        {
            return new EOIMap
            {
                XAxis = xAxis,
                YAxis = yAxis,
                Values = values,
                SourceSymbol = sourceSymbol
            };
        }
        
        /// <summary>
        /// Gets the value at the specified grid indices.
        /// </summary>
        public double GetValue(int xIndex, int yIndex)
        {
            if (Values == null) return 0;
            if (xIndex < 0 || xIndex >= Values.GetLength(0)) return 0;
            if (yIndex < 0 || yIndex >= Values.GetLength(1)) return 0;
            return Values[xIndex, yIndex];
        }
        
        /// <summary>
        /// Sets the value at the specified grid indices.
        /// </summary>
        public void SetValue(int xIndex, int yIndex, double value)
        {
            if (Values == null) return;
            if (xIndex >= 0 && xIndex < Values.GetLength(0) && 
                yIndex >= 0 && yIndex < Values.GetLength(1))
            {
                Values[xIndex, yIndex] = value;
            }
        }
        
        /// <summary>
        /// Gets the number of X-axis values.
        /// </summary>
        public int XCount
        {
            get { return XAxis != null ? XAxis.Length : 0; }
        }
        
        /// <summary>
        /// Gets the number of Y-axis values.
        /// </summary>
        public int YCount
        {
            get { return YAxis != null ? YAxis.Length : (Values != null ? Values.GetLength(1) : 0); }
        }
    }
}
