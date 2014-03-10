//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Subgraph renderer that draws subnodes as circuit elements, and subedges as wires.
    /// Also draw virtual representations of group pins for editing.</summary>
    /// <typeparam name="TElement">Element type, must implement IElementType, and IGraphNode</typeparam>
    /// <typeparam name="TWire">Wire type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TPin">Pin type, must implement ICircuitPin, IEdgeRoute</typeparam>
    public class D2dSubCircuitRenderer<TElement, TWire, TPin> : D2dCircuitRenderer<TElement, TWire, TPin>
        where TElement : class, ICircuitElement
        where TWire : class, IGraphEdge<TElement, TPin>
        where TPin : class, ICircuitPin
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="defaultTheme">Diagram theme for rendering graph</param>
        public D2dSubCircuitRenderer(D2dDiagramTheme defaultTheme)
            : base(defaultTheme)
        {
            m_fakeInputLinkPen = D2dFactory.CreateSolidBrush(Color.DarkOrchid);
            m_fakeOutputLinkPen = D2dFactory.CreateSolidBrush(Color.SlateGray);
            m_subGraphPinNodePen = D2dFactory.CreateSolidBrush(Color.SandyBrown);
            m_subGraphPinPen = D2dFactory.CreateSolidBrush(Color.DeepSkyBlue);
            m_pinBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDarkDark);
            m_visiblePinBrush = D2dFactory.CreateSolidBrush(Color.Black);
            m_hiddrenPinBrush = D2dFactory.CreateSolidBrush(Color.Gray); 

            var props = new D2dStrokeStyleProperties();
            props.EndCap = D2dCapStyle.Round;
            props.StartCap = D2dCapStyle.Round;
            props.DashStyle = D2dDashStyle.DashDot;
            m_VirtualLinkStrokeStyle = D2dFactory.CreateD2dStrokeStyle(props);
        }

        public RectangleF VisibleWorldBounds { get; set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_fakeInputLinkPen != null)
                {
                    m_fakeInputLinkPen.Dispose();
                    m_fakeInputLinkPen = null;
                }
                if (m_fakeOutputLinkPen != null)
                {
                    m_fakeOutputLinkPen.Dispose();
                    m_fakeOutputLinkPen = null;
                }
                if (m_subGraphPinNodePen != null)
                {
                    m_subGraphPinNodePen.Dispose();
                    m_subGraphPinNodePen = null;
                }
                if (m_subGraphPinPen != null)
                {
                    m_subGraphPinPen.Dispose();
                    m_subGraphPinPen = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DrawEyeIcon(RectangleF rect, D2dBrush pen, float strokeWidth, D2dGraphics g)
        {
            //g.DrawRectangle(rect, pen);
            float delta =  rect.Width/3;
            var p1 = new PointF(rect.X, rect.Y + rect.Height/2);
            var p2 = new PointF(p1.X + delta, rect.Y);
            var p3 = new PointF(p1.X + 2 * delta, rect.Y);
            var p4 = new PointF(rect.X+rect.Width, rect.Y + rect.Height/2);
            g.DrawBezier(p1,p2,p3,p4,pen,strokeWidth);// top lid
            p2 = new PointF(p2.X , rect.Y + rect.Height );
            p3 = new PointF(p3.X , rect.Y + rect.Height );
            g.DrawBezier(p1, p2, p3, p4, pen, strokeWidth); //bottom lid

            PointF irisCenter = new PointF(rect.X + rect.Width/2, rect.Y + rect.Height/2);
            float irisRadius = 0.2f*Math.Min(rect.Width, rect.Height);
            RectangleF irisRect = new RectangleF(irisCenter.X - irisRadius, irisCenter.Y - irisRadius, 2* irisRadius, 2* irisRadius);
            g.DrawEllipse(irisRect, pen, strokeWidth *1.8f);
        }
       
        public void DrawFloatingGroupPin(ICircuitGroupPin<TElement> grpPin, bool inputSide, DiagramDrawingStyle style, D2dGraphics g)
        {
            SizeF pinNameSize = g.MeasureText(grpPin.Name, Theme.TextFormat);
            PointF p;

            if (inputSide)
            {
                p = GetGroupPinLocation(grpPin, true);

                RectangleF pinRect = new RectangleF(p.X + CircuitGroupPinInfo.FloatingPinBoxWidth - Theme.PinSize,
                                             grpPin.Bounds.Location.Y + Theme.PinMargin + Theme.PinOffset,
                                             Theme.PinSize, Theme.PinSize);
                // draw output pin for input floating pins
                g.DrawRectangle(pinRect, m_subGraphPinPen);
                if (grpPin.Info.Pinned)
                    D2dUtil.DrawPin((int)(p.X + CircuitGroupPinInfo.FloatingPinBoxWidth), (int)p.Y, true, true, m_pinBrush, g);
                else
                    D2dUtil.DrawPin((int)(p.X + CircuitGroupPinInfo.FloatingPinBoxWidth), (int)p.Y + Theme.PinSize / 2, false, true, m_pinBrush, g);

                RectangleF bounds = new RectangleF(p.X, p.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
                RectangleF alignRect = new RectangleF(
                    bounds.Left, bounds.Bottom + Theme.PinMargin, pinNameSize.Width, Theme.RowSpacing);
                var textAlignment = Theme.TextFormat.TextAlignment;              
                Theme.TextFormat.TextAlignment = D2dTextAlignment.Leading;
                g.DrawText(grpPin.Name, Theme.TextFormat, alignRect.Location, Theme.TextBrush);
                Theme.TextFormat.TextAlignment = textAlignment;

            }
            else
            {
                // assume vertical scroll bar width = 16
                p = GetGroupPinLocation(grpPin, false);

                RectangleF pinRect = new RectangleF(p.X + 1, grpPin.Bounds.Location.Y + Theme.PinMargin + Theme.PinOffset,
                                             Theme.PinSize, Theme.PinSize);
                // draw input pin for output floating pins
                g.DrawRectangle(pinRect, m_subGraphPinPen);
                // draw pin icon                   
                if (grpPin.Info.Pinned)
                    D2dUtil.DrawPin((int)p.X, (int)p.Y, true, false, m_pinBrush, g);
                else
                    D2dUtil.DrawPin((int)p.X, (int)p.Y + Theme.PinSize / 2, false, false, m_pinBrush, g);

                // draw label
                RectangleF bounds = new RectangleF(p.X, p.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
                RectangleF alignRectF = new RectangleF(bounds.Right - pinNameSize.Width, bounds.Bottom + Theme.PinMargin,
                    pinNameSize.Width, Theme.RowSpacing);

                var textAlignment = Theme.TextFormat.TextAlignment;
                Theme.TextFormat.TextAlignment = D2dTextAlignment.Trailing;
                g.DrawText(grpPin.Name, Theme.TextFormat, alignRectF, Theme.TextBrush);
                Theme.TextFormat.TextAlignment = textAlignment;          
            }

            // draw the fake pin node itself
            float savedStrokeWidth = Theme.StrokeWidth;
            Theme.StrokeWidth = 2.0f;
            if (style == DiagramDrawingStyle.Normal)
                g.DrawRectangle(new RectangleF(p.X, p.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight), m_subGraphPinNodePen);
            else
                g.DrawRectangle(new RectangleF(p.X, p.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight), Theme.HotBrush);
            Theme.StrokeWidth = savedStrokeWidth;


            if (!grpPin.Info.ExternalConnected)
            {
                RectangleF eyeRect = GetVisibilityCheckRect(grpPin, inputSide);
                DrawEyeIcon(eyeRect, grpPin.Info.Visible ? m_visiblePinBrush : m_hiddrenPinBrush, 1.0f, g);

            }
            
            // draw fake edge that connects group pin fake node
            DrawGroupPinNodeFakeEdge(grpPin, p, inputSide, style, g);
         }


        /// <summary>
        /// Gets the bounding rectangle of a circuit element in local space, which is the same as
        /// world space except for sub-circuits</summary>
        /// <param name="pin">floating group pin to get the bounds for</param>
        /// <param name="inputSide">true if the pin is on the left side?</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Rectangle completely bounding the node</returns>
        public virtual RectangleF GetBounds(ICircuitGroupPin<TElement> pin, bool inputSide, D2dGraphics g)
        {
            var p = GetGroupPinLocation(pin, inputSide);
            var result = new RectangleF(p.X, p.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
            return result;
        }
        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public override GraphHitRecord<TElement, TWire, TPin> Pick(
            IGraph<TElement, TWire, TPin> graph, TWire priorityEdge, PointF p, D2dGraphics g)
        {
            var hitRecord = base.Pick(graph, priorityEdge, p, g);
            if (hitRecord.Node != null || hitRecord.Edge != null || hitRecord.Part != null)
                return hitRecord;

            // check whether hits virtual parts of group pin
            var group = graph.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
            foreach (var pin in group.Inputs.Concat(group.Info.HiddenInputPins))
            {
                var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                // check whether hit the thumbtack of a floating pin
                var pinRect = GetThumbtackRect(grpPin, true);
                if (pinRect.Contains(p))
                {
                    var pinPart = new DiagramPin(pinRect);
                    return new GraphHitRecord<TElement, TWire, TPin>((TPin) grpPin, pinPart);
                }

                // check whether hit the visibility check(eye icon)
                var eyeRect = GetVisibilityCheckRect(grpPin, true);
                if (eyeRect.Contains(p))
                {
                    var eyePart = new DiagramVisibilityCheck(eyeRect);
                    return new GraphHitRecord<TElement, TWire, TPin>((TPin)grpPin, eyePart);
                }
               

                // check whether hit the floating pin label-part        
                PointF grpPos = GetGroupPinLocation(grpPin, true);
                RectangleF bounds = new RectangleF(grpPos.X, grpPos.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
                SizeF nameSize = g.MeasureText(grpPin.Name, Theme.TextFormat);
                RectangleF labelBounds = new RectangleF(bounds.Left, bounds.Bottom + Theme.PinMargin, (int)nameSize.Width, Theme.RowSpacing);
                //labelBounds = GdiUtil.Transform(g.Transform, labelBounds);
                var labelPart = new DiagramLabel(
                new Rectangle((int)labelBounds.Left, (int)labelBounds.Top, (int)labelBounds.Width, (int)labelBounds.Height),
                TextFormatFlags.SingleLine | TextFormatFlags.Left);    
    
                if (labelBounds.Contains(p))
                 return new GraphHitRecord<TElement, TWire, TPin>((TPin)grpPin, labelPart);               

                // check whether hit the floating pin node
                if (bounds.Contains(p))
                {
                    var result = new GraphHitRecord<TElement, TWire, TPin>((TPin) grpPin, null);
                    result.DefaultPart = labelPart;
                    return result;
                }
            }

            foreach (var pin in group.Outputs.Concat(group.Info.HiddenOutputPins))
            {
                var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                // check whether hit the thumbtack of a floating pin
                var pinRect = GetThumbtackRect(grpPin, false);
                if (pinRect.Contains(p))
                {
                    var pinPart = new DiagramPin(pinRect);
                    return new GraphHitRecord<TElement, TWire, TPin>((TPin)grpPin, pinPart);
                }

                // check whether hit the visibility check(eye icon)
                var eyeRect = GetVisibilityCheckRect(grpPin, false);
                if (eyeRect.Contains(p))
                {
                    var eyePart = new DiagramVisibilityCheck(eyeRect);
                    return new GraphHitRecord<TElement, TWire, TPin>((TPin)grpPin, eyePart);
                }

                // check whether hit the floating pin label-part     
                PointF grpPos = GetGroupPinLocation(grpPin, false);
                RectangleF bounds = new RectangleF(grpPos.X, grpPos.Y, CircuitGroupPinInfo.FloatingPinBoxWidth, CircuitGroupPinInfo.FloatingPinBoxHeight);
                 SizeF nameSize = g.MeasureText(grpPin.Name, Theme.TextFormat);
                 RectangleF labelBounds = new RectangleF(bounds.Right - (int)nameSize.Width, bounds.Bottom + Theme.PinMargin, (int)nameSize.Width, Theme.RowSpacing);
                //labelBounds = GdiUtil.Transform(g.Transform, labelBounds);
                var labelPart = new DiagramLabel(
                 new Rectangle((int)labelBounds.Left, (int)labelBounds.Top, (int)labelBounds.Width, (int)labelBounds.Height),
                 TextFormatFlags.SingleLine | TextFormatFlags.Right);

                if (labelBounds.Contains(p))
                    return new GraphHitRecord<TElement, TWire, TPin>((TPin)grpPin, labelPart);
              
                // check whether hit the floating pin node
                if (bounds.Contains(p))
                {
                    var result = new GraphHitRecord<TElement, TWire, TPin>((TPin)grpPin, null);
                    result.DefaultPart = labelPart;
                    return result;
                }
            }      

            return hitRecord;
        }

        private PointF GetGroupPinLocation(ICircuitGroupPin<TElement> grpPin,  bool inputSide)
        {
             if (inputSide)
                 return new PointF(VisibleWorldBounds.X + CircuitGroupPinInfo.FloatingPinBoxWidth + Theme.PinMargin, grpPin.Bounds.Location.Y);
             else
                 return new PointF(VisibleWorldBounds.X + VisibleWorldBounds.Width - Theme.PinMargin - 2 * CircuitGroupPinInfo.FloatingPinBoxWidth - 16, grpPin.Bounds.Location.Y);
        }

        private RectangleF GetThumbtackRect(ICircuitGroupPin<TElement> grpPin,  bool inputSide)
        {

            PointF p = GetGroupPinLocation(grpPin, inputSide);

            int xOffset = inputSide ? CircuitGroupPinInfo.FloatingPinBoxWidth : -D2dUtil.ThumbtackSize;
            if (grpPin.Info.Pinned)
                return new RectangleF(p.X + xOffset, p.Y, D2dUtil.ThumbtackSize, D2dUtil.ThumbtackSize);

            return new RectangleF(p.X + xOffset, p.Y + Theme.PinSize / 2.0f, D2dUtil.ThumbtackSize, D2dUtil.ThumbtackSize);
        }

        private RectangleF GetVisibilityCheckRect(ICircuitGroupPin<TElement> grpPin, bool inputSide)
        {

            RectangleF pinRect, eyeRect;
            PointF p = GetGroupPinLocation(grpPin, inputSide);
            float width = CircuitGroupPinInfo.FloatingPinBoxWidth - Theme.PinSize - 3;
            if (inputSide)
            {
                pinRect = new RectangleF(p.X + CircuitGroupPinInfo.FloatingPinBoxWidth - Theme.PinSize,
                                         grpPin.Bounds.Location.Y + Theme.PinMargin + Theme.PinOffset,
                                         Theme.PinSize, Theme.PinSize);

                eyeRect = new RectangleF(p.X - 2 - width, pinRect.Y, width, pinRect.Height);
            }
            else
            {
                pinRect = new RectangleF(p.X + 1, grpPin.Bounds.Location.Y + Theme.PinMargin + Theme.PinOffset,
                                           Theme.PinSize, Theme.PinSize);
                eyeRect = new RectangleF(p.X + CircuitGroupPinInfo.FloatingPinBoxWidth + 2, pinRect.Y, width, pinRect.Height);
            }
            return eyeRect;
        }

        private void DrawGroupPinNodeFakeEdge(ICircuitGroupPin<TElement> grpPin, PointF grpPinPos, bool inputSide, DiagramDrawingStyle style, D2dGraphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(grpPin.InternalElement, g);
            D2dBrush pen = (style == DiagramDrawingStyle.Normal) ? SubGraphPinBrush : Theme.GetOutLineBrush(style);
            if (inputSide)
            {             
                PointF op = grpPinPos;
                float x1 = op.X + CircuitGroupPinInfo.FloatingPinBoxWidth;
                float y1 = op.Y + CircuitGroupPinInfo.FloatingPinBoxHeight / 2;

                Point ip = grpPin.InternalElement.Bounds.Location;
                float x2 = ip.X;
                float y2 = ip.Y + GetPinOffset(grpPin.InternalElement, grpPin.InternalPinIndex, true );

                DrawWire(g, m_fakeInputLinkPen, x1, y1, x2, y2, 1.0f, m_VirtualLinkStrokeStyle);
            }
            else
            {
                Point op = grpPin.InternalElement.Bounds.Location;
                float x1 = op.X + info.Size.Width;
                float y1 = op.Y + GetPinOffset(grpPin.InternalElement, grpPin.InternalPinIndex,false);

                PointF ip = grpPinPos;
                float x2 = ip.X;
                float y2 = ip.Y + CircuitGroupPinInfo.FloatingPinBoxHeight / 2;
                DrawWire(g, m_fakeOutputLinkPen, x1, y1, x2, y2, 1.0f, m_VirtualLinkStrokeStyle);
            }
          
        }
        

        private const int MaxNameOverhang = 64;
 
        private D2dBrush m_subGraphPinNodePen;
        private D2dBrush m_subGraphPinPen;
        //private float m_subGraphPinPenWidth = 3.0f;
        private D2dBrush m_fakeInputLinkPen;
        private D2dBrush m_fakeOutputLinkPen;
        private D2dBrush m_pinBrush;
        private D2dBrush m_visiblePinBrush;
        private D2dBrush m_hiddrenPinBrush;

        private D2dStrokeStyle m_VirtualLinkStrokeStyle;
    }
}
