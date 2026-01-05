using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using VAGSuite.Models;
using VAGSuite.Services;
using Zuby.ADGV;

namespace VAGSuite.Components
{
    /// <summary>
    /// Encapsulates the grid view for map data display and editing using AdvancedDataGridView.
    /// </summary>
    public class MapGridComponent : System.Windows.Forms.UserControl
    {
        #region Private Fields

        private readonly IDataConversionService _conversionService;
        private readonly IMapRenderingService _renderingService;
        
        private Zuby.ADGV.AdvancedDataGridView gridControl1;
        
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
        public event DataGridViewCellPaintingEventHandler CustomDrawCell;
        public event EventHandler SelectionChanged;
        public event DataGridViewCellEventHandler CellValueChanged;

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
            
            // Set column headers based on X-axis values
            for (int i = 0; i < gridControl1.Columns.Count; i++)
            {
                if (_xAxisValues != null && i < _xAxisValues.Length)
                {
                    gridControl1.Columns[i].HeaderText = FormatAxisValue(_xAxisValues[i]);
                }
            }
        }

        private string FormatAxisValue(int rawX)
        {
            if (_viewType == ViewType.Hexadecimal)
            {
                return rawX.ToString(rawX <= 255 ? "X2" : "X4");
            }
            else
            {
                double temp = (double)rawX * _correctionFactor + _correctionOffset;
                return temp.ToString("F2");
            }
        }

        /// <summary>
        /// Gets data from the grid as byte array
        /// </summary>
        public byte[] GetData(MapData data, ViewConfiguration config)
        {
            DataTable dt = (DataTable)gridControl1.DataSource;
            if (dt == null) return data.Content;
            return _conversionService.ConvertFromDataTable(dt, data, config, config.IsUpsideDown);
        }

        /// <summary>
        /// Gets the current cell value at specified position
        /// </summary>
        public object GetCellValue(int rowIndex, int columnIndex)
        {
            if (gridControl1 != null && gridControl1.RowCount > rowIndex && columnIndex >= 0 && columnIndex < gridControl1.ColumnCount)
            {
                return gridControl1.Rows[rowIndex].Cells[columnIndex].Value;
            }
            return null;
        }

        /// <summary>
        /// Sets the cell value at specified position
        /// </summary>
        public void SetCellValue(int rowIndex, int columnIndex, object value)
        {
            if (gridControl1 != null && gridControl1.RowCount > rowIndex && columnIndex >= 0 && columnIndex < gridControl1.ColumnCount)
            {
                gridControl1.Rows[rowIndex].Cells[columnIndex].Value = value;
                OnDataChanged();
            }
        }

        /// <summary>
        /// Gets the number of rows in the grid
        /// </summary>
        public int RowCount
        {
            get { return gridControl1?.RowCount ?? 0; }
        }

        /// <summary>
        /// Gets the number of columns in the grid
        /// </summary>
        public int ColumnCount
        {
            get { return gridControl1?.ColumnCount ?? 0; }
        }

        #endregion

        #region Private Methods

        private void InitializeComponent()
        {
            this.gridControl1 = new Zuby.ADGV.AdvancedDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.SuspendLayout();

            this.gridControl1.AllowUserToAddRows = false;
            this.gridControl1.AllowUserToDeleteRows = false;
            this.gridControl1.Dock = DockStyle.Fill;
            this.gridControl1.Location = new Point(0, 0);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.ReadOnly = false;
            this.gridControl1.Size = new Size(400, 300);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.RowHeadersWidth = 60;

            // Wire up events
            this.gridControl1.CellPainting += GridControl1_CellPainting;
            this.gridControl1.SelectionChanged += GridControl1_SelectionChanged;
            this.gridControl1.CellValueChanged += GridControl1_CellValueChanged;
            this.gridControl1.KeyDown += GridControl1_KeyDown;
            this.gridControl1.RowPostPaint += GridControl1_RowPostPaint;

            this.Controls.Add(this.gridControl1);
            this.Dock = DockStyle.Fill;
            this.Size = new Size(400, 300);

            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.ResumeLayout(false);
        }

