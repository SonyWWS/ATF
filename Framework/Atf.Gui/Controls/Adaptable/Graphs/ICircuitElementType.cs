//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Interface for circuit element types, which define the appearance, inputs,
    /// and outputs of the element. Consider using the Element and Group classes
    /// in the Sce.Atf.Controls.Adaptable.Graphs namespace.</summary>
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
        /// Gets a read-only list of input pins for this element type. For Groups, this list
        /// only includes pins whose Info.Visible property is true. Consider using GetAllInputPins()
        /// or GetInputPin() when using ICircuitGroupPin's InternalPinIndex to look for the
        /// corresponding pin.</summary>
        IList<ICircuitPin> Inputs
        {
            get;
        }

        /// <summary>
        /// Gets a read-only list of output pins for this element type. For Groups, this list
        /// only includes pins whose Info.Visible property is true. Consider using GetAllOutputPins()
        /// or GetOutputPin() when using ICircuitGroupPin's InternalPinIndex to look for the
        /// corresponding pin.</summary>
        IList<ICircuitPin> Outputs
        {
            get;
        }
    }

    /// <summary>
    /// Extension methods for ICircuitElementType</summary>
    public static class CircuitElementTypes
    {
        /// <summary>
        /// Gets all the input pins for this element, including hidden pins (if this is a Group).</summary>
        /// <param name="type">The type</param>
        /// <returns>An enumeration of all the input pins for this type of circuit element</returns>
        public static IEnumerable<ICircuitPin> GetAllInputPins(this ICircuitElementType type)
        {
            var group = type as Group;
            if (group == null)
                return type.Inputs;
            return group.AllInputPins;
        }

        /// <summary>
        /// Gets all the output pins for this element, including hidden pins (if this is a Group).</summary>
        /// <param name="type">The type</param>
        /// <returns>An enumeration of all the output pins for this type of circuit element</returns>
        public static IEnumerable<ICircuitPin> GetAllOutputPins(this ICircuitElementType type)
        {
            var group = type as Group;
            if (group == null)
                return type.Outputs;
            return group.AllOutputPins;
        }

        /// <summary>
        /// Gets the input pin, taking into account whether 'type' is a Group or not.</summary>
        /// <param name="type">The type</param>
        /// <param name="index">The zero-based index</param>
        /// <returns>The input pin whose zero-based index is 'index'.</returns>
        public static ICircuitPin GetInputPin(this ICircuitElementType type, int index)
        {
            var group = type as Group;
            if (group == null)
                return type.Inputs[index];
            return group.InputPin(index);
        }

        /// <summary>
        /// Gets the output pin, taking into account whether 'type' is a Group or not.</summary>
        /// <param name="type">The type</param>
        /// <param name="index">The zero-based index</param>
        /// <returns>The output pin whose zero-based index is 'index'.</returns>
        public static ICircuitPin GetOutputPin(this ICircuitElementType type, int index)
        {
            var group = type as Group;
            if (group == null)
                return type.Outputs[index];
            return group.OutputPin(index);
        }
    }
}
