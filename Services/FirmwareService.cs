using System;
using System.Collections.Generic;
using System.IO;

namespace VAGSuite.Services
{
    public class FirmwareService
    {
        private AppSettings _appSettings;

        public FirmwareService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        /// <summary>
        /// Gets firmware information from the binary file
        /// </summary>
        public FirmwareInfo GetFirmwareInfo(string currentFile)
        {
            var info = new FirmwareInfo();

            if (string.IsNullOrEmpty(currentFile) || !File.Exists(currentFile))
            {
                return info;
            }

            try
            {
                byte[] allBytes = File.ReadAllBytes(currentFile);
                IEDCFileParser parser = Tools.Instance.GetParserForFile(currentFile, false);

                if (parser == null)
                {
                    return info;
                }

                partNumberConverter pnc = new partNumberConverter();
                ECUInfo ecuinfo = pnc.ConvertPartnumber(parser.ExtractBoschPartnumber(allBytes), allBytes.Length);

                info.PartNumber = parser.ExtractBoschPartnumber(allBytes);
                info.SoftwareID = ecuinfo.SoftwareID;
                if (string.IsNullOrEmpty(info.SoftwareID))
                {
                    info.SoftwareID = parser.ExtractPartnumber(allBytes);
                }
                info.SoftwareVersion = parser.ExtractSoftwareNumber(allBytes);
                info.AdditionalInfo = parser.ExtractInfo(allBytes);
                info.CarMake = ecuinfo.CarMake;
                info.CarType = ecuinfo.CarType;
                info.EngineType = FormatEngineType(ecuinfo);
                info.EcuType = ecuinfo.EcuType;

                // Add horsepower/torque info if available
                string hpinfo = string.Empty;
                string tqinfo = string.Empty;
                if (ecuinfo.HP > 0) hpinfo = ecuinfo.HP.ToString() + " bhp";
                if (ecuinfo.TQ > 0) tqinfo = ecuinfo.TQ.ToString() + " Nm";

                if (!string.IsNullOrEmpty(hpinfo) || !string.IsNullOrEmpty(tqinfo))
                {
                    info.EngineType += " (";
                    if (!string.IsNullOrEmpty(hpinfo)) info.EngineType += hpinfo;
                    if (!string.IsNullOrEmpty(hpinfo) && !string.IsNullOrEmpty(tqinfo)) info.EngineType += "/";
                    if (!string.IsNullOrEmpty(tqinfo)) info.EngineType += tqinfo;
                    info.EngineType += ")";
                }

                // Get checksum type
                var checksumResult = Tools.Instance.UpdateChecksum(currentFile, true);
                info.ChecksumType = FormatChecksumType(checksumResult);

                // Get number of codeblocks
                info.NumberOfCodeblocks = DetermineNumberOfCodeblocks();
            }
            catch (Exception E)
            {
                Console.WriteLine("GetFirmwareInfo error: " + E.Message);
            }

            return info;
        }

        /// <summary>
        /// Determines the number of codeblocks in the current file
        /// </summary>
        public int DetermineNumberOfCodeblocks()
        {
            List<int> blockIds = new List<int>();
            foreach (SymbolHelper sh in Tools.Instance.m_symbols)
            {
                if (!blockIds.Contains(sh.CodeBlock) && sh.CodeBlock != 0) blockIds.Add(sh.CodeBlock);
            }
            return blockIds.Count;
        }

        private string FormatEngineType(ECUInfo ecuinfo)
        {
            string enginedetails = ecuinfo.EngineType;
            if (!string.IsNullOrEmpty(ecuinfo.HP.ToString()) && ecuinfo.HP > 0)
            {
                enginedetails += " (" + ecuinfo.HP.ToString() + " HP)";
            }
            return enginedetails;
        }

        private string FormatChecksumType(ChecksumResultDetails result)
        {
            string chkType = string.Empty;

            if (result.TypeResult == ChecksumType.VAG_EDC15P_V41) chkType = "VAG EDC15P V4.1";
            else if (result.TypeResult == ChecksumType.VAG_EDC15P_V41V2) chkType = "VAG EDC15P V4.1v2";
            else if (result.TypeResult == ChecksumType.VAG_EDC15P_V41_2002) chkType = "VAG EDC15P V4.1 2002+";
            else if (result.TypeResult != ChecksumType.Unknown) chkType = result.TypeResult.ToString();

            chkType += " " + result.CalculationResult.ToString();

            return chkType;
        }
    }

    /// <summary>
    /// Firmware information container
    /// </summary>
    public class FirmwareInfo
    {
        public string PartNumber { get; set; }
        public string SoftwareID { get; set; }
        public string SoftwareVersion { get; set; }
        public string AdditionalInfo { get; set; }
        public string CarMake { get; set; }
        public string CarType { get; set; }
        public string EngineType { get; set; }
        public string EcuType { get; set; }
        public string ChecksumType { get; set; }
        public int NumberOfCodeblocks { get; set; }
    }
}