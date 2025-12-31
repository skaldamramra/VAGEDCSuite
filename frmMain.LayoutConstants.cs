namespace VAGSuite
{
    public partial class frmMain
    {
        /// <summary>
        /// Layout constants for designer initialization
        /// </summary>
        private static class LayoutConstants
        {
            // Form dimensions
            public const int DefaultFormWidth = 1168;
            public const int DefaultFormHeight = 517;

            // Ribbon dimensions
            public const int RibbonHeight = 142;
            public const int StatusBarHeight = 28;

            // Docking panel dimensions
            public const int SymbolDockPanelWidth = 527;
            public const int SymbolDockPanelHeight = 200;

            // Grid column widths
            public const int SymbolNameColumnWidth = 150;
            public const int SymbolAddressColumnWidth = 64;
            public const int SymbolDescriptionColumnWidth = 165;
            public const int SymbolUserDescriptionWidth = 138;

            // Context menu dimensions
            public const int ContextMenuWidth = 159;
            public const int ContextMenuHeight = 116;
        }

        /// <summary>
        /// Control IDs for button items
        /// </summary>
        private static class ControlIds
        {
            // File operations
            public const int OpenFile = 2;
            public const int SaveAs = 22;
            public const int CreateBackup = 35;
            public const int ExportXDF = 47;

            // Project operations
            public const int CreateProject = 23;
            public const int OpenProject = 24;
            public const int CloseProject = 25;

            // Actions
            public const int Checksum = 17;
            public const int BinaryCompare = 1;
            public const int CompareFiles = 3;

            // Tuning - Injection
            public const int DriverWish = 37;
            public const int TorqueLimiter = 38;
            public const int SmokeLimiter = 39;
            public const int IQByMap = 64;
            public const int IQByMAF = 65;
            public const int SOILimiter = 66;
            public const int StartOfInjection = 67;
            public const int InjectorDuration = 68;
            public const int StartIQ = 69;
            public const int BIPBasicCharacteristic = 70;
            public const int PIDMapP = 71;
            public const int PIDMapI = 72;
            public const int PIDMapD = 73;
            public const int DurationLimiter = 74;
            public const int MAFLimiter = 75;
            public const int MAPLimiter = 76;

            // Tuning - Turbo
            public const int TargetBoost = 40;
            public const int BoostPressureLimiter = 41;
            public const int BoostPressureLimitSVBL = 42;
            public const int N75Map = 43;
            public const int EGRMap = 44;
            public const int MAFLinearization = 77;
            public const int MAPLinearization = 78;

            // Tools
            public const int ViewFileInHex = 20;
            public const int SearchMaps = 21;
            public const int AirmassResult = 45;
            public const int ActivateLaunchControl = 48;
            public const int ActivateSmokeLimiters = 61;
            public const int EditEEProm = 49;
            public const int MergeFiles = 51;
            public const int SplitFiles = 52;
            public const int ExportToExcel = 62;
            public const int ExcelImport = 63;

            // Settings
            public const int AppSettings = 11;
            public const int LookupPartnumber = 36;

            // Help
            public const int CheckForUpdates = 12;
            public const int ReleaseNotes = 14;
            public const int EDC15PDocumentation = 15;
            public const int UserManual = 54;
            public const int About = 16;

            // Status bar
            public const int FilenameText = 34;
            public const int ChecksumStatus = 50;
            public const int ReadOnly = 46;
            public const int Partnumber = 4;
            public const int AdditionalInfo = 5;
            public const int CodeBlock1 = 6;
            public const int CodeBlock2 = 7;
            public const int CodeBlock3 = 8;
            public const int SymCount = 9;
            public const int UpdateText = 13;

            // VCDS Diagnostic
            public const int VCDSDiagStart = 79;
        }
    }
}