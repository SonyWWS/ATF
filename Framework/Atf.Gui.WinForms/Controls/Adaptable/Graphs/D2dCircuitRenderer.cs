//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Graph renderer that draws graph nodes as circuit elements, and edges as wires.
    /// Elements have 0 or more output pins, where wires originate, and 0 or more input
    /// pins, where wires end. Output pins are on the right and input pins are on the left
    /// side of elements.</summary>
    /// <typeparam name="TElement">Element type, must implement IElementType, and IGraphNode</typeparam>
    /// <typeparam name="TWire">Wire type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TPin">Pin type, must implement ICircuitPin, IEdgeRoute</typeparam>
    public class D2dCircuitRenderer<TElement, TWire, TPin> : D2dGraphRenderer<TElement, TWire, TPin>, IDisposable
        where TElement : class, ICircuitElement
        where TWire : class, IGraphEdge<TElement, TPin>
        where TPin : class, ICircuitPin
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Diagram theme for rendering graph</param>
        /// <param name="documentRegistry">An optional document registry, used to clear the internal
        /// element type cache when a document is removed</param>
        public D2dCircuitRenderer(D2dDiagramTheme theme, IDocumentRegistry documentRegistry = null)
        {
            m_theme = theme;
            m_theme.Redraw += new EventHandler(theme_Redraw);
            SetPinSpacing();
            m_elementBody.RadiusX = 6;
            m_elementBody.RadiusY = 6;

            EdgeThickness = 2.0f;
            m_subGraphPinBrush = D2dFactory.CreateSolidBrush(Color.SandyBrown);
            MaxCollapsedGroupPinNameLength = 25;

            m_documentRegistry = documentRegistry;
            if (m_documentRegistry != null)
            {
                m_documentRegistry.DocumentRemoved += DocumentRegistryOnDocumentRemoved;
                m_documentRegistry.ActiveDocumentChanging += DocumentRegistryOnActiveDocumentChanging;
                m_documentRegistry.ActiveDocumentChanged += DocumentRegistryOnActiveDocumentChanged;
            }

            RoundedBorder = true;
        }

        /// <summary>
        /// Specifies the pin drawing style </summary>
        public enum PinStyle
        {
            Default, // non-filled, location adjacent to border but internal
            OnBorderFilled,
        }

        /// <summary>
        /// Gets or sets the pin drawing style</summary>
        public PinStyle PinDrawStyle
        {
            get { return m_pinDrawStyle; }
            set { m_pinDrawStyle = value; }
        }

        /// <summary>
        /// Get the string to use on the title bar of the circuit element</summary>
        /// <param name="element">Circuit element</param>
        /// <returns>String to use on title bar of circuit element</returns>
        protected virtual string GetElementTitle(TElement element)
        {
            return element.Type.Name;
        }

        /// <summary>
        /// Get the display name of the circuit element, which is drawn below and outside the circuit element. 
        /// Returns null or the empty string to not draw anything in this place</summary>
        /// <param name="element">Circuit element</param>
        /// <returns>Display name of the circuit element</returns>
        protected virtual string GetElementDisplayName(TElement element)
        {
            return element.Name;
        }

        /// <summary>
        /// Get element type cache</summary>
        protected Dictionary<ICircuitElementType, ElementTypeInfo> ElementTypeCache
        {
            get { return m_elementTypeCache; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the background area of the title should be filled</summary>
        public bool TitleBackgroundFilled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether edges of elements have a rounded 
        /// rather than a square or sharp appearance</summary>
        public bool RoundedBorder { set; get; }

        /// <summary>
        /// Gets or sets the threshold size that shows the details in a circuit element</summary>
        /// <remarks>Title, pins, and labels(after transformation) smaller than the threshold won’t be displayed for speed optimization.
        /// Default is 6. Set value 0 effectively turns off this optimization.</remarks>
        public int DetailsThresholdSize { get; set; }

        /// <summary>
        /// Gets or sets a Boolean that determines if wires should be selected as a fallback
        /// if no elements are selected, in a marquee selection</summary>
        public bool RectangleSelectsWires { get; set; }

        /// <summary>
        /// Gets or sets D2dDiagramTheme</summary>
        public D2dDiagramTheme Theme
        {
            get { return m_theme; }
            set
            {
                if (m_theme != value)
                {
                    if (m_theme != null)
                        m_theme.Redraw -= new EventHandler(theme_Redraw);
                    SetPinSpacing();
                    m_theme = value;
                }
            }
        }


        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_theme.Redraw -= new EventHandler(theme_Redraw);
                m_subGraphPinBrush.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets D2dBrush for subgraph pins</summary>
        public D2dBrush SubGraphPinBrush
        {
            get { return m_subGraphPinBrush; }
            set
            {
                m_subGraphPinBrush.Dispose();
                m_subGraphPinBrush = value;
            }
        }

        /// <summary>
        /// Called when content of graph object changes</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        public override void OnGraphObjectChanged(object sender, ItemChangedEventArgs<object> e)
        {
            if (e.Item.Is<ICircuitElementType>())
                Invalidate(e.Item.Cast<ICircuitElementType>());
        }

        /// <summary>
        /// Invalidates cached info for element type name</summary>
        /// <param name="elementType">Element type to invalidate</param>
        public void Invalidate(ICircuitElementType elementType)
        {
            m_elementTypeCache.Remove(elementType);
            OnRedraw();
        }

        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="element">Element to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(TElement element, DiagramDrawingStyle style, D2dGraphics g)
        {
            DiagramDrawingStyle cusomStyle = GetCustomStyle(element);
            if (cusomStyle != DiagramDrawingStyle.None)
                style = cusomStyle; // custom style overrides a regular style 

            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    Draw(element, g, false);
                    break;
                case DiagramDrawingStyle.Selected:
                case DiagramDrawingStyle.LastSelected:
                case DiagramDrawingStyle.Hot:
                case DiagramDrawingStyle.DropTarget:
                case DiagramDrawingStyle.DragSource:
                default:
                    Draw(element, g, false);
                    DrawOutline(element, m_theme.GetOutLineBrush(style), g);
                    break;

                case DiagramDrawingStyle.Ghosted:
                case DiagramDrawingStyle.Hidden:
                    DrawGhost(element, g);
                    break;
            }
        }

        /// <summary>
        /// Draws a graph edge</summary>
        /// <param name="edge">Edge to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(TWire edge, DiagramDrawingStyle style, D2dGraphics g)
        {
            TPin inputPin = edge.ToRoute;
            TPin outputPin = edge.FromRoute;
            if (RectangleSelectsWires && style == DiagramDrawingStyle.LastSelected) // last selected is not well defined in multi-edge selection   
                style = DiagramDrawingStyle.Selected;

            D2dBrush pen = (style == DiagramDrawingStyle.Normal) ? GetPen(inputPin) : m_theme.GetOutLineBrush(style);
            if (edge.Is<IEdgeStyleProvider>())
                DrawEdgeUsingStyleInfo(edge.Cast<IEdgeStyleProvider>(), pen, g);
            else
                DrawWire(edge.FromNode, outputPin, edge.ToNode, inputPin, g, pen);
        }

        private void DrawEdgeUsingStyleInfo(IEdgeStyleProvider edgeStyleProvider, D2dBrush pen, D2dGraphics g)
        {
            foreach (var edgeInfo in edgeStyleProvider.GetData(this, WorldOffset(m_graphPath), g))
            {
                if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.Bezier)
                {
                    var curve = edgeInfo.EdgeData.As<BezierCurve2F>();
                    g.DrawBezier(curve.P1, curve.P2, curve.P3, curve.P4, pen, edgeInfo.Thickness);
                }
                else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.Line)
                {
                    var line = edgeInfo.EdgeData.As<PointF[]>();
                    if (line != null)
                        g.DrawLine(line[0], line[1], pen, edgeInfo.Thickness);
                }
                else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.Polyline)
                {
                    var lines = edgeInfo.EdgeData.As<PointF[]>();
                    if (lines != null)
                        g.DrawLines(lines, pen, edgeInfo.Thickness);
                }
                else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.BezierSpline)
                {
                    var curves = edgeInfo.EdgeData.As<IEnumerable<BezierCurve2F>>();
                    foreach (var curve in curves)
                    {
                        g.DrawBezier(curve.P1, curve.P2, curve.P3, curve.P4, pen, edgeInfo.Thickness);
                    }
                }
                else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.None)
                    return;

            }
        }


        /// <summary>
        /// Gets group pin position in group local space</summary>
        /// <param name="groupPin">Group pin</param>
        /// <param name="group">Owner</param>/param>
        /// <param name="inputSide">True if pin side is input, false for output side</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Group pin position</returns>
        public Point GetGroupPinPosition(ICircuitGroupType<TElement, TWire, TPin> group, ICircuitGroupPin<TElement> groupPin, bool inputSide, D2dGraphics g)
        {

            if (inputSide) // group pin box on the left edge
            {
                Point op = group.Bounds.Location;
                op.Y += GetPinOffset(group.Cast<TElement>(), groupPin.Index, true);
                return op;
            }
            else
            {
                ElementTypeInfo info = GetElementTypeInfo(group.Cast<TElement>(), g);
                Point ip = group.Bounds.Location;
                ip.X += info.Size.Width;
                ip.Y += GetPinOffset(group.Cast<TElement>(), groupPin.Index, false); ;
                return ip;

            }
        }

        /// <summary>
        /// Gets pin position in element local space</summary>
        /// <param name="element">Element</param>
        /// <param name="pinIndex">Pin index</param>
        /// <param name="inputSide">True if pin side is input, false for output side</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Pin position</returns>
        public Point GetPinPosition(TElement element, int pinIndex, bool inputSide, D2dGraphics g)
        {
            if (inputSide) // group pin box on the left edge
            {
                Point op = element.Bounds.Location;
                op.Y += GetPinOffset(element, pinIndex, true);
                if (m_pinDrawStyle == PinStyle.OnBorderFilled)
                    op.X -= m_pinSize / 2;
                return op;
            }
            else
            {
                ElementTypeInfo info = GetElementTypeInfo(element, g);
                Point ip = element.Bounds.Location;
                ip.X += info.Size.Width;
                if (m_pinDrawStyle == PinStyle.OnBorderFilled)
                    ip.X += m_pinSize / 2;
                ip.Y += GetPinOffset(element, pinIndex, false);
                return ip;

            }
        }

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="outputElement">Source element, or null</param>
        /// <param name="outputPin">Source pin, or null</param>
        /// <param name="inputElement">Destination element, or null</param>
        /// <param name="inputPin">Destination pin, or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="endPoint">Endpoint to substitute for source or destination (in client coords), if either is null</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(
            TElement outputElement,
            TPin outputPin,
            TElement inputElement,
            TPin inputPin,
            string label,
            Point endPoint,
            D2dGraphics g)
        {
            if (inputElement == null || inputPin == null)
            {
                // dragging from output to endPoint
                DrawWire(outputElement, outputPin, endPoint, true, g);
            }
            else if (outputElement == null || outputPin == null)
            {
                // dragging from input to endPoint
                DrawWire(inputElement, inputPin, endPoint, false, g);
            }
            else
            {
                // routed edge
                DrawWire(outputElement, outputPin, inputElement, inputPin, g, GetPen(outputPin));
            }
        }

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="outputElement">Source element, or null</param>
        /// <param name="outputPin">Source pin, or null</param>
        /// <param name="inputElement">Destination element, or null</param>
        /// <param name="inputPin">Destination pin, or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="startPoint">Start point, in client coordinates</param>
        /// <param name="endPoint">End point, in client coordinates</param>
        /// <param name="g">Graphics object</param>
        public override void DrawPartialEdge(
            TElement outputElement,
            TPin outputPin,
            TElement inputElement,
            TPin inputPin,
            string label,
            PointF startPoint,
            PointF endPoint,
            D2dGraphics g)
        {
            D2dBrush pen = outputPin != null ? GetPen(outputPin) : GetPen(inputPin);
            var inverse = g.Transform;
            inverse.Invert();

            PointF start = startPoint;
            PointF end = endPoint;
            if (outputPin == null)
            {
                start = Matrix3x2F.TransformPoint(inverse, startPoint);
            }

            if (inputPin == null)
            {
                end = Matrix3x2F.TransformPoint(inverse, endPoint);
            }

            DrawWire(g, pen, start.X, start.Y, end.X, end.Y, 0, null);
            DrawWire(g, pen, start.X, start.Y, end.X, end.Y, 0, null);
        }

        /// <summary>
        /// Gets the bounding rectangle of a circuit element in local space, which is the same as
        /// world space except for sub-circuits</summary>
        /// <param name="element">Element to get the bounds for</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Rectangle completely bounding the node</returns>
        public override RectangleF GetBounds(TElement element, D2dGraphics g)
        {
            RectangleF result = GetElementBounds(element, g);

            if (!string.IsNullOrEmpty(element.Name))
                // Add in the label at bottom. Keep in sync with CircuitEditingContext.Resize().
                result.Height += LabelHeight;

            return result;
        }


        /// <summary>
        /// Finds node and/or edge hit by the given rectangle</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="rect">Given rectangle in graph space</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given rectangle</returns>
        public override IEnumerable<object> Pick(
            IGraph<TElement, TWire, TPin> graph,
            RectangleF rect,
            D2dGraphics g)
        {
            var pickedNodes = base.Pick(graph, rect, g).ToArray(); // base version picks nodes only
            if (!RectangleSelectsWires || pickedNodes.Length > 0)
            {
                if (pickedNodes.Length == 1 && pickedNodes[0].Is<ICircuitGroupType<TElement, TWire, TPin>>())
                {
                    var pickedGroup = pickedNodes[0].Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                    var pickedSubItems = PickSubItems(pickedGroup, rect, g).ToArray();
                    if (pickedSubItems.Length > 0) // try to pick the sub-nodes expanded to the lowest level
                    {
                        return pickedSubItems;
                    }
                }
                return pickedNodes;
            }

            List<object> pickedEdges = new List<object>();

            // only marquee select edges if no nodes are selected
            if (RectangleSelectsWires)
            {
                foreach (TWire edge in graph.Edges)
                {
                    RectangleF edgeBounds = GetBounds(edge, g);
                    if (edgeBounds.IntersectsWith(rect))
                        pickedEdges.Add(edge);
                }
            }
            return pickedEdges;
        }

        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test in graph space</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public override GraphHitRecord<TElement, TWire, TPin> Pick(
            IGraph<TElement, TWire, TPin> graph, TWire priorityEdge, PointF p, D2dGraphics g)
        {
            return Pick(graph.Nodes.Reverse(), graph.Edges, priorityEdge, p, g);
        }


        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="nodes">Nodes to test, usually in reverse order of rendering</param>
        /// <param name="edges">Edges to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test in graph space</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public override GraphHitRecord<TElement, TWire, TPin> Pick(
            IEnumerable<TElement> nodes, IEnumerable<TWire> edges, TWire priorityEdge, PointF p, D2dGraphics g)
        {
            TElement pickedElement = null;
            TPin pickedInput = null;
            TPin pickedOutput = null;
            TWire pickedWire = null;

            if (priorityEdge != null &&
                PickEdge(priorityEdge, p, g))
            {
                pickedWire = priorityEdge;
            }
            else
            {
                foreach (TWire edge in edges)
                {
                    if (PickEdge(edge, p, g))
                    {
                        pickedWire = edge;
                        break;
                    }
                }
            }

            Point pickedInputPos = new Point();
            Point pickedOutputPos = new Point();
            foreach (TElement element in nodes)
            {
                if (Pick(element, g, p))
                {
                    if (pickedElement != null && element.Is<ICircuitGroupType<TElement, TWire, TPin>>())
                    {
                        bool higherPriority = true;
                        var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                        if (pickedElement.Is<ICircuitGroupType<TElement, TWire, TPin>>())
                        {
                            var pickedGroup = pickedElement.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                            if (group.Info.PickingPriority <= pickedGroup.Info.PickingPriority)
                                higherPriority = false;
                        }

                        if (group.Expanded && higherPriority)
                        {
                            pickedElement = element;
                            pickedInput = PickInput(element, g, p);
                            pickedOutput = PickOutput(element, g, p);
                        }
                    }
                    else if (pickedElement == null)
                    {
                        pickedElement = element;
                        pickedInput = PickInput(element, g, p);
                        pickedOutput = PickOutput(element, g, p);
                    }

                    if (pickedInput != null)
                        pickedInputPos = GetPinPosition(pickedElement, pickedInput.Index, true, g);
                    if (pickedOutput != null)
                        pickedOutputPos = GetPinPosition(pickedElement, pickedOutput.Index, false, g);
                }
            }

            DiagramLabel label = null;
            var subPick = new Pair<IEnumerable<TElement>, object>();
            var borderPart = new DiagramBorder(pickedElement);
            TPin pickedSubInput = null;
            TPin pickedSubOutput = null;

            if (pickedElement != null) // if an element is picked, further check if its label or expander is picked. They take priority over wire picking.
            {
                RectangleF bounds = GetElementBounds(pickedElement, g);
                var labelBounds = new RectangleF(
                    bounds.Left, bounds.Bottom + m_pinMargin, bounds.Width, m_rowSpacing);

                label = new DiagramLabel(
                      new Rectangle((int)labelBounds.Left, (int)labelBounds.Top, (int)labelBounds.Width, (int)labelBounds.Height),
                      TextFormatFlags.SingleLine | TextFormatFlags.HorizontalCenter);
                if (labelBounds.Contains(p))
                    return new GraphHitRecord<TElement, TWire, TPin>(pickedElement, label);


                if (pickedElement.Is<ICircuitGroupType<TElement, TWire, TPin>>())
                {
                    var hitRecord = PickExpander(pickedElement.Cast<ICircuitGroupType<TElement, TWire, TPin>>(), p, g);
                    if (hitRecord != null)
                        return hitRecord;

                    // check title bar
                    var titkeBar = new RectangleF(bounds.Left - m_theme.PickTolerance, bounds.Y, bounds.Width, TitleHeight);
                    if (titkeBar.Contains(p))
                        return new GraphHitRecord<TElement, TWire, TPin>(pickedElement, new DiagramTitleBar(pickedElement));


                    if (pickedWire == null && pickedOutput == null && pickedInput == null) // check border lastly
                    {
                        var border = new RectangleF(bounds.Left - m_theme.PickTolerance, bounds.Y, 2 * m_theme.PickTolerance, bounds.Height);
                        if (border.Contains(p))
                            borderPart.Border = DiagramBorder.BorderType.Left;
                        else
                        {
                            border.Offset(bounds.Width, 0);
                            if (border.Contains(p))
                                borderPart.Border = DiagramBorder.BorderType.Right;
                            else
                            {
                                border = new RectangleF(bounds.Left, bounds.Y - m_theme.PickTolerance, bounds.Width, 2 * m_theme.PickTolerance);
                                if (border.Contains(p))
                                    borderPart.Border = DiagramBorder.BorderType.Top;
                                else
                                {
                                    border.Offset(0, bounds.Height);
                                    if (border.Contains(p))
                                        borderPart.Border = DiagramBorder.BorderType.Bottom;
                                }
                            }
                        }
                    }

                    subPick = PickSubItem(pickedElement.Cast<ICircuitGroupType<TElement, TWire, TPin>>(), p, g,
                        out pickedSubInput, out pickedSubOutput);
                }
            }

            if (pickedSubInput != null)
            {
                pickedInput = pickedSubInput;
                pickedInputPos = GetPinPosition(subPick.First.First().Cast<TElement>(), pickedSubInput.Index, true, g);
                pickedInputPos.Offset(ParentWorldOffset(subPick.First));
            }
            if (pickedSubOutput != null)
            {
                pickedOutput = pickedSubOutput;
                pickedOutputPos = GetPinPosition(subPick.First.First().Cast<TElement>(), pickedSubOutput.Index, false, g);
                pickedOutputPos.Offset(ParentWorldOffset(subPick.First));
            }

            if (pickedWire != null && pickedElement != null &&
                pickedInput == null && pickedOutput == null && // favor picking pin over wire, need to check no pin is picked
                pickedSubInput == null && pickedSubOutput == null)
            {
                // favor picking wire over element, but we don't want to select wires that are hidden by the element
                // a rough check here probably should suffice most of the time: the bounds of the wire should not be enclosed by the picked node
                var elementBounds = GetElementBounds(pickedElement, g);
                var curveBounds = GetWireBounds(pickedWire, g);
                if (!elementBounds.Contains(curveBounds))
                    return new GraphHitRecord<TElement, TWire, TPin>(null, pickedWire, null, null);
            }

            var result = new GraphHitRecord<TElement, TWire, TPin>(pickedElement, pickedWire, pickedOutput, pickedInput);
            if (pickedElement != null && pickedWire == null && pickedOutput == null && pickedInput == null)
                result.Part = borderPart.Border == DiagramBorder.BorderType.None ? null : borderPart;
            result.DefaultPart = label;
            result.SubItem = subPick.First == null ? null : subPick.First.First();
            result.SubPart = subPick.Second;
            result.ToRoutePos = pickedInputPos;
            result.FromRoutePos = pickedOutputPos;

            if (subPick.Second.Is<TWire>())
                result.SubItem = subPick.Second; // if a sub-edge is picked

            result.HitPathInversed = subPick.First;
            return result;
        }

        /// <summary>
        /// Tests if edge is hit by point</summary>
        /// <param name="edge">Edge to test</param>
        /// <param name="p">Point to test</param>
        /// <param name="g">D2dGraphics object</param>
        /// <param name="xOffset">optional x offset added to edge</param>
        /// <param name="yOffset">optional y offset added to edge</param>
        /// <returns>True iff edge hits point</returns>
        protected virtual bool PickEdge(TWire edge, PointF p, D2dGraphics g, float xOffset = 0, float yOffset = 0)
        {
            ElementTypeInfo fromInfo = GetElementTypeInfo(edge.FromNode, g);
            TPin inputPin = edge.ToRoute;
            TPin outputPin = edge.FromRoute;

            Point p1 = edge.FromNode.Bounds.Location;
            float x1 = p1.X + fromInfo.Size.Width + xOffset;
            float y1 = p1.Y + GetPinOffset(edge.FromNode, outputPin.Index, false) + yOffset;

            Point p2 = edge.ToNode.Bounds.Location;
            float x2 = p2.X + xOffset; ;
            float y2 = p2.Y + GetPinOffset(edge.ToNode, inputPin.Index, true) + yOffset;

            float tanLen = GetTangentLength(x1, x2);

            BezierCurve2F curve = new BezierCurve2F(
                new Vec2F(x1, y1),
                new Vec2F(x1 + tanLen, y1),
                new Vec2F(x2 - tanLen, y2),
                new Vec2F(x2, y2));

            Vec2F hitPoint = new Vec2F();
            return BezierCurve2F.Pick(curve, new Vec2F(p.X, p.Y), m_theme.PickTolerance, ref hitPoint);
        }


        private void theme_Redraw(object sender, EventArgs e)
        {
            m_elementTypeCache.Clear(); // invalidate cached info
            OnRedraw();
        }

        private void Draw(TElement element, D2dGraphics g, bool outline)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            Point p = element.Bounds.Location;
            p.Offset(WorldOffset(m_graphPath));

            RectangleF bounds = new RectangleF(p, info.Size);
            ICircuitElementType type = element.Type;

            bool groupExpanded = false;
            ICircuitGroupType<TElement, TWire, TPin> group = null;
            if (element.Is<ICircuitGroupType<TElement, TWire, TPin>>())
            {
                group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                groupExpanded = group.Expanded;
            }

            // Check if the user is actively connecting wires. If there are no connections that can
            //  be made to this element, then draw it as a ghost.
            if (RouteConnecting != null)
            {
                bool atLeastOneValidPin = false;
                var allPins = groupExpanded ?
                    group.Inputs.Concat(group.Info.HiddenInputPins).Concat(group.Outputs).Concat(group.Info.HiddenOutputPins)
                    : type.Inputs.Concat(type.Outputs);
                foreach (TPin pin in allPins)
                {
                    EdgeRouteDrawMode pinDrawMode = GetEdgeRouteDrawMode(element, pin);
                    if (pinDrawMode != EdgeRouteDrawMode.CannotConnect)
                    {
                        atLeastOneValidPin = true;
                        break;
                    }
                }
                if (!atLeastOneValidPin)
                {
                    DrawGhost(element, g);
                    if (!groupExpanded)
                        return;
                }
            }

            float scaleX = g.Transform.M11; // assume no rotation.
            bool useRoundedRect = RoundedBorder ? (scaleX * m_elementBody.RadiusX) > 3.0f : false;
            bool drawPins = !groupExpanded && (scaleX * m_pinSize) > DetailsThresholdSize;
            bool drawText = (m_theme.TextFormat.FontHeight * scaleX) > DetailsThresholdSize;
            int titleHeight = TitleHeight;

            m_elementBody.Rect = bounds;

            var titleRect = new RectangleF(bounds.X, bounds.Y, bounds.Width, titleHeight);

            var fillBrush = m_theme.GetCustomOrDefaultBrush(type.Name);
            var gradientBrush = fillBrush as D2dLinearGradientBrush;
            if (gradientBrush != null)
            {
                gradientBrush.StartPoint = bounds.Location;
                gradientBrush.EndPoint = new PointF(bounds.Left, bounds.Bottom);
            }
            else
                fillBrush = m_theme.FillBrush;

            if (useRoundedRect)
            {
                g.FillRoundedRectangle(m_elementBody, fillBrush);
                if (TitleBackgroundFilled)
                {
                    var titleBody = m_elementBody;
                    titleBody.Rect = new RectangleF(bounds.X, bounds.Y, bounds.Width, titleHeight);
                    g.FillRoundedRectangle(titleBody, m_theme.GetFillTitleBrush(element.Type.Name));
                    // We don't want the lower part of the title rect rounded. D2d does not support half-rounded rect, fill again
                    var lowerRect = new RectangleF(bounds.X, bounds.Y + titleHeight * 0.5f, bounds.Width, titleHeight * 0.5f);
                    g.FillRectangle(lowerRect, m_theme.GetFillTitleBrush(element.Type.Name));

                    g.DrawRoundedRectangle(m_elementBody, m_theme.GetFillTitleBrush(element.Type.Name));
                }
                if (outline)
                    g.DrawRoundedRectangle(m_elementBody, m_theme.OutlineBrush);
            }
            else
            {
                g.FillRectangle(bounds, fillBrush);
                if (TitleBackgroundFilled)
                {
                    g.FillRectangle(titleRect, m_theme.GetFillTitleBrush(element.Type.Name));
                    g.DrawRectangle(bounds, m_theme.GetFillTitleBrush(element.Type.Name), 1);
                }
                if (outline)
                    g.DrawRectangle(bounds, m_theme.OutlineBrush);
            }

            // draw the separate line between title and content
            if (!TitleBackgroundFilled && 
                (info.Size.Height > TitleHeight + 2 * m_pinMargin)) // check non-empty content
                g.DrawLine(p.X, p.Y + titleHeight, p.X + info.Size.Width, p.Y + titleHeight, m_theme.OutlineBrush);

            if (drawPins)
            {
                int pinY = titleHeight + m_pinMargin;
                foreach (TPin inputPin in type.Inputs)
                {
                    EdgeRouteDrawMode pinDrawMode = GetEdgeRouteDrawMode(element, inputPin);
                    if (pinDrawMode != EdgeRouteDrawMode.CannotConnect)
                    {
                        var pen = GetPen(inputPin);
                        if (PinDrawStyle == PinStyle.Default)
                            g.DrawRectangle(new RectangleF(p.X, p.Y + pinY + m_pinOffset, m_pinSize, m_pinSize), pen, 1.0f);
                        else if (PinDrawStyle == PinStyle.OnBorderFilled)
                            g.FillRectangle(new RectangleF(p.X - m_pinSize * 0.5f, p.Y + pinY + m_pinOffset, m_pinSize, m_pinSize), pen);
                        var pinText = inputPin.Name;
                        if (!groupExpanded)
                            pinText = TruncatePinText(pinText);

                        g.DrawText(pinText, m_theme.TextFormat,
                                   new PointF(p.X + m_pinSize + m_pinMargin, p.Y + pinY), m_theme.TextBrush);
                    }
                    pinY += m_rowSpacing;
                }

                pinY = titleHeight + m_pinMargin;
                int i = 0;
                foreach (TPin outputPin in type.Outputs)
                {
                    EdgeRouteDrawMode pinDrawMode = GetEdgeRouteDrawMode(element, outputPin);
                    if (pinDrawMode != EdgeRouteDrawMode.CannotConnect)
                    {
                        var pen = GetPen(outputPin);
                        if (PinDrawStyle == PinStyle.Default)
                            g.DrawRectangle(new RectangleF(bounds.Right - m_pinSize, p.Y + pinY + m_pinOffset, m_pinSize, m_pinSize), pen, 1.0f);
                        else if (PinDrawStyle == PinStyle.OnBorderFilled)
                            g.FillRectangle(new RectangleF(bounds.Right - m_pinSize * 0.5f, p.Y + pinY + m_pinOffset, m_pinSize, m_pinSize), pen);

                        var pinText = outputPin.Name;
                        if (!groupExpanded)
                            pinText = TruncatePinText(pinText);

                        g.DrawText(pinText, m_theme.TextFormat, new PointF(p.X + info.OutputLeftX[i], p.Y + pinY),
                                   m_theme.TextBrush);
                    }
                    pinY += m_rowSpacing;
                    i++;
                }
            }

            Image image = type.Image;
            if (image != null)
            {
                var bitMap = m_theme.GetBitmap(type);
                if (bitMap == null)
                {
                    m_theme.RegisterBitmap(type, image);
                    bitMap = m_theme.GetBitmap(type);
                }
                if (bitMap != null)
                    g.DrawBitmap(bitMap, new RectangleF(p.X + info.Interior.X, p.Y + info.Interior.Y, info.Interior.Width, info.Interior.Height), 1, D2dBitmapInterpolationMode.Linear);

            }

            if (drawText)
            {
                string title = GetElementTitle(element);
                if (!string.IsNullOrEmpty(title))
                {
                    var halignment = m_theme.TextFormat.TextAlignment;
                    var valignment = m_theme.TextFormat.ParagraphAlignment;

                    m_theme.TextFormat.TextAlignment = D2dTextAlignment.Center;
                    m_theme.TextFormat.ParagraphAlignment = D2dParagraphAlignment.Center;

                    int titleOffset = 0;
                    if (group != null)
                        titleOffset += 2 * ExpanderSize;
                    RectangleF textRect = new RectangleF(titleRect.X + titleOffset, titleRect.Y, titleRect.Width - titleOffset, titleRect.Height);
                    g.DrawText(title, m_theme.TextFormat, textRect, m_theme.TextBrush);

                    m_theme.TextFormat.TextAlignment = halignment;
                    m_theme.TextFormat.ParagraphAlignment = valignment;
                }

                string displayName = GetElementDisplayName(element);
                if (!string.IsNullOrEmpty(displayName))
                {

                    RectangleF alignRect = new RectangleF(bounds.Left - MaxNameOverhang, bounds.Bottom + m_pinMargin,
                        bounds.Width + 2 * MaxNameOverhang, m_rowSpacing);
                    var textAlignment = m_theme.TextFormat.TextAlignment;
                    m_theme.TextFormat.TextAlignment = D2dTextAlignment.Center;
                    g.DrawText(displayName, m_theme.TextFormat, alignRect, m_theme.TextBrush);
                    m_theme.TextFormat.TextAlignment = textAlignment;

                }
            }

            if (element.Is<IReference<TElement>>())
                g.DrawLink(p.X + bounds.Width - 2 * ExpanderSize, p.Y + 2 * m_pinMargin + 1, ExpanderSize, m_theme.HotBrush);

            if (group != null)
            {
                RectangleF expanderRect = GetExpanderRect(p);
                g.DrawExpander(expanderRect.X, expanderRect.Y, expanderRect.Width, m_theme.OutlineBrush, group.Expanded);

                if (group.Expanded)
                {
                    DrawExpandedGroup(element, g);
                }
            }
        }

        /// <summary>
        /// Truncates very long pin names into the form "prefix...suffix" where prefix and suffix are the 
        /// beginning and ending substrings of the original name.
        /// </summary>
        /// <param name="pinText">Original pin name</param>
        /// <returns>Original pin name if it is less than MaxCollapsedGroupPinNameLength, otherwise the 
        /// truncated version</returns>
        private string TruncatePinText(string pinText)
        {
            if (pinText.Length < MaxCollapsedGroupPinNameLength) return pinText;

            var sb = new StringBuilder();
            sb.Append(pinText.Substring(0, m_truncatedPinNameSubstringLength));
            sb.Append("...");
            sb.Append(pinText.Substring(pinText.Length - m_truncatedPinNameSubstringLength));
            return sb.ToString();
        }

        /// <summary>
        /// Draws expanded group</summary>
        /// <param name="element">Group to draw</param>
        /// <param name="g">D2dGraphics object</param>
        protected void DrawExpandedGroup(TElement element, D2dGraphics g)
        {
            m_graphPath.Push(element);
            var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
            DrawExpandedGroupPins(element, g);

            // ensure to draw the drag target first,  prevent it from hiding the drag sources 
            var subNodes = group.SubNodes.ToArray();
            for (int i = 0; i < subNodes.Count(); ++i)
            {
                var subNode = subNodes[i];
                DiagramDrawingStyle customStyle = GetCustomStyle(subNode);
                if (customStyle == DiagramDrawingStyle.DropTarget)
                {
                    subNodes[i] = subNodes[0];
                    subNodes[0] = subNode;
                }
            }

            foreach (var subNode in subNodes)
            {
                DiagramDrawingStyle customStyle = GetCustomStyle(subNode);
                if (customStyle == DiagramDrawingStyle.None)
                {
                    if (GetStyle != null)
                        Draw(subNode, GetStyle(subNode), g);
                    else
                        Draw(subNode, g, false);
                }
                else
                    Draw(subNode, customStyle, g);
            }

            // draw sub-edges after recursively draw sub-nodes
            foreach (var subEdge in group.SubEdges)
            {
                var style = GetStyle(subEdge);
                Draw(subEdge, style, g);
            }

            m_graphPath.Pop();
        }

        /// <summary>
        /// Gets the pin drawing mode for given element and its pin</summary>
        /// <param name="element">Element to get the pin drawing mode for</param>
        /// <param name="pin">Pin to get the pin drawing mode for</param>
        /// <returns>Drawing mode of edge routes</returns>
        public override EdgeRouteDrawMode GetEdgeRouteDrawMode(TElement element, TPin pin)
        {
            // check if the user is actively connecting wires
            RouteConnectingInfo info = RouteConnecting;
            if (info != null)
            {
                // display the start node and pin prominently
                if (info.StartNode == element &&
                    info.StartRoute == pin)
                    return EdgeRouteDrawMode.CanConnect;

                return info.EditableGraph.CanConnect(info.StartNode, info.StartRoute, element, pin) ?
                    EdgeRouteDrawMode.CanConnect :
                    EdgeRouteDrawMode.CannotConnect;
            }

            return EdgeRouteDrawMode.Normal;
        }

        /// <summary>
        /// Returns if the given element is picked by the given point</summary>
        /// <param name="element">Element to test</param>
        /// <param name="g">D2dGraphics object</param>
        /// <param name="p">Point to test in graph space</param>
        /// <returns>True iff given element is picked by given point</returns>
        protected virtual bool Pick(TElement element, D2dGraphics g, PointF p)
        {
            RectangleF bounds = GetBounds(element, g);
            int pickTolerance = m_theme.PickTolerance;
            bounds.Inflate(pickTolerance, pickTolerance);
            return bounds.Contains(p.X, p.Y);
        }


        private void DrawExpandedGroupPins(TElement element, D2dGraphics g)
        {
            var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
            if (group.Info.ShowExpandedGroupPins)
            {
                Point subOffset = SubGraphOffset(element);

                int x1, y1;
                foreach (var pin in group.Inputs)
                {
                    var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                    // draw group pin box on the left edge
                    Point op = group.Bounds.Location;
                    op.Offset(ParentWorldOffset(m_graphPath));
                    x1 = op.X;
                    y1 = op.Y + grpPin.Bounds.Location.Y + m_groupPinExpandedOffset + subOffset.Y;
                    g.DrawRectangle(new RectangleF(x1 - m_pinSize / 2, y1 - m_pinSize / 2, m_pinSize, m_pinSize),
                                    grpPin.Info.Color, 1.0f, null);

                    // draw virtual link from the subnode to the group pin box when there are incoming wires
                    if (!grpPin.Info.ExternalConnected && CircuitDefaultStyle.ShowVirtualLinks)
                    {
                        Point ip = grpPin.InternalElement.Bounds.Location;
                        ip.Offset(WorldOffset(m_graphPath));
                        int y2 = ip.Y + GetPinOffset(grpPin.InternalElement, grpPin.InternalPinIndex, true);
                        int x2 = ip.X;

                        DrawWire(g, SubGraphPinBrush, x1, y1, x2, y2, 1.0f, null);
                        //DrawLine(g, SubGraphPinBrush, x1, y1, x2, y2, 1.0f, null);
                    }

                }

                foreach (var pin in group.Outputs)
                {
                    var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                    ElementTypeInfo info = GetElementTypeInfo(element, g);

                    // draw group pin box on the right edge
                    Point op = group.Bounds.Location;
                    op.Offset(ParentWorldOffset(m_graphPath));
                    x1 = op.X + info.Size.Width;
                    y1 = op.Y + grpPin.Bounds.Location.Y + m_groupPinExpandedOffset + subOffset.Y;

                    g.DrawRectangle(new RectangleF(x1 - m_pinSize / 2, y1 - m_pinSize / 2, m_pinSize, m_pinSize),
                                    grpPin.Info.Color, 1.0f, null);

                    // draw virtual link from the subnode to the group pin box when there are outgoing wires
                    if (!grpPin.Info.ExternalConnected && CircuitDefaultStyle.ShowVirtualLinks)
                    {
                        info = GetElementTypeInfo(grpPin.InternalElement, g);
                        Point ip = grpPin.InternalElement.Bounds.Location;
                        ip.Offset(WorldOffset(m_graphPath));
                        int y2 = ip.Y + GetPinOffset(grpPin.InternalElement, grpPin.InternalPinIndex, false);
                        int x2 = ip.X + info.Size.Width;

                        DrawWire(g, SubGraphPinBrush, x2, y2, x1, y1, 1.0f, null);
                        //DrawLine(g, SubGraphPinBrush, x1, y1, x2, y2, 1.0f, null);
                    }
                }
            }
        }

        private void DrawGhost(TElement element, D2dGraphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            Point p = element.Bounds.Location;
            p.Offset(WorldOffset(m_graphPath));
            Rectangle bounds = new Rectangle(p, info.Size);
            m_elementBody.Rect = bounds;
            if (RoundedBorder)
                g.FillRoundedRectangle(m_elementBody, m_theme.GhostBrush);
            else
                g.FillRectangle(bounds, m_theme.GhostBrush);
        }

        private void DrawOutline(TElement element, D2dBrush pen, D2dGraphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            Point p = element.Bounds.Location;
            RectangleF bounds = new RectangleF(p, info.Size);
            bounds.Offset(WorldOffset(m_graphPath));
            bounds.Inflate(1, 1);

            float scaleX = g.Transform.M11; // assume no rotation.
            bool useRoundedRect = RoundedBorder ? (scaleX * m_elementBody.RadiusX) > 3.0f : false;
            if (useRoundedRect)
            {
                m_elementBody.Rect = bounds;
                g.DrawRoundedRectangle(m_elementBody, pen, 2);
            }
            else
            {
                g.DrawRectangle(bounds, pen, 2);
            }
        }

        /// <summary>
        /// Find input pin, if any, at given point</summary>
        /// <param name="element">Element</param>
        /// <param name="g">Graphics object</param>
        /// <param name="p">Point to test, in world space</param>
        /// <returns>Input pin hit by p; null otherwise</returns>
        private TPin PickInput(TElement element, D2dGraphics g, PointF p)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            return PickPin(element, true, element.Bounds.X, element.Bounds.Location.Y, info, p);
        }

        /// <summary>
        /// Find output pin, if any, at given point</summary>
        /// <param name="element">Element containing pin</param>
        /// <param name="g">Graphics object</param>
        /// <param name="p">Point to test, in world space</param>
        /// <returns>Output pin hit by p; null otherwise</returns>
        private TPin PickOutput(TElement element, D2dGraphics g, PointF p)
        {
            //info.Size is larger than element.Bounds and is more accurate (?)
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            int pinX;
            if (RouteConnecting == null)
            {
                if (PinDrawStyle == PinStyle.OnBorderFilled)
                    pinX = element.Bounds.X + info.Size.Width - m_pinSize / 2;
                else
                    pinX = element.Bounds.X + info.Size.Width - m_pinSize;
            }
            else
                pinX = element.Bounds.X + info.Size.Width / 2;
            return PickPin(element, false, pinX, element.Bounds.Location.Y, info, p);
        }

        private TPin PickPin(TElement element, bool inputSide, int pinX, int elementY, ElementTypeInfo info, PointF p)
        {
            int pickTolerance = m_theme.PickTolerance;
            var pins = inputSide ? element.Type.Inputs : element.Type.Outputs;
            if (RouteConnecting == null || // or for expanded group pins, 
               (element.Is<ICircuitGroupType<TElement, TWire, TPin>>() && element.Cast<ICircuitGroupType<TElement, TWire, TPin>>().Expanded))
            {
                // normal picking rectangles; the user may want to select the circuit element instead, for example
                int pinIndex = 0;
                foreach (TPin pin in pins)
                {
                    int y = elementY + GetPinOffset(element, pinIndex, inputSide);
                    var normalPinBounds = new RectangleF(pinX, y, m_pinSize, m_pinSize);
                    normalPinBounds.Inflate(pickTolerance, pickTolerance);
                    if (normalPinBounds.Contains(p.X, p.Y))
                        return pin;
                    ++pinIndex;
                }
            }
            else
            {
                // Use Extra-large picking rectangles for the pins since the user is actively making a connection.
                // info.Size.Width is larger (?) and more accurate for the width than element.Bounds.
                int pinIndex = 0;
                foreach (TPin pin in pins)
                {
                    int y = elementY + GetPinOffset(element, pinIndex, inputSide);
                    ++pinIndex;
                    EdgeRouteDrawMode drawMode = GetEdgeRouteDrawMode(element, pin);
                    if (drawMode == EdgeRouteDrawMode.CanConnect)
                    {
                        var currentBounds = new RectangleF(pinX, y, info.Size.Width, m_pinSize);
                        currentBounds.Inflate(pickTolerance, pickTolerance);
                        if (currentBounds.Contains(p.X, p.Y))
                            return pin;
                    }
                }

            }
            return null;
        }

        // Try to pick the expander on a group element recursively
        private GraphHitRecord<TElement, TWire, TPin> PickExpander(ICircuitGroupType<TElement, TWire, TPin> pickedElement, PointF p, D2dGraphics g)
        {
            var stack = new Stack<TElement>();
            stack.Push(pickedElement.Cast<TElement>());

            bool goDown = false;
            do
            {
                var current = stack.Peek();
                var location = current.Bounds.Location;
                location.Offset(ParentWorldOffset(stack));
                RectangleF expanderRect = GetExpanderRect(location);
                expanderRect.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
                if (expanderRect.Contains(p))
                {
                    var diagExapander = new DiagramExpander(expanderRect);

                    var result = new GraphHitRecord<TElement, TWire, TPin>(pickedElement.Cast<TElement>(), diagExapander);
                    if (stack.Count > 1)
                    {
                        result.SubItem = current;
                        result.HitPathInversed = stack;
                    }
                    return result;
                }

                var group = current.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                goDown = false;
                foreach (var child in group.SubNodes.AsIEnumerable<ICircuitGroupType<TElement, TWire, TPin>>())
                {
                    // only push in the first child that hits the point                  
                    var childElement = child.Cast<TElement>();
                    RectangleF bounds = GetBounds(childElement, g);
                    bounds.Offset(WorldOffset(stack));
                    bounds.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
                    if (bounds.Contains(p.X, p.Y))
                    {
                        stack.Push(child.Cast<TElement>());
                        goDown = true;
                        break;
                    }

                }
            } while (goDown);

            return null;
        }

        // Try to pick the child element of a group element and the pin of the child element recursively
        private Pair<IEnumerable<TElement>, object> PickSubItem(ICircuitGroupType<TElement, TWire, TPin> pickedElement, PointF p, D2dGraphics g,
            out TPin pickedSubInput, out TPin pickedSubOutput)
        {
            pickedSubInput = null;
            pickedSubOutput = null;
            var result = new Pair<IEnumerable<TElement>, object>();
            if (!pickedElement.Expanded)
                return result;

            var stack = new Stack<TElement>();
            stack.Push(pickedElement.Cast<TElement>());

            var subgroup_stack = new Stack<TElement>();
            subgroup_stack.Push(pickedElement.Cast<TElement>());
            TWire pickedWire = null;

            bool goDown;
            do
            {
                var current = stack.Peek();
                var group = current.Cast<ICircuitGroupType<TElement, TWire, TPin>>(); // the stack top must be a group if we can go down
                goDown = false;
                bool hitSubNode = false;
                PointF offset = WorldOffset(subgroup_stack);
                foreach (var child in group.SubNodes)
                {
                    // only push in the first child that hits the point                  
                    RectangleF bounds = GetBounds(child, g);
                    bounds.Offset(offset);
                    bounds.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
                    if (bounds.Contains(p.X, p.Y))
                    {
                        stack.Push(child.Cast<TElement>());
                        hitSubNode = true; // sub-node is hit 
                        if (child.Is<ICircuitGroupType<TElement, TWire, TPin>>())
                        {
                            subgroup_stack.Push(child.Cast<TElement>()); // update group hierarchy path 
                            // go down when the child is expanded
                            goDown = child.Cast<ICircuitGroupType<TElement, TWire, TPin>>().Expanded;
                            break;
                        }
                    }
                }
                if (!hitSubNode) // try picking sub-edges
                {
                    foreach (var subEdge in group.SubEdges)
                    {
                        if (PickEdge(subEdge, p, g, offset.X, offset.Y))
                        {
                            pickedWire = subEdge;
                            break;
                        }
                    }
                }
            } while (goDown);

            if (pickedWire != null)
            {
                result.First = stack;
                result.Second = pickedWire;
            }
            else if (stack.Peek() != pickedElement.Cast<TElement>())// stack top is the picked subitem
            {
                var subItem = stack.Peek();
                result.First = stack; // get hit path 

                // try pick subItem
                RectangleF bounds = GetElementBounds(subItem, g);
                bounds.Offset(ParentWorldOffset(stack));
                ElementTypeInfo info = GetElementTypeInfo(subItem, g);

                if (PinDrawStyle == PinStyle.OnBorderFilled)
                {
                    pickedSubInput = PickPin(subItem, true, (int)bounds.X - m_pinSize / 2, (int)bounds.Y, info, p);
                    pickedSubOutput = PickPin(subItem, false, (int)bounds.X + info.Size.Width - m_pinSize / 2, (int)bounds.Y, info, p);

                }
                else
                {
                    pickedSubInput = PickPin(subItem, true, (int)bounds.X, (int)bounds.Y, info, p);
                    pickedSubOutput = PickPin(subItem, false, (int)bounds.X + info.Size.Width - m_pinSize, (int)bounds.Y, info, p);

                }
                result.Second = pickedSubInput ?? pickedSubOutput;
                if (result.Second == null) // check border lastly
                {
                    var border = new RectangleF(bounds.Left - m_theme.PickTolerance, bounds.Y, 2 * m_theme.PickTolerance, bounds.Height);
                    if (border.Contains(p))
                        result.Second = new DiagramBorder(subItem, DiagramBorder.BorderType.Left);
                    else
                    {
                        border.Offset(bounds.Width, 0);
                        if (border.Contains(p))
                            result.Second = new DiagramBorder(subItem, DiagramBorder.BorderType.Right);
                        else
                        {
                            border = new RectangleF(bounds.Left, bounds.Y - m_theme.PickTolerance, bounds.Width, 2 * m_theme.PickTolerance);
                            if (border.Contains(p))
                                result.Second = new DiagramBorder(subItem, DiagramBorder.BorderType.Top);
                            else
                            {
                                border.Offset(0, bounds.Height);
                                if (border.Contains(p))
                                    result.Second = new DiagramBorder(subItem, DiagramBorder.BorderType.Bottom);
                            }
                        }
                    }
                }
            }
            return result;
        }


        // Try to pick the sub-nodes expanded to the (same) lowest level
        private IEnumerable<TElement> PickSubItems(ICircuitGroupType<TElement, TWire, TPin> pickedElement, RectangleF rect, D2dGraphics g)
        {
            if (!pickedElement.Expanded)
                return EmptyEnumerable<TElement>.Instance;

            var stack = new Stack<TElement>();
            stack.Push(pickedElement.Cast<TElement>());

            var subgroupStack = new Stack<TElement>();
            subgroupStack.Push(pickedElement.Cast<TElement>());

            bool goDown;
            do
            {
                var current = stack.Pop();
                var group = current.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                ICircuitGroupType<TElement, TWire, TPin> subGroup = null;
                goDown = false;
                int hitGroups = 0;
                foreach (var child in group.SubNodes)
                {
                    var visible = child.As<IVisible>();
                    if (visible != null && !visible.Visible)
                        continue;

                    // push children that hits the rect                  
                    RectangleF bounds = GetBounds(child, g);
                    bounds.Offset(WorldOffset(subgroupStack));
                    bounds.Inflate(m_theme.PickTolerance, m_theme.PickTolerance);
                    if (bounds.IntersectsWith(rect))
                    {
                        stack.Push(child.Cast<TElement>());
                        if (child.Is<ICircuitGroupType<TElement, TWire, TPin>>())
                        {
                            subgroupStack.Push(child.Cast<TElement>());
                            // go down when only one child is hit, and that child is an expanded group node
                            if (child.Cast<ICircuitGroupType<TElement, TWire, TPin>>().Expanded)
                            {
                                subGroup = child.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                                ++hitGroups;
                            }

                        }
                    }
                }

                if (hitGroups == 1)
                {
                    stack.Clear();
                    stack.Push(subGroup.Cast<TElement>());
                    goDown = true;
                }
            } while (goDown);
            return stack;
        }

        /// <summary>
        /// Obtains ElementTypeInfo for element</summary>
        /// <param name="element">Element to obtain ElementTypeInfo</param>
        /// <param name="g">D2dGraphics object</param>
        /// <returns>ElementTypeInfo for element</returns>
        protected ElementTypeInfo GetElementTypeInfo(TElement element, D2dGraphics g)
        {
            // look it up in the cache
            ICircuitElementType type = element.Type;
            string title = GetElementTitle(element);
            ElementTypeInfo cachedInfo;
            if (m_elementTypeCache.TryGetValue(type, out cachedInfo))
            {
                if (cachedInfo.numInputs != type.Inputs.Count || cachedInfo.numOutputs != type.Outputs.Count ||
                    cachedInfo.Title != title)
                {
                    m_elementTypeCache.Remove(type);
                    Invalidate(type);
                }
                else
                    return cachedInfo;
            }


            // not in cache, recompute
            if (element.Is<ICircuitGroupType<TElement, TWire, TPin>>())
            {
                var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                if (m_elementTypeCache.TryGetValue(group, out cachedInfo))
                {
                    if (cachedInfo.numInputs != group.Inputs.Count || cachedInfo.numOutputs != group.Outputs.Count ||
                        cachedInfo.Title != title)
                    {
                        m_elementTypeCache.Remove(group);
                        Invalidate(group);
                    }
                    else
                        return cachedInfo;
                }
                return GetHierarchicalElementTypeInfo(group, g);

            }

            ElementSizeInfo sizeInfo = GetElementSizeInfo(type, g, title);
            var info = new ElementTypeInfo
            {
                Title = title,
                Size = sizeInfo.Size,
                Interior = sizeInfo.Interior,
                OutputLeftX = sizeInfo.OutputLeftX.ToArray(),
                numInputs = type.Inputs.Count,
                numOutputs = type.Outputs.Count,
            };

            m_elementTypeCache.Add(type, info);

            return info;
        }

        /// <summary>Computes interior and exterior size as well as the x positions of output pins</summary>
        /// <param name="type">Circuit element type</param>
        /// <param name="g">Graphics that can be used for measuring strings</param>
        /// <param name="title">The string to use on the title bar of the circuit element</param>
        /// <returns>Element size info; cannot be null</returns>
        /// <remarks>Clients using customized rendering should override this method
        /// to adjust sizes accordingly. These sizes are used by drag-from picking.</remarks>
        protected virtual ElementSizeInfo GetElementSizeInfo(ICircuitElementType type, D2dGraphics g, string title = null)
        {
            SizeF typeNameSize = new SizeF();
            if (title != null)
                typeNameSize = g.MeasureText(title, m_theme.TextFormat);
            else
                g.MeasureText(type.Name, m_theme.TextFormat);

            int width = (int)typeNameSize.Width + 2 * m_pinMargin + 4 * ExpanderSize + 1;

            IList<ICircuitPin> inputPins = type.Inputs;
            IList<ICircuitPin> outputPins = type.Outputs;
            int inputCount = inputPins.Count;
            int outputCount = outputPins.Count;
            int minRows = Math.Min(inputCount, outputCount);
            int maxRows = Math.Max(inputCount, outputCount);

            bool isCollapsedGroup = false;
            if (type.Is<ICircuitGroupType<TElement, TWire, TPin>>())
            {
                // If this is a group, it must be collapsed, because expanded groups
                // are handled by GetHierarchicalElementSizeInfo.
                isCollapsedGroup = true;
            }

            int[] outputLeftX = new int[outputCount];

            int height = m_rowSpacing + 2 * m_pinMargin;
            height += Math.Max(
                maxRows * m_rowSpacing,
                minRows * m_rowSpacing + type.InteriorSize.Height - m_pinMargin);

            bool imageRight = true;
            for (int i = 0; i < maxRows; i++)
            {
                double rowWidth = 2 * m_pinMargin;
                if (inputCount > i)
                {
                    var pinText = inputPins[i].Name;
                    if (isCollapsedGroup)
                        pinText = TruncatePinText(pinText);
                    SizeF labelSize = g.MeasureText(pinText, m_theme.TextFormat);
                    rowWidth += labelSize.Width + m_pinSize + m_pinMargin;
                }
                else
                {
                    imageRight = false;
                }


                if (outputCount > i)
                {
                    var pinText = outputPins[i].Name;
                    if (isCollapsedGroup)
                        pinText = TruncatePinText(pinText);
                    SizeF labelSize = g.MeasureText(pinText, m_theme.TextFormat);
                    outputLeftX[i] = (int)labelSize.Width;
                    rowWidth += labelSize.Width + m_pinSize + m_pinMargin;
                }

                rowWidth += type.InteriorSize.Width;

                width = Math.Max(width, (int)rowWidth);
            }

            if (inputCount == outputCount)
                width = Math.Max(width, type.InteriorSize.Width + 2);

            width = Math.Max(width, MinElementWidth);
            height = Math.Max(height, MinElementHeight);

            Size size = new Size(width, height);
            Rectangle interior = new Rectangle(
                imageRight ? width - type.InteriorSize.Width : 1,
                height - type.InteriorSize.Height,
                type.InteriorSize.Width,
                type.InteriorSize.Height);

            for (int i = 0; i < outputLeftX.Length; i++)
                outputLeftX[i] = width - m_pinMargin - m_pinSize - outputLeftX[i];

            return new ElementSizeInfo(size, interior, outputLeftX);
        }

        private ElementTypeInfo GetHierarchicalElementTypeInfo(ICircuitGroupType<TElement, TWire, TPin> group, D2dGraphics g)
        {
            ElementSizeInfo sizeInfo = GetHierarchicalElementSizeInfo(group, g);

            var info = new ElementTypeInfo
            {
                Title = GetElementTitle(group.As<TElement>()),
                Size = sizeInfo.Size,
                Interior = sizeInfo.Interior,
                OutputLeftX = sizeInfo.OutputLeftX.ToArray(),
                numInputs = group.Inputs.Count,
                numOutputs = group.Outputs.Count,
            };

            m_elementTypeCache.Add(group, info);
            return info;
        }

        /// <summary>Computes interior and exterior size as well as the x positions of output pins for expanded group</summary>
        /// <param name="group">Group</param>
        /// <param name="g">Graphics object that can be used for measuring strings</param>
        /// <returns>Element size info; cannot be null</returns>
        protected virtual ElementSizeInfo GetHierarchicalElementSizeInfo(ICircuitGroupType<TElement, TWire, TPin> group, D2dGraphics g)
        {
            if (!group.Expanded) // group element collapsed, treat like non-Hierarchical node
                return GetElementSizeInfo(group, g, GetElementTitle(group.As<TElement>()));



            Rectangle grpBounds;
            if (group.AutoSize)
            {
                // measure size of the expanded group element 
                grpBounds = new Rectangle(); // always start with origin for sub-contents area
                foreach (var subNode in group.SubNodes)
                {

                    ElementSizeInfo sizeInfo = subNode.Is<ICircuitGroupType<TElement, TWire, TPin>>()
                                                   ? GetHierarchicalElementSizeInfo(
                                                       subNode.Cast
                                                           <ICircuitGroupType<TElement, TWire, TPin>>(),
                                                       g)
                                                   : GetElementSizeInfo(subNode.Type, g, GetElementTitle(subNode));

                    //if this is the first time through, then lets just set the location and size to
                    //be the current node. Then the box will take the minimum space to hold all of the subnodes.
                    if (grpBounds.Width == 0 && grpBounds.Height == 0)
                    {
                        grpBounds.Location = subNode.Bounds.Location;
                        grpBounds.Size = sizeInfo.Size;
                    }
                    else
                    {
                        grpBounds = Rectangle.Union(grpBounds, new Rectangle(subNode.Bounds.Location, sizeInfo.Size));
                    }
                }

                // compensate group pin positions
                int yMin = int.MaxValue;
                int yMax = int.MinValue;
                foreach (var pin in group.Inputs)
                {
                    var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                    if (grpPin.Bounds.Location.Y < yMin)
                        yMin = grpPin.Bounds.Location.Y;
                    if (grpPin.Bounds.Location.Y > yMax)
                        yMax = grpPin.Bounds.Location.Y;
                }
                if (yMin == int.MaxValue) // if no visible input pins
                    yMin = grpBounds.Y;

                foreach (var pin in group.Outputs)
                {
                    var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                    if (grpPin.Bounds.Location.Y < yMin)
                        yMin = grpPin.Bounds.Location.Y;
                    if (grpPin.Bounds.Location.Y > yMax)
                        yMax = grpPin.Bounds.Location.Y;
                }
                if (yMax == int.MinValue) // if no visible output pins
                    yMax = grpBounds.Y + grpBounds.Height;

                int width = grpBounds.Width + m_subContentOffset.X;
                int height = Math.Max(yMax - yMin, grpBounds.Height);
                height += LabelHeight; // compensate that the  bottom of sub-nodes may have non-empty names
                height += TitleHeight;

                if (group.Info.MinimumSize.Width > width)
                    width = group.Info.MinimumSize.Width;
                if (group.Info.MinimumSize.Height > height)
                    height = group.Info.MinimumSize.Height;


                grpBounds = Rectangle.Union(grpBounds,
                                            new Rectangle(grpBounds.Location.X, yMin, width, height));

            }
            else
            {
                grpBounds = group.Bounds;
            }


            // measure offset of right pins
            int outputCount = group.Outputs.Count();
            int[] grpOutputLeftX = new int[outputCount];
            for (int i = 0; i < grpOutputLeftX.Length; i++)
            {
                SizeF labelSize = g.MeasureText(group.Outputs[i].Name, m_theme.TextFormat);
                grpOutputLeftX[i] = grpBounds.Size.Width - m_pinMargin - m_pinSize - (int)labelSize.Width;

            }

            if (group.AutoSize)
            {
                bool includeHMargin = group.Info.MinimumSize.IsEmpty;
                bool includeVMargin = includeHMargin;
                if (!group.Info.MinimumSize.IsEmpty) // MinimumSize is set, 
                {
                    if (group.Info.MinimumSize.Width >= grpBounds.Size.Width)
                        includeHMargin = false;
                    if (group.Info.MinimumSize.Height >= grpBounds.Size.Height)
                        includeVMargin = false;
                }
                if (includeHMargin || includeVMargin)
                {
                    return new ElementSizeInfo(new Size(grpBounds.Size.Width + (includeHMargin ? m_subContentOffset.X : 0),
                                                        grpBounds.Size.Height + (includeVMargin ? m_subContentOffset.Y : 0)),
                                               grpBounds,
                                               grpOutputLeftX);
                }

                return new ElementSizeInfo(grpBounds.Size,
                                               grpBounds,
                                               grpOutputLeftX);
            }

            return new ElementSizeInfo(
                grpBounds.Size,
                new Rectangle(0, 0, 0, 0),
                grpOutputLeftX);

        }

        private void DrawWire(
            TElement outputElement,
            TPin outputPin,
            TElement inputElement,
            TPin inputPin,
            D2dGraphics g,
            D2dBrush pen)
        {
            ElementTypeInfo info = GetElementTypeInfo(outputElement, g);

            Point op = outputElement.Bounds.Location;
            op.Offset(WorldOffset(m_graphPath));
            int x1 = op.X + info.Size.Width;
            if (PinDrawStyle == PinStyle.OnBorderFilled)
                x1 += m_pinSize / 2;
            int y1 = op.Y + GetPinOffset(outputElement, outputPin.Index, false);

            Point ip = inputElement.Bounds.Location;
            ip.Offset(WorldOffset(m_graphPath));
            int x2 = ip.X;
            if (PinDrawStyle == PinStyle.OnBorderFilled)
                x2 -= m_pinSize / 2;
            int y2 = ip.Y + GetPinOffset(inputElement, inputPin.Index, true);

            DrawWire(g, pen, x1, y1, x2, y2, 0.0f, null);
        }

        private void DrawWire(
            TElement element,
            TPin pin,
            Point p,
            bool fromOutput,
            D2dGraphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);

            PointF ep = element.Bounds.Location;
            float x = ep.X;
            if (PinDrawStyle == PinStyle.OnBorderFilled)
                x += m_pinSize * 0.5f;
            float y = ep.Y + GetPinOffset(element, pin.Index, !fromOutput);
            if (fromOutput)
                x += info.Size.Width;

            var inverse = g.Transform;
            inverse.Invert();
            PointF end = Matrix3x2F.TransformPoint(inverse, p);

            var pen = GetPen(pin);
            if (fromOutput)
                DrawWire(g, pen, x, y, end.X, end.Y, 0, null);
            else
                DrawWire(g, pen, end.X, end.Y, x, y, 0, null);
        }

        /// <summary>Draw a wire</summary>
        /// <param name="g">Graphics object</param>
        /// <param name="pen">D2dBrush pen</param>
        /// <param name="x1">X-coordinate of starting point of wire</param>
        /// <param name="y1">Y-coordinate of starting point of wire</param>
        /// <param name="x2">X-coordinate of ending point of wire</param>
        /// <param name="y2">Y-coordinate of ending point of wire</param>
        /// <param name="strokeWidth">Stroke width; use 0 to get the default stroke width</param>
        /// <param name="strokeStyle">D2dStrokeStyle; use null to get default behavior</param>
        protected void DrawWire(D2dGraphics g, D2dBrush pen, float x1, float y1, float x2, float y2, float strokeWidth, D2dStrokeStyle strokeStyle)
        {
            float thickness = strokeWidth == 0 ? EdgeThickness : strokeWidth;
            float tanLen = GetTangentLength(x1, x2);
            g.DrawBezier(
                    new PointF(x1, y1),
                    new PointF(x1 + tanLen, y1),
                    new PointF(x2 - tanLen, y2),
                    new PointF(x2, y2),
                    pen, thickness, strokeStyle);
        }

        //private void DrawLine(D2dGraphics g, D2dBrush pen, float x1, float y1, float x2, float y2, float strokeWidth, D2dStrokeStyle strokeStyle)
        //{
        //    float thickness = strokeWidth == 0 ? EdgeThickness : strokeWidth;
        //    g.DrawLine(
        //            new PointF(x1, y1),
        //            new PointF(x2, y2),
        //            pen, thickness, strokeStyle);
        //}

        private float GetTangentLength(float x1, float x2)
        {
            const int minTanLen = 32;
            float tanLen = Math.Abs(x1 - x2) / 2;
            tanLen = Math.Max(tanLen, minTanLen);
            return tanLen;
        }

        /// <summary>
        /// Get pen for drawing pin</summary>
        /// <param name="pin">Pin to draw</param>
        /// <returns>Pen for drawing pin</returns>
        protected virtual D2dBrush GetPen(TPin pin)
        {
            D2dBrush pen = m_theme.GetCustomBrush(pin.TypeName);
            if (pen == null)
                pen = m_theme.GhostBrush;

            return pen;
        }

        /// <summary>
        /// Gets pin offset</summary>
        /// <param name="element">Element containing pin</param>
        /// <param name="pinIndex">Pin index</param>
        /// <param name="inputSide">True if pin side is input, false for output side</param>
        /// <returns>Pin offset</returns>
        public virtual int GetPinOffset(ICircuitElement element, int pinIndex, bool inputSide)
        {
            if (inputSide)
            {
                if (pinIndex < element.Type.Inputs.Count())
                {
                    var pin = element.Type.Inputs[pinIndex];
                    if (pin.Is<ICircuitGroupPin<TElement>>())
                    {
                        var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                        if (group.Expanded)
                        {
                            var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                            return grpPin.Bounds.Location.Y + m_groupPinExpandedOffset + group.Info.Offset.Y;
                        }
                    }
                }
            }
            else
            {
                if (pinIndex < element.Type.Outputs.Count())
                {
                    var pin = element.Type.Outputs[pinIndex];
                    if (pin.Is<ICircuitGroupPin<TElement>>())
                    {
                        var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                        if (group.Expanded)
                        {
                            var grpPin = pin.Cast<ICircuitGroupPin<TElement>>();
                            return grpPin.Bounds.Location.Y + m_groupPinExpandedOffset + group.Info.Offset.Y;
                        }
                    }
                }
            }

            return m_rowSpacing + 2 * m_pinMargin + pinIndex * m_rowSpacing + m_pinOffset + m_pinSize / 2;
        }

        private void SetPinSpacing()
        {
            m_pinMargin = m_theme.PinMargin;
            m_rowSpacing = m_theme.RowSpacing;
            m_pinOffset = m_theme.PinOffset;
            m_pinSize = m_theme.PinSize;
            m_groupPinExpandedOffset = 2 * m_rowSpacing;
            if (!m_subContentOffseExternalSet)
                m_subContentOffset = new Point(m_rowSpacing + 4 * m_pinMargin, m_rowSpacing + 4 * m_pinMargin);
        }

        // Get the element bounds, not including the space for the label below the element.
        private RectangleF GetElementBounds(TElement element, D2dGraphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            return new RectangleF(element.Bounds.Location, info.Size);
        }


        private RectangleF GetWireBounds(TWire wire, D2dGraphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(wire.FromNode, g);
            RectangleF result = new RectangleF();
            if (wire.Is<IEdgeStyleProvider>())
            {
                bool firstTime = true;
                foreach (var edgeInfo in wire.Cast<IEdgeStyleProvider>().GetData(this, WorldOffset(m_graphPath), g))
                {
                    if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.Bezier)
                    {
                        var curve = edgeInfo.EdgeData.As<BezierCurve2F>();
                        var cpts = new PointF[] { curve.P1, curve.P2, curve.P3, curve.P4 };
                        if (firstTime)
                        {
                            result = GetPointsBounds(cpts);
                            firstTime = false;
                        }
                        else
                            result = RectangleF.Union(result, GetPointsBounds(cpts));
                    }
                    else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.Line)
                    {
                        var line = edgeInfo.EdgeData.As<PointF[]>();
                        if (line != null)
                        {
                            if (firstTime)
                            {
                                result = D2dUtil.MakeRectangle(line[0], line[1]);
                                firstTime = false;
                            }
                            else
                                result = RectangleF.Union(result, D2dUtil.MakeRectangle(line[0], line[1]));
                        }

                    }
                    else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.Polyline)
                    {
                        var lines = edgeInfo.EdgeData.As<PointF[]>();
                        if (lines != null)
                        {
                            if (firstTime)
                            {
                                result = GetPointsBounds(lines);
                                firstTime = false;
                            }
                            else
                                result = RectangleF.Union(result, GetPointsBounds(lines));
                        }
                    }
                    else if (edgeInfo.ShapeType == EdgeStyleData.EdgeShape.BezierSpline)
                    {
                        var curves = edgeInfo.EdgeData.As<IEnumerable<BezierCurve2F>>();

                        foreach (var curve in curves)
                        {
                            var cpts = new PointF[] { curve.P1, curve.P2, curve.P3, curve.P4 };
                            if (firstTime)
                            {
                                result = GetPointsBounds(cpts);
                                firstTime = false;
                            }
                            else
                                result = RectangleF.Union(result, GetPointsBounds(cpts));
                        }
                    }

                }
            }
            else
            {
                Point op = wire.FromNode.Bounds.Location;
                op.Offset(WorldOffset(m_graphPath));
                int x1 = op.X + info.Size.Width;
                if (PinDrawStyle == PinStyle.OnBorderFilled)
                    x1 += m_pinSize / 2;
                int y1 = op.Y + GetPinOffset(wire.FromNode, wire.FromRoute.Index, false);

                Point ip = wire.ToNode.Bounds.Location;
                ip.Offset(WorldOffset(m_graphPath));
                int x2 = ip.X;
                if (PinDrawStyle == PinStyle.OnBorderFilled)
                    x2 -= m_pinSize / 2;
                int y2 = ip.Y + GetPinOffset(wire.ToNode, wire.ToRoute.Index, true);
                result = D2dUtil.MakeRectangle(new PointF(x1, y1), new PointF(x2, y2));
            }

            return result;
        }

        private RectangleF GetPointsBounds(IEnumerable<PointF> points)
        {
            float minX = points.Min(p => p.X);
            float minY = points.Min(p => p.Y);
            float maxX = points.Max(p => p.X);
            float maxY = points.Max(p => p.Y);

            return new RectangleF(new PointF(minX, minY), new SizeF(maxX - minX, maxY - minY));
        }

        /// <summary>
        /// Gets expander rectangle</summary>
        /// <param name="p">Upper-left corner point of expander</param>
        /// <returns>Expander rectangle</returns>
        protected RectangleF GetExpanderRect(PointF p)
        {
            return new RectangleF(p.X + m_pinMargin + 1, p.Y + 2 * m_pinMargin + 1, ExpanderSize, ExpanderSize);
        }

        /// <summary>
        /// Gets or sets margin offset to be added to draw all sub-elements when the group is expanded inline</summary>
        public Point SubContentOffset
        {
            get { return m_subContentOffset; }
            set
            {
                m_subContentOffset = value;
                m_subContentOffseExternalSet = true;
            }
        }

        /// <summary>
        /// Gets or sets vertical (Y) offset to be added to draw group pins on the border of the group 
        /// when the group is expanded inline</summary>
        public int GroupPinExpandedOffset
        {
            get { return m_groupPinExpandedOffset; }
            set { m_groupPinExpandedOffset = value; }
        }

        /// <summary>
        /// Gets title height at the top of an element</summary>
        public int TitleHeight
        {
            get { return m_rowSpacing + m_pinMargin; }
        }

        /// <summary>
        /// Gets label height at the bottom of an element</summary>
        public int LabelHeight
        {
            get { return m_rowSpacing + m_pinMargin; }
        }

        // The upper-left corner of the sub-nodes and floating pinYs defines the origin offset of sub-contents
        // relative to the containing group. This offset is used when expanding groups, 
        // so that the contained sub-nodes are drawn within the expanded space
        private Point SubGraphOffset(TElement element)
        {
            Point offset = Point.Empty;
            if (element.Is<ICircuitGroupType<TElement, TWire, TPin>>())
            {
                var group = element.Cast<ICircuitGroupType<TElement, TWire, TPin>>();
                offset = group.Info.Offset;
            }

            return offset;
        }

        /// <summary>
        /// Accumulated world offset of given drawing stack</summary>
        /// <param name="graphPath">Elements in drawing stack</param>
        /// <returns>Accumulated world offset of elements</returns>
        public Point WorldOffset(IEnumerable<TElement> graphPath)
        {

            Point offset = new Point();
            Point nodeDelta = Point.Empty;
            foreach (var element in graphPath)
            {
                nodeDelta = SubGraphOffset(element);

                offset.X += element.Bounds.Location.X + nodeDelta.X;
                offset.Y += element.Bounds.Location.Y + nodeDelta.Y;

                // nested subgraph contents  offset
                offset.Offset(m_subContentOffset);
            }

            return offset;

        }

        /// <summary>
        /// Accumulated world offset of current drawing stack</summary>
        public Point CurrentWorldOffset
        {
            get { return WorldOffset(m_graphPath); }
        }

        /// <summary>
        /// Gets or sets the maximum length of a pin name on a collapsed circuit group. If a pin name is longer than
        /// this when the group is collapsed, then the pin name will be truncated and shown with an ellipsis. The
        /// default is 25 characters and the minimum is 5 characters.</summary>
        public int MaxCollapsedGroupPinNameLength
        {
            get { return m_maxCollapsedGroupPinNameLength; }
            set
            {
                if (value < 5)
                    throw new ArgumentOutOfRangeException("the minimum value is 5");
                m_maxCollapsedGroupPinNameLength = value;
                m_truncatedPinNameSubstringLength = (value - 3) / 2;
            }
        }

        /// <summary>
        /// accumulated offset of next to top drawing stack
        /// </summary>
        private Point ParentWorldOffset(IEnumerable<TElement> graphPath)
        {

            var offset = new Point();
            Point node_delta = Point.Empty;

            foreach (var element in graphPath.Skip(1)) // m_graphPath is a stack, top item is the most nested 
            {
                node_delta = SubGraphOffset(element);

                offset.X += element.Bounds.Location.X + node_delta.X;
                offset.Y += element.Bounds.Location.Y + node_delta.Y;

                // nested subgraph contents  offset
                int margin = m_rowSpacing + 4 * m_pinMargin;
                offset.Offset(margin, margin);
            }

            return offset;

        }

        private void DocumentRegistryOnDocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> itemRemovedEventArgs)
        {
            m_cachePerDocument.Remove(itemRemovedEventArgs.Item);
        }

        private void DocumentRegistryOnActiveDocumentChanging(object sender, EventArgs eventArgs)
        {
            if (m_documentRegistry.ActiveDocument != null)
                m_cachePerDocument[m_documentRegistry.ActiveDocument] = m_elementTypeCache;
        }

        private void DocumentRegistryOnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            if (m_documentRegistry.ActiveDocument == null ||
                !m_cachePerDocument.TryGetValue(m_documentRegistry.ActiveDocument, out m_elementTypeCache))
            {
                m_elementTypeCache = new Dictionary<ICircuitElementType, ElementTypeInfo>();
            }
        }

        #region Private Classes

        /// <summary>
        /// Class to hold cached element type layout, in pixels (or DIPs)</summary>
        protected class ElementTypeInfo
        {
            /// <summary>Element title</summary>
            public string Title;
            /// <summary>Element size</summary>
            public Size Size;
            /// <summary>Element interior size</summary>
            public Rectangle Interior;
            /// <summary>Array of horizontal offsets of output pins in pixels</summary>
            public int[] OutputLeftX;

            // The following are properties of the ICircuitElementType that reflect info in the cached object
            // If any of these differ from the cached object the cached object should be invalidated so it can be re-drawn to reflect these changes.
            /// <summary>
            /// Number of input pins. Property of the ICircuitElementType that reflects info in the cached object.</summary>
            public int numInputs;
            /// <summary>
            /// Number of output pins. Property of the ICircuitElementType that reflects info in the cached object.</summary>
            public int numOutputs;
        }

        /// <summary>
        /// Size info for a CircuitElement type</summary>
        public class ElementSizeInfo
        {
            /// <summary>
            /// Constructor for gathering size information, in pixels (or DIPs)</summary>
            /// <param name="size">Element size in pixels</param>
            /// <param name="interior">Element interior rectangle in pixels</param>
            /// <param name="outputLeftX">Horizontal offset of output pins in pixels</param>
            public ElementSizeInfo(Size size, Rectangle interior, IEnumerable<int> outputLeftX)
            {
                m_size = size;
                m_interior = interior;
                m_outputLeftX = outputLeftX;
            }

            /// <summary>
            /// Gets the size in pixels. The size includes the whole visual circuit element
            /// but not the label below it.</summary>
            public Size Size { get { return m_size; } }

            /// <summary>
            /// Gets the interior rectangle in pixels</summary>
            public Rectangle Interior { get { return m_interior; } }

            /// <summary>
            /// Gets the horizontal offset of output pins in pixels</summary>
            public IEnumerable<int> OutputLeftX { get { return m_outputLeftX; } }

            private readonly Size m_size;
            private readonly Rectangle m_interior;
            private readonly IEnumerable<int> m_outputLeftX;
        }

        #endregion

        /// <summary>
        /// Size of category expanders in pixels</summary>
        protected const int ExpanderSize = GdiUtil.ExpanderSize;
        private D2dBrush m_subGraphPinBrush;

        private D2dRoundedRect m_elementBody = new D2dRoundedRect();
        private D2dDiagramTheme m_theme;

        private int m_rowSpacing;
        private int m_pinSize = 8;
        private int m_pinOffset;
        private int m_pinMargin = 2;
        private Point m_subContentOffset;
        private bool m_subContentOffseExternalSet;


        private int m_groupPinExpandedOffset; // the y offset to be added to draw group pins on the border of the group when the group is expanded inline


        private Dictionary<ICircuitElementType, ElementTypeInfo> m_elementTypeCache = new Dictionary<ICircuitElementType, ElementTypeInfo>();
        private readonly Stack<TElement> m_graphPath = new Stack<TElement>(); // current drawing stack
        private readonly Dictionary<IDocument, Dictionary<ICircuitElementType, ElementTypeInfo>> m_cachePerDocument =
            new Dictionary<IDocument, Dictionary<ICircuitElementType, ElementTypeInfo>>();
        private IDocumentRegistry m_documentRegistry;
        private PinStyle m_pinDrawStyle;

        private int m_maxCollapsedGroupPinNameLength;
        private int m_truncatedPinNameSubstringLength;

        private const int MinElementWidth = 4;
        private const int MinElementHeight = 4;
        //private const int HighlightingWidth = 3;
        private const int MaxNameOverhang = 64;
    }
}
