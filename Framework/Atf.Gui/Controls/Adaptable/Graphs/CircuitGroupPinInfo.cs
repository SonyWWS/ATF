//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Contains options for specifying the behavior or appearance of a ICircuitGroupPin.</summary>
    public class CircuitGroupPinInfo
    {

        static CircuitGroupPinInfo()
        {
            FloatingPinNodeHeight = 28;      // default floating pin total height(including the bottom label part)
            FloatingPinNodeMargin = 2;       // default floating pin margin in the same element (minimum vertical space between the pins)
            FloatingPinElementMargin = 28;   // default floating pin margin between different elements(minimum vertical space between the elements)

            FloatingPinBoxHeight = 15;       // default floating pin box height(excluding the bottom label part)
            FloatingPinBoxWidth = 22;        // default floating pin box width 
        }
        /// <summary>
        /// Gets or sets whether the group pin is 'pinned'. When pinned, the pin will not be automatically
        /// relocated when the internal element that the pin is connected to is relocated.</summary>
        public bool Pinned
        {
            get { return m_pinned; }
            set
            {
                if (m_pinned != value)
                {
                    m_pinned = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group pin is rendered in the graph view.</summary>
        public bool Visible
        {
            get { return m_visible; }
            set
            {
                if (m_visible != value)
                {
                    m_visible = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the group pin is connected to an external wire.</summary>
        public bool ExternalConnected
        {
            get { return m_externalConnected; }
            set
            {
                if (m_externalConnected != value)
                {
                    m_externalConnected = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the group pin color when expanded</summary>
        public Color Color
        {
            get { return m_color; }
            set { m_color = value; }
        }
                
        // group pin can be displayed as a floating pin (virtual) node in the group pin editor

        /// <summary>
        /// The total height of floating group pin box( including the label part at the bottom)</summary>
        public static int FloatingPinNodeHeight { get; set; }

        /// <summary>
        /// floating pin margin of the same element</summary>
        public static int FloatingPinNodeMargin { get; set; }

        /// <summary>
        /// floating pin margin between different elements</summary>
        public static int FloatingPinElementMargin { get; set; }

        /// <summary>
        /// The height of floating group pin box ( shown along the expanded group edge)</summary>
        public static int FloatingPinBoxHeight { get; set; }

        /// <summary>
        /// The width of floating group pin box ( shown along the expanded group edge)</summary>
        public static int FloatingPinBoxWidth { get; set; }


        /// <summary>
        /// Event handler that is called when this info object changes</summary>
        public event EventHandler Changed;

        private Color m_color = Color.SandyBrown;
        private bool m_pinned;
        private bool m_visible;
        private bool m_externalConnected;
    }
}
