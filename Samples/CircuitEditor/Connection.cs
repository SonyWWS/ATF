//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for connections in a circuit</summary>
    public class Connection : Wire, IGraphEdge<Module, ICircuitPin>
#if !CS_4
        , IGraphEdge<IGraphNode, IEdgeRoute>
#endif
    {
       
        #region IGraphEdge Members

        /// <summary>
        /// Gets edge's source node</summary>
        Module IGraphEdge<Module>.FromNode
        {
            get { return OutputElement.Cast<Module>(); }
        }

        /// <summary>
        /// Gets the route taken from the source node</summary>
        ICircuitPin IGraphEdge<Module, ICircuitPin>.FromRoute
        {
            get { return OutputPin; }
        }

        /// <summary>
        /// Gets edge's destination node</summary>
        Module IGraphEdge<Module>.ToNode
        {
            get { return InputElement.Cast<Module>(); }
        }

        /// <summary>
        /// Gets the route taken to the destination node</summary>
        ICircuitPin IGraphEdge<Module, ICircuitPin>.ToRoute
        {
            get { return InputPin; }
        }

        /// <summary>
        /// Gets label on connection</summary>
        string IGraphEdge<Module>.Label
        {
            get { return Label; }
        }

        // In VS2008 projects, we need to be able to cast Connection to IGraphEdge<IGraphNode, IEdgeRoute>.
        // In VS2010 and later, with C# 4, the 'out' keyword is used in IGraphEdge.
        #if !CS_4
        /// <summary>
        /// Gets edge's destination node</summary>
        IGraphNode IGraphEdge<IGraphNode>.ToNode
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).ToNode; }
        }
        /// <summary>
        /// Gets the route taken to the destination node</summary>
        IEdgeRoute IGraphEdge<IGraphNode, IEdgeRoute>.ToRoute
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).ToRoute; }
        }
        /// <summary>
        /// Gets edge's source node</summary>
        IGraphNode IGraphEdge<IGraphNode>.FromNode
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).FromNode; }
        }
        /// <summary>
        /// Gets the route taken from the source node</summary>
        IEdgeRoute IGraphEdge<IGraphNode, IEdgeRoute>.FromRoute
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).FromRoute; }
        }
        /// <summary>
        /// Gets label on connection</summary>
        string IGraphEdge<IGraphNode>.Label
        {
            get { return ((IGraphEdge<Module>)this).Label; }
        }
        #endif
        #endregion

        /// <summary>
        /// Gets label attribute on connection</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.connectionType.labelAttribute; }
        }

        /// <summary>
        /// Gets input module attribute for connection</summary>
        protected override AttributeInfo InputElementAttribute
        {
            get { return Schema.connectionType.inputModuleAttribute; }
        }

        /// <summary>
        /// Gets output module attribute for connection</summary>
        protected override AttributeInfo OutputElementAttribute
        {
            get { return Schema.connectionType.outputModuleAttribute; }
        }

        /// <summary>
        /// Gets input pin attribute for connection</summary>
        protected override AttributeInfo InputPinAttribute
        {
            get { return Schema.connectionType.inputPinAttribute; }
        }

        /// <summary>
        /// Gets output pin attribute for connection</summary>
        protected override AttributeInfo OutputPinAttribute
        {
            get { return Schema.connectionType.outputPinAttribute; }
        }

    }
}
