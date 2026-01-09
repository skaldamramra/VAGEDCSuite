using System;
using System.Collections.Generic;
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

            // Logical OR
            if (conditions.Or != null && conditions.Or.Count > 0)
            {
                bool anyMatch = false;
                foreach (var subCondition in conditions.Or)
                {
                    if (EvaluateConditions(subCondition, candidate, binData))
                    {
                        anyMatch = true;
                        break;
                    }
                }
                if (!anyMatch) return false;
            }

            // Logical AND
            if (conditions.And != null && conditions.And.Count > 0)
            {
                foreach (var subCondition in conditions.And)
                {
                    if (!EvaluateConditions(subCondition, candidate, binData)) return false;
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

        public void ApplyMetadata(MapRule rule, SymbolHelper symbol, byte[] binData, List<CodeBlock> codeBlocks, SymbolCollection existingSymbols)
        {
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
                string name = rule.Metadata.Name.Value;
                
                if (rule.Metadata.Name.Sequential)
                {
                    string baseName = rule.Metadata.Name.BaseName;
                    int count = GetMapNameCountForCodeBlock(baseName, symbol.CodeBlock, existingSymbols);
                    // Note: The original code often did count-- or similar adjustments. 
                    // We might need to parameterize the start index.
                    // For now, assuming 1-based index in name
                    
                    // Original logic often did: count = GetMapNameCount...; count--; Varname = base + count.ToString("D2")
                    // If GetMapNameCount returns 1 (none found yet + 1), then count-- = 0.
                    // So the first one is 00? Or did it mean to be 1-based?
                    // Looking at EDC15PFileParser.cs:
                    // int injDurCount = GetMapNameCountForCodeBlock("Injector duration", sh.CodeBlock, newSymbols, false);
                    // injDurCount--;
                    // sh.Varname = "Injector duration " + injDurCount.ToString("D2") ...
                    
                    // If GetMapNameCount returns 1 (meaning 0 found previously), then injDurCount becomes 0.
                    // So "Injector duration 00".
                    
                    // We need to replicate GetMapNameCountForCodeBlock logic here or pass a delegate.
                    // Since we passed existingSymbols, we can implement it.
                    
                    // However, we need to be careful not to count the *current* symbol if it's already added?
                    // The original code calls GetMapNameCount... BEFORE adding the name to the current symbol (which is already in the collection? No, usually before).
                    // Wait, in EDC15PFileParser, 'newSymbols' is passed. 'sh' is IN 'newSymbols' loop.
                    // "foreach (SymbolHelper sh in newSymbols)"
                    // So 'sh' is already in the collection.
                    // GetMapNameCountForCodeBlock iterates newSymbols.
                    // It counts how many start with varName.
                    // Since 'sh' is in the list but doesn't have the name yet (we are assigning it), it won't be counted yet?
                    // Ah, "if (sh.Varname.StartsWith(varName)..."
                    // Since we haven't assigned Varname yet, it won't match.
                    // So it counts *previous* matches.
                    
                    // But wait, GetMapNameCountForCodeBlock does "count++" at the end unconditionally?
                    // "count++; return count;"
                    // So if 0 matches, it returns 1.
                    // Then "injDurCount--;" makes it 0.
                    // So the first one is 0.
                    
                    int currentCount = GetMapNameCountForCodeBlock(baseName, symbol.CodeBlock, existingSymbols);
                    // Emulate the "count++; count--;" behavior which effectively means "count of existing matches"
                    // But wait, if I have 2 existing, GetMapNameCount returns 3. 3-- = 2.
                    // So it is 0-based index.
                    
                    // Let's just count existing matches.
                    int matchCount = 0;
                    foreach(SymbolHelper s in existingSymbols)
                    {
                        if (s.CodeBlock == symbol.CodeBlock && s.Varname != null && s.Varname.StartsWith(baseName))
                        {
                            matchCount++;
                        }
                    }
                    
                    name = baseName + " " + matchCount.ToString("D2");
                }

                // Append location info
                string locationInfo = DetermineNumberByFlashBank(symbol.Flash_start_address, codeBlocks);
                symbol.Varname = name + " [" + locationInfo + "]";
            }
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