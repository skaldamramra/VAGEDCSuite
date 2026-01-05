using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using System.IO;

namespace VAGSuite
{
    public partial class frmPartNumberList : KryptonForm
    {
        
        private string m_selectedpartnumber = "";
        private DataTable partnumbers = new DataTable();
        public string Selectedpartnumber
        {
            get { return m_selectedpartnumber; }
            set { m_selectedpartnumber = value; }
        }

        public frmPartNumberList()
        {
            InitializeComponent();
            VAGSuite.Theming.VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
            partnumbers.Columns.Add("FILENAME");
            partnumbers.Columns.Add("PARTNUMBER");
            partnumbers.Columns.Add("ECUTYPE");
            partnumbers.Columns.Add("CARTYPE");
            partnumbers.Columns.Add("TUNER");
            partnumbers.Columns.Add("STAGE");
            partnumbers.Columns.Add("INFO");
            partnumbers.Columns.Add("SPEED");
        }

        private void LoadPartNumbersFromFiles()
        {
            if (Directory.Exists(Application.StartupPath + "\\Binaries"))
            {

                string[] binfiles = Directory.GetFiles(Application.StartupPath + "\\Binaries", "*.BIN");
                foreach (string binfile in binfiles)
                {
                    string speed = "20";
                    string binfilename = Path.GetFileNameWithoutExtension(binfile);
                    string partnumber = "";

                    string enginetype = "";
                    string cartype = "";
                    string tuner = "";
                    string stage = "";
                    string additionalinfo = "";
                    if (binfilename.Contains("-"))
                    {
                        char[] sep = new char[1];
                        sep.SetValue('-', 0);
                        string[] values = binfilename.Split(sep);
                        if (values.Length == 1)
                        {
                            partnumber = (string)binfilename;
                            partnumbers.Rows.Add(binfile, partnumber, enginetype, cartype, tuner, stage, additionalinfo, speed);
                        }
                        else if (values.Length == 3)
                        {
                            cartype = (string)values.GetValue(0);
                            enginetype = (string)values.GetValue(1);
                            partnumber = (string)values.GetValue(2);
                            partnumbers.Rows.Add(binfile, partnumber, enginetype, cartype, tuner, stage, additionalinfo, speed);
                        }
                        else if (values.Length == 4)
                        {
                            cartype = (string)values.GetValue(0);
                            enginetype = (string)values.GetValue(1);
                            partnumber = (string)values.GetValue(2);
                            tuner = (string)values.GetValue(3);
                            partnumbers.Rows.Add(binfile, partnumber, enginetype, cartype, tuner, stage, additionalinfo, speed);
                        }
                        else if (values.Length == 5)
                        {
                            cartype = (string)values.GetValue(0);
                            enginetype = (string)values.GetValue(1);
                            partnumber = (string)values.GetValue(2);
                            tuner = (string)values.GetValue(3);
                            stage = (string)values.GetValue(4);
                            partnumbers.Rows.Add(binfile, partnumber, enginetype, cartype, tuner, stage, additionalinfo, speed);
                        }
                        else if (values.Length > 5)
                        {
                            cartype = (string)values.GetValue(0);
                            enginetype = (string)values.GetValue(1);
                            partnumber = (string)values.GetValue(2);
                            tuner = (string)values.GetValue(3);
                            stage = (string)values.GetValue(4);
                            for (int tel = 5; tel < values.Length; tel++)
                            {
                                additionalinfo += (string)values.GetValue(tel) + " ";
                            }
                            partnumbers.Rows.Add(binfile, partnumber, enginetype, cartype, tuner, stage, additionalinfo, speed);
                        }
                    }
                    else
                    {
                        partnumber = (string)binfilename;
                        partnumbers.Rows.Add(binfile, partnumber, enginetype, cartype, tuner, stage, additionalinfo, speed);
                    }
                    Application.DoEvents();
                }
            }
            
        }

        private void frmPartNumberList_Load(object sender, EventArgs e)
        {
            PartnumberCollection pnc = new PartnumberCollection();
            DataTable dt = pnc.GeneratePartNumberCollection();

            LoadPartNumbersFromFiles();

            gridControl1.DataSource = dt;
        }

        private void gridView1_DoubleClick(object sender, EventArgs e)
        {
            if (gridControl1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = gridControl1.SelectedRows[0];
                DataRowView dv = (DataRowView)row.DataBoundItem;
                if (dv != null)
                {
                    m_selectedpartnumber = dv.Row["Partnumber"].ToString();
                    if (!string.IsNullOrEmpty(m_selectedpartnumber))
                    {
                        this.Close();
                    }
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (gridControl1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = gridControl1.SelectedRows[0];
                DataRowView dv = (DataRowView)row.DataBoundItem;
                if (dv != null)
                {
                    m_selectedpartnumber = dv.Row["Partnumber"].ToString();
                }
            }
            this.Close();
        }

        private int CheckInAvailableLibrary(string partnumber)
        {
            int retval = 0;
            foreach (DataRow dr in partnumbers.Rows)
            {
                if (dr["PARTNUMBER"] != DBNull.Value)
                {
                    if (dr["PARTNUMBER"].ToString() == partnumber)
                    {
                        retval = 1;
                        break;
                    }
                }
            }
            return retval;
        }

        private void gridControl1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridControl1.Columns[e.ColumnIndex].Name == "Partnumber" || gridControl1.Columns[e.ColumnIndex].DataPropertyName == "Partnumber")
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    int type = CheckInAvailableLibrary(e.Value.ToString());
                    if (type == 1)
                    {
                        e.CellStyle.BackColor = Color.YellowGreen;
                    }
                }
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text files|*.txt";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                // Simple text export for DataGridView
                StringBuilder sb = new StringBuilder();
                foreach (DataGridViewColumn col in gridControl1.Columns)
                {
                    sb.Append(col.HeaderText + "\t");
                }
                sb.AppendLine();
                foreach (DataGridViewRow row in gridControl1.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        sb.Append(cell.Value?.ToString() + "\t");
                    }
                    sb.AppendLine();
                }
                File.WriteAllText(sfd.FileName, sb.ToString());
            }
        }
    }
}