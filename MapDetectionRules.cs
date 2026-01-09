using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAGSuite
{
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

        [XmlElement("CustomValidator")]
        public List<CustomValidator> CustomValidators { get; set; }

        [XmlElement("Or")]
        public List<Conditions> Or { get; set; }

        [XmlElement("And")]
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
        
        [XmlElement("Default")]
        public string Default { get; set; }
        
        [XmlElement("IfMapSelectorNotEmpty")]
        public string IfMapSelectorNotEmpty { get; set; }
    }

    public class AxisMetadata
    {
        public string Description { get; set; }
        public string Units { get; set; }
        public double? Correction { get; set; }
        public double? Offset { get; set; }
    }
}