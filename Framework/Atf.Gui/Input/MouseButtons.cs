using System;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Mouse button enums</summary>
    [Flags]
    public enum MouseButtons
    {
        /// <summary>No mouse buttons</summary>
        None = 0,
        /// <summary>Left mouse button</summary>
        Left = 1048576,
        /// <summary>Right mouse button</summary>
        Right = 2097152,
        /// <summary>Middle mouse button</summary>
        Middle = 4194304,
        /// <summary>X mouse button 1</summary>
        XButton1 = 8388608,
        /// <summary>X mouse button 2</summary>
        XButton2 = 16777216,
    }
}