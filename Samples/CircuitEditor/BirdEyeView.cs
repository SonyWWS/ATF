//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;


namespace CircuitEditorSample
{
    [Export(typeof(BirdEyeView))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BirdEyeView : IInitializable
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_controlRegistry.ControlAdded += ControlRegistry_ControlAdded;
            m_controlRegistry.ControlRemoved += ControlRegistry_ControlRemoved;
            m_controlRegistry.ActiveControlChanged += new EventHandler(m_controlRegistry_ActiveControlChanged);
            InitControl();
        }
        
        #endregion
       
        private void ControlRegistry_ControlAdded(object sender, ItemInsertedEventArgs<ControlInfo> e)
        {
            var d2dControl = e.Item.Control as D2dAdaptableControl;
            if (d2dControl == null) return;
            var group = d2dControl.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Group>();
            var circut = d2dControl.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Circuit>();
            if (circut == null && group == null) return;           
            d2dControl.SizeChanged += (s, ev) => InvalidateTarget((D2dAdaptableControl)s);
            d2dControl.MouseMove += d2dSource_MouseMove;
            var xform = d2dControl.As<ITransformAdapter>();
            if (xform != null) xform.TransformChanged += xform_TransformChanged;
            var validationContext = d2dControl.ContextAs<IValidationContext>();
            if(validationContext != null) validationContext.Ended += (s, ev) => UpdateGraphBounds();            
        }
        private void ControlRegistry_ControlRemoved(object sender, ItemRemovedEventArgs<ControlInfo> e)
        {
            var d2dControl = e.Item.Control as D2dAdaptableControl;
            ClearControlContext(d2dControl);
        }
        private void m_controlRegistry_ActiveControlChanged(object sender, EventArgs e)
        {
            var cinfo = m_controlRegistry.ActiveControl;
            var d2dControl = cinfo.Control as D2dAdaptableControl;
            SourceControlChanged(d2dControl);                      
        }

