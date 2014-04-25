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
        /// contained within this ICircuitContainer</summary>
        IList<Element> Elements { get; }

        /// <summary>
        /// Gets the modifiable list of wires that are completely contained within this circuit container.
        /// In other words, each wire must connect to two circuit elements that are in Elements.</summary>
        IList<Wire> Wires { get; }

        /// <summary>
        /// Gets the modifiable list of annotations that are owned by this ICircuitContainer</summary>
        IList<Annotation> Annotations { get; }

        /// <summary>
        /// Finds the element and pin that matched the pin target</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide);

        /// <summary>
        /// Finds the element and pin that fully matched the pin target for this circuit container, 
        /// including the template instance node</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide);

        /// <summary>
        /// Gets or sets whether the graph is expanded</summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Gets or sets whether or not the contents of the container have been changed</summary>
        bool Dirty{ get; set; }
        
        /// <summary>
        /// Synchronizes internal data and contents due to editing</summary>
        void Update();
    }
}
