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
        Color CalculateCellColor(int value, int maxValue, bool isOnlineMode, bool isRedWhite);
        
        /// <summary>
        /// Formats cell display text with units
        /// </summary>
        string FormatCellDisplayText(int value, ViewConfiguration config, MapMetadata metadata, bool isSixteenBit);
        
        /// <summary>
        /// Determines if cell should show open-loop indicator
        /// </summary>
        bool ShouldShowOpenLoopIndicator(int rowIndex, int colIndex, byte[] openLoop, int[] xAxis, int[] yAxis, string xAxisName, string yAxisName);
    }
}
