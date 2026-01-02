using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using System.Runtime.InteropServices;
using Nevron.Chart;
using Nevron.GraphicsCore;
using Nevron.Chart.WinForm;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraBars.Docking;
using System.IO;
using System.Globalization;
using VAGSuite.MapViewerEventArgs;
using VAGSuite.Models;
using VAGSuite.Services;
using VAGSuite.Components;

namespace VAGSuite
{
    public partial class MapViewerEx : DevExpress.XtraEditors.XtraUserControl //IMapViewer
    {
        // Phase 3 Services - Business Logic (initialized in constructor for backward compatibility)
        private IMapRenderingService _mapRenderingService;
        private IChartService _chartService;
        private IClipboardService _clipboardService;
        private ISmoothingService _smoothingService;
        private IDataConversionService _dataConversionService;
        
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
                gridView1.PaintStyleName = "UltraFlat";
                gridView1.Appearance.Row.Font = new Font("Tahoma", 8);
                this.Font = new Font("Tahoma", 8);
                gridView1.Appearance.Row.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                gridView1.Appearance.Row.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            }
            else if (vs == ViewSize.ExtraSmallView)
            {
                gridView1.PaintStyleName = "UltraFlat";
                gridView1.Appearance.Row.Font = new Font("Tahoma", 7);
                this.Font = new Font("Tahoma", 7);
                gridView1.Appearance.Row.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
                gridView1.Appearance.Row.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
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

        private void ShowHitInfo(GridHitInfo hi)
        {
            if (hi.InRowCell)
            {

                if (afr_counter != null)
                {
                    // fetch correct counter
                    int current_afrcounter = (int)afr_counter[(afr_counter.Length - ((hi.RowHandle + 1) * m_TableWidth)) + hi.Column.AbsoluteIndex];
                    // show number of measurements in balloon
                    string detailline = "# measurements: " + current_afrcounter.ToString();
                    toolTipController1.ShowHint(detailline, "Information", Cursor.Position);
                }
            }
            else
            {
                toolTipController1.HideHint();
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
                gridView1.OptionsView.ColumnAutoWidth = value;
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
                    gridView1.OptionsBehavior.Editable = false; // don't let the user edit a compare viewer
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
            get { return (int)trackBarControl1.EditValue; }
            set { trackBarControl1.EditValue = value; }
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

        private void FillData(NMeshSurfaceSeries surface)
        {
            DataTable dt = (DataTable)gridControl1.DataSource;
            if (dt == null) return;
            _chartService.FillSurfaceData(surface, dt, m_viewtype, correction_factor, correction_offset, _isCompareViewer);
        }

        private void FillDataOriginal(NMeshSurfaceSeries surface)
        {
            DataTable dt = (DataTable)gridControl1.DataSource;
            if (dt == null || m_map_original_content == null) return;
            _chartService.FillOriginalData(surface, dt, m_map_original_content, m_issixteenbit, m_viewtype, correction_factor, correction_offset);
        }

        private void FillDataCompare(NMeshSurfaceSeries surface)
        {
            DataTable dt = (DataTable)gridControl1.DataSource;
            if (dt == null || m_map_compare_content == null) return;
            _chartService.FillCompareData(surface, dt, m_map_compare_content, m_issixteenbit, m_viewtype, correction_factor, correction_offset);
        }

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
            groupControl1.Text = "X: " + m_x_axis_name + " Y: " + m_y_axis_name + " Z: " + m_z_axis_name;
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
            InitializeComponent();
            toolStripComboBox1.SelectedIndex = 0;
            toolStripComboBox2.SelectedIndex = 0;

            nChartControl1.MouseWheel += new MouseEventHandler(nChartControl1_MouseWheel);
            nChartControl1.MouseDown += new MouseEventHandler(nChartControl1_MouseDown);
            nChartControl1.MouseUp += new MouseEventHandler(nChartControl1_MouseUp);

            gridView1.MouseMove += new MouseEventHandler(gridView1_MouseMove);
            

            // Initialize Phase 3 Services with default implementations
            _mapRenderingService = new MapRenderingService();
            _chartService = new ChartService();
            _dataConversionService = new DataConversionService();
            _clipboardService = new ClipboardService(_dataConversionService);
            _smoothingService = new SmoothingService(_dataConversionService);
            
            // Initialize Phase 5 Components
            InitializePhase5Components();
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
                    YAxisAddress = y_axisAddress
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
            // Create component instances
            _mapGridComponent = new MapGridComponent(_dataConversionService, _mapRenderingService);
            _chart3DComponent = new Chart3DComponent(_chartService);
            _chart2DComponent = new Chart2DComponent(_chartService);
            
            // Wire up the external chart control to the component
            _chart3DComponent.SetChartControl(nChartControl1);
            _chart2DComponent.SetChartControl(nChartControl2);
            
            // Wire up component events to MapViewerEx handlers
            _chart3DComponent.ViewChanged += OnChart3DViewChanged;
        }

        /// <summary>
        /// Uses MapGridComponent to render cell with proper coloring and formatting.
        /// This method delegates to the component for rendering logic while keeping control in MapViewerEx.
        /// </summary>
        private void RenderCellWithComponent(DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e, int cellValue)
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
                SolidBrush sb = new SolidBrush(cellColor);
                e.Graphics.FillRectangle(sb, e.Bounds);
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
            object cellValue = gridView1.GetRowCellValue(rowHandle, gridView1.Columns[columnIndex]);
            if (cellValue == null) return 0;
            
            if (m_viewtype == ViewType.Hexadecimal)
            {
                return Convert.ToInt32(cellValue.ToString(), 16);
            }
            else
            {
                return Convert.ToInt32(ConvertToDouble(cellValue.ToString()));
            }
        }

        /// <summary>
        /// Sets cell value in grid using component patterns.
        /// </summary>
        private void SetCellValueInGrid(int rowHandle, int columnIndex, object value)
        {
            gridView1.SetRowCellValue(rowHandle, gridView1.Columns[columnIndex], value);
        }

        private void OnChart3DViewChanged(object sender, SurfaceGraphViewChangedEventArgsEx e)
        {
            CastSurfaceGraphChangedEventEx(e.DepthX, e.DepthY, e.Zoom, e.Rotation, e.Elevation);
        }

        void gridView1_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_map_name == "TargetAFR" || m_map_name == "FeedbackAFR" || m_map_name == "FeedbackvsTargetAFR")
            {
                ShowHitInfo(gridView1.CalcHitInfo(new Point(e.X, e.Y)));
            }
        }

        void nChartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                nChartControl1.Controller.Tools.Clear();
                NDragTool dragTool = null;
                dragTool = new NTrackballTool();
                nChartControl1.Controller.Tools.Add(dragTool);
            }
            //<GS-07062010>
            CastSurfaceGraphChangedEventEx(nChartControl1.Charts[0].Projection.XDepth, nChartControl1.Charts[0].Projection.YDepth, nChartControl1.Charts[0].Projection.Zoom, nChartControl1.Charts[0].Projection.Rotation, nChartControl1.Charts[0].Projection.Elevation);


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
        }

        void nChartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                nChartControl1.Controller.Tools.Clear();
                NDragTool dragTool = null;
                dragTool = new NOffsetTool();
                nChartControl1.Controller.Tools.Add(dragTool);
                //dragTool.BeginDragMouseCommand.MouseButton = MouseButtons.Right;
                //dragTool.EndDragMouseCommand.MouseButton = MouseButtons.Right;
            }

        }

        void nChartControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            // wieltje wordt zoomin/out
            if (e.Delta > 0)
            {
                nChartControl1.Charts[0].Projection.Zoom += 5;
                nChartControl1.Refresh();
            }
            else
            {
                nChartControl1.Charts[0].Projection.Zoom -= 5;
                nChartControl1.Refresh();
            }
            //<GS-07062010>
            CastSurfaceGraphChangedEventEx(nChartControl1.Charts[0].Projection.XDepth, nChartControl1.Charts[0].Projection.YDepth, nChartControl1.Charts[0].Projection.Zoom, nChartControl1.Charts[0].Projection.Rotation, nChartControl1.Charts[0].Projection.Elevation);
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
            m_TableWidth = tablewidth;
            m_issixteenbit = issixteenbits;

            if (m_map_length != 0 && m_map_name != string.Empty)
            {
                MapViewerState state = CreateMapViewerState();
                DataTable dt = _dataConversionService.ConvertToDataTable(state.Data, state.Configuration);
                
                // Update MaxValueInTable and Real Min/Max from the data
                m_MaxValueInTable = 0;
                m_realMaxValue = double.MinValue;
                m_realMinValue = double.MaxValue;

                foreach (DataRow row in dt.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        int val = _dataConversionService.ParseValue(item.ToString(), m_viewtype);
                        if (val > m_MaxValueInTable) m_MaxValueInTable = val;
                        
                        double realVal = _dataConversionService.ApplyCorrection(val, correction_factor, _isCompareViewer ? 0 : correction_offset);
                        if (realVal > m_realMaxValue) m_realMaxValue = realVal;
                        if (realVal < m_realMinValue) m_realMinValue = realVal;
                    }
                }

                gridControl1.DataSource = dt;

                if (!gridView1.OptionsView.ColumnAutoWidth)
                {
                    for (int c = 0; c < gridView1.Columns.Count; c++)
                    {
                        gridView1.Columns[c].Width = 40;
                    }
                }

                // set axis indicator width
                int indicatorwidth = -1;
                using (Graphics g = gridControl1.CreateGraphics())
                {
                    for (int i = 0; i < y_axisvalues.Length; i++)
                    {
                        string yval = _dataConversionService.FormatValue(Convert.ToInt32(y_axisvalues.GetValue(i)), m_viewtype, true);
                        SizeF size = g.MeasureString(yval, this.Font);
                        if (size.Width > indicatorwidth) indicatorwidth = (int)size.Width;
                        m_textheight = (int)size.Height;
                    }
                }
                if (indicatorwidth > 0)
                {
                    gridView1.IndicatorWidth = indicatorwidth + 6;
                }

                int maxxval = 0;
                for (int i = 0; i < x_axisvalues.Length; i++)
                {
                    int xval = Convert.ToInt32(x_axisvalues.GetValue(i));
                    if (xval > 0xF000)
                    {
                        xval = 0x10000 - xval;
                        xval = -xval;
                        x_axisvalues.SetValue(xval, i);
                    }
                    if (xval > maxxval) maxxval = xval;
                }
                if (maxxval <= 255) m_xformatstringforhex = "X2";
            }

            if (m_TableWidth > 1)
            {
                xtraTabControl1.SelectedTabPage = xtraTabPage1;
               
                SetViewTypeParams(m_vs);
                trackBarControl1.Properties.Minimum = 0;
                trackBarControl1.Properties.Maximum = x_axisvalues.Length - 1;
                labelControl8.Text = X_axis_name + " values";
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
                xtraTabControl1.SelectedTabPage = xtraTabPage2;
                
                SetViewTypeParams(m_vs);
                trackBarControl1.Properties.Minimum = 0;
                trackBarControl1.Properties.Maximum = x_axisvalues.Length - 1;
                labelControl8.Text = X_axis_name + " values";
                /*** end test ***/
                trackBarControl1.Properties.Minimum = 0;
                trackBarControl1.Properties.Maximum = 0;
                trackBarControl1.Enabled = false;
                labelControl8.Text = X_axis_name;

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
                _chart2DComponent.RefreshChart(state);
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

        private double ConvertToDouble(string v)
        {
            double d = 0;
            string newv = string.Empty;
            foreach (char c in v)
            {
                if (c == ',' || c == '.') newv += c;
                else if (c >= 0x30 && c <= 0x39) newv += c;
                else if (c == 'A' || c == 'a') newv += c;
                else if (c == 'B' || c == 'b') newv += c;
                else if (c == 'C' || c == 'c') newv += c;
                else if (c == 'D' || c == 'd') newv += c;
                else if (c == 'E' || c == 'e') newv += c;
                else if (c == 'F' || c == 'f') newv += c;
            }
            if (newv == "") return d;
            string vs = "";
            vs = newv.Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            Double.TryParse(vs, out d);
            return d;
        }



        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            try
            {
                if (e.CellValue == null || e.CellValue == DBNull.Value) return;

                int cellValue = _dataConversionService.ParseValue(e.CellValue.ToString(), m_viewtype);

                // Use IMapRenderingService for color calculation
                Color cellColor = _mapRenderingService.CalculateCellColor(
                    cellValue,
                    m_MaxValueInTable,
                    m_OnlineMode,
                    m_isRedWhite);

                // Apply coloring if not disabled
                if (!m_disablecolors)
                {
                    using (SolidBrush sb = new SolidBrush(cellColor))
                    {
                        e.Graphics.FillRectangle(sb, e.Bounds);
                    }
                }

                // Use IDataConversionService for value formatting
                string displayText = _dataConversionService.FormatValue(cellValue, m_viewtype, m_issixteenbit);

                // Apply correction factor and offset if needed
                if (m_viewtype == ViewType.Easy && (correction_offset != 0 || correction_factor != 1))
                {
                    double correctedValue = _dataConversionService.ApplyCorrection(cellValue, correction_factor, correction_offset);

                    if (m_map_name.StartsWith("Injector duration") || m_map_name.StartsWith("Start of injection"))
                    {
                        e.DisplayText = correctedValue.ToString("F1") + "\u00b0";
                    }
                    else if (m_map_name.StartsWith("N75"))
                    {
                        e.DisplayText = correctedValue.ToString("F0") + @"%";
                    }
                    else
                    {
                        e.DisplayText = correctedValue.ToString("F2");
                    }
                }
                else if (m_viewtype == ViewType.Easy && m_map_name.StartsWith("N75"))
                {
                    e.DisplayText = displayText + @"%";
                }
                else
                {
                    e.DisplayText = displayText;
                }

                // Open Loop Indicator logic refactored to service
                if (_mapRenderingService.ShouldShowOpenLoopIndicator(e.RowHandle, e.Column.AbsoluteIndex, open_loop, x_axisvalues, y_axisvalues, m_xaxisUnits, m_yaxisUnits))
                {
                    if (m_StandardFill == 1)
                    {
                        using (Pen p = new Pen(Brushes.Black, 2))
                        {
                            e.Graphics.DrawRectangle(p, e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2);
                        }
                    }
                    else if (m_StandardFill > 1)
                    {
                        Point[] pnts = new Point[4];
                        pnts[0] = new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y);
                        pnts[1] = new Point(e.Bounds.X + e.Bounds.Width - (e.Bounds.Height / 2), e.Bounds.Y);
                        pnts[2] = new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y + (e.Bounds.Height / 2));
                        pnts[3] = new Point(e.Bounds.X + e.Bounds.Width, e.Bounds.Y);
                        e.Graphics.FillPolygon(Brushes.SeaGreen, pnts, System.Drawing.Drawing2D.FillMode.Winding);
                    }
                }
                if (m_selectedrowhandle >= 0 && m_selectedcolumnindex >= 0)
                {
                    if (e.RowHandle == m_selectedrowhandle && e.Column.AbsoluteIndex == m_selectedcolumnindex)
                    {
                        using (SolidBrush sbsb = new SolidBrush(Color.Yellow))
                        {
                            e.Graphics.FillRectangle(sbsb, e.Bounds);
                        }
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }


        private void gridView1_RowUpdated(object sender, DevExpress.XtraGrid.Views.Base.RowObjectEventArgs e)
        {
            m_datasourceMutated = true;
            simpleButton2.Enabled = true;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            // Restore original map content
            if (m_map_original_content != null)
            {
                m_map_content = (byte[])m_map_original_content.Clone();
            }
            ShowTable(m_TableWidth, m_issixteenbit);
            m_datasourceMutated = false;
            simpleButton2.Enabled = false;
            simpleButton3.Enabled = false;
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
                onSliderMove(this, new SliderMoveEventArgs((int)trackBarControl1.EditValue, m_map_name, m_filename));
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
                gridView1.ClearSelection();
                gridView1.SelectCell(rowhandle, gridView1.Columns[colindex]);
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

        private void gridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            
            m_datasourceMutated = true;
            simpleButton2.Enabled = true;
            simpleButton3.Enabled = true; // Enable the undo button when data is mutated
            // Update m_map_content with the latest data from the grid
            m_map_content = GetDataFromGridView(m_isUpsideDown);
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
                    //   UpdateChartControlSlice(GetDataFromGridView(false));
                }
            }
        }

        private void StartSingleLineGraphTimer()
        {
            timer3.Stop();
            timer3.Start();
        }

        private void StartChartUpdateTimer()
        {
            timer1.Stop();
            timer1.Start();
        }

        private void StartSurfaceChartUpdateTimer()
        {
            timer2.Stop();
            timer2.Start();
        }

        private void gridView1_KeyDown(object sender, KeyEventArgs e)
        {
            // Delegate to MapGridComponent for key handling (Add/Subtract/PageUp/PageDown/Home/End)
            _mapGridComponent.GridView1_KeyDown(sender, e);
        }

        private void gridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.RowHandle >= 0)
            {
                try
                {
                    if (y_axisvalues != null && y_axisvalues.Length > e.RowHandle)
                    {
                        string yvalue;
                        int index = m_isUpsideDown ? (y_axisvalues.Length - 1) - e.RowHandle : e.RowHandle;
                        int rawY = Convert.ToInt32(y_axisvalues.GetValue(index));

                        if (m_viewtype == ViewType.Hexadecimal)
                        {
                            yvalue = rawY.ToString("X4");
                        }
                        else
                        {
                            yvalue = ConvertYAxisValue(rawY.ToString());
                        }

                        Rectangle r = new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2);
                        e.Graphics.DrawRectangle(Pens.LightSteelBlue, r);
                        using (var gb = new System.Drawing.Drawing2D.LinearGradientBrush(e.Bounds, e.Appearance.BackColor2, e.Appearance.BackColor2, System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                        {
                            e.Graphics.FillRectangle(gb, e.Bounds);
                        }
                        e.Graphics.DrawString(yvalue, this.Font, Brushes.MidnightBlue, new PointF(e.Bounds.X + 4, e.Bounds.Y + 1 + (e.Bounds.Height - m_textheight) / 2));
                        e.Handled = true;
                    }
                }
                catch (Exception E)
                {
                    Console.WriteLine("gridView1_CustomDrawRowIndicator: " + E.Message);
                }
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(m_yaxisUnits) || !string.IsNullOrEmpty(m_xaxisUnits))
                    {
                        using (Font f = new Font(this.Font.FontFamily, 6))
                        {
                            SizeF xsize = e.Graphics.MeasureString(m_xaxisUnits, f);
                            e.Graphics.DrawString(m_xaxisUnits, f, Brushes.Black, e.Bounds.X + e.Bounds.Width - xsize.Width - 1, e.Bounds.Y);
                            e.Graphics.DrawString(m_yaxisUnits, f, Brushes.Black, e.Bounds.X + 1, e.Bounds.Y + e.Bounds.Height / 2);
                            e.Handled = true;
                        }
                    }
                }
                catch { }
            }
        }

        private string ConvertYAxisValue(string currValue)
        {
            string retval = currValue.ToString();
            if (m_Yaxiscorrectionfactor == 1) return currValue;
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                provider = new CultureInfo("nl-NL");
                float temp = (float)Convert.ToDouble(currValue, provider);
                temp *= (float)m_Yaxiscorrectionfactor;
                temp += (float)m_Yaxiscorrectionoffset;
                retval = temp.ToString("F1");
            }
            catch (Exception E)
            {
                Console.WriteLine("MapViewer::ConvertYAxisValue: " + E.Message);
            }
            return retval;
        }
        private string ConvertXAxisValue(string currValue)
        {
            string retval = currValue.ToString();
            if (m_Xaxiscorrectionfactor == 1) return currValue;
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                provider = new CultureInfo("nl-NL");
                float temp = (float)Convert.ToDouble(currValue, provider);
                temp *= (float)m_Xaxiscorrectionfactor;
                temp += (float)m_Xaxiscorrectionoffset;

                retval = temp.ToString("F2");
            }
            catch (Exception E)
            {
                Console.WriteLine("MapViewer::ConvertXAxisValue: " + E.Message);
            }
            return retval;
        }

        private void gridView1_CustomDrawColumnHeader(object sender, DevExpress.XtraGrid.Views.Grid.ColumnHeaderCustomDrawEventArgs e)
        {
            try
            {
                if (x_axisvalues != null && e.Column != null && x_axisvalues.Length > e.Column.VisibleIndex)
                {
                    string xvalue;
                    int rawX = Convert.ToInt32(x_axisvalues.GetValue(e.Column.VisibleIndex));
                    if (m_viewtype == ViewType.Hexadecimal)
                    {
                        xvalue = rawX.ToString(m_xformatstringforhex);
                    }
                    else
                    {
                        xvalue = ConvertXAxisValue(rawX.ToString());
                    }

                    Rectangle r = new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, e.Bounds.Width - 2, e.Bounds.Height - 2);
                    e.Graphics.DrawRectangle(Pens.LightSteelBlue, r);
                    using (var gb = new System.Drawing.Drawing2D.LinearGradientBrush(e.Bounds, e.Appearance.BackColor2, e.Appearance.BackColor2, System.Drawing.Drawing2D.LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(gb, e.Bounds);
                    }
                    e.Graphics.DrawString(xvalue, this.Font, Brushes.MidnightBlue, new PointF(e.Bounds.X + 3, e.Bounds.Y + 1 + (e.Bounds.Height - m_textheight) / 2));
                    e.Handled = true;
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("gridView1_CustomDrawColumnHeader: " + E.Message);
            }
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
                    _sp_dragging = null;
                    timer4.Enabled = false;
                    CastSliderMoveEvent();
                }
            }
        }

        private void chartControl1_CustomDrawSeriesPoint(object sender, CustomDrawSeriesPointEventArgs e)
        {
            
        }

        private void chartControl1_ObjectHotTracked(object sender, HotTrackEventArgs e)
        {
            if (e.Object is Series)
            {
                Series s = (Series)e.Object;
                if (e.AdditionalObject is SeriesPoint)
                {
                    SeriesPoint sp = (SeriesPoint)e.AdditionalObject;
                    _sp_dragging = (SeriesPoint)e.AdditionalObject;
                    //timer4.Enabled = true;
                    // alleen hier selecteren, niet meer blinken
                    if (_sp_dragging != null)
                    {
                        string yaxisvalue = _sp_dragging.Argument;
                        int rowhandle = -1;
                        for (int t = 0; t < y_axisvalues.Length; t++)
                        {
                            if (y_axisvalues.GetValue(t).ToString() == yaxisvalue)
                            {
                                rowhandle = (y_axisvalues.Length - 1) - t;
                            }
                        }
                        if (m_TableWidth == 1)
                        {
                            // single column graph.. 
                            int numberofrows = m_map_length;
                            if (m_issixteenbit) numberofrows /= 2;
                            rowhandle = (numberofrows - 1) - Convert.ToInt32(yaxisvalue);
                        }
                        if (rowhandle != -1)
                        {
                            gridView1.ClearSelection();
                            gridView1.SelectCell(rowhandle, gridView1.Columns[(int)trackBarControl1.Value]);
                        }
                    }

                    string detailline = Y_axis_name + ": " + sp.Argument + Environment.NewLine + Z_axis_name + ": " + sp.Values[0].ToString();
                    if (m_map_name.StartsWith("Ign_map_0!") || m_map_name.StartsWith("Ign_map_4!")) detailline += " \u00b0";// +"C";
                    toolTipController1.ShowHint(detailline, "Details", Cursor.Position);
                }
            }
            else
            {
                toolTipController1.HideHint();
                
            }

        }

        private void chartControl1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private SeriesPoint _sp_dragging;

        private void chartControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_isDragging = true;
                timer4.Enabled = true;
                _mouse_drag_x = e.X;
                _mouse_drag_y = e.Y;
                toolTipController1.HideHint();
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
            RefreshMeshGraph();

            timer2.Enabled = false;
        }

        public void SetSelectedTabPageIndex(int tabpageindex)
        {
            xtraTabControl1.SelectedTabPageIndex = tabpageindex;
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
                onGraphSelectionChanged(this, new GraphSelectionChangedEventArgs(xtraTabControl1.SelectedTabPageIndex, m_map_name));
            }
        }

        private void xtraTabControl1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (xtraTabControl1.SelectedTabPage == xtraTabPage1)
            {
                // 3d graph
                Console.WriteLine("RefreshMeshGraph on tabindex changed");

                RefreshMeshGraph();
            }
            else
            {
                UpdateChartControlSlice(GetDataFromGridView(false));
            }
            CastGraphSelectionChangedEvent();
        }

        private void chartControl1_CustomDrawSeries(object sender, CustomDrawSeriesEventArgs e)
        {
            
        }

        private void chartControl1_MouseUp(object sender, MouseEventArgs e)
        {
            m_isDragging = false;
            _sp_dragging = null;
            timer4.Enabled = false;
        }


        private void chartControl1_MouseMove(object sender, MouseEventArgs e)
        {
        }


        private void groupControl1_DoubleClick(object sender, EventArgs e)
        {
            gridView1.OptionsView.AllowCellMerge = !gridView1.OptionsView.AllowCellMerge;
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            nChartControl1.Charts[0].Projection.Zoom += 5;
            nChartControl1.Refresh();
            CastSurfaceGraphChangedEventEx(nChartControl1.Charts[0].Projection.XDepth, nChartControl1.Charts[0].Projection.YDepth, nChartControl1.Charts[0].Projection.Zoom, nChartControl1.Charts[0].Projection.Rotation, nChartControl1.Charts[0].Projection.Elevation);

        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            nChartControl1.Charts[0].Projection.Zoom -= 5;
            nChartControl1.Refresh();
            CastSurfaceGraphChangedEventEx(nChartControl1.Charts[0].Projection.XDepth, nChartControl1.Charts[0].Projection.YDepth, nChartControl1.Charts[0].Projection.Zoom, nChartControl1.Charts[0].Projection.Rotation, nChartControl1.Charts[0].Projection.Elevation);
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            nChartControl1.Charts[0].Projection.Rotation += 5;
            nChartControl1.Refresh();
            CastSurfaceGraphChangedEventEx(nChartControl1.Charts[0].Projection.XDepth, nChartControl1.Charts[0].Projection.YDepth, nChartControl1.Charts[0].Projection.Zoom, nChartControl1.Charts[0].Projection.Rotation, nChartControl1.Charts[0].Projection.Elevation);
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            nChartControl1.Charts[0].Projection.Rotation -= 5;
            nChartControl1.Refresh();
            CastSurfaceGraphChangedEventEx(nChartControl1.Charts[0].Projection.XDepth, nChartControl1.Charts[0].Projection.YDepth, nChartControl1.Charts[0].Projection.Zoom, nChartControl1.Charts[0].Projection.Rotation, nChartControl1.Charts[0].Projection.Elevation);
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
        }

        private void gridView1_CustomRowCellEdit(object sender, DevExpress.XtraGrid.Views.Grid.CustomRowCellEditEventArgs e)
        {
        }

        private void MapViewerCellEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextEdit)
            {
                TextEdit txtedit = (TextEdit)sender;
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
                DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
                
                // Prepare cells array in format expected by ClipboardService: [colIndex, rowHandle, value, ...]
                object[] cells = new object[cellcollection.Length * 3];
                for (int i = 0; i < cellcollection.Length; i++)
                {
                    DevExpress.XtraGrid.Views.Base.GridCell gc = cellcollection[i];
                    int colIndex = gc.Column.AbsoluteIndex;
                    int rowHandle = gc.RowHandle;
                    object o = gridView1.GetRowCellValue(gc.RowHandle, gc.Column);
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
            gridView1.SelectAll();
            CopySelectionToClipboard();
            gridView1.ClearSelection();
        }

        private void copySelectedCellsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
            if (cellcollection.Length > 0)
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
            DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
            if (cellcollection.Length >= 1)
            {
                try
                {
                    int rowhandlefrom = cellcollection[0].RowHandle;
                    int colindexfrom = cellcollection[0].Column.AbsoluteIndex;
                    
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
                        gridView1.SetRowCellValue(pasteInfo.Row, gridView1.Columns[pasteInfo.Column], _dataConversionService.FormatValue(val, m_viewtype, m_issixteenbit));
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
                gridView1.SetRowCellValue(pasteInfo.Row, gridView1.Columns[pasteInfo.Column], _dataConversionService.FormatValue(val, m_viewtype, m_issixteenbit));
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
                double _workvalue = Convert.ToDouble(toolStripTextBox1.Text);
                DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
                if (cellcollection.Length > 0)
                {
                    foreach (DevExpress.XtraGrid.Views.Base.GridCell cell in cellcollection)
                    {
                        try
                        {
                            object cellVal = gridView1.GetRowCellValue(cell.RowHandle, cell.Column);
                            if (cellVal == null) continue;

                            int rawValue = _dataConversionService.ParseValue(cellVal.ToString(), m_viewtype);
                            double workingValue = rawValue;

                            if (m_viewtype == ViewType.Easy)
                            {
                                workingValue = _dataConversionService.ApplyCorrection(rawValue, correction_factor, correction_offset);
                            }

                            switch (toolStripComboBox1.SelectedIndex)
                            {
                                case 0: workingValue += _workvalue; break;
                                case 1: workingValue *= _workvalue; break;
                                case 2: if (_workvalue != 0) workingValue /= _workvalue; break;
                                case 3: workingValue = _workvalue; break;
                            }

                            int finalRawValue;
                            if (m_viewtype == ViewType.Easy)
                            {
                                finalRawValue = (int)Math.Round((workingValue - correction_offset) / (correction_factor != 0 ? correction_factor : 1));
                            }
                            else
                            {
                                finalRawValue = (int)Math.Round(workingValue);
                            }

                            // Clamp
                            int max = m_issixteenbit ? 0xFFFF : 0xFF;
                            if (finalRawValue > max) finalRawValue = max;
                            if (finalRawValue < 0) finalRawValue = 0;

                            gridView1.SetRowCellValue(cell.RowHandle, cell.Column, _dataConversionService.FormatValue(finalRawValue, m_viewtype, m_issixteenbit));
                        }
                        catch (Exception cE)
                        {
                            Console.WriteLine("toolStripButton3_Click cell: " + cE.Message);
                        }
                    }
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
            if (this.Parent is DevExpress.XtraBars.Docking.DockPanel)
            {
                DevExpress.XtraBars.Docking.DockPanel pnl = (DevExpress.XtraBars.Docking.DockPanel)this.Parent;
                if (pnl.FloatForm == null)
                {
                    pnl.FloatSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    pnl.FloatLocation = new System.Drawing.Point(1, 1);
                    pnl.MakeFloat();
                    // alleen grafiek
                    splitContainer1.Panel1Collapsed = true;
                    splitContainer1.Panel2Collapsed = false;
                }
                else
                {
                    pnl.Restore();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }
            }
            else if (this.Parent is DevExpress.XtraBars.Docking.ControlContainer)
            {
                DevExpress.XtraBars.Docking.ControlContainer container = (DevExpress.XtraBars.Docking.ControlContainer)this.Parent;
                if (container.Panel.FloatForm == null)
                {
                    container.Panel.FloatSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    container.Panel.FloatLocation = new System.Drawing.Point(1, 1);
                    container.Panel.MakeFloat();
                    splitContainer1.Panel1Collapsed = true;
                    splitContainer1.Panel2Collapsed = false;
                }
                else
                {
                    container.Panel.Restore();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }
                
            }
            CastSplitterMovedEvent();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (this.Parent is DevExpress.XtraBars.Docking.DockPanel)
            {
                DevExpress.XtraBars.Docking.DockPanel pnl = (DevExpress.XtraBars.Docking.DockPanel)this.Parent;
                if (pnl.FloatForm == null)
                {
                    pnl.FloatSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    pnl.FloatLocation = new System.Drawing.Point(1, 1);
                    pnl.MakeFloat();
                    // alleen grafiek
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = true;
                }
                else
                {
                    pnl.Restore();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }
            }
            else if (this.Parent is DevExpress.XtraBars.Docking.ControlContainer)
            {
                DevExpress.XtraBars.Docking.ControlContainer container = (DevExpress.XtraBars.Docking.ControlContainer)this.Parent;
                if (container.Panel.FloatForm == null)
                {
                    container.Panel.FloatSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    container.Panel.FloatLocation = new System.Drawing.Point(1, 1);
                    container.Panel.MakeFloat();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = true;
                }
                else
                {
                    container.Panel.Restore();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }

            }
            CastSplitterMovedEvent();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (this.Parent is DevExpress.XtraBars.Docking.DockPanel)
            {
                DevExpress.XtraBars.Docking.DockPanel pnl = (DevExpress.XtraBars.Docking.DockPanel)this.Parent;
                if (pnl.FloatForm == null)
                {
                    pnl.FloatSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    pnl.FloatLocation = new System.Drawing.Point(1, 1);
                    pnl.MakeFloat();
                    // alleen grafiek
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }
                else
                {
                    pnl.Restore();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }
            }
            else if (this.Parent is DevExpress.XtraBars.Docking.ControlContainer)
            {
                DevExpress.XtraBars.Docking.ControlContainer container = (DevExpress.XtraBars.Docking.ControlContainer)this.Parent;
                if (container.Panel.FloatForm == null)
                {
                    container.Panel.FloatSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
                    container.Panel.FloatLocation = new System.Drawing.Point(1, 1);
                    container.Panel.MakeFloat();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }
                else
                {
                    container.Panel.Restore();
                    splitContainer1.Panel1Collapsed = false;
                    splitContainer1.Panel2Collapsed = false;
                }

            }
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
            if (_sp_dragging != null)
            {
                string yaxisvalue = _sp_dragging.Argument;
                int rowhandle = -1;
                for (int t = 0; t < y_axisvalues.Length; t++)
                {
                    if (y_axisvalues.GetValue(t).ToString() == yaxisvalue)
                    {
                        rowhandle = (y_axisvalues.Length - 1) - t;
                    }
                }
                if (m_TableWidth == 1)
                {
                    // single column graph.. 
                    int numberofrows = m_map_length;
                    if (m_issixteenbit) numberofrows /= 2;
                    rowhandle = (numberofrows - 1) - Convert.ToInt32(yaxisvalue);
                }
                if (rowhandle != -1)
                {
                    if (tmr_toggle)
                    {
                        gridView1.SelectCell(rowhandle, gridView1.Columns[(int)trackBarControl1.Value]);
                        tmr_toggle = false;
                    }
                    else
                    {
                        gridView1.ClearSelection();
                        tmr_toggle = true;
                    }
                }
            }
        }

        private void popupContainerEdit1_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.ConvertEditValueEventArgs e)
        {
//            e.Value = System.IO.Path.GetFileName(m_filename) + " : " + m_map_name + " flash address : " + m_map_address.ToString("X6") + " sram address : " + m_map_sramaddress.ToString("X4");
        }

        private void gridView1_SelectionChanged_1(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            if (!m_prohibitcellchange)
            {
                DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
                if (cellcollection.Length == 1)
                {
                    object o = cellcollection.GetValue(0);
                    if (o is DevExpress.XtraGrid.Views.Base.GridCell)
                    {
                        DevExpress.XtraGrid.Views.Base.GridCell cell = (DevExpress.XtraGrid.Views.Base.GridCell)o;
                        CastSelectEvent(cell.RowHandle, cell.Column.AbsoluteIndex);
                    }

                }
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
            try
            {
                nChartControl1.Charts[0].Projection.XDepth = depthx;
                nChartControl1.Charts[0].Projection.YDepth = depthy;
                nChartControl1.Charts[0].Projection.Zoom = zoom;
                nChartControl1.Charts[0].Projection.Rotation = rotation;
                nChartControl1.Charts[0].Projection.Elevation = elevation;
                nChartControl1.Refresh();
            }
            catch (Exception E)
            {
                Console.WriteLine("SetSurfaceGraphViewEx:" + E.Message);
            }

        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // refresh the mapviewer with the values like selected
            if (m_prohibit_viewchange) return;
            switch (toolStripComboBox3.SelectedIndex)
            {
                case -1:
                case 0:
                    m_viewtype = ViewType.Hexadecimal;
                    break;
                case 1:
                    m_viewtype = ViewType.Decimal;
                    break;
                case 2:
                    m_viewtype = ViewType.Easy;
                    break;
                case 3:
                    m_viewtype = ViewType.ASCII;
                    break;
                default:
                    m_viewtype = ViewType.Hexadecimal;
                    break;
            }
            ReShowTable();
            CastViewTypeChangedEvent();
        }

        private void gridView1_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
           
        }

        private void gridView1_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            if (m_issixteenbit)
            {
                if (m_viewtype == ViewType.Hexadecimal)
                {
                    try
                    {
                        int value = Convert.ToInt32(Convert.ToString(e.Value), 16);
                        if (value > 0xFFFF)
                        {
                            e.Valid = false;
                            e.ErrorText = "Value not valid...";
                        }
                    }
                    catch (Exception hE)
                    {
                        e.Valid = false;
                        e.ErrorText = hE.Message;
                    }
                }
                else
                {
                    double dvalue = Convert.ToDouble(e.Value);
                    int value = 0;
                    if (m_viewtype == ViewType.Easy)
                    {
                        if (gridView1.ActiveEditor != null)
                        {
                            if (gridView1.ActiveEditor.EditValue.ToString() != gridView1.ActiveEditor.OldEditValue.ToString())
                            {
                                Console.WriteLine(gridView1.ActiveEditor.IsModified.ToString());
                                dvalue = Convert.ToDouble(gridView1.ActiveEditor.EditValue);
                                value = (int)Math.Round((dvalue - correction_offset) / (correction_factor != 0 ? correction_factor : 1));
                            }
                            else
                            {
                                value = (int)Math.Round(Convert.ToDouble(e.Value));
                            }
                        }
                        else
                        {
                            value = Convert.ToInt32(Convert.ToString(e.Value));
                        }
                    }
                    else
                    {
                        value = Convert.ToInt32(Convert.ToString(e.Value));
                    }
                    if (Math.Abs(value) > 78643)
                    {
                        e.Valid = false;
                        e.ErrorText = "Value not valid...";
                    }
                    else
                    {
                        e.Value = value;
                    }
                }

            }
            else
            {
                if (m_viewtype == ViewType.Hexadecimal)
                {
                    try
                    {
                        int value = Convert.ToInt32(Convert.ToString(e.Value), 16);
                        if (value > 0xFF)
                        {
                            e.Valid = false;
                            e.ErrorText = "Value not valid...";
                        }
                    }
                    catch(Exception hE)
                    {
                        e.Valid = false;
                        e.ErrorText = hE.Message;
                    }
                }
                else
                {
                    double dvalue = Convert.ToDouble(e.Value);
                    int value = 0;
                    if (m_viewtype == ViewType.Easy)
                    {
                        if (gridView1.ActiveEditor != null)
                        {
                            if (gridView1.ActiveEditor.EditValue.ToString() != gridView1.ActiveEditor.OldEditValue.ToString())
                            {
                                Console.WriteLine(gridView1.ActiveEditor.IsModified.ToString());
                                dvalue = Convert.ToDouble(gridView1.ActiveEditor.EditValue);
                                value = (int)Math.Round((dvalue - correction_offset) / (correction_factor != 0 ? correction_factor : 1));
                            }
                            else
                            {
                                value = (int)Math.Round(Convert.ToDouble(e.Value));
                            }
                        }
                        else
                        {
                            value = Convert.ToInt32(Convert.ToString(e.Value));
                        }
                    }
                    else
                    {
                        value = Convert.ToInt32(Convert.ToString(e.Value));
                    }

                    if (value > 255)
                    {
                        e.Valid = false;
                        e.ErrorText = "Value not valid...";
                    }
                    else
                    {
                        e.Value = value;
                    }
                }
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
            if (m_viewtype == ViewType.Easy )
            {
                gridView1.ActiveEditor.EditValue = ConvertToEasyValue((float)Convert.ToDouble(gridView1.ActiveEditor.EditValue)).ToString("F2");
                Console.WriteLine("Started editor with value: " + gridView1.ActiveEditor.EditValue.ToString());
            }
        }

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
        }

        private void gridView1_HiddenEditor(object sender, EventArgs e)
        {
            Console.WriteLine("Hidden editor with value: " + gridView1.GetFocusedRowCellDisplayText(gridView1.FocusedColumn));
        }

        private void MapViewer_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
            }
        }

        internal void ClearSelection()
        {
            gridView1.ClearSelection();
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
            DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
            if (cellcollection.Length > 2)
            {
                // Use the refactored SmoothingService for proportional smoothing
                object[] cells = new object[cellcollection.Length];
                for (int i = 0; i < cellcollection.Length; i++)
                {
                    cells[i] = cellcollection[i];
                }
                
                _smoothingService.SmoothProportional(cells, gridView1, x_axisvalues, y_axisvalues);
            }
        }

        private void smoothSelectionToolProportionalStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_viewtype == ViewType.Hexadecimal)
            {
                MessageBox.Show("Smoothing cannot be done in Hex view!");
                return;
            }
            DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
            if (cellcollection.Length > 2)
            {
                object[] cells = new object[cellcollection.Length];
                for (int i = 0; i < cellcollection.Length; i++)
                {
                    cells[i] = cellcollection[i];
                }
                _smoothingService.SmoothInterpolated(cells, gridView1, x_axisvalues, y_axisvalues);
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
                gridView1.ClearSelection();
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
                gridView1.Focus();

            }
        }

        private void SelectCellsWithValue(double value)
        {
            for (int rh = 0; rh < gridView1.RowCount; rh++)
            {
                for (int ch = 0; ch < gridView1.Columns.Count; ch++)
                {
                    try
                    {
                        object ov = gridView1.GetRowCellValue(rh, gridView1.Columns[ch]);
                        if (ov == null) continue;

                        int rawVal = _dataConversionService.ParseValue(ov.ToString(), m_viewtype);
                        double val = _dataConversionService.ApplyCorrection(rawVal, correction_factor, correction_offset);
                        
                        double diff = Math.Abs(val - value);
                        if (diff < 0.009)
                        {
                            gridView1.SelectCell(rh, gridView1.Columns[ch]);
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
