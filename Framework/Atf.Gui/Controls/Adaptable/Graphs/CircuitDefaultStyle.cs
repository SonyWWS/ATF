//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Contains default rendering settings for circuit render</summary>
    public class CircuitDefaultStyle
    {
        /// <summary>
        /// Gets or sets the drawing style of wires</summary>
        public static EdgeStyle EdgeStyle
        {
            get { return s_edgeStyle; }
            set { s_edgeStyle = value; }
        }

        /// <summary>
        /// Gets or sets whether to show the group pins when the group is expanded</summary>
        public static bool ShowExpandedGroupPins
        {
            get { return s_showExpandedGroupPins; }
            set { s_showExpandedGroupPins = value; }
        }

        /// <summary>
        /// Gets or sets whether to show a link between a group pin and its associated internal node
        /// when the group is expanded and the group pin is unconnected. When a group pin is connected to an external
        /// node, then a link (wire) is always shown. If this is false, ShowExpandedGroupPins should be false, too.</summary>
        public static bool ShowVirtualLinks
        {
            get { return s_showVirtualLinks; }
            set { s_showVirtualLinks = value; }
        }

        static private EdgeStyle s_edgeStyle= EdgeStyle.Default;
        static private bool s_showExpandedGroupPins = true;
        static private bool s_showVirtualLinks = true;
  
    }
}
