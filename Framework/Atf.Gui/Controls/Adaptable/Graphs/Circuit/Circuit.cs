//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
#if DEBUG_VERBOSE
using System.Diagnostics;
#endif

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to a circuit and observable context with change notification events</summary>
    public abstract class Circuit : DomNodeAdapter, IGraph<Element, Wire, ICircuitPin>, IAnnotatedDiagram, ICircuitContainer
    {

        // required  child info
        protected abstract ChildInfo ElementChildInfo { get; }
        protected abstract ChildInfo WireChildInfo { get; }

        // optional child info
        protected virtual ChildInfo AnnotationChildInfo
        {
            get { return null; }
        }
      
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode</summary>
        protected override void OnNodeSet()
        {
            // cache these list wrapper objects
            m_elements = new DomNodeListAdapter<Element>(DomNode, ElementChildInfo);
            m_wires = new DomNodeListAdapter<Wire>(DomNode, WireChildInfo);
            if (AnnotationChildInfo != null)
                m_annotations = new DomNodeListAdapter<Annotation>(DomNode, AnnotationChildInfo);
 
            foreach (var connection in Wires)
                connection.SetPinTarget();

            DomNode.AttributeChanged += new EventHandler<AttributeEventArgs>(DomNode_AttributeChanged);
            DomNode.ChildInserted += new EventHandler<ChildEventArgs>(DomNode_ChildInserted);
            DomNode.ChildRemoved += new EventHandler<ChildEventArgs>(DomNode_ChildRemoved);

            base.OnNodeSet();

         }

        /// <summary>
        /// Gets an editable list of all modules in the circuit</summary>
        public IList<Element> Elements
        {
            get { return m_elements; }
        }

        /// <summary>
        /// Gets an editable list of all connections in the circuit</summary>
        public IList<Wire> Wires
        {
            get { return m_wires; }
        }

        /// <summary>
        /// Gets an editable list of all annotations in the circuit</summary>
        public IList<Annotation> Annotations
        {
            get { return m_annotations ?? s_emptyAnnotations;}
        }

        public bool Expanded
        {
            get { return true; } // a circuit is always expanded
            set { } // no operation, see above
        }


        /// <summary>
        /// Gets/sets a value indicating whether or not the contents of the group have been changed.</summary>
        public bool Dirty
        {
            get { return m_dirty; }
            set { m_dirty = value; }
        }

        public void Update()
        {
            Dirty = false;
        }

        public Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget,  bool inputSide)
        {
            var result = new Pair<Element, ICircuitPin>();


            foreach (var module in Elements)
            {
                result = module.MatchPinTarget(pinTarget, inputSide);
                if (result.First != null)
                    break;
            }
           

            return result;
        }

        public Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            var result = new Pair<Element, ICircuitPin>();


            foreach (var module in Elements)
            {
                result = module.FullyMatchPinTarget(pinTarget, inputSide);
                if (result.First != null)
                    break;
            }


            return result;
        }

        #region IGraph Members

        /// <summary>
        /// Gets all visible nodes in the circuit</summary>
        ///<remarks>IGraph.Nodes is called during circuit rendering, and picking(in reverse order). </remarks>
        IEnumerable<Element> IGraph<Element, Wire, ICircuitPin>.Nodes
        {
            get
            {
                //if (Dirty)
                //    return EmptyEnumerable<Module>.Instance;
                return m_elements.Where(x => x.Visible);
            }
        }

        /// <summary>
        /// Gets all connections between visible nodes in the circuit</summary>
        IEnumerable<Wire> IGraph<Element, Wire, ICircuitPin>.Edges
        {
            // Nodes are always drawn on top of edges, no in-line editing help here  
            get
            {
                //if (Dirty)
                //    return EmptyEnumerable<Connection>.Instance;
                return m_wires.Where(x => x.InputElement.Visible && x.OutputElement.Visible);               
            }
        }

        #endregion

        #region IAnnotatedDiagram Members

        /// <summary>
        /// Gets the sequence of annotations in the context</summary>
        IEnumerable<IAnnotation> IAnnotatedDiagram.Annotations
        {
            get { return Adapters.AsIEnumerable<IAnnotation>(Annotations); }
        }

        #endregion

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            Dirty = true;
 #if DEBUG_VERBOSE
            if (e.DomNode == DomNode || e.DomNode.Parent == DomNode)
            {
                Trace.TraceInformation("{0} element  {1} -- Attribute {2} changed from  {3} to {4}",
                    CircuitUtil.GetDomNodeName(DomNode),  CircuitUtil.GetDomNodeName(e.DomNode), e.AttributeInfo.Name, e.OldValue, e.NewValue);
            }
#endif
        }


        // Need to explicitly add Inputs and Outputs of sub-graph pins when nodes are inserted or deleted   
        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            Dirty = true;
#if DEBUG_VERBOSE
            if (e.Parent == DomNode)
            {
                Trace.TraceInformation("{0} --  Added {1} to parent {2}",
                      CircuitUtil.GetDomNodeName(DomNode),  CircuitUtil.GetDomNodeName(e.Child),  CircuitUtil.GetDomNodeName(e.Parent));
            }
#endif
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            Dirty = true;
#if DEBUG_VERBOSE
            if (e.Parent == DomNode)
            {
                Trace.TraceInformation("{0} --  Removed {1}  from parent {2}", CircuitUtil.GetDomNodeName(DomNode),
                                         CircuitUtil.GetDomNodeName(e.Child), CircuitUtil.GetDomNodeName(e.Parent));
            }
#endif
        }

        private static IList<Annotation> s_emptyAnnotations = new List<Annotation>().AsReadOnly();

        private DomNodeListAdapter<Element> m_elements;
        private DomNodeListAdapter<Wire> m_wires;
        private DomNodeListAdapter<Annotation> m_annotations;
        private bool m_dirty;
    }
}
