namespace VAGSuite.Models
{
    /// <summary>
    /// Represents axis values and their transformations
    /// </summary>
    public class AxisData
    {
        public int[] XAxisValues { get; set; }
        public int[] YAxisValues { get; set; }
        
        public int XAxisAddress { get; set; }
        public int YAxisAddress { get; set; }
        
        public double XCorrectionFactor { get; set; } = 1.0;
        public double XCorrectionOffset { get; set; } = 0.0;
        
        public double YCorrectionFactor { get; set; } = 1.0;
        public double YCorrectionOffset { get; set; } = 0.0;
        
        public string ConvertXValue(string value)
        {
            if (XCorrectionFactor == 1.0) return value;
            
            try
            {
                var temp = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                temp *= (float)XCorrectionFactor;
                temp += (float)XCorrectionOffset;
                return temp.ToString("F2");
            }
            catch
            {
                return value;
            }
        }
        
        public string ConvertYValue(string value)
        {
            if (YCorrectionFactor == 1.0) return value;
            
            try
            {
                var temp = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                temp *= (float)YCorrectionFactor;
                temp += (float)YCorrectionOffset;
                return temp.ToString("F1");
            }
            catch
            {
                return value;
            }
        }
    }
}
