//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for pins, which are the sources and destinations for
    /// wires between circuit elements</summary>
    public interface ICircuitPin : IEdgeRoute
    {
        /// <summary>
        /// Gets pin name</summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets pin type name</summary>
        string TypeName
        {
            get;
        }

        /// <summary>
        /// Gets index of this pin in the owning ICircuitElementType's input/output list</summary>
        int Index
        {
            get;
        }
    }
}
