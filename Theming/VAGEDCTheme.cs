using System.Drawing;

namespace VAGSuite.Theming
{
    /// <summary>
    /// Defines a complete VAGEDC Dark theme - VAGEDC Dark skin color assignments
    /// </summary>
    public class VAGEDCTheme
    {
        public string Name { get; set; }
        public bool IsDarkMode { get; set; }

        // ===== WINDOW/FORM COLORS =====
        /// <summary>Main editor/workspace background - #1E1E1E</summary>
        public Color WindowBackground { get; set; }
        /// <summary>Sidebar/panel background - #252526</summary>
        public Color PanelBackground { get; set; }
        /// <summary>Elevated surfaces (cards, groups) - #2D2D2D</summary>
        public Color CardBackground { get; set; }

        // ===== TEXT COLORS =====
        /// <summary>Primary text color - #D4D4D4</summary>
        public Color TextPrimary { get; set; }
        /// <summary>Secondary/muted text - #BBBBBB</summary>
        public Color TextSecondary { get; set; }

        // ===== BORDER COLORS =====
        /// <summary>Primary border color - #333333</summary>
        public Color BorderPrimary { get; set; }

        // ===== CONTROL COLORS =====
        /// <summary>Input control background - #3C3C3C</summary>
        public Color ControlBackground { get; set; }
        /// <summary>Control hover state - #454545</summary>
        public Color ControlHover { get; set; }
        /// <summary>Control active/pressed state - #007ACC</summary>
        public Color ControlActive { get; set; }
        /// <summary>Control disabled state - #3A3D41</summary>
        public Color ControlDisabled { get; set; }

        // ===== GRID/TABLE COLORS =====
        /// <summary>Data grid background - #252526</summary>
        public Color GridBackground { get; set; }
        /// <summary>Alternating row color - #2D2D2D</summary>
        public Color GridAlternateRow { get; set; }
        /// <summary>Selection color - #007ACC</summary>
        public Color GridSelection { get; set; }
        /// <summary>Grid border color - #333333</summary>
        public Color GridBorder { get; set; }
        /// <summary>Column/row header background - #252526</summary>
        public Color GridHeaderBackground { get; set; }
        /// <summary>Header text color - #D4D4D4</summary>
        public Color GridHeaderText { get; set; }
        /// <summary>Row hover color - #3A3D41</summary>
        public Color GridHoverRow { get; set; }
        public Font GridHeaderFont { get; set; }
        public Font GridCellFont { get; set; }

        // ===== TOOLBAR/STATUSBAR COLORS =====
        /// <summary>Toolbar background - #252526</summary>
        public Color ToolbarBackground { get; set; }
        /// <summary>Toolbar text color - #D4D4D4</summary>
        public Color ToolbarText { get; set; }
        /// <summary>Toolbar item hover - #007ACC</summary>
        public Color ToolbarHover { get; set; }
        /// <summary>Status bar background - #007ACC (VAGEDC Dark accent)</summary>
        public Color StatusBarBackground { get; set; }
        /// <summary>Status bar text - #FFFFFF</summary>
        public Color StatusBarText { get; set; }

        // ===== SEMANTIC COLORS =====
        /// <summary>Primary accent - VAGEDC Dark Blue #007ACC</summary>
        public Color AccentPrimary { get; set; }
        /// <summary>Secondary accent - Muted Emerald #6AE7B7</summary>
        public Color AccentSecondary { get; set; }
        /// <summary>Warning color - Amber #D7BA7D</summary>
        public Color Warning { get; set; }
        /// <summary>Success color - Green #B5CEA8</summary>
        public Color Success { get; set; }
        /// <summary>Danger/Error color - Red #F44747</summary>
        public Color Danger { get; set; }

        // ===== VAGEDC DARK SKIN COLORS =====
        /// <summary>VAGEDC Dark selection blue - #264F78</summary>
        public Color SelectionBackground { get; set; }
        /// <summary>Widget border - #303031</summary>
        public Color WidgetBorder { get; set; }
        /// <summary>Menu separator - #454545</summary>
        public Color MenuSeparator { get; set; }
        
