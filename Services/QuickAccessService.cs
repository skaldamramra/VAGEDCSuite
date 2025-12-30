using System;

namespace VAGSuite.Services
{
    public class QuickAccessService
    {
        private MapViewerService _mapViewerService;

        public QuickAccessService(MapViewerService mapViewerService)
        {
            _mapViewerService = mapViewerService;
        }

        /// <summary>
        /// Opens a table viewer for Driver wish map
        /// </summary>
        public void OpenDriverWish()
        {
            FindAndOpenTableViewer("Driver wish", 2);
        }

        /// <summary>
        /// Opens a table viewer for Torque limiter map
        /// </summary>
        public void OpenTorqueLimiter()
        {
            FindAndOpenTableViewer("Torque limiter", 2);
        }

        /// <summary>
        /// Opens a table viewer for Smoke limiter map
        /// </summary>
        public void OpenSmokeLimiter()
        {
            FindAndOpenTableViewer("Smoke limiter", 2);
        }

        /// <summary>
        /// Opens a table viewer for Boost target map
        /// </summary>
        public void OpenBoostTargetMap()
        {
            FindAndOpenTableViewer("Boost target map", 2);
        }

        /// <summary>
        /// Opens a table viewer for Boost limit map
        /// </summary>
        public void OpenBoostLimitMap()
        {
            FindAndOpenTableViewer("Boost limit map", 2);
        }

        /// <summary>
        /// Opens a table viewer for SVBL Boost limiter map
        /// </summary>
        public void OpenSVBLBoostLimiter()
        {
            FindAndOpenTableViewer("SVBL Boost limiter", 2);
        }

        /// <summary>
        /// Opens a table viewer for N75 duty cycle map
        /// </summary>
        public void OpenN75DutyCycle()
        {
            FindAndOpenTableViewer("N75 duty cycle", 2);
        }

        /// <summary>
        /// Opens a table viewer for EGR map
        /// </summary>
        public void OpenEGRMap()
        {
            FindAndOpenTableViewer("EGR", 2);
        }

        /// <summary>
        /// Opens a table viewer for IQ by MAP map
        /// </summary>
        public void OpenIQByMAP()
        {
            FindAndOpenTableViewer("IQ by MAP", 2);
        }

        /// <summary>
        /// Opens a table viewer for IQ by MAF map
        /// </summary>
        public void OpenIQByMAF()
        {
            FindAndOpenTableViewer("IQ by MAF", 2);
        }

        /// <summary>
        /// Opens a table viewer for SOI limiter map
        /// </summary>
        public void OpenSOILimiter()
        {
            FindAndOpenTableViewer("SOI limiter", 2);
        }

        /// <summary>
        /// Opens a table viewer for Start of injection map
        /// </summary>
        public void OpenStartOfInjection()
        {
            FindAndOpenTableViewer("Start of injection", 2);
        }

        /// <summary>
        /// Opens a table viewer for Injector duration map
        /// </summary>
        public void OpenInjectorDuration()
        {
            FindAndOpenTableViewer("Injector duration", 2);
        }

        /// <summary>
        /// Opens a table viewer for Start IQ map
        /// </summary>
        public void OpenStartIQ()
        {
            FindAndOpenTableViewer("Start IQ", 2);
        }

        /// <summary>
        /// Opens a table viewer for BIP Basic characteristic line
        /// </summary>
        public void OpenBIPBasicCharacteristic()
        {
            FindAndOpenTableViewer("BIP Basic characteristic line", 2);
        }

        /// <summary>
        /// Opens a table viewer for PID map P (Proportional)
        /// </summary>
        public void OpenPIDMapP()
        {
            FindAndOpenTableViewer("PID map - P amplification", 2);
        }

        /// <summary>
        /// Opens a table viewer for PID map I (Integral)
        /// </summary>
        public void OpenPIDMapI()
        {
            FindAndOpenTableViewer("PID map - I amplification", 2);
        }

        /// <summary>
        /// Opens a table viewer for PID map D (Derivative)
        /// </summary>
        public void OpenPIDMapD()
        {
            FindAndOpenTableViewer("PID map - D amplification", 2);
        }

