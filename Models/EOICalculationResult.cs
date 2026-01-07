using System;
using System.Text;

namespace VAGSuite.Models
{
    /// <summary>
    /// Result of EOI calculation including metadata.
    /// </summary>
    public class EOICalculationResult
    {
        /// <summary>
        /// Calculated End of Injection map.
        /// </summary>
        public EOIMap EOIMap { get; set; }
        
        /// <summary>
        /// Calculated actual duration map (intermediate result).
        /// </summary>
        public EOIMap ActualDurationMap { get; set; }
        
        /// <summary>
        /// Original SOI map used for calculation.
        /// </summary>
        public EOIMap SOIMap { get; set; }
        
        /// <summary>
        /// Duration maps used for calculation.
        /// </summary>
        public DurationMapSet DurationMaps { get; set; }
        
        /// <summary>
        /// Time taken to perform calculation.
        /// </summary>
        public TimeSpan CalculationTime { get; set; }
        
        /// <summary>
        /// Whether single duration mode was used (no selector).
        /// </summary>
        public bool UsedSingleDuration { get; set; }
        
        /// <summary>
        /// Map of which duration maps were used for each cell.
        /// Format: "0" for single map, "0,1" for interpolated.
        /// </summary>
        public string[,] DurationMapUsage { get; set; }
        
        /// <summary>
        /// Gets a summary string for display.
        /// </summary>
        public string GetSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine("EOI Calculation Summary");
            sb.AppendLine("=======================");
            sb.AppendLine(string.Format("Calculation Time: {0:F2} ms", 
                CalculationTime.TotalMilliseconds));
            sb.AppendLine(string.Format("Mode: {0}", 
                UsedSingleDuration ? "Single Duration" : "Multiple Duration with Selector"));
            sb.AppendLine(string.Format("Duration Maps: {0}", 
                DurationMaps != null ? DurationMaps.Count.ToString() : "0"));
            
            if (SOIMap != null)
            {
                sb.AppendLine(string.Format("SOI Map Size: {0} x {1}", 
                    SOIMap.XCount, SOIMap.YCount));
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// Gets the duration map indices used at the specified cell.
        /// </summary>
        public string GetDurationMapUsageAt(int xIndex, int yIndex)
        {
            if (DurationMapUsage == null) return "";
            if (xIndex < 0 || xIndex >= DurationMapUsage.GetLength(0)) return "";
            if (yIndex < 0 || yIndex >= DurationMapUsage.GetLength(1)) return "";
            return DurationMapUsage[xIndex, yIndex];
        }
    }
}
