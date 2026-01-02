using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;
using VAGSuite.Models;
using VAGSuite.Services;

namespace VAGSuite.Components
{
    /// <summary>
    /// Encapsulates the grid view for map data display and editing.
    /// </summary>
    public class MapGridComponent : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private readonly IDataConversionService _conversionService;
        private readonly IMapRenderingService _renderingService;
        
        private DevExpress.XtraGrid.GridControl gridControl1;
        private GridView gridView1;
        
        // State references
        private int _tableWidth;
        private bool _isSixteenBit;
        private ViewType _viewType;
        private int _maxValueInTable;
        private bool _onlineMode;
        private bool _isRedWhite;
        private bool _disableColors;
        private string _mapName;
        private string _xAxisName;
        private string _yAxisName;
        private byte[] _openLoop;
        private int[] _xAxisValues;
        private int[] _yAxisValues;
        private double _correctionFactor;
        private double _correctionOffset;
        private bool _isCompareViewer;
        private bool _isUpsideDown;

        #endregion

        #region Events

        public event EventHandler DataChanged;
        public event EventHandler<DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs> CustomDrawCell;
        public event EventHandler<DevExpress.Data.SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs> CellValueChanged;

        #endregion

        #region Constructors

        public MapGridComponent()
        {
            _conversionService = new DataConversionService();
            _renderingService = new MapRenderingService();
            InitializeComponent();
        }

