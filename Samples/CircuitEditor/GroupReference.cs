//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class GroupReference : Sce.Atf.Controls.Adaptable.Graphs.GroupReference,
         ICircuitGroupType<Module, Connection, ICircuitPin>, // for circuit render
         IReference<Module>
    {
        #region Fill required AttributeInfos 
        /// <summary>
        /// Gets name attribute for group instance</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.groupType.nameAttribute; }
        }

        /// <summary>
        /// Gets label attribute for group instance</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets x-coordinate position attribute for group instance</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.groupType.xAttribute; }
        }

        /// <summary>
        /// Gets y-coordinate position attribute for group instance</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.groupType.yAttribute; }
        }

        /// <summary>
        /// Gets visibility attribute for group instance</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.groupType.visibleAttribute; }
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

        protected override AttributeInfo ShowExpandedGroupPinsAttribute
        {
            get { return Schema.groupTemplateRefType.refShowExpandedGroupPinsAttribute; }
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


        protected override AttributeInfo GuidRefAttribute
        {
            get { return Schema.groupTemplateRefType.guidRefAttribute; }
        }

        #endregion

        #region ICircuitGroupType members

        IEnumerable<Module> IHierarchicalGraphNode<Module, Connection, ICircuitPin>.SubNodes
        {
            get
            {
                if (Group != null)
                    return Group.Elements.AsIEnumerable<Module>();
                return EmptyEnumerable<Module>.Instance;
            }
        }

        /// <summary>
        /// Gets the group's (subgraph's) internal edges</summary>
        IEnumerable<Connection> ICircuitGroupType<Module, Connection, ICircuitPin>.SubEdges
        {
            get
            {
                if (Group != null)
                    return Group.Wires.AsIEnumerable<Connection>();
                return EmptyEnumerable<Connection>.Instance;
            }
        }

        #endregion

        #region IReference<Module> members

        bool IReference<Module>.CanReference(Module item)
        {
            return item.Is<Group>();
        }

        Module IReference<Module>.Target
        {
            get { return Template.Target.As<Module>(); }
            set
            {
                throw new InvalidOperationException("The group template determines the target");
            }
        }
        #endregion

    }
}
