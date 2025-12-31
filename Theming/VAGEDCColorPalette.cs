using System.Drawing;

namespace VAGSuite.Theming
{
    /// <summary>
    /// Color palette for VAGEDC Dark theme based on Modern ECU Tuning Editor Design Manual 2025
    /// </summary>
    public static class VAGEDCColorPalette
    {
        // ===== PRIMARY COLORS =====
        public static readonly Color Primary500 = Color.FromArgb(30, 58, 138);      // Deep Navy
        public static readonly Color Primary600 = Color.FromArgb(30, 64, 175);      // Darker Navy
        
        // ===== SECONDARY COLORS =====
        public static readonly Color Secondary500 = Color.FromArgb(16, 185, 129);   // Emerald Green
        
        // ===== SEMANTIC COLORS =====
        public static readonly Color Warning500 = Color.FromArgb(239, 68, 68);      // Crimson
        public static readonly Color Success500 = Color.FromArgb(5, 150, 105);      // Forest Green
        public static readonly Color Danger500 = Color.FromArgb(220, 38, 38);       // Blood Red
        
        // ===== DARK MODE NEUTRALS =====
        public static readonly Color Gray900 = Color.FromArgb(15, 23, 42);          // Onyx (Background)
        public static readonly Color Gray800 = Color.FromArgb(30, 41, 59);          // Panels
        public static readonly Color Gray700 = Color.FromArgb(51, 65, 85);          // Cards/Elevated
        public static readonly Color Gray600 = Color.FromArgb(71, 85, 105);         // Borders
        public static readonly Color Gray500 = Color.FromArgb(100, 116, 139);       // Disabled
        
        // ===== LIGHT MODE NEUTRALS =====
        public static readonly Color Gray50 = Color.FromArgb(248, 250, 252);        // Light Background
        public static readonly Color Gray100 = Color.FromArgb(241, 245, 249);       // Light Panels
        public static readonly Color Gray200 = Color.FromArgb(226, 232, 240);       // Light Borders
        
        // ===== TEXT COLORS =====
        public static readonly Color TextPrimaryDark = Color.FromArgb(248, 250, 252);   // Near-white
        public static readonly Color TextSecondaryDark = Color.FromArgb(203, 213, 225); // Light gray
        public static readonly Color TextPrimaryLight = Color.FromArgb(15, 23, 42);     // Near-black
        public static readonly Color TextSecondaryLight = Color.FromArgb(71, 85, 105);  // Dark gray
    }
}
