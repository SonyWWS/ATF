//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// A type converter which provides a set of standard values when queried </summary>
    /// <remarks> This can be used on a PropertyDescriptor to force property editors 
    /// to see a set of standard values. e.g. a string based property can be made to act
    /// like an Enum property. </remarks>
    public class StandardValuesConverter : TypeConverter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="values">Values to include in the collection</param>
        public StandardValuesConverter(Array values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            m_values = new StandardValuesCollection(values);
            
            // Check if all items are exclusive using a hash set
            var set = new HashSet<object>();
            m_exclusive = values.Cast<object>().All(set.Add); 
        }

        /// <summary>
        /// Gets whether all the values in the collection are unique.</summary>
        /// <param name="context">Not used</param>
        /// <returns><c>True</c> if all values in the collection are exclusive</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return m_exclusive;
        }

        /// <summary>
        /// Gets whether standard values are supported by this TypeConverter. Returns true.</summary>
        /// <param name="context">Not used</param>
        /// <returns>True</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Gets the collection of standard values.</summary>
        /// <param name="context">Not used</param>
        /// <returns>The collection of standard values</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return m_values;
        }

        private readonly StandardValuesCollection m_values;
        private readonly bool m_exclusive;
    }

    /// <summary>
    /// A type converter which dynamically provides a set of standard values when queried </summary>
    /// <remarks> This can be used on a PropertyDescriptor to force property editors 
    /// to see a set of standard values. e.g. a string based property can be made to act
    /// like an Enum property. </remarks>
    public class DynamicStandardValuesConverter : TypeConverter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="getValues">Function that provides the GetStandardValues functionality</param>
        public DynamicStandardValuesConverter(Func<ITypeDescriptorContext, Array> getValues)
        {
            Requires.NotNull(getValues, "getValues");
            m_getValues = getValues;
        }

        /// <summary>
        /// Gets whether all the values in the collection are unique.</summary>
        /// <param name="context">Not used</param>
        /// <returns><c>True</c> if all values in the collection are exclusive</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            var values = m_getValues(context);
            var set = new HashSet<object>();
            return values.Cast<object>().All(set.Add);
        }

        /// <summary>
        /// Gets whether standard values are supported by this TypeConverter. Returns true.</summary>
        /// <param name="context">Not used</param>
        /// <returns>True</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Gets the collection of standard values.</summary>
        /// <param name="context">Not used</param>
        /// <returns>The collection of standard values</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(m_getValues(context));
        }

        private readonly Func<ITypeDescriptorContext, Array> m_getValues;
    }
}
