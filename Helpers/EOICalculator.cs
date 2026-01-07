using System;
using System.Collections.Generic;
using System.Linq;
using VAGSuite.Models;

namespace VAGSuite.Helpers
{
    /// <summary>
    /// Core EOI calculation algorithms.
    /// Translated from utils.py
    /// </summary>
    public class EOICalculator
    {
        private readonly BilinearInterpolator _interpolator;
        
        public EOICalculator()
        {
            _interpolator = new BilinearInterpolator();
        }
        
        /// <summary>
        /// Calculates End of Injection map.
        /// Translated from utils.py:get_eoi()
        /// </summary>
        public EOICalculationResult CalculateEOI(
            EOIMap soiMap,
            DurationMapSet durationMaps,
            EOIMap selectorMap)
        {
            var startTime = DateTime.Now;
            
            // Dump map information for debugging
            DumpMapInfo(soiMap, durationMaps, selectorMap);

            // Calculate actual duration for each cell
            string[,] durationUsage;
            var actualDuration = CalculateActualDuration(
                soiMap, durationMaps, selectorMap, out durationUsage);
            
            // Calculate EOI: EOI = SOI - Actual Duration
            var eoiMap = new EOIMap
            {
                XAxis = soiMap.XAxis,
                YAxis = soiMap.YAxis,
                Values = new double[soiMap.Values.GetLength(0), 
                                   soiMap.Values.GetLength(1)]
            };
            
            for (int i = 0; i < soiMap.Values.GetLength(0); i++)
            {
                for (int j = 0; j < soiMap.Values.GetLength(1); j++)
                {
                    eoiMap.Values[i, j] = soiMap.Values[i, j] 
                                        - actualDuration.Values[i, j];
                }
            }
            
            var calculationTime = DateTime.Now - startTime;
            
            return new EOICalculationResult
            {
                EOIMap = eoiMap,
                ActualDurationMap = actualDuration,
                SOIMap = soiMap,
                DurationMaps = durationMaps,
                CalculationTime = calculationTime,
                UsedSingleDuration = (selectorMap == null),
                DurationMapUsage = durationUsage
            };
        }
        
