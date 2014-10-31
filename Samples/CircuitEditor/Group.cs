//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to group in a circuit</summary>
    public class Group : Sce.Atf.Controls.Adaptable.Graphs.Group, ICircuitGroupType<Module, Connection, ICircuitPin>, IGraph<Module, Connection, ICircuitPin>
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the group's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing:
        /// creates a DomNodeListAdapter for various circuit elements.</summary>
        protected override void OnNodeSet()
        {
            m_modules = new DomNodeListAdapter<Module>(DomNode, Schema.groupType.moduleChild);
            m_connections = new DomNodeListAdapter<Connection>(DomNode, Schema.groupType.connectionChild);
            new DomNodeListAdapter<Annotation>(DomNode, Schema.groupType.annotationChild);
            new DomNodeListAdapter<GroupPin>(DomNode, Schema.groupType.inputChild);
            new DomNodeListAdapter<GroupPin>(DomNode, Schema.groupType.outputChild);
            m_thisModule = DomNode.Cast<Module>();

            base.OnNodeSet();
        }

        /// <summary>
        /// Gets the bounding rectangle for the node in world space (or local space if a
        /// hierarchy is involved, as with sub-circuits). The location portion should always
        /// be accurate, but the renderer should be queried for the size of the rectangle.
        /// See D2dGraphRenderer.GetBounds() or use ILayoutContext.GetBounds().</summary>
        /// <remarks>Note: CircuitEditorSample.Group does not drive from Module due to single inheritance in C#,
        /// but groupType derives from moduleType in XML schema. Now it is possible that the DomNode underneath
        /// is adapted to both Module and CircuitGroup, which can bring in two Element.Bounds: one from Module->Element,
        /// another from Group->Element. Explicitly use bounds from Module for graph drawing.</remarks>
        Rectangle IGraphNode.Bounds
        {
            get { return m_thisModule.Bounds; }
        }

        /// <summary>
        /// Gets or sets the bounding rectangle for the node in world space (or local space if a
        /// hierarchy is involved, as with sub-circuits). The location portion should always
        /// be accurate, but the renderer should be queried for the size of the rectangle.
        /// See D2dGraphRenderer.GetBounds() or use ILayoutContext.GetBounds().</summary>
        public override Rectangle Bounds
        {
            get { return m_thisModule.Bounds; }
            set { m_thisModule.Bounds = value; }
        }

        /// <summary>
        /// Gets name attribute for group</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        /// <summary>
        /// Gets label attribute on group</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets x-coordinate position attribute for group</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        /// <summary>
        /// Gets y-coordinate position attribute for group</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        /// <summary>
        /// Gets visible attribute for group</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        /// <summary>
        /// Gets minimum width (when the group is expanded) attribute for group</summary>
        protected override AttributeInfo MinWidthAttribute
        {
            get { return Schema.groupType.minwidthAttribute; }
        }

        /// <summary>
        /// Gets minimum height (when the group is expanded) attribute for group</summary>
        protected override AttributeInfo MinHeightAttribute
        {
            get { return Schema.groupType.minheightAttribute; }
        }

        /// <summary>
        /// Gets width (when the group is expanded) attribute for group</summary>
        protected override AttributeInfo WidthAttribute
        {
            get { return Schema.groupType.widthAttribute; }
        }

        /// <summary>
        /// Gets height (when the group is expanded) attribute for group</summary>
        protected override AttributeInfo HeightAttribute
        {
            get { return Schema.groupType.heightAttribute; }
        }

        /// <summary>
        /// Gets autosize attribute for group.
        /// When autosize is true, container size is computed.</summary>
        protected override AttributeInfo AutosizeAttribute
        {
            get { return Schema.groupType.autosizeAttribute; }
        }

        /// <summary>
        /// Gets expanded attribute for group</summary>
        protected override AttributeInfo ExpandedAttribute
        {
            get { return Schema.groupType.expandedAttribute; }
        }

        /// <summary>
        /// Gets showExpandedGroupPins attribute for group</summary>
        protected override AttributeInfo ShowExpandedGroupPinsAttribute
        {
            get { return Schema.groupType.showExpandedGroupPinsAttribute; }
        }

        /// <summary>
        /// Gets ChildInfo for Modules in group</summary>
        protected override ChildInfo ElementChildInfo
        {
            get { return Schema.groupType.moduleChild; }
        }

        /// <summary>
        /// Gets ChildInfo for Wires in group</summary>
        protected override ChildInfo WireChildInfo
        {
            get { return Schema.groupType.connectionChild; }
        }

        /// <summary>
        /// Gets ChildInfo for input group pins in group</summary>
        protected override ChildInfo InputChildInfo
        {
            get { return Schema.groupType.inputChild; }
        }

        /// <summary>
        /// Gets ChildInfo for output group pins in group</summary>
        protected override ChildInfo OutputChildInfo
        {
            get { return Schema.groupType.outputChild; }
        }

        /// <summary>
        /// Gets group pin type.
        /// A group pin is a pin on a grouped sub-circuit; it extends the information of a pin
        /// to preserve the internal pin/module which is connected to the outside circuit.</summary>
        protected override DomNodeType GroupPinType
        {
            get { return Schema.groupPinType.Type; }
        }

        // optional child info
        /// <summary>
        /// Gets ChildInfo for annotations (comments) in group.
        /// Return null if annotations are not supported.</summary>
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
    }
}
