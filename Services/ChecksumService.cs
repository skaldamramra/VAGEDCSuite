using System;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles all checksum verification and correction operations.
    /// Extracted from frmMain to separate checksum logic from UI.
    /// </summary>
    public class ChecksumService
    {
        private readonly AppSettings _appSettings;

        public ChecksumService(AppSettings appSettings)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        /// <summary>
        /// Result of a checksum verification operation
        /// </summary>
        public class ChecksumVerificationResult
        {
            public bool IsValid { get; set; }
            public ChecksumType Type { get; set; }
            public string TypeDescription { get; set; }
            public int NumberTotal { get; set; }
            public int NumberPassed { get; set; }
            public int NumberFailed { get; set; }
            public bool WasUpdated { get; set; }
            public string StatusMessage { get; set; }
        }

        /// <summary>
        /// Verifies and optionally corrects checksums in the file.
        /// </summary>
        public ChecksumVerificationResult VerifyChecksum(string filename, bool showQuestion, bool showInfo)
        {
            var result = new ChecksumVerificationResult
            {
                IsValid = false,
                Type = ChecksumType.Unknown,
                TypeDescription = "---"
            };

            ChecksumResultDetails checksumDetails;

            if (_appSettings.AutoChecksum)
            {
                // Auto-correct mode
                checksumDetails = Tools.Instance.UpdateChecksum(filename, false);
                result.WasUpdated = true;
                result.IsValid = checksumDetails.CalculationOk;
                result.Type = checksumDetails.TypeResult;
                result.TypeDescription = GetChecksumTypeDescription(checksumDetails.TypeResult);
                result.NumberTotal = checksumDetails.NumberChecksumsTotal;
                result.NumberPassed = checksumDetails.NumberChecksumsOk;
                result.NumberFailed = checksumDetails.NumberChecksumsFail;

                if (showInfo)
                {
                    result.StatusMessage = checksumDetails.CalculationOk
                        ? $"Checksums are correct [{result.TypeDescription}]"
                        : $"Checksums are INCORRECT [{result.TypeDescription}]";
                }
            }
            else
            {
                // Verify-only mode first
                checksumDetails = Tools.Instance.UpdateChecksum(filename, true);
                result.IsValid = checksumDetails.CalculationOk;
                result.Type = checksumDetails.TypeResult;
                result.TypeDescription = GetChecksumTypeDescription(checksumDetails.TypeResult);
                result.NumberTotal = checksumDetails.NumberChecksumsTotal;
                result.NumberPassed = checksumDetails.NumberChecksumsOk;
                result.NumberFailed = checksumDetails.NumberChecksumsFail;

                if (!checksumDetails.CalculationOk)
                {
                    if (showQuestion && checksumDetails.TypeResult != ChecksumType.Unknown)
                    {
                        // Caller should show dialog and call CorrectChecksum if user approves
                        result.StatusMessage = $"Checksums invalid. Type: {result.TypeDescription}, " +
                            $"Passed: {result.NumberPassed}/{result.NumberTotal}";
                    }
                    else if (showInfo && checksumDetails.TypeResult == ChecksumType.Unknown)
                    {
                        result.StatusMessage = "Checksum for this filetype is not yet implemented";
                    }
                }
                else if (showInfo)
                {
                    result.StatusMessage = $"Checksums are correct [{result.TypeDescription}]";
                }
            }

            return result;
        }

        /// <summary>
        /// Corrects checksums in the file (called after user confirms).
        /// </summary>
        public ChecksumVerificationResult CorrectChecksum(string filename)
        {
            var checksumDetails = Tools.Instance.UpdateChecksum(filename, false);
            
            return new ChecksumVerificationResult
            {
                IsValid = checksumDetails.CalculationOk,
                Type = checksumDetails.TypeResult,
                TypeDescription = GetChecksumTypeDescription(checksumDetails.TypeResult),
                NumberTotal = checksumDetails.NumberChecksumsTotal,
                NumberPassed = checksumDetails.NumberChecksumsOk,
                NumberFailed = checksumDetails.NumberChecksumsFail,
                WasUpdated = true,
                StatusMessage = checksumDetails.CalculationOk
                    ? "Checksums corrected successfully"
                    : "Checksum correction failed"
            };
        }

        /// <summary>
        /// Gets a human-readable description of the checksum type.
        /// </summary>
        private string GetChecksumTypeDescription(ChecksumType type)
        {
            switch (type)
            {
                case ChecksumType.VAG_EDC15P_V41:
                    return "VAG EDC15P V4.1";
                case ChecksumType.VAG_EDC15P_V41V2:
                    return "VAG EDC15P V4.1v2";
                case ChecksumType.VAG_EDC15P_V41_2002:
                    return "VAG EDC15P V4.1 2002+";
                case ChecksumType.Unknown:
                    return "Unknown";
                default:
                    return type.ToString();
            }
        }

        /// <summary>
        /// Gets a status bar message for the checksum result.
        /// </summary>
        public string GetStatusBarMessage(ChecksumVerificationResult result)
        {
            if (result.Type == ChecksumType.Unknown)
                return "---";

            return result.IsValid
                ? $"Checksum Ok {result.TypeDescription}"
                : $"Checksum failed {result.TypeDescription}";
        }
    }
}
