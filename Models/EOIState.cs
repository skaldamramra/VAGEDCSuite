namespace VAGSuite.Models
{
    /// <summary>
    /// State container for EOI 3D visualization
    /// </summary>
    public class EOIState
    {
        /// <summary>
        /// The calculated EOI map to display
        /// </summary>
        public EOIMap EOIMap { get; set; }

        /// <summary>
        /// Metadata for axis labels and titles
        /// </summary>
        public MapMetadata Metadata { get; set; }

        /// <summary>
        /// Whether to show wireframe mode
        /// </summary>
        public bool WireframeMode { get; set; }

        /// <summary>
        /// Whether tooltips are enabled
        /// </summary>
        public bool TooltipsEnabled { get; set; } = true;

        /// <summary>
        /// Current rotation angle (0-360 degrees)
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Current elevation angle (-60 to 60 degrees)
        /// </summary>
        public float Elevation { get; set; }

        /// <summary>
        /// Current zoom level (0.5 to 5.0)
        /// </summary>
        public float Zoom { get; set; } = 1.2f;

        /// <summary>
        /// Creates an EOIState from calculation result
        /// </summary>
        public static EOIState FromResult(EOICalculationResult result, string mapName = "EOI Map")
        {
            return new EOIState
            {
                EOIMap = result.EOIMap,
                Metadata = new MapMetadata
                {
                    Name = mapName,
                    Description = "End of Injection calculation result",
                    XAxisName = "RPM",
                    YAxisName = "IQ (mg/st)",
                    ZAxisName = "EOI (degrees)"
                },
                WireframeMode = false,
                TooltipsEnabled = true,
                Rotation = 0f,
                Elevation = 25f,
                Zoom = 1.2f
            };
        }
    }
}