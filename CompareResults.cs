using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace VAGSuite
{
    public partial class CompareResults : System.Windows.Forms.UserControl
    {
        public delegate void NotifySelectSymbol(object sender, SelectSymbolEventArgs e);
        public event CompareResults.NotifySelectSymbol onSymbolSelect;

        private string m_OriginalFilename = string.Empty;

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
            gridColumn2.DefaultCellStyle.Format = format;
            gridColumn3.DefaultCellStyle.Format = format;
            gridColumn4.DefaultCellStyle.Format = format;
            gridColumn5.DefaultCellStyle.Format = format;
            gridColumn12.DefaultCellStyle.Format = format;
            gridColumn13.DefaultCellStyle.Format = format;
        }


        public CompareResults()
        {
            InitializeComponent();
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
            if (gridControl1.Columns[e.ColumnIndex].Name == gridColumn6.Name)
            {
                DataGridViewRow row = gridControl1.Rows[e.RowIndex];
                DataRowView dr = (DataRowView)row.DataBoundItem;
                object o = dr.Row["CATEGORY"];
                Color c = Color.White;
                if (o != DBNull.Value)
                {
                    int cat = Convert.ToInt32(o);
                    if (cat == (int)XDFCategories.Fuel) c = Color.LightSteelBlue;
                    else if (cat == (int)XDFCategories.Ignition) c = Color.LightGreen;
                    else if (cat == (int)XDFCategories.Boost_control) c = Color.OrangeRed;
                    else if (cat == (int)XDFCategories.Misc) c = Color.LightGray;
                    else if (cat == (int)XDFCategories.Sensor) c = Color.Yellow;
                    else if (cat == (int)XDFCategories.Correction) c = Color.LightPink;
                    else if (cat == (int)XDFCategories.Idle) c = Color.BurlyWood;
                }
                if (c != Color.White)
                {
                    e.CellStyle.BackColor = c;
                }
            }
            
            // Handle missing symbol indicators (Salmon/CornflowerBlue)
            DataGridViewRow dgvRow = gridControl1.Rows[e.RowIndex];
            DataRowView drv = (DataRowView)dgvRow.DataBoundItem;
            if (drv.Row["MissingInOriFile"] != DBNull.Value && (bool)drv.Row["MissingInOriFile"])
            {
                e.CellStyle.BackColor = Color.Salmon;
            }
            else if (drv.Row["MissingInCompareFile"] != DBNull.Value && (bool)drv.Row["MissingInCompareFile"])
            {
                e.CellStyle.BackColor = Color.CornflowerBlue;
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
