//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    [Export(typeof(CircuitMagnifier))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CircuitMagnifier : IInitializable
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_controlRegistry.ControlAdded += new EventHandler<ItemInsertedEventArgs<ControlInfo>>(ControlRegistry_ControlAdded);
            m_controlRegistry.ControlRemoved += new EventHandler<ItemRemovedEventArgs<ControlInfo>>(ControlRegistry_ControlRemoved);
            InitMagnifier();
        }
        #endregion

        private void ControlRegistry_ControlAdded(object sender, ItemInsertedEventArgs<ControlInfo> e)
        {
            var d2dControl = e.Item.Control as D2dAdaptableControl;
            if (d2dControl == null) return;
            var group = d2dControl.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Group>();
            var circut = d2dControl.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Circuit>();
            if (circut == null && group == null) return;
                        
            d2dControl.MouseMove += control_MouseMove;
            d2dControl.Leave += (s, ev) => ClearMagnifierContext((D2dAdaptableControl)s);
            d2dControl.MouseLeave += (s, ev) => ClearMagnifierContext((D2dAdaptableControl)s);
        }

        private void ControlRegistry_ControlRemoved(object sender, ItemRemovedEventArgs<ControlInfo> e)
        {
            var d2dControl = e.Item.Control as D2dAdaptableControl;
            ClearMagnifierContext(d2dControl);
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {            
            var d2dControl = (D2dAdaptableControl)sender;            
            var group = d2dControl.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Group>();
            if (group != null)
            {
                m_circuitMagnifierControl.Hide();
                m_circuitGroupMagnifierControl.Show();
                UpdateControl(d2dControl, m_circuitGroupMagnifierControl, e.Location);
                return;
            }
            var circut = d2dControl.ContextAs<Sce.Atf.Controls.Adaptable.Graphs.Circuit>();
            if (circut != null)
            {
                m_circuitMagnifierControl.Show();
                m_circuitGroupMagnifierControl.Hide();
                UpdateControl(d2dControl, m_circuitMagnifierControl,e.Location);
            }
        }

        private void UpdateControl(D2dAdaptableControl source, D2dAdaptableControl target, Point cpt)
        {
            
            target.Context = source.Context;
            if (target.Context == null) return;
            

            // transform current mouse point to graph space.
            var srcXform = source.As<ITransformAdapter>();
            Matrix3x2F mtrx = Matrix3x2F.Invert(srcXform.Transform);            
            var gpt = Matrix3x2F.TransformPoint(mtrx, cpt);

            var targetXform = target.As<ITransformAdapter>();
            
            var csz = target.ClientSize;
            PointF translation = new PointF(csz.Width / 2 - gpt.X * targetXform.Scale.X, csz.Height / 2 - gpt.Y * targetXform.Scale.Y);
            targetXform.Translation = translation;
            var edgeEditor = source.As<D2dGraphEdgeEditAdapter<Module, Connection, ICircuitPin>>();
            m_activeEdgeEditor = (edgeEditor != null && edgeEditor.IsDraggingEdge) ? edgeEditor : null;            
            target.Invalidate();
        }

        private D2dGraphEdgeEditAdapter<Module, Connection, ICircuitPin> m_activeEdgeEditor;

        private void ClearMagnifierContext(D2dAdaptableControl control)
        {
            if (control != null)
            {
                if (control.Context == m_circuitMagnifierControl.Context)
                    m_circuitMagnifierControl.Context = null;

                if (control.Context == m_circuitGroupMagnifierControl.Context)
                    m_circuitGroupMagnifierControl.Context = null;

                m_activeEdgeEditor = null;                
            }
        }
        
        private Control m_magnifierControl;
        private D2dAdaptableControl m_circuitMagnifierControl;
        private D2dAdaptableControl m_circuitGroupMagnifierControl;
        private ControlInfo m_controlInfo;
        private void InitMagnifier()
        {

            m_magnifierControl = new Control();
            m_magnifierControl.Name = "Magnifier";
            
            // create magnifier view
            m_controlInfo = new ControlInfo("Magnifier".Localize(), "Magnified view", StandardControlGroup.Left);

            m_circuitMagnifierControl = CreateControl(false);
            m_magnifierControl.Controls.Add(m_circuitMagnifierControl);


            m_circuitGroupMagnifierControl = CreateControl(true);            
            m_magnifierControl.Controls.Add(m_circuitGroupMagnifierControl);

            m_controlHostService.RegisterControl(m_magnifierControl, m_controlInfo, null);

            m_circuitMagnifierControl.DrawingD2d += m_magnifier_Painting;
            m_circuitGroupMagnifierControl.DrawingD2d += m_magnifier_Painting;
            UpdateControlName(1.25f);
        }

        private bool m_updatingName;
        private void UpdateControlName(float scale)
        {
            if (m_updatingName) return;

            try
            {
                m_updatingName = true;
                string scaleStr = string.Format(": {0:0.0}X", scale);
                string name = "Magnifier";
                m_controlInfo.Name = name + scaleStr;
                var xform = m_circuitMagnifierControl.Cast<ITransformAdapter>();
                xform.Scale = new PointF(scale, scale);

                xform = m_circuitGroupMagnifierControl.Cast<ITransformAdapter>();
                xform.Scale = new PointF(scale, scale);
            }
            finally
            {
                m_updatingName = false;
            }

        }

        private void m_magnifier_Painting(object sender, EventArgs e)
        {
            var d2dControl = (D2dAdaptableControl)sender;
            if (d2dControl.Context != null)
            {                
                var saveXform = d2dControl.D2dGraphics.Transform;
                var dragEdge = m_activeEdgeEditor != null ? m_activeEdgeEditor.GetDraggingEdge() : null;
                if (dragEdge != null)
                {
                    PointF start = dragEdge.StartPoint;
                    PointF end = dragEdge.EndPoint;
                    if (dragEdge.FromRoute == null)
                    {
                        start = Matrix3x2F.TransformPoint(saveXform, start);
                    }

                    if (dragEdge.ToNode == null)
                    {
                        end = Matrix3x2F.TransformPoint(saveXform, end);
                    }
                 
                    m_editor.CircuitRenderer.DrawPartialEdge(
                      dragEdge.FromNode,
                      dragEdge.FromRoute,
                      dragEdge.ToNode,
                      dragEdge.ToRoute,
                      dragEdge.Label,
                      start,
                      end,
                      d2dControl.D2dGraphics
                  );                    
                }
                else
                {
                    m_activeEdgeEditor = null;                    
                }

                var csz = d2dControl.ClientSize;

                d2dControl.D2dGraphics.Transform = Matrix3x2F.Identity;
                var currentCursor = Cursor.Current;
                var rect = new Rectangle();
                rect.Location = new Point(csz.Width / 2 - currentCursor.HotSpot.X, csz.Height / 2 - currentCursor.HotSpot.Y);
                rect.Size = Cursor.Current.Size;
                d2dControl.D2dGraphics.BeginGdiSection();
                Graphics g = d2dControl.D2dGraphics.Graphics;
                Cursor.Current.Draw(g, rect);
                d2dControl.D2dGraphics.EndGdiSection();
                d2dControl.D2dGraphics.Transform = saveXform;
            }
        }

        private D2dAdaptableControl CreateControl(bool createCircutGroup)
        {
            var d2dControl = new D2dAdaptableControl();
            d2dControl.SuspendLayout();

            var transformAdapter = new TransformAdapter();
            transformAdapter.EnforceConstraints = false; //to allow the canvas to be panned to view negative coordinates
            transformAdapter.Scale = new PointF(1.25f, 1.25f);
            transformAdapter.UniformScale = true;
            transformAdapter.MinScale = new PointF(1.0f, 1.0f);
            transformAdapter.MaxScale = new PointF(5, 5);

            var annotationAdaptor = new D2dAnnotationAdapter(m_editor.Theme); // display annotations under diagram
            var gridAdapter = new D2dGridAdapter();
            gridAdapter.Enabled = false;
            gridAdapter.Visible = true;
            

            D2dGraphAdapter<Module, Connection, ICircuitPin> graphAdapter;
            if (createCircutGroup)
            {
                var subG = new D2dSubgraphAdapter<Module, Connection, ICircuitPin>(m_editor.SubCircuitRenderer, transformAdapter);
                subG.SettingVisibleWorldBoundsDisabled = true;
                graphAdapter = subG;
            }
            else
            {
                graphAdapter = new D2dGraphAdapter<Module, Connection, ICircuitPin>(m_editor.CircuitRenderer, transformAdapter);
            }
            
            d2dControl.Adapt(

                transformAdapter,
                new MouseWheelManipulator(transformAdapter),
                gridAdapter,
                annotationAdaptor, //Needs to be before circuitAdapter so that comments appear under elements.
                graphAdapter);

            d2dControl.ResumeLayout();
            d2dControl.Dock = DockStyle.Fill;

            transformAdapter.TransformChanged += (s, e) => UpdateControlName(((ITransformAdapter)s).Scale.X);
            return d2dControl;
        }

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService = null;

        [Import(AllowDefault = false)]
        private IControlRegistry m_controlRegistry = null;

        [Import(AllowDefault = false)]
        private Editor m_editor = null;

    }
}
