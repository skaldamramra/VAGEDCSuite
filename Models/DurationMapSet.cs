using System.Collections.Generic;

namespace VAGSuite.Models
{
    /// <summary>
    /// Contains a set of duration maps (DURA_0 through DURA_5)
    /// and optional selector map.
    /// </summary>
    public class DurationMapSet
    {
        /// <summary>
        /// List of duration maps (typically 6 maps).
        /// </summary>
        public List<EOIMap> Maps { get; set; }
        
        /// <summary>
        /// Selector map (null if single duration configuration).
        /// </summary>
        public EOIMap Selector { get; set; }
        
        /// <summary>
        /// Gets whether this is a single duration configuration.
        /// </summary>
        public bool IsSingleDuration
        {
            get { return Selector == null || Maps.Count <= 1; }
        }
        
        /// <summary>
        /// Gets the number of duration maps.
        /// </summary>
        public int Count
        {
            get { return Maps != null ? Maps.Count : 0; }
        }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public DurationMapSet()
        {
            Maps = new List<EOIMap>();
        }
        
        /// <summary>
        /// Gets the duration map at the specified index.
        /// </summary>
        public EOIMap GetMap(int index)
        {
            if (Maps == null || index < 0 || index >= Maps.Count) return null;
            return Maps[index];
        }
        
        /// <summary>
        /// Adds a duration map to the set.
        /// </summary>
        public void AddMap(EOIMap map)
        {
            if (Maps == null) Maps = new List<EOIMap>();
            Maps.Add(map);
        }
    }
}
