using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Windows.Forms;

namespace VAGSuite.Services
{
    public class MapDescription
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UseCase { get; set; }
        public string TuningAdvice { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\n\nDescription:\n{1}\n\nUse Case:\n{2}\n\nTuning Advice:\n{3}", 
                Title, Description, UseCase, TuningAdvice);
        }
    }

    public class MapDescriptionService
    {
        private static MapDescriptionService _instance;
        private Dictionary<string, MapDescription> _descriptions = new Dictionary<string, MapDescription>();

        public static MapDescriptionService Instance
        {
            get
            {
                if (_instance == null) _instance = new MapDescriptionService();
                return _instance;
            }
        }

        private MapDescriptionService()
        {
            LoadDescriptions();
        }

        private void LoadDescriptions()
        {
            string path = Path.Combine(Application.StartupPath, "Docs/EDC15P_Descriptions.xml");
            
            // Debug: Try to find the file in parent directories if not found (for dev environment)
            if (!File.Exists(path))
            {
                path = Path.Combine(Application.StartupPath, "../../Docs/EDC15P_Descriptions.xml");
            }

            if (!File.Exists(path))
            {
                Console.WriteLine("MapDescriptionService: XML file not found at " + path);
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                int count = 0;
                foreach (XmlNode node in doc.SelectNodes("//Map"))
                {
                    MapDescription desc = new MapDescription
                    {
                        Key = node.Attributes["Key"]?.Value,
                        Title = node.SelectSingleNode("Title")?.InnerText,
                        Description = node.SelectSingleNode("Description")?.InnerText,
                        UseCase = node.SelectSingleNode("UseCase")?.InnerText,
                        TuningAdvice = node.SelectSingleNode("TuningAdvice")?.InnerText
                    };

                    if (!string.IsNullOrEmpty(desc.Key))
                    {
                        _descriptions[desc.Key.ToLower()] = desc;
                        count++;
                    }
                }
                Console.WriteLine("MapDescriptionService: Loaded " + count + " descriptions from " + path);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading map descriptions: " + ex.Message);
            }
        }

        public string GetDescription(string varName)
        {
            string lowerVar = varName.ToLower();
            
            // Try exact match first
            foreach (var kvp in _descriptions)
            {
                if (lowerVar.Contains(kvp.Key))
                {
                    return kvp.Value.ToString();
                }
            }

            return string.Empty;
        }
    }
}