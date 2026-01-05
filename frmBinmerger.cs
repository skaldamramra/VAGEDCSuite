using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using ComponentFactory.Krypton.Toolkit;

namespace VAGSuite
{
    public partial class frmBinmerger : KryptonForm
    {
        public frmBinmerger()
        {
            InitializeComponent();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(buttonEdit1.Text))
                {
                    if (File.Exists(buttonEdit2.Text))
                    {
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            // 3 bestanden bekend.. converteren
                            FileInfo fi = new FileInfo(buttonEdit1.Text);
                            FileInfo fi2 = new FileInfo(buttonEdit2.Text);
                            if (fi.Length == fi2.Length)
                            {
                                using (FileStream fs = File.Create(saveFileDialog1.FileName))
                                using (BinaryWriter bw = new BinaryWriter(fs))
                                using (FileStream fsi1 = File.OpenRead(buttonEdit1.Text))
                                using (BinaryReader br1 = new BinaryReader(fsi1))
                                using (FileStream fsi2 = File.OpenRead(buttonEdit2.Text))
                                using (BinaryReader br2 = new BinaryReader(fsi2))
                                {
                                    for (int tel = 0; tel < fi.Length; tel++)
                                    {
                                        Byte ib1 = br1.ReadByte();
                                        Byte ib2 = br2.ReadByte();
                                        bw.Write(ib2);
                                        bw.Write(ib1);
                                    }
                                }
                                KryptonMessageBox.Show("Files merged successfully", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                KryptonMessageBox.Show("File lengths don't match, unable to merge!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            
                        }
                    }
                }
            }
            catch (Exception E)
            {
                KryptonMessageBox.Show(E.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Close();
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                buttonEdit1.Text = openFileDialog1.FileName;
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                buttonEdit2.Text = openFileDialog1.FileName;
            }
        }
    }
}