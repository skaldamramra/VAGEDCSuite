using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Theming;

namespace VAGSuite
{
    public partial class CompareResults : System.Windows.Forms.UserControl
    {
        public delegate void NotifySelectSymbol(object sender, SelectSymbolEventArgs e);
        public event CompareResults.NotifySelectSymbol onSymbolSelect;

        private string m_OriginalFilename = string.Empty;

        // VAGEDC Dark skin compatible category colors (darker variants for dark backgrounds)
        private static readonly Dictionary<XDFCategories, Color> CategoryColors = new Dictionary<XDFCategories, Color>
        {
            { XDFCategories.Fuel, Color.FromArgb(70, 130, 180) },      // SteelBlue
            { XDFCategories.Ignition, Color.FromArgb(60, 179, 113) },  // MediumSeaGreen
            { XDFCategories.Boost_control, Color.FromArgb(205, 92, 92) }, // IndianRed
            { XDFCategories.Misc, Color.FromArgb(119, 136, 153) },     // LightSlateGray
            { XDFCategories.Sensor, Color.FromArgb(218, 165, 32) },    // GoldenRod
            { XDFCategories.Correction, Color.FromArgb(255, 105, 180) }, // HotPink
            { XDFCategories.Idle, Color.FromArgb(222, 184, 135) },     // BurlyWood
        };

        public string OriginalFilename
        {
            get { return m_OriginalFilename; }
            set { m_OriginalFilename = value; }
        }
        private string m_CompareFilename = string.Empty;

        public string CompareFilename
        {
            get { return m_CompareFilename; }
            set { m_CompareFilename = value; }
        }

        private SymbolCollection m_originalSymbolCollection = new SymbolCollection();

        public SymbolCollection OriginalSymbolCollection
        {
            get { return m_originalSymbolCollection; }
            set { m_originalSymbolCollection = value; }
        }
        

        private SymbolCollection m_compareSymbolCollection = new SymbolCollection();

        public SymbolCollection CompareSymbolCollection
        {
            get { return m_compareSymbolCollection; }
            set { m_compareSymbolCollection = value; }
        }

        private bool m_UseForFind = false;

        public bool UseForFind
        {
            get { return m_UseForFind; }
            set
            {
                m_UseForFind = value;
                if (m_UseForFind)
                {
                    // hide certain columns
                    gridColumn7.Visible = false;
                    gridColumn8.Visible = false;
                    gridColumn9.Visible = false;
                    gridColumn10.Visible = false;
                    gridColumn11.Visible = false;
                    gridColumn12.Visible = false;
                    gridColumn13.Visible = false;
                    gcMissingInCompareFile.Visible = false;
                    gcMissingInOriFile.Visible = false;
                    showDifferenceMapToolStripMenuItem.Visible = false;
                    saveLayoutToolStripMenuItem.Visible = false;
                }
            }
        }

        private string m_filename = "";

        public string Filename
        {
            get { return m_filename; }
            set { m_filename = value; }
        }

        private bool m_ShowAddressesInHex = true;

        public bool ShowAddressesInHex
        {
            get { return m_ShowAddressesInHex; }
            set { m_ShowAddressesInHex = value; }
        }

        public void SetFilterMode(bool IsHexMode)
        {
            string format = IsHexMode ? "X6" : "";
            // Flash address (gridColumn3) should show in hex/dec based on setting
            gridColumn3.DefaultCellStyle.Format = format;
            // Length columns should always display in decimal (no formatting)
            gridColumn4.DefaultCellStyle.Format = "";
            gridColumn5.DefaultCellStyle.Format = "";
            // SRAM address (gridColumn2) - optional, can be hidden or shown
            gridColumn2.DefaultCellStyle.Format = format;
        }


        public CompareResults()
        {
            InitializeComponent();
            ApplyThemeToControl();
        }

