using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace VAGSuite.Theming
{
    /// <summary>
    /// Manages application of VAGEDC custom themes - VAGEDC Dark skin for Krypton/WinForms
    /// </summary>
    public class VAGEDCThemeManager
    {
        private static VAGEDCThemeManager _instance;
        private VAGEDCTheme _currentTheme;
        private bool _isCustomThemeActive = false;
        private PrivateFontCollection _fontCollection = new PrivateFontCollection();
        private FontFamily _sourceSansProFamily;
        private ComponentFactory.Krypton.Toolkit.KryptonPalette _customPalette;

        public ComponentFactory.Krypton.Toolkit.KryptonPalette CustomPalette => _customPalette;

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

            // Initialize the palette that will be used by complex controls
            _customPalette = new ComponentFactory.Krypton.Toolkit.KryptonPalette();
            _customPalette.BasePaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Office2010Black;
            ConfigureCustomPalette();
        }

        private void ConfigureCustomPalette()
        {
            // ===== BUTTON STYLES =====
            
            // ButtonStandalone - Primary button style (VAGEDC Dark skin)
            var btnStandalone = _customPalette.ButtonStyles.ButtonStandalone;
            btnStandalone.StateCommon.Content.ShortText.Color1 = Color.White;
            btnStandalone.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Bold);

            // Disabled State - VAGEDC Dark disabled appearance
            btnStandalone.StateDisabled.Back.Color1 = VAGEDCColorPalette.Gray500;
            btnStandalone.StateDisabled.Content.ShortText.Color1 = Color.White;

            // Tracking (Hover) State - VAGEDC Dark hover blue
            btnStandalone.StateTracking.Back.Color1 = VAGEDCColorPalette.Primary600;
            btnStandalone.StateTracking.Content.ShortText.Color1 = Color.White;

            // Pressed State - VAGEDC Dark accent
            btnStandalone.StatePressed.Back.Color1 = VAGEDCColorPalette.Primary500;
            btnStandalone.StatePressed.Content.ShortText.Color1 = Color.White;

            // Normal State - VAGEDC Dark button blue
            btnStandalone.StateNormal.Back.Color1 = VAGEDCColorPalette.Primary600;
            btnStandalone.StateNormal.Content.ShortText.Color1 = Color.White;

            // ===== COMBOBOX STYLES =====
            var comboStyle = _customPalette.InputControlStyles.InputControlStandalone;
            comboStyle.StateCommon.Back.Color1 = _currentTheme.ControlBackground;
            comboStyle.StateCommon.Content.ShortText.Color1 = _currentTheme.TextPrimary;
            comboStyle.StateCommon.Border.Color1 = VAGEDCColorPalette.Gray600;

            // ===== TEXTBOX STYLES =====
            var textBoxStyle = _customPalette.InputControlStyles.InputControlStandalone;
            textBoxStyle.StateCommon.Back.Color1 = _currentTheme.ControlBackground;
            textBoxStyle.StateCommon.Content.ShortText.Color1 = _currentTheme.TextPrimary;
            textBoxStyle.StateCommon.Border.Color1 = VAGEDCColorPalette.Gray600;

            // ===== GROUPBOX STYLES =====
            var groupStyle = _customPalette.ControlStyles.ControlGroupBox;
            groupStyle.StateCommon.Back.Color1 = _currentTheme.PanelBackground;
            groupStyle.StateCommon.Border.Color1 = VAGEDCColorPalette.Gray600;

            // ===== PANEL STYLES =====
            var panelClient = _customPalette.PanelStyles.PanelClient;
            panelClient.StateCommon.Color1 = _currentTheme.PanelBackground;

            var panelCommon = _customPalette.PanelStyles.PanelAlternate;
            panelCommon.StateCommon.Color1 = _currentTheme.PanelBackground;

            // ===== TAB STYLES (for Docking/Navigator) =====
            var tabStyle = _customPalette.TabStyles.TabStandardProfile;
            tabStyle.StateCommon.Back.Color1 = VAGEDCColorPalette.Gray700;
            tabStyle.StateCommon.Content.ShortText.Color1 = _currentTheme.TextSecondary;
            tabStyle.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);

            tabStyle.StateSelected.Back.Color1 = _currentTheme.WindowBackground;
            tabStyle.StateSelected.Content.ShortText.Color1 = Color.White;
            tabStyle.StateSelected.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Bold);

            // Tab hover state
            tabStyle.StateTracking.Back.Color1 = VAGEDCColorPalette.Gray600;

            // ===== LABEL STYLES =====
            var labelStyle = _customPalette.LabelStyles.LabelNormalControl;
            labelStyle.StateCommon.ShortText.Color1 = _currentTheme.TextPrimary;
            labelStyle.StateCommon.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);

            // ===== SEPARATOR STYLES =====
            var sepStyle = _customPalette.SeparatorStyles.SeparatorLowProfile;
            sepStyle.StateCommon.Back.Color1 = VAGEDCColorPalette.Gray600;

            // ===== CHECKBOX STYLES (using CheckBoxStandalone) =====
            var checkBoxStyle = _customPalette.ButtonStyles.ButtonStandalone;
            checkBoxStyle.StateCommon.Content.ShortText.Color1 = _currentTheme.TextPrimary;
            checkBoxStyle.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
            checkBoxStyle.StateCheckedNormal.Back.Color1 = VAGEDCColorPalette.Primary500;

            // ===== RADIO BUTTON STYLES (using RadioButtonStandalone) =====
            var radioStyle = _customPalette.ButtonStyles.ButtonStandalone;
            radioStyle.StateCommon.Content.ShortText.Color1 = _currentTheme.TextPrimary;
            radioStyle.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
            radioStyle.StateCheckedNormal.Back.Color1 = VAGEDCColorPalette.Primary500;

            // ===== COMMON FALLBACK =====
            _customPalette.Common.StateCommon.Back.Color1 = _currentTheme.PanelBackground;
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
        /// Deactivates custom theme and returns to Krypton skin
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
        /// Resets form and all controls to use Krypton skin defaults
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
            // Reset standard controls
            if (control is Panel || control is GroupBox || control is Label)
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

            // Apply global font
            Font customFont = GetCustomFont(9f, FontStyle.Regular);

            if (form is ComponentFactory.Krypton.Toolkit.KryptonForm kForm)
            {
                // Force Custom Palette to enable dark title bar (chrome)
                kForm.PaletteMode = ComponentFactory.Krypton.Toolkit.PaletteMode.Custom;
                kForm.AllowFormChrome = true;

                kForm.StateCommon.Back.Color1 = _currentTheme.WindowBackground;
                kForm.StateCommon.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;

                // Header (Title Bar) - VAGEDC Dark skin style
                kForm.StateCommon.Header.Back.Color1 = VAGEDCColorPalette.Gray700; // #2D2D2D
                kForm.StateCommon.Header.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kForm.StateCommon.Header.Content.ShortText.Color1 = _currentTheme.TextPrimary;
                kForm.StateCommon.Header.Content.ShortText.Font = GetCustomFont(10f, FontStyle.Bold);

                // Border - VAGEDC Dark skin subtle border
                kForm.StateCommon.Border.Draw = ComponentFactory.Krypton.Toolkit.InheritBool.True;
                kForm.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.All;
                kForm.StateCommon.Border.Color1 = VAGEDCColorPalette.Gray600; // #333333
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
                // VAGEDC Dark skin button: Solid accent color with subtle border
                kButton.ButtonStyle = ComponentFactory.Krypton.Toolkit.ButtonStyle.Standalone;
                
                // StateCommon - Set the base for ALL states to be dark
                // We remove the hardcoded blue from StateCommon so it doesn't leak into StateDisabled
                kButton.StateCommon.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.All;
                kButton.StateCommon.Border.Width = 1;
                kButton.StateCommon.Border.Rounding = 2;
                kButton.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
                
                // Force StateCommon text to white to ensure visibility in all states (including disabled fallback)
                kButton.StateCommon.Content.ShortText.Color1 = Color.White;
                kButton.StateCommon.Content.ShortText.Color2 = Color.White;
                kButton.StateCommon.Content.ShortText.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;

                // Normal State - primary accent for VAGEDC Dark skin
                kButton.StateNormal.Back.Color1 = VAGEDCColorPalette.Primary600; // #0E639C
                kButton.StateNormal.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateNormal.Content.ShortText.Color1 = Color.White;

                // StateDisabled - VAGEDC Dark skin disabled appearance
                kButton.StateDisabled.Back.Color1 = VAGEDCColorPalette.Gray500; // #3A3D41
                kButton.StateDisabled.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateDisabled.Content.ShortText.Color1 = Color.White; // Pure white for maximum contrast
                kButton.StateDisabled.Content.ShortText.Color2 = Color.White;
                kButton.StateDisabled.Content.ShortText.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateDisabled.Border.Color1 = VAGEDCColorPalette.Gray600;

                // OverrideDefault - This handles the "AcceptButton" state which often stays white
                kButton.OverrideDefault.Back.Color1 = VAGEDCColorPalette.Primary600; // #0E639C
                kButton.OverrideDefault.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.OverrideDefault.Border.Color1 = VAGEDCColorPalette.Primary500; // #007ACC
                kButton.OverrideDefault.Content.ShortText.Color1 = Color.White;

                // Tracking (Hover) - VAGEDC Dark hover blue
                kButton.StateTracking.Back.Color1 = VAGEDCColorPalette.Primary600; // #0E639C
                kButton.StateTracking.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StateTracking.Border.Color1 = Color.White;
                kButton.StateTracking.Content.ShortText.Color1 = Color.White;
                kButton.StateTracking.Content.ShortText.Color2 = Color.White;
                kButton.StateTracking.Content.ShortText.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;

                // Pressed - VAGEDC Dark accent
                kButton.StatePressed.Back.Color1 = VAGEDCColorPalette.Primary500; // #007ACC
                kButton.StatePressed.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.StatePressed.Content.ShortText.Color1 = Color.White;
                kButton.StatePressed.Content.ShortText.Color2 = Color.White;
                kButton.StatePressed.Content.ShortText.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;

                // OverrideFocus - Ensure focused buttons don't revert to system style
                kButton.OverrideFocus.Back.Color1 = VAGEDCColorPalette.Primary600; // #0E639C
                kButton.OverrideFocus.Back.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.OverrideFocus.Content.ShortText.Color1 = Color.White;
                kButton.OverrideFocus.Content.ShortText.Color2 = Color.White;
                kButton.OverrideFocus.Content.ShortText.ColorStyle = ComponentFactory.Krypton.Toolkit.PaletteColorStyle.Solid;
                kButton.OverrideFocus.Border.Color1 = Color.White;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonLinkLabel kLink)
            {
                kLink.StateCommon.ShortText.Color1 = VAGEDCColorPalette.Primary500; // VAGEDC Dark Blue
                kLink.StateCommon.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
                // Use overrides for link states in Krypton 4.5.9
                kLink.OverrideNotVisited.ShortText.Color1 = VAGEDCColorPalette.Primary500;
                kLink.OverridePressed.ShortText.Color1 = Color.FromArgb(20, 142, 224); // Lighter blue
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonGroupBox kGroup)
            {
                kGroup.StateCommon.Back.Color1 = _currentTheme.PanelBackground;
                kGroup.StateCommon.Content.ShortText.Color1 = _currentTheme.TextPrimary;
                kGroup.StateCommon.Content.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
                kGroup.StateCommon.Border.Color1 = VAGEDCColorPalette.Gray600;
                kGroup.StateCommon.Border.DrawBorders = ComponentFactory.Krypton.Toolkit.PaletteDrawBorders.All;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonComboBox kCombo)
            {
                kCombo.StateCommon.ComboBox.Back.Color1 = _currentTheme.ControlBackground;
                kCombo.StateCommon.ComboBox.Content.Color1 = _currentTheme.TextPrimary;
                kCombo.StateCommon.ComboBox.Border.Color1 = VAGEDCColorPalette.Gray600;
                kCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonTextBox kTextBox)
            {
                kTextBox.StateCommon.Back.Color1 = _currentTheme.ControlBackground;
                kTextBox.StateCommon.Content.Color1 = _currentTheme.TextPrimary;
                kTextBox.StateCommon.Border.Color1 = VAGEDCColorPalette.Gray600;
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonCheckBox kCheckBox)
            {
                kCheckBox.StateCommon.ShortText.Color1 = _currentTheme.TextPrimary;
                kCheckBox.StateCommon.ShortText.Font = GetCustomFont(9f, FontStyle.Regular);
            }
            else if (control is ComponentFactory.Krypton.Toolkit.KryptonTrackBar kTrackBar)
            {
                kTrackBar.StateCommon.Tick.Color1 = _currentTheme.TextSecondary;
                kTrackBar.StateCommon.Track.Color1 = VAGEDCColorPalette.Primary500;
            }

            // Standard Panel
            if (control is Panel && !(control is ComponentFactory.Krypton.Toolkit.KryptonPanel))
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
                treeView.LineColor = VAGEDCColorPalette.Gray600;

                // Note: TreeView selection colors are set via DrawMode in individual forms
                // Selected items should use VAGEDC Dark selection blue (#264F78) with white text
            }
            // Standard ListView
            else if (control is ListView)
            {
                ListView listView = (ListView)control;
                listView.BackColor = _currentTheme.GridBackground;
                listView.ForeColor = _currentTheme.TextPrimary;
                listView.BorderStyle = BorderStyle.FixedSingle;
            }
            // DataGridView (used by ADGV in MapViewerEx)
            else if (control is DataGridView dataGridView)
            {
                ApplyThemeToDataGridView(dataGridView);
            }
            // ContextMenuStrip
            else if (control is ContextMenuStrip contextMenuStrip)
            {
                contextMenuStrip.Renderer = GetToolStripRenderer();
                contextMenuStrip.BackColor = _currentTheme.ToolbarBackground;
            }
            // MenuStrip
            else if (control is MenuStrip menuStrip)
            {
                menuStrip.Renderer = GetToolStripRenderer();
                menuStrip.BackColor = _currentTheme.ToolbarBackground;
            }
            // StatusStrip
            else if (control is StatusStrip statusStrip)
            {
                statusStrip.Renderer = GetToolStripRenderer();
                statusStrip.BackColor = _currentTheme.StatusBarBackground;
            }
            
            // Recursively apply to children
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }
        
        
        /// <summary>
        /// Applies VAGEDC Dark skin to a DataGridView control
        /// </summary>
        private void ApplyThemeToDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = _currentTheme.GridBackground;
            dgv.ForeColor = _currentTheme.TextPrimary;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.GridColor = VAGEDCColorPalette.Gray600;
            
            // Alternating row color
            dgv.AlternatingRowsDefaultCellStyle.BackColor = _currentTheme.GridAlternateRow;
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = _currentTheme.TextPrimary;
            
            // Default row style
            dgv.RowsDefaultCellStyle.BackColor = _currentTheme.GridBackground;
            dgv.RowsDefaultCellStyle.ForeColor = _currentTheme.TextPrimary;
            
            // Selection style - VAGEDC Dark selection blue
            dgv.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgv.DefaultCellStyle.SelectionBackColor = VAGEDCColorPalette.Primary500;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            
            // Header styles
            dgv.ColumnHeadersDefaultCellStyle.BackColor = _currentTheme.GridHeaderBackground;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = _currentTheme.GridHeaderText;
            dgv.ColumnHeadersDefaultCellStyle.Font = GetCustomFont(9f, FontStyle.Bold);
            
            dgv.RowHeadersDefaultCellStyle.BackColor = _currentTheme.GridHeaderBackground;
            dgv.RowHeadersDefaultCellStyle.ForeColor = _currentTheme.GridHeaderText;
            
            // Border
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            // dgv.BorderColor = VAGEDCColorPalette.Gray600; // This property does not exist in DataGridView
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
    /// Custom color table for ToolStrip controls - VAGEDC Dark skin style
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
        public override Color MenuItemSelected => VAGEDCColorPalette.Gray600;
        public override Color MenuItemBorder => VAGEDCColorPalette.Gray600;
        public override Color MenuBorder => VAGEDCColorPalette.Gray600;
        public override Color StatusStripGradientBegin => _theme.StatusBarBackground;
        public override Color StatusStripGradientEnd => _theme.StatusBarBackground;
        public override Color SeparatorLight => VAGEDCColorPalette.Gray600;
        public override Color SeparatorDark => VAGEDCColorPalette.Gray700;
    }
}
