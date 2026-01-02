using System;
using VAGSuite.Models;

namespace VAGSuite.Services
{
    public class MapValidationService : IMapValidationService
    {
        private readonly IDataConversionService _dataConversionService;

        public MapValidationService(IDataConversionService dataConversionService)
        {
            _dataConversionService = dataConversionService ?? throw new ArgumentNullException(nameof(dataConversionService));
        }

        public bool ValidateEditorValue(object value, MapViewerState state, out string errorText, out object validatedValue)
        {
            errorText = string.Empty;
            validatedValue = value;

            try
            {
                if (state.Data.IsSixteenBit)
                {
                    if (state.Configuration.ViewType == ViewType.Hexadecimal)
                    {
                        int val = Convert.ToInt32(Convert.ToString(value), 16);
                        if (val > 0xFFFF)
                        {
                            errorText = "Value not valid (max 0xFFFF)";
                            return false;
                        }
                    }
                    else
                    {
                        double dval = Convert.ToDouble(value);
                        int val = 0;
                        if (state.Configuration.ViewType == ViewType.Easy)
                        {
                            // Easy view values are already converted back to raw in the caller or handled here
                            // The legacy code had complex logic for ActiveEditor, but we simplify to raw range check
                            val = (int)Math.Round((dval - state.Configuration.CorrectionOffset) / (state.Configuration.CorrectionFactor != 0 ? state.Configuration.CorrectionFactor : 1));
                        }
                        else
                        {
                            val = (int)Math.Round(dval);
                        }

                        if (Math.Abs(val) > 78643) // Legacy limit
                        {
                            errorText = "Value not valid (out of range)";
                            return false;
                        }
                        validatedValue = val;
                    }
                }
                else
                {
                    if (state.Configuration.ViewType == ViewType.Hexadecimal)
                    {
                        int val = Convert.ToInt32(Convert.ToString(value), 16);
                        if (val > 0xFF)
                        {
                            errorText = "Value not valid (max 0xFF)";
                            return false;
                        }
                    }
                    else
                    {
                        double dval = Convert.ToDouble(value);
                        int val = 0;
                        if (state.Configuration.ViewType == ViewType.Easy)
                        {
                            val = (int)Math.Round((dval - state.Configuration.CorrectionOffset) / (state.Configuration.CorrectionFactor != 0 ? state.Configuration.CorrectionFactor : 1));
                        }
                        else
                        {
                            val = (int)Math.Round(dval);
                        }

                        if (val > 255 || val < 0)
                        {
                            errorText = "Value not valid (0-255)";
                            return false;
                        }
                        validatedValue = val;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                errorText = ex.Message;
                return false;
            }
        }
    }
}