//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Manages access to a cached value that can be invalidated and is recomputed if (and only if) it has to be</summary>
    /// <typeparam name="T">Type of the wrapped value</typeparam>
    public class Cached<T>
    {
        /// <summary>
        /// Constructor using computation function</summary>
        /// <param name="computeFunc">Function to recompute the value</param>
        /// <remarks>When using this constructor, the value starts as dirty and is computed on first read access.</remarks>
        public Cached(Func<T> computeFunc)
        {
            m_dirty = true;
            m_computeFunc = computeFunc;
        }

        /// <summary>
        /// Constructor using computation function and current value</summary>
        /// <param name="computeFunc">Function to recompute the value</param>
        /// <param name="currentValue">Current value (setting it prevents initial computation)</param>
        /// <remarks>When using this constructor, the value is NOT computed initially but only after first invalidation.</remarks>
        public Cached(Func<T> computeFunc, T currentValue)
        {
            m_dirty = false;
            m_computeFunc = computeFunc;
            m_value = currentValue;
        }

        /// <summary>
        /// Gets the current value (recomputed if necessary)</summary>
        public T Value
        {
            get
            {
                if (m_dirty)
                {
                    m_value = m_computeFunc();
                    m_dirty = false;
                }
                return m_value;
            }
        }

        /// <summary>
        /// Invalidates the value. It is recomputed on next read access.</summary>
        public void Invalidate()
        {
            m_dirty = true;
        }
        
        private bool m_dirty = true;
        private T m_value;
        private readonly Func<T> m_computeFunc;
    }
}
