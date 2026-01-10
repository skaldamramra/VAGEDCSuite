using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VAGSuite
{
    /// <summary>
    /// Debug utility for comparing FileParser vs XML MapRules detection.
    /// Only active in Debug builds.
    /// </summary>
    public static class DebugMapComparison
    {
        private static List<SymbolHelper> _fileParserMaps = new List<SymbolHelper>();
        private static List<SymbolHelper> _xmlMapRulesMaps = new List<SymbolHelper>();
        private static bool _isInitialized = false;
        private static string _currentFileName = string.Empty;

        /// <summary>
        /// Initializes the comparison tracker for a new file.
        /// </summary>
        public static void Initialize(string filename)
        {
#if DEBUG
            _fileParserMaps.Clear();
            _xmlMapRulesMaps.Clear();
            _currentFileName = filename;
            _isInitialized = true;
#endif
        }

        /// <summary>
        /// Adds a map from the FileParser detection path.
        /// </summary>
        public static void AddFileParserMap(SymbolHelper map)
        {
#if DEBUG
            if (_isInitialized)
            {
                _fileParserMaps.Add(map.Clone());
            }
#endif
        }

        /// <summary>
        /// Adds a map from the XML MapRules detection path.
        /// </summary>
        public static void AddXmlMapRulesMap(SymbolHelper map)
        {
#if DEBUG
            if (_isInitialized)
            {
                _xmlMapRulesMaps.Add(map.Clone());
            }
#endif
        }

        /// <summary>
        /// Generates comparison CSV files in the project directory.
        /// Call this after NameKnownMaps() completes.
        /// </summary>
        public static void GenerateComparisonFiles()
        {
#if DEBUG
            if (!_isInitialized) return;

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Generate FileParser maps file
                string fpFilePath = Path.Combine(baseDir, "fileparser_maps_debug.csv");
                WriteMapsToCsv(fpFilePath, _fileParserMaps, "FileParser");

                // Generate XML MapRules maps file
                string xmlFilePath = Path.Combine(baseDir, "xml_maprules_debug.csv");
                WriteMapsToCsv(xmlFilePath, _xmlMapRulesMaps, "XML MapRules");

                // Optionally generate a comparison summary
                string summaryFilePath = Path.Combine(baseDir, "map_comparison_summary.txt");
                WriteSummaryFile(summaryFilePath);

                Console.WriteLine("Debug map comparison files generated:");
                Console.WriteLine("  - " + fpFilePath);
                Console.WriteLine("  - " + xmlFilePath);
                Console.WriteLine("  - " + summaryFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating debug comparison files: " + ex.Message);
            }
#endif
        }

        /// <summary>
        /// Resets the tracker. Call this when done or on error.
        /// </summary>
        public static void Reset()
        {
#if DEBUG
            _fileParserMaps.Clear();
            _xmlMapRulesMaps.Clear();
            _isInitialized = false;
            _currentFileName = string.Empty;
#endif
        }

        private static void WriteMapsToCsv(string filePath, List<SymbolHelper> maps, string source)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Header row
                writer.WriteLine("# Source: " + source);
                writer.WriteLine("# File: " + _currentFileName);
                writer.WriteLine("# Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine();
                
                // Column headers
                writer.WriteLine("MapName,Length,X_Axis_Length,Y_Axis_Length,X_Axis_ID_Hex,Y_Axis_ID_Hex,X_Axis_Address_Hex,Y_Axis_Address_Hex,Flash_Start_Address_Hex,Category,Subcategory,X_Axis_Description,Y_Axis_Description,Z_Axis_Description,X_Axis_Units,Y_Axis_Units,Correction,Offset,X_Axis_Correction,Y_Axis_Correction,X_Axis_Offset,Y_Axis_Offset,CodeBlock");

                // Data rows
                foreach (SymbolHelper map in maps)
                {
                    writer.WriteLine(EscapeCsvValue(map.Varname) + "," +
                        map.Length + "," +
                        map.X_axis_length + "," +
                        map.Y_axis_length + "," +
                        "0x" + map.X_axis_ID.ToString("X4") + "," +
                        "0x" + map.Y_axis_ID.ToString("X4") + "," +
                        "0x" + map.X_axis_address.ToString("X8") + "," +
                        "0x" + map.Y_axis_address.ToString("X8") + "," +
                        "0x" + map.Flash_start_address.ToString("X8") + "," +
                        EscapeCsvValue(map.Category) + "," +
                        EscapeCsvValue(map.Subcategory) + "," +
                        EscapeCsvValue(map.X_axis_descr) + "," +
                        EscapeCsvValue(map.Y_axis_descr) + "," +
                        EscapeCsvValue(map.Z_axis_descr) + "," +
                        EscapeCsvValue(map.XaxisUnits) + "," +
                        EscapeCsvValue(map.YaxisUnits) + "," +
                        map.Correction.ToString("G17") + "," +
                        map.Offset.ToString("G17") + "," +
                        map.X_axis_correction.ToString("G17") + "," +
                        map.Y_axis_correction.ToString("G17") + "," +
                        map.X_axis_offset.ToString("G17") + "," +
                        map.Y_axis_offset.ToString("G17") + "," +
                        map.CodeBlock);
                }
            }
        }

        private static void WriteSummaryFile(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("=== Map Detection Comparison Summary ===");
                writer.WriteLine();
                writer.WriteLine("File: " + _currentFileName);
                writer.WriteLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine();

                writer.WriteLine("Summary:");
                writer.WriteLine("  FileParser maps count: " + _fileParserMaps.Count);
                writer.WriteLine("  XML MapRules maps count: " + _xmlMapRulesMaps.Count);
                writer.WriteLine();

                // Find maps only in FileParser
                writer.WriteLine("=== Maps ONLY in FileParser (not in XML) ===");
                var fpOnly = FindUniqueMaps(_fileParserMaps, _xmlMapRulesMaps);
                foreach (var map in fpOnly)
                {
                    writer.WriteLine("  " + map.Varname + " @ 0x" + map.Flash_start_address.ToString("X8"));
                }
                if (fpOnly.Count == 0) writer.WriteLine("  (none)");
                writer.WriteLine();

                // Find maps only in XML
                writer.WriteLine("=== Maps ONLY in XML MapRules (not in FileParser) ===");
                var xmlOnly = FindUniqueMaps(_xmlMapRulesMaps, _fileParserMaps);
                foreach (var map in xmlOnly)
                {
                    writer.WriteLine("  " + map.Varname + " @ 0x" + map.Flash_start_address.ToString("X8"));
                }
                if (xmlOnly.Count == 0) writer.WriteLine("  (none)");
                writer.WriteLine();

                // Find common maps
                writer.WriteLine("=== Common Maps (in both) ===");
                var common = FindCommonMaps(_fileParserMaps, _xmlMapRulesMaps);
                foreach (var map in common)
                {
                    writer.WriteLine("  " + map.Varname + " @ 0x" + map.Flash_start_address.ToString("X8"));
                }
                if (common.Count == 0) writer.WriteLine("  (none)");
            }
        }

        private static List<SymbolHelper> FindUniqueMaps(List<SymbolHelper> source, List<SymbolHelper> compareTo)
        {
            var unique = new List<SymbolHelper>();
            foreach (var map in source)
            {
                bool found = false;
                foreach (var other in compareTo)
                {
                    if (AreMapsAtSameLocation(map, other))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) unique.Add(map);
            }
            return unique;
        }

        private static List<SymbolHelper> FindCommonMaps(List<SymbolHelper> source, List<SymbolHelper> compareTo)
        {
            var common = new List<SymbolHelper>();
            foreach (var map in source)
            {
                foreach (var other in compareTo)
                {
                    if (AreMapsAtSameLocation(map, other))
                    {
                        common.Add(map);
                        break;
                    }
                }
            }
            return common;
        }

        private static bool AreMapsAtSameLocation(SymbolHelper a, SymbolHelper b)
        {
            // Two maps are considered the same if they're at the same flash address
            return a.Flash_start_address == b.Flash_start_address;
        }

        private static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            
            // Escape quotes and commas
            string result = value.Replace("\"", "\"\"");
            if (result.Contains(",") || result.Contains("\"") || result.Contains("\n"))
            {
                result = "\"" + result + "\"";
            }
            return result;
        }
    }
}