        private void GridControl1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Draw Y-axis values in row headers
            if (_yAxisValues != null)
            {
                int index = _isUpsideDown ? (_yAxisValues.Length - 1) - e.RowIndex : e.RowIndex;
                if (index >= 0 && index < _yAxisValues.Length)
                {
                    string yvalue;
                    int rawY = Convert.ToInt32(_yAxisValues.GetValue(index));

                    if (_viewType == ViewType.Hexadecimal)
                    {
                        yvalue = rawY.ToString("X4");
                    }
                    else
                    {
                        double temp = (double)rawY * _correctionFactor + _correctionOffset;
                        yvalue = temp.ToString("F1");
                    }

                    using (Brush b = new SolidBrush(Color.MidnightBlue))
                    {
                        e.Graphics.DrawString(yvalue, this.Font, b, e.RowBounds.Location.X + 4, e.RowBounds.Location.Y + 4);
                    }
                }
            }
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

        private void GridControl1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            try
            {
                if (e.Value == null || e.Value == DBNull.Value)
                    return;

                int cellValue = 0;
                string valStr = e.Value.ToString();
                if (_viewType == ViewType.Hexadecimal)
                {
                    cellValue = Convert.ToInt32(valStr, 16);
                }
                else if (_viewType == ViewType.ASCII)
                {
                    cellValue = valStr.Length > 0 ? (int)valStr[0] : 0;
                }
                else
                {
                    cellValue = Convert.ToInt32(Convert.ToDouble(valStr));
                }

                // Calculate cell color
                Color cellColor = _renderingService.CalculateCellColor(
                    cellValue,
                    _maxValueInTable,
                    _onlineMode,
                    _isRedWhite);

                if (!_disableColors)
                {
                    e.CellStyle.BackColor = cellColor;
                }

                // Format display text for Easy view
                if (_viewType == ViewType.Easy && (_correctionOffset != 0 || _correctionFactor != 1))
                {
                    double correctedValue = _conversionService.ApplyCorrection(cellValue, _correctionFactor, _correctionOffset);
                    // We don't change e.Value here as it's data-bound, but we can influence painting
                }

                // Raise custom event for external painting
                CustomDrawCell?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                Console.WriteLine("MapGridComponent: GridControl1_CellPainting error: " + ex.Message);
            }
        }

        private void GridControl1_SelectionChanged(object sender, EventArgs e)
        {
            SelectionChanged?.Invoke(sender, e);
        }

        private void GridControl1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            OnDataChanged();
            CellValueChanged?.Invoke(sender, e);
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public void GridControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (gridControl1.SelectedCells.Count > 0)
            {
                int increment = 0;
                if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus) increment = 1;
                else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) increment = -1;
                else if (e.KeyCode == Keys.PageUp) increment = _viewType == ViewType.Hexadecimal ? 0x10 : 10;
                else if (e.KeyCode == Keys.PageDown) increment = _viewType == ViewType.Hexadecimal ? -0x10 : -10;
                else if (e.KeyCode == Keys.Home)
                {
                    e.Handled = true;
                    foreach (DataGridViewCell cell in gridControl1.SelectedCells)
                    {
                        int val = _isSixteenBit ? 0xFFFF : 0xFF;
                        cell.Value = _viewType == ViewType.Hexadecimal ? val.ToString(_isSixteenBit ? "X4" : "X2") : val.ToString();
                    }
                    return;
                }
                else if (e.KeyCode == Keys.End)
                {
                    e.Handled = true;
                    foreach (DataGridViewCell cell in gridControl1.SelectedCells)
                    {
                        cell.Value = _viewType == ViewType.Hexadecimal ? (_isSixteenBit ? "0000" : "00") : "0";
                    }
                    return;
                }

                if (increment != 0)
                {
                    e.Handled = true;
                    foreach (DataGridViewCell cell in gridControl1.SelectedCells)
                    {
                        int value = 0;
                        if (cell.Value == null) continue;

                        if (_viewType == ViewType.Hexadecimal)
                            value = Convert.ToInt32(cell.Value.ToString(), 16);
                        else
                            value = Convert.ToInt32(cell.Value.ToString());

                        value += increment;

                        if (!_isSixteenBit)
                        {
                            if (value > 0xFF) value = 0xFF;
                            if (value < 0) value = 0;
                        }
                        else
                        {
                            if (value > 0xFFFF) value = 0xFFFF;
                            if (value < -0x8000) value = -0x8000; // Support signed 16-bit
                        }

                        if (_viewType == ViewType.Hexadecimal)
                            cell.Value = value.ToString(_isSixteenBit ? "X4" : "X2");
                        else
                            cell.Value = value.ToString();
                    }
                }
            }
        }

        #endregion
    }
}
