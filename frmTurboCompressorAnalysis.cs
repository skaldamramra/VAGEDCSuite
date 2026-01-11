using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using ComponentFactory.Krypton.Toolkit;
using VAGSuite.Models;
using VAGSuite.Services;
using VAGSuite.Rendering;
using VAGSuite.Theming;

namespace VAGSuite
{
    public partial class frmTurboCompressorAnalysis : KryptonForm
    {
        private readonly TurboCompressorService _turboService;
        private readonly CompressorMapRenderer _renderer;
        private EngineParameters _engineParams;
        private BoostMapData _boostMap;
        private List<CompressorPlotPoint> _plotPoints;
        private List<SymbolHelper> _availableBoostMaps;

        public frmTurboCompressorAnalysis(IEDCFileParser parser, SymbolCollection symbols)
        {
            _turboService = new TurboCompressorService(parser, symbols);
            _renderer = new CompressorMapRenderer();
            InitializeComponent();
            
            // Set the application icon
            this.Icon = new System.Drawing.Icon("vagedc.ico");
            
            ApplyTheme();
            InitializeGraph();
            LoadInitialData();
            
            // Wire up tooltips
            zedMap.IsShowPointValues = true;
            zedMap.PointValueEvent += new ZedGraphControl.PointValueHandler(ZedMap_PointValueEvent);
        }

        private string ZedMap_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {
            if (_plotPoints == null || iPt < 0 || iPt >= _plotPoints.Count) return string.Empty;

            var pt = _plotPoints[iPt];
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"RPM: {pt.RPM:F0}");
            sb.AppendLine($"Injected Quantity: {pt.InjectedQuantityMgStroke:F1} mg/st");
            sb.AppendLine($"Boost Target: {pt.BoostPressureBar:F3} bar (abs)");
            sb.AppendLine($"Pressure Ratio: {pt.PressureRatio:F2}");
            sb.AppendLine($"Airflow: {pt.MassAirflowLbsMin:F2} lbs/min (corrected)");
            sb.AppendLine($"Volume Flow: {pt.VolumeFlowCFM:F1} CFM");
            sb.AppendLine($"Est. Manifold Temp: {pt.IntakeManifoldTempC:F1} °C");
            
            if (_boostMap != null)
            {
                sb.AppendLine($"Debug Map: {_boostMap.MapName}");
                sb.AppendLine($"Debug Points: {iPt + 1}/{_plotPoints.Count}");
                sb.AppendLine($"Debug Size: X:{_boostMap.DebugXLen}, Y:{_boostMap.DebugYLen}");
                sb.AppendLine($"Debug X: {_boostMap.DebugXDescr}");
                sb.AppendLine($"Debug Y: {_boostMap.DebugYDescr}");
            }
            
            return sb.ToString();
        }

