//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Extends CanvasControl3D to provide Scene graph rendering and picking</summary>
    public class DesignControl : CanvasControl3D
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="scene">Scene graph</param>
        public DesignControl(Scene scene)
        {
            m_scene = scene;

            if(scene == null)
            {
                throw new Exception("scene is null from ctor!!");
            }
            if (m_scene == null)
            {
                throw new Exception("m_scene is null from ctor!!");
            }
            m_renderAction = new RenderAction(RenderStateGuardian);
            m_pickAction = new PickAction(RenderStateGuardian);

            // default render states. These correspond to the state of the toggles on the toolbar,
            //  like wireframe on/off, lighting on/off, backface culling on/off, and textures on/off.
            m_renderState.RenderMode = RenderMode.Smooth | RenderMode.CullBackFace | RenderMode.Textured | RenderMode.Lit | RenderMode.Alpha;
            m_renderState.WireframeColor = new Vec4F(0, 0.015f, 0.376f, 1.0f);
            m_renderState.SolidColor = new Vec4F(1,1,1, 1.0f);
        }

        #region Events

        /// <summary>
        /// Event that is raised after Scene graph nodes are picked</summary>
        public event EventHandler<PickEventArgs> Picked;

        /// <summary>
        /// Event that is raised when beginning a pick operation</summary>
        public event EventHandler Picking;

        /// <summary>
        /// Event that is raised when manipulator drag starts</summary>
        public event EventHandler<ManipulatorEventArgs> ManipulatorStart;

        /// <summary>
        /// Event that is raised during manipulator dragging</summary>
        public event EventHandler<ManipulatorEventArgs> ManipulatorUpdate;

        /// <summary>
        /// Event that is raised when a manipulator drag ends</summary>
        public event EventHandler<ManipulatorEventArgs> ManipulatorEnd;

        #endregion

        /// <summary>
        /// Gets and sets the render action</summary>
        public IRenderAction RenderAction
        {
            get { return m_renderAction; }
            set { m_renderAction = value; }
        }

        /// <summary>
        /// Gets and sets the pick action</summary>
        public IPickAction PickAction
        {
            get 
            {
                if (m_pickAction != null)
                    m_pickAction.Set(m_renderAction);

                return m_pickAction;
            }
            set { m_pickAction = value; }
        }

        /// <summary>
        /// Gets the current manipulator</summary>
        public IManipulator Manipulator
        {
            get { return m_manipulator; }
        }

        /// <summary>
        /// Sets the manipulator (same as the Manipulator property) and its corresponding SceneNode</summary>
        /// <param name="manipulator">Manipulator to set</param>
        /// <param name="manipulatorNode">SceneNode to set</param>
        /// <param name="parentToWorld">Transform of the manipulator's parent's coordinate system to the world coordinate system</param>
        public void SetManipulatorNode(IManipulator manipulator, SceneNode manipulatorNode, Matrix4F parentToWorld)
        {
            m_manipulator = manipulator;
            m_manipulatorNode = manipulatorNode;
            m_manipulatorParentToWorld = parentToWorld;
        }

        /// <summary>
        /// Gets whether the design control is manipulating an object</summary>
        public static bool IsManipulating
        {
            get { return s_isManipulating; }
        }

        /// <summary>
        /// Gets the current render state</summary>
        public RenderState RenderState
        {
            get { return m_renderState; }
        }

        /// <summary>
        /// Saves the user's settings to this DesignControl by adding attributes and
        /// elements to 'controlElement'</summary>
        /// <param name="controlElement">XML node. The caller already named it. 
        /// User settings are added as attributes and elements to this node</param>
        /// <param name="xmlDoc">The XmlDocument in which child XmlElements are created, for example</param>
        public virtual void SaveState(XmlElement controlElement, XmlDocument xmlDoc)
        {
            controlElement.SetAttribute("RenderMode",
                string.Format("{0:x}",(int)RenderState.RenderMode));
            controlElement.SetAttribute("NearZ", Camera.PerspectiveNearZ.ToString());
            controlElement.SetAttribute("FarZ", Camera.FarZ.ToString());
        }

        /// <summary>
        /// Loads the user's settings. Complements SaveState().</summary>
        /// <param name="controlElement">XML node containing settings</param>
        /// <param name="xmlDoc">XmlDocument that contains 'controlElement'</param>
        public virtual void LoadState(XmlElement controlElement, XmlDocument xmlDoc)
        {
            string renderModeAttribute = controlElement.GetAttribute("RenderMode");
            if (renderModeAttribute != string.Empty)
            {
                RenderState.RenderMode =
                    (RenderMode)int.Parse(renderModeAttribute, NumberStyles.AllowHexSpecifier);
            }

            string nearAttribute = controlElement.GetAttribute("NearZ");
            if (nearAttribute != string.Empty)
                Camera.PerspectiveNearZ = float.Parse(nearAttribute);

            string farAttribute = controlElement.GetAttribute("FarZ");
            if (farAttribute != string.Empty)
                Camera.FarZ = float.Parse(farAttribute);
        }

        /// <summary>
        /// Clears temporary references to prevent large amounts of managed memory
        /// from being held on to unnecessarily</summary>
        public virtual void Clear()
        {
            s_isManipulating = false;
            m_manipulatorHitRecords = null;
            m_renderAction.Clear();
            m_pickAction.Clear();
        }

        /// <summary>
        /// Gets whether or not this control has had its Invalidate method called but Paint() has not
        /// been called. Allows for a nice optimization to avoid calling Invalidate unnecessarily.</summary>
        public bool IsInvalidated
        {
            get { return m_invalidated; }
        }

        #region Overrides

        /// <summary>
        /// Gets or sets the background color of the DesignControl</summary>
        public override Color BackColor
        {
            get;set;
        }

        /// <summary>
        /// Performs a picking test to see if any manipulator is visible on the screen at the given coordinates</summary>
        /// <param name="x">X-coordinate in screen coordinates (like from MouseEventArgs)</param>
        /// <param name="y">Y-coordinate in screen coordinates (like from MouseEventArgs)</param>
        /// <param name="hits">Resulting HitRecord(s) if there was a manipulator found</param>
        /// <returns>The found manipulator or null if none found</returns>
        protected IManipulator TestForManipulator(int x, int y, out HitRecord[] hits)
        {
            IManipulator manipulator = null;
            m_pickAction.TypeFilter = typeof(IManipulator);

            SetCurrentContext();
            m_pickAction.Set(m_renderAction);
            m_pickAction.Init(Camera, x, y, x, y, true, false);
            Render(m_pickAction, true, true);
            hits = m_pickAction.GetHits();
            m_pickAction.TypeFilter = null;

            // Analyze hits
            if (hits.Length > 0)
                manipulator = hits[0].RenderObject as IManipulator;

            m_pickAction.Clear();

            return manipulator;
        }

        /// <summary>
        /// Raises the MouseDown event</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Check if we've hit a manipulator. Only control with the left mouse button so that
            //  right-clicks can bring up the context menu rather than hitting the manipulator.
            if (CanvasControl3D.ControlScheme.IsControllingCamera(ModifierKeys,e)==false &&
                e.Button == MouseButtons.Left)
            {
                FirstMousePoint = new Point(e.X, e.Y);
                Clear();

                HitRecord[] hits;
                IManipulator manipulator = TestForManipulator(e.X, e.Y, out hits);
                if (manipulator != null)
                {
                    SetManipulator(manipulator, hits);
                    OnManipulatorStart(new ManipulatorEventArgs(manipulator));
                    
                    int width = base.Width;
                    int height = base.Height;
                    float x = (float)e.X / (float)width - 0.5f;
                    float y = 1.0f - ((float)e.Y / (float)height) - 0.5f;
                    manipulator.OnHit(hits, x, y, m_pickAction, m_renderAction, Camera, Context);
                    
                    // Don't call base method if a manipulator was found. Why?
                    return;
                }
            }
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the MouseMove event</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (CanvasControl3D.ControlScheme.IsControllingCamera(ModifierKeys, e) == false &&
                m_manipulator != null)
            {
                // If we're actively dragging a manipulator, then handle that.
                if (m_manipulatorHitRecords != null)
                {
                    int width = base.Width;
                    int height = base.Height;
                    float x = (float)e.X / (float)width - 0.5f;
                    float y = 1.0f - ((float)e.Y / (float)height) - 0.5f;

                    try
                    {
                        // Push render state
                        m_scene.StateStack.Push(m_renderState);
                        m_manipulator.OnDrag(m_manipulatorHitRecords, x, y, m_pickAction, m_renderAction, Camera, Context);
                        m_scene.StateStack.Pop();

                        OnManipulatorUpdate(new ManipulatorEventArgs(m_manipulator));

                        // Redraw
                        Refresh();
                    }
                    catch (Exception ex)
                    {
                        Clear();
                        MessageBox.Show(this, ex.Message,
                            "Error".Localize("title of a dialog box that displays information about an error that has occurred"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                //Otherwise, since we know a manipulator is active (m_manipulator is not null), see if
                // the user has moved the cursor over it.
                else 
                {
                    HitRecord[] hits;

                    IManipulator manipulator = TestForManipulator(e.X, e.Y, out hits);
                    if (manipulator != null)
                    {
                        if (m_lastCursor == null)
                        {
                            m_lastCursor = Cursor;
                            Cursor = Cursors.SizeAll;
                        }
                    }
                    else
                    {
                        if (m_lastCursor != null)
                        {
                            Cursor = m_lastCursor;
                            m_lastCursor = null;
                        }
                    }
                    base.OnMouseMove(e);
                }
            }
            else
                base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the MouseUp event</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (m_manipulator != null &&
                m_manipulatorHitRecords != null)
            {
                int width = base.Width;
                int height = base.Height;
                float x = (float)e.X / (float)width - 0.5f;
                float y = 1.0f - ((float)e.Y / (float)height) - 0.5f;
                m_manipulator.OnEndDrag(m_manipulatorHitRecords, x, y, m_pickAction, m_renderAction, Camera, Context);

                OnManipulatorEnd(new ManipulatorEventArgs(m_manipulator));
                Clear();
                Invalidate();
            }
            else if (base.IsPicking)
            {
                SetCurrentContext();
                m_pickAction.TypeFilter = null;
                OnPicking(new EventArgs());
                bool multiSelect = base.DragOverThreshold;
                m_pickAction.Init(Camera, FirstMousePoint.X, FirstMousePoint.Y,
                    CurrentMousePoint.X, CurrentMousePoint.Y, multiSelect, true);

                // end drag operations, so the selection rectangle doesn't get drawn
                Render(m_pickAction, true, false);

                base.OnMouseUp(e);

                //Get all objects under the cursor. Let consumer of this event have a
                // full hit array even for single selection in case hitArray[0] doesn't
                // meet the type requirement.
                HitRecord[] hitArray = m_pickAction.GetHits(true);
                OnPicked(new PickEventArgs(hitArray, multiSelect));
            }
            else
            {
                base.OnMouseUp(e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnGotFocus(EventArgs e)
        {
            SetCurrentContext();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragEnter"></see> event</summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs"></see> that contains the event data</param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            Focus();
            base.OnDragEnter(drgevent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Render(m_renderAction, false, false);
            m_invalidated = false;
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.Invalidated event with a specified region of the
        /// control to invalidate</summary>
        /// <param name="invalidatedArea">A System.Drawing.Rectangle representing the area to invalidate</param>
        protected override void NotifyInvalidate(Rectangle invalidatedArea)
        {
            if (!m_invalidated)
            {
                base.NotifyInvalidate(invalidatedArea);
                m_invalidated = true;
            }
        }

        /// <summary>
        /// Raises the Resize event</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected override void OnResize(EventArgs e)
        {
            // Update RenderAction's view frustum
            if (m_renderAction != null)
            {
                if (Width > 0 && Height > 0)
                {

                    Camera.Aspect = (float)Width / (float)Height;
                }
            }
            base.OnResize(e);

        }

        /// <summary>
        /// Function called before OnMouseMove() is called. If base.WndProc() is not called,
        /// then OnMouseMove() is not called. Thus, we can consume messages that we don't want
        /// to pass on to OnMouseMove(), etc. In order to improve performance when the user
        /// is dragging around an object and is snapping that object to other objects in a
        /// large level, we want to ignore the current mouse movement message if there is another
        /// mouse movement message in the queue. This fixes the "delayed trail" problem.</summary>
        /// <param name="msg">Windows message</param>
        protected override void WndProc(ref Message msg)
        {
            if (msg.Msg == Sce.Atf.User32.WM_MOUSEMOVE)
            {
                // first, determine if the mouse has really moved. there are many spurious messages, potentially.
                int xy = msg.LParam.ToInt32();
                if (s_oldXY == xy)
                    return;
                s_oldXY = xy;

                // secondly, determine if there is a newer mouse move message available already.
                Sce.Atf.User32.MSG newerMsg;
                if (Sce.Atf.User32.PeekMessage(out newerMsg, 0, Sce.Atf.User32.WM_MOUSEMOVE, Sce.Atf.User32.WM_MOUSEMOVE,
                    Sce.Atf.User32.PM_NOREMOVE | Sce.Atf.User32.PM_QS_INPUT | Sce.Atf.User32.PM_NOYIELD))
                {
                    // Documentation says that PeekMessage does not filter out WM_QUIT messages, so let's
                    // make sure that we have a mouse move message.
                    if (newerMsg.msg == Sce.Atf.User32.WM_MOUSEMOVE)
                        return;
                }
            }
            base.WndProc(ref msg);
        }

        #endregion

        /// <summary>
        /// Raises the Picked event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnPicked(PickEventArgs e)
        {
            EventHandler<PickEventArgs> handler = Picked;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the Picking event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnPicking(EventArgs e)
        {
            EventHandler handler = Picking;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the ManipulatorStart event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnManipulatorStart(ManipulatorEventArgs e)
        {
            EventHandler<ManipulatorEventArgs> handler = ManipulatorStart;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the ManipulatorUpdate event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnManipulatorUpdate(ManipulatorEventArgs e)
        {
            EventHandler<ManipulatorEventArgs> handler = ManipulatorUpdate;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the ManipulatorEnd event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnManipulatorEnd(ManipulatorEventArgs e)
        {
            EventHandler<ManipulatorEventArgs> handler = ManipulatorEnd;
            if (handler != null)
                handler(this, e);
        }

        // For 'action', first set the TypeFilter and call Init. If picking is true and pickManipulatorOnly
        // is true, then only the currently active manipulator is tested against, which is a huge speed-up.
        private void Render(IRenderAction action, bool picking, bool pickManipulatorOnly)
        {
            base.BeginPaint(picking);

            action.Title = Camera.ViewTypeName;
            action.ViewportWidth = Width;
            action.ViewportHeight = Height;
            
            // If we're doing a picking operation, then we don't need to display the results to the user, etc.
            if (picking)
            {
                RenderState pickState = new RenderState(m_renderState);
                pickState.RenderMode &= ~(RenderMode.Textured | RenderMode.Lit | RenderMode.Alpha);
                pickState.OverrideChildState = RenderMode.Textured | RenderMode.Lit | RenderMode.Alpha;
                pickState.InheritState = 0;

                RenderStateGuardian.Reset();

                // Manipulator picking can be further optimized to only render the manipulator.
                if (pickManipulatorOnly &&
                    m_manipulator != null &&
                    m_manipulatorNode != null)
                {
                    // Replace the scene node's children temporarily with just our one child of interest.
                    // Replacing the children is easier than creating a new Scene because the manipulators
                    //  need the HitRecord.GraphPath to have the full correct Scene, not a temp Scene.
                    List<SceneNode> originalChildren = new List<SceneNode>(m_scene.Children);
                    m_scene.Children.Clear();
                    m_scene.Children.Add(m_manipulatorNode);

                    m_scene.StateStack.Push(pickState);
                    action.PushMatrix(m_manipulatorParentToWorld, false);
                    action.Dispatch(m_scene, Camera);
                    action.PopMatrix();
                    m_scene.StateStack.Pop();

                    m_scene.Children.Clear();
                    foreach (SceneNode node in originalChildren)
                        m_scene.Children.Add(node);
                }
                else
                {   //Regular picking. Render the whole scene.
                    m_scene.StateStack.Push(pickState);
                    action.Dispatch(m_scene, Camera);
                    m_scene.StateStack.Pop();
                }

                // When picking, we don't want to display this buffer, so don't swap OpenGl buffers.
                // This property gets reset by base.EndPaint().
                vSwapBuffers = false;
            }
            else
            {
                m_scene.StateStack.Push(m_renderState);
                RenderStateGuardian.Reset();
                action.Dispatch(m_scene, Camera);
                RenderAxisSystem(action);
                m_scene.StateStack.Pop();
            }

            base.EndPaint();
        }

        private void RenderAxisSystem(IRenderAction action)
        {
            // Calc width and height
            float width, height;
            float nearP = Camera.Frustum.Near * 1.1f;

            if (Camera.Frustum.IsOrtho)
            {
                width = (Camera.Frustum.Right - Camera.Frustum.Left) / 2 * 0.95f;
                height = (Camera.Frustum.Top - Camera.Frustum.Bottom) / 2 * 0.90f;
            }
            else
            {
                //  Set the projection to orthogonal for perspective views
                float h = (float)Height / (float)Width;

                OTK.OpenGL.GL.MatrixMode(OTK.OpenGL.MatrixMode.Projection);
                OTK.OpenGL.GL.LoadIdentity();

                OTK.OpenGL.GL.Ortho(-1, 1, -h, h, 1, 1000);
                nearP = 1.1f;
                width = 0.92f;
                height = h * 0.90f;
            }

            // Push the view matrix
            OTK.OpenGL.GL.MatrixMode(OTK.OpenGL.MatrixMode.Modelview);
            Matrix4F V = new Matrix4F(Camera.ViewMatrix);
            V.Translation = Vec3F.ZeroVector;

            // Disable lighting
            OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Lighting);
            OTK.OpenGL.GL.Disable(OTK.OpenGL.EnableCap.Texture2D);

            OTK.OpenGL.GL.PushMatrix();
            OTK.OpenGL.GL.LoadIdentity();
            OTK.OpenGL.GL.Translate(-width, -height, nearP);
            Util3D.glMultMatrixf(V);

            // Render the system
            RenderAxis(width/15);

            OTK.OpenGL.GL.PopMatrix();
        }

        private void RenderAxis(float s)
        {
            // Render X
            OTK.OpenGL.GL.Color3(1.0f, 0.0f, 0.0f);

            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.Vertex3(s, 0, 0);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.RasterPos3(s, 0, 0);
            OTK.OpenGL.GL.CallLists(1, OTK.OpenGL.ListNameType.UnsignedByte, Marshal.StringToHGlobalAnsi("x"));

            // Render Y
            OTK.OpenGL.GL.Color3(0.0f, 1.0f, 0.0f);

            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.Vertex3(0, s, 0);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.RasterPos3(s, 0, 0);
            OTK.OpenGL.GL.CallLists(1, OTK.OpenGL.ListNameType.UnsignedByte, Marshal.StringToHGlobalAnsi("y"));

            // Render Z
            OTK.OpenGL.GL.Color3(0.0f, 0.0f,1.0f);

            OTK.OpenGL.GL.Begin(OTK.OpenGL.PrimitiveType.Lines);
            OTK.OpenGL.GL.Vertex3(0, 0, 0);
            OTK.OpenGL.GL.Vertex3(0, 0, s);
            OTK.OpenGL.GL.End();

            OTK.OpenGL.GL.RasterPos3(s, 0, 0);
            OTK.OpenGL.GL.CallLists(1, OTK.OpenGL.ListNameType.UnsignedByte, Marshal.StringToHGlobalAnsi("z"));
        }

        private void SetManipulator(IManipulator manipulator, HitRecord[] hits)
        {
            s_isManipulating = true;
            m_manipulatorHitRecords = hits;
        }

        private readonly Scene m_scene;
        private IRenderAction m_renderAction;
        private IPickAction m_pickAction;
        private IManipulator m_manipulator;
        private SceneNode m_manipulatorNode;
        private Matrix4F m_manipulatorParentToWorld;
        private HitRecord[] m_manipulatorHitRecords; //hit records for a mousedown or mouse drag operation on a manipulator
        private readonly RenderState m_renderState = new RenderState();
        private bool m_invalidated;
        private Cursor m_lastCursor;

        private static bool s_isManipulating;
        
        // for WndProc, to know if the mouse has really moved in a WM_MOUSEMOVE message.
        private static int s_oldXY = -1;
    }
}
