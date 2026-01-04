using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using ComponentFactory.Krypton.Navigator;
using Zuby.ADGV;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using VAGSuite.MapViewerEventArgs;
using VAGSuite.Models;
using VAGSuite.Services;
using VAGSuite.Components;
using VAGSuite.Theming;

namespace VAGSuite
{
    public partial class MapViewerEx : System.Windows.Forms.UserControl //IMapViewer
    {
        // Phase 3 Services - Business Logic (initialized in constructor for backward compatibility)
        private IMapRenderingService _mapRenderingService;
        private IChartService _chartService;
        private IClipboardService _clipboardService;
        private ISmoothingService _smoothingService;
        private IDataConversionService _dataConversionService;
        private IMapOperationService _mapOperationService;
        private IMapValidationService _mapValidationService;
        private MapViewerController _controller;
        
        // Phase 5 Components - UI Component instances
        private MapGridComponent _mapGridComponent;
        private Chart3DComponent _chart3DComponent;
        private Chart2DComponent _chart2DComponent;

        private bool m_isDifferenceViewer = false;

        public bool IsDifferenceViewer
        {
            get { return m_isDifferenceViewer; }
            set
            {
                m_isDifferenceViewer = value;
                if (m_isDifferenceViewer)
                {
                    // disable buttons etc
                    simpleButton10.Visible = false;
                    simpleButton2.Visible = false;
                }
            }
        }

        public delegate void ViewerClose(object sender, EventArgs e);
        public event ViewerClose onClose;
        public delegate void AxisEditorRequested(object sender, AxisEditorRequestedEventArgs e);
        public event MapViewerEx.AxisEditorRequested onAxisEditorRequested;
        public delegate void ReadDataFromSRAM(object sender, ReadFromSRAMEventArgs e);
        public event MapViewerEx.ReadDataFromSRAM onReadFromSRAM;
        public delegate void WriteDataToSRAM(object sender, WriteToSRAMEventArgs e);
        public event MapViewerEx.WriteDataToSRAM onWriteToSRAM;
        public delegate void ViewTypeChanged(object sender, ViewTypeChangedEventArgs e);
        public event MapViewerEx.ViewTypeChanged onViewTypeChanged;
        public delegate void GraphSelectionChanged(object sender, GraphSelectionChangedEventArgs e);
        public event MapViewerEx.GraphSelectionChanged onGraphSelectionChanged;
        public delegate void SurfaceGraphViewChanged(object sender, SurfaceGraphViewChangedEventArgs e);
        public event MapViewerEx.SurfaceGraphViewChanged onSurfaceGraphViewChanged;
        public delegate void SurfaceGraphViewChangedEx(object sender, SurfaceGraphViewChangedEventArgsEx e);
        public event MapViewerEx.SurfaceGraphViewChangedEx onSurfaceGraphViewChangedEx;
        public delegate void NotifySaveSymbol(object sender, SaveSymbolEventArgs e);
        public event MapViewerEx.NotifySaveSymbol onSymbolSave;
        public delegate void NotifyReadSymbol(object sender, ReadSymbolEventArgs e);
        public event MapViewerEx.NotifyReadSymbol onSymbolRead;
        public delegate void SplitterMoved(object sender, SplitterMovedEventArgs e);
        public event MapViewerEx.SplitterMoved onSplitterMoved;
        public delegate void SelectionChanged(object sender, CellSelectionChangedEventArgs e);
        public event MapViewerEx.SelectionChanged onSelectionChanged;
        public delegate void NotifyAxisLock(object sender, AxisLockEventArgs e);
        public event MapViewerEx.NotifyAxisLock onAxisLock;
        public delegate void NotifySliderMove(object sender, SliderMoveEventArgs e);
        public event MapViewerEx.NotifySliderMove onSliderMove;
        
        private bool m_issixteenbit = false;
        private int m_TableWidth = 8;
        private bool m_datasourceMutated = false;
        private int m_MaxValueInTable = 0;
        private bool m_prohibitcellchange = false;
        private bool m_prohibitsplitchange = false;
        private bool m_prohibitgraphchange = false;
        private ViewType m_viewtype = ViewType.Hexadecimal;
        private ViewType m_previousviewtype = ViewType.Easy;
        private bool m_prohibit_viewchange = false;
        private bool m_trackbarBlocked = true;
        private ViewSize m_vs = ViewSize.NormalView;
        private bool m_OverlayVisible = true;
        private double m_realMaxValue = -65535;
        private double m_realMinValue = 65535;

        private double m_Yaxiscorrectionfactor = 1;
        private double m_Yaxiscorrectionoffset = 0;

        public double Yaxiscorrectionoffset
        {
            get { return m_Yaxiscorrectionoffset; }
            set { m_Yaxiscorrectionoffset = value; }
        }

        public double Yaxiscorrectionfactor
        {
            get { return m_Yaxiscorrectionfactor; }
            set { m_Yaxiscorrectionfactor = value; }
        }

        private string m_xaxisUnits = string.Empty;

        public string XaxisUnits
        {
            get { return m_xaxisUnits; }
            set
            {
                m_xaxisUnits = value.ToUpper();
            }
        }
        private string m_yaxisUnits = string.Empty;

        public string YaxisUnits
        {
            get { return m_yaxisUnits; }
            set
            {
                m_yaxisUnits = value.ToUpper();
            }
        }

        private double m_Xaxiscorrectionfactor = 1;
        private double m_Xaxiscorrectionoffset = 0;

        public double Xaxiscorrectionoffset
        {
            get { return m_Xaxiscorrectionoffset; }
            set { m_Xaxiscorrectionoffset = value; }
        }

        public double Xaxiscorrectionfactor
        {
            get { return m_Xaxiscorrectionfactor; }
            set { m_Xaxiscorrectionfactor = value; }
        }

        byte[] open_loop;

        public byte[] Open_loop
        {
            get { return open_loop; }
            set { open_loop = value; }
        }

        public void SetViewSize(ViewSize vs)
        {
            m_vs = vs; 
            if (vs == ViewSize.SmallView)
            {
                this.Font = new Font("Tahoma", 8);
                gridControl1.DefaultCellStyle.Font = new Font("Tahoma", 8);
            }
            else if (vs == ViewSize.ExtraSmallView)
            {
                this.Font = new Font("Tahoma", 7);
                gridControl1.DefaultCellStyle.Font = new Font("Tahoma", 7);
            }
        }
        
        private SymbolCollection m_SymbolCollection = new SymbolCollection();

        public SymbolCollection mapSymbolCollection
        {
            get { return m_SymbolCollection; }
            set { m_SymbolCollection = value; }
        }

        public ViewType Viewtype
        {
            get { return m_viewtype; }
            set
            {
                m_viewtype = value;
                m_prohibit_viewchange = true;
                toolStripComboBox3.SelectedIndex = (int)m_viewtype;
                m_prohibit_viewchange = false;
            }
        }

        int[] afr_counter;

        public int[] Afr_counter
        {
            get { return afr_counter; }
            set { afr_counter = value; }
        }

        private void ShowHitInfo(DataGridView.HitTestInfo hi)
        {
            if (hi.Type == DataGridViewHitTestType.Cell)
            {
    
                if (afr_counter != null)
                {
                    // fetch correct counter
                    int current_afrcounter = (int)afr_counter[(afr_counter.Length - ((hi.RowIndex + 1) * m_TableWidth)) + hi.ColumnIndex];
                    // show number of measurements in balloon
                    string detailline = "# measurements: " + current_afrcounter.ToString();
                    // Show tooltip using the centralized TooltipService, anchored to gridControl1.
                    // We convert the screen cursor position to the gridControl1 client coordinates.
                    TooltipService.ShowForControl(gridControl1, gridControl1.PointToClient(Cursor.Position), "Information", detailline);
                }
            }
            else
            {
                TooltipService.Hide();
            }
        }

        public int MaxValueInTable
        {
            get { return m_MaxValueInTable; }
            set { m_MaxValueInTable = value; }
        }

        public bool AutoSizeColumns
        {
            set
            {
                gridControl1.AutoSizeColumnsMode = value ? DataGridViewAutoSizeColumnsMode.Fill : DataGridViewAutoSizeColumnsMode.None;
            }
        }

        private bool m_disablecolors = false;

        public bool DisableColors
        {
            get
            {
                return m_disablecolors;
            }
            set
            {
                m_disablecolors = value;
                Invalidate();
            }
        }

        private string m_filename;
        private bool m_isRedWhite = false;
        private int m_textheight = 12;
        private string m_xformatstringforhex = "X4";
        private bool m_isDragging = false;
        private int _mouse_drag_x = 0;
        private int _mouse_drag_y = 0;
        private bool m_prohibitlock_change = false;

        private bool _isCompareViewer = false;

        public bool IsCompareViewer
        {
            get { return _isCompareViewer; }
            set
            {
                _isCompareViewer = value;
                if (_isCompareViewer)
                {
                    gridControl1.ReadOnly = true; // don't let the user edit a compare viewer
                    toolStripButton3.Enabled = false;
                    toolStripTextBox1.Enabled = false;
                    toolStripComboBox1.Enabled = false;
                    smoothSelectionToolStripMenuItem.Enabled = false;
                    pasteSelectedCellsToolStripMenuItem.Enabled = false;
                    //exportMapToolStripMenuItem.Enabled = false;
                    simpleButton2.Enabled = false;
//                    simpleButton3.Enabled = false;
                    //btnSaveToRAM.Enabled = false;
                    //btnReadFromRAM.Enabled = false;
                }
            }
        }

        private bool m_tableVisible = false;

        public int SliderPosition
        {
            get { return (int)trackBarControl1.Value; }
            set { trackBarControl1.Value = value; }
        }

        public bool TableVisible
        {
            get { return m_tableVisible; }
            set
            {
                m_tableVisible = value;
                splitContainer1.Panel1Collapsed = !m_tableVisible;
            }
        }

        public int LockMode
        {
            get
            {
                return toolStripComboBox2.SelectedIndex;
            }
            set
            {
                m_prohibitlock_change = true;
                toolStripComboBox2.SelectedIndex = value;
                m_prohibitlock_change = false;
            }

        }

        private double _max_y_axis_value = 0;

        public double Max_y_axis_value
        {
            get
            {
                return _max_y_axis_value;
            }
            set {
                _max_y_axis_value = value;
            }
        }

        private bool m_isRAMViewer = false;