        private void d2dSource_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender == m_sourceControl && m_targetControl != null)
                m_targetControl.Invalidate();
        }

        
        
        void xform_TransformChanged(object sender, EventArgs e)
        {            
            var controlAdapter = sender.Cast<ControlAdapter>();
            var d2dControl = (D2dAdaptableControl)controlAdapter.AdaptedControl;
            DrawTarget(d2dControl);            
        }


        private void DrawTarget(D2dAdaptableControl source)
        {
            if (source == m_sourceControl && m_targetControl != null)
                m_targetControl.DrawD2d();
        }

        private void InvalidateTarget(D2dAdaptableControl source)
        {
            if (source == m_sourceControl && m_targetControl != null)
                m_targetControl.Invalidate();
        }
      
       
        private void SourceControlChanged(D2dAdaptableControl source)
        {
            if (source == null 
                || source == m_sourceControl
                || source == m_targetControl) 
                return;

            m_sourceControl = null;
            m_targetControl = null;
            var group = source.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Group>();
            if (group != null)
            {
                m_circuitControl.Hide();
                m_groupControl.Show();                               
                UpdateContext(source, m_groupControl);
                return;
            }

            var circut = source.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Circuit>();
            if (circut != null)
            {
                m_circuitControl.Show();                
                m_groupControl.Hide();
                UpdateContext(source, m_circuitControl);
            }
        }

        private void UpdateContext(D2dAdaptableControl source, D2dAdaptableControl target)
        {            
            target.Context = source.Context;
            if (target.Context == null) return;
            m_targetControl = target;
            m_sourceControl = source;
            UpdateGraphBounds();
        }

        private void UpdateGraphBounds()
        {
            if (m_targetControl == null || m_sourceControl == null
               || m_targetControl.Context != m_sourceControl.Context ) return;

            m_currentGraphBounds = RectangleF.Empty;
            var vc = m_sourceControl.Context.Cast<ViewingContext>();
            var bounds = vc.GetBounds();

            if (m_sourceControl.ContextIs<Sce.Atf.Controls.Adaptable.Graphs.Group>())
            {
                bounds.Inflate(m_editor.Theme.PinSize + CircuitGroupPinInfo.FloatingPinBoxWidth, 0);
            }
            var srcXform = m_sourceControl.As<ITransformAdapter>();
            Matrix3x2F invXform = Matrix3x2F.Invert(srcXform.Transform);
            var gBounds = Matrix3x2F.Transform(invXform, bounds);
            m_currentGraphBounds = gBounds;
            Fit(m_targetControl, gBounds);       
        }

        private void Fit(D2dAdaptableControl control, RectangleF gBounds)
        {
            if (m_currentGraphBounds.IsEmpty) return;
            var targetXform = control.As<ITransformAdapter>();
            var crect = control.ClientRectangle;
            crect.Inflate(-12, -12); // 8 pixels margin.
            if (crect.Width < 1 || crect.Height < 1) return;

            float sx = MathUtil.Clamp(crect.Width / gBounds.Width, targetXform.MinScale.X, targetXform.MaxScale.X);
            float sy = MathUtil.Clamp(crect.Height / gBounds.Height, targetXform.MinScale.Y, targetXform.MaxScale.Y);
            float scale = Math.Min(sx, sy);
            crect.X += (int)(crect.Width - gBounds.Width * scale) / 2;
            crect.Y += (int)(crect.Height - gBounds.Height * scale) / 2;
            float tx = crect.X - gBounds.X * scale;
            float ty = crect.Y - gBounds.Y * scale;
            targetXform.SetTransform(scale, scale, tx, ty);
            control.Invalidate();
        }
     
        private void ClearControlContext(D2dAdaptableControl control)
        {
            if (control != null)
            {
                bool clear = control == m_sourceControl
                    || (m_targetControl != null && control.Context == m_targetControl.Context);

                if(clear)
                {
                    m_sourceControl = null;
                    m_targetControl = null;
                    m_circuitControl.Context = null;
                    m_groupControl.Context = null;
                }
            }
        }

        
        private void InitControl()
        {

            var m_control = new Control();
            m_control.Name = "BirdEyeView";

            m_controlInfo = new ControlInfo(m_panelName, "bird's eye view", StandardControlGroup.Left);

            m_circuitControl = CreateControl(false);
            m_control.Controls.Add(m_circuitControl);

            m_groupControl = CreateControl(true);
            m_control.Controls.Add(m_groupControl);

            m_controlHostService.RegisterControl(m_control, m_controlInfo, null);            
        }

        private bool m_updatingName;
        private void UpdateControlName(float scale)
        {
            if (m_updatingName) return;

            try
            {
                m_updatingName = true;
                string scaleStr = string.Format(": {0:0.000000}X", scale);
                m_controlInfo.Name = m_panelName + scaleStr;
                var xform = m_circuitControl.Cast<ITransformAdapter>();
                xform.Scale = new PointF(scale, scale);
                xform = m_groupControl.Cast<ITransformAdapter>();
                xform.Scale = new PointF(scale, scale);
            }
            finally
            {
                m_updatingName = false;
            }
        }

        private D2dAdaptableControl CreateControl(bool createCircutGroup)
        {
            var d2dControl = new D2dAdaptableControl();
            d2dControl.SuspendLayout();
            d2dControl.Dock = DockStyle.Fill;
            var transformAdapter = new TransformAdapter();
            transformAdapter.EnforceConstraints = false; //to allow the canvas to be panned to view negative coordinates
            transformAdapter.Scale = new PointF(1.25f, 1.25f);
            transformAdapter.UniformScale = true;
            transformAdapter.MinScale = new PointF(0.000001f, 0.000001f);
            transformAdapter.MaxScale = new PointF(5, 5);

            var annotationAdaptor = new D2dAnnotationAdapter(m_editor.Theme); // display annotations under diagram
            annotationAdaptor.PickingDisabled = true;
            var gridAdapter = new D2dGridAdapter();
            gridAdapter.Enabled = false;
            gridAdapter.Visible = true;

            D2dGraphAdapter<Module, Connection, ICircuitPin> graphAdapter;
            if (createCircutGroup)
            {
                var subG = new D2dSubgraphAdapter<Module, Connection, ICircuitPin>(m_editor.SubCircuitRenderer, transformAdapter);               
                graphAdapter = subG;
            }
            else
            {
                graphAdapter = new D2dGraphAdapter<Module, Connection, ICircuitPin>(m_editor.CircuitRenderer, transformAdapter);
            }

            
            d2dControl.Adapt(
                transformAdapter,                
                gridAdapter,
                annotationAdaptor, 
                graphAdapter,
                new DraggableRect(this));
            
            d2dControl.ResumeLayout();
           
            transformAdapter.TransformChanged += (s, e) => UpdateControlName(((ITransformAdapter)s).Scale.X);            
            d2dControl.SizeChanged += d2dControl_SizeChanged;
            
            return d2dControl;
        }        

        private void d2dControl_SizeChanged(object sender, EventArgs e)
        {
            var d2dControl = (D2dAdaptableControl)sender;
            if (d2dControl.Visible) Fit(d2dControl, m_currentGraphBounds);            
        }


        private RectangleF m_currentGraphBounds;
        

        private D2dAdaptableControl m_sourceControl;
        private D2dAdaptableControl m_targetControl;
        private D2dAdaptableControl m_circuitControl;
        private D2dAdaptableControl m_groupControl;
        private ControlInfo m_controlInfo;
        private readonly string m_panelName = "Bird's-eye view".Localize();

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService = null;

        [Import(AllowDefault = false)]
        private IControlRegistry m_controlRegistry = null;

        [Import(AllowDefault = false)]
        private Editor m_editor = null;

        private class DraggableRect : DraggingControlAdapter
        {
            private BirdEyeView m_birdEyeView;
            public DraggableRect(BirdEyeView birdEyeView)
            {
                m_birdEyeView = birdEyeView;
            }

            /// <summary>
            /// Binds the adapter to the adaptable control. Called in the order that the adapters
            /// were defined on the control.</summary>
            /// <param name="control">Adaptable control</param>
            protected override void Bind(AdaptableControl control)
            {
                var d2dControl = (D2dAdaptableControl)control;
                m_transformAdapter = control.As<ITransformAdapter>();
                d2dControl.DrawingD2d += PaintD2d;
                d2dControl.MouseWheel += control_MouseWheel;
                base.Bind(control);
            }

            /// <summary>
            /// Unbinds the adapter from the adaptable control</summary>
            /// <param name="control">Adaptable control</param>
            protected override void Unbind(AdaptableControl control)
            {
                var d2dControl = control as D2dAdaptableControl;
                d2dControl.DrawingD2d -= PaintD2d;
                d2dControl.MouseWheel -= control_MouseWheel;
                m_transformAdapter = null;                
                base.Unbind(control);
            }

            private void PaintD2d(object sender, EventArgs e)
            {                
                if (!IsContextValid()) return;
                var control = (D2dAdaptableControl)AdaptedControl;
                var sourceControl = m_birdEyeView.m_sourceControl;

                var srcXformAdapter = sourceControl.Cast<ITransformAdapter>();

                D2dGraphics g= control.D2dGraphics;
                Matrix3x2F invXform = g.Transform;
                float scaleX = invXform.M11;
                invXform.Invert();

                Point cpt = control.PointToClient(Control.MousePosition);
                var gpt = Matrix3x2F.TransformPoint(invXform, cpt);

                // transform client rect of source control to graph space.
                var srcInvXform = Matrix3x2F.Invert(srcXformAdapter.Transform);
                var grect = Matrix3x2F.Transform(srcInvXform, sourceControl.ClientRectangle);

                float strokeWidth = m_dragging || ( grect.Contains(gpt) && control.Focused)
                    ? 3.0f / scaleX : 1.5f / scaleX;
                g.DrawRectangle(grect, Color.Yellow, strokeWidth);

                Point srcCpt = sourceControl.PointToClient(Control.MousePosition);
                var srcGpt = Matrix3x2F.TransformPoint(srcInvXform, srcCpt);
                if ( sourceControl.Focused  && grect.Contains(srcGpt) 
                    && !control.ClientRectangle.Contains(cpt))
                {
                    float cursorSize = 7.0f / scaleX;
                    RectangleF cursorRect = new RectangleF(srcGpt.X - cursorSize / 2, srcGpt.Y - cursorSize / 2,
                        cursorSize, cursorSize);
                    g.FillEllipse(cursorRect, Color.Yellow);
                }
            }

            private bool IsContextValid()
            {
                var sourceControl = m_birdEyeView.m_sourceControl;
                return AdaptedControl.Context != null 
                    && sourceControl != null;
            }

            private void control_MouseWheel(object sender, MouseEventArgs e)
            {
                if (!AdaptedControl.ClientRectangle.Contains(e.Location)) return;
                if (e.Button != MouseButtons.None
                    || !IsContextValid()) return;
                    
                // apply transformation to source control.
                var sourceControl = m_birdEyeView.m_sourceControl;
                var srcXformAdapter = sourceControl.Cast<ITransformAdapter>();
                var srcInvXform = Matrix3x2F.Invert(srcXformAdapter.Transform);
                var grect = Matrix3x2F.Transform(srcInvXform, sourceControl.ClientRectangle);
                var inxXform = Matrix3x2F.Invert(m_transformAdapter.Transform);
                var gpt = Matrix3x2F.TransformPoint(inxXform, e.Location);
                if (!grect.Contains(gpt)) return;

                // zoom at the center of the grect.
                gpt.X = grect.X + grect.Width /2;
                gpt.Y = grect.Y + grect.Height /2;

                // transform gpt to client space of sourcecontrol.
                var srcCpt = Matrix3x2F.TransformPoint(srcXformAdapter.Transform, gpt);

                PointF translation = srcXformAdapter.Translation;
                PointF scale = srcXformAdapter.Scale;
                PointF scaleCenterStart = new PointF(
                    (srcCpt.X - translation.X) / scale.X,
                    (srcCpt.Y - translation.Y) / scale.Y);

                float delta = 1.0f + e.Delta / 1200.0f;
                scale = new PointF(
                    scale.X * delta,
                    scale.Y * delta);

                // constrain scale before calculating translation to maintain scroll center position
                scale = srcXformAdapter.ConstrainScale(scale);

                translation = new PointF(
                    srcCpt.X - scaleCenterStart.X * scale.X,
                    srcCpt.Y - scaleCenterStart.Y * scale.Y);

                srcXformAdapter.SetTransform(
                    scale.X,
                    scale.Y,
                    translation.X,
                    translation.Y);
            }

            protected override void OnMouseMove(object sender, MouseEventArgs e)
            {
                base.OnMouseMove(sender, e);
                AdaptedControl.Invalidate();
            }
            private PointF m_hitGpt;
            protected override void OnMouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button != MouseButtons.Left || !IsContextValid()) return;

                var sourceControl = m_birdEyeView.m_sourceControl;
                var srcXformAdapter = sourceControl.Cast<ITransformAdapter>();
                var srcInvXform = Matrix3x2F.Invert(srcXformAdapter.Transform);
                var grect = Matrix3x2F.Transform(srcInvXform, sourceControl.ClientRectangle);
                var inxXform = Matrix3x2F.Invert(m_transformAdapter.Transform);
                var gpt = Matrix3x2F.TransformPoint(inxXform, e.Location);
                
                if (!grect.Contains(gpt))
                {

                    var srcScale = srcXformAdapter.Scale;
                    float gcx = grect.X + grect.Width / 2;
                    float gcy = grect.Y + grect.Height / 2;
                    grect.X += gpt.X - gcx;
                    grect.Y += gpt.Y - gcy;
                    float tx = -grect.X * srcScale.X;
                    float ty = -grect.Y * srcScale.Y;
                    srcXformAdapter.Translation = new PointF(tx, ty);
                }
                m_hitGpt.X = gpt.X - grect.X;
                m_hitGpt.Y = gpt.Y - grect.Y;

                base.OnMouseDown(sender, e);
                AdaptedControl.Capture = true;
            }


            
            private bool m_dragging;
            protected override void OnDragging(MouseEventArgs e)
            {
                base.OnDragging(e);
                if (e.Button != MouseButtons.Left || !IsContextValid()) return;

                AdaptedControl.Capture = true;
                    

                m_dragging = true;
                var sourceControl = m_birdEyeView.m_sourceControl;
                var srcXformAdapter = sourceControl.Cast<ITransformAdapter>();
                var inxXform = Matrix3x2F.Invert(m_transformAdapter.Transform);
                var gpt = Matrix3x2F.TransformPoint(inxXform, e.Location);
                 gpt.X -= m_hitGpt.X;
                 gpt.Y -= m_hitGpt.Y;
                var srcScale = srcXformAdapter.Scale;
                srcXformAdapter.Translation = new PointF(-gpt.X * srcScale.X, -gpt.Y * srcScale.Y);
            }

            protected override void OnEndDrag(MouseEventArgs e)
            {
                base.OnEndDrag(e);
                m_dragging = false;
            }

            private ITransformAdapter m_transformAdapter;

        }
    }
}
