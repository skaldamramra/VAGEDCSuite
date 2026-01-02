using System;
using System.Globalization;
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
            
            // New format: "VT=<viewType>;<col>:<row>:<value>:~..."
            // Backwards-compatible parsing performed in Paste methods
            sb.Append("VT=");
            sb.Append(((int)viewType).ToString(CultureInfo.InvariantCulture));
            sb.Append(";");
            
            for (int i = 0; i < cells.Length; i += 3)
            {
                if (i + 2 < cells.Length)
                {
                    int colIndex = Convert.ToInt32(cells[i], CultureInfo.InvariantCulture);
                    int rowHandle = Convert.ToInt32(cells[i + 1], CultureInfo.InvariantCulture);
                    object value = cells[i + 2];
                    
                    sb.Append(colIndex.ToString(CultureInfo.InvariantCulture));
                    sb.Append(":");
                    sb.Append(rowHandle.ToString(CultureInfo.InvariantCulture));
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
                int viewtypeinclipboard = -1;
                ViewType vtclip = currentViewType;
                
                // New format detection: VT=<int>;<payload>
                if (serialized.StartsWith("VT="))
                {
                    int semicolon = serialized.IndexOf(';');
                    if (semicolon > 3)
                    {
                        string vtstr = serialized.Substring(3, semicolon - 3);
                        if (!int.TryParse(vtstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out viewtypeinclipboard))
                            viewtypeinclipboard = -1;
                        serialized = serialized.Substring(semicolon + 1);
                    }
                    else
                    {
                        // malformed; fallback to entire string (legacy fallback will try)
                        serialized = serialized.Substring("VT=".Length);
                    }
                }
                else
                {
                    // Legacy format fallback: first character may be viewtype digit
                    if (serialized.Length > 0 && char.IsDigit(serialized[0]))
                    {
                        if (int.TryParse(serialized.Substring(0, 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out viewtypeinclipboard))
                        {
                            serialized = serialized.Substring(1);
                        }
                    }
                }
                
                if (viewtypeinclipboard >= 0)
                {
                    vtclip = (ViewType)viewtypeinclipboard;
                }
                
                char[] sep = new char[1];
                sep.SetValue('~', 0);
                string[] cells = serialized.Split(sep);
                
                int rowhandlefrom = Convert.ToInt32(targetCells[0]);
                int colindexfrom = Convert.ToInt32(targetCells[1]);
                int originalrowoffset = -1;
                int originalcolumnoffset = -1;
                
                foreach (string cell in cells)
                {
                    if (string.IsNullOrWhiteSpace(cell)) continue;
                    char[] sep2 = new char[1];
                    sep2.SetValue(':', 0);
                    string[] vals = cell.Split(sep2);
                    
                    if (vals.Length < 3) continue;
                    
                    // Parse colindex and rowhandle
                    if (!int.TryParse(vals[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int colindex)) continue;
                    if (!int.TryParse(vals[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int rowhandle)) continue;
                    
                    int ivalue = 0;
                    double dvalue = 0;
                    
                    if (vtclip == ViewType.Hexadecimal)
                    {
                        // Use robust hex parsing
                        if (!int.TryParse(vals[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ivalue))
                        {
                            int.TryParse(vals[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out ivalue);
                        }
                        dvalue = ivalue;
                    }
                    else if (vtclip == ViewType.Decimal)
                    {
                        int.TryParse(vals[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out ivalue);
                        dvalue = ivalue;
                    }
                    else if (vtclip == ViewType.Easy)
                    {
                        double.TryParse(vals[2], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out dvalue);
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
                int viewtypeinclipboard = -1;
                ViewType vtclip = currentViewType;
                
                // New format detection: VT=<int>;<payload>
                if (serialized.StartsWith("VT="))
                {
                    int semicolon = serialized.IndexOf(';');
                    if (semicolon > 3)
                    {
                        string vtstr = serialized.Substring(3, semicolon - 3);
                        if (!int.TryParse(vtstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out viewtypeinclipboard))
                            viewtypeinclipboard = -1;
                        serialized = serialized.Substring(semicolon + 1);
                    }
                    else
                    {
                        serialized = serialized.Substring("VT=".Length);
                    }
                }
                else
                {
                    // Legacy format fallback: first character may be viewtype digit
                    if (serialized.Length > 0 && char.IsDigit(serialized[0]))
                    {
                        if (int.TryParse(serialized.Substring(0, 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out viewtypeinclipboard))
                        {
                            serialized = serialized.Substring(1);
                        }
                    }
                }
                
                if (viewtypeinclipboard >= 0)
                {
                    vtclip = (ViewType)viewtypeinclipboard;
                }
                
                char[] sep = new char[1];
                sep.SetValue('~', 0);
                string[] cells = serialized.Split(sep);
                
                foreach (string cell in cells)
                {
                    if (string.IsNullOrWhiteSpace(cell)) continue;
                    char[] sep2 = new char[1];
                    sep2.SetValue(':', 0);
                    string[] vals = cell.Split(sep2);
                    
                    if (vals.Length < 3) continue;
                    
                    // Parse colindex and rowhandle
                    if (!int.TryParse(vals[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int colindex)) continue;
                    if (!int.TryParse(vals[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int rowhandle)) continue;
                    
                    if (rowhandle >= 0 && colindex >= 0)
                    {
                        if (targetCells.Length > 2)
                        {
                            ((System.Collections.Generic.List<PasteCellInfo>)targetCells[2]).Add(new PasteCellInfo
                            {
                                Row = rowhandle,
                                Column = colindex,
                                Value = vals[2].ToString()
                            });
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
