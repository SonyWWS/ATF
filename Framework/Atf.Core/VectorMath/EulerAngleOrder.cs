//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Order of Euler rotations (reads left to right)</summary>
    public enum EulerAngleOrder
    {
        /// <summary>X -> Y -> Z</summary>
        XYZ,
        /// <summary>Y -> Z -> X</summary>
        YZX,
        /// <summary>Z -> X -> Y</summary>
        ZXY,
        /// <summary>X -> Z -> Y</summary>
        XZY,
        /// <summary>Y -> X -> Z</summary>
        YXZ,
        /// <summary>Z -> Y -> X</summary>
        ZYX
    };
}
