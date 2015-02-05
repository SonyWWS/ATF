﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to an animation</summary>
    public class UIAnimation : UIObject, ICurveSet
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event. Creates several curves, automatically added to the animation.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_curves = GetChildList<ICurve>(UISchema.UIAnimationType.curveChild);
            if (m_curves.Count > 0)
                return;
            
            // add few sample curves.

            // add x channel
            Curve curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "curve1";
            curve.DisplayName = "X channel";
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = float.MinValue;
            curve.MaxY = float.MaxValue;
            curve.CurveColor = Color.Black;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "x-Time";
            curve.YLabel = "x-Pos";

            IControlPoint cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 1;
            curve.AddControlPoint(cp);
            cp = curve.CreateControlPoint();
            cp.X = 500;
            cp.Y = 10;
            curve.AddControlPoint(cp);
            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);

            // add y channel
            curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "Curve2";
            curve.DisplayName = "Y Channel";
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = float.MinValue;
            curve.MaxY = float.MaxValue;
            curve.CurveColor = Color.Black;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "Y-Time";
            curve.YLabel = "Y-Pos";

            cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 30;
            curve.AddControlPoint(cp);
            cp = curve.CreateControlPoint();
            cp.X = 500;
            cp.Y = 40;
            curve.AddControlPoint(cp);
            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);

            // add Z channel
            curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "Z Channel";
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = float.MinValue;
            curve.MaxY = float.MaxValue;
            curve.CurveColor = Color.Black;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "Z-Time";
            curve.YLabel = "Z-Pos";

            cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 60;
            curve.AddControlPoint(cp);
            cp = curve.CreateControlPoint();
            cp.X = 500;
            cp.Y = 100;
            curve.AddControlPoint(cp);
            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);

            // add red channel
            curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "Red Channel";
            curve.CurveColor = Color.Red;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "R-Time";
            curve.YLabel = "R-Value";
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = 0;
            curve.MaxY = 255;

            cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 0;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();            
            cp.X = 28.5306816f;
            cp.Y = 53.2748222f;            
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.TangentInType = CurveTangentTypes.Fixed;
            cp.TangentOutType = CurveTangentTypes.Fixed;
            cp.X = 76.55121f;
            cp.Y = 139.914139f;
            cp.TangentIn = new Vec2F(0.100534618f, -0.994933546f);
            cp.TangentOut = new Vec2F(0.100534618f, -0.994933546f);
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.TangentInType = CurveTangentTypes.Spline;
            cp.TangentOutType = CurveTangentTypes.Stepped;
            cp.X = 180.1634f;
            cp.Y = 52.07632f;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.TangentInType = CurveTangentTypes.Spline;
            cp.TangentOutType = CurveTangentTypes.Stepped;
            cp.X = 320.363647f;
            cp.Y = 2.0f;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.TangentInType = CurveTangentTypes.Flat;
            cp.TangentOutType = CurveTangentTypes.Flat;
            cp.X = 500.3228f;
            cp.Y = 47.90871f;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.TangentInType = CurveTangentTypes.Flat;
            cp.TangentOutType = CurveTangentTypes.Linear;
            cp.X = 800.749023f;
            cp.Y = 2.0f;
            curve.AddControlPoint(cp);           
            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);

            // add green channel
            curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "Green Channel";
            curve.CurveColor = Color.Green;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "G-Time";
            curve.YLabel = "G-Value";            
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = 0;
            curve.MaxY = 255;

            cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 0;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.X = 800;
            cp.Y = 255;
            curve.AddControlPoint(cp);

            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);

            // add blue channel
            curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "Blue Channel";
            curve.CurveColor = Color.Blue;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "B-Time";
            curve.YLabel = "B-value";
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = 0;
            curve.MaxY = 255;

            cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 128;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.X = 900;
            cp.Y = 128;
            curve.AddControlPoint(cp);

            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);

            // add alpha channel
            curve = (new DomNode(UISchema.curveType.Type)).As<Curve>();
            curve.Name = "Alpha Channel";
            curve.CurveColor = Color.White;
            curve.PreInfinity = CurveLoopTypes.Cycle;
            curve.PostInfinity = CurveLoopTypes.Cycle;
            curve.XLabel = "A-Time";
            curve.YLabel = "A-Value";
            curve.MinX = 0;
            curve.MaxX = 1000;
            curve.MinY = 0;
            curve.MaxY = 255;

            cp = curve.CreateControlPoint();
            cp.X = 0;
            cp.Y = 255;
            curve.AddControlPoint(cp);

            cp = curve.CreateControlPoint();
            cp.X = 1000;
            cp.Y = 0;
            curve.AddControlPoint(cp);

            CurveUtils.ComputeTangent(curve);            
            m_curves.Add(curve);            
        }

        #region ICurveSet Members

        /// <summary>
        /// Gets list of the curves associated with an animation.</summary>
        public IList<ICurve> Curves
        {
            get { return m_curves; }
        }

        private IList<ICurve> m_curves;
        #endregion
    }
}