        /// <summary>
        /// Opens a table viewer for Injection duration limiter
        /// </summary>
        public void OpenDurationLimiter()
        {
            FindAndOpenTableViewer("Injection duration limiter B", 2);
        }

        /// <summary>
        /// Opens a table viewer for IQ by MAF limiter
        /// </summary>
        public void OpenMAFLimiter()
        {
            FindAndOpenTableViewer("IQ by MAF limiter 1", 2);
        }

        /// <summary>
        /// Opens a table viewer for IQ by MAP limiter
        /// </summary>
        public void OpenMAPLimiter()
        {
            FindAndOpenTableViewer("IQ by MAP limiter", 2);
        }

        /// <summary>
        /// Opens a table viewer for MAF linearization
        /// </summary>
        public void OpenMAFLinearization()
        {
            FindAndOpenTableViewer("MAF linearization", 2);
        }

        /// <summary>
        /// Opens a table viewer for MAP linearization
        /// </summary>
        public void OpenMAPLinearization()
        {
            FindAndOpenTableViewer("MAP linearization", 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic IQ Limit (1-10)
        /// </summary>
        public void OpenVCDSDiagnosticIQLimit(int limitNumber)
        {
            if (limitNumber < 1 || limitNumber > 10) return;
            FindAndOpenTableViewer("VCDS Diagnostic IQ Limit " + limitNumber, 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic MAF Limit (1-2)
        /// </summary>
        public void OpenVCDSDiagnosticMAFLimit(int limitNumber)
        {
            if (limitNumber < 1 || limitNumber > 2) return;
            FindAndOpenTableViewer("VCDS Diagnostic MAF Limit " + limitNumber, 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic MAP Limit (1-3)
        /// </summary>
        public void OpenVCDSDiagnosticMAPLimit(int limitNumber)
        {
            if (limitNumber < 1 || limitNumber > 3) return;
            FindAndOpenTableViewer("VCDS Diagnostic MAP Limit " + limitNumber, 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic Torque Limit
        /// </summary>
        public void OpenVCDSDiagnosticTorqueLimit()
        {
            FindAndOpenTableViewer("VCDS Diagnostic Torque Limit", 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic Torque Display offset
        /// </summary>
        public void OpenVCDSDiagnosticTorqueOffset()
        {
            FindAndOpenTableViewer("VCDS Diagnostic Torque Display offset", 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic MAF Display offset
        /// </summary>
        public void OpenVCDSDiagnosticMAFOffset()
        {
            FindAndOpenTableViewer("VCDS Diagnostic MAF Display offset", 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic MAP Display offset
        /// </summary>
        public void OpenVCDSDiagnosticMAPOffset()
        {
            FindAndOpenTableViewer("VCDS Diagnostic MAP Display offset", 2);
        }

        /// <summary>
        /// Opens a table viewer for VCDS Diagnostic IQ Display offset
        /// </summary>
        public void OpenVCDSDiagnosticIQOffset()
        {
            FindAndOpenTableViewer("VCDS Diagnostic IQ Display offset", 2);
        }

        /// <summary>
        /// Helper method to find and open a table viewer by symbol name and code block
        /// </summary>
        private void FindAndOpenTableViewer(string symbolName, int codeBlock)
        {
            // Access symbols dynamically from Tools.Instance
            SymbolCollection currentSymbols = Tools.Instance.m_symbols;
            if (currentSymbols == null) return;

            foreach (SymbolHelper sh in currentSymbols)
            {
                if (sh.Varname.StartsWith(symbolName) && sh.CodeBlock == codeBlock)
                {
                    _mapViewerService.StartTableViewer(sh, Tools.Instance.m_currentfile, currentSymbols);
                    return;
                }
            }

            // Try userdescription as fallback
            foreach (SymbolHelper sh in currentSymbols)
            {
                if (sh.Userdescription != null && sh.Userdescription.StartsWith(symbolName) && sh.CodeBlock == codeBlock)
                {
                    _mapViewerService.StartTableViewer(sh, Tools.Instance.m_currentfile, currentSymbols);
                    return;
                }
            }
        }
    }
}