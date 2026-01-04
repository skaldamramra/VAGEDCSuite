using System.Drawing;
 
namespace VAGSuite.Theming
{
    /// <summary>
    /// Color palette for VAGEDC Dark theme aligned to VS Code Dark+ color tokens
    /// </summary>
    public static class VAGEDCColorPalette
    {
        // ===== PRIMARY COLORS =====
        // Primary accent colors: Primary500 is VS Code accent (#007ACC)
        public static readonly Color Primary500 = Color.FromArgb(0, 122, 204);    // VS Code Blue #007ACC
        public static readonly Color Primary600 = Color.FromArgb(14, 99, 156);    // Button/Strong Blue #0E639C
        
        // ===== SECONDARY COLORS =====
        public static readonly Color Secondary500 = Color.FromArgb(110, 231, 183);  // Muted Emerald
        
        // ===== SEMANTIC COLORS =====
        public static readonly Color Warning500 = Color.FromArgb(248, 113, 113);    // Muted Red
        public static readonly Color Success500 = Color.FromArgb(52, 211, 153);     // Muted Green
        public static readonly Color Danger500 = Color.FromArgb(239, 68, 68);       // Red
        
        // ===== DARK MODE NEUTRALS (mapped to VS Code Dark+) =====
        public static readonly Color Gray900 = Color.FromArgb(30, 30, 30);    // Editor/Window background #1E1E1E
        public static readonly Color Gray800 = Color.FromArgb(37, 37, 38);    // Side bar / panel background ~#252526
        public static readonly Color Gray700 = Color.FromArgb(45, 45, 45);    // Elevated surfaces / tabs #2D2D2D
        public static readonly Color Gray600 = Color.FromArgb(51, 51, 51);    // Borders / subtle lines #333333
        public static readonly Color Gray500 = Color.FromArgb(58, 61, 65);    // Disabled / muted #3A3D41
        public static readonly Color Gray400 = Color.FromArgb(187, 187, 187); // Muted text #BBBBBB
        
        // ===== LIGHT MODE NEUTRALS =====
        public static readonly Color Gray50 = Color.FromArgb(248, 250, 252);        // Light Background
        public static readonly Color Gray100 = Color.FromArgb(241, 245, 249);       // Light Panels
        public static readonly Color Gray200 = Color.FromArgb(226, 232, 240);       // Light Borders
        
        // ===== TEXT COLORS =====
        public static readonly Color TextPrimaryDark = Color.FromArgb(212, 212, 212);   // ~#D4D4D4
        public static readonly Color TextSecondaryDark = Color.FromArgb(187, 187, 187); // #BBBBBB
        public static readonly Color TextPrimaryLight = Color.FromArgb(15, 23, 42);     // Near-black (unchanged)
        public static readonly Color TextSecondaryLight = Color.FromArgb(71, 85, 105);  // Dark gray (unchanged)
    }
}
