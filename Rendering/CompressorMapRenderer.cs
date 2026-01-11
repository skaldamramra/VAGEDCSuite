using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using VAGSuite.Models;
using VAGSuite.Theming;

namespace VAGSuite.Rendering
{
    public class CompressorMapRenderer
    {
        private CompressorMapCoordinates _currentCoords;
        private Bitmap _currentMapImage;

        public CompressorMapRenderer()
        {
        }

        public void LoadMap(CompressorMapType mapType)
        {
            _currentCoords = GetCoordinates(mapType);
            if (_currentCoords != null && !string.IsNullOrEmpty(_currentCoords.ResourceName))
            {
                try
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_currentCoords.ResourceName))
                    {
                        if (stream != null)
                        {
                            _currentMapImage = new Bitmap(stream);
                            _currentCoords.OriginalWidth = _currentMapImage.Width;
                            _currentCoords.OriginalHeight = _currentMapImage.Height;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading map image: " + ex.Message);
                    _currentMapImage = null;
                }
            }
            else
            {
                _currentMapImage = null;
            }
        }

        public void Render(Graphics g, int width, int height, List<CompressorPlotPoint> points)
        {
            // Fill background with VAGEDC Dark
            g.Clear(VAGEDCColorPalette.Gray900);

            if (_currentMapImage != null)
            {
                // Draw map image scaled to fit
                g.DrawImage(_currentMapImage, 0, 0, width, height);
            }
            else
            {
                // Draw "No Map" placeholder
                using (var font = new Font("Segoe UI", 12))
                using (var brush = new SolidBrush(VAGEDCColorPalette.TextSecondaryDark))
                {
                    g.DrawString("Select a compressor map or view plot only.", font, brush, 20, 20);
                }
            }

            if (points == null || points.Count == 0 || _currentCoords == null) return;

            // Draw Plot Points
            RenderPoints(g, width, height, points);
        }

        private void RenderPoints(Graphics g, int currentWidth, int height, List<CompressorPlotPoint> points)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Use VAGEDC Theme colors
            Color nominalColor = VAGEDCColorPalette.Success500;
            using (Pen linePen = new Pen(nominalColor, 3))
            using (SolidBrush pointBrush = new SolidBrush(nominalColor))
            {
                PointF? prevPoint = null;

                foreach (var pt in points)
                {
                    PointF pixelPt = TranslateToPixels(pt.MassAirflowLbsMin, pt.PressureRatio, currentWidth, height);

                    // Draw Line
                    if (prevPoint.HasValue)
                    {
                        g.DrawLine(linePen, prevPoint.Value, pixelPt);
                    }

                    // Draw Point
                    g.FillEllipse(pointBrush, pixelPt.X - 4, pixelPt.Y - 4, 8, 8);
                    
                    // Draw RPM Label (optional, every few points to avoid clutter)
                    if (pt.RPM % 1000 == 0 || pt.RPM == points[0].RPM || pt.RPM == points[points.Count-1].RPM)
                    {
                        using (Font f = new Font("Segoe UI", 8))
                        {
                            g.DrawString(pt.RPM.ToString(), f, Brushes.White, pixelPt.X + 5, pixelPt.Y - 15);
                        }
                    }

                    prevPoint = pixelPt;
                }
            }
        }

        private PointF TranslateToPixels(double massFlow, double pr, int currentWidth, int height)
        {
            if (_currentCoords == null) return PointF.Empty;

            // Adjust multipliers based on current control size vs original image size
            double scaleX = (double)currentWidth / _currentCoords.OriginalWidth;
            double scaleY = (double)height / _currentCoords.OriginalHeight;

            // Pressure ratio on maps is usually Gauge + 1 (Absolute), but the PR calculation already gives absolute.
            // The maps in ctrlCompressorMap seem to use PR-1 for the Y axis multiplier logic in some cases?
            // Let's re-examine ctrlCompressorMap.cs:372
            // float y_location = (float)(y_offset - pressureRatioAtmNom * y_multiplier);
            // And pressureRatioAtmNom was calculated then -= 1 (line 365).
            // So the multipliers are "per Bar boost" (Gauge PR).
            
            double gaugePR = pr - 1.0;

            float x = (float)((_currentCoords.XOffset + (massFlow * _currentCoords.XMultiplier)) * scaleX);
            float y = (float)((_currentCoords.YOffset - (gaugePR * _currentCoords.YMultiplier)) * scaleY);

            return new PointF(x, y);
        }

        private CompressorMapCoordinates GetCoordinates(CompressorMapType type)
        {
            // Data extracted from ctrlCompressorMap.cs
            switch (type)
            {
                case CompressorMapType.GT17:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.GT17.jpg", XOffset = 42, YOffset = 539, XMultiplier = 10.67, YMultiplier = 166 };
                case CompressorMapType.T25_55:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.t25_55_saab.gif", XOffset = 64, YOffset = 865, XMultiplier = 20, YMultiplier = 396 };
                case CompressorMapType.T25_60:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.t25-60trim.gif", XOffset = 60, YOffset = 867, XMultiplier = 17.28, YMultiplier = 398 };
                case CompressorMapType.GT2871R:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.gt2871r-48.jpg", XOffset = 50, YOffset = 595, XMultiplier = 9.56, YMultiplier = 276.5 };
                case CompressorMapType.GT28RS:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.gt28rscompress.gif", XOffset = 55, YOffset = 460, XMultiplier = 8, YMultiplier = 211 };
                case CompressorMapType.GT30R:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.gt30rcompress.gif", XOffset = 50, YOffset = 463, XMultiplier = 6.4, YMultiplier = 158 };
                case CompressorMapType.TD04:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.td04-15g-cfm.gif", XOffset = 66, YOffset = 576, XMultiplier = 10.45, YMultiplier = 234.5 };
                case CompressorMapType.TD0416T:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.td04h-16t-cfm.gif", XOffset = 64, YOffset = 573, XMultiplier = 8.27, YMultiplier = 233 };
                case CompressorMapType.TD0418T:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.td04h-18t-cfm.gif", XOffset = 65, YOffset = 576, XMultiplier = 8.27, YMultiplier = 234 };
                case CompressorMapType.TD0419T:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.td04h-19t-cfm.gif", XOffset = 65, YOffset = 576, XMultiplier = 8.27, YMultiplier = 234 };
                case CompressorMapType.TD0620G:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.td06h-20g-cfm.gif", XOffset = 58, YOffset = 577, XMultiplier = 8.30, YMultiplier = 235 };
                case CompressorMapType.GT3071R86:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.GT3071R86.jpg", XOffset = 42, YOffset = 556, XMultiplier = 6.67, YMultiplier = 171 };
                case CompressorMapType.GT40R:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.gt40rcompress.gif", XOffset = 54, YOffset = 482, XMultiplier = 5.31, YMultiplier = 171 };
                case CompressorMapType.HX40W:
                    return new CompressorMapCoordinates { MapType = type, ResourceName = "T7.Compressormaps.hx40w.jpg", XOffset = 35, YOffset = 762, XMultiplier = 5.03, YMultiplier = 167 };
                default:
                    return null;
            }
        }
    }
}
