//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Class representing a 3D Bezier curve segment</summary>
    public class BezierCurve : IFormattable
    {
        /// <summary>
        /// Construct a Bezier curve from 4 control points</summary>
        /// <param name="controlPoints">Array of control points</param>
        public BezierCurve(Vec3F[] controlPoints)
        {
            m_ctrlPoints = controlPoints;

            // Calcualte coefficients
            m_coefficients = new Vec3F[3];

            m_coefficients[2] = 3.0f * (m_ctrlPoints[1] - m_ctrlPoints[0]);
            m_coefficients[1] = 3.0f * (m_ctrlPoints[2] - m_ctrlPoints[1]) - m_coefficients[2];
            m_coefficients[0] = m_ctrlPoints[3] - m_ctrlPoints[0] - m_coefficients[2] - m_coefficients[1];
        }

        /// <summary>
        /// Evaluate the curve on parameter t [0,1]</summary>
        /// <param name="t">Parameter</param>
        /// <returns>Resulting 3D vector</returns>
        public Vec3F Evaluate(float t)
        {
            Vec3F result = new Vec3F();

            float tSquared = t * t;
            float tCubed = tSquared * t;

            result = (m_coefficients[0] * tCubed) + (m_coefficients[1] * tSquared) + (m_coefficients[2] * t) + m_ctrlPoints[0];
            return result;
        }

        /// <summary>
        /// Gets Bezier curve control points</summary>
        public Vec3F[] ControlPoints
        {
            get { return m_ctrlPoints; }
        }

        /// <summary>
        /// Gets Bezier curve coefficients</summary>
        public Vec3F[] Coefficients
        {
            get { return m_coefficients; }
        }

        /// <summary>
        /// Returns a string representation of this object for GUIs. For persistence, use
        /// ToString("R", CultureInfo.InvariantCulture).</summary>
        /// <returns>String representation of object</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        #region IFormattable
        /// <summary>
        /// Returns the string representation of this object</summary>
        /// <param name="format">Optional standard numeric format string for a floating point number.
        /// If null, "R" is used for round-trip support in case the string is persisted.
        /// http://msdn.microsoft.com/en-us/library/vstudio/dwhawy9k(v=vs.100).aspx </param>
        /// <param name="formatProvider">Optional culture-specific formatting provider. This is usually
        /// a CultureInfo object or NumberFormatInfo object. If null, the current culture is used.
        /// Use CultureInfo.InvariantCulture for persistence.</param>
        /// <returns>String representation of object</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                "{1}{0} {2}{0} {3}{0} {4}{0} {5}{0} {6}{0} {7}{0} {8}{0} {9}{0} {10}{0} {11}{0} {12}",
                listSeparator,
                m_ctrlPoints[0].X.ToString(format, formatProvider),
                m_ctrlPoints[0].Y.ToString(format, formatProvider),
                m_ctrlPoints[0].Z.ToString(format, formatProvider),
                m_ctrlPoints[1].X.ToString(format, formatProvider),
                m_ctrlPoints[1].Y.ToString(format, formatProvider),
                m_ctrlPoints[1].Z.ToString(format, formatProvider),
                m_ctrlPoints[2].X.ToString(format, formatProvider),
                m_ctrlPoints[2].Y.ToString(format, formatProvider),
                m_ctrlPoints[2].Z.ToString(format, formatProvider),
                m_ctrlPoints[3].X.ToString(format, formatProvider),
                m_ctrlPoints[3].Y.ToString(format, formatProvider),
                m_ctrlPoints[3].Z.ToString(format, formatProvider));

        }
        #endregion

        readonly Vec3F[] m_ctrlPoints;
        readonly Vec3F[] m_coefficients;
    }
}
