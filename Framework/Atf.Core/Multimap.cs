//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// A class to map a key to one or more values</summary>
    /// <typeparam name="Key">Key to map values to</typeparam>
    /// <typeparam name="Value">Value to map to key</typeparam>
    [Serializable]
    public class Multimap<Key, Value>
    {
        /// <summary>
        /// Constructor that uses the default IEqualityComparer for the type of the key</summary>
        public Multimap()
            : this(null)
        {
        }
        
        /// <summary>
        /// Constructor that specifies the method to test for key equality</summary>
        /// <param name="comparer">The comparer used to compare keys, or null to use the
        /// default comparer for the type of the key</param>
        public Multimap(IEqualityComparer<Key> comparer)
        {
            m_keyValues = new Dictionary<Key, List<Value>>(comparer);
        }
        
        /// <summary>
        /// Gets all keys in the map</summary>
        public IEnumerable<Key> Keys
        {
            get { return m_keyValues.Keys; }
        }

        /// <summary>
        /// Checks if map contains a given key</summary>
        /// <param name="key">Key</param>
        /// <returns><c>True</c> if map contains key</returns>
        public bool ContainsKey(Key key)
        {
            return m_keyValues.ContainsKey(key);
        }

        /// <summary>
        /// Checks if map contains a given key/value pair</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns><c>True</c> if map contains key/value pair</returns>
        public bool ContainsKeyValue(Key key, Value value)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
                return false;

            return values.Contains(value);
        }

        /// <summary>
        /// Finds the collection of values associated with the given key. If the key isn't found,
        /// an empty collection is returned.</summary>
        /// <param name="key">Key</param>
        /// <returns>A collection of 0 or more values</returns>
        public IEnumerable<Value> Find(Key key)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
                return EmptyEnumerable<Value>.Instance;

            return values;
        }

        /// <summary>
        /// Finds the first value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <returns>The first value for the key, or null if key is not in map</returns>
        public Value FindFirst(Key key)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
                return default(Value);
            return values[0];
        }

        /// <summary>
        /// Finds the last value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <returns>The last value for the key, or null if key is not in map</returns>
        public Value FindLast(Key key)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
                return default(Value);
            return values[values.Count - 1];
        }

        /// <summary>
        /// Gets the first value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <param name="result">The first value for the key, or default if key is not in map</param>
        /// <returns><c>True</c> if the key has an associated value</returns>
        public bool TryGetFirst(Key key, out Value result)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
            {
                result = default(Value);
                return false;
            }

            result = values[0];
            return true;
        }

        /// <summary>
        /// Gets the last value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <param name="result">The last value for the key, or default if key is not in map</param>
        /// <returns><c>True</c> if the key has an associated value</returns>
        public bool TryGetLast(Key key, out Value result)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
            {
                result = default(Value);
                return false;
            }

            result = values[values.Count - 1];
            return true;
        }
        
        /// <summary>
        /// Indexer, returns collection of values for a given key</summary>
        public IEnumerable<Value> this[Key key]
        {
            get { return Find(key); }
        }

        /// <summary>
        /// Adds a key/value pair to the map so that the value is last among possibly
        /// many values that are already associated with this key</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <remarks>If the value is already present, it is removed and then added last,
        /// so duplicate values aren't possible</remarks>
        public void Add(Key key, Value value)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
                values = m_keyValues[key] = new List<Value>();
            else
                values.Remove(value);

            values.Add(value);
        }

        /// <summary>
        /// Adds a key/value pair to the map so that the value is first among possibly
        /// many values that are already associated with this key</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <remarks>If the value is already present, it is removed and then added first,
        /// so duplicate values aren't possible.</remarks>
        public void AddFirst(Key key, Value value)
        {
            List<Value> values;
            if (!m_keyValues.TryGetValue(key, out values))
                values = m_keyValues[key] = new List<Value>();
            else
                values.Remove(value);

            values.Insert(0, value);
        }

        /// <summary>
        /// Removes all values associated with the given key from the map</summary>
        /// <param name="key">Key</param>
        /// <returns><c>True</c> if the value was removed</returns>
        public bool Remove(Key key)
        {
            return m_keyValues.Remove(key);
        }

        /// <summary>
        /// Removes a value from the map</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns><c>True</c> if the value was removed</returns>
        public bool Remove(Key key, Value value)
        {
            List<Value> values;
            if (m_keyValues.TryGetValue(key, out values))
            {
                if (values.Remove(value))
                {
                    if (values.Count == 0)
                        m_keyValues.Remove(key);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears the map</summary>
        public void Clear()
        {
            m_keyValues.Clear();
        }

        // All lists will contain at least one object of type Value.
        private readonly Dictionary<Key, List<Value>> m_keyValues;
    }
}
