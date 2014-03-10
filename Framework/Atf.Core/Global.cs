//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Class that holds a global instance of any reference type</summary>
    /// <typeparam name="T">Type to be instanced as a global</typeparam>
    /// <remarks>Global differs from the Singleton design pattern in that the instance
    /// isn't guaranteed to be the only instance of the given type.</remarks>
    public static class Global<T> where T : class, new()
    {
        /// <summary>
        /// Gets the global instance</summary>
        public static readonly T Instance = new T();
    }
}
