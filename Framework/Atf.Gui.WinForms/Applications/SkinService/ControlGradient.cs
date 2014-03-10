//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Struct for linear gradient information properties</summary>
    public struct ControlGradient
    {
        /// <summary>
        /// Gets or sets starting gradient color</summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color StartColor { get; set; }

        /// <summary>
        /// Gets or sets ending gradient color</summary>
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color EndColor { get; set; }

        /// <summary>
        /// Gets or sets linear gradient direction</summary>
        [DefaultValue(LinearGradientMode.Vertical)]
        public LinearGradientMode LinearGradientMode { get; set; }

        /// <summary>
        /// Gets or sets text color</summary>
        [DefaultValue(typeof(SystemColors), "ControlText")]
        public Color TextColor { get; set; }
    }
}
