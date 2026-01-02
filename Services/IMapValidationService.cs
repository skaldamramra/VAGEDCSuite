using System;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    public interface IMapValidationService
    {
        /// <summary>
        /// Validates a value entered in the grid editor based on current view type and bit width.
        /// </summary>
        /// <param name="value">The value to validate</param>
        /// <param name="state">Current viewer state</param>
        /// <param name="errorText">Error message if invalid</param>
        /// <param name="validatedValue">The parsed/clamped value if valid</param>
        /// <returns>True if valid</returns>
        bool ValidateEditorValue(object value, MapViewerState state, out string errorText, out object validatedValue);
    }
}