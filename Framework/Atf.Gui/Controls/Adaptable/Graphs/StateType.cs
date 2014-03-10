//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// State types for state-transition diagrams</summary>
    public enum StateType
    {
        /// <summary>
        /// A normal state</summary>
        Normal,

        /// <summary>
        /// The start pseudostate</summary>
        Start,

        /// <summary>
        /// The final pseudostate</summary>
        Final,

        /// <summary>
        /// The shallow history pseudostate</summary>
        ShallowHistory,

        /// <summary>
        /// The deep history pseudostate</summary>
        DeepHistory,

        /// <summary>
        /// A conditional pseudostate</summary>
        Conditional,
    }
}
