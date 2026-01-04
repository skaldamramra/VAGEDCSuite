using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Theming;

namespace VAGSuite
{
    public partial class frmInfoBox : KryptonForm
    {
        public frmInfoBox(string Message)
        {
            InitializeComponent();
            VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
            label1.Text = Message;
            this.ShowDialog();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}