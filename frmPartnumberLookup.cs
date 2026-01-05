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
    public partial class frmPartnumberLookup : KryptonForm
    {
        private string m_fileNameToSave = string.Empty;

        public string FileNameToSave
        {
            get { return m_fileNameToSave; }
            set { m_fileNameToSave = value; }
        }


        private bool m_open_File = false;

        private bool m_compare_File = false;

        private bool m_createNewFile = false;

        public bool CreateNewFile
        {
            get { return m_createNewFile; }
            set { m_createNewFile = value; }
        }

        public bool Compare_File
        {
            get { return m_compare_File; }
            set { m_compare_File = value; }
        }

        public bool Open_File
        {
            get { return m_open_File; }
            set { m_open_File = value; }
        }

        public frmPartnumberLookup()
        {
            InitializeComponent();
            VAGSuite.Theming.VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
        }

        private void ConvertPartNumber()
        {
            partNumberConverter pnc = new partNumberConverter();
            ECUInfo ecuinfo = pnc.ConvertPartnumber(kryptonTextBoxPartNumber.Text, 0);
            
            lblCarModel.Text = "---";
            lblECUType.Text = "---";
            lblFuel.Text = "---";
            lblCarType.Text = "---";
            lblRating.Text = "--/--";
            lblSoftwareID.Text = "---";

            if (ecuinfo.CarMake != string.Empty)
            {

                lblCarModel.Text = ecuinfo.CarMake;
                lblECUType.Text = ecuinfo.EcuType;
                lblFuel.Text = ecuinfo.FuelType;
                lblCarType.Text = ecuinfo.CarType;
                lblSoftwareID.Text = ecuinfo.SoftwareID;
                if (ecuinfo.HP > 0 || ecuinfo.TQ > 0)
                {
                    lblRating.Text = ecuinfo.HP + " hp/" + ecuinfo.TQ + " Nm";
                }
                else
                {
                    lblRating.Text = "";
                }

                if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "Binaries\\" + kryptonTextBoxPartNumber.Text + ".BIN")))
                {
                    simpleButton2.Enabled = true;
                    simpleButton3.Enabled = true;
                    simpleButton4.Enabled = true;
                }
                else
                {
                    simpleButton2.Enabled = false;
                    simpleButton3.Enabled = false;
                    simpleButton4.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("The entered partnumber was not recognized by VAGEDCSuite");
            }
        }

        private void buttonEdit1_ButtonClick(object sender, EventArgs e)
        {
            //ConvertPartNumber();
            frmPartNumberList pnl = new frmPartNumberList();
            pnl.ShowDialog();
            if (pnl.Selectedpartnumber != null)
            {
                if (pnl.Selectedpartnumber != string.Empty)
                {
                    kryptonTextBoxPartNumber.Text = pnl.Selectedpartnumber;
                }
            }
            if (kryptonTextBoxPartNumber.Text != "")
            {
                ConvertPartNumber();
            }
            else
            {
                simpleButton2.Enabled = false;
                simpleButton3.Enabled = false;
                simpleButton4.Enabled = false;
            }

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonEdit1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConvertPartNumber();
            }
        }

        internal void LookUpPartnumber(string p)
        {
            kryptonTextBoxPartNumber.Text = p;
            ConvertPartNumber();
        }

        public string GetFileToOpen()
        {
            string retval = string.Empty;
            if (kryptonTextBoxPartNumber.Text != string.Empty)
            {
                if (System.IO.File.Exists(Path.Combine(Application.StartupPath, "Binaries\\" + kryptonTextBoxPartNumber.Text + ".BIN")))
                {
                    retval = Path.Combine(Application.StartupPath, "Binaries\\" + kryptonTextBoxPartNumber.Text + ".BIN");
                }
            }
            return retval;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            m_open_File = true;
            this.Close();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            m_compare_File = true;
            this.Close();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            // show a savefile dialog
            m_createNewFile = true;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Binary files|*.bin";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // copt ori to 
                m_fileNameToSave = sfd.FileName;
                m_createNewFile = true;
                this.Close();
            }
        }
    }
}