using System.Drawing;

namespace VAGSuite.Theming
{
    /// <summary>
    /// Color palette for VAGEDC Dark theme based on Modern ECU Tuning Editor Design Manual 2025
    /// </summary>
    public static class VAGEDCColorPalette
    {
        // ===== PRIMARY COLORS =====
        public static readonly Color Primary500 = Color.FromArgb(55, 65, 81);       // Muted Gray-Blue (Image 1)
        public static readonly Color Primary600 = Color.FromArgb(75, 85, 99);       // Lighter Gray-Blue
        
        // ===== SECONDARY COLORS =====
        public static readonly Color Secondary500 = Color.FromArgb(110, 231, 183);  // Muted Emerald
        
        // ===== SEMANTIC COLORS =====
        public static readonly Color Warning500 = Color.FromArgb(248, 113, 113);    // Muted Red
        public static readonly Color Success500 = Color.FromArgb(52, 211, 153);     // Muted Green
        public static readonly Color Danger500 = Color.FromArgb(239, 68, 68);       // Red
        
        // ===== DARK MODE NEUTRALS (Image 1 Palette) =====
        public static readonly Color Gray900 = Color.FromArgb(18, 18, 18);          // Deep Black/Gray - #121212
        public static readonly Color Gray800 = Color.FromArgb(30, 30, 30);          // Panel Gray - #1E1E1E
        public static readonly Color Gray700 = Color.FromArgb(45, 45, 45);          // Elevated Gray - #2D2D2D
        public static readonly Color Gray600 = Color.FromArgb(64, 64, 64);          // Borders - #404040
        public static readonly Color Gray500 = Color.FromArgb(107, 114, 128);       // Disabled - #6B7280
        public static readonly Color Gray400 = Color.FromArgb(156, 163, 175);       // Muted text - #9CA3AF
        
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