        /// <summary>
        /// Creates the VAGEDC Dark theme
        /// </summary>
        public static VAGEDCTheme CreateDarkTheme(Font headerFont = null, Font cellFont = null)
        {
            return new VAGEDCTheme
            {
                Name = "VAGEDC Dark",
                IsDarkMode = true,

                // ===== WINDOW/FORM =====
                WindowBackground = VAGEDCColorPalette.Gray900,      // #1E1E1E
                PanelBackground = VAGEDCColorPalette.Gray800,       // #252526
                CardBackground = VAGEDCColorPalette.Gray700,        // #2D2D2D

                // ===== TEXT =====
                TextPrimary = VAGEDCColorPalette.TextPrimaryDark,    // #D4D4D4
                TextSecondary = VAGEDCColorPalette.TextSecondaryDark, // #BBBBBB

                // ===== BORDERS =====
                BorderPrimary = VAGEDCColorPalette.Gray600,         // #333333

                // ===== CONTROLS =====
                ControlBackground = Color.FromArgb(60, 60, 60),      // #3C3C3C (VAGEDC Dark input bg)
                ControlHover = VAGEDCColorPalette.Gray700,          // #2D2D2D
                ControlActive = VAGEDCColorPalette.Primary500,      // #007ACC
                ControlDisabled = VAGEDCColorPalette.Gray500,       // #3A3D41

                // ===== GRID/TABLE =====
                GridBackground = VAGEDCColorPalette.Gray800,        // #252526
                GridAlternateRow = VAGEDCColorPalette.Gray700,      // #2D2D2D
                GridSelection = VAGEDCColorPalette.Primary500,      // #007ACC
                GridBorder = VAGEDCColorPalette.Gray600,            // #333333
                GridHeaderBackground = VAGEDCColorPalette.Gray800,  // #252526
                GridHeaderText = VAGEDCColorPalette.TextPrimaryDark, // #D4D4D4
                GridHoverRow = Color.FromArgb(58, 61, 65),          // #3A3D41
                GridHeaderFont = headerFont ?? new Font("Segoe UI", 12f, FontStyle.Bold),
                GridCellFont = cellFont ?? new Font("Segoe UI", 12f, FontStyle.Regular),

                // ===== TOOLBAR/STATUSBAR =====
                ToolbarBackground = VAGEDCColorPalette.Gray800,     // #252526
                ToolbarText = VAGEDCColorPalette.TextPrimaryDark,   // #D4D4D4
                ToolbarHover = VAGEDCColorPalette.Primary500,       // #007ACC
                StatusBarBackground = VAGEDCColorPalette.Primary500, // #007ACC (VAGEDC Dark accent)
                StatusBarText = Color.White,

                // ===== SEMANTIC =====
                AccentPrimary = VAGEDCColorPalette.Primary500,      // #007ACC
                AccentSecondary = VAGEDCColorPalette.Secondary500,  // #6AE7B7
                Warning = VAGEDCColorPalette.Warning500,            // #D7BA7D
                Success = VAGEDCColorPalette.Success500,            // #B5CEA8
                Danger = VAGEDCColorPalette.Danger500,              // #F44747

                // ===== VAGEDC DARK SKIN =====
                SelectionBackground = Color.FromArgb(38, 79, 120),   // #264F78
                WidgetBorder = Color.FromArgb(48, 48, 49),          // #303031
                MenuSeparator = VAGEDCColorPalette.Gray600           // #333333
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

                // ===== WINDOW/FORM =====
                WindowBackground = VAGEDCColorPalette.Gray50,       // #F8FAFC
                PanelBackground = Color.White,
                CardBackground = VAGEDCColorPalette.Gray50,         // #F1F5F9

                // ===== TEXT =====
                TextPrimary = VAGEDCColorPalette.TextPrimaryLight,  // #0F172A
                TextSecondary = VAGEDCColorPalette.TextSecondaryLight, // #475569

                // ===== BORDERS =====
                BorderPrimary = VAGEDCColorPalette.Gray200,         // #E2E8F0

                // ===== CONTROLS =====
                ControlBackground = Color.White,
                ControlHover = VAGEDCColorPalette.Gray50,           // #F1F5F9
                ControlActive = VAGEDCColorPalette.Primary500,      // #007ACC
                ControlDisabled = VAGEDCColorPalette.Gray200,       // #CBD5E1

                // ===== GRID/TABLE =====
                GridBackground = Color.White,
                GridAlternateRow = VAGEDCColorPalette.Gray50,       // #F1F5F9
                GridSelection = VAGEDCColorPalette.Primary500,      // #007ACC
                GridBorder = VAGEDCColorPalette.Gray200,            // #E2E8F0
                GridHeaderBackground = VAGEDCColorPalette.Gray100,  // #F1F5F9
                GridHeaderText = VAGEDCColorPalette.TextPrimaryLight,
                GridHoverRow = Color.FromArgb(241, 245, 249),       // #F1F5F9

                // ===== TOOLBAR/STATUSBAR =====
                ToolbarBackground = VAGEDCColorPalette.Gray100,     // #F1F5F9
                ToolbarText = VAGEDCColorPalette.TextPrimaryLight,
                ToolbarHover = VAGEDCColorPalette.Primary500,       // #007ACC
                StatusBarBackground = VAGEDCColorPalette.Gray50,    // #F8FAFC
                StatusBarText = VAGEDCColorPalette.TextSecondaryLight,

                // ===== SEMANTIC =====
                AccentPrimary = VAGEDCColorPalette.Primary500,      // #007ACC
                AccentSecondary = VAGEDCColorPalette.Secondary500,
                Warning = VAGEDCColorPalette.Warning500,
                Success = VAGEDCColorPalette.Success500,
                Danger = VAGEDCColorPalette.Danger500,

                // ===== VAGEDC LIGHT SKIN =====
                SelectionBackground = Color.FromArgb(209, 231, 255), // #D1E5FF
                WidgetBorder = Color.FromArgb(203, 213, 225),       // #CBD5E1
                MenuSeparator = VAGEDCColorPalette.Gray200
            };
        }
    }
}
