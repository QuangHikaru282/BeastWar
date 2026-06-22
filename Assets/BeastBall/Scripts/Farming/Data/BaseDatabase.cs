using System.Collections.Generic;
using UnityEngine;

namespace BeastBall.Farming
{
    /// <summary>
    /// Base class for creating generic ScriptableObject databases with O(1) string key lookup.
    /// </summary>
    public abstract class BaseDatabase<T> : ScriptableObject where T : class, IDatabaseEntry
    {
        [SerializeReference]
        public List<T> Entries;

        private Dictionary<string, T> m_LookupDictionnary;

        public T GetFromID(string uniqueID)
        {
            if (m_LookupDictionnary != null && m_LookupDictionnary.TryGetValue(uniqueID, out var entry))
            {
                return entry;
            }
            return null;
        }

        // Must be explicitly called at runtime (e.g., by GameManager) to build the lookup table
        public void Init()
        {
            m_LookupDictionnary = new Dictionary<string, T>();
            if (Entries == null) return;

            foreach (var entry in Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key)) continue;
                m_LookupDictionnary.TryAdd(entry.Key, entry);
            }
        }
    }
}
