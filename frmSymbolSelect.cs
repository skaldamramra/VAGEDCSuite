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
    public partial class frmSymbolSelect : KryptonForm
    {
        SymbolCollection m_symbols;

        public string SelectedSymbol
        {
            get
            {
                if (lookUpEdit1.SelectedItem != null)
                {
                    return lookUpEdit1.SelectedItem.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public frmSymbolSelect(SymbolCollection symbols)
        {
            m_symbols = symbols;
            InitializeComponent();
            VAGSuite.Theming.VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
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

        private void frmSymbolSelect_Load(object sender, EventArgs e)
        {
            lookUpEdit1.Items.Clear();
            if (m_symbols != null)
            {
                foreach (SymbolHelper sh in m_symbols)
                {
                    lookUpEdit1.Items.Add(sh.Varname);
                }
            }
        }
    }
}