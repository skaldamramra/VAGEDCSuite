using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Zuby.ADGV;


namespace VAGSuite
{
    public partial class frmTransactionLog : KryptonForm
    {
        public delegate void RollBack(object sender, RollInformationEventArgs e);
        public event frmTransactionLog.RollBack onRollBack;
        public delegate void RollForward(object sender, RollInformationEventArgs e);
        public event frmTransactionLog.RollForward onRollForward;
        public delegate void NoteChanged(object sender, RollInformationEventArgs e);
        public event frmTransactionLog.NoteChanged onNoteChanged;

        public class RollInformationEventArgs : System.EventArgs
        {
            private TransactionEntry _entry;

            public TransactionEntry Entry
            {
                get { return _entry; }
                set { _entry = value; }
            }

            
            public RollInformationEventArgs(TransactionEntry entry)
            {
                this._entry = entry;
            }
        }

        public frmTransactionLog()
        {
            InitializeComponent();
        }

        public void SetTransactionLog(TransactionLog log)
        {
            gridControl1.DataSource = log.TransCollection; // should be based on symbolname
            // select the last entry and scroll to it
            gridControl1.AutoResizeColumns();
            UpdateButtons();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            // get selected row and decide what to show/not to show
            Point p = gridControl1.PointToClient(Cursor.Position);
            DataGridView.HitTestInfo hitinfo = gridControl1.HitTest(p.X, p.Y);
            
            if (hitinfo.Type == DataGridViewHitTestType.Cell)
            {
                if (gridControl1.Rows[hitinfo.RowIndex].DataBoundItem is TransactionEntry)
                {
                    TransactionEntry sh = (TransactionEntry)gridControl1.Rows[hitinfo.RowIndex].DataBoundItem;
                    if (sh.IsRolledBack)
                    {
                        rollForwardToolStripMenuItem.Enabled = true;
                        rolllBackToolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        rollForwardToolStripMenuItem.Enabled = false;
                        rolllBackToolStripMenuItem.Enabled = true;
                    }
                }
            }
        }

        private void CastRollBackEvent(TransactionEntry entry)
        {
            if (onRollBack != null)
            {
                onRollBack(this, new RollInformationEventArgs(entry));
            }
        }

        private void CastNoteChangedEvent(TransactionEntry entry)
        {
            if (onNoteChanged != null)
            {
                onNoteChanged(this, new RollInformationEventArgs(entry));
            }
        }

        private void CastRollForwardEvent(TransactionEntry entry)
        {
            if (onRollForward != null)
            {
                onRollForward(this, new RollInformationEventArgs(entry));
            }

        }

        private void rolllBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridControl1.SelectedRows.Count > 0)
            {
                if (gridControl1.SelectedRows[0].DataBoundItem is TransactionEntry)
                {
                    TransactionEntry sh = (TransactionEntry)gridControl1.SelectedRows[0].DataBoundItem;
                    CastRollBackEvent(sh);
                }
            }
        }

        private void rollForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gridControl1.SelectedRows.Count > 0)
            {
                if (gridControl1.SelectedRows[0].DataBoundItem is TransactionEntry)
                {
                    TransactionEntry sh = (TransactionEntry)gridControl1.SelectedRows[0].DataBoundItem;
                    CastRollForwardEvent(sh);
                }
            }
        }

        private void UpdateButtons()
        {
            if (gridControl1.SelectedRows.Count == 0)
            {
                simpleButton3.Enabled = false;
                simpleButton4.Enabled = false;
            }
            else
            {
                if (gridControl1.SelectedRows[0].DataBoundItem is TransactionEntry)
                {
                    TransactionEntry sh = (TransactionEntry)gridControl1.SelectedRows[0].DataBoundItem;
                    if (sh.IsRolledBack)
                    {
                        simpleButton4.Enabled = true;
                        simpleButton3.Enabled = false;
                    }
                    else
                    {
                        simpleButton4.Enabled = false;
                        simpleButton3.Enabled = true;
                    }
                }
            }
        }

        private void gridControl1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // highlight the rollback column?
        }

        private void gridControl1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            // cast rollback
            if (gridControl1.SelectedRows.Count > 0)
            {
                if (gridControl1.SelectedRows[0].DataBoundItem is TransactionEntry)
                {
                    TransactionEntry sh = (TransactionEntry)gridControl1.SelectedRows[0].DataBoundItem;
                    CastRollBackEvent(sh);
                    // move the selected cursor one row up, if possible
                    int idx = gridControl1.SelectedRows[0].Index;
                    if (idx > 0)
                    {
                        gridControl1.ClearSelection();
                        gridControl1.Rows[idx - 1].Selected = true;
                    }
                }
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            // cast roll forward
            if (gridControl1.SelectedRows.Count > 0)
            {
                if (gridControl1.SelectedRows[0].DataBoundItem is TransactionEntry)
                {
                    TransactionEntry sh = (TransactionEntry)gridControl1.SelectedRows[0].DataBoundItem;
                    CastRollForwardEvent(sh);
                    int idx = gridControl1.SelectedRows[0].Index;
                    if (idx < gridControl1.Rows.Count - 1)
                    {
                        gridControl1.ClearSelection();
                        gridControl1.Rows[idx + 1].Selected = true;
                    }
                }
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            // show the details for this transaction (including data, meaning 2 mapviewers showing the details)
            frmInfoBox info = new frmInfoBox("Still needs to be implemented");
        }

        private void gridControl1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // note was edited?
            if (e.RowIndex >= 0 && gridControl1.Columns[e.ColumnIndex].DataPropertyName == "Note")
            {
                // save the transaction log again (and reload?)
                TransactionEntry sh = (TransactionEntry)gridControl1.Rows[e.RowIndex].DataBoundItem;
                CastNoteChangedEvent(sh);
            }
        }

        private void frmTransactionLog_Shown(object sender, EventArgs e)
        {
           /* try
            {
                if (gridView1.RowCount > 0)
                {
                    gridView1.FocusedRowHandle = gridView1.RowCount - 1;
                    gridView1.MakeRowVisible(gridView1.FocusedRowHandle, false);
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }*/

        }
    }
}