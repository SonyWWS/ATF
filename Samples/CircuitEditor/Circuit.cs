//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class Circuit : Sce.Atf.Controls.Adaptable.Graphs.Circuit, IGraph<Module, Connection, ICircuitPin>
#if !CS_4
        , IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute>
#endif
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode</summary>
        protected override void OnNodeSet()
        {
            // cache these list wrapper objects
            m_modules = new DomNodeListAdapter<Module>(DomNode, Schema.circuitType.moduleChild);
            m_connections = new DomNodeListAdapter<Connection>(DomNode, Schema.circuitType.connectionChild);
            m_annotations = new DomNodeListAdapter<Annotation>(DomNode, Schema.circuitType.annotationChild);
            base.OnNodeSet();
        }

        protected override ChildInfo ElementChildInfo
        {
            get { return Schema.circuitType.moduleChild; }
        }

        protected override ChildInfo WireChildInfo
        {
            get { return Schema.circuitType.connectionChild; }
        }

        // optional child info
        protected override ChildInfo AnnotationChildInfo
        {
            get { return Schema.circuitType.annotationChild; }
        }

        #region IGraph Members

        /// <summary>
        /// Gets all visible nodes in the circuit</summary>
        ///<remarks>IGraph.Nodes is called during circuit rendering, and picking(in reverse order). </remarks>
        IEnumerable<Module> IGraph<Module, Connection, ICircuitPin>.Nodes
        {
            get
            {
                return m_modules.Where(x => x.Visible);
            }
        }

        /// <summary>
        /// Gets all connections between visible nodes in the circuit</summary>
        IEnumerable<Connection> IGraph<Module, Connection, ICircuitPin>.Edges
        {
            get
            {
                  return m_connections.Where(x => x.InputElement.Visible && x.OutputElement.Visible);
            }
        }

        // In VS2008, without C# 4's covariance and contravariance, we have to manually implement variations of
        //  interfaces that use the 'out' or 'in' keywords on the generic type parameters.
#if !CS_4
        IEnumerable<IGraphEdge<IGraphNode, IEdgeRoute>> IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute>.Edges
        {
            get { return ((IGraph<Module, Connection, ICircuitPin>) this).Edges.Cast<IGraphEdge<IGraphNode, IEdgeRoute>>(); }
        }

        IEnumerable<IGraphNode> IGraph<IGraphNode, IGraphEdge<IGraphNode, IEdgeRoute>, IEdgeRoute>.Nodes
        {
            get { return ((IGraph<Module, Connection, ICircuitPin>)this).Nodes.Cast<IGraphNode>(); }
        }
#endif

        #endregion
       

        private DomNodeListAdapter<Module> m_modules;
        private DomNodeListAdapter<Connection> m_connections;
        private DomNodeListAdapter<Annotation> m_annotations;
    }
}
