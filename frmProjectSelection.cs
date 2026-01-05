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
    public partial class frmProjectSelection : KryptonForm
    {
        public frmProjectSelection()
        {
            InitializeComponent();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            // selected row = ok
            if (gridControl1.SelectedRows.Count > 0)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public void SetDataSource(DataTable dt)
        {
            if (dt != null)
            {
                gridControl1.DataSource = dt;
            }
        }

        public string GetProjectName()
        {
            string retval = string.Empty;
            if (gridControl1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = gridControl1.SelectedRows[0];
                if (row != null)
                {
                    DataRowView dv = (DataRowView)row.DataBoundItem;
                    if (dv != null)
                    {
                        if (dv.Row["Projectname"] != DBNull.Value)
                        {
                            retval = dv.Row["Projectname"].ToString();
                        }
                    }
                }
            }
            return retval;
        }
    }
}