        public bool IsRAMViewer
        {
            get { return m_isRAMViewer; }
            set
            {
                m_isRAMViewer = value;
                // Also when software is open
//                simpleButton8.Enabled = m_isRAMViewer;
//                simpleButton9.Enabled = m_isRAMViewer;
            }
        }

        private bool m_isOpenSoftware = false;


        public bool IsOpenSoftware
        {
            get { return m_isOpenSoftware; }
            set
            {
                m_isOpenSoftware = value;
                // Also when software is open
//                simpleButton8.Enabled = m_isOpenSoftware;
//                simpleButton9.Enabled = m_isOpenSoftware;
            }
        }

        private bool m_isUpsideDown = false;

        public bool IsUpsideDown
        {
            get { return m_isUpsideDown; }
            set { m_isUpsideDown = value; }
        }

        private double correction_factor = 1;

        public double Correction_factor
        {
            get { return correction_factor; }
            set { correction_factor = value; }
        }
        private double correction_offset = 0;

        public double Correction_offset
        {
            get { return correction_offset; }
            set { correction_offset = value; }
        }

        public bool GraphVisible
        {
            get
            {
                return !splitContainer1.Panel2Collapsed;
            }
            set
            {
                splitContainer1.Panel2Collapsed = !value;
            }
        }
        private void btnToggleOverlay_Click(object sender, EventArgs e)
        {
            // do/don't show the graph overlay
            m_OverlayVisible = !m_OverlayVisible;
            RefreshMeshGraph();

        }

        private void RefreshMeshGraph()
        {
            try
            {
                // Use the refactored Chart3DComponent
                if (_chart3DComponent != null)
                {
                    _chart3DComponent.LoadData(CreateMapViewerState());
                    _chart3DComponent.RefreshChart();
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Failed to refresh mesh chart: " + E.Message);
            }
        }

        double now_realMaxValue = double.MinValue;
        double now_realMinValue = double.MaxValue;


        public bool IsRedWhite
        {
            get { return m_isRedWhite; }
            set { m_isRedWhite = value; }
        }


        public string Filename
        {
            get { return m_filename; }
            set { m_filename = value; }
        }


        public bool DatasourceMutated
        {
            get { return m_datasourceMutated; }
            set { m_datasourceMutated = value; }
        }
        private bool m_SaveChanges = false;

        public bool SaveChanges
        {
            get { return m_SaveChanges; }
            set { m_SaveChanges = value; }
        }
        private byte[] m_map_content;

        public byte[] Map_content
        {
            get { return m_map_content; }
            set
            {
                m_map_content = value;
                // Initialize m_map_original_content when m_map_content is first set
                if (m_map_original_content == null && value != null)
                {
                    m_map_original_content = (byte[])value.Clone();
                }
            }
        }

        private byte[] m_map_compare_content;

        public byte[] Map_compare_content
        {
            get { return m_map_compare_content; }
            set { m_map_compare_content = value; }
        }

        private byte[] m_map_original_content;

        public byte[] Map_original_content
        {
            get { return m_map_original_content; }
            set { m_map_original_content = value; }
        }
        private bool m_UseNewCompare = false;

        public bool UseNewCompare
        {
            get { return m_UseNewCompare; }
            set { m_UseNewCompare = value; }
        }

        private bool m_OnlineMode = false;

        public bool OnlineMode
        {
            get { return m_OnlineMode; }
            set
            {
                m_OnlineMode = value;
                Console.WriteLine("RefreshMeshGraph on online mode");
                if (m_OnlineMode)
                {
                    RefreshMeshGraph();
                    UpdateChartControlSlice(GetDataFromGridView(false));
                    ShowTable(m_TableWidth, m_issixteenbit);
                }
                else
                {
                    RefreshMeshGraph();
                    UpdateChartControlSlice(GetDataFromGridView(false));
                    ShowTable(m_TableWidth, m_issixteenbit);
                }
            }
        }

        private bool _autoUpdateIfSRAM = false;

        public bool AutoUpdateIfSRAM
        {
            get { return _autoUpdateIfSRAM; }
            set { _autoUpdateIfSRAM = value; }
        }

        private int _autoUpdateInterval = 20;

        public int AutoUpdateInterval
        {
            get { return _autoUpdateInterval; }
            set { _autoUpdateInterval = value; }
        }

        private Int32 m_map_address = 0;

        public Int32 Map_address
        {
            get { return m_map_address; }
            set { m_map_address = value; }
        }


        private Int32 m_map_sramaddress = 0;

        public Int32 Map_sramaddress
        {
            get { return m_map_sramaddress; }
            set { m_map_sramaddress = value; }
        }
        private Int32 m_map_length = 0;

        public Int32 Map_length
        {
            get { return m_map_length; }
            set { m_map_length = value; }
        }
        private string m_map_name = string.Empty;

        public string Map_name
        {
            get { return m_map_name; }
            set
            {
                m_map_name = value;
                this.Text = "Table details [" + m_map_name + "]";
                //groupControl1.Text = "Symbol data [" + m_map_name + "] " + m_z_axis_name;
                SetGroupText();
            }
        }

        private void SetGroupText()
        {
            groupControl1.Values.Heading = "X: " + m_x_axis_name + " Y: " + m_y_axis_name + " Z: " + m_z_axis_name;
        }

        private string m_map_descr = string.Empty;

        public string Map_descr
        {
            get { return m_map_descr; }
            set
            {
                m_map_descr = value;
            }
        }

        private XDFCategories m_map_cat = XDFCategories.Undocumented;

        public XDFCategories Map_cat
        {
            get { return m_map_cat; }
            set
            {
                m_map_cat = value;
                //.Text = m_map_descr;
            }
        }

        private string m_x_axis_name = string.Empty;

        public string X_axis_name
        {
            get { return m_x_axis_name; }
            set
            {
                m_x_axis_name = value;
                SetGroupText();
            }
        }
        private string m_y_axis_name = string.Empty;

        public string Y_axis_name
        {
            get { return m_y_axis_name; }
            set
            {
                m_y_axis_name = value;
                SetGroupText();
            }
        }
        private string m_z_axis_name = string.Empty;

        public string Z_axis_name
        {
            get { return m_z_axis_name; }
            set
            {
                m_z_axis_name = value;
                //groupControl1.Text = "Symbol data [" + m_map_name + "] " + m_z_axis_name;
                //groupControl1.Text = "Symbol data " + m_z_axis_name;
                SetGroupText();
            }
        }

        private int x_axisAddress = 0;

        public int X_axisAddress
        {
            get { return x_axisAddress; }
            set { x_axisAddress = value; }
        }
        private int y_axisAddress = 0;

        public int Y_axisAddress
        {
            get { return y_axisAddress; }
            set { y_axisAddress = value; }
        }


        private int[] x_axisvalues;

        public int[] X_axisvalues
        {
            get { return x_axisvalues; }
            set { x_axisvalues = value; }
        }
        private int[] y_axisvalues;

        public int[] Y_axisvalues
        {
            get { return y_axisvalues; }
            set { y_axisvalues = value; }
        }


        public MapViewerEx()
        {
            Console.WriteLine("MapViewerEx: Constructor started");
            InitializeComponent();
            toolStripComboBox1.SelectedIndex = 0;
            toolStripComboBox2.SelectedIndex = 0;

            this.Load += (s, e) => {
                Console.WriteLine("MapViewerEx: Load event fired");
            };

            nChartControl1.MouseWheel += new MouseEventHandler(nChartControl1_MouseWheel);
            nChartControl1.MouseDown += new MouseEventHandler(nChartControl1_MouseDown);
            nChartControl1.MouseUp += new MouseEventHandler(nChartControl1_MouseUp);

            gridControl1.MouseMove += new MouseEventHandler(gridView1_MouseMove);

            // Initialize Phase 5 Components
            InitializePhase5Components();

            // Initialize Phase 3 Services with default implementations
            _mapRenderingService = new MapRenderingService();
            _chartService = new ChartService();
            _dataConversionService = new DataConversionService();
            _clipboardService = new ClipboardService(_dataConversionService);
            _smoothingService = new SmoothingService(_dataConversionService);
            _mapOperationService = new MapOperationService(_dataConversionService);
            _mapValidationService = new MapValidationService(_dataConversionService);
            _controller = new MapViewerController(this);
            
        }
        
        /// <summary>
        /// Creates a MapViewerState from the current MapViewerEx state
        /// </summary>
        private MapViewerState CreateMapViewerState()
        {
            return new MapViewerState
            {
                Data = new MapData
                {
                    Content = m_map_content,
                    OriginalContent = m_map_original_content,
                    CompareContent = m_map_compare_content,
                    Address = m_map_address,
                    SramAddress = m_map_sramaddress,
                    Length = m_map_length,
                    IsSixteenBit = m_issixteenbit,
                    TableWidth = m_TableWidth,
                    MaxValueInTable = m_MaxValueInTable,
                    OpenLoop = open_loop
                },
                Metadata = new MapMetadata
                {
                    Name = m_map_name,
                    Description = m_map_descr,
                    Filename = m_filename,
                    Category = m_map_cat,
                    XAxisName = m_x_axis_name,
                    YAxisName = m_y_axis_name,
                    ZAxisName = m_z_axis_name,
                    XAxisUnits = m_xaxisUnits,
                    YAxisUnits = m_yaxisUnits
                },
                Axes = new AxisData
                {
                    XAxisValues = x_axisvalues,
                    YAxisValues = y_axisvalues,
                    XAxisAddress = x_axisAddress,
                    YAxisAddress = y_axisAddress,
                    XCorrectionFactor = m_Xaxiscorrectionfactor,
                    XCorrectionOffset = m_Xaxiscorrectionoffset,
                    YCorrectionFactor = m_Yaxiscorrectionfactor,
                    YCorrectionOffset = m_Yaxiscorrectionoffset
                },
                Configuration = new ViewConfiguration
                {
                    ViewType = m_viewtype,
                    ViewSize = m_vs,
                    IsUpsideDown = m_isUpsideDown,
                    DisableColors = m_disablecolors,
                    IsRedWhite = m_isRedWhite,
                    CorrectionFactor = correction_factor,
                    CorrectionOffset = correction_offset,
                    LockMode = toolStripComboBox2.SelectedIndex,
                    TableVisible = !splitContainer1.Panel1Collapsed,
                    GraphVisible = !splitContainer1.Panel2Collapsed
                },
                IsDirty = m_datasourceMutated,
                IsCompareMode = _isCompareViewer,
                IsDifferenceMode = m_isDifferenceViewer,
                IsRAMViewer = m_isRAMViewer,
                IsOnlineMode = m_OnlineMode,
                MaxValueInTable = m_MaxValueInTable,
                RealMaxValue = m_realMaxValue,
                RealMinValue = m_realMinValue
            };
        }
        
        /// <summary>
        /// Initializes Phase 5 UI components
        /// </summary>
        private void InitializePhase5Components()
        {
            try
            {
                Console.WriteLine("MapViewerEx: Initializing Phase 5 Components...");
                
                // Ensure services are initialized before components
                if (_dataConversionService == null) _dataConversionService = new DataConversionService();
                if (_mapRenderingService == null) _mapRenderingService = new MapRenderingService();
                if (_chartService == null) _chartService = new ChartService();

                // Create component instances
                _mapGridComponent = new MapGridComponent(_dataConversionService, _mapRenderingService);
                _chart3DComponent = new Chart3DComponent(_chartService, _dataConversionService);
                _chart2DComponent = new Chart2DComponent(_chartService);
                
                // Apply Theme
                VAGEDCThemeManager.Instance.ApplyThemeToForm(this.FindForm() ?? new Form());
                ApplyThemeToADGV();
                ApplyThemeToNavigator();
                ApplyThemeToToolStrip();
                UpdateMeshButtonHighlights();

                // Wire up the external chart control to the component
                // Verified: nChartControl1 is now OpenTK.GLControl and nChartControl2 is ZedGraph.ZedGraphControl
                if (nChartControl1 != null)
                {
                    _chart3DComponent.SetChartControl(nChartControl1);
                    nChartControl1.Dock = DockStyle.Fill;
                    nChartControl1.Visible = true;
                    nChartControl1.BringToFront();

                    // Bring navigational buttons to front so they are visible over the OpenTK render
                    simpleButton7.BringToFront(); // Zoom In
                    simpleButton6.BringToFront(); // Zoom Out
                    simpleButton4.BringToFront(); // Rotate Left
                    simpleButton5.BringToFront(); // Rotate Right
                    btnToggleWireframe.BringToFront(); // Wireframe Toggle
                    btnToggleTooltips.BringToFront(); // Tooltip Toggle

                    // Ensure overlay buttons have readable labels and themed font/color so they remain visible
                    try
                    {
                        var btnFont = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Bold);
                        KryptonButton[] overlayButtons = new KryptonButton[] { simpleButton7, simpleButton6, simpleButton4, simpleButton5, btnToggleWireframe, btnToggleTooltips };
                        foreach (var kb in overlayButtons)
                        {
                            if (kb != null)
                            {
                                if (string.IsNullOrEmpty(kb.Values.Text))
                                {
                                    // Fallback labels for small overlay buttons
                                    if (kb == simpleButton7) kb.Values.Text = "+";
                                    else if (kb == simpleButton6) kb.Values.Text = "-";
                                    else if (kb == simpleButton4) kb.Values.Text = "<";
                                    else if (kb == simpleButton5) kb.Values.Text = ">";
                                    else if (kb == btnToggleWireframe) kb.Values.Text = "W";
                                    else if (kb == btnToggleTooltips) kb.Values.Text = "T";
                                }
                                // Apply a compact, legible font and theme color so labels are visible over GLControl
                                kb.StateCommon.Content.ShortText.Font = btnFont;
                                kb.StateCommon.Content.ShortText.Color1 = VAGEDCThemeManager.Instance.CurrentTheme.TextPrimary;
                                // Prefer text-only for these overlay buttons
                                kb.Values.Image = null;
                                kb.Refresh();
                            }
                        }
                    }
                    catch { }

                    // Tooltips removed for buildability in legacy Krypton 4.5.9

                    Console.WriteLine($"MapViewerEx: nChartControl1 size: {nChartControl1.Width}x{nChartControl1.Height}");
                }
                
                if (nChartControl2 != null)
                {
                    _chart2DComponent.SetChartControl(nChartControl2);
                }
                
                // Wire up component events to MapViewerEx handlers
                _chart3DComponent.ViewChanged += OnChart3DViewChanged;
                Console.WriteLine("MapViewerEx: Phase 5 Components initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("MapViewerEx: CRITICAL ERROR in InitializePhase5Components: " + ex.Message);
            }
        }

        private void ApplyThemeToADGV()
        {
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            gridControl1.BackgroundColor = theme.GridBackground;
            gridControl1.GridColor = theme.GridBorder;
            gridControl1.DefaultCellStyle.BackColor = theme.GridBackground;
            gridControl1.DefaultCellStyle.ForeColor = theme.TextPrimary;
            gridControl1.DefaultCellStyle.SelectionBackColor = theme.GridSelection;
            gridControl1.DefaultCellStyle.SelectionForeColor = Color.White;
            
            gridControl1.ColumnHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
            gridControl1.ColumnHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
            gridControl1.ColumnHeadersDefaultCellStyle.Font = theme.GridHeaderFont;
            gridControl1.EnableHeadersVisualStyles = false;

            gridControl1.RowHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
            gridControl1.RowHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
        }

        private void ApplyThemeToNavigator()
        {
            // Style Navigator tabs to match VS Code dark mode
            xtraTabControl1.PaletteMode = PaletteMode.Custom;
            xtraTabControl1.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            
            xtraTabControl1.StateCommon.Bar.CheckButtonGap = 2;
            xtraTabControl1.StateCommon.Tab.Content.ShortText.Font = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);
            
            // Active Tab (VS Code style: Darker background, blue indicator/accent)
            xtraTabControl1.StateSelected.Tab.Back.Color1 = Color.FromArgb(30, 30, 30);
            xtraTabControl1.StateSelected.Tab.Back.ColorStyle = PaletteColorStyle.Solid;
            xtraTabControl1.StateSelected.Tab.Content.ShortText.Color1 = Color.White;
            
            // Inactive Tab
            xtraTabControl1.StateCommon.Tab.Back.Color1 = Color.FromArgb(45, 45, 45);
            xtraTabControl1.StateCommon.Tab.Back.ColorStyle = PaletteColorStyle.Solid;
            xtraTabControl1.StateCommon.Tab.Content.ShortText.Color1 = Color.FromArgb(150, 150, 150);

            // Ensure the font is Source Sans Pro
            xtraTabControl1.StateCommon.Tab.Content.ShortText.Font = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);
        }

