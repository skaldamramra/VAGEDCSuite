namespace VAGSuite.Models
{
    /// <summary>
    /// Configuration for how the map is displayed
    /// </summary>
    public class ViewConfiguration
    {
        public ViewType ViewType { get; set; } = ViewType.Hexadecimal;
        public ViewSize ViewSize { get; set; } = ViewSize.NormalView;
        
        public bool IsUpsideDown { get; set; }
        public bool DisableColors { get; set; }
        public bool IsRedWhite { get; set; }
        
        public double CorrectionFactor { get; set; } = 1.0;
        public double CorrectionOffset { get; set; } = 0.0;
        
        public int LockMode { get; set; }
        public double MaxYAxisValue { get; set; }
        
        public bool TableVisible { get; set; }
        public bool GraphVisible { get; set; } = true;
        
        public int SelectedTabPageIndex { get; set; }
        public int SliderPosition { get; set; }
    }
}
