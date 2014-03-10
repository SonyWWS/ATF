//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class CircuitEditingContext : Sce.Atf.Controls.Adaptable.Graphs.CircuitEditingContext, 
        IEditableGraphContainer<Module, Connection, ICircuitPin>
    {
        protected override DomNodeType WireType
        {
            get { return Schema.connectionType.Type; }
        }

      

        #region IEditableGraphContainer<Module, Connection, ICircuitPin>
        /// <summary>
        /// Can move the given modules into this container
        /// </summary>
        bool IEditableGraphContainer<Module, Connection, ICircuitPin>.CanMove(object newParent, IEnumerable<object> movingObjects)
        {
            if (newParent.Is<IReference<Module>>())
                return false; 
            var editableGraphContainer =
                DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanMove(newParent, movingObjects);
        }

        void IEditableGraphContainer<Module, Connection, ICircuitPin>.Move(object newParent, IEnumerable<object> movingObjects)
        {
            var editableGraphContainer =
               DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            editableGraphContainer.Move(newParent, movingObjects);
        }

        bool IEditableGraphContainer<Module, Connection, ICircuitPin>.CanResize(object container, DiagramBorder borderPart)
        {
            var editableGraphContainer =
               DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanResize(container, borderPart);
        }

        void IEditableGraphContainer<Module, Connection, ICircuitPin>.Resize(object container, int newWidth, int newHeight)
        {
            var editableGraphContainer =
                 DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            editableGraphContainer.Resize(container, newWidth, newHeight);
        }

        #endregion

        public bool CanConnect(Module fromNode, ICircuitPin fromRoute, Module toNode, ICircuitPin toRoute)
        {
            var editableGraphContainer =
               DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanConnect(fromNode, fromRoute, toNode, toRoute);
        }

        public Connection Connect(Module fromNode, ICircuitPin fromRoute, Module toNode, ICircuitPin toRoute, Connection existingEdge)
        {
            var editableGraphContainer =
            DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.Connect(fromNode, fromRoute, toNode, toRoute, existingEdge).Cast<Connection>();
        }

        public bool CanDisconnect(Connection edge)
        {
            var editableGraphContainer =
              DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            return editableGraphContainer.CanDisconnect(edge);
        }

        public void Disconnect(Connection edge)
        {
            var editableGraphContainer =
                DomNode.Cast<CircuitEditingContext>() as IEditableGraphContainer<Element, Wire, ICircuitPin>;
            editableGraphContainer.Disconnect(edge);
        }
    }
}