        private void ApplyThemeToToolStrip()
        {
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            toolStrip1.Renderer = VAGEDCThemeManager.Instance.GetToolStripRenderer();
            toolStrip1.BackColor = theme.ToolbarBackground;
            toolStrip1.ForeColor = theme.ToolbarText;
            
            Font toolStripFont = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                item.Font = toolStripFont;
                item.ForeColor = theme.ToolbarText;
                if (item is ToolStripComboBox combo)
                {
                    // Fix white-on-white for ToolStripComboBox (Map modifiers and View types)
                    // Verified: VS Code Dark+ uses #3C3C3C for input backgrounds
                    combo.FlatStyle = FlatStyle.Flat;
                    combo.ComboBox.BackColor = Color.FromArgb(60, 60, 60);
                    combo.ComboBox.ForeColor = Color.White;
                }
                else if (item is ToolStripTextBox textBox)
                {
                    textBox.BackColor = Color.FromArgb(60, 60, 60);
                    textBox.ForeColor = Color.White;
                }
            }

            // Style the GroupBox (Top Ribbon/Label area)
            // CRITICAL: Must set PaletteMode to Custom explicitly to override global manager
            groupControl1.PaletteMode = PaletteMode.Custom;
            groupControl1.Palette = VAGEDCThemeManager.Instance.CustomPalette;
            
            // Use StateNormal instead of StateCommon to ensure higher priority over global styles
            groupControl1.StateNormal.Back.Color1 = theme.PanelBackground;
            groupControl1.StateNormal.Back.ColorStyle = PaletteColorStyle.Solid;
            groupControl1.StateNormal.Content.ShortText.Font = VAGEDCThemeManager.Instance.GetCustomFont(10f, FontStyle.Bold);
            groupControl1.StateNormal.Content.ShortText.Color1 = theme.TextPrimary;
            groupControl1.StateNormal.Border.Color1 = theme.BorderPrimary;
            groupControl1.StateNormal.Border.DrawBorders = PaletteDrawBorders.All;

