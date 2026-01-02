using System.Drawing;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles rendering of map data in grid view
    /// </summary>
    public class MapRenderingService : IMapRenderingService
    {
        public Color CalculateCellColor(int value, int maxValue, bool isOnlineMode, bool isRedWhite)
        {
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
        
        public string FormatCellDisplayText(int value, ViewConfiguration config, MapMetadata metadata)
        {
            string cellValueString = value.ToString();
            
            if (metadata != null && !string.IsNullOrEmpty(metadata.ZAxisName))
            {
                if (metadata.ZAxisName == "Seconds")
                {
                    double seconds = (double)value / 100;
                    cellValueString = seconds.ToString("F2");
                }
                else if (metadata.ZAxisName == "Degrees")
                {
                    double degrees = (double)value - 128;
                    cellValueString = degrees.ToString();
                }
            }
            
            return cellValueString;
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
