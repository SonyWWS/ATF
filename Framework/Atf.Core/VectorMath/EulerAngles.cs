//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Euler angles representing a 3D rotation</summary>
    public struct EulerAngles3F : IFormattable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="angles">Euler angles on the x, y, and z axes</param>
        /// <param name="order">Order that the rotations should be applied</param>
        public EulerAngles3F(Vec3F angles, EulerAngleOrder order)
        {
            m_Angles = angles;
            m_Order = order;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="angles">Array of 3 floats to supply the Euler angles on
        /// the x, y, and z axes</param>
        /// <param name="order">Order that the rotations should be applied</param>
        public EulerAngles3F(float[] angles, EulerAngleOrder order)
        {
            m_Angles = new Vec3F(angles);
            m_Order = order;
        }

        /// <summary>
        /// Gets and sets the 3 Euler angles</summary>
        public Vec3F Angles
        {
            get { return m_Angles; }
            set { m_Angles = value; }
        }

        /// <summary>
        /// Gets and sets the order that the rotations should be applied in</summary>
        public EulerAngleOrder RotOrder
        {
            get { return m_Order; }
            set { m_Order = value; }
        }

        /// <summary>
        /// Calculates the Euler angles for rotating around, in order, the x, y, and z axes</summary>
        /// <returns>Euler angles</returns>
        public Vec3F CalculateOrderedAngles()
        {
            Matrix3F mat = CalculateMatrix();
            float x, y, z;
            mat.GetEulerAngles(out x, out y, out z);
            return new Vec3F(x, y, z);
        }

        /// <summary>
        /// Calculates the rotation matrix representing the given Euler angles and in the correct order</summary>
        /// <returns>Rotation matrix</returns>
        public Matrix3F CalculateMatrix()
        {
            Matrix3F M = new Matrix3F();
            Matrix3F temp = new Matrix3F();

            switch (m_Order)
            {
                case EulerAngleOrder.XYZ:
                    if (m_Angles.X != 0)
                    {
                        temp.RotX(m_Angles.X);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Y != 0)
                    {
                        temp.RotY(m_Angles.Y);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Z != 0)
                    {
                        temp.RotZ(m_Angles.Z);
                        M.Mul(M, temp);
                    }
                    break;
                case EulerAngleOrder.XZY:
                    if (m_Angles.X != 0)
                    {
                        temp.RotX(m_Angles.X);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Z != 0)
                    {
                        temp.RotZ(m_Angles.Z);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Y != 0)
                    {
                        temp.RotY(m_Angles.Y);
                        M.Mul(M, temp);
                    }
                    break;
                case EulerAngleOrder.YXZ:
                    if (m_Angles.Y != 0)
                    {
                        temp.RotY(m_Angles.Y);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.X != 0)
                    {
                        temp.RotX(m_Angles.X);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Z != 0)
                    {
                        temp.RotZ(m_Angles.Z);
                        M.Mul(M, temp);
                    }
                    break;
                case EulerAngleOrder.YZX:
                    if (m_Angles.Y != 0)
                    {
                        temp.RotY(m_Angles.Y);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Z != 0)
                    {
                        temp.RotZ(m_Angles.Z);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.X != 0)
                    {
                        temp.RotX(m_Angles.X);
                        M.Mul(M, temp);
                    }
                    break;
                case EulerAngleOrder.ZXY:
                    if (m_Angles.Z != 0)
                    {
                        temp.RotZ(m_Angles.Z);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.X != 0)
                    {
                        temp.RotX(m_Angles.X);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Y != 0)
                    {
                        temp.RotY(m_Angles.Y);
                        M.Mul(M, temp);
                    }
                    break;
                case EulerAngleOrder.ZYX:
                    if (m_Angles.Z != 0)
                    {
                        temp.RotZ(m_Angles.Z);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.Y != 0)
                    {
                        temp.RotY(m_Angles.Y);
                        M.Mul(M, temp);
                    }
                    if (m_Angles.X != 0)
                    {
                        temp.RotX(m_Angles.X);
                        M.Mul(M, temp);
                    }
                    break;
                default:
                    throw new Exception("Illegal euler rotation order ");
            }

            return M;
        }

        /// <summary>
        /// Returns a string representation of this object for GUIs. For persistence, use
        /// ToString("R", CultureInfo.InvariantCulture).</summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                 "{0}{4} {1}{4} {2}{4} {3}",
                 Angles.X.ToString(format, formatProvider),
                 Angles.Y.ToString(format, formatProvider),
                 Angles.Z.ToString(format, formatProvider),
                 RotOrder,
                 listSeparator);

        }
        #endregion

        private Vec3F m_Angles;
        private EulerAngleOrder m_Order;
    };
}
