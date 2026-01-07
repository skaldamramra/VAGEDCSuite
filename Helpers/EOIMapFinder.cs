using System;
using System.Collections.Generic;
using VAGSuite.Models;

namespace VAGSuite.Helpers
{
    /// <summary>
    /// Discovers SOI, duration, and selector maps in symbol collection.
    /// Translated from mapfinder.py
    /// </summary>
    public class EOIMapFinder
    {
        /// <summary>
        /// Finds all SOI maps in the symbol collection.
        /// Translated from mapfinder.py:get_soi()
        /// </summary>
        public List<SymbolHelper> FindSOIMaps(SymbolCollection symbols)
        {
            if (symbols == null) return new List<SymbolHelper>();
            
            var soiSymbols = new List<SymbolHelper>();
            
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && (s.Varname.Contains("Start of") || s.Varname.Contains("SOI")))
                {
                    // Support various SOI map dimensions across different ECUs (EDC15, EDC16, MSA)
                    // EDC15P: 14x16 or 16x14
                    // EDC15V/MSA: often 13x16 or 14x10
                    // EDC16: can vary
                    if ((s.X_axis_length >= 10 && s.Y_axis_length >= 10) || s.Varname.Contains("(SOI)") || s.Varname.Contains("(N108 SOI)"))
                    {
                        soiSymbols.Add(s);
                    }
                }
            }
            
            // Sort by Flash_start_address
            soiSymbols.Sort((a, b) => a.Flash_start_address.CompareTo(b.Flash_start_address));
            
