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
    public partial class frmSearchMaps : KryptonForm
    {
        public frmSearchMaps()
        {
            InitializeComponent();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public bool SearchForNumericValues
        {
            get
            {
                return kryptonCheckBox3.Checked;
            }
        }
        public bool SearchForStringValues
        {
            get
            {
                return kryptonCheckBox4.Checked;
            }
        }

        public bool IncludeSymbolNames
        {
            get
            {
                return kryptonCheckBox1.Checked;
            }
        }

        public bool IncludeSymbolDescription
        {
            get
            {
                return kryptonCheckBox2.Checked;
            }
        }

        public decimal NumericValueToSearchFor
        {
            get
            {
                return kryptonNumericUpDown1.Value;
            }
        }

        public string StringValueToSearchFor
        {
            get
            {
                return kryptonTextBox1.Text;
            }
        }

    }
}