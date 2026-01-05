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
    public partial class frmEEPromEditor : KryptonForm
    {
        EDC15P_EEPROM eeprom = new EDC15P_EEPROM();
        private string _filename = string.Empty;
        public frmEEPromEditor()
        {
            InitializeComponent();
        }

        internal void LoadFile(string filename)
        {
            eeprom.LoadFile(filename);
            _filename = filename;

            textEdit1.Text = eeprom.Immo;
            checkEdit1.Checked = eeprom.ImmoActive;
            textEdit4.Text = eeprom.Key.ToString();
            textEdit2.Text = eeprom.Mileage.ToString();
            textEdit3.Text = eeprom.Vin;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            eeprom.Vin = textEdit3.Text;
            eeprom.Immo = textEdit1.Text;
            eeprom.ImmoActive = checkEdit1.Checked;
            try
            {
                eeprom.Key = Convert.ToInt32(textEdit4.Text);
            }
            catch (Exception) { }
            eeprom.UpdateFile(_filename);
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}