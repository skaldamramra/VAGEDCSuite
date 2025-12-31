namespace VAGSuite.Models
{
    /// <summary>
    /// Complete state of the map viewer
    /// </summary>
    public class MapViewerState
    {
        public MapData Data { get; set; }
        public MapMetadata Metadata { get; set; }
        public AxisData Axes { get; set; }
        public ViewConfiguration Configuration { get; set; }
        
        public bool IsDirty { get; set; }
        public bool IsCompareMode { get; set; }
        public bool IsDifferenceMode { get; set; }
        public bool IsRAMViewer { get; set; }
        public bool IsOnlineMode { get; set; }
        
        public int MaxValueInTable { get; set; }
        public double RealMaxValue { get; set; } = -65535;
        public double RealMinValue { get; set; } = 65535;
        
        public int[] AfrCounter { get; set; }
        public byte[] OpenLoop { get; set; }
    }
}
