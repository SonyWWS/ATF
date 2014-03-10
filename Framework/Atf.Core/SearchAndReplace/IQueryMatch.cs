//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface to the actual data that matched a query</summary>
    public interface IQueryMatch
    {
        /// <summary>
        /// Gets the value of the matching data</summary>
        /// <returns>Matching data value</returns>
        object GetValue();

        /// <summary>
        /// Replaces the value of the matching data</summary>
        /// <param name="value">New value</param>
        void SetValue(object value);
    }
}