        private void ApplyThemeToControl()
        {
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            
            // Apply VAGEDC Dark skin colors to the DataGridView
            if (gridControl1 != null)
            {
                // Disable visual styles to allow custom header colors
                gridControl1.EnableHeadersVisualStyles = false;

                gridControl1.BackgroundColor = theme.GridBackground;
                gridControl1.ForeColor = theme.TextPrimary;
                gridControl1.DefaultCellStyle.BackColor = theme.GridBackground;
                gridControl1.DefaultCellStyle.ForeColor = theme.TextPrimary;
                gridControl1.ColumnHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
                gridControl1.ColumnHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
                gridControl1.RowHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
                gridControl1.RowHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
                gridControl1.GridColor = theme.GridBorder;
                gridControl1.BorderStyle = BorderStyle.FixedSingle;
                
                // Apply VAGEDC Dark skin selection colors
                gridControl1.DefaultCellStyle.SelectionBackColor = VAGEDCColorPalette.Primary500;
                gridControl1.DefaultCellStyle.SelectionForeColor = Color.White;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Force dark scrollbars as soon as the Win32 handle exists
            VAGEDCThemeManager.Instance.ApplyDarkScrollbars(this);
        }

        public void SetGridWidth()
        {
            foreach (System.Windows.Forms.DataGridViewColumn col in gridControl1.Columns)
            {
                col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        private void CastSelectEvent(int m_map_address, int m_map_length, string m_map_name, int symbolnumber1, int symbolnumber2, int codeblock1, int codeblock2)
        {
            if (onSymbolSelect != null)
            {
                onSymbolSelect(this, new SelectSymbolEventArgs(m_map_address, m_map_length, m_map_name, m_filename, false, m_compareSymbolCollection, symbolnumber1, symbolnumber2, codeblock1, codeblock2));
            }
        }

        private void CastDifferenceEvent(int m_map_address, int m_map_length, string m_map_name, int symbolnumber1, int symbolnumber2, int codeblock1, int codeblock2)
        {
            if (onSymbolSelect != null)
            {
                onSymbolSelect(this, new SelectSymbolEventArgs(m_map_address, m_map_length, m_map_name, m_filename, true, m_compareSymbolCollection, symbolnumber1, symbolnumber2, codeblock1, codeblock2));
            }
        }

        public void OpenGridViewGroups(object ctrl, int groupleveltoexpand)
        {
            // ADGV does not support grouping in the same way as XtraGrid
        }

        /// <summary>
        /// Sorts the grid data so that known maps (non-empty category) appear first
        /// </summary>
        public void SortByCategory()
        {
            if (gridControl1.DataSource is DataTable dt)
            {
                // Create a DataView and sort by CATEGORYNAME (empty strings last)
                DataView dv = new DataView(dt);
                dv.Sort = "CATEGORYNAME ASC";
                
                // Reorder rows: known maps first, then unknown
                DataTable sortedDt = dv.ToTable();
                
                // Move rows with non-empty category to top
                DataTable resultDt = dt.Clone();
                
                // First add rows with non-empty category
                foreach (DataRow row in sortedDt.Rows)
                {
                    if (!string.IsNullOrEmpty(row["CATEGORYNAME"]?.ToString()))
                    {
                        resultDt.ImportRow(row);
                    }
                }
                
                // Then add rows with empty category
                foreach (DataRow row in sortedDt.Rows)
                {
                    if (string.IsNullOrEmpty(row["CATEGORYNAME"]?.ToString()))
                    {
                        resultDt.ImportRow(row);
                    }
                }
                
                gridControl1.DataSource = resultDt;
            }
        }

        private void StartTableViewer()
        {
            if (gridControl1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = gridControl1.SelectedRows[0];
                DataRowView dr = (DataRowView)row.DataBoundItem;
                string Map_name = dr.Row["SYMBOLNAME"].ToString();
                int address = Convert.ToInt32(dr.Row["FLASHADDRESS"].ToString());
                int length = Convert.ToInt32(dr.Row["LENGTHBYTES"].ToString());
                int symbolnumber1 = 0;
                int symbolnumber2 = 0;
                int codeblock1 = 1;
                int codeblock2 = 1;
                if (dr.Row["SymbolNumber1"] != DBNull.Value)
                {
                    symbolnumber1 = Convert.ToInt32(dr.Row["SymbolNumber1"]);
                }
                if (dr.Row["SymbolNumber2"] != DBNull.Value)
                {
                    symbolnumber2 = Convert.ToInt32(dr.Row["SymbolNumber2"]);
                }
                try
                {
                    if (dr.Row["CodeBlock1"] != DBNull.Value)
                    {
                        codeblock1 = Convert.ToInt32(dr.Row["CodeBlock1"]);
                    }
                    if (dr.Row["CodeBlock2"] != DBNull.Value)
                    {
                        codeblock2 = Convert.ToInt32(dr.Row["CodeBlock2"]);
                    }
                }
                catch (Exception)
                {

                }
                CastSelectEvent(address, length, Map_name, symbolnumber1, symbolnumber2, codeblock1, codeblock2);
            }
        }

        private void gridControl1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                StartTableViewer();
            }
        }

        public class SelectSymbolEventArgs : System.EventArgs
        {

            private int _codeBlock1;

            public int CodeBlock1
            {
                get { return _codeBlock1; }
                set { _codeBlock1 = value; }
            }
            private int _codeBlock2;

            public int CodeBlock2
            {
                get { return _codeBlock2; }
                set { _codeBlock2 = value; }
            }


            private int _symbolnumber1;

            public int Symbolnumber1
            {
                get { return _symbolnumber1; }
                set { _symbolnumber1 = value; }
            }
            private int _symbolnumber2;

            public int Symbolnumber2
            {
                get { return _symbolnumber2; }
                set { _symbolnumber2 = value; }
            }
            private int _address;
            private int _length;
            private string _mapname; 
            private string _filename;
            private bool _showdiffmap;
            private SymbolCollection _symbols;

            public SymbolCollection Symbols
            {
                get { return _symbols; }
                set { _symbols = value; }
            }


            public bool ShowDiffMap
            {
                get
                {
                    return _showdiffmap;
                }
            }

            public int SymbolAddress
            {
                get
                {
                    return _address;
                }
            }

            public int SymbolLength
            {
                get
                {
                    return _length;
                }
            }

            public string SymbolName
            {
                get
                {
                    return _mapname;
                }
            }

            public string Filename
            {
                get
                {
                    return _filename;
                }
            }

            public SelectSymbolEventArgs(int address, int length, string mapname, string filename, bool showdiffmap, SymbolCollection symColl, int symbolnumber1, int symbolnumber2, int codeblock1, int codeblock2)
            {
                this._address = address;
                this._length = length;
                this._mapname = mapname;
                this._filename = filename;
                this._showdiffmap = showdiffmap;
                this._symbols = symColl;
                this._symbolnumber1 = symbolnumber1;
                this._symbolnumber2 = symbolnumber2;
                this._codeBlock1 = codeblock1;
                this._codeBlock2 = codeblock2;
            }
        }

        private void gridControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartTableViewer();
                e.Handled = true;
            }
        }

