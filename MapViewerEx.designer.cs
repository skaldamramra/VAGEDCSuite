using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace VAGSuite
{
    partial class MapViewerEx
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapViewerEx));
            
            // Main Layout Containers
            this.mainPanel = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.graphPanel = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.sliderPanel = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.bottomPanel = new ComponentFactory.Krypton.Toolkit.KryptonPanel();
            this.infoLabel = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            
            // Controls
            this.gridControl1 = new Zuby.ADGV.AdvancedDataGridView();
            
            // 3D Graph Controls
            this.nChartControl1 = new OpenTK.GLControl();
            this.btnToggleOverlay = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnToggleWireframe = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.btnToggleTooltips = new ComponentFactory.Krypton.Toolkit.KryptonButton();
            this.simpleButton4 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Rotate Left
            this.simpleButton5 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Rotate Right
            this.simpleButton6 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Zoom Out
            this.simpleButton7 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Zoom In
            
            // 2D Graph Controls
            this.nChartControl2 = new ZedGraph.ZedGraphControl();
            this.trackBarControl1 = new ComponentFactory.Krypton.Toolkit.KryptonTrackBar();
            this.labelControl8 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            this.labelControl9 = new ComponentFactory.Krypton.Toolkit.KryptonLabel();
            
            // Bottom Buttons
            this.simpleButton1 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Close
            this.simpleButton2 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Save
            this.simpleButton3 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Undo
            this.simpleButton8 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Save to ECU
            this.simpleButton9 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Read from ECU
            this.simpleButton10 = new ComponentFactory.Krypton.Toolkit.KryptonButton(); // Read
            
            this.btnGraph3D = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            this.btnGraph2D = new ComponentFactory.Krypton.Toolkit.KryptonCheckButton();
            
            // ToolStrip
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBox3 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            
            // Context Menu
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copySelectedCellsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteSelectedCellsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inOrgininalPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.atCurrentlySelectedLocationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editXaxisSymbolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editYaxisSymbolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smoothSelectionToolProportionalStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            
            // Timers
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.timer4 = new System.Windows.Forms.Timer(this.components);
            this.timer5 = new System.Windows.Forms.Timer(this.components);

            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
            this.mainPanel.SuspendLayout();
            this.mainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphPanel)).BeginInit();
            this.graphPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliderPanel)).BeginInit();
            this.sliderPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomPanel)).BeginInit();
            this.bottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();

            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.mainLayout);
            this.mainPanel.Controls.Add(this.bottomPanel);
            this.mainPanel.Controls.Add(this.infoLabel);
            this.mainPanel.Controls.Add(this.toolStrip1);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(876, 664);
            this.mainPanel.TabIndex = 0;

            // 
            // toolStrip1
            // 
            this.toolStrip1.Font = new System.Drawing.Font("Source Sans Pro", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1,
            this.toolStripTextBox1,
            this.toolStripButton3,
            this.toolStripSeparator4,
            this.toolStripLabel3,
            this.toolStripComboBox3,
            this.toolStripSeparator2,
            this.toolStripButton7,
            this.toolStripButton6,
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripButton4,
            this.toolStripButton5,
            this.toolStripSeparator1,
            this.toolStripLabel1,
            this.toolStripComboBox2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(876, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";

            // 
            // infoLabel
            // 
            this.infoLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.infoLabel.Location = new System.Drawing.Point(0, 25);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(876, 20);
            this.infoLabel.TabIndex = 2;
            this.infoLabel.Values.Text = "Map Information";
            this.infoLabel.StateCommon.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.infoLabel.Padding = new System.Windows.Forms.Padding(5, 2, 5, 2);

            // 
            // mainLayout (TableLayoutPanel)
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.gridControl1, 0, 0);
            this.mainLayout.Controls.Add(this.graphPanel, 0, 1);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 45);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 2;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.Size = new System.Drawing.Size(876, 579);
            this.mainLayout.TabIndex = 3;

            // 
            // gridControl1
            // 
            this.gridControl1.ContextMenuStrip = this.contextMenuStrip1;
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Font = new System.Drawing.Font("Source Sans Pro", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gridControl1.Location = new System.Drawing.Point(3, 3);
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(870, 283);
            this.gridControl1.TabIndex = 0;
            this.gridControl1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.gridControl1.MultiSelect = true;
            this.gridControl1.AllowUserToAddRows = false;
            this.gridControl1.AllowUserToDeleteRows = false;
            this.gridControl1.RowHeadersVisible = true;
            this.gridControl1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.advancedDataGridView1_CellPainting);
            this.gridControl1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.advancedDataGridView1_CellValueChanged);
            this.gridControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridView1_KeyDown);

            //
            // graphPanel
            //
            this.graphPanel.Controls.Add(this.btnToggleOverlay);
            this.graphPanel.Controls.Add(this.btnToggleWireframe);
            this.graphPanel.Controls.Add(this.btnToggleTooltips);
            this.graphPanel.Controls.Add(this.simpleButton4);
            this.graphPanel.Controls.Add(this.simpleButton5);
            this.graphPanel.Controls.Add(this.simpleButton6);
            this.graphPanel.Controls.Add(this.simpleButton7);
            this.graphPanel.Controls.Add(this.nChartControl1);
            this.graphPanel.Controls.Add(this.nChartControl2);
            this.graphPanel.Controls.Add(this.sliderPanel);
            this.graphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphPanel.Location = new System.Drawing.Point(3, 292);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(870, 284);
            this.graphPanel.TabIndex = 1;

            //
            // sliderPanel
            //
            this.sliderPanel.Controls.Add(this.labelControl9);
            this.sliderPanel.Controls.Add(this.labelControl8);
            this.sliderPanel.Controls.Add(this.trackBarControl1);
            this.sliderPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sliderPanel.Location = new System.Drawing.Point(0, 244);
            this.sliderPanel.Name = "sliderPanel";
            this.sliderPanel.Size = new System.Drawing.Size(870, 40);
            this.sliderPanel.TabIndex = 10;
            this.sliderPanel.Visible = false;

            //
            // nChartControl1 (3D)
            //
            this.nChartControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.nChartControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nChartControl1.Location = new System.Drawing.Point(0, 0);
            this.nChartControl1.Name = "nChartControl1";
            this.nChartControl1.Size = new System.Drawing.Size(870, 244);
            this.nChartControl1.TabIndex = 0;
            this.nChartControl1.VSync = false;
            this.nChartControl1.Visible = true;

            //
            // nChartControl2 (2D)
            //
            this.nChartControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nChartControl2.Location = new System.Drawing.Point(0, 0);
            this.nChartControl2.Name = "nChartControl2";
            this.nChartControl2.Size = new System.Drawing.Size(870, 244);
            this.nChartControl2.TabIndex = 1;
            this.nChartControl2.Visible = false;

            // Overlay Buttons (Anchored Top-Right)
            this.simpleButton7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton7.Location = new System.Drawing.Point(836, 10);
            this.simpleButton7.Name = "simpleButton7";
            this.simpleButton7.Size = new System.Drawing.Size(24, 24);
            this.simpleButton7.TabIndex = 2;
            this.simpleButton7.Values.Text = "+";
            this.simpleButton7.Click += new System.EventHandler(this.simpleButton7_Click);

            this.simpleButton6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton6.Location = new System.Drawing.Point(836, 40);
            this.simpleButton6.Name = "simpleButton6";
            this.simpleButton6.Size = new System.Drawing.Size(24, 24);
            this.simpleButton6.TabIndex = 3;
            this.simpleButton6.Values.Text = "-";
            this.simpleButton6.Click += new System.EventHandler(this.simpleButton6_Click);

            this.simpleButton4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton4.Location = new System.Drawing.Point(836, 70);
            this.simpleButton4.Name = "simpleButton4";
            this.simpleButton4.Size = new System.Drawing.Size(24, 24);
            this.simpleButton4.TabIndex = 4;
            this.simpleButton4.Values.Text = "<";
            this.simpleButton4.Click += new System.EventHandler(this.simpleButton4_Click);

            this.simpleButton5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton5.Location = new System.Drawing.Point(836, 100);
            this.simpleButton5.Name = "simpleButton5";
            this.simpleButton5.Size = new System.Drawing.Size(24, 24);
            this.simpleButton5.TabIndex = 5;
            this.simpleButton5.Values.Text = ">";
            this.simpleButton5.Click += new System.EventHandler(this.simpleButton5_Click);

            this.btnToggleOverlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleOverlay.Location = new System.Drawing.Point(836, 130);
            this.btnToggleOverlay.Name = "btnToggleOverlay";
            this.btnToggleOverlay.Size = new System.Drawing.Size(24, 24);
            this.btnToggleOverlay.TabIndex = 6;
            this.btnToggleOverlay.Visible = false;
            this.btnToggleOverlay.Click += new System.EventHandler(this.btnToggleOverlay_Click);

            this.btnToggleWireframe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleWireframe.Location = new System.Drawing.Point(836, 160);
            this.btnToggleWireframe.Name = "btnToggleWireframe";
            this.btnToggleWireframe.Size = new System.Drawing.Size(24, 24);
            this.btnToggleWireframe.TabIndex = 7;
            this.btnToggleWireframe.Values.Text = "W";
            this.btnToggleWireframe.Click += new System.EventHandler(this.btnToggleWireframe_Click);

            this.btnToggleTooltips.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnToggleTooltips.Location = new System.Drawing.Point(836, 190);
            this.btnToggleTooltips.Name = "btnToggleTooltips";
            this.btnToggleTooltips.Size = new System.Drawing.Size(24, 24);
            this.btnToggleTooltips.TabIndex = 8;
            this.btnToggleTooltips.Values.Text = "T";
            this.btnToggleTooltips.Click += new System.EventHandler(this.btnToggleTooltips_Click);

            //
            // trackBarControl1
            //
            this.trackBarControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarControl1.Location = new System.Drawing.Point(100, 5);
            this.trackBarControl1.Name = "trackBarControl1";
            this.trackBarControl1.Size = new System.Drawing.Size(670, 30);
            this.trackBarControl1.TabIndex = 9;
            this.trackBarControl1.Visible = true;
            this.trackBarControl1.ValueChanged += new System.EventHandler(this.trackBarControl1_ValueChanged);

            //
            // labelControl8
            //
            this.labelControl8.Location = new System.Drawing.Point(5, 10);
            this.labelControl8.Name = "labelControl8";
            this.labelControl8.Size = new System.Drawing.Size(90, 20);
            this.labelControl8.TabIndex = 10;
            this.labelControl8.Values.Text = "MAP values";
            this.labelControl8.Visible = true;

            //
            // labelControl9
            //
            this.labelControl9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelControl9.Location = new System.Drawing.Point(776, 10);
            this.labelControl9.Name = "labelControl9";
            this.labelControl9.Size = new System.Drawing.Size(90, 20);
            this.labelControl9.TabIndex = 11;
            this.labelControl9.Values.Text = "MAP";
            this.labelControl9.Visible = true;

            // 
            // bottomPanel
            //
            this.bottomPanel.Controls.Add(this.btnGraph2D);
            this.bottomPanel.Controls.Add(this.btnGraph3D);
            this.bottomPanel.Controls.Add(this.simpleButton3); // Undo
            this.bottomPanel.Controls.Add(this.simpleButton9); // Read ECU
            this.bottomPanel.Controls.Add(this.simpleButton8); // Save ECU
            this.bottomPanel.Controls.Add(this.simpleButton10); // Read
            this.bottomPanel.Controls.Add(this.simpleButton2); // Save
            this.bottomPanel.Controls.Add(this.simpleButton1); // Close
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 624);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(876, 40);
            this.bottomPanel.TabIndex = 4;

            // Buttons Layout
            this.simpleButton3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.simpleButton3.Location = new System.Drawing.Point(10, 8);
            this.simpleButton3.Name = "simpleButton3";
            this.simpleButton3.Size = new System.Drawing.Size(100, 25);
            this.simpleButton3.TabIndex = 0;
            this.simpleButton3.Values.Text = "Undo changes";
            this.simpleButton3.Click += new System.EventHandler(this.simpleButton3_Click);

            this.btnGraph3D.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGraph3D.Location = new System.Drawing.Point(120, 8);
            this.btnGraph3D.Name = "btnGraph3D";
            this.btnGraph3D.Size = new System.Drawing.Size(40, 25);
            this.btnGraph3D.TabIndex = 6;
            this.btnGraph3D.Values.Text = "3D";
            this.btnGraph3D.Click += new System.EventHandler(this.btnGraph3D_Click);

            this.btnGraph2D.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGraph2D.Location = new System.Drawing.Point(165, 8);
            this.btnGraph2D.Name = "btnGraph2D";
            this.btnGraph2D.Size = new System.Drawing.Size(40, 25);
            this.btnGraph2D.TabIndex = 7;
            this.btnGraph2D.Values.Text = "2D";
            this.btnGraph2D.Click += new System.EventHandler(this.btnGraph2D_Click);

            this.simpleButton9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton9.Location = new System.Drawing.Point(450, 8);
            this.simpleButton9.Name = "simpleButton9";
            this.simpleButton9.Size = new System.Drawing.Size(100, 25);
            this.simpleButton9.TabIndex = 1;
            this.simpleButton9.Values.Text = "Read from ECU";
            this.simpleButton9.Visible = false;
            this.simpleButton9.Click += new System.EventHandler(this.simpleButton9_Click);

            this.simpleButton8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton8.Location = new System.Drawing.Point(556, 8);
            this.simpleButton8.Name = "simpleButton8";
            this.simpleButton8.Size = new System.Drawing.Size(100, 25);
            this.simpleButton8.TabIndex = 2;
            this.simpleButton8.Values.Text = "Save to ECU";
            this.simpleButton8.Visible = false;
            this.simpleButton8.Click += new System.EventHandler(this.simpleButton8_Click);

            this.simpleButton10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton10.Location = new System.Drawing.Point(662, 8);
            this.simpleButton10.Name = "simpleButton10";
            this.simpleButton10.Size = new System.Drawing.Size(75, 25);
            this.simpleButton10.TabIndex = 3;
            this.simpleButton10.Values.Text = "Read";
            this.simpleButton10.Click += new System.EventHandler(this.simpleButton10_Click);

            this.simpleButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton2.Location = new System.Drawing.Point(743, 8);
            this.simpleButton2.Name = "simpleButton2";
            this.simpleButton2.Size = new System.Drawing.Size(75, 25);
            this.simpleButton2.TabIndex = 4;
            this.simpleButton2.Values.Text = "Save";
            this.simpleButton2.Click += new System.EventHandler(this.simpleButton2_Click);

            this.simpleButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleButton1.Location = new System.Drawing.Point(824, 8);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(40, 25);
            this.simpleButton1.TabIndex = 5;
            this.simpleButton1.Values.Text = "X";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);

            // ToolStrip Items Configuration
            this.toolStripComboBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.toolStripComboBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.toolStripComboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.toolStripComboBox1.Items.AddRange(new object[] { "Addition", "Multiply", "Divide", "Fill" });
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(121, 25);

            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(60, 25);
            this.toolStripTextBox1.Text = "2";
            this.toolStripTextBox1.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "Execute";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);

            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);

            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(51, 22);
            this.toolStripLabel3.Text = "Viewtype";

            this.toolStripComboBox3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.toolStripComboBox3.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.toolStripComboBox3.Items.AddRange(new object[] { "Hex view ", "Decimal view ", "Easy view" });
            this.toolStripComboBox3.Name = "toolStripComboBox3";
            this.toolStripComboBox3.Size = new System.Drawing.Size(160, 25);
            this.toolStripComboBox3.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox3_SelectedIndexChanged);
            this.toolStripComboBox3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripComboBox3_KeyDown);

            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);

            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton7.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton7.Image")));
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton7.Text = "Toggle graph/map";
            this.toolStripButton7.Click += new System.EventHandler(this.toolStripButton7_Click);

            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton6.Text = "Maximize window";
            this.toolStripButton6.Click += new System.EventHandler(this.toolStripButton6_Click);

            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Toggle graph section";
            this.toolStripButton1.Visible = false;
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);

            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "Toggle hexview";
            this.toolStripButton2.Visible = false;
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);

            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "Maximize graph";
            this.toolStripButton4.Visible = false;
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripButton4_Click);

            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "Maximize table";
            this.toolStripButton5.Visible = false;
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click);

            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);

            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(77, 22);
            this.toolStripLabel1.Text = "Axis lock mode";
            this.toolStripLabel1.Visible = false;

            this.toolStripComboBox2.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.toolStripComboBox2.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.toolStripComboBox2.Items.AddRange(new object[] { "Autoscale", "Lock to peak in maps", "Lock to map limit" });
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            this.toolStripComboBox2.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox2.Visible = false;
            this.toolStripComboBox2.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox2_SelectedIndexChanged);

            // Context Menu Items
            this.copySelectedCellsToolStripMenuItem.Name = "copySelectedCellsToolStripMenuItem";
            this.copySelectedCellsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.copySelectedCellsToolStripMenuItem.Text = "Copy selected cells";
            this.copySelectedCellsToolStripMenuItem.Click += new System.EventHandler(this.copySelectedCellsToolStripMenuItem_Click);

            this.pasteSelectedCellsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inOrgininalPositionToolStripMenuItem,
            this.atCurrentlySelectedLocationToolStripMenuItem});
            this.pasteSelectedCellsToolStripMenuItem.Name = "pasteSelectedCellsToolStripMenuItem";
            this.pasteSelectedCellsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.pasteSelectedCellsToolStripMenuItem.Text = "Paste selected cells";

            this.inOrgininalPositionToolStripMenuItem.Name = "inOrgininalPositionToolStripMenuItem";
            this.inOrgininalPositionToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.inOrgininalPositionToolStripMenuItem.Text = "At original position";
            this.inOrgininalPositionToolStripMenuItem.Click += new System.EventHandler(this.inOrgininalPositionToolStripMenuItem_Click);

            this.atCurrentlySelectedLocationToolStripMenuItem.Name = "atCurrentlySelectedLocationToolStripMenuItem";
            this.atCurrentlySelectedLocationToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.atCurrentlySelectedLocationToolStripMenuItem.Text = "At currently selected location";
            this.atCurrentlySelectedLocationToolStripMenuItem.Click += new System.EventHandler(this.atCurrentlySelectedLocationToolStripMenuItem_Click);

            this.editXaxisSymbolToolStripMenuItem.Name = "editXaxisSymbolToolStripMenuItem";
            this.editXaxisSymbolToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.editXaxisSymbolToolStripMenuItem.Text = "Edit x-axis";
            this.editXaxisSymbolToolStripMenuItem.Click += new System.EventHandler(this.editXaxisSymbolToolStripMenuItem_Click);

            this.editYaxisSymbolToolStripMenuItem.Name = "editYaxisSymbolToolStripMenuItem";
            this.editYaxisSymbolToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.editYaxisSymbolToolStripMenuItem.Text = "Edit y-axis";
            this.editYaxisSymbolToolStripMenuItem.Click += new System.EventHandler(this.editYaxisSymbolToolStripMenuItem_Click);

            this.smoothSelectionToolStripMenuItem.Name = "smoothSelectionToolStripMenuItem";
            this.smoothSelectionToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.smoothSelectionToolStripMenuItem.Text = "Smooth selection";
            this.smoothSelectionToolStripMenuItem.Click += new System.EventHandler(this.smoothSelectionToolStripMenuItem_Click);

            this.smoothSelectionToolProportionalStripMenuItem.Name = "smoothSelectionToolProportionalStripMenuItem";
            this.smoothSelectionToolProportionalStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.smoothSelectionToolProportionalStripMenuItem.Text = "Proportional Smooth selection";
            this.smoothSelectionToolProportionalStripMenuItem.Click += new System.EventHandler(this.smoothSelectionToolProportionalStripMenuItem_Click);

            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copySelectedCellsToolStripMenuItem,
            this.pasteSelectedCellsToolStripMenuItem,
            this.editXaxisSymbolToolStripMenuItem,
            this.editYaxisSymbolToolStripMenuItem,
            this.smoothSelectionToolStripMenuItem,
            this.smoothSelectionToolProportionalStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(179, 114);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);

            // Timers
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            this.timer4.Enabled = true;
            this.timer4.Interval = 500;
            this.timer4.Tick += new System.EventHandler(this.timer4_Tick);
            this.timer5.Interval = 10000;
            this.timer5.Tick += new System.EventHandler(this.timer5_Tick);

            // MapViewerEx
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Name = "MapViewerEx";
            this.Size = new System.Drawing.Size(876, 664);
            this.VisibleChanged += new System.EventHandler(this.MapViewer_VisibleChanged);

            ((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.mainLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.graphPanel)).EndInit();
            this.graphPanel.ResumeLayout(false);
            this.graphPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sliderPanel)).EndInit();
            this.sliderPanel.ResumeLayout(false);
            this.sliderPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomPanel)).EndInit();
            this.bottomPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private ComponentFactory.Krypton.Toolkit.KryptonPanel mainPanel;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel graphPanel;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel sliderPanel;
        private ComponentFactory.Krypton.Toolkit.KryptonPanel bottomPanel;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel infoLabel;
        
        public Zuby.ADGV.AdvancedDataGridView gridControl1;
        
        private OpenTK.GLControl nChartControl1;
        private ZedGraph.ZedGraphControl nChartControl2;
        
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton1;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton2;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton3;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton4;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton5;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton6;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton7;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton8;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton9;
        private ComponentFactory.Krypton.Toolkit.KryptonButton simpleButton10;
        
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton btnGraph3D;
        private ComponentFactory.Krypton.Toolkit.KryptonCheckButton btnGraph2D;

        private ComponentFactory.Krypton.Toolkit.KryptonButton btnToggleOverlay;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnToggleWireframe;
        private ComponentFactory.Krypton.Toolkit.KryptonButton btnToggleTooltips;
        
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl9;
        private ComponentFactory.Krypton.Toolkit.KryptonLabel labelControl8;
        private ComponentFactory.Krypton.Toolkit.KryptonTrackBar trackBarControl1;
        
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton6;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox2;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox3;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copySelectedCellsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteSelectedCellsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inOrgininalPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem atCurrentlySelectedLocationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editXaxisSymbolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editYaxisSymbolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothSelectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem smoothSelectionToolProportionalStripMenuItem;
        
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Timer timer4;
        private System.Windows.Forms.Timer timer5;
    }
}
