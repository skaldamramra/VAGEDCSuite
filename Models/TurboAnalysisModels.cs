using System;
using System.Collections.Generic;
using System.Drawing;

namespace VAGSuite.Models
{
    public class EngineParameters
    {
        public double DisplacementLiters { get; set; } = 1.9;
        public double DisplacementCubicInches => DisplacementLiters * 61.0237;
        public int NumberOfCylinders { get; set; } = 4;
        public string EngineCode { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string SoftwareVersion { get; set; } = string.Empty;
    }

    public class BoostMapData
    {
        public string MapName { get; set; } = string.Empty;
        public double[] RpmPoints { get; set; }
        public double[] BoostPressureBar { get; set; } // Absolute
        public double SelectedIQ { get; set; } // The IQ used for the slice
        public long FlashAddress { get; set; }
        public int DebugXLen { get; set; }
        public int DebugYLen { get; set; }
        public string DebugXDescr { get; set; }
        public string DebugYDescr { get; set; }

        public int PointCount => RpmPoints?.Length ?? 0;

        public bool IsValid()
        {
            return RpmPoints != null && 
                   BoostPressureBar != null && 
                   RpmPoints.Length == BoostPressureBar.Length &&
                   RpmPoints.Length > 0;
        }
    }

    public class EnvironmentalParameters
    {
        public double AltitudeMeters { get; set; } = 0;
        public double AmbientTempCelsius { get; set; } = 20;
        public double VolumetricEfficiencyPercent { get; set; } = 85;

        public double AtmosphericPressurePSI
        {
            get
            {
                const double seaLevelPSI = 14.7;
                double altitudeFeet = AltitudeMeters * 3.28084;
                return seaLevelPSI - (altitudeFeet / 2000.0);
            }
        }

        public double VolumetricEfficiencyDecimal => VolumetricEfficiencyPercent / 100.0;
    }

    public class CompressorPlotPoint
    {
        public double RPM { get; set; }
        public double BoostPressureBar { get; set; } // Absolute
        public double MassAirflowLbsMin { get; set; } // Corrected
        public double PressureRatio { get; set; }
        public double IntakeManifoldTempC { get; set; }
        public double VolumeFlowCFM { get; set; }
        public double InjectedQuantityMgStroke { get; set; }
        public AtmosphericCondition Condition { get; set; }
    }

    public enum AtmosphericCondition
    {
        Nominal,
        HighAltitude,
        SeaLevel
    }

    public enum CompressorMapType
    {
        None = -1,
        GT17 = 0,
        T25_55 = 1,
        T25_60 = 2,
        TD04 = 3,
        TD0416T = 4,
        TD0418T = 5,
        TD0419T = 6,
        TD0620G = 7,
        GT2871R = 8,
        GT28RS = 9,
        GT3071R86 = 10,
        GT30R = 11,
        GT40R = 12,
        HX40W = 13
    }

    public class CompressorMapCoordinates
    {
        public double XOffset { get; set; }
        public double YOffset { get; set; }
        public double XMultiplier { get; set; }
        public double YMultiplier { get; set; }
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public CompressorMapType MapType { get; set; }
        public string ResourceName { get; set; }
    }
}
