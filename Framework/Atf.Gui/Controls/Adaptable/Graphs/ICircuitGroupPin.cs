//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for group pins, which are virtual pins that expose real pins of elements of the
    /// group to other circuit elements outside the group</summary>
    /// <typeparam name="TElement">ICircuitElement element</typeparam>
    public interface ICircuitGroupPin<out TElement>: ICircuitPin
         where TElement : class, ICircuitElement
    {
        /// <summary>
        /// Gets the circuit element within this group that this group pin connects to</summary>
        TElement InternalElement
        {
            get;
        }
        
        /// <summary>
        /// Gets the index of the pin on InternalElement that this group pin connects to</summary>
        int InternalPinIndex
        {
            get;
        }

        /// <summary>
        /// Gets the bounding rectangle for the group pin in local space</summary>
        /// <remarks>Currently only y coordinate is used for drawing; x coordinate is auto-placed.</remarks>
        Rectangle Bounds
        {
            get;
        }

        /// <summary>
        /// Gets the CircuitGroupPinInfo object which controls various options on this circuit group pin</summary>
        CircuitGroupPinInfo Info
        {
            get;
        }
    }
}
