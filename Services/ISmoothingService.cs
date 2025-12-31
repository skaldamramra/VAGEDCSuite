using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Provides smoothing algorithms for map data
    /// </summary>
    public interface ISmoothingService
    {
        /// <summary>
        /// Applies linear smoothing to selection
        /// </summary>
        void SmoothLinear(object[] cells, object gridView);
        
        /// <summary>
        /// Applies proportional smoothing based on axis values
        /// </summary>
        void SmoothProportional(object[] cells, object gridView, int[] xAxis, int[] yAxis);
    }
}
