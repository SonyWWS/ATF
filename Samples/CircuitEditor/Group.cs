//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class Group : Sce.Atf.Controls.Adaptable.Graphs.Group, ICircuitGroupType<Module, Connection, ICircuitPin>, IGraph<Module, Connection, ICircuitPin>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the group's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            m_modules = new DomNodeListAdapter<Module>(DomNode, Schema.groupType.moduleChild);
            m_connections = new DomNodeListAdapter<Connection>(DomNode, Schema.groupType.connectionChild);
            m_annotations = new DomNodeListAdapter<Annotation>(DomNode, Schema.groupType.annotationChild);
            m_inputs = new DomNodeListAdapter<GroupPin>(DomNode, Schema.groupType.inputChild);
            m_outputs = new DomNodeListAdapter<GroupPin>(DomNode, Schema.groupType.outputChild);
            m_thisModule = DomNode.Cast<Module>();

            base.OnNodeSet();
        }

        // Note CircuitEditorSample.Group does not drive from Module due to single inheritance in C#,
        // but groupType derives from moduleType in xml schema. Now it is possible that the underneath DomNode
        // is adapted to both Module & CircuitGroup, which can bring in two Element.Bounds: one from Module->Element,
        // another from Group->Element. Explicitly use bounds from Module for graph drawing.
        Rectangle IGraphNode.Bounds
        {
            get { return m_thisModule.Bounds; }
        }

        public override Rectangle Bounds
        {
            get { return m_thisModule.Bounds; }
            set { m_thisModule.Bounds = value; }
        }

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        protected override AttributeInfo MinWidthAttribute
        {
            get { return Schema.groupType.minwidthAttribute; }
        }

        protected override AttributeInfo MinHeightAttribute
        {
            get { return Schema.groupType.minheightAttribute; }
        }

        protected override AttributeInfo WidthAttribute
        {
            get { return Schema.groupType.widthAttribute; }
        }

        protected override AttributeInfo HeightAttribute
        {
            get { return Schema.groupType.heightAttribute; }
        }

        protected override AttributeInfo AutosizeAttribute
        {
            get { return Schema.groupType.autosizeAttribute; }
        }

        protected override AttributeInfo ExpandedAttribute
        {
            get { return Schema.groupType.expandedAttribute; }
        }

        protected override AttributeInfo ShowExpandedGroupPinsAttribute
        {
            get { return Schema.groupType.showExpandedGroupPinsAttribute; }
        }

        protected override ChildInfo ElementChildInfo
        {
            get { return Schema.groupType.moduleChild; }
        }

        protected override ChildInfo WireChildInfo
        {
            get { return Schema.groupType.connectionChild; }
        }

        protected override ChildInfo InputChildInfo
        {
            get { return Schema.groupType.inputChild; }
        }

        protected override ChildInfo OutputChildInfo
        {
            get { return Schema.groupType.outputChild; }
        }

        protected override DomNodeType GroupPinType
        {
            get { return Schema.groupPinType.Type; }
        }

        // optional child info
        protected override ChildInfo AnnotationChildInfo
        {
            get { return Schema.groupType.annotationChild; }
        }

        #region IHierarchicalGraphNode and ICircuitGroupType Members

        /// <summary>
        /// Gets the sequence of nodes that are children of this group (hierarchical graph node)</summary>
        IEnumerable<Module> IHierarchicalGraphNode<Module, Connection, ICircuitPin>.SubNodes
        {
            get
            {
                var graph = (IGraph<Module, Connection, ICircuitPin>)this;
                return graph.Nodes;
            }
        }

        /// <summary>
        /// Gets the group's (subgraph's) internal edges</summary>
        IEnumerable<Connection> ICircuitGroupType<Module, Connection, ICircuitPin>.SubEdges
        {
            get { return m_connections; }
        }

    


        #endregion

        #region IGraph<Module,Connection,ICircuitPin> Members

        /// <summary>
        /// Gets the nodes in the group</summary>
        IEnumerable<Module> IGraph<Module, Connection, ICircuitPin>.Nodes
        {
            get { return m_modules; }
        }

        /// <summary>
        /// Gets the edges in the group</summary>
        IEnumerable<Connection> IGraph<Module, Connection, ICircuitPin>.Edges
        {
            get
            {
                return m_connections;
            }
        }

        #endregion

        private DomNodeListAdapter<Module> m_modules;
        private DomNodeListAdapter<Connection> m_connections;
        private Module m_thisModule;
        private DomNodeListAdapter<Annotation> m_annotations;
        private DomNodeListAdapter<GroupPin> m_inputs;
        private DomNodeListAdapter<GroupPin> m_outputs;
    }
}
