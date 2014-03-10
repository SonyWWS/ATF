//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Class for a rule that constrains an attribute with a numeric value to have
    /// a maximum value</summary>
    public class NumericMaxRule : AttributeRule
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="maximum">Maximum value</param>
        /// <param name="inclusive">Value indicating if value may be equal to maximum</param>
        public NumericMaxRule(double maximum, bool inclusive)
        {
            m_maximum = maximum;
            m_inclusive = inclusive;
        }

                    /// <summary>
        /// Validates the given value for assignment to the given attribute</summary>
        /// <param name="value">Value to validate</param>
        /// <param name="info">Attribute info</param>
        /// <returns>True iff value is valid for the given attribute</returns>
        public override bool Validate(object value, AttributeInfo info)
        {
            if (value is Byte)
            {
                Byte v = (Byte)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is SByte)
            {
                SByte v = (SByte)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is UInt16)
            {
                UInt16 v = (UInt16)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is Int16)
            {
                Int16 v = (Int16)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is UInt32)
            {
                UInt32 v = (UInt32)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is Int32)
            {
                Int32 v = (Int32)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is UInt64)
            {
                UInt64 v = (UInt64)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is Int64)
            {
                Int64 v = (Int64)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is Single)
            {
                Single v = (Single)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }
            if (value is Double)
            {
                Double v = (Double)value;
                return v < m_maximum || (m_inclusive && v == m_maximum);
            }

            return false;
        }

        private readonly double m_maximum;
        private readonly bool m_inclusive;
    }
}
