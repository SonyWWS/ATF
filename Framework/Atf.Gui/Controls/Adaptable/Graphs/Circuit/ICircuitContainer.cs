//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;


namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Used in EditingContext of CircuitEditor to apply editing operations both to Circuit or Group.
    /// A graph container should support elements addition, removal, and insertion(i.e. IList)
    /// </summary>
    public interface ICircuitContainer
    {
        /// <summary>
        /// Gets the modifiable list of circuit elements and circuit group elements (see Group) that are
        /// contained within this ICircuitContainer.</summary>
        IList<Element> Elements { get; }

        /// <summary>
        /// Gets the modifiable list of wires that are completely contained within this circuit container.
        /// In other words, each wire must connect to two circuit elements that are in Elements.</summary>
        IList<Wire> Wires { get; }

        /// <summary>
        /// Gets the modifiable list of annotations that are owned by this ICircuitContainer</summary>
        IList<Annotation> Annotations { get; }

        /// <summary>
        /// Finds the element and pin that match the pin target for this circuit container.</summary>
        /// <param name="pinTarget"></param>
        /// <param name="inputSide"></param>
        /// <returns></returns>
        Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide);

        /// <summary>
        /// Find the element and pin that match the pin target, including the template instance node.</summary>
        Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide);

        /// <summary>
        /// Gets or sets whether the graph is expanded</summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Gets/sets a value indicating whether or not the contents of the container have been changed.</summary>
        bool Dirty{ get; set; }
        
        /// <summary>
        /// Synchronize internal data and contents due to editings</summary>
        void Update();
    }
}
