using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Core service for map data operations
    /// </summary>
    public interface IMapDataService
    {
        /// <summary>
        /// Loads map data from byte array
        /// </summary>
        MapViewerState LoadMapData(byte[] content, MapMetadata metadata, AxisData axes);
        
        /// <summary>
        /// Saves current map data
        /// </summary>
        byte[] SaveMapData(MapViewerState state);
        
        /// <summary>
        /// Validates map data integrity
        /// </summary>
        bool ValidateMapData(MapData data);
    }
}
