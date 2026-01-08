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
    public partial class ctrlAxisEditor : System.Windows.Forms.UserControl
    {
        public delegate void ViewerClose(object sender, EventArgs e);
        public event ctrlAxisEditor.ViewerClose onClose;
        
        public delegate void DataSave(object sender, EventArgs e);
        public event ctrlAxisEditor.DataSave onSave;


        public ctrlAxisEditor()
        {
            InitializeComponent();
            
            // Apply VAGEDC Dark theme to this control and its children
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            this.BackColor = theme.WindowBackground;
            VAGEDCThemeManager.Instance.ApplyThemeToControl(this);
            
            // Explicitly apply theme to the DataGridView (ADGV)
            ApplyThemeToDataGridView();
            
            // Apply Source Sans Pro font to ToolStrip and all its items
            Font toolStripFont = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);
            if (toolStrip1 != null)
            {
                toolStrip1.Font = toolStripFont;
                toolStrip1.BackColor = theme.ToolbarBackground;
                toolStrip1.ForeColor = theme.ToolbarText;
                toolStrip1.Renderer = VAGEDCThemeManager.Instance.GetToolStripRenderer();
                
                foreach (ToolStripItem item in toolStrip1.Items)
                {
                    item.Font = toolStripFont;
                    item.ForeColor = theme.ToolbarText;
                }
            }
        }
        
        /// <summary>
        /// Applies VAGEDC Dark skin to the DataGridView control
        /// </summary>
        private void ApplyThemeToDataGridView()
        {
            if (gridControl1 == null) return;
            
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            Font headerFont = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Bold);
            Font cellFont = VAGEDCThemeManager.Instance.GetCustomFont(9f, FontStyle.Regular);
            
            // Control-level settings
            gridControl1.BackgroundColor = theme.GridBackground;
            gridControl1.ForeColor = theme.TextPrimary;
            gridControl1.BorderStyle = BorderStyle.FixedSingle;
            gridControl1.GridColor = VAGEDCColorPalette.Gray600;
            gridControl1.Font = cellFont;
            
            // Disable visual styles to allow custom coloring
            try
            {
                gridControl1.EnableHeadersVisualStyles = false;
            }
            catch { /* Property may not exist on all DataGridView variants */ }
            
            // Column headers - dark background with light text
            gridControl1.ColumnHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
            gridControl1.ColumnHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
            gridControl1.ColumnHeadersDefaultCellStyle.Font = headerFont;
            gridControl1.ColumnHeadersDefaultCellStyle.SelectionBackColor = theme.GridHeaderBackground;
            gridControl1.ColumnHeadersDefaultCellStyle.SelectionForeColor = theme.GridHeaderText;
            
            // Row headers - dark background with light text
            gridControl1.RowHeadersDefaultCellStyle.BackColor = theme.GridHeaderBackground;
            gridControl1.RowHeadersDefaultCellStyle.ForeColor = theme.GridHeaderText;
            gridControl1.RowHeadersDefaultCellStyle.Font = headerFont;
            gridControl1.RowHeadersDefaultCellStyle.SelectionBackColor = theme.GridHeaderBackground;
            gridControl1.RowHeadersDefaultCellStyle.SelectionForeColor = theme.GridHeaderText;
            
            // Alternating row color
            gridControl1.AlternatingRowsDefaultCellStyle.BackColor = theme.GridAlternateRow;
            gridControl1.AlternatingRowsDefaultCellStyle.ForeColor = theme.TextPrimary;
            gridControl1.AlternatingRowsDefaultCellStyle.Font = cellFont;
            
            // Default row style
            gridControl1.RowsDefaultCellStyle.BackColor = theme.GridBackground;
            gridControl1.RowsDefaultCellStyle.ForeColor = theme.TextPrimary;
            gridControl1.RowsDefaultCellStyle.Font = cellFont;
            
            // Selection style - VAGEDC Dark selection blue
            gridControl1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            gridControl1.DefaultCellStyle.SelectionBackColor = VAGEDCColorPalette.Primary500;
            gridControl1.DefaultCellStyle.SelectionForeColor = Color.White;
            gridControl1.DefaultCellStyle.Font = cellFont;
            
            // Cell border style
            gridControl1.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            
            // Apply dark scrollbars via Win32 API
            if (gridControl1.IsHandleCreated)
            {
                ApplyDarkScrollbars(gridControl1);
            }
        }
        
        /// <summary>
        /// Applies dark theme to scrollbars using Windows API
        /// </summary>
        private void ApplyDarkScrollbars(Control control)
        {
            try
            {
                // Use reflection or direct Win32 call for dark scrollbars
                if (control.IsHandleCreated)
                {
                    SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
                }
                
                foreach (Control child in control.Controls)
                {
                    ApplyDarkScrollbars(child);
                }
            }
            catch { /* Ignore errors for scrollbar theming */ }
        }
        
        [System.Runtime.InteropServices.DllImport("uxtheme.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private int m_axisID = 0;

        public int AxisID
        {
            get { return m_axisID; }
            set { m_axisID = value; }
        }
        private int m_axisAddress = 0;

        public int AxisAddress
        {
            get { return m_axisAddress; }
            set { m_axisAddress = value; }
        }

        private float m_correctionFactor = 1;

        public float CorrectionFactor
        {
            get { return m_correctionFactor; }
            set { m_correctionFactor = value; }
        }

        private string m_fileName = string.Empty;

        public string FileName
        {
            get { return m_fileName; }
            set { m_fileName = value; }
        }

        private string m_Map_name = string.Empty;

        public string Map_name
        {
            get { return m_Map_name; }
            set { m_Map_name = value; }
        }

        public void SetData(float[] data)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("VALUE");
            foreach (float f in data)
            {
                dt.Rows.Add(f.ToString("F3"));
            }
            gridControl1.DataSource = dt;
        }

        public float[] GetData()
        {
            float[] retval = new float[1];
            retval.SetValue(0, 0);
            DataTable dt = (DataTable)gridControl1.DataSource;
            if(dt != null)
            {
                retval = new float[dt.Rows.Count];
                int idx = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    retval.SetValue((float)Convert.ToDouble(dr["VALUE"].ToString()), idx++);
                }
            }
            return retval;
        }

        private void CastSaveEvent()
        {
            if (onSave != null)
            {
                onSave(this, EventArgs.Empty);
            }
        }

        private void CastCloseEvent()
        {
            if (onClose != null)
            {
                onClose(this, EventArgs.Empty);
            }
        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            gridControl1.EndEdit();
            CastSaveEvent();
        }

        private void gridControl1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            KryptonMessageBox.Show("Invalid input value", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.ThrowException = false;
        }
    }
}
