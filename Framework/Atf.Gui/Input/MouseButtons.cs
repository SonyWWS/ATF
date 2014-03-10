using System;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Mouse button enums</summary>
    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1048576,
        Right = 2097152,
        Middle = 4194304,
        XButton1 = 8388608,
        XButton2 = 16777216,
    }
}