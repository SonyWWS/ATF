//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    public class CircuitUtil
    {

        // returns a list of all modules, internal connections between them, and connections
        //  to external modules
        public static void GetSubGraph(
            ICircuitContainer graphContainer,
            IEnumerable<object> objects, // [in] selected objects, edges possible
            HashSet<Element> modules, // [out] elements in the selected objects
            ICollection<Wire> internalConnections,
            ICollection<Wire> incomingConnections,
            ICollection<Wire> outgoingConnections)
        {
            // build the set of modules, and add them to result
            foreach (var module in objects.AsIEnumerable<Element>())
                modules.Add(module);

            // add connections to modules
            foreach (var connection in graphContainer.Wires)
            {
                bool output = modules.Contains(connection.OutputElement);
                bool input = modules.Contains(connection.InputElement);
                if (output && input)
                {
                    internalConnections.Add(connection);
                }
                else if (output)
                {
                    outgoingConnections.Add(connection);
                }
                else if (input)
                {
                    incomingConnections.Add(connection);
                }
            }
        }

        /// <summary>
        /// Debugging Helper method
        /// </summary>
        /// <param name="domNode"></param>
        /// <returns></returns>
        static public string GetDomNodeName(DomNode domNode)
        {
            string result = string.Empty;

            if (domNode.Is<Element>())
                result = domNode.Cast<Element>().Name;
            else if (domNode.Is<GroupPin>())
                result = "Group Pin : " + domNode.Cast<GroupPin>().Name;
            else if (domNode.Is<Wire>())
            {
                var connection = domNode.Cast<Wire>();
                int inputPinIndex, outputPinIndex;
                // during undo/redo, the pin index may temporarily out of range, need to check index before call OutputPin|InputPin
                if (connection.IsValid(out inputPinIndex, out outputPinIndex))
                    result = "Edge from " + connection.OutputElement.Name + "[" + connection.OutputPin.Name + "]" +
                    " to " + connection.InputElement.Name + "[" + connection.InputPin.Name + "]";
                else
                    result = "Edge from " + connection.OutputElement.Name + "[" + outputPinIndex + "]" +
                       " to " + connection.InputElement.Name + "[" + inputPinIndex + "]";

            }
            else if (domNode.Is<Circuit>())
            {
                var doc = domNode.As<IDocument>();
                if (doc != null && doc.Uri != null)
                    result = "Circuit " + Path.GetFileNameWithoutExtension(doc.Uri.LocalPath);
                else
                    result = "Circuit " + domNode.GetId();
            }
            if (result == string.Empty)
                result = domNode.GetId() ?? domNode.ToString();
            return result;
        }

        static public IGraph<Element, Wire, ICircuitPin> EmptyCircuit
        {
            get { return s_emptyGraph; }
        }

        /// <summary>
        /// Gets a value indicating whether the object is a group template instance.
        /// Returns:true if it is a template instance; otherwise, false.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        static public bool IsGroupTemplateInstance(object node)
        {
            return node.Is<IReference<Group>>();
        }

        ///<summary>Gets the graph path of the specified group.</summary>
        static public string GetGroupPath(Group group)
        {
            var sb = new StringBuilder(); 
            foreach (var domNode in group.DomNode.Lineage.Reverse())
            {
                if (domNode.Is<IDocument>())
                    sb.Append(Path.GetFileNameWithoutExtension(domNode.Cast<IDocument>().Uri.LocalPath));
                else if (domNode.Is<Element>())
                    sb.Append(domNode.Cast<Element>().Name);
                sb.Append(":");      
            }
            if (sb.Length >0)
                sb.Length -= 1; // remove trailing seperator         
            return sb.ToString();
        }

        /// <summary>
        /// Get the group template the instance references.</summary>
        /// <param name="templateInstance"></param>
        /// <returns></returns>
        static public Group GetGroupTemplate(object templateInstance)
        {
            var instance = templateInstance.Cast<IReference<Group>>();
            return instance.Target;
        }

        /// <summary>
        /// The last item of hitPath is the picked item</summary> 
        static public TEdgeRoute EdgeRouteTraverser<TEdgeRoute>(AdaptablePath<object> hitPath, object destNode, TEdgeRoute startRoute)
             where TEdgeRoute : class, ICircuitPin
        {
            if (!hitPath.Last.Is<Element>())
                return null;
            int fromIndex = hitPath.Count - 1; //start from the hit sub-item
           
            int toIndex = hitPath.IndexOf(destNode);
            if (toIndex < 0 || toIndex > fromIndex)
                return null;


            var circuitPin = startRoute;
            var currentElement = hitPath.Last;
            for (int i = fromIndex - 1; i >= toIndex; --i)
            {
                ICircuitPin matchedPin = null;
                var parent = hitPath[i];
                if (parent.Is<Group>())
                {
                    var group = parent.Cast<Group>();
                    foreach (var grpPin in group.InputGroupPins)
                    {
                        if (grpPin.InternalElement.Equals(currentElement) &&
                            grpPin.InternalElement.Type.Inputs[grpPin.InternalPinIndex] == circuitPin)
                        {
                            matchedPin = grpPin;
                            break;
                        }
                    }
                    if (matchedPin == null)
                    {
                        foreach (var grpPin in group.OutputGroupPins)
                        {
                            if (grpPin.InternalElement.Equals(currentElement) &&
                                grpPin.InternalElement.Type.Outputs[grpPin.InternalPinIndex] == circuitPin)
                            {
                                matchedPin = grpPin;
                            }
                        }
                    }
                }
                if (matchedPin == null)
                    return null;
                circuitPin = matchedPin.Cast<TEdgeRoute>();
                currentElement = parent;
            }
            return circuitPin;
        }

        // Empty graph to simplify code when there is no graph
        private class EmptyGraph : IGraph<Element, Wire, ICircuitPin>
        {
            public IEnumerable<Element> Nodes
            {
                get { return EmptyEnumerable<Element>.Instance; }
            }

            public IEnumerable<Wire> Edges
            {
                get { return EmptyEnumerable<Wire>.Instance; }
            }
        }

        private static EmptyGraph s_emptyGraph = new EmptyGraph();
  
    }
}
