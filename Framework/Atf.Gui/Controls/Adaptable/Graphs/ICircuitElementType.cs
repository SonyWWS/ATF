//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for circuit element types, which define the appearance, inputs,
    /// and outputs of the element</summary>
    public interface ICircuitElementType
    {
        /// <summary>
        /// Gets the element type name</summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets desired interior size, in pixels, of this element type</summary>
        Size InteriorSize
        {
            get;
        }

        /// <summary>
        /// Gets image to draw for this element type</summary>
        Image Image
        {
            get;
        }

        /// <summary>
        /// Gets the list of input pins for this element type; the list is considered
        /// to be read-only</summary>
        IList<ICircuitPin> Inputs
        {
            get;
        }

        /// <summary>
        /// Gets the list of output pins for this element type; the list is considered
        /// to be read-only</summary>
        IList<ICircuitPin> Outputs
        {
            get;
        }
    }
}
