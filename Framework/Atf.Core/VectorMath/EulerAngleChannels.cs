//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Bit-flags indicating which axes have rotations</summary>
    [Flags]
    public enum EulerAngleChannels
    {
        X = 0x1,
        Y = 0x2,
        Z = 0x4,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        XYZ = X | Y | Z
    };

    /// <summary>
    /// Extension methods for the EulerAngleChannels enum</summary>
    public static class EulerAngleChannel
    {
        /// <summary>
        /// Indicates whether x-axis angle flag is set</summary>
        /// <param name="EulerAngleChannels">EulerAngleChannels value</param>
        /// <returns>True iff x-axis angle flag is set</returns>
        public static bool FreedomInX(this EulerAngleChannels EulerAngleChannels)
        {
            return (EulerAngleChannels & EulerAngleChannels.X) != 0;
        }

        /// <summary>
        /// Indicates whether y-axis angle flag is set</summary>
        /// <param name="EulerAngleChannels">EulerAngleChannels value</param>
        /// <returns>True iff y-axis angle flag is set</returns>
        public static bool FreedomInY(this EulerAngleChannels EulerAngleChannels)
        {
            return (EulerAngleChannels & EulerAngleChannels.Y) != 0;
        }

        /// <summary>
        /// Indicates whether z-axis angle flag is set</summary>
        /// <param name="EulerAngleChannels">EulerAngleChannels value</param>
        /// <returns>True iff z-axis angle flag is set</returns>
        public static bool FreedomInZ(this EulerAngleChannels EulerAngleChannels)
        {
            return (EulerAngleChannels & EulerAngleChannels.Z) != 0;
        }
    }
}
