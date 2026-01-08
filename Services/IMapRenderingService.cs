using System.Drawing;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles rendering of map data in grid view
    /// </summary>
    public interface IMapRenderingService
    {
        /// <summary>
        /// Calculates cell background color based on value
        /// </summary>
        /// <param name="value">Current cell value</param>
        /// <param name="maxValue">Maximum value in table</param>
        /// <param name="isOnlineMode">Whether online mode is active</param>
        /// <param name="isRedWhite">Whether red-white color scheme is active</param>
        /// <param name="originalValue">Original value (for delta mode)</param>
        /// <param name="deltaMode">Whether delta mode is active</param>
        /// <returns>Color for the cell background</returns>
        Color CalculateCellColor(int value, int maxValue, bool isOnlineMode, bool isRedWhite, int originalValue = 0, bool deltaMode = false);
        
        /// <summary>
        /// Formats cell display text with units
        /// </summary>
        /// <param name="value">Current cell value</param>
        /// <param name="config">View configuration</param>
        /// <param name="metadata">Map metadata</param>
        /// <param name="isSixteenBit">Whether values are 16-bit</param>
        /// <param name="originalValue">Original value (for delta mode)</param>
        /// <returns>Formatted display text</returns>
        string FormatCellDisplayText(int value, ViewConfiguration config, MapMetadata metadata, bool isSixteenBit, int originalValue = 0);
        
        /// <summary>
        /// Determines if cell should show open-loop indicator
        /// </summary>
        bool ShouldShowOpenLoopIndicator(int rowIndex, int colIndex, byte[] openLoop, int[] xAxis, int[] yAxis, string xAxisName, string yAxisName);
    }
}
