//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;
using Sce.Atf.Direct2D;
using Sce.Atf.Dom;


namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Provides information about an edge's shape and appearance
    /// </summary>
    public class WireStyleProvider<TElement, TWire, TPin> : DomNodeAdapter, IEdgeStyleProvider
        where TElement : class, ICircuitElement
        where TWire : class, IGraphEdge<TElement, TPin>
        where TPin : class, ICircuitPin
    {

        public EdgeStyle EdgeStyle
        {
            get
            {
                return m_useDefaulStyle ? CircuitDefaultStyle.EdgeStyle : m_edgeStyle;
            }
            set
            {
                m_edgeStyle = value;
                m_useDefaulStyle = false;
            }
        }

        #region IEdgeStyleProvider Members

        /// <summary>
        /// Retrieves the data to be plotted for the edge.</summary>
        /// <param name="render">The render to retrieve the data for.</param>
        /// <param name="worldOffset">Current world offset of the render when drawing this edge.</param>
        /// <param name="g">Graphics object</param>
        /// <returns>The data to plot for the edge.</returns>
        /// <remarks>The data is returned in world space</remarks>
        IEnumerable<EdgeStyleData> IEdgeStyleProvider.GetData(DiagramRenderer render, Point worldOffset, D2dGraphics g)
        {
            var circuitRender = render as D2dCircuitRenderer<TElement, TWire, TPin>;
            var dataPoints = GetGroupPinChainData(circuitRender, worldOffset, g);
            IEnumerable<EdgeStyleData> result = EmptyEnumerable<EdgeStyleData>.Instance;
            if (EdgeStyle ==  EdgeStyle.DirectCurve)
            {
                // skip group pin positions where group pins are hidden
                var visiblePoints = dataPoints.Where(
                    pt => pt.Group == null || !pt.Group.Expanded  || pt.Group.Info.ShowExpandedGroupPins).ToArray();
                var curves = new EdgeStyleData[visiblePoints.Length - 1];
                for (int i = 0; i < visiblePoints.Length - 1; ++i )
                {
                    var bezier = SetupBezierCurve(visiblePoints[i].Pos, visiblePoints[i+1].Pos);
                    var edgeData = new EdgeStyleData
                                       {
                                           ShapeType = EdgeStyleData.EdgeShape.Bezier,
                                           EdgeData = bezier,
                                       };
                    curves[i] = edgeData;
                }
                result = curves;
            }
            else if (EdgeStyle == EdgeStyle.Default)
            {
                var curves = new EdgeStyleData[dataPoints.Count - 1];
                for (int i=0; i< dataPoints.Count-1;++i)
                {
                    var bezier = SetupBezierCurve(dataPoints[i].Pos, dataPoints[i+ 1].Pos);
                    var edgeData = new EdgeStyleData  
                                        {
                                            ShapeType = EdgeStyleData.EdgeShape.Bezier,
                                            EdgeData = bezier,
                                         };
                    curves[i] = edgeData;
                }
                result = curves;
            }
            else if (EdgeStyle == EdgeStyle.Polyline)
            {
                var polyline = new EdgeStyleData();
                var lines = new PointF[dataPoints.Count];
                for (int v = 0; v < dataPoints.Count; ++v)
                    lines[v] = dataPoints[v].Pos;
                polyline.ShapeType = EdgeStyleData.EdgeShape.Polyline;
                polyline.EdgeData = lines;
                result = new[] { polyline };
            }

            return result;
        }

        #endregion

        private static BezierCurve2F SetupBezierCurve(PointF p0, PointF p1)
        {
            float tanLen = Math.Abs(p1.X - p0.X) / 2.0f;
            return new BezierCurve2F(
                  new Vec2F(p0.X, p0.Y),
                  new Vec2F(p0.X + tanLen, p0.Y),
                  new Vec2F(p1.X - tanLen, p1.Y),
                  new Vec2F(p1.X, p1.Y));
        }

        private void InterpolateData(IList<Pair<PointF, Group>> dataPoints, int start, int end)
        {
            var A = new Vec2F(dataPoints[start - 1].First);
            var B = new Vec2F(dataPoints[end + 1].First);
            var D = B - A;
 
            for (int i= start; i<=end ;++i)
            {   
                float t =(dataPoints[i].First.X - A.X)/D.X;
                var P = A + t*D;
                dataPoints[i] = new Pair<PointF, Group>(new PointF( P.X,  P.Y), dataPoints[i].Second);
            }
        }

        private class GroupPinData
        {
            public PointF Pos;
            public GroupPin GroupPin;
            public ICircuitGroupType<TElement, TWire, TPin> Group;
            public bool IsReal; // true when the data point does not belongs to the internal virtual link
        }

        private IList<GroupPinData> GetGroupPinChainData(D2dCircuitRenderer<TElement, TWire, TPin> circuitRender, Point worldOffset, D2dGraphics g)
        {
            var connection = DomNode.Cast<Wire>();
            var dataPoints = new List<GroupPinData>();
            if (circuitRender != null)
            {
                var relativePath = new List<ICircuitGroupType<TElement, TWire, TPin>>();
                
                // --- edge starts from the output pin 
                var group = connection.OutputElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
                foreach (var groupPin in connection.OutputPinSinkChain)
                {                   
                    var pt = circuitRender.GetPinPosition(group.Cast<TElement>(), groupPin.Index, false, g);
                    pt.Offset(worldOffset);
                    pt.Offset(circuitRender.WorldOffset(relativePath.AsIEnumerable<TElement>()));
                    relativePath.Add(group);
                    dataPoints.Add(new GroupPinData { Pos = pt, Group = group, GroupPin = groupPin });
                    if (!group.Expanded)
                        break;
                    group = groupPin.InternalElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
                }

                dataPoints.Reverse(); // upward
                if (relativePath.Any())
                {
                    var startGroup = relativePath.Last();
                    if (startGroup.Expanded) // the edge starts from an expanded group pin, need to draw the virtual link
                    {
                        // the first data starts from sub-node/pin    
                        var firstGroupPin = dataPoints[0].GroupPin;
                        Point p0 = circuitRender.GetPinPosition(firstGroupPin.InternalElement.Cast<TElement>(), firstGroupPin.InternalPinIndex, false, g);
                        p0.Offset(worldOffset);
                        p0.Offset(circuitRender.WorldOffset(relativePath.AsIEnumerable<TElement>()));
                        dataPoints.Insert(0, new GroupPinData { Pos = p0 });
                    }
                  
                }
                else // edge starts from a non-expanded node
                {
                    Point p0 = circuitRender.GetPinPosition(connection.OutputElement.Cast<TElement>(), connection.OutputPin.Index, false, g);
                    p0.Offset(worldOffset);
                    dataPoints.Add(new GroupPinData { Pos = p0});
                }

                int numInputPoints= dataPoints.Count;
                dataPoints[dataPoints.Count - 1].IsReal = true;

                // --- edge ends at the input pin 
                relativePath.Clear();
                group = connection.InputElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
                foreach (var groupPin in connection.InputPinSinkChain)
                {
                    var pt = circuitRender.GetPinPosition(group.Cast<TElement>(), groupPin.Index, true, g);
                    pt.Offset(worldOffset);
                    pt.Offset(circuitRender.WorldOffset(relativePath.AsIEnumerable<TElement>()));
                    relativePath.Add(group);
                    dataPoints.Add(new GroupPinData { Pos = pt, Group = group, GroupPin = groupPin });
                    if (!group.Expanded)
                        break;
                    group = groupPin.InternalElement.As<ICircuitGroupType<TElement, TWire, TPin>>();
                }

                if (relativePath.Any())
                {
                    var lastGroupPin = dataPoints[dataPoints.Count - 1].GroupPin;
                    var lastGroup = dataPoints[dataPoints.Count - 1].Group;
                    if (lastGroup.Expanded) // the edge ends at an expanded group pin, need to draw the virtual link to subnode
                    {
                        // the last data ends at sub-node/pin              
                        Point pn = circuitRender.GetPinPosition(lastGroupPin.InternalElement.Cast<TElement>(), lastGroupPin.InternalPinIndex, true, g);
                        pn.Offset(worldOffset);
                        pn.Offset(circuitRender.WorldOffset(relativePath.AsIEnumerable<TElement>()));
                        dataPoints.Add(new GroupPinData { Pos = pn});
                    }
                  
                }
                else // edge ends at a non-expanded node
                {
                    Point pn = circuitRender.GetPinPosition(connection.InputElement.Cast<TElement>(), connection.InputPin.Index, true, g);
                    pn.Offset(worldOffset);
                    dataPoints.Add(new GroupPinData { Pos = pn });
                }

                dataPoints[numInputPoints].IsReal = true;
            }

            return dataPoints;
        }

        private EdgeStyle m_edgeStyle;
        private bool m_useDefaulStyle = true;

    }
}