        /// <summary>
        /// Calculates actual duration for each cell in SOI map.
        /// Translated from utils.py:get_actual_duration()
        /// </summary>
        private EOIMap CalculateActualDuration(
            EOIMap soiMap,
            DurationMapSet durationMaps,
            EOIMap selectorMap,
            out string[,] durationUsage)
        {
            int xCount = soiMap.Values.GetLength(0);
            int yCount = soiMap.Values.GetLength(1);
            
            var actualDuration = new EOIMap
            {
                XAxis = soiMap.XAxis,
                YAxis = soiMap.YAxis,
                Values = new double[xCount, yCount]
            };
            
            durationUsage = new string[xCount, yCount];
            
            // If no selector, use first duration map only
            if (selectorMap == null)
            {
                if (durationMaps.Count == 0)
                {
                    // If no duration maps, set all values to 0
                    for (int i = 0; i < xCount; i++)
                    {
                        for (int j = 0; j < yCount; j++)
                        {
                            actualDuration.Values[i, j] = 0;
                            durationUsage[i, j] = "No duration maps found";
                        }
                    }
                }
                else
                {
                    var map = durationMaps.Maps[0];
                    string mapName = map.SourceSymbol?.Varname ?? "Duration Map 0";
                    
                    for (int i = 0; i < xCount; i++)
                    {
                        for (int j = 0; j < yCount; j++)
                        {
                            double rpm = soiMap.XAxis[i];
                            double iq = soiMap.YAxis[j];
                            
                            double val = _interpolator.Interpolate(map, rpm, iq);
                            actualDuration.Values[i, j] = val;
                            durationUsage[i, j] = $"{mapName}: {val:F2}° (Weight: 1.0)";
                        }
                    }
                }
            }
            else
            {
                // Use selector to choose duration maps
                int xLimit = Math.Min(xCount, soiMap.XAxis.Length);
                int yLimit = Math.Min(yCount, soiMap.YAxis.Length);

                for (int i = 0; i < xLimit; i++)
                {
                    for (int j = 0; j < yLimit; j++)
                    {
                        double rpm = soiMap.XAxis[i];
                        double iq = soiMap.YAxis[j];
                        double currentSOI = soiMap.Values[i, j];
                        
                        // Select which duration maps to use based on fractional map index
                        double fractionalMapIndex = GetFractionalMapIndex(currentSOI, selectorMap);
                        
                        int map1Index = (int)Math.Floor(fractionalMapIndex);
                        int map2Index = (int)Math.Ceiling(fractionalMapIndex);
                        
                        // Ensure indices are within bounds of available duration maps
                        map1Index = Math.Max(0, Math.Min(map1Index, durationMaps.Count - 1));
                        map2Index = Math.Max(0, Math.Min(map2Index, durationMaps.Count - 1));

                        // Interpolate from selected maps
                        double duration1 = _interpolator.Interpolate(durationMaps.Maps[map1Index], rpm, iq);
                        double duration2 = _interpolator.Interpolate(durationMaps.Maps[map2Index], rpm, iq);
                        
                        // Calculate interpolation weight based on the fractional part of the map index
                        // Example: Index 2.3 -> Map 2 (70%) and Map 3 (30%)
                        // weight = 1.0 - 0.3 = 0.7
                        double weight = 1.0 - (fractionalMapIndex - Math.Floor(fractionalMapIndex));
                        if (map1Index == map2Index) weight = 1.0;

                        double finalDuration = (duration1 * weight) + (duration2 * (1.0 - weight));
                        actualDuration.Values[i, j] = finalDuration;
                        
                        string map1Name = durationMaps.Maps[map1Index].SourceSymbol?.Varname ?? $"Map {map1Index}";
                        string map2Name = durationMaps.Maps[map2Index].SourceSymbol?.Varname ?? $"Map {map2Index}";
                        
                        if (map1Index == map2Index)
                        {
                            durationUsage[i, j] = $"{map1Name}: {duration1:F2}° (Weight: 1.0)";
                        }
                        else
                        {
                            durationUsage[i, j] = $"{map1Name}: {duration1:F2}° * {weight:F2} + {map2Name}: {duration2:F2}° * {(1.0 - weight):F2}";
                        }
                    }
                }
            }
            
            return actualDuration;
        }
        