            return soiSymbols;
        }

        /// <summary>
        /// Finds all unique codebanks that contain SOI maps.
        /// </summary>
        public List<int> FindCodeBanksWithSOIMaps(SymbolCollection symbols)
        {
            var codeBanks = new HashSet<int>();
            if (symbols == null) return new List<int>();
            
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && (s.Varname.Contains("Start of") || s.Varname.Contains("SOI")))
                {
                    if ((s.X_axis_length >= 10 && s.Y_axis_length >= 10) || s.Varname.Contains("(SOI)") || s.Varname.Contains("(N108 SOI)"))
                    {
                        // Extract codebank from symbol name pattern like "Start of injection (SOI) X °C [N]"
                        int codeBank = ExtractCodeBank(s.Varname);
                        
                        if (codeBank < 0)
                        {
                            // Fallback to CodeBlock property if name pattern fails
                            codeBank = s.CodeBlock;
                        }

                        if (codeBank >= 0)
                        {
                            codeBanks.Add(codeBank);
                        }
                    }
                }
            }
            
            var sortedBanks = new List<int>(codeBanks);
            sortedBanks.Sort();
            return sortedBanks;
        }

        /// <summary>
        /// Finds SOI maps for a specific codebank.
        /// Only returns maps that have a temperature in their name (e.g., "Start of injection (SOI) -20 °C").
        /// Excludes maps like "SOI limiter (temperature)" that don't have temperature values.
        /// </summary>
        public List<SymbolHelper> FindSOIMapsByCodeBank(SymbolCollection symbols, int codeBank)
        {
            var soiSymbols = new List<SymbolHelper>();
            if (symbols == null) return soiSymbols;
            
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && (s.Varname.Contains("Start of") || s.Varname.Contains("SOI")))
                {
                    if ((s.X_axis_length >= 10 && s.Y_axis_length >= 10) || s.Varname.Contains("(SOI)") || s.Varname.Contains("(N108 SOI)"))
                    {
                        int cb = ExtractCodeBank(s.Varname);
                        if (cb < 0) cb = s.CodeBlock;

                        if (cb == codeBank)
                        {
                            // Only add maps that have a temperature in their name
                            // This filters out "SOI limiter (temperature)" and similar non-temperature maps
                            double temp = ExtractTemperature(s.Varname);
                            if (!double.IsNaN(temp))
                            {
                                soiSymbols.Add(s);
                            }
                        }
                    }
                }
            }
            
            // Sort by extracting temperature from name
            soiSymbols.Sort((a, b) =>
            {
                double tempA = ExtractTemperature(a.Varname);
                double tempB = ExtractTemperature(b.Varname);
                return tempA.CompareTo(tempB);
            });
            
            return soiSymbols;
        }

        /// <summary>
        /// Finds duration maps for a specific codebank.
        /// Sorts by the numeric suffix in the name (e.g., "Injector duration 05" -> index 5).
        /// This ensures the list order matches the selector map indices.
        /// </summary>
        public List<SymbolHelper> FindDurationMapsByCodeBank(SymbolCollection symbols, int codeBank)
        {
            var durationSymbols = new List<SymbolHelper>();
            if (symbols == null) return durationSymbols;
            
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && s.Varname.Contains("Injector duration"))
                {
                    int cb = ExtractCodeBank(s.Varname);
                    if (cb < 0) cb = s.CodeBlock;

                    if (cb == codeBank)
                    {
                        durationSymbols.Add(s);
                    }
                }
            }
            
            // Sort by the numeric suffix in the name to match selector indices
            // Pattern: "Injector duration XX" where XX is a 1-2 digit number
            durationSymbols.Sort((a, b) => {
                int indexA = ExtractDurationMapIndex(a.Varname);
                int indexB = ExtractDurationMapIndex(b.Varname);
                return indexA.CompareTo(indexB);
            });
            
            return durationSymbols;
        }
        
        /// <summary>
        /// Extracts the numeric index from a duration map name.
        /// E.g., "Injector duration 05" -> 5, "Injector duration 10" -> 10
        /// </summary>
        private int ExtractDurationMapIndex(string varname)
        {
            if (string.IsNullOrEmpty(varname)) return 0;
            
            // Look for pattern "Injector duration XX" at the start
            const string prefix = "Injector duration ";
            if (varname.StartsWith(prefix))
            {
                string numberPart = varname.Substring(prefix.Length);
                // Extract leading digits
                int i = 0;
                while (i < numberPart.Length && char.IsDigit(numberPart[i]))
                {
                    i++;
                }
                if (i > 0)
                {
                    string numberStr = numberPart.Substring(0, i);
                    if (int.TryParse(numberStr, out int index))
                    {
                        return index;
                    }
                }
            }
            
            // Fallback: sort by full name
            return 0;
        }

        /// <summary>
        /// Finds the selector map for a specific codebank.
        /// </summary>
        public SymbolHelper FindSelectorMapByCodeBank(SymbolCollection symbols, int codeBank)
        {
            if (symbols == null) return null;
            
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && s.Varname.Contains("Selector for"))
                {
                    int cb = ExtractCodeBank(s.Varname);
                    if (cb < 0) cb = s.CodeBlock;

                    if (cb == codeBank)
                    {
                        return s;
                    }
                }
            }
            
            // Fallback: Search by characteristics
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.X_axis_length == 6 &&
                    s.Y_axis_length == 1 &&
                    s.YaxisUnits != null &&
                    s.YaxisUnits.Contains("Grad KW"))
                {
                    int cb = ExtractCodeBank(s.Varname);
                    if (cb == codeBank)
                    {
                        return s;
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Extracts the codebank number from a symbol name.
        /// Pattern: "Name [N]" where N is the flash bank number
        /// </summary>
        private int ExtractCodeBank(string varname)
        {
            if (string.IsNullOrEmpty(varname)) return -1;
            
            // Look for pattern like "[N]" at the end of the name
            int bracketStart = varname.LastIndexOf('[');
            int bracketEnd = varname.LastIndexOf(']');
            
            if (bracketStart >= 0 && bracketEnd > bracketStart)
            {
                string numberStr = varname.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                if (int.TryParse(numberStr, out int codeBank))
                {
                    return codeBank;
                }
            }
            
            // Alternative: check CodeBlock property directly
            return -1;
        }

        /// <summary>
        /// Extracts temperature from SOI map name.
        /// Pattern: "Start of injection (SOI) X °C" or similar
        /// </summary>
        /// <summary>
        /// Extracts temperature from SOI map name.
        /// Returns double.NaN if no valid temperature is found in the name.
        /// </summary>
        public double ExtractTemperature(string varname)
        {
            if (string.IsNullOrEmpty(varname)) return double.NaN;
            
            // Look for pattern like "X °C", "X°C", "-X °C", "-X°C"
            int degreeIndex = varname.IndexOf("°C");
            if (degreeIndex < 0)
            {
                degreeIndex = varname.IndexOf("deg C");
            }
            
            if (degreeIndex >= 0)
            {
                // Find the start of the number by going backwards from the degree symbol
                int i = degreeIndex - 1;
                
                // Skip any whitespace between number and °C
                while (i >= 0 && char.IsWhiteSpace(varname[i]))
                {
                    i--;
                }
                
                if (i < 0) return double.NaN;
                
                // Now find the start of the number (could include negative sign)
                int numStart = i;
                while (numStart >= 0 && (char.IsDigit(varname[numStart]) || varname[numStart] == '-' || varname[numStart] == '.'))
                {
                    numStart--;
                }
                
                // Extract the number
                string numberStr = varname.Substring(numStart + 1, i - numStart).Trim();
                
                if (double.TryParse(numberStr, out double temp))
                {
                    return temp;
                }
            }
            
            // Return NaN if no valid temperature found - don't use default
            return double.NaN;
        }

        /// <summary>
        /// Gets all unique temperatures from SOI maps in a codebank.
        /// </summary>
        public List<double> GetSOITemperatures(SymbolCollection symbols, int codeBank)
        {
            var temps = new HashSet<double>();
            var soiMaps = FindSOIMapsByCodeBank(symbols, codeBank);
            
            for (int i = 0; i < soiMaps.Count; i++)
            {
                SymbolHelper s = soiMaps[i];
                double temp = ExtractTemperature(s.Varname);
                
                // Only add if we found a valid temperature (not NaN)
                if (!double.IsNaN(temp))
                {
                    temps.Add(temp);
                }
            }
            
            var sortedTemps = new List<double>(temps);
            sortedTemps.Sort();
            return sortedTemps;
        }

        /// <summary>
        /// Determines if the ECU uses multiple duration maps with selector.
        /// </summary>
        public bool UsesMultipleDurationMaps(SymbolCollection symbols, int codeBank)
        {
            var durationMaps = FindDurationMapsByCodeBank(symbols, codeBank);
            var selectorMap = FindSelectorMapByCodeBank(symbols, codeBank);
            
            // Multiple duration ECU if:
            // 1. More than one duration map exists
            // 2. A selector map exists
            return durationMaps.Count > 1 && selectorMap != null;
        }
        
        /// <summary>
        /// Finds duration maps (DURA_0 through DURA_5).
        /// Translated from mapfinder.py:get_durations()
        /// </summary>
        public List<SymbolHelper> FindDurationMaps(SymbolCollection symbols)
        {
            if (symbols == null) return new List<SymbolHelper>();
            
            var durationSymbols = new List<SymbolHelper>();
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && s.Varname.Contains("Injector duration"))
                {
                    durationSymbols.Add(s);
                }
            }
            
            // Sort by the numeric suffix in the name to match selector indices
            // Pattern: "Injector duration XX" where XX is a 1-2 digit number
            durationSymbols.Sort((a, b) => {
                int indexA = ExtractDurationMapIndex(a.Varname);
                int indexB = ExtractDurationMapIndex(b.Varname);
                return indexA.CompareTo(indexB);
            });
            
            return durationSymbols;
        }
        
        /// <summary>
        /// Finds the duration selector map.
        /// Translated from mapfinder.py:get_selector()
        /// </summary>
        public SymbolHelper FindSelectorMap(SymbolCollection symbols)
        {
            if (symbols == null) return null;
            
            // Primary: Search by name
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.Varname != null && s.Varname.Contains("Selector for"))
                {
                    return s;
                }
            }
            
            // Fallback: Search by characteristics
            // X_axis_length == 6, Y_axis_length == 1, YaxisUnits == "Grad KW"
            for (int i = 0; i < symbols.Count; i++)
            {
                SymbolHelper s = symbols[i];
                if (s.X_axis_length == 6 && 
                    s.Y_axis_length == 1 && 
                    s.YaxisUnits != null && 
                    s.YaxisUnits.Contains("Grad KW"))
                {
                    return s;
                }
            }
            
            return null; // May be null if not found
        }
        
        /// <summary>
        /// Applies correction formula to SOI map values.
        /// Formula from mapfinder.py line 138: x * -0.023437 + 78
        /// </summary>
        public double ApplySOICorrection(int rawValue)
        {
            return rawValue * -0.023437 + 78.0;
        }
        
        /// <summary>
        /// Applies correction formula to duration map values.
        /// Formula from mapfinder.py line 187: x * 0.023437
        /// </summary>
        public double ApplyDurationCorrection(int rawValue)
        {
            return rawValue * 0.023437;
        }
        
        /// <summary>
        /// Applies correction formula to selector map values.
        /// Formula from mapfinder.py line 170: round(x / 256, 0)
        /// </summary>
        public int ApplySelectorCorrection(int rawValue)
        {
            return (int)Math.Round(rawValue / 256.0);
        }
        
        /// <summary>
        /// Applies X-axis correction for SOI maps.
        /// Formula from mapfinder.py line 144: x * 0.01
        /// </summary>
        public double ApplySOIXAxisCorrection(int rawValue)
        {
            return rawValue * 0.01;
        }
        
        /// <summary>
        /// Applies Y-axis correction for duration maps.
        /// Formula from mapfinder.py line 191: x (identity)
        /// </summary>
        public double ApplyDurationYAxisCorrection(int rawValue)
        {
            return rawValue;
        }
        
        /// <summary>
        /// Static helper to extract codebank number from symbol name.
        /// </summary>
        public static int ExtractCodeBankStatic(string varname)
        {
            if (string.IsNullOrEmpty(varname)) return -1;
            
            int bracketStart = varname.LastIndexOf('[');
            int bracketEnd = varname.LastIndexOf(']');
            
            if (bracketStart >= 0 && bracketEnd > bracketStart)
            {
                string numberStr = varname.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);
                if (int.TryParse(numberStr, out int codeBank))
                {
                    return codeBank;
                }
            }
            
            return -1;
        }
        
        /// <summary>
        /// Static helper to extract temperature from SOI map name.
        /// </summary>
        public static double ExtractTemperatureStatic(string varname)
        {
            if (string.IsNullOrEmpty(varname)) return -1;
            
            int degreeIndex = varname.IndexOf("°C");
            if (degreeIndex < 0)
            {
                degreeIndex = varname.IndexOf("deg C");
            }
            
            if (degreeIndex >= 0)
            {
                // Find the start of the number by going backwards from the degree symbol
                int i = degreeIndex - 1;
                
                // Skip any whitespace between number and °C
                while (i >= 0 && char.IsWhiteSpace(varname[i]))
                {
                    i--;
                }
                
                if (i < 0) return 90.0;
                
                // Now find the start of the number (could include negative sign)
                int numStart = i;
                while (numStart >= 0 && (char.IsDigit(varname[numStart]) || varname[numStart] == '-' || varname[numStart] == '.'))
                {
                    numStart--;
                }
                
                // Extract the number
                string numberStr = varname.Substring(numStart + 1, i - numStart).Trim();
                if (double.TryParse(numberStr, out double temp))
                {
                    return temp;
                }
            }
            
            return 90.0;
        }
    }
}
