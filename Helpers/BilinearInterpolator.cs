using System;
using VAGSuite.Models;

namespace VAGSuite.Helpers
{
    /// <summary>
    /// 2D bilinear interpolation with extrapolation fallback.
    /// Translated from edcmap.py:RegularGridInterpolatorNNextrapol
    /// </summary>
    public class BilinearInterpolator
    {
        /// <summary>
        /// Performs bilinear interpolation on a 2D map.
        /// Falls back to nearest neighbor for out-of-bounds values.
        /// </summary>
        /// <param name="map">The map to interpolate from</param>
        /// <param name="x">X coordinate (e.g., RPM)</param>
        /// <param name="y">Y coordinate (e.g., IQ)</param>
        /// <returns>Interpolated value</returns>
        public double Interpolate(EOIMap map, double x, double y)
        {
            if (map == null || map.Values == null) return 0;

            // Handle 1D maps (where one axis has length 1)
            if (map.XAxis.Length <= 1 || map.YAxis.Length <= 1)
            {
                return NearestNeighbor(map, x, y);
            }
            
            // Find surrounding grid points
            int x1Index, x2Index, y1Index, y2Index;
            double x1, x2, y1, y2;
            
            if (!FindSurroundingPoints(map.XAxis, x, out x1Index, out x2Index, out x1, out x2))
            {
                // Out of bounds on X - use nearest neighbor
                return NearestNeighbor(map, x, y);
            }
            
            if (!FindSurroundingPoints(map.YAxis, y, out y1Index, out y2Index, out y1, out y2))
            {
                // Out of bounds on Y - use nearest neighbor
                return NearestNeighbor(map, x, y);
            }
            
            // Safety check for array bounds before access
            if (x1Index < 0 || x2Index >= map.Values.GetLength(0) ||
                y1Index < 0 || y2Index >= map.Values.GetLength(1))
            {
                return NearestNeighbor(map, x, y);
            }

            // Get values at four corners
            double Q11 = map.Values[x1Index, y1Index];
            double Q12 = map.Values[x1Index, y2Index];
            double Q21 = map.Values[x2Index, y1Index];
            double Q22 = map.Values[x2Index, y2Index];
            
            // Bilinear interpolation formula
            double xDiff = x2 - x1;
            double yDiff = y2 - y1;
            
            if (Math.Abs(xDiff) < 1e-10 || Math.Abs(yDiff) < 1e-10)
            {
                // Degenerate case - return nearest
                return Q11;
            }
            
            double R1 = ((x2 - x) / xDiff) * Q11 + ((x - x1) / xDiff) * Q21;
            double R2 = ((x2 - x) / xDiff) * Q12 + ((x - x1) / xDiff) * Q22;
            
            double result = ((y2 - y) / yDiff) * R1 + ((y - y1) / yDiff) * R2;
            
            return result;
        }
        
        /// <summary>
        /// Finds the two surrounding points in a sorted array.
        /// </summary>
        private bool FindSurroundingPoints(
            double[] axis, 
            double value,
            out int index1, 
            out int index2, 
            out double value1, 
            out double value2)
        {
            index1 = 0;
            index2 = 0;
            value1 = 0;
            value2 = 0;
            
            if (axis == null || axis.Length < 2) return false;
            
            // Check bounds (handle both ascending and descending axes)
            double min = Math.Min(axis[0], axis[axis.Length - 1]);
            double max = Math.Max(axis[0], axis[axis.Length - 1]);

            if (value < min || value > max)
            {
                return false;  // Out of bounds
            }
            
            // Find surrounding indices
            for (int i = 0; i < axis.Length - 1; i++)
            {
                double v1 = axis[i];
                double v2 = axis[i + 1];
                
                if ((value >= v1 && value <= v2) || (value >= v2 && value <= v1))
                {
                    index1 = i;
                    index2 = i + 1;
                    value1 = v1;
                    value2 = v2;
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Returns nearest neighbor value for out-of-bounds points.
        /// </summary>
        private double NearestNeighbor(EOIMap map, double x, double y)
        {
            if (map == null || map.Values == null) return 0;
            
            // Find nearest X index
            int xIndex = 0;
            if (map.XAxis != null && map.XAxis.Length > 0)
            {
                double minXDist = double.MaxValue;
                for (int i = 0; i < map.XAxis.Length; i++)
                {
                    double dist = Math.Abs(map.XAxis[i] - x);
                    if (dist < minXDist)
                    {
                        minXDist = dist;
                        xIndex = i;
                    }
                }
            }
            
            // Find nearest Y index
            int yIndex = 0;
            if (map.YAxis != null && map.YAxis.Length > 0)
            {
                double minYDist = double.MaxValue;
                for (int i = 0; i < map.YAxis.Length; i++)
                {
                    double dist = Math.Abs(map.YAxis[i] - y);
                    if (dist < minYDist)
                    {
                        minYDist = dist;
                        yIndex = i;
                    }
                }
            }
            
            // Final safety check for array bounds
            if (xIndex >= 0 && xIndex < map.Values.GetLength(0) &&
                yIndex >= 0 && yIndex < map.Values.GetLength(1))
            {
                return map.Values[xIndex, yIndex];
            }
            
            return 0;
        }
    }
}
