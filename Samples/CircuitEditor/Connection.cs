//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for connections in a circuit
    /// </summary>
    public class Connection : Wire, IGraphEdge<Module, ICircuitPin>
#if !CS_4
        , IGraphEdge<IGraphNode, IEdgeRoute>
#endif
    {
       
        #region IGraphEdge Members

        Module IGraphEdge<Module>.FromNode
        {
            get { return OutputElement.Cast<Module>(); }
        }

        ICircuitPin IGraphEdge<Module, ICircuitPin>.FromRoute
        {
            get { return OutputPin; }
        }

        Module IGraphEdge<Module>.ToNode
        {
            get { return InputElement.Cast<Module>(); }
        }

        ICircuitPin IGraphEdge<Module, ICircuitPin>.ToRoute
        {
            get { return InputPin; }
        }

        string IGraphEdge<Module>.Label
        {
            get { return Label; }
        }

        // In VS2008 projects, we need to be able to cast Connection to IGraphEdge<IGraphNode, IEdgeRoute>.
        // In VS2010 and later, with C# 4, the 'out' keyword is used in IGraphEdge.
        #if !CS_4
        IGraphNode IGraphEdge<IGraphNode>.ToNode
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).ToNode; }
        }
        IEdgeRoute IGraphEdge<IGraphNode, IEdgeRoute>.ToRoute
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).ToRoute; }
        }
        IGraphNode IGraphEdge<IGraphNode>.FromNode
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).FromNode; }
        }
        IEdgeRoute IGraphEdge<IGraphNode, IEdgeRoute>.FromRoute
        {
            get { return ((IGraphEdge<Module, ICircuitPin>)this).FromRoute; }
        }
        string IGraphEdge<IGraphNode>.Label
        {
            get { return ((IGraphEdge<Module>)this).Label; }
        }
        #endif
        #endregion

        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.connectionType.labelAttribute; }
        }

        protected override AttributeInfo InputElementAttribute
        {
            get { return Schema.connectionType.inputModuleAttribute; }
        }

        protected override AttributeInfo OutputElementAttribute
        {
            get { return Schema.connectionType.outputModuleAttribute; }
        }

        protected override AttributeInfo InputPinAttribute
        {
            get { return Schema.connectionType.inputPinAttribute; }
        }

        protected override AttributeInfo OutputPinAttribute
        {
            get { return Schema.connectionType.outputPinAttribute; }
        }

    }
}