        private void ApplyTheme()
        {
            VAGEDCThemeManager.Instance.ApplyThemeToForm(this);
            this.BackColor = VAGEDCColorPalette.Gray900;
            
            // Set specific colors for labels if they don't inherit
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is KryptonLabel lbl)
                {
                    lbl.StateNormal.ShortText.Color1 = VAGEDCColorPalette.TextPrimaryDark;
                }
            }
        }

        private void InitializeGraph()
        {
            GraphPane myPane = zedMap.GraphPane;
            var theme = VAGEDCThemeManager.Instance.CurrentTheme;
            Color darkBg = VAGEDCColorPalette.Gray900;
            Color gridColor = VAGEDCColorPalette.Gray700;
            Color offWhite = VAGEDCColorPalette.TextPrimaryDark;

            zedMap.BackColor = darkBg;
            myPane.Fill = new Fill(darkBg);
            myPane.Chart.Fill = new Fill(darkBg);
            myPane.Chart.Border.Color = gridColor;
            myPane.Legend.IsVisible = true;
            myPane.Legend.Fill = new Fill(darkBg);
            myPane.Legend.FontSpec.FontColor = offWhite;

            myPane.Title.Text = "Turbo Compressor Analysis";
            myPane.Title.FontSpec.FontColor = offWhite;
            myPane.Title.FontSpec.Family = "Source Sans Pro";
            myPane.Title.FontSpec.IsBold = true;

            myPane.XAxis.Title.Text = "Corrected Air Flow (lbs/min)";
            myPane.XAxis.Color = gridColor;
            myPane.XAxis.Title.FontSpec.FontColor = offWhite;
            myPane.XAxis.Title.FontSpec.Family = "Source Sans Pro";
            myPane.XAxis.Scale.FontSpec.FontColor = offWhite;
            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.XAxis.MajorGrid.Color = Color.FromArgb(50, gridColor);

            myPane.YAxis.Title.Text = "Pressure Ratio";
            myPane.YAxis.Color = gridColor;
            myPane.YAxis.Title.FontSpec.FontColor = offWhite;
            myPane.YAxis.Title.FontSpec.Family = "Source Sans Pro";
            myPane.YAxis.Scale.FontSpec.FontColor = offWhite;
            myPane.YAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.Color = Color.FromArgb(50, gridColor);
        }

        private void LoadInitialData()
        {
            _engineParams = _turboService.ExtractEngineParameters(Tools.Instance.m_currentfile);
            
            // Populate Engine UI
            txtDisplacement.Text = _engineParams.DisplacementLiters.ToString("N1");
            txtCylinders.Text = _engineParams.NumberOfCylinders.ToString();

            // Populate Boost Map Dropdown
            _availableBoostMaps = _turboService.GetAllBoostMaps(Tools.Instance.m_symbols);
            cmbBoostMap.Items.Clear();
            foreach (var sym in _availableBoostMaps)
            {
                string displayName = $"{sym.Varname} (0x{sym.Flash_start_address:X}) [CB: {sym.CodeBlock}]";
                cmbBoostMap.Items.Add(displayName);
            }

            if (cmbBoostMap.Items.Count > 0)
            {
                cmbBoostMap.SelectedIndex = 0;
            }

            // Populate Compressor Map Dropdown
            cmbCompressorMap.Items.Clear();
            foreach (var name in Enum.GetNames(typeof(CompressorMapType)))
            {
                cmbCompressorMap.Items.Add(name);
            }
            cmbCompressorMap.SelectedItem = CompressorMapType.None.ToString();
        }

        private void cmbBoostMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBoostMap.SelectedIndex >= 0 && cmbBoostMap.SelectedIndex < _availableBoostMaps.Count)
            {
                var selectedSymbol = _availableBoostMaps[cmbBoostMap.SelectedIndex];
                _boostMap = _turboService.ExtractBoostMap(Tools.Instance.m_currentfile, selectedSymbol);
                UpdatePlot();
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            UpdatePlot();
        }

        private void UpdatePlot()
        {
            var envParams = new EnvironmentalParameters
            {
                AltitudeMeters = (double)numAltitude.Value,
                AmbientTempCelsius = (double)numTemp.Value,
                VolumetricEfficiencyPercent = (double)numVE.Value
            };

            // Update displacement from UI if user edited it
            if (double.TryParse(txtDisplacement.Text, out double disp))
            {
                _engineParams.DisplacementLiters = disp;
            }

            // Ensure we have a map selected
            if (_boostMap == null && cmbBoostMap.SelectedIndex >= 0)
            {
                var selectedSymbol = _availableBoostMaps[cmbBoostMap.SelectedIndex];
                _boostMap = _turboService.ExtractBoostMap(Tools.Instance.m_currentfile, selectedSymbol);
            }

            _plotPoints = _turboService.GeneratePlotPoints(_boostMap, _engineParams, envParams);
            
            GraphPane myPane = zedMap.GraphPane;
            myPane.CurveList.Clear();
            myPane.GraphObjList.Clear(); // Clear labels
            
            // Ensure axes auto-scale to show new data
            myPane.XAxis.Scale.MinAuto = true;
            myPane.XAxis.Scale.MaxAuto = true;
            myPane.YAxis.Scale.MinAuto = true;
            myPane.YAxis.Scale.MaxAuto = true;

            // Draw Compressor Map if selected
            if (Enum.TryParse(cmbCompressorMap.Text, out CompressorMapType mapType) && mapType != CompressorMapType.None)
            {
                // Toggle tooltips based on UI
                zedMap.IsShowPointValues = chkTooltips.Checked;

                var vectorMap = _renderer.GetVectorMap(mapType);
                if (vectorMap != null)
                {
                    // Plot Surge Line
                    if (vectorMap.SurgeLine.Count > 0)
                    {
                        PointPairList surgeList = new PointPairList();
                        foreach (var p in vectorMap.SurgeLine) surgeList.Add(p.X, p.Y);
                        LineItem surgeCurve = myPane.AddCurve("Surge Line", surgeList, Color.FromArgb(200, 244, 71, 71), SymbolType.None);
                        surgeCurve.Line.Width = 3.0f;
                        surgeCurve.Line.IsAntiAlias = true;
                        surgeCurve.Label.IsVisible = true;
                    }

                    // Plot Choke Line
                    if (vectorMap.ChokeLine.Count > 0)
                    {
                        PointPairList chokeList = new PointPairList();
                        foreach (var p in vectorMap.ChokeLine) chokeList.Add(p.X, p.Y);
                        LineItem chokeCurve = myPane.AddCurve("Choke Line", chokeList, Color.FromArgb(150, 100, 100, 100), SymbolType.None);
                        chokeCurve.Line.Width = 2.0f;
                        chokeCurve.Line.IsAntiAlias = true;
                        chokeCurve.Label.IsVisible = true;
                    }

                    // Plot Efficiency Islands
                    bool firstIsland = true;
                    foreach (var island in vectorMap.EfficiencyIslands)
                    {
                        PointPairList islandList = new PointPairList();
                        foreach (var p in island.Points) islandList.Add(p.X, p.Y);
                        
                        LineItem islandCurve = myPane.AddCurve(firstIsland ? "Efficiency" : string.Empty, islandList, Color.FromArgb(150, 150, 150, 150), SymbolType.None);
                        islandCurve.Line.Width = 1.0f;
                        islandCurve.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
                        islandCurve.Line.IsAntiAlias = true;
                        islandCurve.Line.IsSmooth = true;
                        islandCurve.Line.SmoothTension = 0.5f;
                        islandCurve.Label.IsVisible = firstIsland;
                        firstIsland = false;

                        // Add efficiency label text
                        if (island.Points.Count > 0)
                        {
                            var midPt = island.Points[island.Points.Count / 2];
                            TextObj text = new TextObj(island.Label, (float)midPt.X, (float)midPt.Y, CoordType.AxisXYScale, AlignH.Center, AlignV.Center);
                            text.FontSpec.FontColor = Color.FromArgb(180, 180, 180, 180);
                            text.FontSpec.Fill.IsVisible = false;
                            text.FontSpec.Border.IsVisible = false;
                            text.FontSpec.Size = 8;
                            myPane.GraphObjList.Add(text);
                        }
                    }
                }
            }

            if (_plotPoints != null && _plotPoints.Count > 0)
            {
                PointPairList list = new PointPairList();
                foreach (var pt in _plotPoints)
                {
                    list.Add(pt.MassAirflowLbsMin, pt.PressureRatio);
                }

                LineItem curve = myPane.AddCurve("Boost Curve", list, VAGEDCColorPalette.Primary500, SymbolType.Circle);
                curve.Line.Width = 3.0f;
                curve.Line.IsSmooth = true;
                curve.Line.SmoothTension = 0.5f;
                curve.Symbol.Size = 8.0f;
                curve.Symbol.Fill = new Fill(VAGEDCColorPalette.Gray900);
                curve.Symbol.Border.Color = VAGEDCColorPalette.Primary500;
                curve.Symbol.Border.Width = 2.0f;
            }

            zedMap.AxisChange();
            zedMap.Invalidate();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "PNG Image|*.png";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    zedMap.GetImage().Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void cmbCompressorMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePlot();
        }

        private void chkTooltips_CheckedChanged(object sender, EventArgs e)
        {
            zedMap.IsShowPointValues = chkTooltips.Checked;
            zedMap.Invalidate();
        }

    }
}
