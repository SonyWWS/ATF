//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapter for adding floating group pin location and label editing capabilities to a subgraph control</summary>
    public class GroupPinEditor: DraggingControlAdapter, IItemDragAdapter, IDisposable    
    {
        public Func<ICircuitElement, int, bool, int> GetPinOffset; // callback to compute group pin y offset

        /// <summary>
        /// Constructor</summary>
        /// <param name="transformAdapter">Transform adapter</param>
        public GroupPinEditor(ITransformAdapter transformAdapter)
        {
            m_transformAdapter = transformAdapter;
        }


        [Flags]
        private enum MeasurePinNode
        {       
            DesiredLocation = 1,
            MaximumNameWidth = 2,        
        }


        #region IItemDragAdapter Members

        /// <summary>
        /// Begins dragging any selected items managed by the adapter. May be called
        /// by another adapter when it begins dragging.</summary>
        /// <param name="initiator">Control adapter that is initiating the drag</param>
        void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
        {
           
            // drag all selected nodes, and any edges impinging on them
            var draggingNodes = new ActiveCollection<Element>();
         
            foreach (var node in m_selectionContext.GetSelection<Element>())
                draggingNodes.Add(node);
                 
            m_draggingNodes.AddRange(draggingNodes.GetSnapshot());

            var draggingGrpPins= new ActiveCollection<GroupPin>();
            // add the selected floating pins
            foreach (var grpin in m_selectionContext.GetSelection<GroupPin>())         
                draggingGrpPins.Add(grpin);
            m_draggingGroupPins.AddRange(draggingGrpPins.GetSnapshot());

            if (m_draggingGroupPins.Any())
            {
                m_originalPinY = new int[m_draggingGroupPins.Count];
                for (int i = 0; i < m_originalPinY.Length; i++)
                    m_originalPinY[i] = m_draggingGroupPins[i].Bounds.Location.Y;
            }
        }

        /// <summary>
        /// Called before a transaction is initiated and before EndDrag() is called, to
        /// indicate that the drag will be ending. The implementor can use this opportunity
        /// to prepare for being included in the caller's transaction.</summary>
        /// <remarks>If the location of the dragged objects is backed by the DOM, use this
        /// opportunity to relocate the selected objects to their original positions when
        /// the drag began. Then, EndDrag() can move the objects to their final location
        /// and the transaction will record the correct before and after positions.</remarks>
        void IItemDragAdapter.EndingDrag()
        {
        }

        /// <summary>
        /// Ends dragging any selected items managed by the adapter. May be called
        /// by another adapter when it ends dragging. May be called from within a transaction.</summary>
        void IItemDragAdapter.EndDrag()
        {
            m_draggingGroupPins.Clear();
            m_draggingNodes.Clear();
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases non-memory resources</summary>
        public void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            control.ContextChanged += control_ContextChanged;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
        
            control.ContextChanged += control_ContextChanged;
            control.ContextChanged -= control_ContextChanged;
        }      

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse move event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left )
            {
                Point currentPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, CurrentPoint);
                Point fristPoint = GdiUtil.InverseTransform(m_transformAdapter.Transform, FirstPoint);
                Point offset = new Point(currentPoint.X - fristPoint.X, currentPoint.Y - fristPoint.Y);
                if (m_draggingNodes.Any())
                {
                     if (!offset.IsEmpty)
                         AdjustLayout(m_selectionContext.GetSelection<Element>(), m_draggingGroupPins, offset);
                }

                if (m_draggingGroupPins.Any() && offset.Y != 0)
                {
                    for (int i = 0; i < m_originalPinY.Length; i++)
                    {
                        m_draggingGroupPins[i].Bounds = new Rectangle(m_draggingGroupPins[i].Bounds.Location.X,  Constrain(m_originalPinY[i] + offset.Y),
                            m_draggingGroupPins[i].Bounds.Width, m_draggingGroupPins[i].Bounds.Height);
                    }
                    AdaptedControl.Invalidate();
                }
            }
        }

        /// <summary>
        /// Custom update min/max translation of the transformAdapter associated with the canvas</summary>
        /// <param name="canvasAdapter">Canvas adapter</param>
        public void UpdateTranslateMinMax(CanvasAdapter canvasAdapter)
        {
            // do nothing( no need to limit translation range of the transformAdapter assoicated with  canvasAdapter)
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            IGraph<Element, Wire, ICircuitPin> graph = AdaptedControl.ContextAs<IGraph<Element, Wire, ICircuitPin>>();
            if (graph == null)
                graph = CircuitUtil.EmptyCircuit;

            if (m_graph != graph)
            {

                m_graph = graph;
            }

            m_layoutContext = AdaptedControl.ContextAs<ILayoutContext>();

            if (m_layoutContext != null)
            {
                m_selectionContext = AdaptedControl.ContextCast<ISelectionContext>();
            }
        }

 
        /// <summary>
        /// Set desired location for fake pins</summary>   
        private void MeasureFakePins(IEnumerable<GroupPin> pinNodes, MeasurePinNode measureParts, Point offset, bool inputSide)
        {
            //int maximumNameWidth = 0;
            if (pinNodes.Any())
            {
                //var subGraph = m_graph.As<Group>();
                //using (Graphics g = AdaptedControl.CreateGraphics())
                {
                    //g.Transform = m_transformAdapter.Transform;
                    if ( (measureParts & MeasurePinNode.DesiredLocation) !=0)
                    {
                        // a floating pin node maps to a subnode and its pin index, needed to sort by both                      
                        var pinList = pinNodes.OrderBy(x => x.InternalElement.Position.Y)
                                               .ThenBy(x => x.InternalElement.PinDisplayOrder(x.InternalPinIndex, inputSide)).ToList();
                        //bool initialOrder = pinList.All(n => n.PinY == int.MinValue);

                        //if (initialOrder && subGraph.DefaultPinOrder == Group.PinOrderStyle.DepthFirst)
                        //{
                        //    int topY = pinNodes.Min(x => x.GroupPin.InternalModule.Position.Y);
                        //    foreach (var fakePinNode in pinList)
                        //    {

                        //        fakePinNode.DesiredLocation = new Point(offset.X, topY + offset.Y + fakePinNode.GroupPin.Index * (PinNodeHeight + PinNodeMargin));
                        //    }
                        //}
                        //else
                        {
                            foreach (var fakePinNode in pinList)
                            {
                                int pinOffset = GetPinOffset(fakePinNode.InternalElement, fakePinNode.InternalPinIndex, inputSide);
                                fakePinNode.DesiredLocation = new Point( offset.X, fakePinNode.InternalElement.Bounds.Location.Y + pinOffset - 8);
                           }

                            // ensure the minimum margin between pin nodes( avoid overlapping among the pin nodes)                                                
                            int lastY = pinList[0].DesiredLocation.Y;
                            for (int i = 1; i < pinList.Count; ++i)
                            {
                                var pin = pinList[i];

                                int delta = pin.DesiredLocation.Y - lastY - (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
                                if (delta < 0) // distance is less than the minimum interval for the current floating pin
                                {
                                    // adjust the desired location to have a safe margin below the  preceding floating pin
                                    pinList[i].DesiredLocation =  new Point(0,  pinList[i].DesiredLocation.Y - delta);
                                }
                                lastY = pin.DesiredLocation.Y;
                            }
                        }
                       
                    }

                    //if (measureParts.HasFlag(MeasurePinNode.MaximumNameWidth))
                    //{
                    //      foreach (var fakePinNode in pinNodes)
                    //      {
                    //          SizeF nameSize = g.MeasureString(fakePinNode.SubGraphPin.Name, m_subGraphRenderer.GetElementFont(fakePinNode, g));
                    //          if (nameSize.Width > maximumNameWidth)
                    //              maximumNameWidth = (int) (nameSize.Width+ 0.5) + PinNodeMargin;
                    //      }
                    //}
                }
            }
            //return maximumNameWidth;
        }

        // adjusting floating pins layout
        public void AdjustLayout(IEnumerable<Element> nodesMoved, IEnumerable<GroupPin> floatingPinsMoved, Point offset)
        {
            var movedNodes = nodesMoved.ToList();
            var subGraph = m_graph.As<Group>();
            if (subGraph == null || nodesMoved.Count() == 0)
                return;

            for (int pass = 0; pass < 2; ++pass) //pass 0 for input side, pass 1 for output side
            {
                var grpPins = pass == 0 ? subGraph.InputGroupPins : subGraph.OutputGroupPins;
                // calculate all free pins of moving nodes' natural position,then ordered bottom up
                // note for selected floating pins( group pins being directly dragged) should sync the move with the mouse
                var freePinsToAdjust = grpPins
                    .Where(x => !m_draggingGroupPins.Contains(x) && movedNodes.Contains(x.InternalElement) && !x.Info.Pinned)
                    .ToList();

                // now adjust free pins
                var againstPins = grpPins
                    .Where(x => !freePinsToAdjust.Contains(x))
                    .OrderBy(y => y.Bounds.Location.Y).ToList();

                MeasureFakePins(freePinsToAdjust, MeasurePinNode.DesiredLocation, offset, pass==0);

                int minY     = int.MinValue; // minimum allowed new y location for free pins

                var floatingPins = freePinsToAdjust.OrderBy(x => x.DesiredLocation.Y).ToList();
                for (int i = 0; i < floatingPins.Count; ++i)
                {
                    var floatingPin = floatingPins[i];
                    PositioningFloatigPin(againstPins,  floatingPin, minY);
                    minY = floatingPin.Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight +
                           CircuitGroupPinInfo.FloatingPinNodeMargin;
                    // update desired location for the rest of the free pins 
                    int delta = floatingPin.Bounds.Location.Y - floatingPin.DesiredLocation.Y;
                    for (int j = i + 1; j < floatingPins.Count; ++j)
                        floatingPins[j].DesiredLocation = new Point(floatingPins[j].DesiredLocation.X, Math.Max(floatingPins[j].DesiredLocation.Y + delta, minY));
                }

            }
         }


        /// <summary>
        /// Resolve pin node position to avoid colliding with any node in againstPinNodes</summary>
        /// <param name="againstPinNodes">List of GroupPins to avoid collision with</param>
        /// <param name="floatingPin">Floating pin to adjust: use its DesiredLocation to check collisions and its ActualLocation will be set upon return</param>
        /// <param name="minY">Minimum Y value the floating pin is allowed to take</param>
        private void PositioningFloatigPin(IList<GroupPin> againstPinNodes,  GroupPin floatingPin, int minY)
        {
            // resolve pin node position to avoid colliding with any against-pins 
            bool overlapped = false;
            for (int p = 0; p < againstPinNodes.Count; ++p) // try to locate sufficient empty space between the static nodes
            {
                var againstpin = againstPinNodes[p];
                if (againstpin.Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin < minY)
                    continue; 
                if (PinOverlap(floatingPin, againstpin))
                {
                    overlapped = true;
                    // find next suitable place from p down (note againstPinNodes y-ascending order, i.e. higher notes appear first)
                    for (int j = p; j < againstPinNodes.Count - 1; ++j)
                    {
                        if (againstPinNodes[j+1].Bounds.Location.Y - againstPinNodes[j ].Bounds.Location.Y >= 2 * (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin))
                        {
                            // find empty space >=  pin node height + margin
                            // place the floating pin just above static pin j
                            int pinNewY = Constrain(againstPinNodes[j].Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
                            floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, pinNewY, floatingPin.Bounds.Width, floatingPin.Bounds.Height);
                            return;
                        }
                    }
                }
            }

            if (overlapped) // no in-between empty space to insert, try at the top region first, then the bottom region
            {
                // top region 
                var topStaticPin = againstPinNodes[0];
                int pinNewY = Constrain(topStaticPin.Bounds.Location.Y - (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
                pinNewY = Math.Max(pinNewY, minY);
                floatingPin.DesiredLocation = new Point(floatingPin.DesiredLocation.X, pinNewY);
                if (pinNewY > minY &&  !PinOverlap(floatingPin, topStaticPin))
                {
                    floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, pinNewY, floatingPin.Bounds.Width,
                                                       floatingPin.Bounds.Height);
                }
                else // bottom region
                {
                    var bottomStaticPin = againstPinNodes[againstPinNodes.Count - 1];
                    pinNewY =
                        Constrain(bottomStaticPin.Bounds.Location.Y +
                                  (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
                    pinNewY = Math.Max(pinNewY, minY);
                    floatingPin.DesiredLocation = new Point(floatingPin.DesiredLocation.X, pinNewY);
                    const int increment = 1; 
                    while (PinOverlap(floatingPin, bottomStaticPin))
                    {
                        pinNewY += increment; 
                        floatingPin.DesiredLocation = new Point(floatingPin.DesiredLocation.X, pinNewY);
                    }

                    floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, pinNewY, floatingPin.Bounds.Width,
                                                       floatingPin.Bounds.Height);
                }
            }
            else // the disired location turns out just fine
            {
                floatingPin.Bounds = new Rectangle(floatingPin.Bounds.Location.X, Constrain(floatingPin.DesiredLocation.Y), floatingPin.Bounds.Width, floatingPin.Bounds.Height);
            }

        }

        // whether freePin's desired location overlaps static pin's actual location
        private static bool PinOverlap(GroupPin freePin, GroupPin againstPin)
        {
            // consider a pin's y interval is  (pinY - PinNodeMargin,  pinY +  PinNodeHeight +PinNodeMargin) top down
            // freePin overlaps againstPin if the two intervals overlap
            var freePinInterval = new Pair<int, int>(freePin.DesiredLocation.Y - CircuitGroupPinInfo.FloatingPinNodeMargin, freePin.DesiredLocation.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
            var pinInterval = new Pair<int, int>(againstPin.Bounds.Location.Y - CircuitGroupPinInfo.FloatingPinNodeMargin, againstPin.Bounds.Location.Y + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin);
            // two intervals i1 = (s1, e1) and i2 = (s2, e2) overlap if and only if s2 < e1 and s1 < e2
            return ((pinInterval.First < freePinInterval.Second) && // s2 < e1
                    (freePinInterval.First < pinInterval.Second)); //s1 < e2


        }

        // Applies constraint to group pinY value
        private int Constrain(int y)
        {
            return y;
            //return Math.Max(0, y);
        }


        private IGraph<Element, Wire, ICircuitPin> m_graph = CircuitUtil.EmptyCircuit;
        private ITransformAdapter m_transformAdapter;
 
        private List<Element> m_draggingNodes= new List<Element>();
        private List<GroupPin> m_draggingGroupPins = new List<GroupPin>();
        private int [] m_originalPinY;        

        private ILayoutContext m_layoutContext;
        private ISelectionContext m_selectionContext;

    }
}