        public MapGridComponent(IDataConversionService conversionService, IMapRenderingService renderingService)
        {
            _conversionService = conversionService ?? throw new ArgumentNullException("conversionService");
            _renderingService = renderingService ?? throw new ArgumentNullException("renderingService");
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads data into the grid from byte array content
        /// </summary>
        public void LoadData(byte[] content, int mapLength, MapViewerState state)
        {
            if (content == null || content.Length == 0) return;

            _tableWidth = state.Data.TableWidth;
            _isSixteenBit = state.Data.IsSixteenBit;
            _viewType = state.Configuration.ViewType;
            _maxValueInTable = state.Data.MaxValueInTable;
            _onlineMode = state.IsOnlineMode;
            _isRedWhite = state.Configuration.IsRedWhite;
            _disableColors = state.Configuration.DisableColors;
            _mapName = state.Metadata.Name;
            _xAxisName = state.Metadata.XAxisName;
            _yAxisName = state.Metadata.YAxisName;
            _openLoop = state.Data.OpenLoop;
            _xAxisValues = state.Axes.XAxisValues;
            _yAxisValues = state.Axes.YAxisValues;
            _correctionFactor = state.Configuration.CorrectionFactor;
            _correctionOffset = state.Configuration.CorrectionOffset;
            _isCompareViewer = state.IsCompareMode;
            _isUpsideDown = state.Configuration.IsUpsideDown;

            DataTable dt = new DataTable();
            
            int numberRows = mapLength / _tableWidth;
            if (_isSixteenBit) numberRows /= 2;

            // Create columns
            for (int c = 0; c < _tableWidth; c++)
            {
                dt.Columns.Add(c.ToString());
            }

            int mapOffset = 0;

            if (_isSixteenBit)
            {
                for (int i = 0; i < numberRows; i++)
                {
                    object[] objarr = new object[_tableWidth];
                    for (int j = 0; j < _tableWidth; j++)
                    {
                        int b = content[mapOffset++];
                        b *= 256;
                        b += content[mapOffset++];

                        if (b > 0xF000)
                        {
                            b = 0x10000 - b;
                            b = -b;
                        }

                        objarr[j] = FormatCellValue(b);
                    }
                    AddRow(dt, objarr);
                }
            }
            else
            {
                for (int i = 0; i < numberRows; i++)
                {
                    object[] objarr = new object[_tableWidth];
                    for (int j = 0; j < _tableWidth; j++)
                    {
                        int b = content[mapOffset++];
                        objarr[j] = FormatCellValue(b);
                    }
                    AddRow(dt, objarr);
                }
            }

            gridControl1.DataSource = dt;
        }

        /// <summary>
        /// Gets data from the grid as byte array
        /// </summary>
        public byte[] GetData()
        {
            byte[] retval = new byte[0]; // Placeholder - actual implementation needed
            return retval;
        }

        /// <summary>
        /// Gets the current cell value at specified position
        /// </summary>
        public object GetCellValue(int rowHandle, int columnIndex)
        {
            if (gridView1 != null && gridView1.RowCount > rowHandle && columnIndex >= 0 && columnIndex < gridView1.Columns.Count)
            {
                return gridView1.GetRowCellValue(rowHandle, gridView1.Columns[columnIndex]);
            }
            return null;
        }

        /// <summary>
        /// Sets the cell value at specified position
        /// </summary>
        public void SetCellValue(int rowHandle, int columnIndex, object value)
        {
            if (gridView1 != null && gridView1.RowCount > rowHandle && columnIndex >= 0 && columnIndex < gridView1.Columns.Count)
            {
                gridView1.SetRowCellValue(rowHandle, gridView1.Columns[columnIndex], value);
                OnDataChanged();
            }
        }

        /// <summary>
        /// Gets the number of rows in the grid
        /// </summary>
        public int RowCount
        {
            get { return gridView1?.RowCount ?? 0; }
        }

        /// <summary>
        /// Gets the number of columns in the grid
        /// </summary>
        public int ColumnCount
        {
            get { return gridView1?.Columns.Count ?? 0; }
        }

        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            // Create grid control
            gridControl1 = new DevExpress.XtraGrid.GridControl();
            gridView1 = new GridView();
            
            ((System.ComponentModel.ISupportInitialize)(gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(gridView1)).BeginInit();

            // 
            // gridControl1
            // 
            gridControl1.Dock = DockStyle.Fill;
            gridControl1.Location = new Point(0, 0);
            gridControl1.MainView = gridView1;
            gridControl1.Name = "gridControl1";
            gridControl1.Size = new Size(400, 300);
            gridControl1.TabIndex = 0;
            gridControl1.Visible = true;

            // 
            // gridView1
            // 
            gridView1.GridControl = gridControl1;
            gridView1.Name = "gridView1";
            gridView1.OptionsView.ColumnAutoWidth = false;
            gridView1.OptionsView.ShowColumnHeaders = false;
            
            // Wire up events
            gridView1.CustomDrawCell += GridView1_CustomDrawCell;
            gridView1.SelectionChanged += GridView1_SelectionChanged;
            gridView1.CellValueChanged += GridView1_CellValueChanged;
            gridView1.KeyDown += GridView1_KeyDown;

            this.Controls.Add(gridControl1);
            this.Dock = DockStyle.Fill;
            this.Size = new Size(400, 300);

            ((System.ComponentModel.ISupportInitialize)(gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(gridView1)).EndInit();
        }

        private string FormatCellValue(int value)
        {
            if (_viewType == ViewType.Hexadecimal)
            {
                return _isSixteenBit ? value.ToString("X4") : value.ToString("X2");
            }
            else if (_viewType == ViewType.ASCII)
            {
                try
                {
                    char c = Convert.ToChar(value);
                    if (c < 0x20) c = ' ';
                    return c.ToString();
                }
                catch
                {
                    return " ";
                }
            }
            else
            {
                return value.ToString();
            }
        }

        private void AddRow(DataTable dt, object[] objarr)
        {
            if (_isUpsideDown)
            {
                System.Data.DataRow r = dt.NewRow();
                r.ItemArray = objarr;
                dt.Rows.InsertAt(r, 0);
            }
            else
            {
                dt.Rows.Add(objarr);
            }
        }

        private void GridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            try
            {
                if (e.CellValue == null || e.CellValue == DBNull.Value)
                    return;

                int cellValue = 0;
                if (_viewType == ViewType.Hexadecimal)
                {
                    cellValue = Convert.ToInt32(e.CellValue.ToString(), 16);
                }
                else
                {
                    cellValue = Convert.ToInt32(Convert.ToDouble(e.CellValue.ToString()));
                }

                // Calculate cell color using the rendering service
                Color cellColor = _renderingService.CalculateCellColor(
                    cellValue,
                    _maxValueInTable,
                    _onlineMode,
                    _isRedWhite);

                // Apply coloring if not disabled
                if (!_disableColors)
                {
                    using (SolidBrush sb = new SolidBrush(cellColor))
                    {
                        e.Graphics.FillRectangle(sb, e.Bounds);
                    }
                }

                // Format display text
                string displayText = _conversionService.FormatValue(cellValue, _viewType, _isSixteenBit);

                // Apply correction if needed
                if (_viewType == ViewType.Easy && (_correctionOffset != 0 || _correctionFactor != 1))
                {
                    double correctedValue = _conversionService.ApplyCorrection(cellValue, _correctionFactor, _correctionOffset);
                    displayText = correctedValue.ToString("F2");
                }

                e.DisplayText = displayText;

                // Raise custom event
                CustomDrawCell?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MapGridComponent: GridView1_CustomDrawCell error: " + ex.Message);
            }
        }

        private void GridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
        }

