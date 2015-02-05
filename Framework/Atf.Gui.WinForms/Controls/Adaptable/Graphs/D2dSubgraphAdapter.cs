//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Control adapter to reference and render a subgraph diagram. Also provides hit testing
    /// with the Pick method, and viewing support with the Frame and EnsureVisible methods.</summary>
    /// <typeparam name="TNode">Node type, must implement IGraphNode</typeparam>
    /// <typeparam name="TEdge">Edge type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TEdgeRoute">Edge route type, must implement IEdgeRoute</typeparam>
    /// <remark>ControlAdapter accesses AdaptableControl, which is a Form's control, hence any class derived from ControlAdapter
    /// belongs to Atf.Gui.WinForms project</remark>    
    public class D2dSubgraphAdapter<TNode, TEdge, TEdgeRoute> : D2dGraphAdapter<TNode, TEdge, TEdgeRoute>
        where TNode : class, ICircuitElement
        where TEdge : class, IGraphEdge<TNode, TEdgeRoute>
        where TEdgeRoute : class, ICircuitPin
    {
        /// <summary>
        /// Constructor</summary>        
        /// <param name="renderer">Graph renderer to draw and hit-test graph</param>
        /// <param name="transformAdapter">Transform adapter</param>
        public D2dSubgraphAdapter(D2dSubCircuitRenderer<TNode, TEdge, TEdgeRoute> renderer,
            ITransformAdapter transformAdapter) : base(renderer, transformAdapter)
        {

            m_renderer = renderer;
            m_transformAdapter = transformAdapter;

        }

 
        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            base.Bind(control);

            var d2dControl = control as D2dAdaptableControl;
            m_d2dGraphics = d2dControl.D2dGraphics;
            d2dControl.ContextChanged += control_ContextChanged;
            m_scaleBrush = D2dFactory.CreateSolidBrush(control.ForeColor);
            m_textFormat = D2dFactory.CreateTextFormat(d2dControl.Font);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            var d2dControl = control as D2dAdaptableControl;
            d2dControl.ContextChanged -= control_ContextChanged;
            base.Unbind(control);
            m_scaleBrush.Dispose();
            m_textFormat.Dispose();
            m_scaleBrush = null;
            m_textFormat = null;
            m_d2dGraphics = null;
        }

        /// <summary>
        /// Renders entire graph</summary>
        protected override void OnRender()
        {
            if (m_graph == s_emptyGraph)
                return;
            base.OnRender();

            m_renderer.VisibleWorldBounds = GdiUtil.InverseTransform(m_transformAdapter.Transform, AdaptedControl.ClientRectangle);

            // render group pins
            var group = m_graph.Cast<ICircuitGroupType<TNode, TEdge, TEdgeRoute>>();
            foreach (var pin in group.Inputs.Concat(group.Info.HiddenInputPins))
            {
                var grpPin = pin.Cast<ICircuitGroupPin<TNode>>();
                DiagramDrawingStyle style = GetStyle(grpPin);
                m_renderer.DrawFloatingGroupPin(grpPin, true, style, m_d2dGraphics);
            }

            foreach (var pin in group.Outputs.Concat(group.Info.HiddenOutputPins))
            {
                var grpPin = pin.Cast<ICircuitGroupPin<TNode>>();
                DiagramDrawingStyle style = GetStyle(grpPin);
                m_renderer.DrawFloatingGroupPin(grpPin, false, style, m_d2dGraphics);
            }
        }

        /// <summary>
        /// Performs hit testing for rectangle bounds, in client coordinates</summary>
        /// <param name="pickRect">Pick rectangle, in client coordinates</param>
        /// <returns>Items that overlap with the rectangle, in client coordinates</returns>
        public override IEnumerable<object> Pick(Rectangle pickRect)
        {
            Matrix3x2F invXform = Matrix3x2F.Invert(m_d2dGraphics.Transform);
            RectangleF rect = D2dUtil.Transform(invXform, pickRect);
            IEnumerable<object> pickedGraphNodes = base.Pick(pickRect);
            foreach (var pickedGraphNode in pickedGraphNodes) 
                yield return pickedGraphNode;
           
            var pickedFloatingPins = new List<object>();
            var circuiElement = m_graph.Cast<ICircuitElementType>();
            foreach (var pin in circuiElement.Inputs)
            {
                var grpPIn = pin.Cast<ICircuitGroupPin<TNode>>();
                RectangleF nodeBounds = m_renderer.GetBounds(grpPIn, true, m_d2dGraphics); 
                if (nodeBounds.IntersectsWith(rect))
                    pickedFloatingPins.Add(pin);
            }

            foreach (var pin in circuiElement.Outputs)
            {
                var grpPIn = pin.Cast<ICircuitGroupPin<TNode>>();
                RectangleF nodeBounds = m_renderer.GetBounds(grpPIn, false, m_d2dGraphics); 
                if (nodeBounds.IntersectsWith(rect))
                    pickedFloatingPins.Add(pin);
            }

            foreach (var floatingPin in pickedFloatingPins)
                yield return floatingPin;
          
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            IGraph<TNode, TEdge, TEdgeRoute> graph = AdaptedControl.ContextAs<IGraph<TNode, TEdge, TEdgeRoute>>();
            if (graph == null)
                graph = s_emptyGraph;
            m_graph = graph;
        }

        private D2dGraphics m_d2dGraphics;
        private readonly D2dSubCircuitRenderer<TNode, TEdge, TEdgeRoute> m_renderer;
        private readonly ITransformAdapter m_transformAdapter;
        private IGraph<TNode, TEdge, TEdgeRoute> m_graph = s_emptyGraph;

        private D2dSolidColorBrush m_scaleBrush;
        private D2dTextFormat m_textFormat;
     }
}
