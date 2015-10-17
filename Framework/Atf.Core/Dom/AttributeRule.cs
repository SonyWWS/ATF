//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Abstract base class for a rule that constrains attribute values. A rule for a numeric
    /// value might constrain the value to a range, while a rule for a string value might
    /// constrain it to an enumeration. See AttributeInfo.AddRule.</summary>
    public abstract class AttributeRule
    {
        /// <summary>
        /// Validates the given value for assignment to the given attribute</summary>
        /// <param name="value">Value to validate</param>
        /// <param name="info">Attribute info</param>
        /// <returns>True, if value is valid for the given attribute</returns>
        public abstract bool Validate(object value, AttributeInfo info);
    }
}
