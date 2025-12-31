using System.Drawing;

namespace VAGSuite.Theming
{
    /// <summary>
    /// Defines a complete theme with all color assignments
    /// </summary>
    public class VAGEDCTheme
    {
        public string Name { get; set; }
        public bool IsDarkMode { get; set; }
        
        // Window/Form colors
        public Color WindowBackground { get; set; }
        public Color PanelBackground { get; set; }
        public Color CardBackground { get; set; }
        
        // Text colors
        public Color TextPrimary { get; set; }
        public Color TextSecondary { get; set; }
        
        // Border colors
        public Color BorderPrimary { get; set; }
        
        // Control colors
        public Color ControlBackground { get; set; }
        public Color ControlHover { get; set; }
        public Color ControlActive { get; set; }
        public Color ControlDisabled { get; set; }
        
        // Grid/Table colors
        public Color GridBackground { get; set; }
        public Color GridAlternateRow { get; set; }
        public Color GridSelection { get; set; }
        public Color GridBorder { get; set; }
        public Color GridHeaderBackground { get; set; }
        public Color GridHeaderText { get; set; }
        public Color GridHoverRow { get; set; }
        public Font GridHeaderFont { get; set; }
        public Font GridCellFont { get; set; }
        
        // Toolbar/StatusBar colors
        public Color ToolbarBackground { get; set; }
        public Color ToolbarText { get; set; }
        public Color ToolbarHover { get; set; }
        public Color StatusBarBackground { get; set; }
        public Color StatusBarText { get; set; }
        
        // Semantic colors (same for both modes)
        public Color AccentPrimary { get; set; }
        public Color AccentSecondary { get; set; }
        public Color Warning { get; set; }
        public Color Success { get; set; }
        public Color Danger { get; set; }
        
        /// <summary>
        /// Creates the VAGEDC Dark theme
        /// </summary>
        public static VAGEDCTheme CreateDarkTheme()
        {
            return new VAGEDCTheme
            {
                Name = "VAGEDC Dark",
                IsDarkMode = true,
                
                // Window/Form
                WindowBackground = VAGEDCColorPalette.Gray900,
                PanelBackground = VAGEDCColorPalette.Gray800,
                CardBackground = VAGEDCColorPalette.Gray700,
                
                // Text
                TextPrimary = VAGEDCColorPalette.TextPrimaryDark,
                TextSecondary = VAGEDCColorPalette.TextSecondaryDark,
                
                // Borders
                BorderPrimary = VAGEDCColorPalette.Gray600,
                
                // Controls
                ControlBackground = VAGEDCColorPalette.Gray800,
                ControlHover = VAGEDCColorPalette.Gray700,
                ControlActive = VAGEDCColorPalette.Primary500,
                ControlDisabled = VAGEDCColorPalette.Gray500,
                
                // Grid/Table
                GridBackground = VAGEDCColorPalette.Gray800,
                GridAlternateRow = VAGEDCColorPalette.Gray700,
                GridSelection = VAGEDCColorPalette.Primary500,
                GridBorder = VAGEDCColorPalette.Gray600,
                GridHeaderBackground = VAGEDCColorPalette.Gray800,  // Changed from Gray700 to Gray800 for darker headers
                GridHeaderText = VAGEDCColorPalette.TextPrimaryDark,
                GridHoverRow = Color.FromArgb(128, 45, 55, 72),  // Gray700 at 50% opacity (#2D3748)
                GridHeaderFont = new Font("Segoe UI", 12f, FontStyle.Bold),  // Semibold 12px for headers
                GridCellFont = new Font("Segoe UI", 12f, FontStyle.Regular),  // Regular 12px for cells
                
                // Toolbar/StatusBar
                ToolbarBackground = VAGEDCColorPalette.Gray800,
                ToolbarText = VAGEDCColorPalette.TextPrimaryDark,
                ToolbarHover = VAGEDCColorPalette.Primary500,
                StatusBarBackground = VAGEDCColorPalette.Gray900,
                StatusBarText = VAGEDCColorPalette.TextSecondaryDark,
                
                // Semantic
                AccentPrimary = VAGEDCColorPalette.Primary500,
                AccentSecondary = VAGEDCColorPalette.Secondary500,
                Warning = VAGEDCColorPalette.Warning500,
                Success = VAGEDCColorPalette.Success500,
                Danger = VAGEDCColorPalette.Danger500
            };
        }
        
        /// <summary>
        /// Creates the VAGEDC Light theme (for future use)
        /// </summary>
        public static VAGEDCTheme CreateLightTheme()
        {
            return new VAGEDCTheme
            {
                Name = "VAGEDC Light",
                IsDarkMode = false,
                
                // Window/Form
                WindowBackground = VAGEDCColorPalette.Gray50,
                PanelBackground = Color.White,
                CardBackground = VAGEDCColorPalette.Gray50,
                
                // Text
                TextPrimary = VAGEDCColorPalette.TextPrimaryLight,
                TextSecondary = VAGEDCColorPalette.TextSecondaryLight,
                
                // Borders
                BorderPrimary = VAGEDCColorPalette.Gray200,
                
                // Controls
                ControlBackground = Color.White,
                ControlHover = VAGEDCColorPalette.Gray50,
                ControlActive = VAGEDCColorPalette.Primary500,
                ControlDisabled = VAGEDCColorPalette.Gray200,
                
                // Grid/Table
                GridBackground = Color.White,
                GridAlternateRow = VAGEDCColorPalette.Gray50,
                GridSelection = VAGEDCColorPalette.Primary500,
                GridBorder = VAGEDCColorPalette.Gray200,
                GridHeaderBackground = VAGEDCColorPalette.Gray100,
                GridHeaderText = VAGEDCColorPalette.TextPrimaryLight,
                
                // Toolbar/StatusBar
                ToolbarBackground = VAGEDCColorPalette.Gray100,
                ToolbarText = VAGEDCColorPalette.TextPrimaryLight,
                ToolbarHover = VAGEDCColorPalette.Primary500,
                StatusBarBackground = VAGEDCColorPalette.Gray50,
                StatusBarText = VAGEDCColorPalette.TextSecondaryLight,
                
                // Semantic (same as dark)
                AccentPrimary = VAGEDCColorPalette.Primary500,
                AccentSecondary = VAGEDCColorPalette.Secondary500,
                Warning = VAGEDCColorPalette.Warning500,
                Success = VAGEDCColorPalette.Success500,
                Danger = VAGEDCColorPalette.Danger500
            };
        }
    }
}
