using System;

namespace VAGSuite.Helpers
{
    /// <summary>
    /// Helper methods for querying symbol collections.
    /// Extracted from frmMain to provide reusable utilities without state.
    /// </summary>
    public static class SymbolQueryHelper
    {
        /// <summary>
        /// Gets the width (Y-axis length) of a symbol.
        /// </summary>
        public static int GetSymbolWidth(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.Y_axis_length;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the height (X-axis length) of a symbol.
        /// </summary>
        public static int GetSymbolHeight(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.X_axis_length;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the total length in bytes of a symbol.
        /// </summary>
        public static int GetSymbolLength(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.Length;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the flash start address of a symbol.
        /// </summary>
        public static long GetSymbolAddress(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.Flash_start_address;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the X-axis address for a symbol.
        /// </summary>
        public static int GetXAxisAddress(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.X_axis_address;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the Y-axis address for a symbol.
        /// </summary>
        public static int GetYAxisAddress(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return sh.Y_axis_address;
                }
            }
            return 0;
        }

        /// <summary>
        /// Reads X-axis values from file for a symbol.
        /// </summary>
        public static int[] GetXAxisValues(string filename, SymbolCollection symbolCollection, string symbolname)
        {
            int ylen = GetSymbolWidth(symbolCollection, symbolname);
            int yaddress = GetYAxisAddress(symbolCollection, symbolname);
            int[] retval = new int[ylen];
            retval.Initialize();
            
            if (yaddress > 0)
            {
                retval = Tools.Instance.readdatafromfileasint(filename, yaddress, ylen, Tools.Instance.m_currentFileType);
            }
            
            return retval;
        }

        /// <summary>
        /// Reads Y-axis values from file for a symbol.
        /// </summary>
        public static int[] GetYAxisValues(string filename, SymbolCollection symbolCollection, string symbolname)
        {
            int xlen = GetSymbolHeight(symbolCollection, symbolname);
            int xaddress = GetXAxisAddress(symbolCollection, symbolname);
            int[] retval = new int[xlen];
            retval.Initialize();
            
            if (xaddress > 0)
            {
                retval = Tools.Instance.readdatafromfileasint(filename, xaddress, xlen, Tools.Instance.m_currentFileType);
            }
            
            return retval;
        }

        /// <summary>
        /// Gets the table dimensions (columns and rows) for a symbol.
        /// </summary>
        public static void GetTableDimensions(SymbolCollection symbolCollection, string symbolname, 
            out int columns, out int rows)
        {
            columns = GetSymbolWidth(symbolCollection, symbolname);
            rows = GetSymbolHeight(symbolCollection, symbolname);
        }

        /// <summary>
        /// Finds a symbol by name in the collection.
        /// </summary>
        public static SymbolHelper FindSymbol(SymbolCollection symbolCollection, string symbolName)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolName || sh.Userdescription == symbolName)
                {
                    return sh;
                }
            }
            return new SymbolHelper();
        }

        /// <summary>
        /// Checks if a symbol exists in the collection.
        /// </summary>
        public static bool SymbolExists(SymbolCollection symbolCollection, string symbolname)
        {
            foreach (SymbolHelper sh in symbolCollection)
            {
                if (sh.Varname == symbolname || sh.Userdescription == symbolname)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
