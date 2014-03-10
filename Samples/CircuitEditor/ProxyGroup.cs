//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Provides a surrogate  that delegates communications with the targeted group on behalf of the group referencing instance. 
    /// </summary>
    public class ProxyGroup: Group, ICircuitGroupType<Module, Connection, ICircuitPin>
    {
        public ProxyGroup(GroupInstance owner, Group target)
        {
            m_owner = owner;
            m_targetGroup = target;
            m_inputs = new List<Sce.Atf.Controls.Adaptable.Graphs.GroupPin>();
            m_outputs = new List<Sce.Atf.Controls.Adaptable.Graphs.GroupPin>();
            m_info = owner.Info;
            m_targetGroup.Changed += new System.EventHandler(targetGroup_Changed);
            var circuitEditingContext = target.Cast<CircuitEditingContext>();
            circuitEditingContext.ItemChanged += circuitEditingContext_ItemChanged;

            // Setting Adaptee will set our DomNode property. Must come after m_info is set.
            var thisAdapter = (IAdapter)this;
            thisAdapter.Adaptee = target.DomNode;
        }

        // For template instances,  dom attribute changed events that lead to ItemChanged in the target group 
        // will bubble up along the templates dom tree that is likely separated from the graph dom tree; 
        // need to reroute target group  ItemChanged from template tree to graph tree.
        void circuitEditingContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {

            foreach (var domNode in m_owner.DomNode.Ancestry)
            {
                var circuitEditingContext = domNode.As<CircuitEditingContext>();
                if (circuitEditingContext != null)
                    circuitEditingContext.NotifyObjectChanged(e.Item);
            }
        }

        public override ICircuitElementType Type
        {
            get { return this; }
        }

        /// <summary>
        /// Gets or sets CircuitGroupInfo object which controls various options on this circuit group.</summary>
        public override CircuitGroupInfo Info
        {
            get { return m_info; }
        }

        public override IGraph<Element, Wire, ICircuitPin> ParentGraph
        {
            get { return m_owner.DomNode.Parent.As<IGraph<Element, Wire, ICircuitPin>>(); }
        }

        Rectangle IGraphNode.Bounds
        {
            get { return m_owner.Bounds; }
        }

        public override Rectangle Bounds
        {
            get { return m_owner.Bounds; }
            set { m_owner.Bounds = value; }
        }

        // ICircuitElementType
        public Size InteriorSize
        {
            get { return m_targetGroup.Type.InteriorSize; }
        }

        public Image Image
        {
            get { return m_targetGroup.Type.Image; }
        }

        public override IList<ICircuitPin> Inputs
        {
            get
            {
                var inputs = m_inputs.OrderBy(n => n.Index).Where(n => n.Visible).ToArray();
                return inputs;
            }
        }

        public override IList<ICircuitPin> Outputs
        {
            get { return m_outputs.OrderBy(n => n.Index).Where(n => n.Visible).ToArray(); }
        }

        // ICircuitGroupType
        public override bool Expanded
        {
            get { return m_owner.Expanded; }
            set { m_owner.Expanded= value; }
        }

        public override bool ShowExpandedGroupPins
        {
            get { return m_owner.ShowExpandedGroupPins; }
            set
            {
                m_owner.ShowExpandedGroupPins = value;
                Info.ShowExpandedGroupPins = value;
            }
        }

        public override bool AutoSize
        {
            get { return m_targetGroup.AutoSize; }
            set { m_targetGroup.AutoSize  = value; }
        }

        protected override void GroupInfoChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowExpandedGroupPins = Info.ShowExpandedGroupPins;
        }

        IEnumerable<Connection> ICircuitGroupType<Module, Connection, ICircuitPin>.SubEdges
        {
            get
            {
                var group = m_targetGroup.DomNode.As<ICircuitGroupType<Module, Connection, ICircuitPin>>();
                return group.SubEdges;
            }
        }

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

        public void Refresh()
        {
            m_inputs.Clear(); 
            m_outputs.Clear();

            var templateInputPins = m_targetGroup.InputGroupPins.ToArray();
            var templateOutputPins = m_targetGroup.OutputGroupPins.ToArray();
            var grpPinCopies = DomNode.Copy(templateInputPins.AsIEnumerable<DomNode>());

            for(int i=0; i< grpPinCopies.Length; ++i  )
            {
                var grpPinNode = grpPinCopies[i];
                var grpin = grpPinNode.Cast<Sce.Atf.Controls.Adaptable.Graphs.GroupPin>();
                grpin.SetPinTarget(true);
                grpin.PinTarget.InstancingNode = m_owner.DomNode;
                // share the template group pin info
                grpin.Info = templateInputPins[i].Info;
                m_inputs.Add(grpin);
            }

            var grpPinCopies2 = DomNode.Copy(templateOutputPins.AsIEnumerable<DomNode>());
            for (int i = 0; i < grpPinCopies2.Length; ++i)
            {
                var grpPinNode = grpPinCopies2[i];
                var grpin = grpPinNode.Cast<Sce.Atf.Controls.Adaptable.Graphs.GroupPin>();
                grpin.SetPinTarget(false);             
                grpin.PinTarget.InstancingNode = m_owner.DomNode;
                // share the template group pin info
                grpin.Info = templateOutputPins[i].Info;
                m_outputs.Add(grpin);
            }
        }

        /// <summary>
        /// Gets a list of the input pins in this group</summary>
        public override IEnumerable<Sce.Atf.Controls.Adaptable.Graphs.GroupPin> InputGroupPins
        {
            get { return m_inputs;}
        }

        /// <summary>
        /// Gets a list of the output pins in this group</summary>
        public override IEnumerable<Sce.Atf.Controls.Adaptable.Graphs.GroupPin> OutputGroupPins
        {
            get{ return m_outputs;}
        }

        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            if (pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur
            var result = m_targetGroup.Cast<Group>().MatchPinTarget(pinTarget, inputSide);
            if (result.First != null)
                result.First = this;
            return result;
        }

        void targetGroup_Changed(object sender, System.EventArgs e)
        {
            m_info.Offset = m_targetGroup.Info.Offset;
            Refresh();
        }

        private Group m_targetGroup;
        private CircuitGroupInfo m_info;
        private GroupInstance m_owner;
        private List<Sce.Atf.Controls.Adaptable.Graphs.GroupPin> m_inputs;
        private List<Sce.Atf.Controls.Adaptable.Graphs.GroupPin> m_outputs;
    }
}
