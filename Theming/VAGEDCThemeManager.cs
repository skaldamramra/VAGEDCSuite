using System;
using System.Drawing;
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
            _currentTheme = VAGEDCTheme.CreateDarkTheme();
        }
        
        public VAGEDCTheme CurrentTheme => _currentTheme;
        public bool IsCustomThemeActive => _isCustomThemeActive;
        
        /// <summary>
        /// Activates the VAGEDC Dark theme
        /// </summary>
        public void ActivateVAGEDCDark(Form mainForm)
        {
            _currentTheme = VAGEDCTheme.CreateDarkTheme();
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
            
            // Apply to all controls recursively
            ApplyThemeToControl(form);
        }
        
        /// <summary>
        /// Recursively applies theme to a control and its children
        /// </summary>
        private void ApplyThemeToControl(Control control)
        {
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
            else if (control is Panel)
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
                treeView.BackColor = _currentTheme.GridBackground;
                treeView.ForeColor = _currentTheme.TextPrimary;
                treeView.BorderStyle = BorderStyle.FixedSingle;
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
        private void ApplyThemeToRibbon(RibbonControl ribbon)
        {
            // Note: DevExpress controls use their skin system
            // We can only override some appearance properties
            // The base skin should be set to a dark one (e.g., "Office 2007 Black")
        }
        
        /// <summary>
        /// Applies theme to DevExpress GridControl
        /// </summary>
        private void ApplyThemeToGrid(GridControl grid)
        {
            if (grid.MainView is GridView gridView)
            {
                // Appearance settings
                gridView.Appearance.Row.BackColor = _currentTheme.GridBackground;
                gridView.Appearance.Row.ForeColor = _currentTheme.TextPrimary;
                
                gridView.Appearance.SelectedRow.BackColor = _currentTheme.GridSelection;
                gridView.Appearance.SelectedRow.ForeColor = Color.White;
                
                gridView.Appearance.FocusedRow.BackColor = _currentTheme.GridSelection;
                gridView.Appearance.FocusedRow.ForeColor = Color.White;
                
                gridView.Appearance.HeaderPanel.BackColor = _currentTheme.GridHeaderBackground;
                gridView.Appearance.HeaderPanel.ForeColor = _currentTheme.GridHeaderText;
                
                gridView.Appearance.GroupRow.BackColor = _currentTheme.GridHeaderBackground;
                gridView.Appearance.GroupRow.ForeColor = _currentTheme.TextPrimary;
                
                // Enable alternating row colors
                gridView.OptionsView.EnableAppearanceOddRow = true;
                gridView.Appearance.OddRow.BackColor = _currentTheme.GridAlternateRow;
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
