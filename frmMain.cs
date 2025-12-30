//DONE: Make a codeblock sequencer.
//DONE: Make sure SOI maps are detected correctly (see screenshot of 4 axis maps issue)
//DONE: Make filelength a variable
//DONE: Axis collection, browser and editor support (refresh open maps after editing an axis)
//DONE: Bugfix, create new project causes System.UnauthorizedAccessException because of default path to Application.StartupPath
//DONE: Compare should not show infobox about missing axis
//DONE: Compare should use correct file for axis and addresses
//DONE: Add axis units to mapviewer
//DONE: Dynograph (torque(Nm) = IQ * 6
//DONE: Add support for flipping maps upside down
//DONE: Find launch control map (limiter actually) 0x4c640, corrupt axis starts with 0x80 0x00 0x00 0x0A
//DONE: Add checksum support
//DONE: axis change in compare mode?
//DONE: undo button in mapviewer (read from file?)
//DONE: EDC15V: sometimes also has three repeating smoke maps (watch the index IDs, they might be shuffled)
/*DONE: Additional information about codeblocks AG/SG indicator
G038906019HJ 1,9l R4 EDC  SG  5259 39S03217 0281010977 FFEWC400 038906019HJ 05/03 <-- Standard Getriebe codeblock 2 (normally manual)
G038906019HJ 1,9l R4 EDC  SG  5259 39S03217 0281010977 FFEWE400 038906019HJ 05/03 <-- Standard Getriebe codeblock 3 (normally 4motion)
G038906019HJ 1,9l R4 EDC  AG  5259 39S03217 0281010977 FFEWB400 038906019HJ 05/03 <-- Automat  Getriebe codeblock 1 (normally automatic)
 *                        ^                                ^
 * */
//DONE: ALSO use codeblock offset in codeblock synchronization option


//TEST: checksum issue with the ARL file (different structure), done for test!
//TEST: Userdescription for symbols + XML import/export
//HOLD: Find RPM limiter (ASZ @541A2 @641A2 and @741A2 value = 0x14B4 = 5300 rpm) <-- not very wise
//TEST: EDC15V: Don't forget the checksum for 1MB files
//TEST: EDC15V: Detect and fill codeBlocks
//HOLD: codeblocks: unreliable

//TODO: Rewrite XDF interface to TunerPro V5 XML specs (not documented...)
//TODO: Add EEPROM read/write support (K-line)
//TODO: KWP1281 support (K-line, slow init)
//TODO: Add EDC15P support ... 85%  LoHi
//TODO: Add EDC15V support ... 70%  LoHi 1MB, 512 Kb    (IMPROVE SUPPORT + CHECKSUM)
//TODO: Add EDC15M support ... 10%  LoHi (Opel?)        (CHECKSUM)
//TODO: Add EDC15C support ... 1%   LoHi                (CHECKSUM) 
//TODO: Add EDC16x support ... 5%   HiLo                (CHECKSUM)
//TODO: Add MSA15  support ... 25%  LoHi                (CHECKSUM)
//TODO: Add MSA6   support ... 0%   8-bit               (CHECKSUM) length indicators only (like ME7 and EDC16)
//TODO: Add EDC17  support ... 1%                       (CHECKSUM)
//TODO: Add major program switches (MAP/MAF switch etc)
//TODO: Add better hex-diff viewer (HexWorkShop example)
//TODO: Compressormap plotter with GT17 and most used upgrade turbo maps, boost/airmass from comperssor map
//TODO: copy from excel into mapviewer
//TODO: A2L/Damos import
//TODO: EDC15V: Don't forget the checksum for 256kB files
//TODO: Check older EDC15V-5 files.. these seem to be different
//TODO: Checksums: determine type on file open and count how many correct in stead of how many false? 12 banks in edc15v?
//TODO: Smoke limiter repeater is seen as a map (3x13) this is incorrect. Fix please vw passat bin @4DC72. 
//      (remember issue with len2Skip, we loose SOI limiter)
//TODO: make partnumber/swid editable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraBars;
using DevExpress.Skins;
using VAGSuite.Services;
using VAGSuite.Helpers;

namespace VAGSuite
{

    public delegate void DelegateStartReleaseNotePanel(string filename, string version);

    public enum GearboxType : int
    {
        Automatic,
        Manual,
        FourWheelDrive
    }

    public enum EDCFileType : int
    {
        EDC15P,
        EDC15P6, // different map structure
        EDC15V,
        EDC15M,
        EDC15C,
        EDC16,
        EDC17,  // 512Kb/2048Kb
        MSA6,
        MSA11,
        MSA12,
        MSA15,
        Unknown
    }

    public enum EngineType : int
    {
        cc1200,
        cc1400,
        cc1600,
        cc1900,
        cc2500
    }

    public enum ImportFileType : int
    {
        XML,
        A2L,
        CSV,
        AS2,
        Damos
    }

    public partial class frmMain : DevExpress.XtraEditors.XtraForm
    {
        private AppSettings m_appSettings;
        private msiupdater m_msiUpdater;
        public DelegateStartReleaseNotePanel m_DelegateStartReleaseNotePanel;
        private frmSplash splash;

        // Refactored services
        private FileOperationsManager _fileOperationsManager;
        private ChecksumService _checksumService;
        private MapViewerCoordinator _mapViewerCoordinator;
        private ImportExportService _importExportService;
        private FileComparisonService _fileComparisonService;
        private ViewSynchronizationService _viewSyncService;
        private TransactionService _transactionService;
        private MapViewerService _mapViewerService;
        private ProjectService _projectService;
        private ExportService _exportService;

        public frmMain()
        {
            try
            {
                splash = new frmSplash();
                splash.Show();
                Application.DoEvents();
            }
            catch (Exception)
            {

            }
                
            InitializeComponent();
            try
            {
                m_DelegateStartReleaseNotePanel = new DelegateStartReleaseNotePanel(this.StartReleaseNotesViewer);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }

        }

        /// <summary>
        /// Initialize services after app settings are loaded
        /// </summary>
        private void InitializeServices()
        {
            _fileOperationsManager = new FileOperationsManager(m_appSettings);
            _checksumService = new ChecksumService(m_appSettings);
            _mapViewerCoordinator = new MapViewerCoordinator(dockManager1, m_appSettings);
            _importExportService = new ImportExportService(m_appSettings);
            _fileComparisonService = new FileComparisonService(m_appSettings);
            _viewSyncService = new ViewSynchronizationService(m_appSettings);
            _transactionService = new TransactionService(m_appSettings);
            _mapViewerService = new MapViewerService(dockManager1, m_appSettings);
            _projectService = new ProjectService(m_appSettings);
            _exportService = new ExportService(m_appSettings);
        }


        private void StartReleaseNotesViewer(string xmlfilename, string version)
        {
            dockManager1.BeginUpdate();
            DockPanel dp = dockManager1.AddPanel(DockingStyle.Right);
            dp.ClosedPanel += new DockPanelEventHandler(dockPanel_ClosedPanel);
            dp.Tag = xmlfilename;
            ctrlReleaseNotes mv = new ctrlReleaseNotes();
            mv.LoadXML(xmlfilename);
            mv.Dock = DockStyle.Fill;
            dp.Width = 500;
            dp.Text = "Release notes: " + version;
            dp.Controls.Add(mv);
            dockManager1.EndUpdate();
        }

