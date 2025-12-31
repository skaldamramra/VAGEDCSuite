using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Core service for map data operations
    /// </summary>
    public class MapDataService : IMapDataService
    {
        private readonly IDataConversionService _conversionService;
        
        public MapDataService(IDataConversionService conversionService)
        {
            _conversionService = conversionService;
        }
        
        public MapViewerState LoadMapData(byte[] content, MapMetadata metadata, AxisData axes)
        {
            var state = new MapViewerState
            {
                Data = new MapData
                {
                    Content = content,
                    Length = content?.Length ?? 0,
                    IsSixteenBit = false,
                    TableWidth = 1
                },
                Metadata = metadata,
                Axes = axes,
                Configuration = new ViewConfiguration
                {
                    ViewType = ViewType.Hexadecimal,
                    ViewSize = ViewSize.NormalView,
                    GraphVisible = true,
                    TableVisible = true
                }
            };
            
            if (state.Data.Length > 0)
            {
                state.MaxValueInTable = CalculateMaxValue(state.Data.Content);
            }
            
            return state;
        }
        
        public byte[] SaveMapData(MapViewerState state)
        {
            if (state?.Data == null)
                return new byte[0];
                
            return state.Data.Content;
        }
        
        public bool ValidateMapData(MapData data)
        {
            if (data == null)
                return false;
                
            if (data.Content == null)
                return false;
                
            if (data.Length != data.Content.Length)
                return false;
                
            return true;
        }
        
        private int CalculateMaxValue(byte[] content)
        {
            if (content == null || content.Length == 0)
                return 0;
                
            int maxVal = 0;
            
            for (int i = 0; i < content.Length - 1; i += 2)
            {
                if (i + 1 < content.Length)
                {
                    int val = content[i] * 256 + content[i + 1];
                    if (val > maxVal)
                        maxVal = val;
                }
            }
            
            return maxVal;
        }
    }
}
