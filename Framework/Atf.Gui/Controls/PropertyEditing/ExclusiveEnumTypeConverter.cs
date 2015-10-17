//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// TypeConverter for use with enum editors, like EnumUITypeEditor. Converts integers to strings.
    /// In ATF property editors, the user will not be able to enter a value that does not match our list of names.</summary>
    public class ExclusiveEnumTypeConverter : EnumTypeConverter
    {
        /// <summary>
        /// Constructor</summary>
        public ExclusiveEnumTypeConverter()
        {
        }

        /// <summary>
        /// Constructor using enum names</summary>
        /// <param name="names">Enum names</param>
        /// <remarks>Enum values default to successive integers, starting with 0.</remarks>
        public ExclusiveEnumTypeConverter(string[] names)
            : base(names)
        {
            DefineEnum(names);
        }

        /// <summary>
        /// Constructor using enum names and values</summary>
        /// <param name="names">Enum names</param>
        /// <param name="values">Enum values</param>
        public ExclusiveEnumTypeConverter(string[] names, int[] values)
            : base(names, values)
        {
            DefineEnum(names, values);
        }

        /// <summary>
        /// Returns whether this object supports a standard set of values that can be picked from a list, using the specified context</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context that can be
        /// used to extract additional information about the environment from which this converter is invoked. This parameter or properties
        /// of this parameter can be null.</param>
        /// <returns><c>True</c> if object supports standard set of values that can be picked from a list in specified context</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of standard values for the data type this type converter is designed for when provided with a format context.</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context that can be
        /// used to extract additional information about the environment from which this converter is invoked. This parameter or properties
        /// of this parameter can be null.</param>
        /// <returns>A <see cref="T:System.ComponentModel.TypeConverter.StandardValuesCollection"/> that holds a standard set of valid values,
        /// or null if the data type does not support a standard set of values.</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = new StandardValuesCollection(new ReadOnlyCollection<string>(Names));
            return values;
        }

        /// <summary>
        /// Returns whether the collection of standard values returned is an exclusive list of possible values, using the specified context</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context that can be
        /// used to extract additional information about the environment from which this converter is invoked. This parameter or properties
        /// of this parameter can be null.</param>
        /// <returns><c>True</c> if collection of standard values returned is exclusive list of possible values in specified context</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
