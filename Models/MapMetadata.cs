namespace VAGSuite.Models
{
    /// <summary>
    /// Metadata about the map (names, descriptions, categories)
    /// </summary>
    public class MapMetadata
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }
        
        public XDFCategories Category { get; set; }
        public XDFSubCategory SubCategory { get; set; }
        
        public string XAxisName { get; set; }
        public string YAxisName { get; set; }
        public string ZAxisName { get; set; }
        
        public string XAxisUnits { get; set; }
        public string YAxisUnits { get; set; }
    }
}
