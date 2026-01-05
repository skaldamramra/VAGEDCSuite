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
    public partial class frmProjectTransactionPurge : KryptonForm
    {
        public frmProjectTransactionPurge()
        {
            InitializeComponent();
        }

        public void SetNumberOfTransactions(int transactionCount)
        {
            label2.Text = "Number of transactions in log: " + transactionCount.ToString();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}