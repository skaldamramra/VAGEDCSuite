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
            
            for (int i = 0; i < cells.Length; i++)
            {
                if (i > 0)
                {
                    if (i % 16 != 0)
                        sb.Append("\t");
                    else
                        sb.AppendLine();
                }
                
                if (cells[i] != null)
                {
                    string value = cells[i].ToString();
                    sb.Append(value);
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
            // Placeholder - actual implementation would handle paste operations
        }
    }
}
