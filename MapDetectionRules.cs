using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAGSuite
{
    // Enums for comparison operators
    public enum ComparisonOperator
    {
        Eq,      // Equal
        Ne,      // Not Equal
        Lt,      // Less Than
        Le,      // Less Than or Equal
        Gt,      // Greater Than
        Ge       // Greater Than or Equal
    }

    [XmlRoot("MapRules")]
    public class MapRules
    {
        [XmlAttribute("ecuType")]
        public string EcuType { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("MapRule")]
        public List<MapRule> Rules { get; set; }
    }

    public class MapRule
    {
        [XmlAttribute("priority")]
        public int Priority { get; set; }

        [XmlAttribute("id")]
        public string Id { get; set; }

        public string Description { get; set; }

        public Conditions Conditions { get; set; }

        public Metadata Metadata { get; set; }
    }

    public class Conditions
    {
        [XmlElement("Length")]
        public IntCondition Length { get; set; }

        [XmlElement("XAxisID")]
        public AxisIDCondition XAxisID { get; set; }

        [XmlElement("YAxisID")]
        public AxisIDCondition YAxisID { get; set; }

        [XmlElement("XAxisLength")]
        public IntCondition XAxisLength { get; set; }

        [XmlElement("YAxisLength")]
        public IntCondition YAxisLength { get; set; }

        [XmlElement("MapSelector")]
        public MapSelectorCondition MapSelector { get; set; }

        [XmlElement("ByteCheck")]
        public ByteCheckCondition ByteCheck { get; set; }

        [XmlElement("AxisValueCheck")]
        public AxisValueCheckCondition AxisValueCheck { get; set; }

        [XmlElement("MapSelectorProperty")]
        public MapSelectorPropertyCondition MapSelectorProperty { get; set; }

        [XmlElement("CodeBlock")]
        public CodeBlockCondition CodeBlock { get; set; }

        [XmlElement("CustomValidator")]
        public List<CustomValidator> CustomValidators { get; set; }

        [XmlArray("Or")]
        [XmlArrayItem("Conditions")]
        public List<Conditions> Or { get; set; }

        [XmlArray("And")]
        [XmlArrayItem("Conditions")]
        public List<Conditions> And { get; set; }
    }

    public class IntCondition
    {
        [XmlText]
        public int Value { get; set; }

        [XmlAttribute("min")]
        public int Min { get; set; }

        [XmlAttribute("max")]
        public int Max { get; set; }

        public bool ShouldSerializeValue() { return Value != 0; }
        public bool ShouldSerializeMin() { return Min != 0; }
        public bool ShouldSerializeMax() { return Max != 0; }
    }

    public class AxisIDCondition
    {
        [XmlAttribute("high")]
        public string HighHex { get; set; }

        [XmlAttribute("exact")]
        public string ExactHex { get; set; }

        public int? GetHigh()
        {
            if (string.IsNullOrEmpty(HighHex)) return null;
            return Convert.ToInt32(HighHex, 16);
        }

        public int? GetExact()
        {
            if (string.IsNullOrEmpty(ExactHex)) return null;
            return Convert.ToInt32(ExactHex, 16);
        }
    }

    public class MapSelectorCondition
    {
        [XmlAttribute("numRepeats")]
        public int NumRepeats { get; set; }
    }

    /// <summary>
    /// Byte-level inspection condition.
    /// Checks raw byte values at specific addresses relative to the symbol.
    /// </summary>
    public class ByteCheckCondition
    {
        /// <summary>
        /// Address reference: "y_axis_address", "x_axis_address", "flash_start_address", or custom offset
        /// </summary>
        [XmlAttribute("address")]
        public string Address { get; set; }

        /// <summary>
        /// Offset from the address (default: 0)
        /// </summary>
        [XmlAttribute("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Expected byte value in hex (e.g., "0x00")
        /// </summary>
        [XmlAttribute("expected")]
        public string Expected { get; set; }

        /// <summary>
        /// Comparison operator (default: Eq)
        /// </summary>
        [XmlAttribute("op")]
        public string Op { get; set; }
    }

    /// <summary>
    /// Axis value runtime check condition.
    /// Checks actual axis data values at runtime.
    /// </summary>
    public class AxisValueCheckCondition
    {
        /// <summary>
        /// Which axis to check: "X", "Y", or "Z"
        /// </summary>
        [XmlAttribute("axis")]
        public string Axis { get; set; }

        /// <summary>
        /// Comparison operator: "lt" (less than), "le", "gt", "ge", "eq", "ne"
        /// </summary>
        [XmlAttribute("comparison")]
        public string Comparison { get; set; }

        /// <summary>
        /// Value to compare against
        /// </summary>
        [XmlAttribute("value")]
        public double Value { get; set; }

        /// <summary>
        /// Check type: "max" (maximum value), "min" (minimum value)
        /// </summary>
        [XmlAttribute("checkType")]
        public string CheckType { get; set; }
    }

    /// <summary>
    /// MapSelector property condition.
    /// Checks properties of the MapSelector object.
    /// </summary>
    public class MapSelectorPropertyCondition
    {
        /// <summary>
        /// Property name: "NumRepeats", "MapIndexes.Length", "MapIndexes[0]", etc.
        /// </summary>
        [XmlAttribute("property")]
        public string Property { get; set; }

        /// <summary>
        /// Comparison operator: "eq", "ne", "lt", "le", "gt", "ge"
        /// </summary>
        [XmlAttribute("comparison")]
        public string Comparison { get; set; }

        /// <summary>
        /// Expected value
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// CodeBlock context condition.
    /// Checks the code block context for the symbol.
    /// </summary>
    public class CodeBlockCondition
    {
        /// <summary>
        /// Expected code block ID(s). Can be comma-separated for multiple values.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Code block type: "Automatic", "Manual", "4x4", or empty for any
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }
    }

    public class CustomValidator
    {
        [XmlAttribute("method")]
        public string Method { get; set; }

        [XmlAttribute("args")]
        public string Args { get; set; }
    }

    public class Metadata
    {
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public NameMetadata Name { get; set; }
        public AxisMetadata XAxis { get; set; }
        public AxisMetadata YAxis { get; set; }
        public AxisMetadata ZAxis { get; set; }
        
        [XmlElement("Correction")]
        public double? Correction { get; set; }
        
        [XmlElement("Offset")]
        public double? Offset { get; set; }
    }

    public class NameMetadata
    {
        [XmlText]
        public string Value { get; set; }

        [XmlAttribute("sequential")]
        public bool Sequential { get; set; }

        [XmlAttribute("baseName")]
        public string BaseName { get; set; }

        /// <summary>
        /// If true, sequential naming is scoped to the current code block only.
        /// </summary>
        [XmlAttribute("codeBlockScoped")]
        public bool CodeBlockScoped { get; set; }

        [XmlElement("Default")]
        public string Default { get; set; }

        [XmlElement("IfMapSelectorNotEmpty")]
        public string IfMapSelectorNotEmpty { get; set; }

        [XmlArray("ConditionalTemplate")]
        [XmlArrayItem("When")]
        public List<ConditionalTemplate> ConditionalTemplates { get; set; }
    }

    /// <summary>
    /// Conditional template for dynamic naming.
    /// </summary>
    public class ConditionalTemplate
    {
        /// <summary>
        /// Condition type: "MapSelectorExists", "MapSelectorProperty", "ByteCheck", etc.
        /// </summary>
        [XmlAttribute("conditionType")]
        public string ConditionType { get; set; }

        /// <summary>
        /// Condition details (property name, expected value, etc.)
        /// </summary>
        [XmlAttribute("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Template string with placeholders like {temperature}, {index}, etc.
        /// </summary>
        [XmlElement("Template")]
        public string Template { get; set; }
    }

    public class AxisMetadata
    {
        public string Description { get; set; }
        public string Units { get; set; }
        public double? Correction { get; set; }
        public double? Offset { get; set; }
    }

    /// <summary>
    /// Dynamic axis address calculation metadata.
    /// Allows specifying offsets from the flash start address for axis data.
    /// </summary>
    public class AxisAddressOffset
    {
        /// <summary>
        /// Offset from flash_start_address to X-axis data (usually negative)
        /// </summary>
        [XmlAttribute("x")]
        public int X { get; set; }

        /// <summary>
        /// Offset from flash_start_address to Y-axis data (usually negative)
        /// </summary>
        [XmlAttribute("y")]
        public int Y { get; set; }
    }

    /// <summary>
    /// Temperature range extraction from MapSelector.
    /// Used for dynamic naming of temperature-based maps.
    /// </summary>
    public class TemperatureFromSelector
    {
        /// <summary>
        /// Index in the MapSelector's MapIndexes array
        /// </summary>
        [XmlAttribute("selectorIndex")]
        public int SelectorIndex { get; set; }

        /// <summary>
        /// Output format: "C" (Celsius), "F" (Fahrenheit)
        /// </summary>
        [XmlAttribute("unit")]
        public string Unit { get; set; }
    }
}