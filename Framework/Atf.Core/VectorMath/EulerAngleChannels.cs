//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Bit-flags indicating which axes have rotations</summary>
    [Flags]
    public enum EulerAngleChannels
    {
        /// <summary>X-axis</summary>
        X = 0x1,
        /// <summary>Y-axis</summary>
        Y = 0x2,
        /// <summary>Z-axis</summary>
        Z = 0x4,
        /// <summary>X- or y-axis</summary>
        XY = X | Y,
        /// <summary>X- or z-axis</summary>
        XZ = X | Z,
        /// <summary>Y- or z-axis</summary>
        YZ = Y | Z,
        /// <summary>X- or y- or z-axis</summary>
        XYZ = X | Y | Z
    };

    /// <summary>
    /// Extension methods for the EulerAngleChannels enum</summary>
    public static class EulerAngleChannel
    {
        /// <summary>
        /// Indicates whether x-axis angle flag is set</summary>
        /// <param name="EulerAngleChannels">EulerAngleChannels value</param>
        /// <returns><c>True</c> if x-axis angle flag is set</returns>
        public static bool FreedomInX(this EulerAngleChannels EulerAngleChannels)
        {
            return (EulerAngleChannels & EulerAngleChannels.X) != 0;
        }

        /// <summary>
        /// Indicates whether y-axis angle flag is set</summary>
        /// <param name="EulerAngleChannels">EulerAngleChannels value</param>
        /// <returns><c>True</c> if y-axis angle flag is set</returns>
        public static bool FreedomInY(this EulerAngleChannels EulerAngleChannels)
        {
            return (EulerAngleChannels & EulerAngleChannels.Y) != 0;
        }

        /// <summary>
        /// Indicates whether z-axis angle flag is set</summary>
        /// <param name="EulerAngleChannels">EulerAngleChannels value</param>
        /// <returns><c>True</c> if z-axis angle flag is set</returns>
        public static bool FreedomInZ(this EulerAngleChannels EulerAngleChannels)
        {
            return (EulerAngleChannels & EulerAngleChannels.Z) != 0;
        }
    }
}
