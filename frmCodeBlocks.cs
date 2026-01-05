using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace VAGSuite
{
    public partial class frmCodeBlocks : KryptonForm
    {
        public frmCodeBlocks()
        {
            InitializeComponent();
            VAGSuite.Theming.VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
        }

        private void frmCodeBlocks_Load(object sender, EventArgs e)
        {
            gridControl1.DataSource = Tools.Instance.codeBlockList;
        }

        private void gridControl1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = gridControl1.Columns[e.ColumnIndex].Name;
            if (colName == "colStartAddress" || colName == "colEndAddress" || colName == "colAddressID")
            {
                try
                {
                    if (e.Value != null)
                    {
                        int addr = Convert.ToInt32(e.Value);
                        e.Value = addr.ToString("X8");
                        e.FormattingApplied = true;
                    }
                }
                catch(Exception) { }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}