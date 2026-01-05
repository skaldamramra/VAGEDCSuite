using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Be.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;

namespace VAGSuite
{
    public partial class frmBinCompare : KryptonForm
    {
        private SymbolCollection m_symbols = new SymbolCollection();

        public SymbolCollection Symbols
        {
            get { return m_symbols; }
            set { m_symbols = value; }
        }
        private string _currentfilename = string.Empty;
        private string _comparefilename = string.Empty;

        private bool m_OutsideSymbolRangeCheck = false;

        public bool OutsideSymbolRangeCheck
        {
            get { return m_OutsideSymbolRangeCheck; }
            set { m_OutsideSymbolRangeCheck = value; }
        }

        public frmBinCompare()
        {
            InitializeComponent();
        }

        private bool CheckOutsideSymbolRange(int address)
        {
            if (m_symbols == null) return true;
            foreach (SymbolHelper sh in m_symbols)
            {
                if (address >= sh.Flash_start_address && address < (sh.Flash_start_address + sh.Length))
                {
                    return false;
                }
            }
            return true;
        }

        private bool ByteCompare(Byte[] ib1, Byte[] ib2, int address)
        {
            for (int t = 0; t < 16; t++)
            {
                if (m_OutsideSymbolRangeCheck)
                {
                    if (CheckOutsideSymbolRange(address + t))
                    {
                        if ((Byte)ib1.GetValue(address + t) != (Byte)ib2.GetValue(address+t))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if ((Byte)ib1.GetValue(address + t) != (Byte)ib2.GetValue(address+t))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void SetCurrentFilename(string filename)
        {
            label1.Text = Path.GetFileName(filename);
            _currentfilename = filename;
        }

        public void SetCompareFilename(string filename)
        {
            label2.Text = Path.GetFileName(filename);
            _comparefilename = filename;
        }

        bool[] linediffs;
        List<string> currstrList = new List<string>();
        List<string> compstrList = new List<string>();

        public void CompareFiles()
        {
            currstrList.Clear();
            compstrList.Clear();
            try
            {
                if (File.Exists(_currentfilename))
                {
                    if (File.Exists(_comparefilename))
                    {
                        FileInfo fi = new FileInfo(_currentfilename);
                        FileInfo fi2 = new FileInfo(_comparefilename);
                        linediffs = new bool[fi.Length / 16];
                        
                        byte[] currfile = File.ReadAllBytes(_currentfilename);
                        byte[] compfile = File.ReadAllBytes(_comparefilename);

                        for (int tel = 0; tel < (fi.Length / 16); tel++)
                        {
                            try
                            {
                                if (!ByteCompare(currfile, compfile, (tel * 16)))
                                {
                                    linediffs[tel] = true;
                                    Int32 addr = tel * 16;
                                    string s1 = addr.ToString("X6") + ": ";
                                    string s2 = s1;
                                    for (int t = 0; t < 16; t++)
                                    {
                                        Byte b1 = (Byte)currfile.GetValue((tel*16) + t);
                                        Byte b2 = (Byte)compfile.GetValue((tel * 16) + t);
                                        s1 += b1.ToString("X2") + " ";
                                        s2 += b2.ToString("X2") + " ";
                                    }
                                    currstrList.Add(s1);
                                    compstrList.Add(s2);
                                }
                                else
                                {
                                    linediffs[tel] = false;
                                    if (!checkButton1.Checked)
                                    {
                                        Int32 addr = tel * 16;
                                        string s1 = addr.ToString("X6") + ": ";
                                        string s2 = s1;
                                        for (int t = 0; t < 16; t++)
                                        {
                                            Byte b1 = (Byte)currfile.GetValue((tel * 16) + t);
                                            Byte b2 = (Byte)compfile.GetValue((tel * 16) + t);
                                            s1 += b1.ToString("X2") + " ";
                                            s2 += b2.ToString("X2") + " ";
                                        }
                                        currstrList.Add(s1);
                                        compstrList.Add(s2);
                                    }
                                }
                            }
                            catch (Exception cE)
                            {
                                Console.WriteLine(cE.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception E)
            {
                KryptonMessageBox.Show(E.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            listBox1.Count = currstrList.Count;
            listBox2.Count = compstrList.Count;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkButton1_CheckedChanged(object sender, EventArgs e)
        {
            CompareFiles();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index >= 0 && e.Index < currstrList.Count)
                {
                    string symbolName = string.Empty;
                    try
                    {
                        int idx = currstrList[e.Index].IndexOf(':');
                        if (idx > 0)
                        {
                            int address = Convert.ToInt32(currstrList[e.Index].Substring(0, idx), 16);
                            if (address > 0)
                            {
                                symbolName = Tools.Instance.GetSymbolNameByAddressInRange(address, address + 0x0F);
                            }
                        }
                    }
                    catch (Exception) { }

                    string drawItem = currstrList[e.Index];
                    if(symbolName != string.Empty) drawItem += " << " + symbolName;
                    
                    Brush textBrush = Brushes.Black;
                    if (!checkButton1.Checked && linediffs[e.Index]) textBrush = Brushes.Red;
                    
                    listBox1.DefaultDrawItem(e, drawItem, textBrush);
                }
            }
            catch (Exception) { }
        }

        private void listBox2_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index >= 0 && e.Index < compstrList.Count)
                {
                    Brush textBrush = Brushes.Black;
                    if (!checkButton1.Checked && linediffs[e.Index]) textBrush = Brushes.Red;
                    
                    listBox2.DefaultDrawItem(e, compstrList[e.Index], textBrush);
                }
            }
            catch (Exception) { }
        }

        void listBox1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                listBox2.SelectedIndex = listBox1.SelectedIndex;
                Application.DoEvents();
            }
            catch (Exception) { }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                listBox2.SelectedIndex = listBox1.SelectedIndex;
                Application.DoEvents();
            }
            catch (Exception) { }
        }

        void listBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                listBox1.SelectedIndex = listBox2.SelectedIndex;
                Application.DoEvents();
            }
            catch (Exception) { }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (!checkButton1.Checked)
            {
                try
                {
                    int currindex = listBox1.SelectedIndex;
                    while (++currindex < linediffs.Length && !linediffs[currindex]) ;
                    if (currindex < linediffs.Length)
                    {
                        listBox1.SelectedIndex = currindex;
                        listBox2.SelectedIndex = currindex;
                    }
                    Application.DoEvents();
                }
                catch (Exception) { }
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            if (!checkButton1.Checked)
            {
                try
                {
                    int currindex = listBox1.SelectedIndex;
                    while (--currindex >= 0 && !linediffs[currindex]) ;
                    if (currindex >= 0)
                    {
                        listBox1.SelectedIndex = currindex;
                        listBox2.SelectedIndex = currindex;
                    }
                    Application.DoEvents();
                }
                catch (Exception) { }
            }
        }

        void listBox2_OnVerticalScroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            listBox1.TopIndex = listBox2.TopIndex;
        }

        void listBox1_OnVerticalScroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            listBox2.TopIndex = listBox1.TopIndex;
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName, false))
                {
                    for(int i = 0; i < currstrList.Count; i ++)
                    {
                        string symbolName = string.Empty;
                        try
                        {
                            int idx = currstrList[i].IndexOf(':');
                            if (idx > 0)
                            {
                                int address = Convert.ToInt32(currstrList[i].Substring(0, idx), 16);
                                if (address > 0)
                                {
                                    symbolName = Tools.Instance.GetSymbolNameByAddressInRange(address, address + 0x0F);
                                }
                            }
                        }
                        catch (Exception) { }
                        
                        string exportLine = currstrList[i] + " ---- " + compstrList[i];
                        if (symbolName != string.Empty) exportLine += " << " + symbolName;
                        sw.WriteLine(exportLine);
                    }
                }
            }
        }
    }
}