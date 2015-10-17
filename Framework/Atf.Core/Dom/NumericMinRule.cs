//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Class for a rule that constrains an attribute with a numeric value to have
    /// a minimum value</summary>
    public class NumericMinRule : AttributeRule
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="minimum">Minimum value</param>
        /// <param name="inclusive">Value indicating if value may be equal to minimum</param>
        public NumericMinRule(double minimum, bool inclusive)
        {
            m_minimum = minimum;
            m_inclusive = inclusive;
        }

        /// <summary>
        /// Returns the minimum value used when validating an attribute value</summary>
        public double MinimumValue { get { return m_minimum; } }

        /// <summary>
        /// Returns whether the minimum value is inclusive when validating an attribute value</summary>
        public bool IsInclusive { get { return m_inclusive; } }

        /// <summary>
        /// Validates the given value for assignment to the given attribute</summary>
        /// <param name="value">Value to validate</param>
        /// <param name="info">Attribute info</param>
        /// <returns><c>True</c> if value is valid for the given attribute</returns>
        public override bool Validate(object value, AttributeInfo info)
        {
            if (value is Byte)
            {
                Byte v = (Byte)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is SByte)
            {
                SByte v = (SByte)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is UInt16)
            {
                UInt16 v = (UInt16)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is Int16)
            {
                Int16 v = (Int16)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is UInt32)
            {
                UInt32 v = (UInt32)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is Int32)
            {
                Int32 v = (Int32)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is UInt64)
            {
                UInt64 v = (UInt64)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is Int64)
            {
                Int64 v = (Int64)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is Single)
            {
                Single v = (Single)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }
            if (value is Double)
            {
                Double v = (Double)value;
                return v > m_minimum || (m_inclusive && v == m_minimum);
            }

            return false;
        }

        private readonly double m_minimum;
        private readonly bool m_inclusive;
    }
}
