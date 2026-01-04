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
    public partial class frmProgress : KryptonForm
    {
        public delegate void CancelEvent(object sender, EventArgs e);
        public event frmProgress.CancelEvent onCancelOperation;


        private void CastCancelEvent()
        {
            if (onCancelOperation != null)
            {
                onCancelOperation(this, new EventArgs());
            }
        }

        public frmProgress()
        {
            // Set chrome properties before InitializeComponent to ensure they take effect
            this.AllowFormChrome = true;
            this.PaletteMode = PaletteMode.Custom;
            InitializeComponent();
            VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
        }

        public void SetProgress(string text)
        {
            if (label1.Values.Text != text)
            {
                label1.Values.Text = text;
                // Force layout update to handle potential text wrapping
                this.PerformLayout();
                Application.DoEvents();
            }
        }

        public void SetProgressPercentage(int percentage)
        {
            if (progressBarControl1.Value != percentage)
            {
                progressBarControl1.Value = percentage;
                this.Height = 200; // Further increase to prevent clipping
                progressBarControl1.Visible = true;
                Application.DoEvents();
            }
        }

        private void linkLabel1_LinkClicked(object sender, EventArgs e)
        {
            // cast event to cancel current operation
            CastCancelEvent();
        }
    }
}