using System;
using System.Collections.Generic;
using VAGSuite.Models;

namespace VAGSuite.Rendering
{
    public class CompressorVectorMap
    {
        public List<PointD> SurgeLine { get; set; } = new List<PointD>();
        public List<EfficiencyIsland> EfficiencyIslands { get; set; } = new List<EfficiencyIsland>();
        public List<PointD> ChokeLine { get; set; } = new List<PointD>();
    }

    public class EfficiencyIsland
    {
        public string Label { get; set; }
        public List<PointD> Points { get; set; } = new List<PointD>();
    }

    public struct PointD
    {
        public double X;
        public double Y;
        public PointD(double x, double y) { X = x; Y = y; }
    }

    public class CompressorMapRenderer
    {
        public CompressorMapRenderer()
        {
        }

        public CompressorVectorMap GetVectorMap(CompressorMapType type)
        {
            var map = new CompressorVectorMap();
            switch (type)
            {
                case CompressorMapType.GT12:
                    // Digitized from GT12.jpg (GT12 41mm, 50 trim, 0.33 A/R)
                    map.SurgeLine.AddRange(new[] {
                        new PointD(2.5, 1.4), new PointD(3.0, 1.6), new PointD(4.0, 1.8),
                        new PointD(5.0, 2.1), new PointD(6.0, 2.4), new PointD(7.0, 2.6),
                        new PointD(8.0, 2.8), new PointD(9.5, 3.1)
                    });
                    map.ChokeLine.AddRange(new[] {
                        new PointD(10.0, 3.1), new PointD(12.0, 2.8), new PointD(12.5, 2.4),
                        new PointD(13.0, 2.0), new PointD(13.5, 1.6), new PointD(14.0, 1.2)
                    });
                    map.EfficiencyIslands.Add(new EfficiencyIsland { Label = "75%", Points = new List<PointD> { new PointD(7.5, 2.0), new PointD(8.5, 2.5), new PointD(10.0, 2.5), new PointD(10.5, 2.0), new PointD(9.0, 1.8), new PointD(7.5, 2.0) } });
                    map.EfficiencyIslands.Add(new EfficiencyIsland { Label = "72%", Points = new List<PointD> { new PointD(6.0, 1.8), new PointD(7.5, 2.8), new PointD(11.0, 2.8), new PointD(12.0, 1.8), new PointD(9.0, 1.6), new PointD(6.0, 1.8) } });
                    map.EfficiencyIslands.Add(new EfficiencyIsland { Label = "70%", Points = new List<PointD> { new PointD(5.0, 1.6), new PointD(7.0, 3.0), new PointD(12.0, 3.0), new PointD(13.0, 1.6), new PointD(9.0, 1.4), new PointD(5.0, 1.6) } });
                    break;

                case CompressorMapType.GT15:
                    // Digitized from GT15.jpg (GT15 45mm, 60 trim, 0.48 A/R)
                    map.SurgeLine.AddRange(new[] {
                        new PointD(3.5, 1.35), new PointD(5.0, 1.5), new PointD(7.0, 1.7),
                        new PointD(9.0, 2.0), new PointD(11.0, 2.3), new PointD(12.0, 2.55)
                    });
                    map.ChokeLine.AddRange(new[] {
                        new PointD(18.0, 2.5), new PointD(20.0, 2.3), new PointD(22.0, 2.0),
                        new PointD(23.0, 1.7), new PointD(24.0, 1.4)
                    });
                    map.EfficiencyIslands.Add(new EfficiencyIsland { Label = "75%", Points = new List<PointD> { new PointD(9.5, 1.45), new PointD(10.5, 1.65), new PointD(11.5, 1.45), new PointD(10.5, 1.35), new PointD(9.5, 1.45) } });
                    map.EfficiencyIslands.Add(new EfficiencyIsland { Label = "74%", Points = new List<PointD> { new PointD(8.0, 1.4), new PointD(11.0, 2.0), new PointD(15.0, 2.0), new PointD(16.0, 1.4), new PointD(12.0, 1.25), new PointD(8.0, 1.4) } });
                    map.EfficiencyIslands.Add(new EfficiencyIsland { Label = "72%", Points = new List<PointD> { new PointD(6.5, 1.4), new PointD(12.0, 2.4), new PointD(18.0, 2.4), new PointD(20.0, 1.4), new PointD(13.0, 1.2), new PointD(6.5, 1.4) } });
                    break;

                case CompressorMapType.GT20:
                    // Digitized from GT20.jpg (GT20 56mm, 55 trim, 0.53 A/R)
                    // Surge line (left border of map)
                    map.SurgeLine.AddRange(new[] {
                        new PointD(6.0, 1.4), new PointD(5.5, 1.6), new PointD(6.5, 1.8),
                        new PointD(7.5, 2.0), new PointD(9.0, 2.2), new PointD(10.5, 2.4),
                        new PointD(12.5, 2.6), new PointD(15.0, 2.7), new PointD(18.0, 2.8)
                    });
                    // Choke line (right border of map)
                    map.ChokeLine.AddRange(new[] {
                        new PointD(18.0, 2.8), new PointD(20.0, 2.8), new PointD(22.0, 2.75),
                        new PointD(24.0, 2.65), new PointD(26.0, 2.5), new PointD(27.0, 2.35),
                        new PointD(28.0, 2.2), new PointD(28.5, 2.0), new PointD(29.0, 1.8)
                    });
                    // 79% efficiency island (innermost)
                    map.EfficiencyIslands.Add(new EfficiencyIsland {
                        Label = "79%",
                        Points = new List<PointD> {
                            new PointD(14.0, 1.6), new PointD(13.8, 1.8), new PointD(14.0, 2.0),
                            new PointD(15.0, 2.2), new PointD(17.0, 2.4), new PointD(19.0, 2.5),
                            new PointD(21.0, 2.5), new PointD(21.5, 2.4), new PointD(21.5, 2.2),
                            new PointD(21.0, 2.0), new PointD(19.0, 1.8), new PointD(17.0, 1.7),
                            new PointD(14.0, 1.6)
                        }
                    });
                    // 75% efficiency island (middle)
                    map.EfficiencyIslands.Add(new EfficiencyIsland {
                        Label = "75%",
                        Points = new List<PointD> {
                            new PointD(13.0, 1.4), new PointD(10.0, 1.6), new PointD(9.5, 1.8),
                            new PointD(10.5, 2.0), new PointD(12.0, 2.3), new PointD(14.0, 2.5),
                            new PointD(16.0, 2.65), new PointD(18.0, 2.7), new PointD(20.0, 2.7),
                            new PointD(22.0, 2.6), new PointD(24.0, 2.4), new PointD(26.0, 2.2),
                            new PointD(27.0, 2.0), new PointD(26.0, 1.8), new PointD(23.0, 1.6),
                            new PointD(18.0, 1.45), new PointD(13.0, 1.4)
                        }
                    });
                    // 65% efficiency island (outer)
                    map.EfficiencyIslands.Add(new EfficiencyIsland {
                        Label = "65%",
                        Points = new List<PointD> {
                            new PointD(12.0, 1.25), new PointD(9.0, 1.3), new PointD(7.0, 1.4),
                            new PointD(6.0, 1.6), new PointD(8.0, 2.0), new PointD(10.0, 2.3),
                            new PointD(14.0, 2.7), new PointD(18.0, 2.8), new PointD(22.0, 2.75),
                            new PointD(26.0, 2.5), new PointD(28.0, 2.2), new PointD(29.0, 1.8),
                            new PointD(29.0, 1.7), new PointD(27.0, 1.5), new PointD(24.0, 1.4),
                            new PointD(20.0, 1.3), new PointD(15.0, 1.25), new PointD(12.0, 1.25)
                        }
                    });
                    break;
            }
            return map;
        }
    }
}
