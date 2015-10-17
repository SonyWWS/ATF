//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// An interface for validating the value of a named property</summary>
    public interface IPropertyValueValidator
    {
        /// <summary>
        /// Returns false if the given formatted value is invalid for the given property name of data owner</summary>
        /// <param name="propertyName">User-readable property name</param>
        /// <param name="formattedValue">Proposed new value for this property</param>
        /// <param name="errorMessage">User-readable error message or empty string if there was no error</param>
        /// <returns><c>True</c> if 'formattedValue' is valid</returns>       
        bool Validate(string propertyName, object formattedValue, out string errorMessage);
    }
}
