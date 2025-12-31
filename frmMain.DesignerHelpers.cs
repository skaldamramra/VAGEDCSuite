namespace VAGSuite
{
    public partial class frmMain
    {
        /// <summary>
        /// Helper methods for designer initialization
        /// </summary>

        /// <summary>
        /// Creates a standard bar button item with common settings
        /// </summary>
        private DevExpress.XtraBars.BarButtonItem CreateBarButton(
            string name,
            string caption,
            int id,
            DevExpress.XtraBars.ItemClickEventHandler handler = null,
            bool enabled = true,
            string hint = null)
        {
            var button = new DevExpress.XtraBars.BarButtonItem();
            button.Caption = caption;
            button.Id = id;
            button.Name = name;
            button.Enabled = enabled;

            if (!string.IsNullOrEmpty(hint))
                button.Hint = hint;

            if (handler != null)
                button.ItemClick += handler;

            return button;
        }

        /// <summary>
        /// Creates a standard bar static item for status display
        /// </summary>
        private DevExpress.XtraBars.BarStaticItem CreateBarStaticItem(
            string name,
            string caption,
            int id,
            string hint = null,
            System.Drawing.StringAlignment alignment = System.Drawing.StringAlignment.Near)
        {
            var item = new DevExpress.XtraBars.BarStaticItem();
            item.Caption = caption;
            item.Id = id;
            item.Name = name;
            item.TextAlignment = alignment;

            if (!string.IsNullOrEmpty(hint))
                item.Hint = hint;

            return item;
        }

        /// <summary>
        /// Creates a grid column with common settings
        /// </summary>
        private DevExpress.XtraGrid.Columns.GridColumn CreateGridColumn(
            string name,
            string caption,
            string fieldName,
            bool visible = true,
            int width = -1,
            bool readOnly = true)
        {
            var column = new DevExpress.XtraGrid.Columns.GridColumn();
            column.Caption = caption;
            column.FieldName = fieldName;
            column.Name = name;
            column.Visible = visible;
            column.OptionsColumn.AllowEdit = !readOnly;
            column.OptionsColumn.ReadOnly = readOnly;

            if (width > 0)
                column.Width = width;

            return column;
        }

        /// <summary>
        /// Adds a button to a ribbon page group with optional separator
        /// </summary>
        private void AddButtonToGroup(
            DevExpress.XtraBars.Ribbon.RibbonPageGroup group,
            DevExpress.XtraBars.BarButtonItem button,
            bool beginGroup = false)
        {
            group.ItemLinks.Add(button, beginGroup);
        }
    }
}