        private void btnBinaryCompare_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (Tools.Instance.m_currentfile != "")
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                //openFileDialog1.Filter = "Binaries|*.bin;*.ori";
                openFileDialog1.Multiselect = false;
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    frmBinCompare bincomp = new frmBinCompare();
                    bincomp.Symbols = Tools.Instance.m_symbols;
                    bincomp.SetCurrentFilename(Tools.Instance.m_currentfile);
                    bincomp.SetCompareFilename(openFileDialog1.FileName);
                    bincomp.CompareFiles();
                    bincomp.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("No file is currently opened, you need to open a binary file first to compare it to another one!");
            }
        }

        private void btnOpenFile_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "Binaries|*.bin;*.ori";
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CloseProject();
                m_appSettings.Lastprojectname = "";
                OpenFile(openFileDialog1.FileName, true);
                m_appSettings.LastOpenedType = 0;
            }
        }


        private void OpenFile(string fileName, bool showMessage)
        {
            // don't allow multiple instances
            lock (this)
            {
                btnOpenFile.Enabled = false;
                btnOpenProject.Enabled = false;
                try
                {
                    // Use the FileOperationsManager service
                    var result = _fileOperationsManager.OpenFile(fileName, showMessage);

                    if (!result.Success)
                    {
                        MessageBox.Show($"Failed to open file: {result.ErrorMessage}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Update UI with file information
                    barReadOnly.Caption = result.IsReadOnly ? "File is READ ONLY" : "Ok";
                    this.Text = $"VAGEDCSuite [ {Path.GetFileName(result.FileName)} ]";
                    barFilenameText.Caption = Path.GetFileName(fileName);

                    // Update grid
                    gridControl1.DataSource = null;
                    Application.DoEvents();
                    gridControl1.DataSource = result.Symbols;
                    Application.DoEvents();
                    
                    try
                    {
                        gridViewSymbols.ExpandAllGroups();
                    }
                    catch (Exception)
                    {
                    }

                    // Update status bar with file information
                    UpdateStatusBarAfterFileOpen(result, showMessage);

                    // Verify checksum
                    VerifyChecksum(fileName, !m_appSettings.AutoChecksum, false);

                    // Load additional symbol descriptions
                    TryToLoadAdditionalSymbols(fileName, ImportFileType.XML, result.Symbols, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnOpenFile.Enabled = true;
                    btnOpenProject.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Updates status bar after successfully opening a file
        /// </summary>
        private void UpdateStatusBarAfterFileOpen(FileOperationsManager.OpenFileResult result, bool showMessage)
        {
            byte[] allBytes = File.ReadAllBytes(result.FileName);
            IEDCFileParser parser = Tools.Instance.GetParserForFile(result.FileName, true);
            
            if (parser != null)
            {
                string partNo = parser.ExtractPartnumber(allBytes);
                string softwareNumber = parser.ExtractSoftwareNumber(allBytes);
                string boschnumber = parser.ExtractBoschPartnumber(allBytes);
                
                partNumberConverter pnc = new partNumberConverter();
                ECUInfo info = pnc.ConvertPartnumber(boschnumber, allBytes.Length);

                partNo = Tools.Instance.StripNonAscii(partNo);
                softwareNumber = Tools.Instance.StripNonAscii(softwareNumber);
                
                barPartnumber.Caption = $"{partNo} {softwareNumber}";
                barAdditionalInfo.Caption = $"{info.PartNumber} {info.CarMake} {info.EcuType} {parser.ExtractInfo(allBytes)}";
                barSymCount.Caption = $"{result.Symbols.Count} symbols";

                // Update launch control button
                if (_fileOperationsManager.GetMapCount("Launch control map", result.Symbols) == 0)
                {
                    btnActivateLaunchControl.Enabled = true;
                }
                else
                {
                    btnActivateLaunchControl.Enabled = false;
                }

                // Update smoke limiter button
                btnActivateSmokeLimiters.Enabled = false;
                try
                {
                    if (result.CodeBlocks.Count > 0)
                    {
                        if ((_fileOperationsManager.GetMapCount("Smoke limiter", result.Symbols) / result.CodeBlocks.Count) == 1)
                        {
                            btnActivateSmokeLimiters.Enabled = true;
                        }
                    }
                }
                catch (Exception)
                {
                }

                // Show missing maps warning if needed
                var missingMaps = _fileOperationsManager.GetMissingCriticalMaps(result.Symbols, parser);
                if (missingMaps.Count > 0 && showMessage)
                {
                    string message = string.Empty;
                    foreach (string mapName in missingMaps)
                    {
                        message += mapName + " missing" + Environment.NewLine;
                    }
                    frmInfoBox infobx = new frmInfoBox(message);
                }
            }
        }

        




        /// <summary>
        /// DEPRECATED: Use _fileOperationsManager.DetectMaps() instead.
        /// Kept for backward compatibility during refactoring.
        /// </summary>
        private SymbolCollection DetectMaps(string filename, out List<CodeBlock> newCodeBlocks,
            out List<AxisHelper> newAxisHelpers, bool showMessage, bool isPrimaryFile)
        {
            return _fileOperationsManager.DetectMaps(filename, out newCodeBlocks, out newAxisHelpers, showMessage, isPrimaryFile);
        }

        /// <summary>
        /// DEPRECATED: Use _fileOperationsManager.GetMapCount() instead.
        /// </summary>
        private int GetMapCount(string varName, SymbolCollection newSymbols)
        {
            return _fileOperationsManager.GetMapCount(varName, newSymbols);
        }

        /// <summary>
        /// DEPRECATED: Use _fileOperationsManager internally.
        /// </summary>
        private bool MapsWithNameMissing(string varName, SymbolCollection newSymbols)
        {
            foreach (SymbolHelper sh in newSymbols)
            {
                if(sh.Varname.StartsWith(varName)) return false;
            }
            return true;
        }

        
        
        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            //TODO: only if mouse on datarow?
            object o = gridViewSymbols.GetFocusedRow();
            if (o is SymbolHelper)
            {
                //SymbolHelper sh = (SymbolHelper)o;
                StartTableViewer();
            }
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            try
            {
                if (e.Column.Name == gcSymbolAddress.Name)
                {
                    if (e.CellValue != null)
                    {
                        //e.DisplayText = Convert.ToInt32(e.CellValue).ToString("X8");
                    }
                }
                else if (e.Column.Name == gcSymbolXID.Name || e.Column.Name == gcSymbolYID.Name)
                {
                }
                else if (e.Column.Name == gcSymbolLength.Name)
                {
                    if (e.CellValue != null)
                    {
                        int len = Convert.ToInt32(e.CellValue);
                        len /= 2;
                        //  e.DisplayText = len.ToString();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void StartTableViewer()
        {
            if (gridViewSymbols.SelectedRowsCount > 0)
            {
                int[] selrows = gridViewSymbols.GetSelectedRows();
                if (selrows.Length > 0)
                {
                    int row = (int)selrows.GetValue(0);
                    if (row >= 0)
                    {
                        SymbolHelper sh = (SymbolHelper)gridViewSymbols.GetRow((int)selrows.GetValue(0));
                        if (sh.Flash_start_address == 0 && sh.Start_address == 0) return;
                        
                        if (sh == null) return;
                        DevExpress.XtraBars.Docking.DockPanel dockPanel;
                        bool pnlfound = false;
                        pnlfound = CheckMapViewerActive(sh);
                        if (!pnlfound)
                        {
                            dockManager1.BeginUpdate();
                            try
                            {
                                MapViewerEx tabdet = new MapViewerEx();
                                tabdet.AutoUpdateIfSRAM = false;
                                tabdet.AutoUpdateInterval = 99999;
                                tabdet.SetViewSize(ViewSize.NormalView);
                                tabdet.Visible = false;
                                tabdet.Filename = Tools.Instance.m_currentfile;
                                tabdet.GraphVisible = true;
                                tabdet.Viewtype = m_appSettings.DefaultViewType;
                                tabdet.DisableColors = m_appSettings.DisableMapviewerColors;
                                tabdet.AutoSizeColumns = m_appSettings.AutoSizeColumnsInWindows;
                                tabdet.GraphVisible = m_appSettings.ShowGraphs;
                                tabdet.IsRedWhite = m_appSettings.ShowRedWhite;
                                tabdet.SetViewSize(m_appSettings.DefaultViewSize);
                                tabdet.Map_name = sh.Varname;
                                tabdet.Map_descr = tabdet.Map_name;
                                tabdet.Map_cat = XDFCategories.Undocumented;
                                SymbolAxesTranslator axestrans = new SymbolAxesTranslator();
                                string x_axis = string.Empty;
                                string y_axis = string.Empty;
                                string x_axis_descr = string.Empty;
                                string y_axis_descr = string.Empty;
                                string z_axis_descr = string.Empty;
                                tabdet.X_axis_name = sh.X_axis_descr;
                                tabdet.Y_axis_name = sh.Y_axis_descr;
                                tabdet.Z_axis_name = sh.Z_axis_descr;
                                tabdet.XaxisUnits = sh.XaxisUnits;
                                tabdet.YaxisUnits = sh.YaxisUnits;
                                tabdet.X_axisAddress = sh.Y_axis_address;
                                tabdet.Y_axisAddress = sh.X_axis_address;

                                tabdet.Xaxiscorrectionfactor = sh.X_axis_correction;
                                tabdet.Yaxiscorrectionfactor = sh.Y_axis_correction;
                                tabdet.Xaxiscorrectionoffset = sh.X_axis_offset;
                                tabdet.Yaxiscorrectionoffset = sh.Y_axis_offset;

                                tabdet.X_axisvalues = GetXaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, tabdet.Map_name);
                                tabdet.Y_axisvalues = GetYaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, tabdet.Map_name);


                                dockPanel = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Right);
                                int dw = 650;
                                if (tabdet.X_axisvalues.Length > 0)
                                {
                                    dw = 30 + ((tabdet.X_axisvalues.Length + 1) * 45);
                                }
                                if (dw < 400) dw = 400;
                                if (dw > 800) dw = 800;
                                dockPanel.FloatSize = new Size(dw, 900);



                                dockPanel.Tag = Tools.Instance.m_currentfile;
                                dockPanel.ClosedPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(dockPanel_ClosedPanel);

                                int columns = 8;
                                int rows = 8;
                                int tablewidth = GetTableMatrixWitdhByName(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, tabdet.Map_name, out columns, out rows);
                                int address = Convert.ToInt32(sh.Flash_start_address);
                                int sramaddress = 0;
                                if (address != 0)
                                {
                                    tabdet.Map_address = address;
                                    tabdet.Map_sramaddress = sramaddress;
                                    int length = Convert.ToInt32(sh.Length);
                                    tabdet.Map_length = length;
                                    byte[] mapdata = new byte[sh.Length];
                                    mapdata.Initialize();
                                    mapdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, address, length, Tools.Instance.m_currentFileType);
                                    tabdet.Map_content = mapdata;
                                    tabdet.Correction_factor = sh.Correction;// GetMapCorrectionFactor(tabdet.Map_name);
                                    tabdet.Correction_offset = sh.Offset;// GetMapCorrectionOffset(tabdet.Map_name);
                                    tabdet.IsUpsideDown = m_appSettings.ShowTablesUpsideDown;
                                    tabdet.ShowTable(columns, true);
                                    tabdet.Dock = DockStyle.Fill;
                                    tabdet.onSymbolSave += new VAGSuite.MapViewerEx.NotifySaveSymbol(tabdet_onSymbolSave);
                                    tabdet.onSymbolRead += new VAGSuite.MapViewerEx.NotifyReadSymbol(tabdet_onSymbolRead);
                                    tabdet.onClose += new VAGSuite.MapViewerEx.ViewerClose(tabdet_onClose);
                                    tabdet.onAxisEditorRequested += new MapViewerEx.AxisEditorRequested(tabdet_onAxisEditorRequested);

                                    tabdet.onSliderMove +=new MapViewerEx.NotifySliderMove(tabdet_onSliderMove);
                                    tabdet.onSplitterMoved +=new MapViewerEx.SplitterMoved(tabdet_onSplitterMoved);
                                    tabdet.onSelectionChanged +=new MapViewerEx.SelectionChanged(tabdet_onSelectionChanged);
                                    tabdet.onSurfaceGraphViewChangedEx += new MapViewerEx.SurfaceGraphViewChangedEx(tabdet_onSurfaceGraphViewChangedEx);
                                    tabdet.onAxisLock +=new MapViewerEx.NotifyAxisLock(tabdet_onAxisLock);
                                    tabdet.onViewTypeChanged +=new MapViewerEx.ViewTypeChanged(tabdet_onViewTypeChanged);

                                    dockPanel.Text = "Symbol: " + tabdet.Map_name + " [" + Path.GetFileName(Tools.Instance.m_currentfile) + "]";
                                    bool isDocked = false;

                                    if (!isDocked)
                                    {
                                        int width = 500;
                                        if (tabdet.X_axisvalues.Length > 0)
                                        {
                                            width = 30 + ((tabdet.X_axisvalues.Length + 1) * 45);
                                        }
                                        if (width < 500) width = 500;
                                        if (width > 800) width = 800;

                                        dockPanel.Width = width;
                                    }
                                    dockPanel.Controls.Add(tabdet);
                                }
                                else
                                {
                                    byte[] mapdata = new byte[sh.Length];
                                    mapdata.Initialize();

                                }
                                tabdet.Visible = true;
                            }
                            catch (Exception newdockE)
                            {
                                Console.WriteLine(newdockE.Message);
                            }
                            Console.WriteLine("End update");
                            dockManager1.EndUpdate();
                        }
                    }
                    System.Windows.Forms.Application.DoEvents();
                }
            }

        }

        private bool CheckMapViewerActive(SymbolHelper sh)
        {
            return _mapViewerService.CheckMapViewerActive(sh, Tools.Instance.m_currentfile);
        }

        private bool isSymbolDisplaySameAddress(SymbolHelper sh, DockPanel pnl)
        {
            return _mapViewerService.IsSymbolDisplaySameAddress(sh, pnl);
        }

        void tabdet_onAxisEditorRequested(object sender, MapViewerEx.AxisEditorRequestedEventArgs e)
        {
            // start axis editor
            foreach (SymbolHelper sh in Tools.Instance.m_symbols)
            {
                if (sh.Varname == e.Mapname)
                {
                    if (e.Axisident == MapViewerEx.AxisIdent.X_Axis) StartAxisViewer(sh, MapViewerService.Axis.XAxis);
                    else if (e.Axisident == MapViewerEx.AxisIdent.Y_Axis) StartAxisViewer(sh, MapViewerService.Axis.YAxis);

                    break;
                }
            }
        }

        void tabdet_onClose(object sender, EventArgs e)
        {
            // close the corresponding dockpanel
            if (sender is MapViewerEx)
            {
                MapViewerEx tabdet = (MapViewerEx)sender;
                string dockpanelname = "Symbol: " + tabdet.Map_name + " [" + Path.GetFileName(tabdet.Filename) + "]";
                string dockpanelname3 = "Symbol difference: " + tabdet.Map_name + " [" + Path.GetFileName(tabdet.Filename) + "]";
                foreach (DevExpress.XtraBars.Docking.DockPanel dp in dockManager1.Panels)
                {
                    if (dp.Text == dockpanelname)
                    {
                        dockManager1.RemovePanel(dp);
                        break;
                    }
                    else if (dp.Text == dockpanelname3)
                    {
                        dockManager1.RemovePanel(dp);
                        break;
                    }
                }
            }
        }

        void tabdet_onSymbolRead(object sender, MapViewerEx.ReadSymbolEventArgs e)
        {
            if (sender is MapViewerEx)
            {
                MapViewerEx mv = (MapViewerEx)sender;
                mv.Map_content = Tools.Instance.readdatafromfile(e.Filename, (int)GetSymbolAddress(Tools.Instance.m_symbols, e.SymbolName), GetSymbolLength(Tools.Instance.m_symbols, e.SymbolName), Tools.Instance.m_currentFileType);
                int cols = 0;
                int rows = 0;
                GetTableMatrixWitdhByName(e.Filename, Tools.Instance.m_symbols, e.SymbolName, out cols, out rows);
                mv.IsRAMViewer = false;
                mv.OnlineMode = false;
                mv.ShowTable(cols, true);
                mv.IsRAMViewer = false;
                mv.OnlineMode = false;
                System.Windows.Forms.Application.DoEvents();
            }
        }

        void tabdet_onSymbolSave(object sender, MapViewerEx.SaveSymbolEventArgs e)
        {
            if (sender is MapViewerEx)
            {
                // juiste filename kiezen 
                MapViewerEx tabdet = (MapViewerEx)sender;
                string note = string.Empty;
                if (m_appSettings.RequestProjectNotes && Tools.Instance.m_CurrentWorkingProject != "")
                {
                    //request a small note from the user in which he/she can denote a description of the change
                    frmChangeNote changenote = new frmChangeNote();
                    changenote.ShowDialog();
                    note = changenote.Note;
                }

                SaveDataIncludingSyncOption(e.Filename, e.SymbolName, e.SymbolAddress, e.SymbolLength, e.SymbolDate, true, note);
                
            }
        }

        private void SaveDataIncludingSyncOption(string fileName, string varName, int address, int length, byte[] data, bool useNote, string note)
        {
            // Use the FileOperationsManager service
            _fileOperationsManager.SaveDataWithSync(fileName, varName, address, length, data, useNote, note, 
                Tools.Instance.m_symbols, Tools.Instance.codeBlockList);
            
            UpdateRollbackForwardControls();
            VerifyChecksum(fileName, false, false);
        }

        private void SaveAxisDataIncludingSyncOption(int address, int length, byte[] data, string fileName, bool useNote, string note)
        {
            // Use the FileOperationsManager service
            _fileOperationsManager.SaveAxisDataWithSync(address, length, data, fileName, useNote, note, 
                Tools.Instance.m_symbols);
            
            UpdateRollbackForwardControls();
            VerifyChecksum(Tools.Instance.m_currentfile, false, false);
        }


        

        private void VerifyChecksum(string filename, bool showQuestion, bool showInfo)
        {
            // Use the ChecksumService
            var result = _checksumService.VerifyChecksum(filename, showQuestion, showInfo);

            // Update status bar
            barChecksum.Caption = _checksumService.GetStatusBarMessage(result);

            // Show dialog if needed
            if (showInfo && !string.IsNullOrEmpty(result.StatusMessage))
            {
                frmInfoBox info = new frmInfoBox(result.StatusMessage);
            }

            // If checksum is invalid and user should be asked
            if (!result.IsValid && showQuestion && result.Type != ChecksumType.Unknown && !result.WasUpdated)
            {
                frmChecksumIncorrect frmchk = new frmChecksumIncorrect();
                frmchk.ChecksumType = result.TypeDescription;
                frmchk.NumberChecksums = result.NumberTotal;
                frmchk.NumberChecksumsFailed = result.NumberFailed;
                frmchk.NumberChecksumsPassed = result.NumberPassed;

                if (frmchk.ShowDialog() == DialogResult.OK)
                {
                    var correctedResult = _checksumService.CorrectChecksum(filename);
                    barChecksum.Caption = _checksumService.GetStatusBarMessage(correctedResult);
                }
            }

            Application.DoEvents();
        }

        

        

        private double GetMapCorrectionOffset(string mapname)
        {
            return 0;
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetTableDimensions() instead.
        /// </summary>
        private int GetTableMatrixWitdhByName(string filename, SymbolCollection curSymbols, string symbolname, out int columns, out int rows)
        {
            SymbolQueryHelper.GetTableDimensions(curSymbols, symbolname, out columns, out rows);
            return columns;
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetSymbolWidth() instead.
        /// </summary>
        private int GetSymbolWidth(SymbolCollection curSymbolCollection, string symbolname)
        {
            return SymbolQueryHelper.GetSymbolWidth(curSymbolCollection, symbolname);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetSymbolHeight() instead.
        /// </summary>
        private int GetSymbolHeight(SymbolCollection curSymbolCollection, string symbolname)
        {
            return SymbolQueryHelper.GetSymbolHeight(curSymbolCollection, symbolname);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetSymbolLength() instead.
        /// </summary>
        private int GetSymbolLength(SymbolCollection curSymbolCollection, string symbolname)
        {
            return SymbolQueryHelper.GetSymbolLength(curSymbolCollection, symbolname);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetSymbolAddress() instead.
        /// </summary>
        private Int64 GetSymbolAddress(SymbolCollection curSymbolCollection, string symbolname)
        {
            return SymbolQueryHelper.GetSymbolAddress(curSymbolCollection, symbolname);
        }

        private double GetMapCorrectionFactor(string symbolname)
        {
            double returnvalue = 1;
            
            return returnvalue;
        }



        

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetYAxisValues() instead.
        /// </summary>
        private int[] GetYaxisValues(string filename, SymbolCollection curSymbols, string symbolname)
        {
            return SymbolQueryHelper.GetYAxisValues(filename, curSymbols, symbolname);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetXAxisAddress() instead.
        /// </summary>
        private int GetXAxisAddress(SymbolCollection curSymbols, string symbolname)
        {
            return SymbolQueryHelper.GetXAxisAddress(curSymbols, symbolname);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetYAxisAddress() instead.
        /// </summary>
        private int GetYAxisAddress(SymbolCollection curSymbols, string symbolname)
        {
            return SymbolQueryHelper.GetYAxisAddress(curSymbols, symbolname);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.GetXAxisValues() instead.
        /// </summary>
        private int[] GetXaxisValues(string filename, SymbolCollection curSymbols, string symbolname)
        {
            return SymbolQueryHelper.GetXAxisValues(filename, curSymbols, symbolname);
        }

        void dockPanel_ClosedPanel(object sender, DevExpress.XtraBars.Docking.DockPanelEventArgs e)
        {
            if (sender is DockPanel)
            {
                DockPanel pnl = (DockPanel)sender;

                foreach (Control c in pnl.Controls)
                {
                    if (c is HexViewer)
                    {
                        HexViewer vwr = (HexViewer)c;
                        vwr.CloseFile();
                    }
                    else if (c is DevExpress.XtraBars.Docking.DockPanel)
                    {
                        DevExpress.XtraBars.Docking.DockPanel tpnl = (DevExpress.XtraBars.Docking.DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is HexViewer)
                            {
                                HexViewer vwr2 = (HexViewer)c2;
                                vwr2.CloseFile();
                            }
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.ControlContainer)
                    {
                        DevExpress.XtraBars.Docking.ControlContainer cntr = (DevExpress.XtraBars.Docking.ControlContainer)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is HexViewer)
                            {
                                HexViewer vwr3 = (HexViewer)c3;
                                vwr3.CloseFile();
                            }
                        }
                    }
                }
                dockManager1.RemovePanel(pnl);
            }
        }

        private void btnCompareFiles_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "Binaries|*.bin;*.ori";
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string compareFile = openFileDialog1.FileName;
                CompareToFile(compareFile);

            }
        }

        private void CompareToFile(string filename)
        {
            if (Tools.Instance.m_symbols.Count > 0)
            {
                dockManager1.BeginUpdate();
                try
                {
                    DevExpress.XtraBars.Docking.DockPanel dockPanel = dockManager1.AddPanel(new System.Drawing.Point(-500, -500));
                    CompareResults tabdet = new CompareResults();
                    tabdet.ShowAddressesInHex = true;
                    tabdet.SetFilterMode(true);
                    tabdet.Dock = DockStyle.Fill;
                    tabdet.Filename = filename;
                    tabdet.onSymbolSelect += new CompareResults.NotifySelectSymbol(tabdet_onSymbolSelect);
                    dockPanel.Controls.Add(tabdet);
                    dockPanel.Text = "Compare results: " + Path.GetFileName(filename);
                    dockPanel.DockTo(dockManager1, DevExpress.XtraBars.Docking.DockingStyle.Left, 1);
                    dockPanel.Width = 500;
                    
                    // Detect maps in comparison file
                    List<CodeBlock> compare_blocks = new List<CodeBlock>();
                    List<AxisHelper> compare_axis = new List<AxisHelper>();
                    SymbolCollection compare_symbols = DetectMaps(filename, out compare_blocks, out compare_axis, false, false);
                    
                    System.Windows.Forms.Application.DoEvents();
                    Console.WriteLine("ori : " + Tools.Instance.m_symbols.Count.ToString());
                    Console.WriteLine("comp : " + compare_symbols.Count.ToString());

                    // Use FileComparisonService for comparison
                    System.Data.DataTable dt = _fileComparisonService.CompareToFile(
                        filename,
                        Tools.Instance.m_currentfile,
                        Tools.Instance.m_symbols,
                        compare_blocks,
                        dockManager1,
                        tabdet_onSymbolSelect);
                    
                    tabdet.CompareSymbolCollection = compare_symbols;
                    tabdet.OriginalSymbolCollection = Tools.Instance.m_symbols;
                    tabdet.OriginalFilename = Tools.Instance.m_currentfile;
                    tabdet.CompareFilename = filename;
                    tabdet.OpenGridViewGroups(tabdet.gridControl1, 1);
                    tabdet.gridControl1.DataSource = dt.Copy();
                }
                catch (Exception E)
                {
                    Console.WriteLine(E.Message);
                }
                dockManager1.EndUpdate();
            }
        }
        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.SymbolExists() instead.
        /// </summary>
        private bool SymbolExists(string symbolname)
        {
            return SymbolQueryHelper.SymbolExists(Tools.Instance.m_symbols, symbolname);
        }

        private void StartCompareMapViewer(string SymbolName, string Filename, int SymbolAddress, int SymbolLength, SymbolCollection curSymbols, int symbolnumber)
        {
            _mapViewerService.StartCompareMapViewer(SymbolName, Filename, SymbolAddress, SymbolLength, curSymbols, symbolnumber);
        }

        /// <summary>
        /// DEPRECATED: Use SymbolQueryHelper.FindSymbol() instead.
        /// </summary>
        private SymbolHelper FindSymbol(SymbolCollection curSymbols, string SymbolName)
        {
            return SymbolQueryHelper.FindSymbol(curSymbols, SymbolName);
        }

        void tabdet_onSymbolSelect(object sender, CompareResults.SelectSymbolEventArgs e)
        {
            
            if (!e.ShowDiffMap)
            {
                DumpDockWindows();
                if (SymbolExists(e.SymbolName))
                {
                    StartTableViewer(e.SymbolName, e.CodeBlock1);
                }
                //DumpDockWindows();
                foreach (SymbolHelper sh in e.Symbols)
                {
                    if (sh.Varname == e.SymbolName || sh.Userdescription == e.SymbolName)
                    {
                        string symName = e.SymbolName;
                        if ((e.SymbolName.StartsWith("2D") || e.SymbolName.StartsWith("3D"))  && sh.Userdescription != string.Empty) symName = sh.Userdescription;
                        StartCompareMapViewer(symName, e.Filename, e.SymbolAddress, e.SymbolLength, e.Symbols, e.Symbolnumber2);
                        break;
                    }
                }
                DumpDockWindows();
            }
            else
            {
                // show difference map
                foreach (SymbolHelper sh in e.Symbols)
                {
                    if (sh.Varname == e.SymbolName || sh.Userdescription == e.SymbolName)
                    {
                        StartCompareDifferenceViewer(sh, e.Filename, e.SymbolAddress);
                        break;
                    }
                }
                
            }
        }

        private void StartCompareDifferenceViewer(SymbolHelper sh, string Filename, int SymbolAddress)
        {
            _mapViewerService.StartCompareDifferenceViewer(sh, Filename, SymbolAddress);
        }

        private void DumpDockWindows()
        {
            // Delegate to FileComparisonService
            _fileComparisonService.DumpDockWindows(dockManager1);
        }

        
        private void StartTableViewer(string symbolname, int codeblock)
        {
            int rtel = 0;
            bool _vwrstarted = false;
            try
            {
                if (Tools.Instance.GetSymbolAddressLike(Tools.Instance.m_symbols, symbolname) > 0)
                {
                    //Console.WriteLine("Option one");
                    gridViewSymbols.ActiveFilter.Clear(); // clear filter
                    gridViewSymbols.ApplyFindFilter("");

                    SymbolCollection sc = (SymbolCollection)gridControl1.DataSource;
                    rtel = 0;
                    foreach (SymbolHelper sh in sc)
                    {
                        if (sh.Varname.StartsWith(symbolname) && sh.CodeBlock == codeblock)
                        {
                            try
                            {
                                int rhandle = gridViewSymbols.GetRowHandle(rtel);
                                gridViewSymbols.OptionsSelection.MultiSelect = true;
                                gridViewSymbols.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
                                gridViewSymbols.ClearSelection();
                                gridViewSymbols.SelectRow(rhandle);
                                //gridViewSymbols.SelectRows(rhandle, rhandle);
                                gridViewSymbols.MakeRowVisible(rhandle, true);
                                gridViewSymbols.FocusedRowHandle = rhandle;
                                //gridViewSymbols.SelectRange(rhandle, rhandle);
                                _vwrstarted = true;
                                StartTableViewer();
                                break;
                            }
                            catch (Exception E)
                            {
                                MessageBox.Show(E.Message);
                            }
                        }

                        rtel++;
                    }
                    if (!_vwrstarted)
                    {
                        rtel = 0;
                        foreach (SymbolHelper sh in sc)
                        {
                            if (sh.Userdescription.StartsWith(symbolname) && sh.CodeBlock == codeblock)
                            {
                                try
                                {
                                    int rhandle = gridViewSymbols.GetRowHandle(rtel);
                                    gridViewSymbols.OptionsSelection.MultiSelect = true;
                                    gridViewSymbols.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
                                    gridViewSymbols.ClearSelection();
                                    gridViewSymbols.SelectRow(rhandle);
                                    //gridViewSymbols.SelectRows(rhandle, rhandle);
                                    gridViewSymbols.MakeRowVisible(rhandle, true);
                                    gridViewSymbols.FocusedRowHandle = rhandle;
                                    //gridViewSymbols.SelectRange(rhandle, rhandle);
                                    _vwrstarted = true;
                                    StartTableViewer();
                                    break;
                                }
                                catch (Exception E)
                                {
                                    MessageBox.Show(E.Message);
                                }
                            }
                            rtel++;
                        }
                    }
                }
                else
                {
                    //Console.WriteLine("Option two");
                    gridViewSymbols.ActiveFilter.Clear(); // clear filter
                    SymbolCollection sc = (SymbolCollection)gridControl1.DataSource;

                    rtel = 0;
                    foreach (SymbolHelper sh in sc)
                    {
                        if (sh.Userdescription.StartsWith(symbolname) && sh.CodeBlock == codeblock)
                        {
                            try
                            {
                                int rhandle = gridViewSymbols.GetRowHandle(rtel);
                                gridViewSymbols.OptionsSelection.MultiSelect = true;
                                gridViewSymbols.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.RowSelect;
                                gridViewSymbols.ClearSelection();
                                gridViewSymbols.SelectRow(rhandle);
                                //gridViewSymbols.SelectRows(rhandle, rhandle);
                                gridViewSymbols.MakeRowVisible(rhandle, true);
                                gridViewSymbols.FocusedRowHandle = rhandle;
                                //gridViewSymbols.SelectRange(rhandle, rhandle);
                                _vwrstarted = true;
                                StartTableViewer();
                                break;
                            }
                            catch (Exception E)
                            {
                                MessageBox.Show(E.Message);
                            }
                        }
                        rtel++;
                    }
                }
            }
            catch (Exception)
            {
                frmInfoBox info = new frmInfoBox("There seems to be a problem opening this map, do you have a file opened?");
            }
        }


        private bool CompareSymbolToCurrentFile(string symbolname, int address, int length, string filename, out double diffperc, out int diffabs, out double diffavg, double correction)
        {
            diffperc = 0;
            diffabs = 0;
            diffavg = 0;

            double totalvalue1 = 0;
            double totalvalue2 = 0;
            bool retval = true;

            if (address > 0)
            {
                int curaddress = (int)GetSymbolAddress(Tools.Instance.m_symbols, symbolname);
                int curlength = GetSymbolLength(Tools.Instance.m_symbols, symbolname);
                byte[] curdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, curaddress, curlength, Tools.Instance.m_currentFileType);
                byte[] compdata = Tools.Instance.readdatafromfile(filename, address, length, Tools.Instance.m_currentFileType);
                if (curdata.Length != compdata.Length)
                {
                    Console.WriteLine("Lengths didn't match: " + symbolname);
                    return false;
                }
                for (int offset = 0; offset < curdata.Length; offset += 2)
                {
                    int ival1 = Convert.ToInt32(curdata.GetValue(offset)) * 256 + Convert.ToInt32(curdata.GetValue(offset + 1));
                    int ival2 = Convert.ToInt32(compdata.GetValue(offset)) * 256 + Convert.ToInt32(compdata.GetValue(offset + 1)) ;
                    if (ival1 != ival2)
                    {
                        retval = false;
                        diffabs++;
                    }
                    totalvalue1 += Convert.ToDouble(ival1);
                    totalvalue2 += Convert.ToDouble(ival2);
                }
                if (curdata.Length > 0)
                {
                    totalvalue1 /= (curdata.Length/2);
                    totalvalue2 /= (compdata.Length/2);
                }
            }
            diffavg = Math.Abs(totalvalue1 - totalvalue2) * correction;
            diffperc = (diffabs * 100) / (length /2);
            return retval;
        }

        private void btnTestFiles_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmBrowseFiles browse = new frmBrowseFiles();
            browse.Show();
                
            /*EDC15P_EEPROM eeprom = new EDC15P_EEPROM();
            eeprom.LoadFile(@"D:\Prive\Audi\TDI\spare eeprom\spare eeprom.bin");
            Console.WriteLine("key: " + eeprom.Key.ToString());
            Console.WriteLine("odo: " + eeprom.Mileage.ToString());
            Console.WriteLine("vin: " + eeprom.Vin);
            Console.WriteLine("immo: " + eeprom.Immo);
            Console.WriteLine("immoact: " + eeprom.ImmoActive.ToString());*/

            /*
            EDC15PTuner tuner = new EDC15PTuner();
            tuner.TuneEDC15PFile(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, 400, 170);*/

            //D:\Prive\ECU\audi\BinCollection
            //
            //ParseDirectory(@"D:\Prive\Audi");
            //ParseDirectory(@"D:\Prive\ECU\audi");
            

            /*
            if (Directory.Exists(@"D:\Prive\Audi\TDI"))
            {
                string[] files = Directory.GetFiles(@"D:\Prive\Audi\TDI", "*.bin", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    OpenFile(file);
                    bool found = false;
                    foreach (SymbolHelper sh in Tools.Instance.m_symbols)
                    {
                        if (sh.Varname.StartsWith("SVBL Boost limiter"))
                        {
                            Console.WriteLine("SVBL @" + sh.Flash_start_address.ToString("X8") + " in " + file);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        Console.WriteLine("No SVBL found in " + file);
                    }
                }
            }*/
        }

        private void ParseDirectory(string folder)
        {
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    IEDCFileParser parser = Tools.Instance.GetParserForFile(file, false);

                   
                    
                        OpenFile(file, false);
                        byte[] allBytes = File.ReadAllBytes(file);
                        string boschnumber = parser.ExtractBoschPartnumber(allBytes);
                        string partnumber = parser.ExtractPartnumber(allBytes);
                        string softwareNumber = parser.ExtractSoftwareNumber(allBytes);
                        partNumberConverter pnc = new partNumberConverter();
                        ECUInfo info = pnc.ConvertPartnumber(boschnumber, allBytes.Length);
                        UInt32 chks = AddChecksum(allBytes);
                        // determine peak trq&hp
                        if (info.EcuType.StartsWith("EDC15P"))
                        {
                            // export to the final folder
                            string destFile = Path.Combine(@"D:\Prive\ECU\audi\BinCollection\output", /*info.CarMake + "_" + info.EcuType + "_" +*/ boschnumber + "_" + softwareNumber + "_" + partnumber + "_" + chks.ToString("X8") + ".bin");
                            if (File.Exists(destFile)) Console.WriteLine("Double file: " + destFile);
                            else
                            {
                                File.Copy(file, destFile, false);
                            }
                        }
                    
                }
            }
            Console.WriteLine("Done");
        }

        private UInt32 AddChecksum(byte[] allBytes)
        {
            UInt32 retval = 0;
            foreach (byte b in allBytes)
            {
                retval += b;
            }
            return retval;
        }

        private void btnAppSettings_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            frmSettings set = new frmSettings();
            set.AppSettings = m_appSettings;
            set.UseCodeBlockSynchroniser = m_appSettings.CodeBlockSyncActive;
            set.ShowTablesUpsideDown = m_appSettings.ShowTablesUpsideDown;
            set.AutoSizeNewWindows = m_appSettings.AutoSizeNewWindows;
            set.AutoSizeColumnsInViewer = m_appSettings.AutoSizeColumnsInWindows;
            set.AutoUpdateChecksum = m_appSettings.AutoChecksum;
            set.ShowAddressesInHex = m_appSettings.ShowAddressesInHex;
            set.ShowGraphsInMapViewer = m_appSettings.ShowGraphs;
            set.UseRedAndWhiteMaps = m_appSettings.ShowRedWhite;
            set.ViewTablesInHex = m_appSettings.Viewinhex;
            set.AutoDockSameFile = m_appSettings.AutoDockSameFile;
            set.AutoDockSameSymbol = m_appSettings.AutoDockSameSymbol;
            set.DisableMapviewerColors = m_appSettings.DisableMapviewerColors;
            set.NewPanelsFloating = m_appSettings.NewPanelsFloating;
            set.AutoLoadLastFile = m_appSettings.AutoLoadLastFile;
            set.DefaultViewType = m_appSettings.DefaultViewType;
            set.DefaultViewSize = m_appSettings.DefaultViewSize;
            set.SynchronizeMapviewers = m_appSettings.SynchronizeMapviewers;
            set.SynchronizeMapviewersDifferentMaps = m_appSettings.SynchronizeMapviewersDifferentMaps;

            set.ProjectFolder = m_appSettings.ProjectFolder;
            set.RequestProjectNotes = m_appSettings.RequestProjectNotes;


            if (set.ShowDialog() == DialogResult.OK)
            {
                m_appSettings.ShowTablesUpsideDown = set.ShowTablesUpsideDown;
                m_appSettings.CodeBlockSyncActive = set.UseCodeBlockSynchroniser;
                m_appSettings.AutoSizeNewWindows = set.AutoSizeNewWindows;
                m_appSettings.AutoSizeColumnsInWindows = set.AutoSizeColumnsInViewer;
                m_appSettings.AutoChecksum = set.AutoUpdateChecksum;
                m_appSettings.ShowAddressesInHex = set.ShowAddressesInHex;
                m_appSettings.ShowGraphs = set.ShowGraphsInMapViewer;
                m_appSettings.ShowRedWhite = set.UseRedAndWhiteMaps;
                m_appSettings.Viewinhex = set.ViewTablesInHex;
                m_appSettings.DisableMapviewerColors = set.DisableMapviewerColors;
                m_appSettings.AutoDockSameFile = set.AutoDockSameFile;
                m_appSettings.AutoDockSameSymbol = set.AutoDockSameSymbol;
                m_appSettings.NewPanelsFloating = set.NewPanelsFloating;
                m_appSettings.DefaultViewType = set.DefaultViewType;
                m_appSettings.DefaultViewSize = set.DefaultViewSize;
                m_appSettings.AutoLoadLastFile = set.AutoLoadLastFile;
                m_appSettings.SynchronizeMapviewers = set.SynchronizeMapviewers;
                m_appSettings.SynchronizeMapviewersDifferentMaps = set.SynchronizeMapviewersDifferentMaps;

                m_appSettings.ProjectFolder = set.ProjectFolder;
                m_appSettings.RequestProjectNotes = set.RequestProjectNotes;

            }
            SetFilterMode();
        }

        private void SetFilterMode()
        {
            if (m_appSettings.ShowAddressesInHex)
            {
                gcSymbolAddress.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolAddress.DisplayFormat.FormatString = "X6";
                gcSymbolAddress.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText;
                gcSymbolLength.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolLength.DisplayFormat.FormatString = "X6";
                gcSymbolLength.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText;
                gcSymbolXID.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolXID.DisplayFormat.FormatString = "X4";
                gcSymbolXID.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText;
                gcSymbolYID.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolYID.DisplayFormat.FormatString = "X4";
                gcSymbolYID.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.DisplayText;
            }
            else
            {
                gcSymbolAddress.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolAddress.DisplayFormat.FormatString = "";
                gcSymbolAddress.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.Value;
                gcSymbolLength.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolLength.DisplayFormat.FormatString = "";
                gcSymbolLength.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.Value;
                gcSymbolXID.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolXID.DisplayFormat.FormatString = "";
                gcSymbolXID.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.Value;
                gcSymbolYID.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
                gcSymbolYID.DisplayFormat.FormatString = "";
                gcSymbolYID.FilterMode = DevExpress.XtraGrid.ColumnFilterMode.Value;
            }
        }

        void InitSkins()
        {
            ribbonControl1.ForceInitialize();
            //barManager1.ForceInitialize();
            BarButtonItem item;

            DevExpress.Skins.SkinManager.Default.RegisterAssembly(typeof(DevExpress.UserSkins.BonusSkins).Assembly);
            DevExpress.Skins.SkinManager.Default.RegisterAssembly(typeof(DevExpress.UserSkins.OfficeSkins).Assembly);

            foreach (DevExpress.Skins.SkinContainer cnt in DevExpress.Skins.SkinManager.Default.Skins)
            {
                item = new BarButtonItem();
                item.Caption = cnt.SkinName;
                //iPaintStyle.AddItem(item);
                rbnPageGroupSkins.ItemLinks.Add(item);
                item.ItemClick += new ItemClickEventHandler(OnSkinClick);
            }

            try
            {
                DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(m_appSettings.Skinname);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            SetToolstripTheme();
        }

        private void SetToolstripTheme()
        {
            //Console.WriteLine("Rendermode was: " + ToolStripManager.RenderMode.ToString());
            //Console.WriteLine("Visual styles: " + ToolStripManager.VisualStylesEnabled.ToString());
            //Console.WriteLine("Skinname: " + appSettings.SkinName);
            //Console.WriteLine("Backcolor: " + defaultLookAndFeel1.LookAndFeel.Painter.Button.DefaultAppearance.BackColor.ToString());
            //Console.WriteLine("Backcolor2: " + defaultLookAndFeel1.LookAndFeel.Painter.Button.DefaultAppearance.BackColor2.ToString());
            try
            {
                Skin currentSkin = CommonSkins.GetSkin(defaultLookAndFeel1.LookAndFeel);
                Color c = currentSkin.TranslateColor(SystemColors.Control);
                ToolStripManager.RenderMode = ToolStripManagerRenderMode.Professional;
                ProfColorTable profcolortable = new ProfColorTable();
                profcolortable.CustomToolstripGradientBegin = c;
                profcolortable.CustomToolstripGradientMiddle = c;
                profcolortable.CustomToolstripGradientEnd = c;
                ToolStripManager.Renderer = new ToolStripProfessionalRenderer(profcolortable);
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// OnSkinClick: Als er een skin gekozen wordt door de gebruiker voer deze
        /// dan door in de user interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnSkinClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            string skinName = e.Item.Caption;
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skinName);
            m_appSettings.Skinname = skinName;
            SetToolstripTheme();
        }

        private string GetAppDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        private void LoadLayoutFiles()
        {
            try
            {
                if (File.Exists(Path.Combine(GetAppDataPath(), "SymbolViewLayout.xml")))
                {
                    gridViewSymbols.RestoreLayoutFromXml(Path.Combine(GetAppDataPath(), "SymbolViewLayout.xml"));
                }
                if(m_appSettings.SymbolDockWidth > 2)
                {
                    dockSymbols.Width = m_appSettings.SymbolDockWidth;
                }
            }
            catch (Exception E1)
            {
                Console.WriteLine(E1.Message);
            }
        }

        private void SaveLayoutFiles()
        {
            try
            {
                m_appSettings.SymbolDockWidth = dockSymbols.Width;
                gridViewSymbols.SaveLayoutToXml(Path.Combine(GetAppDataPath(), "SymbolViewLayout.xml"));

            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                m_appSettings = new AppSettings();
                
                // Initialize refactored services
                InitializeServices();
            }
            catch (Exception)
            {

            }
            InitSkins();
            LoadLayoutFiles();
            
            if (m_appSettings.DebugMode)
            {
                btnTestFiles.Visibility = DevExpress.XtraBars.BarItemVisibility.Always;
            }
            else
            {
                btnTestFiles.Visibility = DevExpress.XtraBars.BarItemVisibility.Never;
            }
        }

        private void btnCheckForUpdates_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (m_msiUpdater != null)
                {
                    m_msiUpdater.CheckForUpdates("Global", "http://trionic.mobixs.eu/vagedcsuite/", "", "", false);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            try
            {
                if (splash != null)
                    splash.Hide();
            }
            catch (Exception)
            {

            }
            try
            {
                if (m_appSettings.AutoLoadLastFile)
                {
                    if (m_appSettings.LastOpenedType == 0)
                    {
                        if (m_appSettings.Lastfilename != "")
                        {
                            if (File.Exists(m_appSettings.Lastfilename))
                            {
                                OpenFile(m_appSettings.Lastfilename, false);
                            }
                        }
                    }
                    else if (m_appSettings.Lastprojectname != "")
                    {
                        OpenProject(m_appSettings.Lastprojectname);
                    }
                }
                SetFilterMode();
            }
            catch (Exception)
            {

            }

            try
            {
                m_msiUpdater = new msiupdater(new Version(System.Windows.Forms.Application.ProductVersion));
                m_msiUpdater.Apppath = System.Windows.Forms.Application.UserAppDataPath;
                m_msiUpdater.onDataPump += new msiupdater.DataPump(m_msiUpdater_onDataPump);
                m_msiUpdater.onUpdateProgressChanged += new msiupdater.UpdateProgressChanged(m_msiUpdater_onUpdateProgressChanged);
                m_msiUpdater.CheckForUpdates("Global", "http://trionic.mobixs.eu/vagedcsuite/", "", "", false);
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        void m_msiUpdater_onUpdateProgressChanged(msiupdater.MSIUpdateProgressEventArgs e)
        {

        }


        private void SetStatusText(string text)
        {
            barUpdateText.Caption = text;
            System.Windows.Forms.Application.DoEvents();
        }

        void m_msiUpdater_onDataPump(msiupdater.MSIUpdaterEventArgs e)
        {
            SetStatusText(e.Data);
            if (e.UpdateAvailable)
            {

                if (e.XMLFile != "" && e.Version.ToString() != "0.0")
                {
                    if (!this.IsDisposed)
                    {
                        try
                        {
                            this.Invoke(m_DelegateStartReleaseNotePanel, e.XMLFile, e.Version.ToString());
                        }
                        catch (Exception E)
                        {
                            Console.WriteLine(E.Message);
                        }
                    }
                }

                //this.Invoke(m_DelegateShowChangeLog, e.Version);
                frmUpdateAvailable frmUpdate = new frmUpdateAvailable();
                frmUpdate.SetVersionNumber(e.Version.ToString());
                if (m_msiUpdater != null)
                {
                    m_msiUpdater.Blockauto_updates = false;
                }
                if (frmUpdate.ShowDialog() == DialogResult.OK)
                {
                    if (m_msiUpdater != null)
                    {
                        m_msiUpdater.ExecuteUpdate(e.Version);
                        System.Windows.Forms.Application.Exit();
                    }
                }
                else
                {
                    // user chose "NO", don't bug him again!
                    if (m_msiUpdater != null)
                    {
                        m_msiUpdater.Blockauto_updates = false;
                    }
                }
            }
        }

        private void btnReleaseNotes_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartReleaseNotesViewer(m_msiUpdater.GetReleaseNotes(), System.Windows.Forms.Application.ProductVersion.ToString());
        }

        private void btnAbout_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmAbout about = new frmAbout();
            about.SetInformation("VAGEDCSuite v" + System.Windows.Forms.Application.ProductVersion.ToString());
            about.ShowDialog();
        }

        private void btnViewFileInHex_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartHexViewer();
        }

        private void StartHexViewer()
        {
            _mapViewerService.StartHexViewer(Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
        }

        private bool ValidateFile()
        {
            bool retval = true;
            if (File.Exists(Tools.Instance.m_currentfile))
            {
                if (Tools.Instance.m_currentfile == string.Empty)
                {
                    retval = false;
                }
                else
                {
                    FileInfo fi = new FileInfo(Tools.Instance.m_currentfile);
                    if (fi.Length != 0x80000)
                    {
                        retval = false;
                    }
                }
            }
            else
            {
                retval = false;
                Tools.Instance.m_currentfile = string.Empty;
            }
            return retval;
        }

        private void btnSearchMaps_ItemClick(object sender, ItemClickEventArgs e)
        {
            // ask the user for which value to search and if searching should include symbolnames and/or symbol description
            if (ValidateFile())
            {
                SymbolCollection result_Collection = new SymbolCollection();
                frmSearchMaps searchoptions = new frmSearchMaps();
                if (searchoptions.ShowDialog() == DialogResult.OK)
                {
                    frmProgress progress = new frmProgress();
                    progress.SetProgress("Start searching data...");
                    progress.SetProgressPercentage(0);
                    progress.Show();
                    System.Windows.Forms.Application.DoEvents();
                    int cnt = 0;
                    foreach (SymbolHelper sh in Tools.Instance.m_symbols)
                    {
                        progress.SetProgress("Searching " + sh.Varname);
                        progress.SetProgressPercentage((cnt * 100) / Tools.Instance.m_symbols.Count);
                        bool hit_found = false;
                        if (searchoptions.IncludeSymbolNames)
                        {
                            if (searchoptions.SearchForNumericValues)
                            {
                                if (sh.Varname.Contains(searchoptions.NumericValueToSearchFor.ToString()))
                                {
                                    hit_found = true;
                                }
                            }
                            if (searchoptions.SearchForStringValues)
                            {
                                if (searchoptions.StringValueToSearchFor != string.Empty)
                                {
                                    if (sh.Varname.Contains(searchoptions.StringValueToSearchFor))
                                    {
                                        hit_found = true;
                                    }
                                }
                            }
                        }
                        if (searchoptions.IncludeSymbolDescription)
                        {
                            if (searchoptions.SearchForNumericValues)
                            {
                                if (sh.Description.Contains(searchoptions.NumericValueToSearchFor.ToString()))
                                {
                                    hit_found = true;
                                }
                            }
                            if (searchoptions.SearchForStringValues)
                            {
                                if (searchoptions.StringValueToSearchFor != string.Empty)
                                {
                                    if (sh.Description.Contains(searchoptions.StringValueToSearchFor))
                                    {
                                        hit_found = true;
                                    }
                                }
                            }
                        }
                        // now search the symbol data
                        if (sh.Flash_start_address < Tools.Instance.m_currentfilelength)
                        {
                            byte[] symboldata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, (int)sh.Flash_start_address, sh.Length, Tools.Instance.m_currentFileType);
                            if (searchoptions.SearchForNumericValues)
                            {
                                for (int i = 0; i < symboldata.Length / 2; i += 2)
                                {
                                    float value = Convert.ToInt32(symboldata.GetValue(i)) * 256;
                                    value += Convert.ToInt32(symboldata.GetValue(i + 1));
                                    value *= (float)GetMapCorrectionFactor(sh.Varname);
                                    value += (float)GetMapCorrectionOffset(sh.Varname);
                                    if (value == (float)searchoptions.NumericValueToSearchFor)
                                    {
                                        hit_found = true;
                                    }
                                }
                            }
                            if (searchoptions.SearchForStringValues)
                            {
                                if (searchoptions.StringValueToSearchFor.Length > symboldata.Length)
                                {
                                    // possible...
                                    string symboldataasstring = System.Text.Encoding.ASCII.GetString(symboldata);
                                    if (symboldataasstring.Contains(searchoptions.StringValueToSearchFor))
                                    {
                                        hit_found = true;
                                    }
                                }
                            }
                        }

                        if (hit_found)
                        {
                            // add to collection
                            result_Collection.Add(sh);
                        }
                        cnt++;
                    }
                    progress.Close();
                    if (result_Collection.Count == 0)
                    {
                        frmInfoBox info = new frmInfoBox("No results found...");
                    }
                    else
                    {
                        // start result screen
                        dockManager1.BeginUpdate();
                        try
                        {
                            SymbolTranslator st = new SymbolTranslator();
                            DevExpress.XtraBars.Docking.DockPanel dockPanel = dockManager1.AddPanel(new System.Drawing.Point(-500, -500));
                            CompareResults tabdet = new CompareResults();
                            tabdet.ShowAddressesInHex = m_appSettings.ShowAddressesInHex;
                            tabdet.SetFilterMode(m_appSettings.ShowAddressesInHex);
                            tabdet.Dock = DockStyle.Fill;
                            tabdet.UseForFind = true;
                            tabdet.Filename = Tools.Instance.m_currentfile;
                            tabdet.onSymbolSelect += new CompareResults.NotifySelectSymbol(tabdet_onSymbolSelectForFind);
                            dockPanel.Controls.Add(tabdet);
                            dockPanel.Text = "Search results: " + Path.GetFileName(Tools.Instance.m_currentfile);
                            dockPanel.DockTo(dockManager1, DevExpress.XtraBars.Docking.DockingStyle.Left, 1);

                            dockPanel.Width = 500;

                            System.Data.DataTable dt = new System.Data.DataTable();
                            dt.Columns.Add("SYMBOLNAME");
                            dt.Columns.Add("SRAMADDRESS", Type.GetType("System.Int32"));
                            dt.Columns.Add("FLASHADDRESS", Type.GetType("System.Int32"));
                            dt.Columns.Add("LENGTHBYTES", Type.GetType("System.Int32"));
                            dt.Columns.Add("LENGTHVALUES", Type.GetType("System.Int32"));
                            dt.Columns.Add("DESCRIPTION");
                            dt.Columns.Add("ISCHANGED", Type.GetType("System.Boolean"));
                            dt.Columns.Add("CATEGORY"); //0
                            dt.Columns.Add("DIFFPERCENTAGE", Type.GetType("System.Double"));
                            dt.Columns.Add("DIFFABSOLUTE", Type.GetType("System.Int32"));
                            dt.Columns.Add("DIFFAVERAGE", Type.GetType("System.Double"));
                            dt.Columns.Add("CATEGORYNAME");
                            dt.Columns.Add("SUBCATEGORYNAME");
                            dt.Columns.Add("SymbolNumber1", Type.GetType("System.Int32"));
                            dt.Columns.Add("SymbolNumber2", Type.GetType("System.Int32"));
                            dt.Columns.Add("CodeBlock1", Type.GetType("System.Int32"));
                            dt.Columns.Add("CodeBlock2", Type.GetType("System.Int32"));

                            string ht = string.Empty;
                            XDFCategories cat = XDFCategories.Undocumented;
                            XDFSubCategory subcat = XDFSubCategory.Undocumented;
                            foreach (SymbolHelper shfound in result_Collection)
                            {
                                string helptext = st.TranslateSymbolToHelpText(shfound.Varname);
                                if (shfound.Varname.Contains("."))
                                {
                                    try
                                    {
                                        shfound.Category = shfound.Varname.Substring(0, shfound.Varname.IndexOf("."));
                                    }
                                    catch (Exception cE)
                                    {
                                        Console.WriteLine("Failed to assign category to symbol: " + shfound.Varname + " err: " + cE.Message);
                                    }
                                }
                                dt.Rows.Add(shfound.Varname, shfound.Start_address, shfound.Flash_start_address, shfound.Length, shfound.Length, helptext, false, 0, 0, 0, 0, shfound.Category, "", shfound.Symbol_number, shfound.Symbol_number, shfound.CodeBlock, shfound.CodeBlock);
                            }
                            tabdet.CompareSymbolCollection = result_Collection;
                            tabdet.OpenGridViewGroups(tabdet.gridControl1, 1);
                            tabdet.gridControl1.DataSource = dt.Copy();

                        }
                        catch (Exception E)
                        {
                            Console.WriteLine(E.Message);
                        }
                        dockManager1.EndUpdate();

                    }


                }
            }
        }

        void tabdet_onSymbolSelectForFind(object sender, CompareResults.SelectSymbolEventArgs e)
        {
            StartTableViewer(e.SymbolName, e.CodeBlock1);
        }

        private void btnSaveAs_ItemClick(object sender, ItemClickEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Binary files|*.bin";
            sfd.Title = "Save current file as... ";
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.Copy(Tools.Instance.m_currentfile, sfd.FileName, true);
                if (MessageBox.Show("Do you want to open the newly saved file?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    m_appSettings.Lastprojectname = "";
                    CloseProject();
                    OpenFile(sfd.FileName, true);
                    m_appSettings.LastOpenedType = 0;
                }
            }
        }

        private void CloseProject()
        {
            Tools.Instance.m_CurrentWorkingProject = string.Empty;
            Tools.Instance.m_currentfile = string.Empty;
            gridControl1.DataSource = null;
            barFilenameText.Caption = "No file";
            m_appSettings.Lastfilename = string.Empty;

            btnCloseProject.Enabled = false;
            btnShowProjectLogbook.Enabled = false;
            btnProduceLatestBinary.Enabled = false;
            btnAddNoteToProject.Enabled = false;
            btnEditProject.Enabled = false;
            btnRebuildFile.Enabled = false;
            btnRollback.Enabled = false;
            btnRollforward.Enabled = false;
            btnShowTransactionLog.Enabled = false;

            this.Text = "VAGEDCSuite";
        }


        private void OpenProject(string projectname)
        {
            // Use the ProjectService for project operations
            bool hasTransactionLog = false;
            _projectService.OpenProject(projectname, ref Tools.Instance.m_currentfile,
                ref Tools.Instance.m_ProjectTransactionLog, ref Tools.Instance.m_ProjectLog, ref hasTransactionLog);
            
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                btnCloseProject.Enabled = true;
                btnAddNoteToProject.Enabled = true;
                btnEditProject.Enabled = true;
                btnShowProjectLogbook.Enabled = true;
                btnProduceLatestBinary.Enabled = true;
                btnRebuildFile.Enabled = true;
                CreateProjectBackupFile();
                UpdateRollbackForwardControls();
                m_appSettings.Lastprojectname = Tools.Instance.m_CurrentWorkingProject;
                this.Text = "VAGEDCSuite [Project: " + projectname + "]";
            }
        }

        private void UpdateRollbackForwardControls()
        {
            bool rollbackEnabled = false;
            bool rollforwardEnabled = false;
            bool showTransactionLogEnabled = false;
            
            _transactionService.UpdateRollbackForwardControls(Tools.Instance.m_ProjectTransactionLog,
                ref rollbackEnabled, ref rollforwardEnabled, ref showTransactionLogEnabled);
            
            btnRollback.Enabled = rollbackEnabled;
            btnRollforward.Enabled = rollforwardEnabled;
            btnShowTransactionLog.Enabled = showTransactionLogEnabled;
        }

        private void CreateProjectBackupFile()
        {
            _transactionService.CreateProjectBackupFile(m_appSettings.ProjectFolder, Tools.Instance.m_CurrentWorkingProject,
                Tools.Instance.m_currentfile, Tools.Instance.m_ProjectLog);
        }


        private void LoadBinaryForProject(string projectname)
        {
            _transactionService.LoadBinaryForProject(projectname, m_appSettings.ProjectFolder, ref Tools.Instance.m_currentfile);
            if (!string.IsNullOrEmpty(Tools.Instance.m_currentfile))
            {
                OpenFile(Tools.Instance.m_currentfile, true);
            }
        }

        private string GetBinaryForProject(string projectname)
        {
            return _transactionService.GetBinaryForProject(projectname, m_appSettings.ProjectFolder, Tools.Instance.m_currentfile);
        }

        private string GetBackupOlderThanDateTime(string project, DateTime mileDT)
        {
            return _transactionService.GetBackupOlderThanDateTime(project, mileDT, m_appSettings.ProjectFolder, Tools.Instance.m_currentfile);
        }

        private void btnRebuildFile_ItemClick(object sender, ItemClickEventArgs e)
        {
            // show the transactionlog again and ask the user upto what datetime he wants to rebuild the file
            // first ask a datetime
            frmRebuildFileParameters filepar = new frmRebuildFileParameters();
            if (filepar.ShowDialog() == DialogResult.OK)
            {

                // get the last backup that is older than the selected datetime
                string file2Process = GetBackupOlderThanDateTime(Tools.Instance.m_CurrentWorkingProject, filepar.SelectedDateTime);
                // now rebuild the file
                // first create a copy of this file
                string tempRebuildFile = m_appSettings.ProjectFolder + "\\" + Tools.Instance.m_CurrentWorkingProject + "rebuild.bin";
                if (File.Exists(tempRebuildFile))
                {
                    File.Delete(tempRebuildFile);
                }
                // CREATE A BACKUP FILE HERE
                CreateProjectBackupFile();
                File.Copy(file2Process, tempRebuildFile);
                // now do all the transactions newer than this file and older than the selected date time
                FileInfo fi = new FileInfo(file2Process);
                foreach (TransactionEntry te in Tools.Instance.m_ProjectTransactionLog.TransCollection)
                {
                    if (te.EntryDateTime >= fi.LastAccessTime && te.EntryDateTime <= filepar.SelectedDateTime)
                    {
                        // apply this change
                        RollForwardOnFile(tempRebuildFile, te);
                    }
                }
                // rename/copy file
                if (filepar.UseAsNewProjectFile)
                {
                    // just delete the current file
                    File.Delete(Tools.Instance.m_currentfile);
                    File.Copy(tempRebuildFile, Tools.Instance.m_currentfile);
                    File.Delete(tempRebuildFile);
                    // done
                }
                else
                {
                    // ask for destination file
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "Save rebuild file as...";
                    sfd.Filter = "Binary files|*.bin";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                        File.Copy(tempRebuildFile, sfd.FileName);
                        File.Delete(tempRebuildFile);
                    }
                }
                if (Tools.Instance.m_CurrentWorkingProject != string.Empty)
                {
                    Tools.Instance.m_ProjectLog.WriteLogbookEntry(LogbookEntryType.ProjectFileRecreated, "Reconstruct upto " + filepar.SelectedDateTime.ToString("dd/MM/yyyy") + " selected file " + file2Process);
                }
                UpdateRollbackForwardControls();
            }
        }

        private void RollForwardOnFile(string file2Rollback, TransactionEntry entry)
        {
            _transactionService.RollForwardOnFile(file2Rollback, entry, Tools.Instance.m_currentFileType);
            VerifyChecksum(Tools.Instance.m_currentfile, false, false);
        }

        private string MakeDirName(string dirname)
        {
            string retval = dirname;
            retval = retval.Replace(@"\", "");
            retval = retval.Replace(@"/", "");
            retval = retval.Replace(@":", "");
            retval = retval.Replace(@"*", "");
            retval = retval.Replace(@"?", "");
            retval = retval.Replace(@">", "");
            retval = retval.Replace(@"<", "");
            retval = retval.Replace(@"|", "");
            return retval;
        }

        private void btnCreateAProject_ItemClick(object sender, ItemClickEventArgs e)
        {
            // show the project properties screen for the user to fill in
            // if a bin file is loaded, ask the user whether this should be the new projects binary file
            // the project XML should contain a reference to this binfile as well as a lot of other stuff
            frmProjectProperties projectprops = new frmProjectProperties();
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                projectprops.BinaryFile = Tools.Instance.m_currentfile;
                projectprops.CarModel = barPartnumber.Caption;// fileheader.getCarDescription().Trim();

                projectprops.ProjectName = DateTime.Now.ToString("yyyyMMdd") + "_" + barAdditionalInfo.Caption;// fileheader.getPartNumber().Trim() + " " + fileheader.getSoftwareVersion().Trim();
            }
            if (projectprops.ShowDialog() == DialogResult.OK)
            {
                if (!Directory.Exists(m_appSettings.ProjectFolder)) Directory.CreateDirectory(m_appSettings.ProjectFolder);
                // create a new folder with these project properties.
                // also copy the binary file into the subfolder for this project
                if (Directory.Exists(m_appSettings.ProjectFolder + "\\" + MakeDirName(projectprops.ProjectName)))
                {
                    frmInfoBox info = new frmInfoBox("The chosen projectname already exists, please choose another one");
                }
                else
                {
                    // create the project
                    Directory.CreateDirectory(m_appSettings.ProjectFolder + "\\" + MakeDirName(projectprops.ProjectName));
                    // copy the selected binary file to this folder
                    string binfilename = m_appSettings.ProjectFolder + "\\" + MakeDirName(projectprops.ProjectName) + "\\" + Path.GetFileName(projectprops.BinaryFile);
                    File.Copy(projectprops.BinaryFile, binfilename);
                    // now create the projectproperties.xml in this new folder
                    System.Data.DataTable dtProps = new System.Data.DataTable("T5PROJECT");
                    dtProps.Columns.Add("CARMAKE");
                    dtProps.Columns.Add("CARMODEL");
                    dtProps.Columns.Add("CARMY");
                    dtProps.Columns.Add("CARVIN");
                    dtProps.Columns.Add("NAME");
                    dtProps.Columns.Add("BINFILE");
                    dtProps.Columns.Add("VERSION");
                    dtProps.Rows.Add(projectprops.CarMake, projectprops.CarModel, projectprops.CarMY, projectprops.CarVIN, MakeDirName(projectprops.ProjectName), binfilename, projectprops.Version);
                    dtProps.WriteXml(m_appSettings.ProjectFolder + "\\" + MakeDirName(projectprops.ProjectName) + "\\projectproperties.xml");
                    OpenProject(projectprops.ProjectName); //?
                }
            }
        }

        private void btnOpenProject_ItemClick(object sender, ItemClickEventArgs e)
        {
            //let the user select a project from the Project folder. If none are present, let the user know
            if (!Directory.Exists(m_appSettings.ProjectFolder)) Directory.CreateDirectory(m_appSettings.ProjectFolder);
            System.Data.DataTable ValidProjects = new System.Data.DataTable();
            ValidProjects.Columns.Add("Projectname");
            ValidProjects.Columns.Add("NumberBackups");
            ValidProjects.Columns.Add("NumberTransactions");
            ValidProjects.Columns.Add("DateTimeModified");
            ValidProjects.Columns.Add("Version");
            string[] projects = Directory.GetDirectories(m_appSettings.ProjectFolder);
            // filter for folders with a projectproperties.xml file
            foreach (string project in projects)
            {
                string[] projectfiles = Directory.GetFiles(project, "projectproperties.xml");

                if (projectfiles.Length > 0)
                {
                    System.Data.DataTable projectprops = new System.Data.DataTable("T5PROJECT");
                    projectprops.Columns.Add("CARMAKE");
                    projectprops.Columns.Add("CARMODEL");
                    projectprops.Columns.Add("CARMY");
                    projectprops.Columns.Add("CARVIN");
                    projectprops.Columns.Add("NAME");
                    projectprops.Columns.Add("BINFILE");
                    projectprops.Columns.Add("VERSION");
                    projectprops.ReadXml((string)projectfiles.GetValue(0));
                    // valid project, add it to the list
                    if (projectprops.Rows.Count > 0)
                    {
                        string projectName = projectprops.Rows[0]["NAME"].ToString();
                        ValidProjects.Rows.Add(projectName, GetNumberOfBackups(projectName), GetNumberOfTransactions(projectName), GetLastAccessTime(projectprops.Rows[0]["BINFILE"].ToString()), projectprops.Rows[0]["VERSION"].ToString());
                    }
                }
            }
            if (ValidProjects.Rows.Count > 0)
            {
                frmProjectSelection projselection = new frmProjectSelection();
                projselection.SetDataSource(ValidProjects);
                if (projselection.ShowDialog() == DialogResult.OK)
                {
                    string selectedproject = projselection.GetProjectName();
                    if (selectedproject != "")
                    {
                        OpenProject(selectedproject);
                    }

                }
            }
            else
            {
                frmInfoBox info = new frmInfoBox("No projects were found, please create one first!");
            }
        }

        private int GetNumberOfBackups(string project)
        {
            return _transactionService.GetNumberOfBackups(project, m_appSettings.ProjectFolder);
        }

        private int GetNumberOfTransactions(string project)
        {
            return _transactionService.GetNumberOfTransactions(project, m_appSettings.ProjectFolder);
        }

        private DateTime GetLastAccessTime(string filename)
        {
            return _transactionService.GetLastAccessTime(filename);
        }

        private void btnCloseProject_ItemClick(object sender, ItemClickEventArgs e)
        {
            CloseProject();
            m_appSettings.Lastprojectname = "";
        }

        private void btnShowTransactionLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            // show new form
            if (Tools.Instance.m_CurrentWorkingProject != string.Empty)
            {
                frmTransactionLog translog = new frmTransactionLog();
                translog.onRollBack += new frmTransactionLog.RollBack(translog_onRollBack);
                translog.onRollForward += new frmTransactionLog.RollForward(translog_onRollForward);
                translog.onNoteChanged += new frmTransactionLog.NoteChanged(translog_onNoteChanged);
                foreach (TransactionEntry entry in Tools.Instance.m_ProjectTransactionLog.TransCollection)
                {
                    entry.SymbolName = Tools.Instance.GetSymbolNameByAddress(entry.SymbolAddress);

                }
                translog.SetTransactionLog(Tools.Instance.m_ProjectTransactionLog);
                translog.Show();
            }
        }


        void translog_onNoteChanged(object sender, frmTransactionLog.RollInformationEventArgs e)
        {
            Tools.Instance.m_ProjectTransactionLog.SetEntryNote(e.Entry);
        }

        void translog_onRollForward(object sender, frmTransactionLog.RollInformationEventArgs e)
        {
            // alter the log!
            // rollback the transaction
            // now reload the list
            RollForward(e.Entry);
            if (sender is frmTransactionLog)
            {
                frmTransactionLog logfrm = (frmTransactionLog)sender;
                if (Tools.Instance.m_ProjectTransactionLog != null)
                {
                    logfrm.SetTransactionLog(Tools.Instance.m_ProjectTransactionLog);
                }
            }
        }

        private void RollForward(TransactionEntry entry)
        {
            _transactionService.RollForward(entry, Tools.Instance.m_currentfile, Tools.Instance.m_currentFileType,
                Tools.Instance.m_ProjectTransactionLog, Tools.Instance.m_ProjectLog);
            UpdateRollbackForwardControls();
        }

        void translog_onRollBack(object sender, frmTransactionLog.RollInformationEventArgs e)
        {
            // alter the log!
            // rollback the transaction
            RollBack(e.Entry);
            // now reload the list
            if (sender is frmTransactionLog)
            {
                frmTransactionLog logfrm = (frmTransactionLog)sender;
                logfrm.SetTransactionLog(Tools.Instance.m_ProjectTransactionLog);
            }
        }


        private void RollBack(TransactionEntry entry)
        {
            _transactionService.RollBack(entry, Tools.Instance.m_currentfile, Tools.Instance.m_currentFileType,
                Tools.Instance.m_ProjectTransactionLog, Tools.Instance.m_ProjectLog);
            VerifyChecksum(Tools.Instance.m_currentfile, false, false);
            UpdateRollbackForwardControls();
        }

        private void btnRollback_ItemClick(object sender, ItemClickEventArgs e)
        {
            //roll back last entry in the log that has not been rolled back
            if (Tools.Instance.m_ProjectTransactionLog != null)
            {
                for (int t = Tools.Instance.m_ProjectTransactionLog.TransCollection.Count - 1; t >= 0; t--)
                {
                    if (!Tools.Instance.m_ProjectTransactionLog.TransCollection[t].IsRolledBack)
                    {
                        RollBack(Tools.Instance.m_ProjectTransactionLog.TransCollection[t]);

                        break;
                    }
                }
            }
        }

        private void btnRollforward_ItemClick(object sender, ItemClickEventArgs e)
        {
            //roll back last entry in the log that has not been rolled back
            if (Tools.Instance.m_ProjectTransactionLog != null)
            {
                for (int t = 0; t < Tools.Instance.m_ProjectTransactionLog.TransCollection.Count; t++)
                {
                    if (Tools.Instance.m_ProjectTransactionLog.TransCollection[t].IsRolledBack)
                    {
                        RollForward(Tools.Instance.m_ProjectTransactionLog.TransCollection[t]);

                        break;
                    }
                }
            }
        }

        private void btnRebuildFile_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            // show the transactionlog again and ask the user upto what datetime he wants to rebuild the file
            // first ask a datetime
            frmRebuildFileParameters filepar = new frmRebuildFileParameters();
            if (filepar.ShowDialog() == DialogResult.OK)
            {

                // get the last backup that is older than the selected datetime
                string file2Process = GetBackupOlderThanDateTime(Tools.Instance.m_CurrentWorkingProject, filepar.SelectedDateTime);
                // now rebuild the file
                // first create a copy of this file
                string tempRebuildFile = m_appSettings.ProjectFolder + "\\" + Tools.Instance.m_CurrentWorkingProject + "rebuild.bin";
                if (File.Exists(tempRebuildFile))
                {
                    File.Delete(tempRebuildFile);
                }
                // CREATE A BACKUP FILE HERE
                CreateProjectBackupFile();
                File.Copy(file2Process, tempRebuildFile);
                FileInfo fi = new FileInfo(file2Process);
                foreach (TransactionEntry te in Tools.Instance.m_ProjectTransactionLog.TransCollection)
                {
                    if (te.EntryDateTime >= fi.LastAccessTime && te.EntryDateTime <= filepar.SelectedDateTime)
                    {
                        // apply this change
                        RollForwardOnFile(tempRebuildFile, te);
                    }
                }
                // rename/copy file
                if (filepar.UseAsNewProjectFile)
                {
                    // just delete the current file
                    File.Delete(Tools.Instance.m_currentfile);
                    File.Copy(tempRebuildFile, Tools.Instance.m_currentfile);
                    File.Delete(tempRebuildFile);
                    // done
                }
                else
                {
                    // ask for destination file
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "Save rebuild file as...";
                    sfd.Filter = "Binary files|*.bin";
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                        File.Copy(tempRebuildFile, sfd.FileName);
                        File.Delete(tempRebuildFile);
                    }
                }
                if (Tools.Instance.m_CurrentWorkingProject != string.Empty)
                {
                    Tools.Instance.m_ProjectLog.WriteLogbookEntry(LogbookEntryType.ProjectFileRecreated, "Reconstruct upto " + filepar.SelectedDateTime.ToString("dd/MM/yyyy") + " selected file " + file2Process);
                }
                UpdateRollbackForwardControls();
            }
        }

        private void btnEditProject_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Tools.Instance.m_CurrentWorkingProject != string.Empty)
            {
                EditProjectProperties(Tools.Instance.m_CurrentWorkingProject);
            }
        }

        private void EditProjectProperties(string project)
        {
            // edit current project properties
            System.Data.DataTable projectprops = new System.Data.DataTable("T5PROJECT");
            projectprops.Columns.Add("CARMAKE");
            projectprops.Columns.Add("CARMODEL");
            projectprops.Columns.Add("CARMY");
            projectprops.Columns.Add("CARVIN");
            projectprops.Columns.Add("NAME");
            projectprops.Columns.Add("BINFILE");
            projectprops.Columns.Add("VERSION");
            projectprops.ReadXml(m_appSettings.ProjectFolder + "\\" + project + "\\projectproperties.xml");

            frmProjectProperties projectproperties = new frmProjectProperties();
            projectproperties.Version = projectprops.Rows[0]["VERSION"].ToString();
            projectproperties.ProjectName = projectprops.Rows[0]["NAME"].ToString();
            projectproperties.CarMake = projectprops.Rows[0]["CARMAKE"].ToString();
            projectproperties.CarModel = projectprops.Rows[0]["CARMODEL"].ToString();
            projectproperties.CarVIN = projectprops.Rows[0]["CARVIN"].ToString();
            projectproperties.CarMY = projectprops.Rows[0]["CARMY"].ToString();
            projectproperties.BinaryFile = projectprops.Rows[0]["BINFILE"].ToString();
            bool _reopenProject = false;
            if (projectproperties.ShowDialog() == DialogResult.OK)
            {
                // delete the original XML file
                if (project != projectproperties.ProjectName)
                {
                    Directory.Move(m_appSettings.ProjectFolder + "\\" + project, m_appSettings.ProjectFolder + "\\" + projectproperties.ProjectName);
                    project = projectproperties.ProjectName;
                    Tools.Instance.m_CurrentWorkingProject = project;
                    // set the working file to the correct folder
                    projectproperties.BinaryFile = Path.Combine(m_appSettings.ProjectFolder + "\\" + project, Path.GetFileName(projectprops.Rows[0]["BINFILE"].ToString()));
                    _reopenProject = true;
                    // open this project

                }

                File.Delete(m_appSettings.ProjectFolder + "\\" + project + "\\projectproperties.xml");
                System.Data.DataTable dtProps = new System.Data.DataTable("T5PROJECT");
                dtProps.Columns.Add("CARMAKE");
                dtProps.Columns.Add("CARMODEL");
                dtProps.Columns.Add("CARMY");
                dtProps.Columns.Add("CARVIN");
                dtProps.Columns.Add("NAME");
                dtProps.Columns.Add("BINFILE");
                dtProps.Columns.Add("VERSION");
                dtProps.Rows.Add(projectproperties.CarMake, projectproperties.CarModel, projectproperties.CarMY, projectproperties.CarVIN, MakeDirName(projectproperties.ProjectName), projectproperties.BinaryFile, projectproperties.Version);
                dtProps.WriteXml(m_appSettings.ProjectFolder + "\\" + MakeDirName(projectproperties.ProjectName) + "\\projectproperties.xml");
                if (_reopenProject)
                {
                    OpenProject(Tools.Instance.m_CurrentWorkingProject);
                }
                Tools.Instance.m_ProjectLog.WriteLogbookEntry(LogbookEntryType.PropertiesEdited, projectproperties.Version);

            }

        }

        private void btnAddNoteToProject_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmChangeNote newNote = new frmChangeNote();
            newNote.ShowDialog();
            if (newNote.Note != string.Empty)
            {
                if (Tools.Instance.m_CurrentWorkingProject != string.Empty)
                {
                    Tools.Instance.m_ProjectLog.WriteLogbookEntry(LogbookEntryType.Note, newNote.Note);
                }
            }
        }

        private void btnShowProjectLogbook_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Tools.Instance.m_CurrentWorkingProject != string.Empty)
            {
                frmProjectLogbook logb = new frmProjectLogbook();

                logb.LoadLogbookForProject(m_appSettings.ProjectFolder, Tools.Instance.m_CurrentWorkingProject);
                logb.Show();
            }
        }

        private void btnProduceLatestBinary_ItemClick(object sender, ItemClickEventArgs e)
        {
            // save binary as
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Binary files|*.bin";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // copy the current project file to the selected destination
                File.Copy(Tools.Instance.m_currentfile, sfd.FileName, true);
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveLayoutFiles();
            if (Tools.Instance.m_CurrentWorkingProject != "")
            {
                CloseProject();
            }
        }

        private void btnCreateBackup_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                VerifyChecksum(Tools.Instance.m_currentfile, false, false);

                if (File.Exists(Tools.Instance.m_currentfile))
                {
                    if (Tools.Instance.m_CurrentWorkingProject != "")
                    {
                        if (!Directory.Exists(m_appSettings.ProjectFolder + "\\" + Tools.Instance.m_CurrentWorkingProject + "\\Backups")) Directory.CreateDirectory(m_appSettings.ProjectFolder + "\\" + Tools.Instance.m_CurrentWorkingProject + "\\Backups");
                        string filename = m_appSettings.ProjectFolder + "\\" + Tools.Instance.m_CurrentWorkingProject + "\\Backups\\" + Path.GetFileNameWithoutExtension(GetBinaryForProject(Tools.Instance.m_CurrentWorkingProject)) + "-backup-" + DateTime.Now.ToString("MMddyyyyHHmmss") + ".BIN";
                        File.Copy(GetBinaryForProject(Tools.Instance.m_CurrentWorkingProject), filename);
                    }
                    else
                    {
                        File.Copy(Tools.Instance.m_currentfile, Path.GetDirectoryName(Tools.Instance.m_currentfile) + "\\" + Path.GetFileNameWithoutExtension(Tools.Instance.m_currentfile) + DateTime.Now.ToString("yyyyMMddHHmmss") + ".binarybackup", true);
                        frmInfoBox info = new frmInfoBox("Backup created: " + Path.GetDirectoryName(Tools.Instance.m_currentfile) + "\\" + Path.GetFileNameWithoutExtension(Tools.Instance.m_currentfile) + DateTime.Now.ToString("yyyyMMddHHmmss") + ".binarybackup");
                    }
                }
            }
        }

        private void btnLookupPartnumber_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmPartnumberLookup lookup = new frmPartnumberLookup();
            lookup.ShowDialog();
            if (lookup.Open_File)
            {
                string filename = lookup.GetFileToOpen();
                if (filename != string.Empty)
                {
                    CloseProject();
                    m_appSettings.Lastprojectname = "";
                    OpenFile(filename, true);
                    m_appSettings.LastOpenedType = 0;

                }
            }
            else if (lookup.Compare_File)
            {
                string filename = lookup.GetFileToOpen();
                if (filename != string.Empty)
                {

                    CompareToFile(filename);
                }
            }
            else if (lookup.CreateNewFile)
            {
                string filename = lookup.GetFileToOpen();
                if (filename != string.Empty)
                {
                    CloseProject();
                    m_appSettings.Lastprojectname = "";
                    File.Copy(filename, lookup.FileNameToSave);
                    OpenFile(lookup.FileNameToSave, true);
                    m_appSettings.LastOpenedType = 0;

                }
            }
        }

        private void btnFirmwareInformation_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                if(File.Exists(Tools.Instance.m_currentfile))
                {

                    byte[] allBytes = File.ReadAllBytes(Tools.Instance.m_currentfile);
                    IEDCFileParser parser = Tools.Instance.GetParserForFile(Tools.Instance.m_currentfile, false);
                    partNumberConverter pnc = new partNumberConverter();
                    ECUInfo ecuinfo = pnc.ConvertPartnumber(parser.ExtractBoschPartnumber(allBytes), allBytes.Length);
                    frmFirmwareInfo info = new frmFirmwareInfo();
                    info.InfoString = parser.ExtractInfo(allBytes);
                    info.partNumber = parser.ExtractBoschPartnumber(allBytes);
                    if(ecuinfo.SoftwareID == "") ecuinfo.SoftwareID = parser.ExtractPartnumber(allBytes);
                    info.SoftwareID = ecuinfo.SoftwareID + " " + parser.ExtractSoftwareNumber(allBytes);
                    info.carDetails = ecuinfo.CarMake + " " + ecuinfo.CarType;
                    string enginedetails = ecuinfo.EngineType;
                    string hpinfo = string.Empty;
                    string tqinfo = string.Empty;
                    if (ecuinfo.HP > 0) hpinfo = ecuinfo.HP.ToString() + " bhp";
                    if (ecuinfo.TQ > 0) tqinfo = ecuinfo.TQ.ToString() + " Nm";
                    if (hpinfo != string.Empty || tqinfo != string.Empty)
                    {
                        enginedetails += " (";
                        if (hpinfo != string.Empty) enginedetails += hpinfo;
                        if (hpinfo != string.Empty && tqinfo != string.Empty) enginedetails += "/";
                        if (tqinfo != string.Empty) enginedetails += tqinfo;
                        enginedetails += ")";
                    }
                    info.EngineType = /*ecuinfo.EngineType*/ enginedetails;
                    info.ecuDetails = ecuinfo.EcuType;
                    //DumpECUInfo(ecuinfo);
                    ChecksumResultDetails result = Tools.Instance.UpdateChecksum(Tools.Instance.m_currentfile, true);
                    string chkType = string.Empty;
                    if (result.TypeResult == ChecksumType.VAG_EDC15P_V41) chkType = "VAG EDC15P V4.1";
                    else if (result.TypeResult == ChecksumType.VAG_EDC15P_V41V2) chkType = "VAG EDC15P V4.1v2";
                    else if (result.TypeResult == ChecksumType.VAG_EDC15P_V41_2002) chkType = "VAG EDC15P V4.1 2002+";
                    else if (result.TypeResult != ChecksumType.Unknown) chkType = result.TypeResult.ToString();

                    chkType += " " + result.CalculationResult.ToString();

                    info.checksumType = chkType;
                    // number of codeblocks?
                    info.codeBlocks = DetermineNumberOfCodeblocks().ToString();
                    info.ShowDialog();
                }
            }
        }

        private int DetermineNumberOfCodeblocks()
        {
            List<int> blockIds = new List<int>();
            foreach (SymbolHelper sh in Tools.Instance.m_symbols)
            {
                if (!blockIds.Contains(sh.CodeBlock) && sh.CodeBlock != 0) blockIds.Add(sh.CodeBlock);
            }
            return blockIds.Count;
        }

        private void DumpECUInfo(ECUInfo ecuinfo)
        {
            Console.WriteLine("Partnr: " + ecuinfo.PartNumber);
            Console.WriteLine("Make  : " + ecuinfo.CarMake);
            Console.WriteLine("Type  : " + ecuinfo.CarType);
            Console.WriteLine("ECU   : " + ecuinfo.EcuType);
            Console.WriteLine("Engine: " + ecuinfo.EngineType);
            Console.WriteLine("SWID  : " + ecuinfo.SoftwareID);
            Console.WriteLine("HP    : " + ecuinfo.HP.ToString());
            Console.WriteLine("TQ    : " + ecuinfo.TQ.ToString());
        }

        private void btnVINDecoder_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmDecodeVIN vindec = new frmDecodeVIN();
            vindec.Show();
            //frmInfoBox info = new frmInfoBox("Not yet implemented");
        }

        private void btnChecksum_ItemClick(object sender, ItemClickEventArgs e)
        {
            
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                if (File.Exists(Tools.Instance.m_currentfile))
                {
                    VerifyChecksum(Tools.Instance.m_currentfile, true, true);
                }
            }
        }


        private void btnDriverWish_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Driver wish", 2);
        }

        private void btnTorqueLimiter_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Torque limiter", 2);
        }

        private void btnSmokeLimiter_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Smoke limiter", 2);
        }

        private void btnTargetBoost_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Boost target map", 2);
        }

        private void btnBoostPressureLimiter_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Boost limit map", 2);
        }

        private void btnBoostPressureLimitSVBL_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("SVBL Boost limiter", 2);
        }

        private void btnN75Map_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("N75 duty cycle", 2);
        }

        private void editXAxisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //
            object o = gridViewSymbols.GetFocusedRow();
            if (o is SymbolHelper)
            {
                SymbolHelper sh = (SymbolHelper)o;
                StartAxisViewer(sh, MapViewerService.Axis.XAxis);
            }
        }
        private void StartAxisViewer(SymbolHelper symbol, MapViewerService.Axis AxisToShow)
        {
            _mapViewerService.StartAxisViewer(symbol, AxisToShow, Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
        }
        
        void axis_Save(object sender, EventArgs e)
        {
            if (sender is ctrlAxisEditor)
            {
                ctrlAxisEditor editor = (ctrlAxisEditor)sender;
                // recalculate the values back and store it in the file at the correct location
                float[] newvalues = editor.GetData();
                // well.. recalculate the data based on these new values
                //editor.CorrectionFactor
                int[] iValues = new int[newvalues.Length];
                // calculate back to integer values
                for (int i = 0; i < newvalues.Length; i++)
                {
                    int iValue = Convert.ToInt32(Convert.ToDouble(newvalues.GetValue(i))/editor.CorrectionFactor);
                    iValues.SetValue(iValue, i);
                }
                byte[] barr = new byte[iValues.Length * 2];
                int bCount = 0;
                for (int i = 0; i < iValues.Length; i++)
                {
                    int iVal = (int)iValues.GetValue(i);
                    byte b1 = (byte)((iVal & 0x00FF00) / 256);
                    byte b2 = (byte)(iVal & 0x0000FF);
                    barr[bCount++] = b1;
                    barr[bCount++] = b2;
                }
                string note = string.Empty;
                if (m_appSettings.RequestProjectNotes && Tools.Instance.m_CurrentWorkingProject != "")
                {
                    //request a small note from the user in which he/she can denote a description of the change
                    frmChangeNote changenote = new frmChangeNote();
                    changenote.ShowDialog();
                    note = changenote.Note;
                }
                SaveAxisDataIncludingSyncOption(editor.AxisAddress, barr.Length, barr, Tools.Instance.m_currentfile, true, note);
                // and we need to update mapviewers maybe?
                UpdateOpenViewers(Tools.Instance.m_currentfile);
            }
        }


        private void UpdateViewer(MapViewerEx tabdet)
        {
            _mapViewerService.UpdateViewer(tabdet, Tools.Instance.m_symbols);
        }

        private void UpdateOpenViewers(string filename)
        {
            _mapViewerService.UpdateOpenViewers(filename, Tools.Instance.m_symbols);
        }

        void axis_Close(object sender, EventArgs e)
        {
            tabdet_onClose(sender, EventArgs.Empty); // recast
        }
       
        private void editYAxisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object o = gridViewSymbols.GetFocusedRow();
            if (o is SymbolHelper)
            {
                SymbolHelper sh = (SymbolHelper)o;
                StartAxisViewer(sh, MapViewerService.Axis.YAxis);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (gvhi != null)
            {
                if (gvhi.InColumnPanel || gvhi.InFilterPanel || gvhi.InGroupPanel)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (gridViewSymbols.FocusedRowHandle < 0)
            {
                e.Cancel = true;
                return;
            }
            try
            {
                object o = gridViewSymbols.GetFocusedRow();
                
                if (o is SymbolHelper)
                {
                    SymbolHelper sh = (SymbolHelper)o;
                    if (sh.X_axis_address > 0 && sh.X_axis_length > 0)
                    {
                        editXAxisToolStripMenuItem.Enabled = true;
                        editXAxisToolStripMenuItem.Text = "Edit x axis (" + sh.X_axis_descr + " " + sh.Y_axis_address.ToString("X8") + ")";
                    }
                    else
                    {
                        editXAxisToolStripMenuItem.Enabled = false;
                        editYAxisToolStripMenuItem.Text = "Edit x axis";
                    }
                    if (sh.Y_axis_address > 0 && sh.Y_axis_length > 0)
                    {
                        editYAxisToolStripMenuItem.Enabled = true;
                        editYAxisToolStripMenuItem.Text = "Edit y axis (" + sh.Y_axis_descr + " " + sh.X_axis_address.ToString("X8") + ")";
                    }
                    else
                    {
                        editYAxisToolStripMenuItem.Enabled = false;
                        editYAxisToolStripMenuItem.Text = "Edit y axis";
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void btnEGRMap_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("EGR", 2);
        }

        DevExpress.XtraGrid.Views.Grid.ViewInfo.GridHitInfo gvhi;

        private void gridControl1_MouseMove(object sender, MouseEventArgs e)
        {
            gvhi = gridViewSymbols.CalcHitInfo(new Point(e.X, e.Y));
        }

        private bool CheckAllTablesAvailable()
        {
            bool retval = true;
            if (Tools.Instance.m_currentfile != "")
            {
                if (File.Exists(Tools.Instance.m_currentfile))
                {
                    //if (MapsWithNameMissing("SVBL", Tools.Instance.m_symbols)) return false;
                    if (MapsWithNameMissing("Torque limiter", Tools.Instance.m_symbols)) return false;
                    if (MapsWithNameMissing("Smoke limiter", Tools.Instance.m_symbols)) return false;
                    if (MapsWithNameMissing("Driver wish", Tools.Instance.m_symbols)) return false;
                    //if (MapsWithNameMissing("Boost limit map", Tools.Instance.m_symbols)) return false;

                }
                else retval = false;
            }
            else retval = false;
            return retval;
        }

        private void btnAirmassResult_ItemClick(object sender, ItemClickEventArgs e)
        {
            DevExpress.XtraBars.Docking.DockPanel dockPanel;
            if (CheckAllTablesAvailable())
            {
                dockManager1.BeginUpdate();
                try
                {
                    ctrlAirmassResult airmassResult = new ctrlAirmassResult();
                    airmassResult.Dock = DockStyle.Fill;
                    dockPanel = dockManager1.AddPanel(DevExpress.XtraBars.Docking.DockingStyle.Right);
                    dockPanel.Tag = Tools.Instance.m_currentfile;
                    dockPanel.ClosedPanel += new DevExpress.XtraBars.Docking.DockPanelEventHandler(dockPanel_ClosedPanel);
                    dockPanel.Text = "Airmass result viewer: " + Path.GetFileName(Tools.Instance.m_currentfile);
                    dockPanel.Width = 800;
                    airmassResult.onStartTableViewer += new ctrlAirmassResult.StartTableViewer(airmassResult_onStartTableViewer);
                    airmassResult.onClose += new ctrlAirmassResult.ViewerClose(airmassResult_onClose);
                    airmassResult.Currentfile = Tools.Instance.m_currentfile;
                    airmassResult.Symbols = Tools.Instance.m_symbols;
                    airmassResult.Currentfile_size = Tools.Instance.m_currentfilelength;
                    IEDCFileParser parser = Tools.Instance.GetParserForFile(Tools.Instance.m_currentfile, false);
                    byte[] allBytes = File.ReadAllBytes(Tools.Instance.m_currentfile);
                    string additionalInfo = parser.ExtractInfo(allBytes);
                    //GetNumberOfCylinders 
                    string bpn = parser.ExtractBoschPartnumber(allBytes);
                    partNumberConverter pnc = new partNumberConverter();

                    ECUInfo info = pnc.ConvertPartnumber(bpn, allBytes.Length);
                    airmassResult.NumberCylinders = pnc.GetNumberOfCylinders(info.EngineType, additionalInfo);
                    airmassResult.ECUType = info.EcuType;
                   
                    
                    airmassResult.Calculate(Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
                    dockPanel.Controls.Add(airmassResult);
                }
                catch (Exception newdockE)
                {
                    Console.WriteLine(newdockE.Message);
                }
                dockManager1.EndUpdate();
            }
        }

        void airmassResult_onClose(object sender, EventArgs e)
        {
            // lookup the panel which cast this event
            if (sender is ctrlAirmassResult)
            {
                string dockpanelname = "Airmass result viewer: " + Path.GetFileName(Tools.Instance.m_currentfile);
                foreach (DevExpress.XtraBars.Docking.DockPanel dp in dockManager1.Panels)
                {
                    if (dp.Text == dockpanelname)
                    {
                        dockManager1.RemovePanel(dp);
                        break;
                    }
                }
            }
        }

        void airmassResult_onStartTableViewer(object sender, ctrlAirmassResult.StartTableViewerEventArgs e)
        {
            StartTableViewer(e.SymbolName, 2);
        }

        /// <summary>
        /// DEPRECATED: Use _exportService.StartXDFExport() instead.
        /// </summary>
        private void btnExportXDF_ItemClick(object sender, ItemClickEventArgs e)
        {
            _exportService.StartXDFExport(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Tools.Instance.m_currentfilelength);
        }

        // van t5
        void tabdet_onViewTypeChanged(object sender, MapViewerEx.ViewTypeChangedEventArgs e)
        {
            _viewSyncService.OnViewTypeChanged(sender, e, dockManager1);
        }

        void tabdet_onSurfaceGraphViewChangedEx(object sender, MapViewerEx.SurfaceGraphViewChangedEventArgsEx e)
        {
            _viewSyncService.OnSurfaceGraphViewChangedEx(sender, e, dockManager1);
        }

        void tabdet_onSplitterMoved(object sender, MapViewerEx.SplitterMovedEventArgs e)
        {
            _viewSyncService.OnSplitterMoved(sender, e, dockManager1);
        }

        void tabdet_onSelectionChanged(object sender, MapViewerEx.CellSelectionChangedEventArgs e)
        {
            _viewSyncService.OnSelectionChanged(sender, e, dockManager1);
        }

        private void SetMapSliderPosition(string filename, string symbolname, int sliderposition)
        {
            foreach (DevExpress.XtraBars.Docking.DockPanel pnl in dockManager1.Panels)
            {
                foreach (Control c in pnl.Controls)
                {
                    if (c is MapViewerEx)
                    {
                        MapViewerEx vwr = (MapViewerEx)c;
                        if (vwr.Map_name == symbolname)
                        {
                            vwr.SliderPosition = sliderposition;
                            vwr.Invalidate();
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.DockPanel)
                    {
                        DevExpress.XtraBars.Docking.DockPanel tpnl = (DevExpress.XtraBars.Docking.DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is MapViewerEx)
                            {
                                MapViewerEx vwr2 = (MapViewerEx)c2;
                                if (vwr2.Map_name == symbolname)
                                {
                                    vwr2.SliderPosition = sliderposition;
                                    vwr2.Invalidate();
                                }
                            }
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.ControlContainer)
                    {
                        DevExpress.XtraBars.Docking.ControlContainer cntr = (DevExpress.XtraBars.Docking.ControlContainer)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is MapViewerEx)
                            {
                                MapViewerEx vwr3 = (MapViewerEx)c3;
                                if (vwr3.Map_name == symbolname)
                                {
                                    vwr3.SliderPosition = sliderposition;
                                    vwr3.Invalidate();
                                }
                            }
                        }
                    }
                }
            }

        }

        void tabdet_onSliderMove(object sender, MapViewerEx.SliderMoveEventArgs e)
        {
            _viewSyncService.OnSliderMove(sender, e, dockManager1);
        }

        private void SetMapScale(string filename, string symbolname, int axismax, int lockmode)
        {
            foreach (DevExpress.XtraBars.Docking.DockPanel pnl in dockManager1.Panels)
            {
                foreach (Control c in pnl.Controls)
                {
                    if (c is MapViewerEx)
                    {
                        MapViewerEx vwr = (MapViewerEx)c;
                        if (vwr.Map_name == symbolname || m_appSettings.SynchronizeMapviewersDifferentMaps)
                        {
                            vwr.Max_y_axis_value = axismax;
                            //vwr.ReShowTable(false);
                            vwr.LockMode = lockmode;
                            vwr.Invalidate();
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.DockPanel)
                    {
                        DevExpress.XtraBars.Docking.DockPanel tpnl = (DevExpress.XtraBars.Docking.DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is MapViewerEx)
                            {
                                MapViewerEx vwr2 = (MapViewerEx)c2;
                                if (vwr2.Map_name == symbolname || m_appSettings.SynchronizeMapviewersDifferentMaps)
                                {
                                    vwr2.Max_y_axis_value = axismax;
                                    //vwr2.ReShowTable(false);
                                    vwr2.LockMode = lockmode;
                                    vwr2.Invalidate();
                                }
                            }
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.ControlContainer)
                    {
                        DevExpress.XtraBars.Docking.ControlContainer cntr = (DevExpress.XtraBars.Docking.ControlContainer)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is MapViewerEx)
                            {
                                MapViewerEx vwr3 = (MapViewerEx)c3;
                                if (vwr3.Map_name == symbolname || m_appSettings.SynchronizeMapviewersDifferentMaps)
                                {

                                    vwr3.Max_y_axis_value = axismax;
                                    vwr3.LockMode = lockmode;
                                    //vwr3.ReShowTable(false);
                                    vwr3.Invalidate();
                                }
                            }
                        }
                    }
                }
            }

        }

        private int FindMaxTableValue(string symbolname, int orgvalue)
        {
            int retval = orgvalue;
            foreach (DevExpress.XtraBars.Docking.DockPanel pnl in dockManager1.Panels)
            {
                foreach (Control c in pnl.Controls)
                {
                    if (c is MapViewerEx)
                    {
                        MapViewerEx vwr = (MapViewerEx)c;
                        if (vwr.Map_name == symbolname)
                        {
                            if (vwr.MaxValueInTable > retval) retval = vwr.MaxValueInTable;
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.DockPanel)
                    {
                        DevExpress.XtraBars.Docking.DockPanel tpnl = (DevExpress.XtraBars.Docking.DockPanel)c;
                        foreach (Control c2 in tpnl.Controls)
                        {
                            if (c2 is MapViewerEx)
                            {
                                MapViewerEx vwr2 = (MapViewerEx)c2;
                                if (vwr2.Map_name == symbolname)
                                {
                                    if (vwr2.MaxValueInTable > retval) retval = vwr2.MaxValueInTable;
                                }
                            }
                        }
                    }
                    else if (c is DevExpress.XtraBars.Docking.ControlContainer)
                    {
                        DevExpress.XtraBars.Docking.ControlContainer cntr = (DevExpress.XtraBars.Docking.ControlContainer)c;
                        foreach (Control c3 in cntr.Controls)
                        {
                            if (c3 is MapViewerEx)
                            {
                                MapViewerEx vwr3 = (MapViewerEx)c3;
                                if (vwr3.Map_name == symbolname)
                                {
                                    if (vwr3.MaxValueInTable > retval) retval = vwr3.MaxValueInTable;
                                }
                            }
                        }
                    }
                }
            }
            return retval;
        }

        void tabdet_onAxisLock(object sender, MapViewerEx.AxisLockEventArgs e)
        {
            _viewSyncService.OnAxisLock(sender, e, dockManager1);
        }

        private void btnActivateLaunchControl_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                if (File.Exists(Tools.Instance.m_currentfile))
                {
                    if (MapsWithNameMissing("Launch control map", Tools.Instance.m_symbols))
                    {
                        byte[] allBytes = File.ReadAllBytes(Tools.Instance.m_currentfile);
                        bool found = true;
                        int offset = 0;
                        while (found)
                        {
                            int LCAddress = Tools.Instance.findSequence(allBytes, offset, new byte[16] { 0xFF, 0xFF, 0x02, 0x00, 0x80, 0x00, 0x00, 0x0A, 0xFF, 0xFF, 0x02, 0x00, 0x00, 0x00, 0x70, 0x17 }, new byte[16] { 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1 });
                            if (LCAddress > 0)
                            {
                                //Console.WriteLine("Working on " + LCAddress.ToString("X8"));
                                btnActivateLaunchControl.Enabled = false;

                                byte[] saveByte = new byte[(0x0E * 2) + 2];
                                int i = 0;
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(0x0E), i++);
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(0), i++); // 1st value = 0 km/h
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(20), i++); // 2nd value = 6 km/h (6 / 0.000039 = 

                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(40), i++); // 

                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(60), i++); // 

                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(80), i++); // 

                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(100), i++); // 
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(120), i++); // 

                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(140), i++); // 
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(160), i++); // 
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(180), i++); // 
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(200), i++); // 
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(220), i++); // 
                                saveByte.SetValue(Convert.ToByte(0), i++);
                                saveByte.SetValue(Convert.ToByte(240), i++); // 
                                saveByte.SetValue(Convert.ToByte(1), i++);
                                saveByte.SetValue(Convert.ToByte(4), i++); // 

                                Tools.Instance.savedatatobinary(LCAddress + 2, saveByte.Length, saveByte, Tools.Instance.m_currentfile, false, Tools.Instance.m_currentFileType);
                                // fill the map with default values as well!
                                VerifyChecksum(Tools.Instance.m_currentfile, false, false);
                                
                                offset = LCAddress + 1;


                            }
                            else found = false;
                        }
                    }
                }
            }
            Application.DoEvents();
            //C2 02 00 xx xx xx xx xx EC 02 00 70 17*/
            if (!btnActivateLaunchControl.Enabled)
            {
                Tools.Instance.m_symbols = DetectMaps(Tools.Instance.m_currentfile, out Tools.Instance.codeBlockList, out Tools.Instance.AxisList, false, true);
                gridControl1.DataSource = null;
                Application.DoEvents();
                gridControl1.DataSource = Tools.Instance.m_symbols;
                Application.DoEvents();
                try
                {
                    gridViewSymbols.ExpandAllGroups();
                }
                catch (Exception)
                {

                }
                Application.DoEvents();

            }

        }

        private void btnEditEEProm_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "Binary files|*.bin";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // check size .. should be 4kb
                FileInfo fi = new FileInfo(ofd.FileName);
                if (fi.Length == 512)
                {
                    frmEEPromEditor editor = new frmEEPromEditor();
                    editor.LoadFile(ofd.FileName);
                    editor.ShowDialog();
                }
            }
        }

        private void gridViewSymbols_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (gridViewSymbols.FocusedColumn.Name == gcSymbolUserdescription.Name)
                {
                    SaveAdditionalSymbols();
                }
                else
                {
                    // start the selected row
                    try
                    {
                        int[] selectedrows = gridViewSymbols.GetSelectedRows();
                        int grouplevel = gridViewSymbols.GetRowLevel((int)selectedrows.GetValue(0));
                        if (grouplevel >= gridViewSymbols.GroupCount)
                        {
                            if (gridViewSymbols.GetFocusedRow() is SymbolHelper)
                            {
                                SymbolHelper sh = (SymbolHelper)gridViewSymbols.GetFocusedRow();
                                StartTableViewer(sh.Varname, sh.CodeBlock);
                                //StartTableViewer();
                            }
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.Message);
                    }
                }

            }
        }

        private void btnMergeFiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmBinmerger frmmerger = new frmBinmerger();
            frmmerger.ShowDialog();
        }

        private void btnSplitFiles_ItemClick(object sender, ItemClickEventArgs e)
        {

            if (Tools.Instance.m_currentfile != "")
            {
                if (File.Exists(Tools.Instance.m_currentfile))
                {
                    string path = Path.GetDirectoryName(Tools.Instance.m_currentfile);
                    FileInfo fi = new FileInfo(Tools.Instance.m_currentfile);
                    FileStream fs = File.Create(path + "\\chip2.bin");
                    BinaryWriter bw = new BinaryWriter(fs);
                    FileStream fs2 = File.Create(path + "\\chip1.bin");
                    BinaryWriter bw2 = new BinaryWriter(fs2);
                    FileStream fsi1 = File.OpenRead(Tools.Instance.m_currentfile);
                    BinaryReader br1 = new BinaryReader(fsi1);
                    bool toggle = false;
                    for (int tel = 0; tel < fi.Length; tel++)
                    {
                        Byte ib1 = br1.ReadByte();
                        if (!toggle)
                        {
                            toggle = true;
                            bw.Write(ib1);
                        }
                        else
                        {
                            toggle = false;
                            bw2.Write(ib1);
                        }
                    }
                    bw.Close();
                    bw2.Close();
                    fs.Close();
                    fs2.Close();
                    fsi1.Close();
                    br1.Close();
                    MessageBox.Show("File split to chip1.bin and chip2.bin");
                }
            }
        }

        private void btnBuildLibrary_ItemClick(object sender, ItemClickEventArgs e)
        {
            frmBrowseFiles browse = new frmBrowseFiles();
            browse.Show();
        }

        private void StartPDFFile(string file, string errormessage)
        {
            try
            {
                if (File.Exists(file))
                {
                    System.Diagnostics.Process.Start(file);
                }
                else
                {
                    MessageBox.Show(errormessage);
                }
            }
            catch (Exception E2)
            {
                Console.WriteLine(E2.Message);
            }
        }

        private void btnUserManual_ItemClick(object sender, ItemClickEventArgs e)
        {
            // start user manual PDF file
            StartPDFFile(Path.Combine(System.Windows.Forms.Application.StartupPath, "EDC15PSuite manual.pdf"), "EDC15P user manual could not be found or opened!");
            
        }
        private void btnEDC15PDocumentation_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartPDFFile(Path.Combine(System.Windows.Forms.Application.StartupPath, "VAG EDC15P.pdf"), "EDC15P documentation could not be found or opened!");
        }

        

        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportDescriptorFile(ImportFileType.XML);
        }

        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportDescriptorFile(ImportFileType.A2L);
        }

        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportDescriptorFile(ImportFileType.CSV);
        }

        private void barButtonItem4_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportDescriptorFile(ImportFileType.AS2);
        }

        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportDescriptorFile(ImportFileType.Damos);
        }

        private void ImportDescriptorFile(ImportFileType importFileType)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "AS2 documents|*.as2";
            if (importFileType == ImportFileType.A2L) ofd.Filter = "A2L documents|*.a2l";
            else if (importFileType == ImportFileType.CSV) ofd.Filter = "CSV documents|*.csv";
            else if (importFileType == ImportFileType.Damos) ofd.Filter = "Damos documents|*.dam";
            else if (importFileType == ImportFileType.XML) ofd.Filter = "XML documents|*.xml";
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                TryToLoadAdditionalSymbols(ofd.FileName, importFileType, Tools.Instance.m_symbols, false);
                gridControl1.DataSource = Tools.Instance.m_symbols;
                gridControl1.RefreshDataSource();
                SaveAdditionalSymbols();
            }
        }




        private void SaveAdditionalSymbols()
        {
            _importExportService.SaveAdditionalSymbols(Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
        }

        /// <summary>
        /// DEPRECATED: Use _importExportService.SaveAdditionalSymbols() instead.
        /// Kept for backward compatibility during refactoring.
        /// </summary>
        private void SaveAdditionalSymbols_OLD()
        {
            _importExportService.SaveAdditionalSymbols(Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
        }

        private void TryToLoadAdditionalSymbols(string filename, ImportFileType importFileType, SymbolCollection symbolCollection, bool fromRepository)
        {
            _importExportService.TryToLoadAdditionalSymbols(filename, importFileType, symbolCollection, fromRepository);
        }

        /// <summary>
        /// DEPRECATED: Use _importExportService.TryToLoadAdditionalSymbols() instead.
        /// </summary>
        private void TryToLoadAdditionalSymbols_OLD(string filename, ImportFileType importFileType, SymbolCollection symbolCollection, bool fromRepository)
        {
            _importExportService.TryToLoadAdditionalSymbols(filename, importFileType, symbolCollection, fromRepository);
        }

        private void TryToLoadAdditionalCSVSymbols(string filename, SymbolCollection coll2load)
        {
            _importExportService.TryToLoadAdditionalCSVSymbols(filename, coll2load);
        }

        /// <summary>
        /// DEPRECATED: Use _importExportService.TryToLoadAdditionalCSVSymbols() instead.
        /// </summary>
        private void TryToLoadAdditionalCSVSymbols_OLD(string filename, SymbolCollection coll2load)
        {
            _importExportService.TryToLoadAdditionalCSVSymbols(filename, coll2load);
        }

        private void TryToLoadAdditionalAS2Symbols(string filename, SymbolCollection coll2load)
        {
            _importExportService.TryToLoadAdditionalAS2Symbols(filename, coll2load);
        }

        /// <summary>
        /// DEPRECATED: Use _importExportService.TryToLoadAdditionalAS2Symbols() instead.
        /// </summary>
        private void TryToLoadAdditionalAS2Symbols_OLD(string filename, SymbolCollection coll2load)
        {
            _importExportService.TryToLoadAdditionalAS2Symbols(filename, coll2load);
        }

        private bool ImportXMLFile(string filename, SymbolCollection coll2load, bool ImportFromRepository)
        {
            return _importExportService.ImportXMLFile(filename, coll2load, ImportFromRepository);
        }

        /// <summary>
        /// DEPRECATED: Use _importExportService.ImportXMLFile() instead.
        /// </summary>
        private bool ImportXMLFile_OLD(string filename, SymbolCollection coll2load, bool ImportFromRepository)
        {
            return _importExportService.ImportXMLFile(filename, coll2load, ImportFromRepository);
        }

        private string GetFileDescriptionFromFile(string file)
        {
            return _importExportService.GetFileDescriptionFromFile(file);
        }

        /// <summary>
        /// DEPRECATED: Use _importExportService.GetFileDescriptionFromFile() instead.
        /// </summary>
        private string GetFileDescriptionFromFile_OLD(string file)
        {
            return _importExportService.GetFileDescriptionFromFile(file);
        }

        private void gridViewSymbols_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {

            if (e.Column.Name == gcSymbolUserdescription.Name)
            {
                _importExportService.SaveAdditionalSymbols(Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
            }
        }

        private void dockManager1_LayoutUpgrade(object sender, DevExpress.Utils.LayoutUpgadeEventArgs e)
        {

        }

        private void btnActivateSmokeLimiters_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Tools.Instance.m_currentfile != string.Empty)
            {
                if (File.Exists(Tools.Instance.m_currentfile))
                {
                    btnActivateSmokeLimiters.Enabled = false;
                    // find the smoke limiter control map (selector)
                    foreach (SymbolHelper sh in Tools.Instance.m_symbols)
                    {
                        if (sh.Varname.StartsWith("Smoke limiter"))
                        {
                            byte[] mapdata = new byte[sh.Length];
                            mapdata.Initialize();
                            mapdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, (int)sh.Flash_start_address, sh.Length, Tools.Instance.m_currentFileType);

                            int selectorAddress = sh.MapSelector.StartAddress;
                            if (selectorAddress > 0)
                            {
                                byte[] mapIndexes = new byte[sh.MapSelector.MapIndexes.Length * 2];
                                int bIdx = 0;
                                for (int i = 0; i < sh.MapSelector.MapIndexes.Length; i++)
                                {
                                    mapIndexes[bIdx++] = Convert.ToByte(i);
                                    mapIndexes[bIdx++] = 0;
                                }
                                Tools.Instance.savedatatobinary(selectorAddress + mapIndexes.Length, mapIndexes.Length, mapIndexes, Tools.Instance.m_currentfile, false, Tools.Instance.m_currentFileType);
                            }
                            for (int i = 1; i < sh.MapSelector.MapIndexes.Length; i++)
                            {
                                // save the map data (copy)
                                int saveAddress = (int)sh.Flash_start_address + i * sh.Length;
                                Tools.Instance.savedatatobinary(saveAddress, sh.Length, mapdata, Tools.Instance.m_currentfile, false, Tools.Instance.m_currentFileType);
                            }
                        }
                    }

                    VerifyChecksum(Tools.Instance.m_currentfile, false, false);
                }
            }
            Application.DoEvents();

            if (!btnActivateSmokeLimiters.Enabled)
            {
                Tools.Instance.m_symbols = DetectMaps(Tools.Instance.m_currentfile, out Tools.Instance.codeBlockList, out Tools.Instance.AxisList, false, true);
                gridControl1.DataSource = null;
                Application.DoEvents();
                gridControl1.DataSource = Tools.Instance.m_symbols;
                Application.DoEvents();
                try
                {

                    gridViewSymbols.ExpandAllGroups();
                }
                catch (Exception)
                {

                }
                Application.DoEvents();

            }
        }

        private void ImportFileInExcelFormat()
        {
            _exportService.ImportFileInExcelFormat(Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
        }

        private void ImportExcelSymbol(string symbolname, string filename)
        {
            _exportService.ImportExcelSymbol(symbolname, filename, Tools.Instance.m_currentfile, Tools.Instance.m_symbols);
        }

        private void StartExcelExport()
        {
            if (gridViewSymbols.SelectedRowsCount > 0)
            {
                int[] selrows = gridViewSymbols.GetSelectedRows();
                if (selrows.Length > 0)
                {
                    SymbolHelper sh = (SymbolHelper)gridViewSymbols.GetRow((int)selrows.GetValue(0));
                    string Map_name = sh.Varname;
                    if ((Map_name.StartsWith("2D") || Map_name.StartsWith("3D")) && sh.Userdescription != "") Map_name = sh.Userdescription;
                    
                    int columns = 8;
                    int rows = 8;
                    GetTableMatrixWitdhByName(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name, out columns, out rows);
                    int address = (int)sh.Flash_start_address;
                    
                    if (address != 0)
                    {
                        int length = sh.Length;
                        byte[] mapdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, address, length, Tools.Instance.m_currentFileType);
                        int[] xaxis = GetXaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name);
                        int[] yaxis = GetYaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name);
                        
                        _exportService.StartExcelExport(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name, address, length, mapdata, columns, rows, xaxis, yaxis, sh);
                    }
                }
            }
        }

        private void StartCSVExport()
        {
            if (gridViewSymbols.SelectedRowsCount > 0)
            {
                int[] selrows = gridViewSymbols.GetSelectedRows();
                if (selrows.Length > 0)
                {
                    SymbolHelper sh = (SymbolHelper)gridViewSymbols.GetRow((int)selrows.GetValue(0));
                    string Map_name = sh.Varname;
                    if ((Map_name.StartsWith("2D") || Map_name.StartsWith("3D")) && sh.Userdescription != "") Map_name = sh.Userdescription;
                    
                    int columns = 8;
                    int rows = 8;
                    GetTableMatrixWitdhByName(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name, out columns, out rows);
                    int address = (int)sh.Flash_start_address;
                    
                    if (address != 0)
                    {
                        int length = sh.Length;
                        byte[] mapdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, address, length, Tools.Instance.m_currentFileType);
                        int[] xaxis = GetXaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name);
                        int[] yaxis = GetYaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name);
                        
                        _exportService.StartCSVExport(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name, address, length, mapdata, columns, rows, xaxis, yaxis, sh);
                    }
                }
            }
        }

        private void StartXMLExport()
        {
            if (gridViewSymbols.SelectedRowsCount > 0)
            {
                int[] selrows = gridViewSymbols.GetSelectedRows();
                if (selrows.Length > 0)
                {
                    SymbolHelper sh = (SymbolHelper)gridViewSymbols.GetRow((int)selrows.GetValue(0));
                    string Map_name = sh.Varname;
                    if ((Map_name.StartsWith("2D") || Map_name.StartsWith("3D")) && sh.Userdescription != "") Map_name = sh.Userdescription;
                    
                    int columns = 8;
                    int rows = 8;
                    GetTableMatrixWitdhByName(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name, out columns, out rows);
                    int address = (int)sh.Flash_start_address;
                    
                    if (address != 0)
                    {
                        int length = sh.Length;
                        byte[] mapdata = Tools.Instance.readdatafromfile(Tools.Instance.m_currentfile, address, length, Tools.Instance.m_currentFileType);
                        int[] xaxis = GetXaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name);
                        int[] yaxis = GetYaxisValues(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name);
                        
                        _exportService.StartXMLExport(Tools.Instance.m_currentfile, Tools.Instance.m_symbols, Map_name, address, length, mapdata, columns, rows, xaxis, yaxis, sh);
                    }
                }
            }
        }

        private void btnExportToExcel_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartExcelExport();
        }

        private void btnExcelImport_ItemClick(object sender, ItemClickEventArgs e)
        {
            ImportFileInExcelFormat();
        }

        private void exportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartExcelExport();
        }

        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartCSVExport();
        }

        private void exportToXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartXMLExport();
        }

        private void btnIQByMap_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("IQ by MAP", 2);
        }

        private void btnIQByMAF_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("IQ by MAF", 2);
        }

        private void btnSOILimiter_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("SOI limiter", 2);
        }

        private void btnStartOfInjection_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Start of injection", 2);
        }

        private void btnInjectorDuration_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Injector duration", 2);
        }

        private void btnStartIQ_ItemClick(object sender, ItemClickEventArgs e)
        {
            StartTableViewer("Start IQ", 2);
        }

    }
}
