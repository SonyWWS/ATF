//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Interface for filtering a string value</summary>
    public interface IStringValueFilter
    {
        /// <summary>
        /// Filter the specified token out of the string value, using the helper object if needed.</summary>
        /// <param name="instance">Helper object</param>
        /// <param name="stringValue">The string to filter</param>
        /// <param name="token">The token to filter out of the string</param>
        /// <returns>The filtered string</returns>
        string FilterStringValue(object instance, string stringValue, string token);
    }
}
