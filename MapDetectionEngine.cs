using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VAGSuite
{
    public class MapDetectionEngine
    {
        public bool EvaluateRule(MapRule rule, SymbolHelper candidate, byte[] binData)
        {
            if (rule.Conditions == null) return false;
            return EvaluateConditions(rule.Conditions, candidate, binData);
        }

        private bool EvaluateConditions(Conditions conditions, SymbolHelper candidate, byte[] binData)
        {
            // Length check
            if (conditions.Length != null && conditions.Length.ShouldSerializeValue())
            {
                if (candidate.Length != conditions.Length.Value) return false;
            }
            if (conditions.Length != null && conditions.Length.ShouldSerializeMin())
            {
                if (candidate.Length < conditions.Length.Min) return false;
            }
            if (conditions.Length != null && conditions.Length.ShouldSerializeMax())
            {
                if (candidate.Length > conditions.Length.Max) return false;
            }

            // Axis ID checks
            if (conditions.XAxisID != null)
            {
                int? high = conditions.XAxisID.GetHigh();
                if (high.HasValue)
                {
                    if (candidate.X_axis_ID / 256 != high.Value) return false;
                }
                int? exact = conditions.XAxisID.GetExact();
                if (exact.HasValue)
                {
                    if (candidate.X_axis_ID != exact.Value) return false;
                }
            }

            if (conditions.YAxisID != null)
            {
                int? high = conditions.YAxisID.GetHigh();
                if (high.HasValue)
                {
                    if (candidate.Y_axis_ID / 256 != high.Value) return false;
                }
                int? exact = conditions.YAxisID.GetExact();
                if (exact.HasValue)
                {
                    if (candidate.Y_axis_ID != exact.Value) return false;
                }
            }

            // Axis Length checks
            if (conditions.XAxisLength != null && conditions.XAxisLength.ShouldSerializeValue())
            {
                if (candidate.X_axis_length != conditions.XAxisLength.Value) return false;
            }
            if (conditions.YAxisLength != null && conditions.YAxisLength.ShouldSerializeValue())
            {
                if (candidate.Y_axis_length != conditions.YAxisLength.Value) return false;
            }

            // Map Selector checks
            if (conditions.MapSelector != null)
            {
                if (candidate.MapSelector == null) return false;
                if (conditions.MapSelector.NumRepeats != 0 && candidate.MapSelector.NumRepeats != conditions.MapSelector.NumRepeats) return false;
            }

            // Byte Check condition
            if (conditions.ByteCheck != null)
            {
                if (!EvaluateByteCheck(conditions.ByteCheck, candidate, binData)) return false;
            }

            // Axis Value Check condition
            if (conditions.AxisValueCheck != null)
            {
                if (!EvaluateAxisValueCheck(conditions.AxisValueCheck, candidate, binData)) return false;
            }

            // MapSelector Property condition
            if (conditions.MapSelectorProperty != null)
            {
                if (!EvaluateMapSelectorProperty(conditions.MapSelectorProperty, candidate)) return false;
            }

            // CodeBlock condition
            if (conditions.CodeBlock != null)
            {
                if (!EvaluateCodeBlockCondition(conditions.CodeBlock, candidate)) return false;
            }

            // Logical OR (Handle multiple <Or> blocks)
            if (conditions.Or != null && conditions.Or.Count > 0)
            {
                foreach (var group in conditions.Or)
                {
                    bool anyMatchInGroup = false;
                    if (group.Conditions != null)
                    {
                        foreach (var subCondition in group.Conditions)
                        {
                            if (EvaluateConditions(subCondition, candidate, binData))
                            {
                                anyMatchInGroup = true;
                                break;
                            }
                        }
                    }
                    if (!anyMatchInGroup) return false; // This group failed to match any condition
                }
            }

            // Logical AND (Handle multiple <And> blocks)
            if (conditions.And != null && conditions.And.Count > 0)
            {
                foreach (var group in conditions.And)
                {
                    if (group.Conditions != null)
                    {
                        foreach (var subCondition in group.Conditions)
                        {
                            if (!EvaluateConditions(subCondition, candidate, binData)) return false;
                        }
                    }
                }
            }

            // Custom Validators
            if (conditions.CustomValidators != null)
            {
                foreach (var validator in conditions.CustomValidators)
                {
                    if (!ExecuteCustomValidator(validator, candidate, binData)) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluates a ByteCheck condition.
        /// Checks raw byte values at specific addresses relative to the symbol.
        /// </summary>
        private bool EvaluateByteCheck(ByteCheckCondition byteCheck, SymbolHelper candidate, byte[] binData)
        {
            if (byteCheck == null) return true;

            int address = 0;
            switch (byteCheck.Address.ToLower())
            {
                case "y_axis_address":
                    address = candidate.Y_axis_address;
                    break;
                case "x_axis_address":
                    address = candidate.X_axis_address;
                    break;
                case "flash_start_address":
                    address = (int)candidate.Flash_start_address;
                    break;
                default:
                    // Custom offset from flash_start_address
                    if (byteCheck.Address.StartsWith("+"))
                    {
                        int offset = int.Parse(byteCheck.Address.Substring(1));
                        address = (int)(candidate.Flash_start_address + offset);
                    }
                    else if (byteCheck.Address.StartsWith("-"))
                    {
                        int offset = int.Parse(byteCheck.Address.Substring(1));
                        address = (int)(candidate.Flash_start_address - offset);
                    }
                    else
                    {
                        // Try to parse as absolute address
                        if (!int.TryParse(byteCheck.Address, out address)) return true;
                    }
                    break;
            }

            address += byteCheck.Offset;

            // Check bounds
            if (address < 0 || address >= binData.Length) return false;

            int expectedValue = Convert.ToInt32(byteCheck.Expected, 16);
            int actualValue = binData[address];

            string op = string.IsNullOrEmpty(byteCheck.Op) ? "eq" : byteCheck.Op.ToLower();
            switch (op)
            {
                case "eq":
                    return actualValue == expectedValue;
                case "ne":
                    return actualValue != expectedValue;
                case "lt":
                    return actualValue < expectedValue;
                case "le":
                    return actualValue <= expectedValue;
                case "gt":
                    return actualValue > expectedValue;
                case "ge":
                    return actualValue >= expectedValue;
                default:
                    return actualValue == expectedValue;
            }
        }

        /// <summary>
        /// Evaluates an AxisValueCheck condition.
        /// Checks actual axis data values at runtime.
        /// </summary>
        private bool EvaluateAxisValueCheck(AxisValueCheckCondition axisCheck, SymbolHelper candidate, byte[] binData)
        {
            if (axisCheck == null) return true;

            // Get the axis data
            double axisValue = 0;
            int axisAddress = 0;
            int axisLength = 0;

            switch (axisCheck.Axis.ToUpper())
            {
                case "X":
                    axisAddress = candidate.X_axis_address;
                    axisLength = candidate.X_axis_length;
                    break;
                case "Y":
                    axisAddress = candidate.Y_axis_address;
                    axisLength = candidate.Y_axis_length;
                    break;
                case "Z":
                    // For Z-axis, we check the map data itself
                    axisAddress = (int)candidate.Flash_start_address;
                    axisLength = candidate.Length;
                    break;
                default:
                    return true;
            }

            // Read axis values and find max/min
            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            for (int i = 0; i < axisLength * 2 && (axisAddress + i) < binData.Length - 1; i += 2)
            {
                if (axisAddress + i + 1 >= binData.Length) break;
                ushort rawValue = (ushort)((binData[axisAddress + i + 1] << 8) | binData[axisAddress + i]);
                double value = rawValue * candidate.Correction + candidate.Offset;
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }

            // Check the appropriate value
            double checkValue = axisCheck.CheckType.ToLower() == "min" ? minValue : maxValue;

            switch (axisCheck.Comparison.ToLower())
            {
                case "lt":
                    return checkValue < axisCheck.Value;
                case "le":
                    return checkValue <= axisCheck.Value;
                case "gt":
                    return checkValue > axisCheck.Value;
                case "ge":
                    return checkValue >= axisCheck.Value;
                case "eq":
                    return Math.Abs(checkValue - axisCheck.Value) < 0.001;
                case "ne":
                    return Math.Abs(checkValue - axisCheck.Value) >= 0.001;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Evaluates a MapSelectorProperty condition.
        /// Checks properties of the MapSelector object.
        /// </summary>
        private bool EvaluateMapSelectorProperty(MapSelectorPropertyCondition propCheck, SymbolHelper candidate)
        {
            if (propCheck == null) return true;
            if (candidate.MapSelector == null) return false;

            double actualValue = 0;
            bool isNumeric = double.TryParse(propCheck.Value, out double expectedValue);

            switch (propCheck.Property.ToLower())
            {
                case "numrepeats":
                    actualValue = candidate.MapSelector.NumRepeats;
                    break;
                case "mapindexes.length":
                case "mapindexes.count":
                    actualValue = candidate.MapSelector.MapIndexes != null ? candidate.MapSelector.MapIndexes.Length : 0;
                    break;
                default:
                    // Handle indexed properties like "MapIndexes[0]"
                    if (propCheck.Property.StartsWith("mapindexes[", StringComparison.OrdinalIgnoreCase))
                    {
                        int indexStart = propCheck.Property.IndexOf('[') + 1;
                        int indexEnd = propCheck.Property.IndexOf(']');
                        if (indexStart > 0 && indexEnd > indexStart)
                        {
                            string indexStr = propCheck.Property.Substring(indexStart, indexEnd - indexStart);
                            if (int.TryParse(indexStr, out int index) && candidate.MapSelector.MapIndexes != null && index < candidate.MapSelector.MapIndexes.Length)
                            {
                                actualValue = candidate.MapSelector.MapIndexes[index];
                            }
                        }
                    }
                    break;
            }

            switch (propCheck.Comparison.ToLower())
            {
                case "eq":
                    return Math.Abs(actualValue - expectedValue) < 0.001;
                case "ne":
                    return Math.Abs(actualValue - expectedValue) >= 0.001;
                case "lt":
                    return actualValue < expectedValue;
                case "le":
                    return actualValue <= expectedValue;
                case "gt":
                    return actualValue > expectedValue;
                case "ge":
                    return actualValue >= expectedValue;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Evaluates a CodeBlock condition.
        /// Checks the code block context for the symbol.
        /// </summary>
        private bool EvaluateCodeBlockCondition(CodeBlockCondition codeBlockCheck, SymbolHelper candidate)
        {
            if (codeBlockCheck == null) return true;

            // If no ID specified, just check that we have a code block
            if (string.IsNullOrEmpty(codeBlockCheck.Id))
            {
                return candidate.CodeBlock != 0 || !string.IsNullOrEmpty(codeBlockCheck.Type);
            }

            // Parse expected IDs (comma-separated)
            var expectedIds = codeBlockCheck.Id.Split(',').Select(int.Parse).ToList();

            if (!expectedIds.Contains(candidate.CodeBlock)) return false;

            // Type check would require additional context (codeBlocks list)
            // For now, we just check the ID

            return true;
        }

        private bool ExecuteCustomValidator(CustomValidator validator, SymbolHelper candidate, byte[] binData)
        {
            // Reflection-based validator execution could go here, or a switch statement for known validators
            // For safety and performance in .NET 4.0, a switch statement is often better unless we need dynamic extensibility
            
            // Example implementation:
            /*
            if (validator.Method == "IsValidTemperatureAxis")
            {
                // Call existing method from somewhere or implement here
                // return IsValidTemperatureAxis(binData, candidate, validator.Args);
            }
            */
            
            // For now, return true to not block if not implemented
            return true; 
        }

        public void ApplyMetadata(MapRule rule, SymbolHelper symbol, byte[] binData, List<CodeBlock> codeBlocks, SymbolCollection existingSymbols, List<SymbolHelper> currentPassSymbols = null)
        {
            // Mark the symbol as coming from MapRules XML
            symbol.MapSource = MapSource.MapRulesXml;

            if (rule.Metadata == null) return;

            if (!string.IsNullOrEmpty(rule.Metadata.Category))
                symbol.Category = rule.Metadata.Category;
            
            if (!string.IsNullOrEmpty(rule.Metadata.Subcategory))
                symbol.Subcategory = rule.Metadata.Subcategory;

            if (rule.Metadata.Correction.HasValue)
                symbol.Correction = rule.Metadata.Correction.Value;

            if (rule.Metadata.Offset.HasValue)
                symbol.Offset = rule.Metadata.Offset.Value;

            // Axis Metadata
            if (rule.Metadata.XAxis != null)
            {
                if (!string.IsNullOrEmpty(rule.Metadata.XAxis.Description)) symbol.X_axis_descr = rule.Metadata.XAxis.Description;
                if (!string.IsNullOrEmpty(rule.Metadata.XAxis.Units)) symbol.XaxisUnits = rule.Metadata.XAxis.Units;
                if (rule.Metadata.XAxis.Correction.HasValue) symbol.X_axis_correction = rule.Metadata.XAxis.Correction.Value;
                if (rule.Metadata.XAxis.Offset.HasValue) symbol.X_axis_offset = rule.Metadata.XAxis.Offset.Value;
            }

            if (rule.Metadata.YAxis != null)
            {
                if (!string.IsNullOrEmpty(rule.Metadata.YAxis.Description)) symbol.Y_axis_descr = rule.Metadata.YAxis.Description;
                if (!string.IsNullOrEmpty(rule.Metadata.YAxis.Units)) symbol.YaxisUnits = rule.Metadata.YAxis.Units;
                if (rule.Metadata.YAxis.Correction.HasValue) symbol.Y_axis_correction = rule.Metadata.YAxis.Correction.Value;
                if (rule.Metadata.YAxis.Offset.HasValue) symbol.Y_axis_offset = rule.Metadata.YAxis.Offset.Value;
            }

            if (rule.Metadata.ZAxis != null)
            {
                if (!string.IsNullOrEmpty(rule.Metadata.ZAxis.Description)) symbol.Z_axis_descr = rule.Metadata.ZAxis.Description;
            }

            // Naming Logic
            if (rule.Metadata.Name != null)
            {
                string name = rule.Metadata.Name.Value ?? "";

                // Meticulous: Calculate the variant index for multi-maps
                // to ensure the correct temperature is extracted from MapData.
                int variantIndex = 0;
                if (symbol.MapSelector != null && symbol.MapSelector.NumRepeats > 1)
                {
                    // existingSymbols contains candidates.
                    // currentPassSymbols contains XML clones already processed.
                    // We check both to find our relative position in the set.
                    if (existingSymbols != null)
                    {
                        foreach (SymbolHelper s in existingSymbols)
                        {
                            if (s.MapSelector == symbol.MapSelector && s.Flash_start_address < symbol.Flash_start_address)
                            {
                                variantIndex++;
                            }
                        }
                    }
                }

                // Handle conditional templates for dynamic naming
                if (rule.Metadata.Name.ConditionalTemplates != null && rule.Metadata.Name.ConditionalTemplates.Count > 0)
                {
                    name = EvaluateConditionalTemplate(rule.Metadata.Name.ConditionalTemplates, symbol, binData, variantIndex);
                }
                else if (!string.IsNullOrEmpty(rule.Metadata.Name.Value))
                {
                    // Safety check: Ensure (XML) suffix is present if not using templates
                    if (!rule.Metadata.Name.Value.Contains("(XML)"))
                    {
                        name = rule.Metadata.Name.Value + " (XML)";
                    }
                }

                if (rule.Metadata.Name.Sequential)
                {
                    // Meticulous: If we have a conditional template, it might have produced a unique name
                    // (like "SOI 90 deg C"). If Sequential is also true, we use the BaseName for counting,
                    // or the template result if BaseName is missing.
                    string baseName = rule.Metadata.Name.BaseName ?? name;
                    if (string.IsNullOrEmpty(baseName)) baseName = "Unknown Map";

                    // Ensure baseName has (XML) for the count check to work against other XML maps
                    if (!baseName.Contains("(XML)")) baseName += " (XML)";
                    
                    int matchCount = 0;

                    // Count in existing (likely FileParser) symbols
                    if (existingSymbols != null)
                    {
                        foreach (SymbolHelper s in existingSymbols)
                        {
                            if (s.Varname != null && s.Varname.StartsWith(baseName))
                            {
                                if (!rule.Metadata.Name.CodeBlockScoped || s.CodeBlock == symbol.CodeBlock)
                                {
                                    matchCount++;
                                }
                            }
                        }
                    }

                    // Count in current pass (XML) symbols
                    if (currentPassSymbols != null)
                    {
                        foreach (SymbolHelper s in currentPassSymbols)
                        {
                            if (s.Varname != null && s.Varname.StartsWith(baseName))
                            {
                                if (!rule.Metadata.Name.CodeBlockScoped || s.CodeBlock == symbol.CodeBlock)
                                {
                                    matchCount++;
                                }
                            }
                        }
                    }

                    // If the current 'name' (from template) is already more specific than baseName,
                    // we append the counter to the baseName to avoid stomping the template result
                    // UNLESS the template result is identical to baseName.
                    // For EDC15P SOI, we want "SOI 90 deg C (XML)". If we append a counter,
                    // it should be "SOI 90 deg C (XML) 00" only if needed.
                    // However, the user reports SOI doesn't have temperature.
                    // This is because 'name' was overwritten by "baseName + counter".
                    
                    name = baseName + " " + matchCount.ToString("D2");
                }

                // Append location info
                string locationInfo = DetermineNumberByFlashBank(symbol.Flash_start_address, codeBlocks);
                symbol.Varname = name + " [" + locationInfo + "]";
            }
        }

        /// <summary>
        /// Evaluates conditional templates for dynamic naming.
        /// </summary>
        private string EvaluateConditionalTemplate(List<ConditionalTemplate> templates, SymbolHelper symbol, byte[] binData, int variantIndex)
        {
            foreach (var template in templates)
            {
                if (template == null || string.IsNullOrEmpty(template.ConditionType)) continue;
                bool conditionMet = false;

                switch (template.ConditionType.ToLower())
                {
                    case "mapselectorexists":
                        conditionMet = symbol.MapSelector != null;
                        break;
                    case "mapselectorproperty":
                        // Parse condition like "NumRepeats=10" or "MapIndexes.Length>1"
                        var parts = template.Condition.Split(new[] { '=', '>', '<' }, 2);
                        if (parts.Length >= 2 && symbol.MapSelector != null)
                        {
                            string propName = parts[0].Trim();
                            string expectedStr = parts[1].Trim();
                            double expectedValue;
                            if (double.TryParse(expectedStr, out expectedValue))
                            {
                                double actualValue = 0;
                                switch (propName.ToLower())
                                {
                                    case "numrepeats":
                                        actualValue = symbol.MapSelector.NumRepeats;
                                        break;
                                    case "mapindexes.length":
                                    case "mapindexes.count":
                                        actualValue = symbol.MapSelector.MapIndexes?.Length ?? 0;
                                        break;
                                }
                                conditionMet = Math.Abs(actualValue - expectedValue) < 0.001;
                            }
                        }
                        break;
                    case "bytecheck":
                        // Parse condition like "y_axis_address+0=00"
                        var byteParts = template.Condition.Split('=');
                        if (byteParts.Length >= 2)
                        {
                            string addressSpec = byteParts[0].Trim();
                            string expectedHex = byteParts[1].Trim();

                            int address = 0;
                            if (addressSpec.StartsWith("y_axis_address"))
                            {
                                address = symbol.Y_axis_address + int.Parse(addressSpec.Substring("y_axis_address".Length));
                            }
                            else if (addressSpec.StartsWith("x_axis_address"))
                            {
                                address = symbol.X_axis_address + int.Parse(addressSpec.Substring("x_axis_address".Length));
                            }

                            if (address >= 0 && address < binData.Length)
                            {
                                int expected = Convert.ToInt32(expectedHex, 16);
                                conditionMet = binData[address] == expected;
                            }
                        }
                        break;
                }

                if (conditionMet)
                {
                    // Replace placeholders in template
                    string result = template.Template;
                    if (symbol.MapSelector != null && symbol.MapSelector.MapIndexes != null && symbol.MapSelector.MapIndexes.Length > 1)
                    {
                        // Meticulous: Check if the selector index is empty (legacy EDC15P logic)
                        bool allEmpty = true;
                        foreach (int idx in symbol.MapSelector.MapIndexes) if (idx != 0) allEmpty = false;
                        
                        if (!allEmpty)
                        {
                            result = result.Replace("{temperature}", GetTemperatureSOIRange(symbol.MapSelector, variantIndex).ToString());
                            result = result.Replace("{index}", variantIndex.ToString());
                            return result;
                        }
                    }
                    else if (template.ConditionType.ToLower() == "default")
                    {
                        return result;
                    }
                }
            }

            // Return default if no condition matched
            return templates.FirstOrDefault(t => t.ConditionType.ToLower() == "default")?.Template ?? "";
        }

        /// <summary>
        /// Calculates temperature range from MapSelector.
        /// This is a simplified version - the actual implementation would need the full context.
        /// </summary>
        private double GetTemperatureSOIRange(MapSelector selector, int index)
        {
            // Calculation from legacy parser: (val * 0.1) - 273.1
            if (selector.MapData != null && index < selector.MapData.Length)
            {
                double val = Convert.ToDouble(selector.MapData.GetValue(index));
                return Math.Round((val * 0.1) - 273.1, 0);
            }
            return 0;
        }

        private int GetMapNameCountForCodeBlock(string varName, int codeBlock, SymbolCollection symbols)
        {
            int count = 0;
            foreach (SymbolHelper sh in symbols)
            {
                if (sh.Varname != null && sh.Varname.StartsWith(varName) && sh.CodeBlock == codeBlock)
                {
                    count++;
                }
            }
            return count;
        }

        private string DetermineNumberByFlashBank(long address, List<CodeBlock> currBlocks)
        {
            foreach (CodeBlock cb in currBlocks)
            {
                if (cb.StartAddress <= address && cb.EndAddress >= address)
                {
                    if (cb.BlockGearboxType == GearboxType.Automatic)
                    {
                        return "codeblock " + cb.CodeID.ToString() + ", automatic";
                    }
                    else if (cb.CodeID == 2) return "codeblock " + cb.CodeID.ToString() + ", manual";
                    else if (cb.CodeID == 3) return "codeblock " + cb.CodeID.ToString() + ", 4x4";
                    return "codeblock " + cb.CodeID.ToString();
                }
            }
            long bankNumber = address / 0x10000;
            return "flashbank " + bankNumber.ToString();
        }
    }
}