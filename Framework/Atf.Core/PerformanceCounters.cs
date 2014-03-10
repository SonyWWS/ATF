//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Text;

namespace Sce.Atf.GraphicsApplications
{
    /// <summary>
    /// Associates each unique string ID with an integer ("count") whose meaning is
    /// client-specific. The resulting data can be prepared in a simple report, sorted by ID.</summary>
    public class PerformanceCounters
    {
        /// <summary>
        /// Clears the counts and IDs</summary>
        public void Clear()
        {
            m_counts.Clear();
        }

        /// <summary>
        /// Gets or sets the count identified by the given ID</summary>
        /// <param name="id">ID</param>
        /// <returns>Count identified by given ID</returns>
        public int this[string id]
        {
            get
            {
                int count;
                m_counts.TryGetValue(id, out count);
                return count;
            }
            set
            {
                int count;
                m_counts.TryGetValue(id, out count);
                m_counts[id] = value;
            }
        }

        /// <summary>
        /// Returns a simple report of each ID and its associated count, sorted alphabetically by ID</summary>
        /// <returns>Simple report of each ID and its associated count, sorted alphabetically by ID</returns>
        public string GetDisplayString()
        {
            StringBuilder sb = new StringBuilder();

            List<KeyValuePair<string, int>> counts = new List<KeyValuePair<string, int>>(m_counts);
            foreach (KeyValuePair<string, int> count in counts)
            {
                sb.Append(count.Key);
                sb.Append(": ");
                sb.Append(count.Value.ToString());
                sb.Append(" ");
            }

            return sb.ToString();
        }

        private readonly SortedDictionary<string, int> m_counts = new SortedDictionary<string, int>();
    }
}
