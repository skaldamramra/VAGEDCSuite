using System;
using System.Collections.Generic;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service interface for End of Injection (EOI) calculations.
    /// </summary>
    public interface IEOICalculatorService
    {
        /// <summary>
        /// Shows the EOI Calculator form with automatic map detection.
        /// </summary>
        void ShowEOICalculator();
        
        /// <summary>
        /// Calculates EOI for the specified maps.
        /// </summary>
        /// <param name="soiMap">Start of Injection map</param>
        /// <param name="durationMaps">List of injector duration maps</param>
        /// <param name="selectorMap">Duration selector map (null if single duration)</param>
        /// <returns>Calculated EOI result</returns>
        EOICalculationResult CalculateEOI(
            EOIMap soiMap, 
            DurationMapSet durationMaps, 
            EOIMap selectorMap);
        
        /// <summary>
        /// Validates that all required maps are available.
        /// </summary>
        /// <param name="symbols">Symbol collection to search</param>
        /// <param name="missingMaps">Output list of missing map names</param>
        /// <returns>True if all required maps found</returns>
        bool ValidateRequiredMaps(
            SymbolCollection symbols, 
            out List<string> missingMaps);
        
        /// <summary>
        /// Gets all available codebanks with SOI maps.
        /// </summary>
        List<int> GetAvailableCodeBanks();
        
        /// <summary>
        /// Gets SOI maps for a specific codebank.
        /// </summary>
        List<SymbolHelper> GetSOIMapsForCodeBank(int codeBank);
        
        /// <summary>
        /// Gets duration maps for a specific codebank.
        /// </summary>
        List<SymbolHelper> GetDurationMapsForCodeBank(int codeBank);
        
        /// <summary>
        /// Gets the selector map for a specific codebank.
        /// </summary>
        SymbolHelper GetSelectorMapForCodeBank(int codeBank);
        
        /// <summary>
        /// Gets all available temperatures from SOI maps in a codebank.
        /// </summary>
        List<double> GetSOITemperaturesForCodeBank(int codeBank);
        
        /// <summary>
        /// Determines if the ECU uses multiple duration maps with selector for a codebank.
        /// </summary>
        bool UsesMultipleDurationMapsForCodeBank(int codeBank);
    }
}
