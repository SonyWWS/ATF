//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for an instance of a group template
    /// </summary>
    public class GroupInstance :  Module, ICircuitGroupType<Module, Connection, ICircuitPin>,
                                  IReference<Module>,
                                  IReference<DomNode>,
                                  IReference<Sce.Atf.Controls.Adaptable.Graphs.Group> // for circuit render
    {
        public GroupInstance()
        {
            m_info = new CircuitGroupInfo();
            m_info.PropertyChanged += groupInfoChanged;
        }

        /// <summary>
        /// Gets an adapter of the specified type, or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type, or null</returns>
        public override object GetAdapter(Type type)
        {
            // if type is assignable to group
            if(typeof(Sce.Atf.Controls.Adaptable.Graphs.Group).IsAssignableFrom(type))
                return ProxyGroup;

            return base.GetAdapter(type);
        }

        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
             base.OnNodeSet();
             OnTargetSet(Target);
             Info.ShowExpandedGroupPins = ShowExpandedGroupPins;
        }


        #region IReference memebers
        public bool CanReference(Module item)
        {
            return item.Is<Group>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        public Module Target
        {
            get { return GetReference<Module>(RefAttribute); }
            set
            {
                SetReference(RefAttribute, value);
                OnTargetSet(value);
            }
        }

        bool IReference<Sce.Atf.Controls.Adaptable.Graphs.Group>.CanReference(Sce.Atf.Controls.Adaptable.Graphs.Group item)
        {
            return item.Is<Group>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        Sce.Atf.Controls.Adaptable.Graphs.Group IReference<Sce.Atf.Controls.Adaptable.Graphs.Group>.Target
        {
            get { return GetReference<Sce.Atf.Controls.Adaptable.Graphs.Group>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }

        bool IReference<DomNode>.CanReference(DomNode item)
        {
            return item.Is<Group>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        DomNode IReference<DomNode>.Target
        {
            get { return GetReference<DomNode>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }
        #endregion
        

        public ProxyGroup ProxyGroup
        {
            get
            {
                if (m_proxyGroup == null)
                {
                    m_proxyGroup = new ProxyGroup(this, Target.As<Group>());                   
                    m_proxyGroup.Refresh();
                } 
                return m_proxyGroup;
            }
        }

        public override ICircuitElementType Type
        {
            get
            {
                if (m_group == null)
                    OnTargetSet(Target);
                return Target.Type;
            }
        }

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.groupType.nameAttribute; }
        }

        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        protected override AttributeInfo XAttribute
        {
            get { return Schema.groupType.xAttribute; }
        }

        protected override AttributeInfo YAttribute
        {
            get { return Schema.groupType.yAttribute; }
        }

        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.groupType.visibleAttribute; }
        }

        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {   
            if (pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur
            var result = Target.Cast<Group>().MatchPinTarget(pinTarget, inputSide);
            if (result.First != null)
                result.First = this;
            return result;
        }

        public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            return MatchPinTarget(pinTarget, inputSide);
        }

        private AttributeInfo RefAttribute
        {
            get { return Schema.groupTemplateRefType.typeRefAttribute; }
        }



        IEnumerable<Module> IHierarchicalGraphNode<Module, Connection, ICircuitPin>.SubNodes
        {
            get {  return m_group != null ? m_group.SubNodes : EmptyEnumerable<Module>.Instance;  }
        }

        // ICircuitElementType
        public Size InteriorSize
        {
            get { return Target.Type.InteriorSize; }
        }


        public Image Image
        {
            get { return Target.Type.Image; }
        }

        public IList<ICircuitPin> Inputs
        {
            get { return ProxyGroup.Type.Inputs; }
        }

        public IList<ICircuitPin> Outputs
        {
            get { return ProxyGroup.Type.Outputs; }
        }


        // ICircuitGroupType
        //public bool Expanded
        //{
        //    get { return GetAttribute<bool>(Schema.groupTemplateRefType.refExpandedAttribute); }
        //    set { SetAttribute(Schema.groupTemplateRefType.refExpandedAttribute, value); }
        //}

        /// <summary>
        /// Gets whether the group (subgraph) is expanded</summary>
        /// <remarks>Overrides should call this base property for the setter, so that Changed event is raised.</remarks>
        public virtual bool Expanded
        {
            get { return m_expanded; }
            set
            {
                if (value != m_expanded)
                {
                    m_expanded = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        public bool ShowExpandedGroupPins
        {
            get { return GetAttribute<bool>(Schema.groupTemplateRefType.refShowExpandedGroupPinsAttribute); }
            set { SetAttribute(Schema.groupTemplateRefType.refShowExpandedGroupPinsAttribute, value); }
        }

        public bool AutoSize
        {
            get { return m_group != null ? m_group.AutoSize : false; }
            set { if (m_group != null) m_group.AutoSize = value; }
        }

        IEnumerable<Connection> ICircuitGroupType<Module, Connection, ICircuitPin>.SubEdges
        {
            get { return m_group != null ? m_group.SubEdges : EmptyEnumerable<Connection>.Instance;  }
        }

        public CircuitGroupInfo Info
        {
            get { return m_info; }
        }

        /// <summary>
        /// Event that is raised when this group reference has changed</summary>
        public event EventHandler Changed;

        /// <summary>
        /// Raises the Changed event, so that listeners can be alerted that the state of this group has changed</summary>
        /// <param name="e">Event args</param>
        public virtual void OnChanged(EventArgs e)
        {
            Changed.Raise(this, e);

            foreach (var domNode in DomNode.Ancestry)
            {
                var circuitEditingContext = domNode.As<CircuitEditingContext>();
                if (circuitEditingContext != null)
                    circuitEditingContext.NotifyObjectChanged(this);
            }
        }

        private void Target_AttributeChanged(object sender, AttributeEventArgs e)
        {
            OnChanged(EventArgs.Empty);
        }

        private void Target_ChildInserted(object sender, ChildEventArgs e)
        {

            OnChanged(EventArgs.Empty);
        }

        private void Target_ChildRemoved(object sender, ChildEventArgs e)
        {
            OnChanged(EventArgs.Empty);
          
        }


        private void OnTargetSet(Module target)
        {
            if (target != null)
            {
                m_group = Target.DomNode.As<ICircuitGroupType<Module, Connection, ICircuitPin>>();
                target.DomNode.AttributeChanged += Target_AttributeChanged;
                target.DomNode.ChildInserted += Target_ChildInserted;
                target.DomNode.ChildRemoved += Target_ChildRemoved;
            }
        }

        private void groupInfoChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowExpandedGroupPins = Info.ShowExpandedGroupPins;
        }

        private ICircuitGroupType<Module, Connection, ICircuitPin> m_group;
        private ProxyGroup m_proxyGroup;
        private CircuitGroupInfo m_info;
        private bool m_expanded;
    }
}
