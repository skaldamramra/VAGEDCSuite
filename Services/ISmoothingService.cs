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

        /// <summary>
        /// Applies interpolated smoothing between 4 corners
        /// </summary>
        void SmoothInterpolated(object[] cells, object gridView, int[] xAxis, int[] yAxis);

        /// <summary>
        /// Applies linear smoothing with neighbor awareness for smoother transitions
        /// </summary>
        void SmoothLinearWithNeighbors(object[] cells, object gridView, int[] xAxis, int[] yAxis, bool isUpsideDown = false);

        /// <summary>
        /// Applies proportional smoothing with actual corner detection and neighbor blending
        /// </summary>
        void SmoothProportionalWithNeighbors(object[] cells, object gridView, int[] xAxis, int[] yAxis, bool isUpsideDown = false);

        /// <summary>
        /// Applies true bilinear interpolation for 2D blocks using axis values
        /// </summary>
        void SmoothBilinear(object[] cells, object gridView, int[] xAxis, int[] yAxis, bool isUpsideDown = false);
    }
}
