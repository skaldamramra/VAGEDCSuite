namespace VAGSuite.Models
{
    /// <summary>
    /// Represents the raw map data and its variants
    /// </summary>
    public class MapData
    {
        public byte[] Content { get; set; }
        public byte[] OriginalContent { get; set; }
        public byte[] CompareContent { get; set; }
        
        public int Address { get; set; }
        public int SramAddress { get; set; }
        public int Length { get; set; }
        
        public bool IsSixteenBit { get; set; }
        public int TableWidth { get; set; }
        
        public MapData Clone()
        {
            return new MapData
            {
                Content = (byte[])Content?.Clone(),
                OriginalContent = (byte[])OriginalContent?.Clone(),
                CompareContent = (byte[])CompareContent?.Clone(),
                Address = Address,
                SramAddress = SramAddress,
                Length = Length,
                IsSixteenBit = IsSixteenBit,
                TableWidth = TableWidth
            };
        }
    }
}
