using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace VAGSuite
{
    public class MapRuleRepository
    {
        private static Dictionary<string, List<MapRule>> _ruleCache = new Dictionary<string, List<MapRule>>();
        private static object _lock = new object();

        public List<MapRule> LoadRules(string ecuType)
        {
            lock (_lock)
            {
                if (_ruleCache.ContainsKey(ecuType))
                {
                    return _ruleCache[ecuType];
                }

                string fileName = ecuType + "_MapRules.xml";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                
                // Also check in a "MapRules" subdirectory
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MapRules", fileName);
                }

                if (!File.Exists(filePath))
                {
                    // Fallback for development environment
                    filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", fileName);
                }

                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Rule file not found: " + filePath);
                    return new List<MapRule>();
                }

                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MapRules));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        MapRules rules = (MapRules)serializer.Deserialize(fs);
                        if (rules.Rules == null) rules.Rules = new List<MapRule>();
                        
                        // Sort by priority (lower number = higher priority)
                        rules.Rules.Sort(delegate(MapRule r1, MapRule r2) {
                            return r1.Priority.CompareTo(r2.Priority);
                        });

                        _ruleCache[ecuType] = rules.Rules;
                        return rules.Rules;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading rules from " + filePath + ": " + ex.Message);
                    return new List<MapRule>();
                }
            }
        }

        public void ClearCache()
        {
            lock (_lock)
            {
                _ruleCache.Clear();
            }
        }
    }
}