        private void GridView1_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            OnDataChanged();
            CellValueChanged?.Invoke(sender, e);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void GridView1_KeyDown(object sender, KeyEventArgs e)
        {
            DevExpress.XtraGrid.Views.Base.GridCell[] cellcollection = gridView1.GetSelectedCells();
            if (cellcollection.Length > 0)
            {
                int increment = 0;
                if (e.KeyCode == Keys.Add) increment = 1;
                else if (e.KeyCode == Keys.Subtract) increment = -1;
                else if (e.KeyCode == Keys.PageUp) increment = _viewType == ViewType.Hexadecimal ? 0x10 : 10;
                else if (e.KeyCode == Keys.PageDown) increment = _viewType == ViewType.Hexadecimal ? -0x10 : -10;
                else if (e.KeyCode == Keys.Home)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    foreach (var gc in cellcollection)
                    {
                        int val = _isSixteenBit ? 0xFFFF : 0xFF;
                        gridView1.SetRowCellValue(gc.RowHandle, gc.Column, _viewType == ViewType.Hexadecimal ? val.ToString(_isSixteenBit ? "X4" : "X2") : val.ToString());
                    }
                    return;
                }
                else if (e.KeyCode == Keys.End)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    foreach (var gc in cellcollection)
                    {
                        gridView1.SetRowCellValue(gc.RowHandle, gc.Column, _viewType == ViewType.Hexadecimal ? (_isSixteenBit ? "0000" : "00") : "0");
                    }
                    return;
                }

                if (increment != 0)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    foreach (var gc in cellcollection)
                    {
                        int value = 0;
                        object cellVal = gridView1.GetRowCellValue(gc.RowHandle, gc.Column);
                        if (cellVal == null) continue;

                        if (_viewType == ViewType.Hexadecimal)
                            value = Convert.ToInt32(cellVal.ToString(), 16);
                        else
                            value = Convert.ToInt32(cellVal.ToString());

                        value += increment;

                        if (!_isSixteenBit)
                        {
                            if (value > 0xFF) value = 0xFF;
                            if (value < 0) value = 0;
                        }
                        else
                        {
                            if (value > 0xFFFF) value = 0xFFFF;
                            // 16-bit can be signed in some contexts but legacy code mostly clamped at 0 or allowed wrap
                        }

                        if (_viewType == ViewType.Hexadecimal)
                            gridView1.SetRowCellValue(gc.RowHandle, gc.Column, value.ToString(_isSixteenBit ? "X4" : "X2"));
                        else
                            gridView1.SetRowCellValue(gc.RowHandle, gc.Column, value.ToString());
                    }
                }
            }
        }

        #endregion
    }
}
