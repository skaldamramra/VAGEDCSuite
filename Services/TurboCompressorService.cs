using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VAGSuite.Models;
using VAGSuite.Helpers;

namespace VAGSuite.Services
{
    public class TurboCompressorService
    {
        private readonly IEDCFileParser _fileParser;
        private readonly SymbolCollection _symbols;

        public TurboCompressorService(IEDCFileParser fileParser, SymbolCollection symbols)
        {
            _fileParser = fileParser;
            _symbols = symbols;
        }

        public EngineParameters ExtractEngineParameters(string currentFile)
        {
            var engineParams = new EngineParameters();
            if (string.IsNullOrEmpty(currentFile) || !File.Exists(currentFile)) return engineParams;

            try
            {
                byte[] allBytes = File.ReadAllBytes(currentFile);
                string infoString = _fileParser.ExtractInfo(allBytes);
                engineParams.PartNumber = _fileParser.ExtractPartnumber(allBytes);
                engineParams.SoftwareVersion = _fileParser.ExtractSoftwareNumber(allBytes);

                // Extract displacement using Regex (e.g. "1,9l", "2.5L")
                var displacementMatch = Regex.Match(infoString, @"(\d)[,.](\d)\s*[lL]");
                if (displacementMatch.Success)
                {
                    double major = double.Parse(displacementMatch.Groups[1].Value);
                    double minor = double.Parse(displacementMatch.Groups[2].Value);
                    engineParams.DisplacementLiters = major + (minor / 10.0);
                }
                else
                {
                    // Fallback based on common VAG engines
                    if (infoString.ToUpper().Contains("1.9") || infoString.ToUpper().Contains("1,9"))
                        engineParams.DisplacementLiters = 1.9;
                    else if (infoString.ToUpper().Contains("2.5") || infoString.ToUpper().Contains("2,5"))
                        engineParams.DisplacementLiters = 2.5;
                    else if (infoString.ToUpper().Contains("2.0") || infoString.ToUpper().Contains("2,0"))
                        engineParams.DisplacementLiters = 2.0;
                }

                // Get cylinders
                partNumberConverter pnc = new partNumberConverter();
                engineParams.NumberOfCylinders = pnc.GetNumberOfCylinders(string.Empty, infoString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting engine parameters: " + ex.Message);
            }

            return engineParams;
        }

        public List<SymbolHelper> GetAllBoostMaps(SymbolCollection symbols)
        {
            string[] searchNames = { "Boost target map", "Desired boost", "Boost map", "KFLDRL", "LDRXN" };
            var matches = symbols.Cast<SymbolHelper>()
                .Where(s => searchNames.Any(name => s.Varname.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(s => s.CodeBlock)
                .ThenBy(s => s.Flash_start_address)
                .ToList();
            return matches;
        }

        public BoostMapData ExtractBoostMap(string filename, SymbolHelper boostSymbol)
        {
            var data = new BoostMapData();
            if (boostSymbol != null)
            {
                data.MapName = boostSymbol.Varname;
                data.FlashAddress = boostSymbol.Flash_start_address;
                data.DebugXLen = boostSymbol.X_axis_length;
                data.DebugYLen = boostSymbol.Y_axis_length;
                data.DebugXDescr = boostSymbol.X_axis_descr;
                data.DebugYDescr = boostSymbol.Y_axis_descr;

                // SKEPTICAL: Robust axis identification.
                // In EDC15/16 files, map headers are [X ID] [X Len] [X Data...] [Y ID] [Y Len] [Y Data...]
                // The data layout follows: Index = X_idx * Y_len + Y_idx.
                // According to MapRules XML: X = IQ, Y = RPM.
                
                // GetXAxisValues reads from Y_axis_address (which should be RPM)
                // GetYAxisValues reads from X_axis_address (which should be IQ)
                int[] axisYRaw = SymbolQueryHelper.GetXAxisValues(filename, _symbols, boostSymbol.Varname);
                int[] axisXRaw = SymbolQueryHelper.GetYAxisValues(filename, _symbols, boostSymbol.Varname);

                int xLen = boostSymbol.X_axis_length; // IQ
                int yLen = boostSymbol.Y_axis_length; // RPM

                // Identification: Prioritize descriptions but sanity check with lengths.
                // In some EDC15 binaries, descriptions are swapped or generic.
                // Usually RPM is the longer axis (16 points).
                bool xIsRpm = boostSymbol.X_axis_descr.ToLower().Contains("rpm") || boostSymbol.XaxisUnits.ToLower().Contains("rpm");
                bool yIsRpm = boostSymbol.Y_axis_descr.ToLower().Contains("rpm") || boostSymbol.YaxisUnits.ToLower().Contains("rpm");
                
                // SKEPTICAL: If length is 16 and other is 10, it's almost certainly 16=RPM, 10=IQ.
                // If descriptions contradict this, the descriptions are likely swapped in the binary/XML.
                if (xLen >= 14 && yLen <= 12)
                {
                    // Strong length signal: X is RPM
                    xIsRpm = true;
                    yIsRpm = false;
                }
                else if (yLen >= 14 && xLen <= 12)
                {
                    // Strong length signal: Y is RPM
                    xIsRpm = false;
                    yIsRpm = true;
                }
                else if (xIsRpm == yIsRpm)
                {
                    // Tie-breaker or neither identified
                    xIsRpm = xLen >= yLen;
                    yIsRpm = !xIsRpm;
                }

                int[] mapData = Tools.Instance.readdatafromfileasint(filename, (int)boostSymbol.Flash_start_address, boostSymbol.Length / 2, Tools.Instance.m_currentFileType);

                // Identify PHYSICAL corrections (RPM usually has correction 1.0, IQ often 0.01 or 0.1)
                double rpmCorrection = 1.0;
                double rpmOffset = 0;
                double iqCorrection = 0.01;
                double iqOffset = 0;

                // Extract corrections from symbol based on their labels
                bool xLabelsRpm = boostSymbol.X_axis_descr.ToLower().Contains("rpm") || boostSymbol.XaxisUnits.ToLower().Contains("rpm");
                bool yLabelsRpm = boostSymbol.Y_axis_descr.ToLower().Contains("rpm") || boostSymbol.YaxisUnits.ToLower().Contains("rpm");

                if (xLabelsRpm)
                {
                    rpmCorrection = boostSymbol.X_axis_correction;
                    rpmOffset = boostSymbol.X_axis_offset;
                    iqCorrection = boostSymbol.Y_axis_correction;
                    iqOffset = boostSymbol.Y_axis_offset;
                }
                else if (yLabelsRpm)
                {
                    rpmCorrection = boostSymbol.Y_axis_correction;
                    rpmOffset = boostSymbol.Y_axis_offset;
                    iqCorrection = boostSymbol.X_axis_correction;
                    iqOffset = boostSymbol.X_axis_offset;
                }
                else
                {
                    // Fallback to defaults if neither labeled
                    if (xLen >= 14) { rpmCorrection = boostSymbol.X_axis_correction; iqCorrection = boostSymbol.Y_axis_correction; }
                    else { rpmCorrection = boostSymbol.Y_axis_correction; iqCorrection = boostSymbol.X_axis_correction; }
                }

                if (xIsRpm)
                {
                    // RPM is X (Outer Loop), IQ is Y (Inner Loop). Layout: RPM_blocks of IQ_values.
                    // Slice = Take the last IQ value from each RPM block.
                    data.RpmPoints = axisXRaw.Select(r => (r * rpmCorrection) + rpmOffset).ToArray();
                    data.SelectedIQ = (axisYRaw[yLen - 1] * iqCorrection) + iqOffset;
                    
                    data.BoostPressureBar = new double[xLen];
                    for (int x = 0; x < xLen; x++)
                    {
                        int index = x * yLen + (yLen - 1);
                        if (index < mapData.Length)
                            data.BoostPressureBar[x] = (mapData[index] * boostSymbol.Correction) + boostSymbol.Offset;
                    }
                }
                else
                {
                    // IQ is X (Outer Loop), RPM is Y (Inner Loop). Layout: IQ_blocks of RPM_values.
                    // Slice = Take the last IQ block (all RPM values).
                    data.RpmPoints = axisYRaw.Select(r => (r * rpmCorrection) + rpmOffset).ToArray();
                    data.SelectedIQ = (axisXRaw[xLen - 1] * iqCorrection) + iqOffset;

                    data.BoostPressureBar = new double[yLen];
                    for (int y = 0; y < yLen; y++)
                    {
                        int index = (xLen - 1) * yLen + y;
                        if (index < mapData.Length)
                            data.BoostPressureBar[y] = (mapData[index] * boostSymbol.Correction) + boostSymbol.Offset;
                    }
                }

                // Final unit conversion and sorting
                if (data.BoostPressureBar != null)
                {
                    // Robust normalization: Determine divisor based on max value in map
                    // to ensure consistent scaling for the entire curve.
                    double maxVal = data.BoostPressureBar.Max();
                    double divisor = 1.0;
                    if (maxVal > 500) divisor = 1000.0;     // mbar (e.g. 2350 -> 2.35)
                    else if (maxVal > 50) divisor = 100.0;  // kPa (e.g. 235 -> 2.35) or bar*100
                    else if (maxVal > 5) divisor = 10.0;    // bar*10 (e.g. 23 -> 2.3)

                    for (int i = 0; i < data.BoostPressureBar.Length; i++)
                    {
                        data.BoostPressureBar[i] /= divisor;
                    }

                    // Ensure points are sorted by RPM to avoid "jagged" plots
                    if (data.IsValid())
                    {
                        var sorted = data.RpmPoints
                            .Select((rpm, index) => new { RPM = rpm, Boost = data.BoostPressureBar[index] })
                            .OrderBy(p => p.RPM)
                            .ToList();

                        data.RpmPoints = sorted.Select(p => p.RPM).ToArray();
                        data.BoostPressureBar = sorted.Select(p => p.Boost).ToArray();
                    }
                }
            }

            return data;
        }

        public double CalculateCorrectedMassAirflow(double rpm, double boostPressureBar, EngineParameters engineParams, EnvironmentalParameters envParams, out double tManifoldC, out double volumeFlowCfm)
        {
            // 1. Calculate Intake Manifold Temperature (estimated)
            tManifoldC = envParams.AmbientTempCelsius + 20; // Rough estimate with IC
            double tManifoldR = (tManifoldC * 1.8) + 32 + 460;

            // 2. Volume Flow (CFM)
            double ve = envParams.VolumetricEfficiencyDecimal;
            double displacementCuIn = engineParams.DisplacementCubicInches;
            volumeFlowCfm = (rpm * displacementCuIn * ve) / (2.0 * 1728.0);

            // 3. Actual Mass Flow (lbs/min)
            double pManifoldPsi = boostPressureBar * 14.5038;
            double actualMassFlow = (pManifoldPsi * volumeFlowCfm * 29.0) / (10.73 * tManifoldR);

            // 4. SAE J1724 Correction to Standard Day (14.7 psia, 59F)
            double pInPsi = envParams.AtmosphericPressurePSI - 0.5; // Assume 0.5 psi intake loss
            double tInR = (envParams.AmbientTempCelsius * 1.8) + 32 + 460;
            
            double correctedMassFlow = actualMassFlow * (14.7 / pInPsi) * Math.Sqrt(tInR / 518.67);

            return correctedMassFlow;
        }

        public double CalculatePressureRatio(double boostPressureBar, double rpm, EnvironmentalParameters envParams)
        {
            // SKEPTICAL: Pressure Ratio = P_out_absolute / P_in_absolute
            // boostPressureBar is already absolute (as converted in ExtractBoostMap).
            double pBoostPsi = boostPressureBar * 14.5038;
            double pAtmPsi = envParams.AtmosphericPressurePSI;
            double intakeLoss = CalculateIntakeLoss((int)rpm);
            
            // Formula: P_out / (P_atm - losses)
            // Current code was: (pBoostPsi + pAtmPsi) / (pAtmPsi - intakeLoss) which double-counts P_atm.
            return pBoostPsi / (pAtmPsi - intakeLoss);
        }

        private double CalculateIntakeLoss(int rpm)
        {
            if (rpm < 880) return 0.08;
            if (rpm < 1260) return 0.10;
            if (rpm < 1640) return 0.17;
            if (rpm < 2020) return 0.28;
            if (rpm < 2400) return 0.42;
            if (rpm < 2780) return 0.50;
            if (rpm < 3160) return 0.58;
            if (rpm < 3540) return 0.65;
            if (rpm < 3920) return 0.74;
            if (rpm < 4300) return 0.82;
            if (rpm < 4680) return 0.92;
            if (rpm < 5060) return 1.03;
            if (rpm < 5440) return 1.07;
            if (rpm < 5820) return 1.10;
            if (rpm < 6000) return 1.08;
            return 1.43;
        }

        public List<CompressorPlotPoint> GeneratePlotPoints(BoostMapData boostMap, EngineParameters engineParams, EnvironmentalParameters envParams)
        {
            var points = new List<CompressorPlotPoint>();
            if (boostMap == null || !boostMap.IsValid()) return points;

            for (int i = 0; i < boostMap.PointCount; i++)
            {
                double rpm = boostMap.RpmPoints[i];
                double boost = boostMap.BoostPressureBar[i];

                double tManifold, vFlow;
                double massAirflow = CalculateCorrectedMassAirflow(rpm, boost, engineParams, envParams, out tManifold, out vFlow);

                points.Add(new CompressorPlotPoint
                {
                    RPM = rpm,
                    BoostPressureBar = boost,
                    MassAirflowLbsMin = massAirflow,
                    PressureRatio = CalculatePressureRatio(boost, rpm, envParams),
                    IntakeManifoldTempC = tManifold,
                    VolumeFlowCFM = vFlow,
                    InjectedQuantityMgStroke = boostMap.SelectedIQ,
                    Condition = AtmosphericCondition.Nominal
                });
            }

            return points;
        }
    }
}
