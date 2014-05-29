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
            m_keyValues = new Dictionary<Key, object>(comparer);
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
        /// <returns>True iff map contains key</returns>
        public bool ContainsKey(Key key)
        {
            return m_keyValues.ContainsKey(key);
        }

        /// <summary>
        /// Checks if map contains a given key/value pair</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True iff map contains key/value pair</returns>
        public bool ContainsKeyValue(Key key, Value value)
        {
            object values;
            if (!m_keyValues.TryGetValue(key, out values))
                return false;

            List<Value> valuesList = values as List<Value>;
            if (valuesList != null)
                return valuesList.Contains(value);

            return values.Equals(value);
        }

        /// <summary>
        /// Finds the collection of values associated with the given key. If the key isn't found,
        /// an empty collection is returned.</summary>
        /// <param name="key">Key</param>
        /// <returns>A collection of 0 or more values</returns>
        public IEnumerable<Value> Find(Key key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object values;
            if (!m_keyValues.TryGetValue(key, out values))
                return EmptyEnumerable<Value>.Instance;

            IEnumerable<Value> result = values as IEnumerable<Value>;
            if (result == null)
                result = new Value[] { (Value)values };
            return result;
        }

        /// <summary>
        /// Finds the first value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <returns>The first value for the key, or null if key is not in map</returns>
        public Value FindFirst(Key key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object values;
            if (!m_keyValues.TryGetValue(key, out values))
                return default(Value);
            List<Value> valuesArray = values as List<Value>;
            return (valuesArray != null) ? valuesArray[0] : (Value)values;
        }

        /// <summary>
        /// Finds the last value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <returns>The last value for the key, or null if key is not in map</returns>
        public Value FindLast(Key key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object values;
            if (!m_keyValues.TryGetValue(key, out values))
                return default(Value);
            List<Value> valuesArray = values as List<Value>;
            return (valuesArray != null) ? valuesArray[valuesArray.Count - 1] : (Value)values;
        }

        /// <summary>
        /// Gets the first value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <param name="result">The first value for the key, or default if key is not in map</param>
        /// <returns>True iff the key has an associated value</returns>
        public bool TryGetFirst(Key key, out Value result)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object values;
            if (!m_keyValues.TryGetValue(key, out values))
            {
                result = default(Value);
                return false;
            }

            List<Value> valuesList = values as List<Value>;
            result = (valuesList != null) ? valuesList[0] : (Value)values;
            return true;
        }

        /// <summary>
        /// Gets the last value associated with the given key</summary>
        /// <param name="key">Key</param>
        /// <param name="result">The last value for the key, or default if key is not in map</param>
        /// <returns>True iff the key has an associated value</returns>
        public bool TryGetLast(Key key, out Value result)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            object values;
            if (!m_keyValues.TryGetValue(key, out values))
            {
                result = default(Value);
                return false;
            }

            List<Value> valuesList = values as List<Value>;
            result = (valuesList != null) ? valuesList[valuesList.Count - 1] : (Value)values;
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
        /// <remarks>If the value is already present, it is removed and then added,
        /// so duplicate values aren't possible</remarks>
        public void Add(Key key, Value value)
        {
            object values;
            if (!m_keyValues.TryGetValue(key, out values))
                m_keyValues[key] = value;
            else
            {
                List<Value> valuesArray = values as List<Value>;
                if (valuesArray == null)
                {
                    m_keyValues.Remove(key);
                    valuesArray = new List<Value>(2);
                    m_keyValues.Add(key, valuesArray);
                    valuesArray.Add((Value)values);
                }
                valuesArray.Remove(value);
                valuesArray.Add(value);
            }
        }

        /// <summary>
        /// Adds a key/value pair to the map so that the value is first among possibly
        /// many values that are already associated with this key</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <remarks>If the value is already present, it is removed and then added,
        /// so duplicate values aren't possible.</remarks>
        public void AddFirst(Key key, Value value)
        {
            object values;
            if (!m_keyValues.TryGetValue(key, out values))
                m_keyValues[key] = value;
            else
            {
                List<Value> valuesArray = values as List<Value>;
                if (valuesArray == null)
                {
                    m_keyValues.Remove(key);
                    valuesArray = new List<Value>(2);
                    m_keyValues.Add(key, valuesArray);
                    valuesArray.Add((Value)values);
                }
                valuesArray.Remove(value);
                valuesArray.Insert(0, value);
            }
        }

        /// <summary>
        /// Removes all values associated with the given key from the map</summary>
        /// <param name="key">Key</param>
        /// <returns>True iff the value was removed</returns>
        public bool Remove(Key key)
        {
            return m_keyValues.Remove(key);
        }

        /// <summary>
        /// Removes a value from the map</summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>True iff the value was removed</returns>
        public bool Remove(Key key, Value value)
        {
            object values;
            if (m_keyValues.TryGetValue(key, out values))
            {
                List<Value> valueList = values as List<Value>;
                if (valueList == null)
                {
                    if (value.Equals(values))
                    {
                        m_keyValues.Remove(key);
                        return true;
                    }
                }
                else
                {
                    // remove from the end
                    for (int i = valueList.Count - 1; i >= 0; i--)
                    {
                        if (valueList[i].Equals(value))
                        {
                            valueList.RemoveAt(i);
                            // if only a single value remains in the list, replace it with the single value
                            if (valueList.Count == 1)
                                m_keyValues[key] = valueList[0];
                            return true;
                        }
                    }
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

        private readonly Dictionary<Key, object> m_keyValues;
    }
}
