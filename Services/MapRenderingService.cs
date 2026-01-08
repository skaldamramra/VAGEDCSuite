using System;
using System.Drawing;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles rendering of map data in grid view
    /// </summary>
    public class MapRenderingService : IMapRenderingService
    {
        /// <summary>
        /// Calculates the color for a cell based on its value and configuration.
        /// When deltaMode is active, colors are based on the difference from original values.
        /// </summary>
        /// <param name="value">Current cell value</param>
        /// <param name="maxValue">Maximum value in table</param>
        /// <param name="isOnlineMode">Whether online mode is active</param>
        /// <param name="isRedWhite">Whether red-white color scheme is active</param>
        /// <param name="originalValue">Original value (for delta mode)</param>
        /// <param name="deltaMode">Whether delta mode is active</param>
        /// <returns>Color for the cell background</returns>
        public Color CalculateCellColor(int value, int maxValue, bool isOnlineMode, bool isRedWhite, int originalValue = 0, bool deltaMode = false)
        {
            // If delta mode is active, use delta coloring
            if (deltaMode && originalValue != 0)
            {
                return CalculateDeltaColor(value, originalValue);
            }
            
            if (maxValue == 0)
                return Color.White;
                
            int normalized = (value * 255) / maxValue;
            
            if (isOnlineMode)
            {
                return CalculateOnlineModeColor(normalized);
            }
            else if (isRedWhite)
            {
                return CalculateRedWhiteColor(normalized);
            }
            else
            {
                return CalculateStandardColor(normalized);
            }
        }
        
        /// <summary>
        /// Calculates color based on difference from original value.
        /// Green tint for positive changes (increased values), Red tint for negative changes (decreased values).
        /// </summary>
        private Color CalculateDeltaColor(int currentValue, int originalValue)
        {
            int diff = currentValue - originalValue;
            
            if (diff == 0)
            {
                // No change - use neutral gray/white
                return Color.FromArgb(240, 240, 240);
            }
            else if (diff > 0)
            {
                // Positive change (increase) - Green tint
                return Color.FromArgb(200, 255, 200);
            }
            else
            {
                // Negative change (decrease) - Red tint
                return Color.FromArgb(255, 200, 200);
            }
        }
        
        private Color CalculateOnlineModeColor(int normalized)
        {
            int red = normalized / 2;
            int green = 255 - red;
            int blue = 255 - red;
            
            if (red < 0) red = 0;
            if (red > 255) red = 255;
            if (normalized > 255) normalized = 255;
            if (green < 0) green = 0;
            if (green > 255) green = 255;
            if (blue < 0) blue = 0;
            if (blue > 255) blue = 255;
            
            return Color.FromArgb(red, green, blue);
        }
        
        private Color CalculateRedWhiteColor(int normalized)
        {
            int red = normalized;
            int green = 255 - normalized;
            int blue = 255 - normalized;
            
            if (red < 0) red = 0;
            if (red > 255) red = 255;
            if (normalized > 255) normalized = 255;
            if (green < 0) green = 0;
            if (green > 255) green = 255;
            if (blue < 0) blue = 0;
            if (blue > 255) blue = 255;
            
            return Color.FromArgb(red, green, blue);
        }
        
        private Color CalculateStandardColor(int normalized)
        {
            int red = normalized;
            int green = 255 - normalized;
            int blue = 0;
            
            if (red < 0) red = 0;
            if (red > 255) red = 255;
            if (normalized > 255) normalized = 255;
            if (green < 0) green = 0;
            if (green > 255) green = 255;
            
            return Color.FromArgb(red, green, blue);
        }
        
        /// <summary>
        /// Formats cell display text. When delta mode is active, shows the signed difference.
        /// </summary>
        /// <param name="value">Current cell value</param>
        /// <param name="config">View configuration</param>
        /// <param name="metadata">Map metadata</param>
        /// <param name="isSixteenBit">Whether values are 16-bit</param>
        /// <param name="originalValue">Original value (for delta mode)</param>
        /// <returns>Formatted display text</returns>
        public string FormatCellDisplayText(int value, ViewConfiguration config, MapMetadata metadata, bool isSixteenBit, int originalValue = 0)
        {
            // If delta mode is active, show the difference
            if (config.IsDeltaMode && originalValue != 0)
            {
                return FormatDeltaDisplayText(value, originalValue, config);
            }
            
            // Original logic for non-delta mode
            if (config.ViewType != ViewType.Easy)
            {
                // Use standard formatting for non-Easy views
                if (config.ViewType == ViewType.Hexadecimal)
                {
                    return isSixteenBit ? value.ToString("X4") : value.ToString("X2");
                }
                return value.ToString();
            }

            // Easy View Logic with Correction Factor/Offset
            double correctedValue = value * config.CorrectionFactor + config.CorrectionOffset;
            string mapName = metadata?.Name ?? string.Empty;

            if (mapName.StartsWith("Injector duration") || mapName.StartsWith("Start of injection"))
            {
                return correctedValue.ToString("F1") + "\u00b0";
            }
            else if (mapName.StartsWith("N75"))
            {
                return correctedValue.ToString("F0") + @"%";
            }
            else if (config.CorrectionFactor != 1.0 || config.CorrectionOffset != 0.0)
            {
                return correctedValue.ToString("F2");
            }

            return value.ToString();
        }
        
        /// <summary>
        /// Formats the delta (difference) as a signed string.
        /// Shows +X for positive changes, -X for negative changes.
        /// </summary>
        private string FormatDeltaDisplayText(int currentValue, int originalValue, ViewConfiguration config)
        {
            int diff = currentValue - originalValue;
            
            if (diff == 0)
            {
                return "0";
            }
            
            // Apply correction factor/offset for Easy view delta
            if (config.ViewType == ViewType.Easy && (config.CorrectionFactor != 1.0 || config.CorrectionOffset != 0.0))
            {
                double diffDouble = diff * config.CorrectionFactor;
                string sign = diffDouble > 0 ? "+" : "";
                return sign + diffDouble.ToString("F2");
            }
            
            // For hex view, show signed difference in hex
            if (config.ViewType == ViewType.Hexadecimal)
            {
                string sign = diff > 0 ? "+" : "";
                return sign + Math.Abs(diff).ToString("X");
            }
            
            // For decimal and other views
            string deltaSign = diff > 0 ? "+" : "";
            return deltaSign + diff.ToString();
        }
        
        public bool ShouldShowOpenLoopIndicator(int rowIndex, int colIndex, byte[] openLoop, int[] xAxis, int[] yAxis, string xAxisName, string yAxisName)
        {
            if (openLoop == null || openLoop.Length == 0) return false;
            if (xAxis == null || yAxis == null) return false;
            if (xAxisName.ToLower() != "mg/c" || yAxisName.ToLower() != "rpm") return false;

            try
            {
                if (colIndex >= 0 && colIndex < xAxis.Length && rowIndex >= 0 && rowIndex < yAxis.Length)
                {
                    int airmassvalue = xAxis[colIndex];
                    
                    // Logic from MapViewerEx.GetOpenLoopValue
                    int index = (yAxis.Length - 1) - rowIndex;
                    if (index * 2 + 1 < openLoop.Length)
                    {
                        int mapopenloop = (int)openLoop[index * 2] * 256 + (int)openLoop[index * 2 + 1];
                        return mapopenloop > airmassvalue;
                    }
                }
            }
            catch
            {
                // Fallback to false on any error
            }

            return false;
        }
    }
}
