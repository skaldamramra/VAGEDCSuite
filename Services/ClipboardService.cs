using System;
using System.Text;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    /// <summary>
    /// Handles clipboard operations for map data
    /// </summary>
    public class ClipboardService : IClipboardService
    {
        private readonly IDataConversionService _conversionService;
        
        public ClipboardService(IDataConversionService conversionService)
        {
            _conversionService = conversionService;
        }
        
        public void CopySelection(object[] cells, ViewType viewType)
        {
            if (cells == null || cells.Length == 0)
                return;
                
            var sb = new StringBuilder();
            
            // Use the same format as MapViewerEx: "viewtype:colindex:rowhandle:value:~"
            sb.Append(((int)viewType).ToString());
            
            for (int i = 0; i < cells.Length; i += 3)
            {
                if (i + 2 < cells.Length)
                {
                    int colIndex = Convert.ToInt32(cells[i]);
                    int rowHandle = Convert.ToInt32(cells[i + 1]);
                    object value = cells[i + 2];
                    
                    sb.Append(colIndex.ToString());
                    sb.Append(":");
                    sb.Append(rowHandle.ToString());
                    sb.Append(":");
                    sb.Append(value.ToString());
                    sb.Append(":~");
                }
            }
            
            try
            {
                System.Windows.Forms.Clipboard.SetText(sb.ToString());
            }
            catch
            {
                // Clipboard access may fail in some environments
            }
        }
        
        public void PasteAtCurrentLocation(object[] targetCells, ViewType currentViewType)
        {
            if (targetCells == null || targetCells.Length < 1)
                return;
                
            try
            {
                if (!System.Windows.Forms.Clipboard.ContainsText())
                    return;
                    
                string serialized = System.Windows.Forms.Clipboard.GetText();
                // Match MapViewerEx format: first char is viewtype, then "colindex:rowhandle:value:~"
                int viewtypeinclipboard = Convert.ToInt32(serialized.Substring(0, 1));
                ViewType vtclip = (ViewType)viewtypeinclipboard;
                serialized = serialized.Substring(1);
                
                char[] sep = new char[1];
                sep.SetValue('~', 0);
                string[] cells = serialized.Split(sep);
                
                int rowhandlefrom = Convert.ToInt32(targetCells[0]);
                int colindexfrom = Convert.ToInt32(targetCells[1]);
                int originalrowoffset = -1;
                int originalcolumnoffset = -1;
                
                foreach (string cell in cells)
                {
                    char[] sep2 = new char[1];
                    sep2.SetValue(':', 0);
                    string[] vals = cell.Split(sep2);
                    
                    if (vals.Length >= 3)
                    {
                        // Match MapViewerEx format: colindex:rowhandle:value
                        int rowhandle = Convert.ToInt32(vals.GetValue(1));
                        int colindex = Convert.ToInt32(vals.GetValue(0));
                        int ivalue = 0;
                        double dvalue = 0;
                        
                        if (vtclip == ViewType.Hexadecimal)
                        {
                            ivalue = Convert.ToInt32(vals.GetValue(2).ToString());
                            dvalue = ivalue;
                        }
                        else if (vtclip == ViewType.Decimal)
                        {
                            ivalue = Convert.ToInt32(vals.GetValue(2));
                            dvalue = ivalue;
                        }
                        else if (vtclip == ViewType.Easy)
                        {
                            dvalue = Convert.ToDouble(vals.GetValue(2));
                        }
                        
                        if (originalrowoffset == -1) originalrowoffset = rowhandle;
                        if (originalcolumnoffset == -1) originalcolumnoffset = colindex;
                        
                        if (rowhandle >= 0 && colindex >= 0)
                        {
                            int targetRow = rowhandlefrom + (rowhandle - originalrowoffset);
                            int targetCol = colindexfrom + (colindex - originalcolumnoffset);
                            
                            // Store the value to be set (actual UI update happens outside this service)
                            if (targetCells.Length > 2)
                            {
                                // Store parsed values for UI to use
                                ((System.Collections.Generic.List<PasteCellInfo>)targetCells[2]).Add(new PasteCellInfo
                                {
                                    Row = targetRow,
                                    Column = targetCol,
                                    Value = vtclip == ViewType.Hexadecimal ? ivalue.ToString("X") : dvalue.ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception pasteE)
            {
                Console.WriteLine("PasteAtCurrentLocation: " + pasteE.Message);
            }
        }
        
        /// <summary>
        /// Pastes data at original position (no offset adjustment)
        /// </summary>
        public void PasteAtOriginalPosition(object[] targetCells, ViewType currentViewType)
        {
            if (targetCells == null || targetCells.Length < 1)
                return;
                
            try
            {
                if (!System.Windows.Forms.Clipboard.ContainsText())
                    return;
                    
                string serialized = System.Windows.Forms.Clipboard.GetText();
                // Match MapViewerEx format: first char is viewtype, then "colindex:rowhandle:value:~"
                int viewtypeinclipboard = Convert.ToInt32(serialized.Substring(0, 1));
                ViewType vtclip = (ViewType)viewtypeinclipboard;
                serialized = serialized.Substring(1);
                
                char[] sep = new char[1];
                sep.SetValue('~', 0);
                string[] cells = serialized.Split(sep);
                
                foreach (string cell in cells)
                {
                    char[] sep2 = new char[1];
                    sep2.SetValue(':', 0);
                    string[] vals = cell.Split(sep2);
                    
                    if (vals.Length >= 3)
                    {
                        // Match MapViewerEx format: colindex:rowhandle:value
                        int rowhandle = Convert.ToInt32(vals.GetValue(1));
                        int colindex = Convert.ToInt32(vals.GetValue(0));
                        
                        if (rowhandle >= 0 && colindex >= 0)
                        {
                            if (targetCells.Length > 2)
                            {
                                ((System.Collections.Generic.List<PasteCellInfo>)targetCells[2]).Add(new PasteCellInfo
                                {
                                    Row = rowhandle,
                                    Column = colindex,
                                    Value = vals.GetValue(2).ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception pasteE)
            {
                Console.WriteLine("PasteAtOriginalPosition: " + pasteE.Message);
            }
        }
    }
    
    /// <summary>
    /// Helper class for storing paste cell information
    /// </summary>
    public class PasteCellInfo
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Value { get; set; }
    }
}
