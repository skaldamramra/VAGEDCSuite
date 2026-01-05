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
    public partial class frmRebuildFileParameters : KryptonForm
    {
        public frmRebuildFileParameters()
        {
            InitializeComponent();
            dateEdit1.Value = DateTime.Now;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        public DateTime SelectedDateTime
        {
            get
            {
                return dateEdit1.Value;
            }
        }

        public bool UseAsNewProjectFile
        {
            get 
            {
                return checkEdit1.Checked;
            }
        }

    }
}