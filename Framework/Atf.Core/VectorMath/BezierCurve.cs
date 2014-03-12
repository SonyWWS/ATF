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
        /// Gets the string representation of this Scea.VectorMath.BezierCurve structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the 3d Bezier curve</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>Obtains the string representation of this Scea.VectorMath.BezierCurve structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D Bezier curve</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return m_ctrlPoints[0].X.ToString("R") + ", " + m_ctrlPoints[0].Y.ToString("R") + ", " + m_ctrlPoints[0].Z.ToString("R") + ", " +
                    m_ctrlPoints[1].X.ToString("R") + ", " + m_ctrlPoints[1].Y.ToString("R") + ", " + m_ctrlPoints[1].Z.ToString("R") + ", " +
                    m_ctrlPoints[2].X.ToString("R") + ", " + m_ctrlPoints[2].Y.ToString("R") + ", " + m_ctrlPoints[2].Z.ToString("R") + ", " +
                    m_ctrlPoints[3].X.ToString("R") + ", " + m_ctrlPoints[3].Y.ToString("R") + ", " + m_ctrlPoints[3].Z.ToString("R");

            return String.Format
           (
                "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})",
                ((double)m_ctrlPoints[0].X).ToString(format, formatProvider),
                ((double)m_ctrlPoints[0].Y).ToString(format, formatProvider),
                ((double)m_ctrlPoints[0].Z).ToString(format, formatProvider),
                ((double)m_ctrlPoints[1].X).ToString(format, formatProvider),
                ((double)m_ctrlPoints[1].Y).ToString(format, formatProvider),
                ((double)m_ctrlPoints[1].Z).ToString(format, formatProvider),
                ((double)m_ctrlPoints[2].X).ToString(format, formatProvider),
                ((double)m_ctrlPoints[2].Y).ToString(format, formatProvider),
                ((double)m_ctrlPoints[2].Z).ToString(format, formatProvider),
                ((double)m_ctrlPoints[3].X).ToString(format, formatProvider),
                ((double)m_ctrlPoints[3].Y).ToString(format, formatProvider),
                ((double)m_ctrlPoints[3].Z).ToString(format, formatProvider)
           );

        }

        readonly Vec3F[] m_ctrlPoints;
        readonly Vec3F[] m_coefficients;
    }
}
