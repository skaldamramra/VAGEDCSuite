using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles clipboard operations for map data
    /// </summary>
    public interface IClipboardService
    {
        /// <summary>
        /// Copies selected cells to clipboard
        /// </summary>
        void CopySelection(object[] cells, ViewType viewType);
        
        /// <summary>
        /// Pastes clipboard data at current location
        /// </summary>
        void PasteAtCurrentLocation(object[] targetCells, ViewType currentViewType);
    }
}
