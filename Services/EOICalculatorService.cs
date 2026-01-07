using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ComponentFactory.Krypton.Docking;
using VAGSuite.Models;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    /// <summary>
    /// Service implementation for End of Injection (EOI) calculations.
    /// </summary>
    public class EOICalculatorService : IEOICalculatorService
    {
        private readonly AppSettings _appSettings;
        private readonly KryptonDockingManager _dockingManager;
        private readonly MapViewerService _mapViewerService;
        private readonly EOIMapFinder _mapFinder;
        private readonly EOICalculator _calculator;
        
        public EOICalculatorService(
            AppSettings appSettings,
            KryptonDockingManager dockingManager,
            MapViewerService mapViewerService)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _dockingManager = dockingManager;
            _mapViewerService = mapViewerService ?? throw new ArgumentNullException(nameof(mapViewerService));
            _mapFinder = new EOIMapFinder();
            _calculator = new EOICalculator();
        }
        
        public void ShowEOICalculator()
        {
            try
            {
                // Validate current file
                if (string.IsNullOrEmpty(Tools.Instance.m_currentfile))
                {
                    MessageBox.Show(
                        "Please open a binary file first.",
                        "No File Loaded",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
                
                // Find all codebanks with SOI maps
                var codeBanks = _mapFinder.FindCodeBanksWithSOIMaps(Tools.Instance.m_symbols);
                
                // Get all maps
                var soiMaps = _mapFinder.FindSOIMaps(Tools.Instance.m_symbols);
                var durationMaps = _mapFinder.FindDurationMaps(Tools.Instance.m_symbols);
                var selectorMap = _mapFinder.FindSelectorMap(Tools.Instance.m_symbols);
                
                if (soiMaps.Count == 0)
                {
                    MessageBox.Show(
                        "No Start of Injection maps found in the current file.",
                        "No Maps Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
                
                // Create and show form
                var form = new frmEOICalculator();
                // Set CalculatorService BEFORE SetMapsWithCodeBankSupport to ensure
                // UpdateTemperatureSlider() can access real SOI temperatures during initialization
                form.CalculatorService = this;
                form.SetMapsWithCodeBankSupport(soiMaps, durationMaps, selectorMap, codeBanks);
                form.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error opening EOI Calculator:\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets all available codebanks with SOI maps.
        /// </summary>
        public List<int> GetAvailableCodeBanks()
        {
            return _mapFinder.FindCodeBanksWithSOIMaps(Tools.Instance.m_symbols);
        }

        /// <summary>
        /// Gets SOI maps for a specific codebank.
        /// </summary>
        public List<SymbolHelper> GetSOIMapsForCodeBank(int codeBank)
        {
            return _mapFinder.FindSOIMapsByCodeBank(Tools.Instance.m_symbols, codeBank);
        }

        /// <summary>
        /// Gets duration maps for a specific codebank.
        /// </summary>
        public List<SymbolHelper> GetDurationMapsForCodeBank(int codeBank)
        {
            return _mapFinder.FindDurationMapsByCodeBank(Tools.Instance.m_symbols, codeBank);
        }

        /// <summary>
        /// Gets the selector map for a specific codebank.
        /// </summary>
        public SymbolHelper GetSelectorMapForCodeBank(int codeBank)
        {
            return _mapFinder.FindSelectorMapByCodeBank(Tools.Instance.m_symbols, codeBank);
        }

        /// <summary>
        /// Gets all available temperatures from SOI maps in a codebank.
        /// </summary>
        public List<double> GetSOITemperaturesForCodeBank(int codeBank)
        {
            return _mapFinder.GetSOITemperatures(Tools.Instance.m_symbols, codeBank);
        }

        /// <summary>
        /// Determines if the ECU uses multiple duration maps with selector for a codebank.
        /// </summary>
        public bool UsesMultipleDurationMapsForCodeBank(int codeBank)
        {
            return _mapFinder.UsesMultipleDurationMaps(Tools.Instance.m_symbols, codeBank);
        }
        
        public EOICalculationResult CalculateEOI(
            EOIMap soiMap,
            DurationMapSet durationMaps,
            EOIMap selectorMap)
        {
            return _calculator.CalculateEOI(soiMap, durationMaps, selectorMap);
        }
        
        public bool ValidateRequiredMaps(
            SymbolCollection symbols,
            out List<string> missingMaps)
        {
            missingMaps = new List<string>();
            
            // Check for SOI maps
            var soiMaps = _mapFinder.FindSOIMaps(symbols);
            if (soiMaps.Count == 0)
            {
                missingMaps.Add("Start of Injection (SOI) map");
            }
            
            // Check for duration maps
            var durationMapList = _mapFinder.FindDurationMaps(symbols);
            if (durationMapList.Count == 0)
            {
                missingMaps.Add("Injector Duration maps");
            }
            
            return missingMaps.Count == 0;
        }
    }
}
