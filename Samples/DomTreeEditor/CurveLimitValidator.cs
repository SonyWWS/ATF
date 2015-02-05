//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.CurveEditing;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Force control points to be within curve limits</summary>
    public class CurveLimitValidator : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            // Only adapt to ICurve.
            if(!DomNode.Is<ICurve>())
                throw new InvalidOperationException("This adapter can only attach to instance of type that implements ICurve");

            DomNode.ChildInserting += (sender, e) =>
                {
                    var cp = e.Child.As<IControlPoint>();
                    if (cp != null)
                    {
                        // check curve limit.
                        var curve = DomNode.Cast<ICurve>();
                        if (cp.X < curve.MinX
                            || cp.X > curve.MaxX
                            || cp.Y < curve.MinY
                            || cp.Y > curve.MaxY)
                            throw new InvalidTransactionException("Cannot add control-point outside curve limits".Localize());
                    }
                    
                };

            DomNode.AttributeChanged += (sender, e) =>
            {
                try
                {                   
                    // prevent re-entry
                    if (m_validating) return;
                    m_validating = true;

                    var pt = e.DomNode.As<IControlPoint>();
                    var curve = pt == null ? null : pt.Parent;
                    if (curve != null)
                    {
                        // keep x and y within curve limits.
                        if (e.AttributeInfo.Equivalent(UISchema.controlPointType.xAttribute))
                        {
                            pt.X = MathUtil.Clamp((float)e.NewValue, curve.MinX, curve.MaxX);
                        }
                        else if (e.AttributeInfo.Equivalent(UISchema.controlPointType.yAttribute))
                        {
                            pt.Y = MathUtil.Clamp((float)e.NewValue, curve.MinY, curve.MaxY);
                        }
                    }
                }
                finally
                {
                    m_validating = false;
                }
            };
        }
        private bool m_validating;
    }
}
