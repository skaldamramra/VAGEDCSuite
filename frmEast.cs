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
    public partial class frmEast : KryptonForm
    {
        public frmEast()
        {
            InitializeComponent();
        }

        private void frmEast_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}