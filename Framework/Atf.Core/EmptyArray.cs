//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Static class to instantiate empty arrays</summary>
    /// <typeparam name="T">Array element type</typeparam>
    public static class EmptyArray<T>
    {
        /// <summary>
        /// Gets the single instance of an empty array</summary>
        public static readonly T[] Instance = new T[0];
    }
}
