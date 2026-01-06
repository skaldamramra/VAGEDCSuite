using System;
using System.Drawing;
using System.Windows.Forms;

namespace VAGSuite.Services
{
    /// <summary>
    /// Custom tooltip form that supports automatic text wrapping.
    /// This solves the limitation of WinForms.ToolTip which doesn't wrap long text.
    /// </summary>
    internal class WrappedTooltipForm : Form
    {
        private Label _contentLabel;
        private const int MaxWidth = 600; // Maximum width before wrapping occurs

        public string Content
        {
            get => _contentLabel.Text;
            set
            {
                _contentLabel.Text = value ?? string.Empty;
                AdjustSize();
            }
        }

        public WrappedTooltipForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form setup - tool window style without caption
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            
            // Create the content label with word wrap enabled
            _contentLabel = new Label();
            _contentLabel.AutoSize = true; // Let label size itself based on content
            _contentLabel.Font = new Font("Segoe UI", 9F);
            _contentLabel.ForeColor = SystemColors.InfoText;
            _contentLabel.BackColor = SystemColors.Info;
            _contentLabel.Padding = new Padding(8);
            
            // Use a FlowLayoutPanel to handle wrapping properly
            FlowLayoutPanel container = new FlowLayoutPanel();
            container.FlowDirection = FlowDirection.TopDown;
            container.AutoSize = true;
            container.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            container.BorderStyle = BorderStyle.FixedSingle;
            container.BackColor = SystemColors.Info;
            container.Padding = new Padding(0);
            container.Margin = new Padding(0);
            
            // Add label to container
            _contentLabel.Margin = new Padding(0);
            container.Controls.Add(_contentLabel);
            
            this.Controls.Add(container);
        }

        /// <summary>
        /// Shows the tooltip at the specified screen location.
        /// </summary>
        /// <param name="owner">The control this tooltip is associated with.</param>
        /// <param name="screenLocation">Screen coordinates where tooltip should appear.</param>
        public void ShowAt(Control owner, Point screenLocation)
        {
            if (owner == null || owner.IsDisposed) return;

            // Position the form
            this.Location = new Point(screenLocation.X + 15, screenLocation.Y + 15);
            
            // Ensure form is visible and on top
            this.Show();
            this.BringToFront();
        }

        /// <summary>
        /// Adjusts the form size based on content.
        /// </summary>
        private void AdjustSize()
        {
            // Force the label to calculate its wrapped size
            _contentLabel.MaximumSize = new Size(MaxWidth, 0);
            
            // The FlowLayoutPanel will auto-size to fit the label
            // Just ensure the form is sized correctly
            this.Size = this.Controls[0].Size;
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void HideAnimated()
        {
            this.Hide();
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // Add WS_EX_NOACTIVATE to prevent stealing focus
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return cp;
            }
        }
    }
}