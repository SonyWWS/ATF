//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for states in state-transition diagrams</summary>
    public interface IState : IGraphNode
    {
        /// <summary>
        /// Gets the state type</summary>
        StateType Type
        {
            get;
        }

        /// <summary>
        /// Gets the visual indicators on the state</summary>
        StateIndicators Indicators
        {
            get;
        }
    }
}
