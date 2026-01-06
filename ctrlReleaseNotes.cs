using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using System.Globalization;

namespace VAGSuite
{
    public partial class ctrlReleaseNotes : UserControl
    {
        private DateTime m_LatestReleaseDate = DateTime.MinValue;
        private const string GITHUB_RELEASES_URL = "https://github.com/skaldamramra/VAGEDCSuite/releases";
        
        public ctrlReleaseNotes()
        {
            InitializeComponent();
        }

        public static string GetExceptionType(Exception ex)
        {
            string exceptionType = ex.GetType().ToString();
            return exceptionType.Substring(
                exceptionType.LastIndexOf('.') + 1);
        }


        public DateTime StringToDateTime(string cultureName, string dateTimeString)
        {
            CultureInfo culture = new CultureInfo(cultureName);

            //Console.WriteLine();

            // Convert each string in the dateStrings array.
            DateTime dateTimeValue = DateTime.MinValue;

            // Display the first part of the output line.
            //Console.Write(lineFmt, dateStr, cultureName, null);

            try
            {
                // Convert the string to a DateTime object.
                dateTimeValue = Convert.ToDateTime(dateTimeString, culture);

                // Display the DateTime object in a fixed format 
                // if Convert succeeded.
                Console.WriteLine("{0:yyyy-MMM-dd}", dateTimeValue);
            }
            catch (Exception ex)
            {
                // Display the exception type if Parse failed.
                Console.WriteLine("{0}", GetExceptionType(ex));
            }
            return dateTimeValue;

        }


        public void LoadXML(string filename)
        {
            DataSet ds = new DataSet();
            //DataTable dt = new DataTable();
            try
            {
                
                /*dt.TableName = "channel";
                dt.Columns.Add("description");
                dt.Columns.Add("link");
                dt.Columns.Add("pubDate");
                dt.Columns.Add("docs");
                dt.Columns.Add("rating");
                dt.Columns.Add("generator");

                dt.ReadXml(filename);*/
                ds.ReadXml(filename);
                
                DataTable itemTable = ds.Tables["item"];
                if (itemTable != null)
                {
                    DataTable channelTable = ds.Tables["channel"];
                    if (channelTable != null && channelTable.Rows.Count > 0 && channelTable.Columns.Contains("pubDate"))
                    {
                        m_LatestReleaseDate = StringToDateTime("en-US", channelTable.Rows[0]["pubDate"].ToString());
                        Console.WriteLine("Release date: " + m_LatestReleaseDate.ToString());
                    }

                    if (!itemTable.Columns.Contains("Date"))
                    {
                        itemTable.Columns.Add("Date", System.Type.GetType("System.DateTime"));
                    }

                    foreach (DataRow dr in itemTable.Rows)
                    {
                        try
                        {
                            if (itemTable.Columns.Contains("pubDate") && dr["pubDate"] != DBNull.Value)
                            {
                                dr["Date"] = StringToDateTime("en-US", dr["pubDate"].ToString()).Date;
                            }
                        }
                        catch (Exception convE)
                        {
                            Console.WriteLine("Failed to convert datetime: " + convE.Message);
                        }
                    }

                    gridControl1.DataSource = itemTable;
                }
                else if (ds.Tables.Count > 0)
                {
                    // Fallback to first table if "item" not found
                    gridControl1.DataSource = ds.Tables[0];
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }

        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // gridControl1.ShowPrintPreview();
        }

        private void btnViewOnGitHub_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(GITHUB_RELEASES_URL);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open GitHub releases page: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridControl1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == gcLink.Index)
            {
                string link = gridControl1.Rows[e.RowIndex].Cells[gcLink.Index].Value as string;
                if (!string.IsNullOrEmpty(link))
                {
                    try
                    {
                        System.Diagnostics.Process.Start(link);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to open link: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


    }
}