            // Style Bottom Panel Buttons
            KryptonButton[] bottomButtons = new KryptonButton[] { simpleButton1, simpleButton2, simpleButton3, simpleButton8, simpleButton9, simpleButton10 };
            foreach (var btn in bottomButtons)
            {
                if (btn != null)
                {
                    // CRITICAL: Force Custom PaletteMode
                    btn.PaletteMode = PaletteMode.Custom;
                    btn.Palette = VAGEDCThemeManager.Instance.CustomPalette;
                    btn.ButtonStyle = ButtonStyle.Standalone;
                    
                    // Use StateNormal to override global defaults
                    // Use StateCommon for the base colors to ensure they are applied correctly
                    // but we must be careful not to let them override StateDisabled
                    btn.StateNormal.Back.Color1 = Color.FromArgb(0, 100, 180); // VS Code Blue
                    btn.StateNormal.Back.ColorStyle = PaletteColorStyle.Solid;
                    btn.StateNormal.Content.ShortText.Color1 = Color.White;
                    btn.StateNormal.Content.ShortText.Font = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);

                    // Explicitly fix disabled state for bottom buttons (Undo, Save, Close, Read)
                    // We use StateDisabled and ensure StateCommon doesn't have conflicting colors
                    btn.StateDisabled.Back.Color1 = Color.FromArgb(45, 45, 45);
                    btn.StateDisabled.Back.ColorStyle = PaletteColorStyle.Solid;
                    btn.StateDisabled.Content.ShortText.Color1 = Color.FromArgb(200, 200, 200); // High contrast light gray
                    btn.StateDisabled.Border.Color1 = Color.FromArgb(64, 64, 64);
                    btn.StateDisabled.Border.DrawBorders = PaletteDrawBorders.All;
                    btn.StateDisabled.Border.Width = 1;

                    // Force refresh to apply state changes
                    btn.Refresh();
                    
                    btn.StateNormal.Border.Color1 = theme.BorderPrimary;
                    btn.StateNormal.Border.DrawBorders = PaletteDrawBorders.All;
                    btn.StateNormal.Border.Width = 1;

                    // Also set StateTracking (Hover) to ensure it doesn't revert to system style
                    btn.StateTracking.Back.Color1 = Color.FromArgb(20, 142, 224);
                    btn.StateTracking.Back.ColorStyle = PaletteColorStyle.Solid;
                    btn.StateTracking.Content.ShortText.Color1 = Color.White;
                    
                    // And StatePressed
                    btn.StatePressed.Back.Color1 = Color.FromArgb(0, 102, 184);
                    btn.StatePressed.Back.ColorStyle = PaletteColorStyle.Solid;
                    btn.StatePressed.Content.ShortText.Color1 = Color.White;
                }
            }

            // Style 3D Mesh Navigation Buttons (Vertical stack)
            KryptonButton[] meshButtons = new KryptonButton[] { simpleButton4, simpleButton5, simpleButton6, simpleButton7, btnToggleWireframe, btnToggleTooltips, btnToggleOverlay };
            foreach (var btn in meshButtons)
            {
                if (btn != null)
                {
                    btn.PaletteMode = PaletteMode.Custom;
                    btn.Palette = VAGEDCThemeManager.Instance.CustomPalette;
                    btn.ButtonStyle = ButtonStyle.Standalone;
                    
                    // Use StateNormal
                    btn.StateNormal.Back.Color1 = Color.FromArgb(60, 60, 60); // Darker for overlay buttons
                    btn.StateNormal.Back.ColorStyle = PaletteColorStyle.Solid;
                    btn.StateNormal.Content.ShortText.Color1 = Color.White;
                    btn.StateNormal.Content.ShortText.Font = VAGEDCThemeManager.Instance.GetCustomFont(8f, FontStyle.Bold);
                    
                    btn.StateNormal.Border.Color1 = Color.White;
                    btn.StateNormal.Border.DrawBorders = PaletteDrawBorders.All;
                    btn.StateNormal.Border.Width = 1;

                    // Hover state for mesh buttons
                    btn.StateTracking.Back.Color1 = Color.FromArgb(80, 80, 80);
                    btn.StateTracking.Back.ColorStyle = PaletteColorStyle.Solid;
                    btn.StateTracking.Content.ShortText.Color1 = Color.White;
                }
            }
        }

        private void UpdateMeshButtonHighlights()
        {
            if (_chart3DComponent == null) return;

            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            Color activeColor = Color.FromArgb(0, 122, 204); // VS Code Blue for active state
            Color inactiveColor = Color.FromArgb(60, 60, 60); // Default dark for overlay buttons

            if (btnToggleWireframe != null)
            {
                bool isWireframe = _chart3DComponent.IsWireframeMode;
                btnToggleWireframe.StateNormal.Back.Color1 = isWireframe ? activeColor : inactiveColor;
            }

            if (btnToggleTooltips != null)
            {
                bool isTooltips = _chart3DComponent.IsTooltipsEnabled;
                btnToggleTooltips.StateNormal.Back.Color1 = isTooltips ? activeColor : inactiveColor;
            }
        }

        /// <summary>
        /// Uses MapGridComponent to render cell with proper coloring and formatting.
        /// This method delegates to the component for rendering logic while keeping control in MapViewerEx.
        /// </summary>
        private void RenderCellWithComponent(DataGridViewCellPaintingEventArgs e, int cellValue)
        {
            // Use MapGridComponent's rendering logic
            _mapGridComponent.LoadData(
                m_map_content,
                m_map_length,
                CreateMapViewerState());
            
            // Apply the component's color calculation
            Color cellColor = _mapRenderingService.CalculateCellColor(
                cellValue,
                m_MaxValueInTable,
                m_OnlineMode,
                m_isRedWhite);
            
            if (!m_disablecolors)
            {
                using (SolidBrush sb = new SolidBrush(cellColor))
                {
                    e.Graphics.FillRectangle(sb, e.CellBounds);
                }
            }
        }

        /// <summary>
        /// Uses Chart3DComponent to refresh the 3D mesh graph.
        /// </summary>
        private void RefreshMeshGraphWithComponent()
        {
            if (_chart3DComponent != null)
            {
                _chart3DComponent.LoadData(CreateMapViewerState());
                _chart3DComponent.RefreshChart();
            }
        }

        /// <summary>
        /// Uses Chart2DComponent to update the 2D chart slice.
        /// </summary>
        private void Update2DChartWithComponent(byte[] data)
        {
            if (_chart2DComponent != null && m_TableWidth == 1)
            {
                MapViewerState state = CreateMapViewerState();
                _chart2DComponent.LoadData(data, m_map_length, state);
                _chart2DComponent.RefreshChart(state);
            }
        }

        /// <summary>
        /// Gets cell value from grid using component patterns.
        /// </summary>
        private int GetCellValueFromGrid(int rowHandle, int columnIndex)
        {
            object cellValue = gridControl1.Rows[rowHandle].Cells[columnIndex].Value;
            if (cellValue == null) return 0;
            
            if (m_viewtype == ViewType.Hexadecimal)
            {
                return Convert.ToInt32(cellValue.ToString(), 16);
            }
            else
            {
                return _dataConversionService.ParseValue(cellValue.ToString(), m_viewtype);
            }
        }

        /// <summary>
        /// Sets cell value in grid using component patterns.
        /// </summary>
        private void SetCellValueInGrid(int rowHandle, int columnIndex, object value)
        {
            gridControl1.Rows[rowHandle].Cells[columnIndex].Value = value;
        }

        private void OnChart3DViewChanged(object sender, SurfaceGraphViewChangedEventArgsEx e)
        {
            CastSurfaceGraphChangedEventEx(e.DepthX, e.DepthY, e.Zoom, e.Rotation, e.Elevation);
        }

        void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_map_name == "TargetAFR" || m_map_name == "FeedbackAFR" || m_map_name == "FeedbackvsTargetAFR")
            {
                ShowHitInfo(gridControl1.HitTest(e.X, e.Y));
            }
        }

        void nChartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            m_isDragging = false;
        }

        private void CastSurfaceGraphChangedEventEx(float xdepth, float ydepth, float zoom, float rotation, float elevation)
        {
            if (onSurfaceGraphViewChangedEx != null)
            {
                if (!m_prohibitgraphchange)
                {
                    onSurfaceGraphViewChangedEx(this, new SurfaceGraphViewChangedEventArgsEx(xdepth, ydepth, zoom, rotation, elevation, m_map_name));
                }
            }
        }

        void nChartControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_isDragging && _chart3DComponent != null)
            {
                float dx = e.X - _mouse_drag_x;
                float dy = e.Y - _mouse_drag_y;

                float rotation, elevation, zoom;
                _chart3DComponent.GetView(out rotation, out elevation, out zoom);

                rotation += dx * 0.5f;
                elevation += dy * 0.5f;

                _chart3DComponent.SetView(rotation, elevation, zoom);

                _mouse_drag_x = e.X;
                _mouse_drag_y = e.Y;
            }
        }

        void nChartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_isDragging = true;
                _mouse_drag_x = e.X;
                _mouse_drag_y = e.Y;
            }
        }

        void nChartControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_chart3DComponent != null)
            {
                float rotation, elevation, zoom;
                _chart3DComponent.GetView(out rotation, out elevation, out zoom);

                if (e.Delta > 0) zoom *= 1.1f;
                else zoom /= 1.1f;

                _chart3DComponent.SetView(rotation, elevation, zoom);
            }
        }


        public bool SaveData()
        {
            bool retval = false;
            if (simpleButton2.Enabled)
            {
                simpleButton2_Click(this, EventArgs.Empty);
                retval = true;
            }
            return retval;
        }


        public void ShowTable(int tablewidth, bool issixteenbits)
        {
            Console.WriteLine($"🧑🔬 [DEBUG] MapViewerEx.ShowTable: Width={tablewidth}, 16bit={issixteenbits}, Name={m_map_name}");
            m_TableWidth = tablewidth;
            m_issixteenbit = issixteenbits;

            if (m_map_length != 0 && m_map_name != string.Empty)
            {
                MapViewerState state = CreateMapViewerState();
                DataTable dt = _dataConversionService.ConvertToDataTable(state.Data, state.Configuration);
                
                // Use the refactored service to calculate statistics
                _dataConversionService.CalculateStatistics(
                    dt,
                    m_viewtype,
                    correction_factor,
                    correction_offset,
                    _isCompareViewer,
                    out m_MaxValueInTable,
                    out m_realMinValue,
                    out m_realMaxValue);

                gridControl1.DataSource = dt;

                if (gridControl1.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.Fill)
                {
                    for (int c = 0; c < gridControl1.Columns.Count; c++)
                    {
                        gridControl1.Columns[c].Width = 40;
                    }
                }

                // set axis indicator width for Y-axis labels and accommodate unit labels
                int indicatorwidth = -1;
                float xUnitWidth = 0f, yUnitWidth = 0f;
                using (Graphics g = gridControl1.CreateGraphics())
                {
                    for (int i = 0; i < y_axisvalues.Length; i++)
                    {
                        string yval = _dataConversionService.FormatValue(Convert.ToInt32(y_axisvalues.GetValue(i)), m_viewtype, true);
                        // Add a small buffer for the dot and padding
                        SizeF size = g.MeasureString(yval + ". ", this.Font);
                        if (size.Width > indicatorwidth) indicatorwidth = (int)size.Width;
                        m_textheight = (int)size.Height;
                    }
                    Font smallFont = VAGEDCThemeManager.Instance.GetCustomFont(8f, FontStyle.Regular);
                    if (!string.IsNullOrEmpty(m_xaxisUnits)) xUnitWidth = g.MeasureString(m_xaxisUnits, smallFont).Width;
                    if (!string.IsNullOrEmpty(m_yaxisUnits)) yUnitWidth = g.MeasureString(m_yaxisUnits, smallFont).Width;
                }
                if (indicatorwidth > 0)
                {
                    // Ensure minimum width for units display in top-left and provide padding
                    int unitWidth = (int)Math.Ceiling(Math.Max(xUnitWidth, yUnitWidth));
                    gridControl1.RowHeadersWidth = Math.Max(Math.Max(indicatorwidth + 10, unitWidth + 18), 45);
                }

                // Apply X-axis labels to column headers
                for (int i = 0; i < x_axisvalues.Length && i < gridControl1.Columns.Count; i++)
                {
                    int xval = Convert.ToInt32(x_axisvalues.GetValue(i));
                    if (xval > 0xF000)
                    {
                        xval = 0x10000 - xval;
                        xval = -xval;
                        x_axisvalues.SetValue(xval, i);
                    }
                    
                    string xlabel;
                    if (m_viewtype == ViewType.Hexadecimal)
                    {
                        xlabel = xval.ToString(xval <= 255 ? "X2" : "X4");
                    }
                    else
                    {
                        double temp = (double)xval * m_Xaxiscorrectionfactor + m_Xaxiscorrectionoffset;
                        xlabel = temp.ToString("F2");
                    }
                    gridControl1.Columns[i].HeaderText = xlabel;
                }
                if (x_axisvalues.Length > 0 && x_axisvalues[0] <= 255) m_xformatstringforhex = "X2";
                Console.WriteLine($"🧑🔬 [DEBUG] MapViewerEx.ShowTable: Data bound. Rows={gridControl1.RowCount}");

                // Disable filter/sort arrows on column headers
                foreach (DataGridViewColumn col in gridControl1.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    // Use reflection to disable filtering if the specific type is not available at compile time
                    try
                    {
                        var filterProp = col.GetType().GetProperty("FilterAndSortEnabled");
                        if (filterProp != null) filterProp.SetValue(col, false, null);
                    }
                    catch { }
                }
            }

            if (m_TableWidth > 1)
            {
                xtraTabControl1.SelectedPage = xtraTabPage1;
               
                SetViewTypeParams(m_vs);
                trackBarControl1.Minimum = 0;
                trackBarControl1.Maximum = x_axisvalues.Length - 1;
                labelControl8.Values.Text = X_axis_name + " values";
                trackBarControl1.Value = 0;
                // Initialize and configure the 3D chart using the component
                if (_chart3DComponent != null)
                {
                    _chart3DComponent.LoadData(CreateMapViewerState());
                    _chart3DComponent.InitializeChart3D();
                    _chart3DComponent.RefreshChart();
                }
            }
            else if (m_TableWidth == 1)
            {
                xtraTabControl1.SelectedPage = xtraTabPage2;
                
                SetViewTypeParams(m_vs);
                trackBarControl1.Minimum = 0;
                trackBarControl1.Maximum = x_axisvalues.Length - 1;
                labelControl8.Values.Text = X_axis_name + " values";
                /*** end test ***/
                trackBarControl1.Minimum = 0;
                trackBarControl1.Maximum = 0;
                trackBarControl1.Enabled = false;
                labelControl8.Values.Text = X_axis_name;

                // Initialize and configure the 2D chart using the component
                if (_chart2DComponent != null)
                {
                    MapViewerState state = CreateMapViewerState();
                    _chart2DComponent.LoadData(m_map_content, m_map_length, state);
                    _chart2DComponent.RefreshChart(state);
                }
            }
            m_trackbarBlocked = false;

            if (this.m_map_address >= 0xF00000 || (m_isOpenSoftware && m_isRAMViewer))
            {
                //this.m_isRAMViewer = true;
                //this.OnlineMode = true;
                if (_autoUpdateIfSRAM)
                {
                    timer5.Interval = _autoUpdateInterval * 1000;
                    timer5.Enabled = true;
                }
            }
            // Reset mutation state after initial table load to prevent false-positive "Data was mutated" warnings
            m_datasourceMutated = false;
            simpleButton2.Enabled = false;
            simpleButton3.Enabled = false;
        }

        // The Init2dChart method is now handled by Chart2DComponent.ConfigureChart

        private void SetViewTypeParams(ViewSize vs)
        {

        }

        private void UpdateChartControlSlice(byte[] data)
        {
            if (_chart2DComponent != null)
            {
                MapViewerState state = CreateMapViewerState();
                _chart2DComponent.LoadData(data, m_map_length, state);
                
                if (m_TableWidth > 1)
                {
                    // Explicitly update the slice based on current slider position
                    _chart2DComponent.UpdateSlice(data, (int)trackBarControl1.Value);
                }
                else
                {
                    _chart2DComponent.RefreshChart(state);
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!m_isRAMViewer)
            {
                if (m_datasourceMutated)
                {
                    DialogResult dr = MessageBox.Show("Data was mutated, do you want to save these changes in you binary?", "Warning", MessageBoxButtons.YesNoCancel);
                    if (dr == DialogResult.Yes)
                    {
                        m_SaveChanges = true;
                        CastSaveEvent();
                        CastCloseEvent();

                    }
                    else if (dr == DialogResult.No)
                    {
                        m_SaveChanges = false;
                        CastCloseEvent();
                    }
                    else
                    {
                        // cancel
                        // do nothing
                    }
                }
                else
                {
                    m_SaveChanges = false;
                    CastCloseEvent();
                }
            }
            else
            {
                m_SaveChanges = false;
                CastCloseEvent();
            }
        }

        private int m_StandardFill = 0;

        public int StandardFill
        {
            get
            {
                return m_StandardFill;
            }
            set
            {
                m_StandardFill = value;
            }
        }




        private void advancedDataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                var theme = VAGEDCThemeManager.Instance.CurrentTheme;
                if (e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    // Column header cell (top row) - paint without ADGV filter glyphs
                    if (e.RowIndex == -1 && e.ColumnIndex >= 0)
                    {
                        e.PaintBackground(e.CellBounds, true);
                        // Draw header text centered and suppress filter glyphs (custom painting avoids ADGV floating filter)
                        string headerText = gridControl1.Columns[e.ColumnIndex].HeaderText;
                        using (SolidBrush brush = new SolidBrush(theme.GridHeaderText))
                        {
                            TextRenderer.DrawText(e.Graphics, headerText, gridControl1.ColumnHeadersDefaultCellStyle.Font ?? e.CellStyle.Font, e.CellBounds, theme.GridHeaderText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                        }
                        e.Handled = true;
                    }
                    // Handle the top-left corner cell (Unit display with diagonal line)
                    else if (e.RowIndex == -1 && e.ColumnIndex == -1)
                    {
                        e.PaintBackground(e.CellBounds, true);
                        using (Pen pen = new Pen(theme.GridBorder, 1))
                        {
                            e.Graphics.DrawLine(pen, e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Right, e.CellBounds.Bottom);
                        }

                        using (SolidBrush brush = new SolidBrush(theme.GridHeaderText))
                        {
                            Font smallFont = VAGEDCThemeManager.Instance.GetCustomFont(8f, FontStyle.Regular);
                            // Measure sizes to place units properly without overlap
                            SizeF ySize = e.Graphics.MeasureString(m_yaxisUnits, smallFont);
                            SizeF xSize = e.Graphics.MeasureString(m_xaxisUnits, smallFont);
                            float yX = e.CellBounds.Right - ySize.Width - 6f;
                            float yY = e.CellBounds.Top + 2f;
                            float xX = e.CellBounds.Left + 4f;
                            float xY = e.CellBounds.Bottom - xSize.Height - 3f;
                            if (!string.IsNullOrEmpty(m_yaxisUnits))
                                e.Graphics.DrawString(m_yaxisUnits, smallFont, brush, new PointF(yX, yY));
                            if (!string.IsNullOrEmpty(m_xaxisUnits))
                                e.Graphics.DrawString(m_xaxisUnits, smallFont, brush, new PointF(xX, xY));
                        }
                        e.Handled = true;
                    }
                    // Handle row headers for Y-axis labels
                    else if (e.RowIndex >= 0 && e.ColumnIndex == -1 && y_axisvalues != null && y_axisvalues.Length > e.RowIndex)
                    {
                        string yvalue;
                        int index = m_isUpsideDown ? (y_axisvalues.Length - 1) - e.RowIndex : e.RowIndex;
                        int rawY = Convert.ToInt32(y_axisvalues.GetValue(index));

                        if (m_viewtype == ViewType.Hexadecimal)
                        {
                            yvalue = rawY.ToString("X4");
                        }
                        else
                        {
                            double temp = (double)rawY * m_Yaxiscorrectionfactor + m_Yaxiscorrectionoffset;
                            yvalue = temp.ToString("F1");
                        }

                        e.PaintBackground(e.CellBounds, true);
                        using (SolidBrush brush = new SolidBrush(theme.GridHeaderText))
                        {
                            // Use the themed grid cell font and vertically center the text
                            var fontToUse = theme.GridCellFont;
                            SizeF measured = e.Graphics.MeasureString(yvalue, fontToUse);
                            float y = e.CellBounds.Y + (e.CellBounds.Height - measured.Height) / 2f;
                            e.Graphics.DrawString(yvalue, fontToUse, brush, new PointF(e.CellBounds.X + 4, y));
                        }
                        e.Handled = true;
                    }
                    return;
                }

                if (e.Value == null || e.Value == DBNull.Value) return;

                int cellValue = _dataConversionService.ParseValue(e.Value.ToString(), m_viewtype);

                // Use IMapRenderingService for color calculation
                Color cellColor = _mapRenderingService.CalculateCellColor(
                    cellValue,
                    m_MaxValueInTable,
                    m_OnlineMode,
                    m_isRedWhite);

                // Apply coloring if not disabled
                if (!m_disablecolors)
                {
                    // Check if cell is selected to provide visual feedback for area selection
                    // Verified: DataGridViewElementStates.Selected is the standard way to check selection in CellPainting
                    if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
                    {
                        using (SolidBrush sb = new SolidBrush(theme.GridSelection))
                        {
                            e.Graphics.FillRectangle(sb, e.CellBounds);
                        }
                    }
                    else
                    {
                        using (SolidBrush sb = new SolidBrush(cellColor))
                        {
                            e.Graphics.FillRectangle(sb, e.CellBounds);
                        }
                    }
                }

                // Use IMapRenderingService for unified value formatting and unit handling
                string displayText = _mapRenderingService.FormatCellDisplayText(
                    cellValue,
                    CreateMapViewerState().Configuration,
                    CreateMapViewerState().Metadata,
                    m_issixteenbit);

                e.PaintContent(e.CellBounds);
                
                // Fix: 3D map view has 2 sets of values in each cell instead of only one
                // The original code was likely calling e.PaintContent AND TextRenderer.DrawText
                // Since we are handling painting manually (e.Handled = true), we should only use DrawText
                // or let PaintContent handle it if we don't override the text.
                // However, we need custom formatting, so we use DrawText.
                
                // Clear the background again to ensure no ghost text from PaintContent
                using (SolidBrush sb = new SolidBrush(m_disablecolors ? Color.White : cellColor))
                {
                    e.Graphics.FillRectangle(sb, e.CellBounds);
                }

                // Enhance selection visibility with bold text and high contrast
                bool isSelected = (e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;
                Font textFont = e.CellStyle.Font;
                Color textColor = e.CellStyle.ForeColor;

                if (isSelected)
                {
                    // Use bold font and black text for selected cells to provide clear visual feedback
                    textFont = new Font(e.CellStyle.Font, FontStyle.Bold);
                    textColor = Color.Black;
                }

                TextRenderer.DrawText(e.Graphics, displayText, textFont, e.CellBounds, textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                // Clean up bold font if created
                if (isSelected && textFont != e.CellStyle.Font)
                {
                    textFont.Dispose();
                }

                // Open Loop Indicator logic refactored to service
                if (_mapRenderingService.ShouldShowOpenLoopIndicator(e.RowIndex, e.ColumnIndex, open_loop, x_axisvalues, y_axisvalues, m_xaxisUnits, m_yaxisUnits))
                {
                    if (m_StandardFill == 1)
                    {
                        using (Pen p = new Pen(Brushes.Black, 2))
                        {
                            e.Graphics.DrawRectangle(p, e.CellBounds.X + 1, e.CellBounds.Y + 1, e.CellBounds.Width - 2, e.CellBounds.Height - 2);
                        }
                    }
                    else if (m_StandardFill > 1)
                    {
                        Point[] pnts = new Point[4];
                        pnts[0] = new Point(e.CellBounds.X + e.CellBounds.Width, e.CellBounds.Y);
                        pnts[1] = new Point(e.CellBounds.X + e.CellBounds.Width - (e.CellBounds.Height / 2), e.CellBounds.Y);
                        pnts[2] = new Point(e.CellBounds.X + e.CellBounds.Width, e.CellBounds.Y + (e.CellBounds.Height / 2));
                        pnts[3] = new Point(e.CellBounds.X + e.CellBounds.Width, e.CellBounds.Y);
                        e.Graphics.FillPolygon(Brushes.SeaGreen, pnts, System.Drawing.Drawing2D.FillMode.Winding);
                    }
                }
                // Real-time tracking highlight (Yellow) takes precedence over selection
                if (m_selectedrowhandle >= 0 && m_selectedcolumnindex >= 0)
                {
                    if (e.RowIndex == m_selectedrowhandle && e.ColumnIndex == m_selectedcolumnindex)
                    {
                        using (SolidBrush sbsb = new SolidBrush(Color.Yellow))
                        {
                            e.Graphics.FillRectangle(sbsb, e.CellBounds);
                        }
                    }
                }
                e.Handled = true;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }


        private void advancedDataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            m_datasourceMutated = true;
            simpleButton2.Enabled = true;
            simpleButton3.Enabled = true; // Enable the undo button when data is mutated
            
            // Update m_map_content with the latest data from the grid
            m_map_content = GetDataFromGridView(m_isUpsideDown);
            
            // Ensure components are updated with the new content
            if (_chart3DComponent != null) _chart3DComponent.LoadData(CreateMapViewerState());
            if (_chart2DComponent != null) _chart2DComponent.LoadData(m_map_content, m_map_length, CreateMapViewerState());

            if (nChartControl1.Visible)
            {
                StartSurfaceChartUpdateTimer();
            }
            else if (nChartControl2.Visible)
            {
                if (m_TableWidth == 1)
                {
                    StartSingleLineGraphTimer();
                }
                else
                {
                    StartChartUpdateTimer();
                }
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            // Restore original map content
            if (m_map_original_content != null)
            {
                m_map_content = (byte[])m_map_original_content.Clone();
                ShowTable(m_TableWidth, m_issixteenbit);
                m_datasourceMutated = false;
                simpleButton2.Enabled = false;
                simpleButton3.Enabled = false;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
                m_SaveChanges = true;
                m_datasourceMutated = false;
                CastSaveEvent();
        }

        private byte[] GetDataFromGridView(bool upsidedown)
        {
            DataTable gdt = (DataTable)gridControl1.DataSource;
            if (gdt == null) return m_map_content;
            MapViewerState state = CreateMapViewerState();
            return _dataConversionService.ConvertFromDataTable(gdt, state.Data, state.Configuration, upsidedown);
        }
        private void CastSliderMoveEvent()
        {
            if (onSliderMove != null)
            {
                onSliderMove(this, new SliderMoveEventArgs((int)trackBarControl1.Value, m_map_name, m_filename));
            }
        }

        private void CastLockEvent(int mode)
        {
            if (onAxisLock != null)
            {
                switch (mode)
                {
                    case 0: // autoscale
                        onAxisLock(this, new AxisLockEventArgs(-1, mode, m_map_name, m_filename));
                        break;
                    case 1: // peak value
                        onAxisLock(this, new AxisLockEventArgs(m_MaxValueInTable, mode, m_map_name, m_filename));
                        break;
                    case 2: // max value
                        int max_value = 0xFFFF;
                        onAxisLock(this, new AxisLockEventArgs(max_value, mode, m_map_name, m_filename));
                        break;
                }
            }
            else
            {
                Console.WriteLine("onAxisLock not registered");
            }
        }

        private void CastSelectEvent(int rowhandle, int colindex)
        {
            if (onSelectionChanged != null)
            {
                // haal eerst de data uit de tabel van de gridview
                onSelectionChanged(this, new CellSelectionChangedEventArgs(rowhandle, colindex, m_map_name));
            }
            else
            {
                Console.WriteLine("onSelectionChanged not registered!");
            }

        }

        public void SelectCell(int rowhandle, int colindex)
        {
            try
            {
                m_prohibitcellchange = true;
                gridControl1.ClearSelection();
                gridControl1.Rows[rowhandle].Cells[colindex].Selected = true;
                m_prohibitcellchange = false;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        public void SetSplitter(int panel1height, int panel2height, int splitdistance, bool panel1collapsed, bool panel2collapsed)
        {
            try
            {
                m_prohibitsplitchange = true;
                if (panel1collapsed)
                {
                    splitContainer1.Panel1Collapsed = true;
                    splitContainer1.Panel2Collapsed = false;
                }
                else if (panel2collapsed)
                {
                    splitContainer1.Panel2Collapsed = true;
                    splitContainer1.Panel1Collapsed = false;
                }
                else
                {
                    splitContainer1.Panel2Collapsed = false;
                    splitContainer1.Panel1Collapsed = false;

                    splitContainer1.SplitterDistance = splitdistance;
                }

                m_prohibitsplitchange = false;
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }


        private void CastSaveEvent()
        {
            if (onSymbolSave != null)
            {
                // haal eerst de data uit de tabel van de gridview
                byte[] mutateddata = GetDataFromGridView(m_isUpsideDown);
                onSymbolSave(this, new SaveSymbolEventArgs(m_map_address, m_map_length, mutateddata, m_map_name, Filename));
                m_datasourceMutated = false;
            }
            else
            {
                Console.WriteLine("onSymbolSave not registered!");
            }

        }

        private void CastSplitterMovedEvent()
        {
            if (onSplitterMoved != null)
            {
                // haal eerst de data uit de tabel van de gridview
                if (!m_prohibitsplitchange)
                {
                    onSplitterMoved(this, new SplitterMovedEventArgs(splitContainer1.Panel1.Height, splitContainer1.Panel2.Height,splitContainer1.SplitterDistance ,splitContainer1.Panel1Collapsed, splitContainer1.Panel2Collapsed, m_map_name));
                }
            }
            else
            {
                Console.WriteLine("onSplitterMoved not registered!");
            }

        }



        private void CastCloseEvent()
        {
            if (onClose != null)
            {
                onClose(this, EventArgs.Empty);
            }
        }

        

        


        
        

        private void groupControl2_Paint(object sender, PaintEventArgs e)
        {

        }


        private void StartSingleLineGraphTimer() => DebounceTimer(timer3);
        private void StartChartUpdateTimer() => DebounceTimer(timer1);
        private void StartSurfaceChartUpdateTimer() => DebounceTimer(timer2);

        private void DebounceTimer(Timer timer)
        {
            timer.Stop();
            timer.Start();
        }

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.T && nChartControl1.Visible)
            {
                btnToggleTooltips_Click(this, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            // Delegate to MapGridComponent for key handling (Add/Subtract/PageUp/PageDown/Home/End)
            _mapGridComponent.GridView1_KeyDown(sender, e);
        }



        internal void ReShowTable()
        {
            ShowTable(m_TableWidth, m_issixteenbit);
        }

        private void chartControl1_Click(object sender, EventArgs e)
        {

        }

        private void trackBarControl1_ValueChanged(object sender, EventArgs e)
        {
            if (!m_trackbarBlocked)
            {
                if (m_TableWidth > 1)
                {
                    UpdateChartControlSlice(GetDataFromGridView(false));
                    timer4.Enabled = false;
                    CastSliderMoveEvent();
                }
            }
        }


        private void chartControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void chartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_isDragging = true;
                timer4.Enabled = true;
                _mouse_drag_x = e.X;
                _mouse_drag_y = e.Y;
                TooltipService.Hide();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateChartControlSlice(m_map_content);
            timer1.Enabled = false;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            Update2DChartWithComponent(m_map_content);
            timer3.Enabled = false;
        }



        private void CastSurfaceGraphChangedEvent(int Pov_x, int Pov_y, int Pov_z, int Pan_x, int Pan_y, double Pov_d)
        {
            if (onSurfaceGraphViewChanged != null)
            {
                if (!m_prohibitgraphchange)
                {
                    onSurfaceGraphViewChanged(this, new SurfaceGraphViewChangedEventArgs(Pov_x, Pov_y, Pov_z, Pan_x, Pan_y, Pov_d, m_map_name));
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            RefreshMeshGraph();
        }

        public void SetSelectedTabPageIndex(int tabpageindex)
        {
            xtraTabControl1.SelectedIndex = tabpageindex;
            Invalidate();
        }

        private void CastViewTypeChangedEvent()
        {
            if (onViewTypeChanged != null)
            {
                onViewTypeChanged(this, new ViewTypeChangedEventArgs(m_viewtype, m_map_name));
                m_previousviewtype = m_viewtype;
            }
        }

        private void CastGraphSelectionChangedEvent()
        {
            if (onGraphSelectionChanged != null)
            {
                onGraphSelectionChanged(this, new GraphSelectionChangedEventArgs(xtraTabControl1.SelectedIndex, m_map_name));
            }
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, EventArgs e)
        {
            if (xtraTabControl1.SelectedPage == xtraTabPage1)
            {
                // 3d graph
                if (nChartControl1 != null)
                {
                    nChartControl1.Visible = true;
                    nChartControl1.BringToFront();
                    
                    // Ensure overlay buttons are on top of the 3D control
                    if (simpleButton7 != null) simpleButton7.BringToFront();
                    if (simpleButton6 != null) simpleButton6.BringToFront();
                    if (simpleButton4 != null) simpleButton4.BringToFront();
                    if (simpleButton5 != null) simpleButton5.BringToFront();
                    if (btnToggleWireframe != null) btnToggleWireframe.BringToFront();
                    if (btnToggleTooltips != null) btnToggleTooltips.BringToFront();
                    if (btnToggleOverlay != null && btnToggleOverlay.Visible) btnToggleOverlay.BringToFront();
                }
                RefreshMeshGraph();
            }
            else
            {
                UpdateChartControlSlice(GetDataFromGridView(false));
            }
            CastGraphSelectionChangedEvent();
        }


        private void chartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            m_isDragging = false;
            timer4.Enabled = false;
        }


        private void chartControl1_MouseMove(object sender, MouseEventArgs e)
        {
        }


        private void groupControl1_DoubleClick(object sender, EventArgs e)
        {
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            // Zoom In: increase zoom value (decreases camera distance)
            if (_chart3DComponent != null)
            {
                float rotation, elevation, zoom;
                _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                zoom *= 1.15f;
                // Clamp zoom between 0.5 and 5.0 (verified from Chart3DComponent line 308)
                zoom = Math.Max(0.5f, Math.Min(5.0f, zoom));
                _chart3DComponent.SetView(rotation, elevation, zoom);
            }
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            // Zoom Out: decrease zoom value (increases camera distance)
            if (_chart3DComponent != null)
            {
                float rotation, elevation, zoom;
                _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                zoom *= 0.85f;
                // Clamp zoom between 0.5 and 5.0 (verified from Chart3DComponent line 308)
                zoom = Math.Max(0.5f, Math.Min(5.0f, zoom));
                _chart3DComponent.SetView(rotation, elevation, zoom);
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            // Rotate Left: decrease rotation by 15 degrees
            if (_chart3DComponent != null)
            {
                float rotation, elevation, zoom;
                _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                rotation -= 15f; // Rotate counter-clockwise
                _chart3DComponent.SetView(rotation, elevation, zoom);
            }
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            // Rotate Right: increase rotation by 15 degrees
            if (_chart3DComponent != null)
            {
                float rotation, elevation, zoom;
                _chart3DComponent.GetView(out rotation, out elevation, out zoom);
                rotation += 15f; // Rotate clockwise
                _chart3DComponent.SetView(rotation, elevation, zoom);
            }
        }

        private void gridView1_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void MapViewerCellEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is KryptonTextBox)
            {
                KryptonTextBox txtedit = (KryptonTextBox)sender;
                if (e.KeyCode == Keys.Add)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;

                    if (m_viewtype == ViewType.Hexadecimal)
                    {
                        int value = Convert.ToInt32(txtedit.Text, 16);
                        value++;
                        if (value > m_MaxValueInTable) m_MaxValueInTable = value;
                        if (m_issixteenbit)
                        {
                            if (value > 0xFFFF) value = 0xFFFF;
                            txtedit.Text = value.ToString("X4");
                        }
                        else
                        {

                            if (value > 0xFF) value = 0xFF;
                            txtedit.Text = value.ToString("X2");
                        }
                    }
                    else
                    {
                        int value = Convert.ToInt32(txtedit.Text);
                        value++;
                        if (value > m_MaxValueInTable) m_MaxValueInTable = value;
                        if (m_issixteenbit)
                        {
                            if (value > 0xFFFF) value = 0xFFFF;
                            txtedit.Text = value.ToString();
                        }
                        else
                        {
                            if (value > 0xFF) value = 0xFF;
                            txtedit.Text = value.ToString();
                        }

                    }

                }
                else if (e.KeyCode == Keys.Subtract)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                    if (m_viewtype == ViewType.Hexadecimal)
                    {
                        int value = Convert.ToInt32(txtedit.Text, 16);
                        value--;
                        if (value < 0) value = 0;
                        if (m_issixteenbit)
                        {
                            txtedit.Text = value.ToString("X4");
                        }
                        else
                        {
                            txtedit.Text = value.ToString("X2");
                        }
                    }
                    else
                    {
                        int value = Convert.ToInt32(txtedit.Text);
                        value--;
                        if (value < 0) value = 0;
                        if (m_issixteenbit)
                        {
                            txtedit.Text = value.ToString();
                        }
                        else
                        {
                            txtedit.Text = value.ToString();
                        }

                    }

                }
            }

        }

        private void CopySelectionToClipboard()
        {
            try
            {
                DataGridViewSelectedCellCollection cellcollection = gridControl1.SelectedCells;
                
                // Prepare cells array in format expected by ClipboardService: [colIndex, rowHandle, value, ...]
                object[] cells = new object[cellcollection.Count * 3];
                for (int i = 0; i < cellcollection.Count; i++)
                {
                    DataGridViewCell gc = cellcollection[i];
                    int colIndex = gc.ColumnIndex;
                    int rowHandle = gc.RowIndex;
                    object o = gc.Value;
                    int value = _dataConversionService.ParseValue(o?.ToString() ?? "0", m_viewtype);
                    
                    cells[i * 3] = colIndex;
                    cells[i * 3 + 1] = rowHandle;
                    cells[i * 3 + 2] = value;
                }
                
                // Use the refactored ClipboardService
                _clipboardService.CopySelection(cells, m_viewtype);
            }
            catch (Exception E)
            {
                Console.WriteLine("CopySelectionToClipboard: " + E.Message);
            }
        }

        private void CopyMapToClipboard()
        {
            gridControl1.SelectAll();
            CopySelectionToClipboard();
            gridControl1.ClearSelection();
        }

        private void copySelectedCellsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedCellCollection cellcollection = gridControl1.SelectedCells;
            if (cellcollection.Count > 0)
            {
                CopySelectionToClipboard();
            }
            else
            {
                if (MessageBox.Show("No selection, copy the entire map?", "Question", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    CopyMapToClipboard();
                }
            }
        }

        private void atCurrentlySelectedLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedCellCollection cellcollection = gridControl1.SelectedCells;
            if (cellcollection.Count >= 1)
            {
                try
                {
                    int rowhandlefrom = cellcollection[0].RowIndex;
                    int colindexfrom = cellcollection[0].ColumnIndex;
                    
                    // Prepare target cells array for ClipboardService
                    object[] targetCells = new object[3];
                    targetCells[0] = rowhandlefrom;
                    targetCells[1] = colindexfrom;
                    targetCells[2] = new System.Collections.Generic.List<PasteCellInfo>();
                    
                    // Use the refactored ClipboardService
                    _clipboardService.PasteAtCurrentLocation(targetCells, m_viewtype);
                    
                    // Apply the paste results
                    var pasteList = (System.Collections.Generic.List<PasteCellInfo>)targetCells[2];
                    foreach (var pasteInfo in pasteList)
                    {
                        int val = _dataConversionService.ParseValue(pasteInfo.Value.ToString(), m_viewtype);
                        gridControl1.Rows[pasteInfo.Row].Cells[pasteInfo.Column].Value = _dataConversionService.FormatValue(val, m_viewtype, m_issixteenbit);
                    }
                }
                catch (Exception pasteE)
                {
                    Console.WriteLine(pasteE.Message);
                }
            }
        }

        private void inOrgininalPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Use the refactored ClipboardService
            object[] targetCells = new object[3];
            targetCells[0] = 0;
            targetCells[1] = 0;
            targetCells[2] = new System.Collections.Generic.List<PasteCellInfo>();
            
            _clipboardService.PasteAtOriginalPosition(targetCells, m_viewtype);
            
            // Apply the paste results
            var pasteList = (System.Collections.Generic.List<PasteCellInfo>)targetCells[2];
            foreach (var pasteInfo in pasteList)
            {
                int val = _dataConversionService.ParseValue(pasteInfo.Value.ToString(), m_viewtype);
                gridControl1.Rows[pasteInfo.Row].Cells[pasteInfo.Column].Value = _dataConversionService.FormatValue(val, m_viewtype, m_issixteenbit);
            }
        }

        private void groupControl1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (m_viewtype != ViewType.Hexadecimal) m_viewtype = ViewType.Hexadecimal;
            else m_viewtype = m_previousviewtype;
            ShowTable(m_TableWidth, m_issixteenbit);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                double workValue = Convert.ToDouble(toolStripTextBox1.Text);
                DataGridViewSelectedCellCollection selectedCells = gridControl1.SelectedCells;
                if (selectedCells.Count > 0)
                {
                    OperationType opType = (OperationType)toolStripComboBox1.SelectedIndex;
                    
                    // Convert DataGridViewSelectedCellCollection to object array for service
                    object[] cells = new object[selectedCells.Count];
                    for (int i = 0; i < selectedCells.Count; i++) cells[i] = selectedCells[i];

                    _mapOperationService.ApplyOperation(
                        CreateMapViewerState(),
                        opType,
                        workValue,
                        cells,
                        (rh, col) => gridControl1.Rows[rh].Cells[Convert.ToInt32(col)].Value,
                        (rh, col, val) => gridControl1.Rows[rh].Cells[Convert.ToInt32(col)].Value = val
                    );
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("toolStripButton3_Click: " + E.Message);
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_map_name != string.Empty && !m_prohibitlock_change)
            {
                switch (toolStripComboBox2.SelectedIndex)
                {
                    case 0: // autoscale
                        CastLockEvent(0);
                        break;
                    case 1: // peak values
                        CastLockEvent(1);
                        break;
                    case 2: // max possible
                        CastLockEvent(2);
                        break;
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            _controller.HandleDockingAction(this.Parent,
                () => {
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                },
                () => {
                    splitContainer1.Panel1Collapsed = true;
                    splitContainer1.Panel2Collapsed = false;
                });
            CastSplitterMovedEvent();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            _controller.HandleDockingAction(this.Parent,
                () => {
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                },
                () => {
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = true;
                });
            CastSplitterMovedEvent();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            _controller.HandleDockingAction(this.Parent,
                () => {
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                },
                () => {
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                });
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (splitContainer1.Panel1Collapsed)
            {
                splitContainer1.Panel1Collapsed = false;
                splitContainer1.Panel2Collapsed = true;
            }
            else if (splitContainer1.Panel2Collapsed)
            {
                splitContainer1.Panel1Collapsed = false;
                splitContainer1.Panel2Collapsed = false;
            }
            else
            {
                splitContainer1.Panel1Collapsed = true;
                splitContainer1.Panel2Collapsed = false;
            }
            CastSplitterMovedEvent();
        }

        bool tmr_toggle = false;

        private void timer4_Tick(object sender, EventArgs e)
        {
            // Logic for highlighting grid cells from graph selection removed
            // as it was tied to DevExpress SeriesPoint.
        }

        private void popupContainerEdit1_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.ConvertEditValueEventArgs e)
        {
//            e.Value = System.IO.Path.GetFileName(m_filename) + " : " + m_map_name + " flash address : " + m_map_address.ToString("X6") + " sram address : " + m_map_sramaddress.ToString("X4");
        }

        private void gridView1_SelectionChanged_1(object sender, EventArgs e)
        {
            if (!m_prohibitcellchange)
            {
                DataGridViewSelectedCellCollection cellcollection = gridControl1.SelectedCells;
                if (cellcollection.Count == 1)
                {
                    DataGridViewCell cell = cellcollection[0];
                    CastSelectEvent(cell.RowIndex, cell.ColumnIndex);
                }
                // Force repaint to show selection highlight when dragging or clicking
                gridControl1.Invalidate();
            }
            
        }

        private bool m_split_dragging = false;

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (m_split_dragging)
            {
                m_split_dragging = false;
                Console.WriteLine("Splitter moved: " + splitContainer1.Panel1.Height.ToString() + ":" + splitContainer1.Panel2.Height.ToString() + splitContainer1.Panel1Collapsed.ToString() + ":" + splitContainer1.Panel2Collapsed.ToString());
                CastSplitterMovedEvent();
            }
        }

        private void splitContainer1_MouseDown(object sender, MouseEventArgs e)
        {
            m_split_dragging = true;
        }

        private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
        {
         
        }

        private void splitContainer1_MouseLeave(object sender, EventArgs e)
        {

        }



        public void SetSurfaceGraphViewEx(float depthx, float depthy, float zoom, float rotation, float elevation)
        {
            if (_chart3DComponent != null)
            {
                _chart3DComponent.SetView(rotation, elevation, zoom);
            }
        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_prohibit_viewchange) return;
            m_viewtype = _controller.GetViewTypeFromIndex(toolStripComboBox3.SelectedIndex);
            ReShowTable();
            CastViewTypeChangedEvent();
        }

        private void gridView1_CellValueChanging(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void gridView1_ValidatingEditor(object sender, DataGridViewCellValidatingEventArgs e)
        {
            string errorText;
            object validatedValue;
            if (!_mapValidationService.ValidateEditorValue(e.FormattedValue, CreateMapViewerState(), out errorText, out validatedValue))
            {
                e.Cancel = true;
                gridControl1.Rows[e.RowIndex].ErrorText = errorText;
            }
            else
            {
                gridControl1.Rows[e.RowIndex].ErrorText = "";
            }
        }

        private void popupContainerEdit1_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
        {
            e.DisplayText = System.IO.Path.GetFileName(m_filename) + " : " + m_map_name + " flash address : " + m_map_address.ToString("X6") + " sram address : " + m_map_sramaddress.ToString("X4");
        }

        private float ConvertToEasyValue(float editorvalue)
        {
            float retval = editorvalue;
            if (m_viewtype == ViewType.Easy )
            {
                retval = (float)((float)editorvalue * (float)correction_factor) + (float)correction_offset;
            }
            return retval;
        }

        private void gridView1_ShownEditor(object sender, EventArgs e)
        {
        }

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
        }

        private void gridView1_HiddenEditor(object sender, EventArgs e)
        {
        }

        private void MapViewer_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
            }
        }

        internal void ClearSelection()
        {
            gridControl1.ClearSelection();
        }

        private int m_selectedrowhandle = -1;
        private int m_selectedcolumnindex = -1;


        public void HighlightCell(int tpsindex, int rpmindex)
        {
            try
            {
                int numberofrows = m_map_content.Length / m_TableWidth;
                if (m_issixteenbit)
                {
                    numberofrows /= 2;
                }
                m_selectedrowhandle = (numberofrows - 1) - rpmindex;
                m_selectedcolumnindex = tpsindex;
 
                if (m_selectedrowhandle > numberofrows) m_selectedrowhandle = numberofrows;
                if (m_selectedcolumnindex > m_TableWidth) m_selectedcolumnindex = m_TableWidth;
 
                gridControl1.Invalidate();
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            SymbolAxesTranslator sat = new SymbolAxesTranslator();
            string x = string.Empty;
            if (x_axisvalues.Length > 0 && x_axisAddress > 0)
            {
                x = X_axis_name + " (" + x_axisAddress.ToString("X8") + ")";
            }

            string y = string.Empty;
            if (y_axisvalues.Length > 0 && y_axisAddress > 0)
            {
                y = Y_axis_name + " (" + y_axisAddress.ToString("X8") + ")";
            }

            if (x != string.Empty)
            {
                editXaxisSymbolToolStripMenuItem.Enabled = true;
                editXaxisSymbolToolStripMenuItem.Text = "Edit x-axis (" + x + ")";
            }
            else
            {
                editXaxisSymbolToolStripMenuItem.Enabled = false;
                editXaxisSymbolToolStripMenuItem.Text = "Edit x-axis";
            }
            if (y != string.Empty)
            {
                editYaxisSymbolToolStripMenuItem.Enabled = true;
                editYaxisSymbolToolStripMenuItem.Text = "Edit y-axis (" + y + ")";
            }
            else
            {
                editYaxisSymbolToolStripMenuItem.Enabled = false;
                editYaxisSymbolToolStripMenuItem.Text = "Edit y-axis";
            }
        }

        private void editXaxisSymbolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastEditXaxisEditorRequested();
        }

        private void CastEditXaxisEditorRequested()
        {
            if (onAxisEditorRequested != null)
            {
                onAxisEditorRequested(this, new AxisEditorRequestedEventArgs(AxisIdent.X_Axis, m_map_name, m_filename));
            }
        }

        private void editYaxisSymbolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CastEditYaxisEditorRequested();
        }

        private void CastEditYaxisEditorRequested()
        {
            if (onAxisEditorRequested != null)
            {
                onAxisEditorRequested(this, new AxisEditorRequestedEventArgs(AxisIdent.Y_Axis, m_map_name, m_filename));
            }
        }

        private void CastReadFromSRAM()
        {
            if (onReadFromSRAM != null)
            {
                onReadFromSRAM(this, new ReadFromSRAMEventArgs(m_map_name));
            }
        }

        private void CastWriteToSRAM()
        {
            if (onWriteToSRAM != null)
            {
                onWriteToSRAM(this, new WriteToSRAMEventArgs(m_map_name, GetDataFromGridView(m_isUpsideDown)));
            }
        }

        private void smoothSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_viewtype == ViewType.Hexadecimal)
            {
                MessageBox.Show("Smoothing cannot be done in Hex view!");
                return;
            }
            DataGridViewSelectedCellCollection cellcollection = gridControl1.SelectedCells;
            if (cellcollection.Count > 2)
            {
                // Use the refactored SmoothingService for proportional smoothing
                object[] cells = new object[cellcollection.Count];
                for (int i = 0; i < cellcollection.Count; i++)
                {
                    cells[i] = cellcollection[i];
                }
                
                _smoothingService.SmoothProportional(cells, gridControl1, x_axisvalues, y_axisvalues);
            }
        }

        private void smoothSelectionToolProportionalStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_viewtype == ViewType.Hexadecimal)
            {
                MessageBox.Show("Smoothing cannot be done in Hex view!");
                return;
            }
            DataGridViewSelectedCellCollection cellcollection = gridControl1.SelectedCells;
            if (cellcollection.Count > 2)
            {
                object[] cells = new object[cellcollection.Count];
                for (int i = 0; i < cellcollection.Count; i++)
                {
                    cells[i] = cellcollection[i];
                }
                _smoothingService.SmoothInterpolated(cells, gridControl1, x_axisvalues, y_axisvalues);
                m_datasourceMutated = true;
                simpleButton2.Enabled = true;
                simpleButton3.Enabled = true;
            }
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            CastReadFromSRAM();
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            CastWriteToSRAM();
        }

        private void btnToggleWireframe_Click(object sender, EventArgs e)
        {
            // Toggle Wireframe Mode
            if (_chart3DComponent != null)
            {
                _chart3DComponent.ToggleRenderMode();
                UpdateMeshButtonHighlights();
            }
        }

        private void btnToggleTooltips_Click(object sender, EventArgs e)
        {
            if (_chart3DComponent != null)
            {
                _chart3DComponent.ToggleTooltips();
                UpdateMeshButtonHighlights();
            }
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            timer5.Enabled = false;
            try
            {
                if (!m_datasourceMutated)
                {
                    CastReadFromSRAM(); // only if no changed made to gridView
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
            timer5.Enabled = true;

        }

        private void toolStripComboBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // see if there's a selection being made
                gridControl1.ClearSelection();
                string strValue = toolStripComboBox3.Text;
                char[] sep = new char[1];
                sep.SetValue(' ', 0);
                string[] strValues = strValue.Split(sep);
                foreach (string strval in strValues)
                {
                    double dblres;
                    if (Double.TryParse(strval, out dblres))
                    {
                        SelectCellsWithValue(dblres);
                    }
                }
                gridControl1.Focus();

            }
        }

        private void SelectCellsWithValue(double value)
        {
            for (int rh = 0; rh < gridControl1.RowCount; rh++)
            {
                for (int ch = 0; ch < gridControl1.Columns.Count; ch++)
                {
                    try
                    {
                        object ov = gridControl1.Rows[rh].Cells[ch].Value;
                        if (ov == null) continue;

                        int rawVal = _dataConversionService.ParseValue(ov.ToString(), m_viewtype);
                        double val = _dataConversionService.ApplyCorrection(rawVal, correction_factor, correction_offset);
                        
                        double diff = Math.Abs(val - value);
                        if (diff < 0.009)
                        {
                            gridControl1.Rows[rh].Cells[ch].Selected = true;
                        }
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine("Failed to select cell: " + E.Message);
                    }
                }
            }
        }

        private void CastReadEvent()
        {
            if (onSymbolRead != null)
            {
                onSymbolRead(this, new ReadSymbolEventArgs(m_map_name, m_filename));
                m_datasourceMutated = false;
            }
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {
            CastReadEvent();
        }

    }
}