        private void DumpMapInfo(EOIMap soiMap, DurationMapSet durationMaps, EOIMap selectorMap)
        {
            try
            {
                Console.WriteLine("=== EOI Calculation Debug Dump ===");
                Console.WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                
                // SOI Map Info
                if (soiMap != null)
                {
                    Console.WriteLine($"[SOI Map] Name: {soiMap.SourceSymbol?.Varname}, Size: {soiMap.XCount}x{soiMap.YCount}");
                    Console.WriteLine($"  X-Axis (RPM): {string.Join(", ", soiMap.XAxis.Select(v => v.ToString("F0")))}");
                    Console.WriteLine($"  Y-Axis (IQ): {string.Join(", ", soiMap.YAxis.Select(v => v.ToString("F1")))}");
                    
                    // Show sample values
                    if (soiMap.Values.GetLength(0) > 0 && soiMap.Values.GetLength(1) > 0)
                    {
                        Console.WriteLine($"  Sample SOI at [0,0]: {soiMap.Values[0, 0]:F2}°");
                        Console.WriteLine($"  Sample SOI at [1,1]: {soiMap.Values[1, 1]:F2}°");
                    }
                }

                // Selector Map Info
                if (selectorMap != null)
                {
                    Console.WriteLine($"[Selector Map] Name: {selectorMap.SourceSymbol?.Varname}, Size: {selectorMap.XCount}x{selectorMap.YCount}");
                    Console.WriteLine($"  X-Axis (SOI Thresholds): {string.Join(", ", selectorMap.XAxis.Select(v => v.ToString("F2")))}");
                    
                    // Show raw and processed indices
                    var rawValues = new List<double>();
                    var processedIndices = new List<int>();
                    for (int i = 0; i < selectorMap.XCount; i++)
                    {
                        double rawVal = selectorMap.Values[i, 0];
                        rawValues.Add(rawVal);
                        int index = rawVal > 255 ? (int)Math.Round(rawVal / 256.0) : (int)Math.Round(rawVal);
                        processedIndices.Add(index);
                    }
                    Console.WriteLine($"  Raw Values: {string.Join(", ", rawValues.Select(v => v.ToString("F0")))}");
                    Console.WriteLine($"  Processed Indices: {string.Join(", ", processedIndices)}");
                }
                else
                {
                    Console.WriteLine("[Selector Map] None (Single duration mode)");
                }

                // Duration Maps Info
                Console.WriteLine($"[Duration Maps] Count: {durationMaps.Count}");
                for (int i = 0; i < durationMaps.Count; i++)
                {
                    var dMap = durationMaps.Maps[i];
                    Console.WriteLine($"  Map {i}: {dMap.SourceSymbol?.Varname}, Size: {dMap.XCount}x{dMap.YCount}");
                    Console.WriteLine($"    X-Axis Range: {dMap.XAxis.Min():F0} - {dMap.XAxis.Max():F0}");
                    Console.WriteLine($"    Y-Axis Range: {dMap.YAxis.Min():F1} - {dMap.YAxis.Max():F1}");
                    
                    // Show sample values
                    if (dMap.Values.GetLength(0) > 0 && dMap.Values.GetLength(1) > 0)
                    {
                        Console.WriteLine($"    Sample at [0,0]: {dMap.Values[0, 0]:F2}°");
                        Console.WriteLine($"    Sample at [1,1]: {dMap.Values[1, 1]:F2}°");
                    }
                }
                Console.WriteLine("==================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during debug dump: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculates the fractional map index from the selector curve based on SOI.
        /// According to BOSCH EDC15 manual section 12.4.
        /// </summary>
        private double GetFractionalMapIndex(double currentSOI, EOIMap selectorMap)
        {
            // Selector values in EDC15 are typically stored as (index * 256) in the binary
            // if they are 16-bit, or just the index if 8-bit.
            double GetNormalizedValue(int index)
            {
                double val = selectorMap.Values[index, 0];
                return val > 255 ? val / 256.0 : val;
            }

            int len = selectorMap.XAxis.Length;
            if (len == 0) return 0;

            // Handle out of bounds (SOI is usually negative, e.g. -36 to +1)
            // Check if axis is ascending or descending
            bool isDescending = selectorMap.XAxis[0] > selectorMap.XAxis[len - 1];

            if (isDescending)
            {
                if (currentSOI >= selectorMap.XAxis[0]) return GetNormalizedValue(0);
                if (currentSOI <= selectorMap.XAxis[len - 1]) return GetNormalizedValue(len - 1);

                for (int i = 0; i < len - 1; i++)
                {
                    if (currentSOI <= selectorMap.XAxis[i] && currentSOI >= selectorMap.XAxis[i + 1])
                    {
                        double x1 = selectorMap.XAxis[i];
                        double x2 = selectorMap.XAxis[i + 1];
                        double y1 = GetNormalizedValue(i);
                        double y2 = GetNormalizedValue(i + 1);
                        
                        // Linear interpolation between thresholds to find fractional map index
                        return y1 + (currentSOI - x1) * (y2 - y1) / (x2 - x1);
                    }
                }
            }
            else
            {
                if (currentSOI <= selectorMap.XAxis[0]) return GetNormalizedValue(0);
                if (currentSOI >= selectorMap.XAxis[len - 1]) return GetNormalizedValue(len - 1);

                for (int i = 0; i < len - 1; i++)
                {
                    if (currentSOI >= selectorMap.XAxis[i] && currentSOI <= selectorMap.XAxis[i + 1])
                    {
                        double x1 = selectorMap.XAxis[i];
                        double x2 = selectorMap.XAxis[i + 1];
                        double y1 = GetNormalizedValue(i);
                        double y2 = GetNormalizedValue(i + 1);
                        
                        return y1 + (currentSOI - x1) * (y2 - y1) / (x2 - x1);
                    }
                }
            }

            return GetNormalizedValue(0);
        }
    }
}
