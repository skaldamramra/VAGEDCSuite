using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraBars.Ribbon;

namespace VAGSuite.Theming
{
    /// <summary>
    /// Manages application of VAGEDC custom themes
    /// </summary>
    public class VAGEDCThemeManager
    {
        private static VAGEDCThemeManager _instance;
        private VAGEDCTheme _currentTheme;
        private bool _isCustomThemeActive = false;
        private PrivateFontCollection _fontCollection = new PrivateFontCollection();
        private FontFamily _sourceSansProFamily;

        public static VAGEDCThemeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new VAGEDCThemeManager();
                return _instance;
            }
        }
        
        private VAGEDCThemeManager()
        {
            LoadCustomFonts();
            _currentTheme = VAGEDCTheme.CreateDarkTheme(
                GetCustomFont(12f, FontStyle.Bold),
                GetCustomFont(12f, FontStyle.Regular)
            );
        }

        private void LoadCustomFonts()
        {
            try
            {
                string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Theming", "SourceSansPro");
                if (Directory.Exists(fontPath))
                {
                    string[] fontFiles = Directory.GetFiles(fontPath, "*.ttf");
                    foreach (string fontFile in fontFiles)
                    {
                        _fontCollection.AddFontFile(fontFile);
                    }

                    if (_fontCollection.Families.Length > 0)
                    {
                        foreach (var family in _fontCollection.Families)
                        {
                            if (family.Name.Contains("Source Sans Pro"))
                            {
                                _sourceSansProFamily = family;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("VAGEDCThemeManager: Error loading custom fonts: " + ex.Message);
            }
        }

        public Font GetCustomFont(float size, FontStyle style)
        {
            if (_sourceSansProFamily != null)
            {
                return new Font(_sourceSansProFamily, size, style);
            }
            return new Font("Segoe UI", size, style);
        }
        
        public VAGEDCTheme CurrentTheme => _currentTheme;
        public bool IsCustomThemeActive => _isCustomThemeActive;
        
        /// <summary>
        /// Activates the VAGEDC Dark theme
        /// </summary>
        public void ActivateVAGEDCDark(Form mainForm)
        {
            _currentTheme = VAGEDCTheme.CreateDarkTheme(
                GetCustomFont(12f, FontStyle.Bold),
                GetCustomFont(12f, FontStyle.Regular)
            );
            _isCustomThemeActive = true;
            ApplyThemeToForm(mainForm);
        }
        
        /// <summary>
        /// Deactivates custom theme and returns to DevExpress skin
        /// </summary>
        public void DeactivateCustomTheme()
        {
            _isCustomThemeActive = false;
            
            // Reset all controls to default appearance
            foreach (Form form in Application.OpenForms)
            {
                ResetFormAppearance(form);
            }
        }
        
        /// <summary>
        /// Resets form and all controls to use DevExpress skin defaults
        /// </summary>
        private void ResetFormAppearance(Form form)
        {
            form.SuspendLayout();
            
            // Reset form colors
            form.BackColor = Color.Empty;
            form.ForeColor = Color.Empty;
            
            // Reset all controls recursively
            ResetControlAppearance(form);
            
            form.ResumeLayout(false);
            form.Refresh();
        }
        
        /// <summary>
        /// Recursively resets control appearance to defaults
        /// </summary>
        private void ResetControlAppearance(Control control)
        {
            // Reset DevExpress GridControl
            if (control is GridControl)
            {
                GridControl grid = (GridControl)control;
                if (grid.MainView is GridView)
                {
                    GridView gridView = (GridView)grid.MainView;
                    
                    // Reset all appearance properties to defaults
                    gridView.Appearance.Row.Reset();
                    gridView.Appearance.SelectedRow.Reset();
                    gridView.Appearance.FocusedRow.Reset();
                    gridView.Appearance.HeaderPanel.Reset();
                    gridView.Appearance.GroupRow.Reset();
                    gridView.Appearance.OddRow.Reset();
                    gridView.OptionsView.EnableAppearanceOddRow = false;
                }
            }
            // Reset standard controls
            else if (control is Panel || control is GroupBox || control is Label)
            {
                control.BackColor = Color.Empty;
                control.ForeColor = Color.Empty;
            }
            else if (control is TextBox)
            {
                TextBox textBox = (TextBox)control;
                textBox.BackColor = Color.Empty;
                textBox.ForeColor = Color.Empty;
            }
            else if (control is Button)
            {
                Button button = (Button)control;
                button.BackColor = Color.Empty;
                button.ForeColor = Color.Empty;
                button.FlatStyle = FlatStyle.Standard;
            }
            else if (control is TreeView || control is ListView)
            {
                control.BackColor = Color.Empty;
                control.ForeColor = Color.Empty;
            }
            
            // Recursively process children
            foreach (Control child in control.Controls)
            {
                ResetControlAppearance(child);
            }
        }
        
        /// <summary>
        /// Applies the current theme to a form and all its controls
        /// </summary>
        public void ApplyThemeToForm(Form form)
        {
            if (!_isCustomThemeActive)
                return;
                
            // Apply to form itself
            form.BackColor = _currentTheme.WindowBackground;
            form.ForeColor = _currentTheme.TextPrimary;
            form.Font = GetCustomFont(9f, FontStyle.Regular);

            // Apply global DevExpress font
            Font customFont = GetCustomFont(9f, FontStyle.Regular);
            DevExpress.Utils.AppearanceObject.DefaultFont = customFont;

            if (form is ComponentFactory.Krypton.Toolkit.KryptonForm kForm)
            {
                // Force Custom Palette to enable dark title bar (chrome)
                kForm.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Custom;
                kForm.AllowFormChrome = true;

                kForm.StateCommon.Back.Color1 = _currentTheme.WindowBackground;
                kForm.StateCommon.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                
                // Header (Title Bar) - VS Code style
                // We use StateCommon to ensure it applies to both active and inactive states
                kForm.StateCommon.Header.Back.Color1 = Color.FromArgb(37, 37, 38); // VS Code Header Gray
                kForm.StateCommon.Header.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kForm.StateCommon.Header.Content.ShortText.Color1 = _currentTheme.TextPrimary;
                kForm.StateCommon.Header.Content.ShortText.Font = GetCustomFont(10f, FontStyle.Bold);
                
                // Border - VS Code style subtle border
                kForm.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
                kForm.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.All;
                kForm.StateCommon.Border.Color1 = Color.FromArgb(64, 64, 64);
                kForm.StateCommon.Border.Width = 1;
            }
            
            // Apply to all controls recursively
            ApplyThemeToControl(form);
        }
        
        /// <summary>
        /// Recursively applies theme to a control and its children
        /// </summary>
        private void ApplyThemeToControl(Control control)
        {
            // Krypton Controls
            if (control is ComponentFactory.Krypton.Toolkit.KryptonPanel kPanel)
            {
                kPanel.StateCommon.Color1 = _currentTheme.PanelBackground;
                kPanel.StateCommon.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonLabel kLabel)
            {
                kLabel.StateCommon.ShortText.Color1 = _currentTheme.TextPrimary;
                kLabel.StateCommon.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
                kLabel.StateCommon.ShortText.MultiLine = ComponentFactory.Krypton.Toolkit.InheritBool.True;
                kLabel.StateCommon.ShortText.MultiLineH = ComponentFactory.Krypton.Toolkit.PaletteRelativeAlign.Near;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonButton kButton)
            {
                // VS Code style button: Solid accent color with subtle border
                kButton.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Standalone;
                
                // StateCommon - Set the base for ALL states to be dark
                kButton.StateCommon.Back.Color1 = Color.FromArgb(0, 100, 180);
                kButton.StateCommon.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateCommon.Border.Color1 = Color.FromArgb(64, 64, 64);
                kButton.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.All;
                kButton.StateCommon.Border.Width = 1;
                kButton.StateCommon.Border.Rounding = 2;
                kButton.StateCommon.Content.ShortText.Color1 = Color.White;
                kButton.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Bold);

                // Normal State - Explicitly reinforce dark color
                kButton.StateNormal.Back.Color1 = Color.FromArgb(0, 100, 180);
                kButton.StateNormal.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;

                // OverrideDefault - This handles the "AcceptButton" state which often stays white
                kButton.OverrideDefault.Back.Color1 = Color.FromArgb(0, 100, 180);
                kButton.OverrideDefault.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.OverrideDefault.Border.Color1 = Color.FromArgb(0, 122, 204);
                kButton.OverrideDefault.Content.ShortText.Color1 = Color.White;

                // Tracking (Hover) - Solid color, no gradients
                kButton.StateTracking.Back.Color1 = Color.FromArgb(20, 142, 224);
                kButton.StateTracking.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateTracking.Border.Color1 = Color.White;
                kButton.StateTracking.Content.ShortText.Color1 = Color.White;

                // Pressed - Solid color, no gradients
                kButton.StatePressed.Back.Color1 = Color.FromArgb(0, 102, 184);
                kButton.StatePressed.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StatePressed.Content.ShortText.Color1 = Color.White;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonLinkLabel kLink)
            {
                kLink.StateCommon.ShortText.Color1 = Color.FromArgb(0, 122, 204); // VS Code Blue
                kLink.StateCommon.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
                // Use overrides for link states in Krypton 4.5.9
                kLink.OverrideNotVisited.ShortText.Color1 = Color.FromArgb(0, 122, 204);
                kLink.OverridePressed.ShortText.Color1 = Color.FromArgb(20, 142, 224);
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonGroupBox kGroup)
            {
                kGroup.StateCommon.Back.Color1 = _currentTheme.PanelBackground;
                kGroup.StateCommon.Content.ShortText.Color1 = _currentTheme.TextPrimary;
                kGroup.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Bold);
                kGroup.StateCommon.Border.Color1 = _currentTheme.BorderPrimary;
                kGroup.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.All;
            }

            // DevExpress RibbonControl
            if (control is RibbonControl)
            {
                ApplyThemeToRibbon((RibbonControl)control);
            }
            // DevExpress GridControl
            else if (control is GridControl)
            {
                ApplyThemeToGrid((GridControl)control);
            }
            // Standard Panel
            else if (control is Panel && !(control is ComponentFactory.Krypton.Toolkit.KryptonPanel))
            {
                Panel panel = (Panel)control;
                panel.BackColor = _currentTheme.PanelBackground;
                panel.ForeColor = _currentTheme.TextPrimary;
            }
            // Standard GroupBox
            else if (control is GroupBox)
            {
                GroupBox groupBox = (GroupBox)control;
                groupBox.BackColor = _currentTheme.CardBackground;
                groupBox.ForeColor = _currentTheme.TextPrimary;
            }
            // Standard Label
            else if (control is Label)
            {
                Label label = (Label)control;
                label.ForeColor = _currentTheme.TextPrimary;
            }
            // Standard TextBox
            else if (control is TextBox)
            {
                TextBox textBox = (TextBox)control;
                textBox.BackColor = _currentTheme.ControlBackground;
                textBox.ForeColor = _currentTheme.TextPrimary;
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }
            // Standard Button
            else if (control is Button)
            {
                Button button = (Button)control;
                button.BackColor = _currentTheme.ControlBackground;
                button.ForeColor = _currentTheme.TextPrimary;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = _currentTheme.BorderPrimary;
            }
            // Standard TreeView
            else if (control is TreeView)
            {
                TreeView treeView = (TreeView)control;
                treeView.BackColor = _currentTheme.WindowBackground;  // Use Gray900 (true black/onyx) for better contrast
                treeView.ForeColor = _currentTheme.TextPrimary;
                treeView.BorderStyle = BorderStyle.FixedSingle;
                treeView.LineColor = _currentTheme.BorderPrimary;
                
                // Note: TreeView selection colors are set via DrawMode in individual forms
                // Selected items should use Navy blue (#1E3A8A) with white text
            }
            // Standard ListView
            else if (control is ListView)
            {
                ListView listView = (ListView)control;
                listView.BackColor = _currentTheme.GridBackground;
                listView.ForeColor = _currentTheme.TextPrimary;
                listView.BorderStyle = BorderStyle.FixedSingle;
            }
            
            // Recursively apply to children
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }
        
        /// <summary>
        /// Applies theme to DevExpress RibbonControl
        /// </summary>
        private void ApplyThemeToController(DevExpress.XtraBars.BarAndDockingController controller)
        {
            Font customFont = GetCustomFont(9f, FontStyle.Regular);
            
            // Apply to Bars (Menus)
            controller.AppearancesBar.Bar.Font = customFont;
            controller.AppearancesBar.ItemsFont = customFont;
            
            // Apply to Ribbon
            controller.AppearancesRibbon.Item.Font = customFont;
            controller.AppearancesRibbon.PageGroupCaption.Font = customFont;
            controller.AppearancesRibbon.PageHeader.Font = customFont;
        }

        private void ApplyThemeToRibbon(RibbonControl ribbon)
        {
            Font customFont = GetCustomFont(9f, FontStyle.Regular);
            
            // Use reflection to set appearances to avoid compile-time errors with version-specific property names
            string[] appearanceNames = { "AppearancePageHeader", "AppearancePageGroupCaption", "AppearanceItem", "AppearanceMenuCaption" };
            foreach (string name in appearanceNames)
            {
                try
                {
                    PropertyInfo prop = ribbon.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                    if (prop != null)
                    {
                        object appearance = prop.GetValue(ribbon, null);
                        if (appearance != null)
                        {
                            PropertyInfo fontProp = appearance.GetType().GetProperty("Font", BindingFlags.Instance | BindingFlags.Public);
                            fontProp?.SetValue(appearance, customFont, null);
                        }
                    }
                }
                catch { }
            }
        }
        
        /// <summary>
        /// Applies theme to DevExpress GridControl
        /// </summary>
        private void ApplyThemeToGrid(GridControl grid)
        {
            if (grid.MainView is GridView gridView)
            {
                // Row appearance settings
                gridView.Appearance.Row.BackColor = _currentTheme.GridBackground;
                gridView.Appearance.Row.ForeColor = _currentTheme.TextPrimary;
                gridView.Appearance.Row.Font = _currentTheme.GridCellFont;
                
                gridView.Appearance.SelectedRow.BackColor = _currentTheme.GridSelection;
                gridView.Appearance.SelectedRow.ForeColor = Color.White;
                
                gridView.Appearance.FocusedRow.BackColor = _currentTheme.GridSelection;
                gridView.Appearance.FocusedRow.ForeColor = Color.White;
                
                // Header appearance with improved typography
                gridView.Appearance.HeaderPanel.BackColor = _currentTheme.GridHeaderBackground;
                gridView.Appearance.HeaderPanel.ForeColor = _currentTheme.GridHeaderText;
                gridView.Appearance.HeaderPanel.Font = _currentTheme.GridHeaderFont;
                gridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                
                gridView.Appearance.GroupRow.BackColor = _currentTheme.GridHeaderBackground;
                gridView.Appearance.GroupRow.ForeColor = _currentTheme.TextPrimary;
                
                // Enable alternating row colors
                gridView.OptionsView.EnableAppearanceOddRow = true;
                gridView.Appearance.OddRow.BackColor = _currentTheme.GridAlternateRow;
                
                // Hover state appearance
                gridView.Appearance.HideSelectionRow.BackColor = _currentTheme.GridHoverRow;
                gridView.Appearance.HideSelectionRow.ForeColor = _currentTheme.TextPrimary;
                
                // Reduce border opacity for subtle appearance
                gridView.Appearance.VertLine.BackColor = Color.FromArgb(51, _currentTheme.GridBorder);  // 20% opacity
                gridView.Appearance.HorzLine.BackColor = Color.FromArgb(51, _currentTheme.GridBorder);  // 20% opacity
                
                // Selected cell with navy border (2px)
                gridView.Appearance.FocusedCell.BackColor = Color.Transparent;
                gridView.Appearance.FocusedCell.BorderColor = _currentTheme.AccentPrimary;
            }
        }
        
        /// <summary>
        /// Gets a ToolStripProfessionalRenderer configured for the current theme
        /// </summary>
        public ToolStripProfessionalRenderer GetToolStripRenderer()
        {
            var colorTable = new VAGEDCToolStripColorTable(_currentTheme);
            return new ToolStripProfessionalRenderer(colorTable);
        }
    }
    
    /// <summary>
    /// Custom color table for ToolStrip controls
    /// </summary>
    internal class VAGEDCToolStripColorTable : ProfessionalColorTable
    {
        private VAGEDCTheme _theme;
        
        public VAGEDCToolStripColorTable(VAGEDCTheme theme)
        {
            _theme = theme;
        }
        
        public override Color ToolStripGradientBegin => _theme.ToolbarBackground;
        public override Color ToolStripGradientMiddle => _theme.ToolbarBackground;
        public override Color ToolStripGradientEnd => _theme.ToolbarBackground;
        public override Color MenuStripGradientBegin => _theme.ToolbarBackground;
        public override Color MenuStripGradientEnd => _theme.ToolbarBackground;
        public override Color MenuItemSelected => _theme.ToolbarHover;
        public override Color MenuItemBorder => _theme.BorderPrimary;
        public override Color MenuBorder => _theme.BorderPrimary;
        public override Color StatusStripGradientBegin => _theme.StatusBarBackground;
        public override Color StatusStripGradientEnd => _theme.StatusBarBackground;
    }
}