        private void gridControl1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            
            if (gridControl1.Columns[e.ColumnIndex].Name == gridColumn6.Name)
            {
                DataGridViewRow row = gridControl1.Rows[e.RowIndex];
                DataRowView dr = (DataRowView)row.DataBoundItem;
                object o = dr.Row["CATEGORY"];
                Color c = theme.GridBackground; // Use theme background as default
                if (o != DBNull.Value)
                {
                    int cat = Convert.ToInt32(o);
                    XDFCategories category = (XDFCategories)cat;
                    if (CategoryColors.TryGetValue(category, out Color categoryColor))
                    {
                        c = categoryColor;
                    }
                }
                if (c != theme.GridBackground)
                {
                    e.CellStyle.BackColor = c;
                    // Ensure text is readable on colored backgrounds
                    e.CellStyle.ForeColor = Color.White;
                }
            }
            
            // Handle missing symbol indicators (VAGEDC Dark skin compatible colors)
            DataGridViewRow dgvRow = gridControl1.Rows[e.RowIndex];
            DataRowView drv = (DataRowView)dgvRow.DataBoundItem;
            if (drv.Row["MissingInOriFile"] != DBNull.Value && (bool)drv.Row["MissingInOriFile"])
            {
                e.CellStyle.BackColor = VAGEDCColorPalette.Danger500; // Red for missing in original
                e.CellStyle.ForeColor = Color.White;
            }
            else if (drv.Row["MissingInCompareFile"] != DBNull.Value && (bool)drv.Row["MissingInCompareFile"])
            {
                e.CellStyle.BackColor = VAGEDCColorPalette.Primary500; // Blue for missing in compare
                e.CellStyle.ForeColor = Color.White;
            }
        }

        private void showDifferenceMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridControl1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = gridControl1.SelectedRows[0];
                DataRowView dr = (DataRowView)row.DataBoundItem;
                string Map_name = dr.Row["SYMBOLNAME"].ToString();
                int address = Convert.ToInt32(dr.Row["FLASHADDRESS"].ToString());
                int length = Convert.ToInt32(dr.Row["LENGTHBYTES"].ToString());
                int symbolnumber1 = 0;
                int symbolnumber2 = 0;
                int codeblock1 = 1;
                int codeblock2 = 1;
                if (dr.Row["SymbolNumber1"] != DBNull.Value)
                {
                    symbolnumber1 = Convert.ToInt32(dr.Row["SymbolNumber1"]);
                }
                if (dr.Row["SymbolNumber2"] != DBNull.Value)
                {
                    symbolnumber2 = Convert.ToInt32(dr.Row["SymbolNumber2"]);
                }
                if (dr.Row["CodeBlock1"] != DBNull.Value)
                {
                    codeblock1 = Convert.ToInt32(dr.Row["CodeBlock1"]);
                }
                if (dr.Row["CodeBlock2"] != DBNull.Value)
                {
                    codeblock2 = Convert.ToInt32(dr.Row["CodeBlock2"]);
                }

                CastDifferenceEvent(address, length, Map_name, symbolnumber1, symbolnumber2, codeblock1, codeblock2);
            }
        }

        private void exportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Export logic for ADGV
        }

        private void saveLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Save layout logic for ADGV
        }

        internal void HideMissingSymbolIndicators()
        {
            gcMissingInOriFile.Visible = false;
            gcMissingInCompareFile.Visible = false;
        }
    }
}
