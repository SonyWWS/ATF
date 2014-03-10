//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

using Tao.OpenGl;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Standard implementation of IRenderAction</summary>
    public class RenderAction : IRenderAction
    {
        /// <summary>
        /// Constructs a RenderAction</summary>
        /// <param name="renderStateGuardian">Render state guardian</param>
        public RenderAction(RenderStateGuardian renderStateGuardian)
        {
            m_renderStateGuardian = renderStateGuardian;
            Clear();
        }

        #region IRenderAction Members

        /// <summary>
        /// Copies all of the given render action's properties to this instance</summary>
        /// <param name="other">Other render action</param>
        public void Set(IRenderAction other)
        {
            m_renderStateGuardian = other.RenderStateGuardian;
            m_height = other.ViewportHeight;
            m_width = other.ViewportWidth;
        }

        /// <summary>
        /// Builds a traverse list from the Scene and dispatches it for rendering</summary>
        /// <param name="scene">The scene to dispatch</param>
        /// <param name="camera">The camera</param>
        public virtual void Dispatch(Scene scene, Camera camera)
        {
            Util3D.RenderStats.ResetFrame();
            PreDispatch(scene, camera);

            // Clear traverse list
            m_traverseList.Clear();
            m_traverseList.SetViewMatrix(camera.ViewMatrix);

            s_stopWatch.Reset();
            s_stopWatch.Start();
            Util3D.RenderStats.TimeForConstraints = s_stopWatch.ElapsedMilliseconds;

            if (m_width > 0 && m_height > 0)
            {
                s_stopWatch.Reset();
                s_stopWatch.Start();
                BuildTraverseList(camera, m_traverseList, scene);
                Util3D.RenderStats.TraverseNodeCount = m_traverseList.Count;
                Util3D.RenderStats.TimeForTraverse = s_stopWatch.ElapsedMilliseconds;

                s_stopWatch.Reset();
                s_stopWatch.Start();
                RenderPass(m_traverseList, camera);
                Util3D.RenderStats.TimeForDispatchTraverseList = s_stopWatch.ElapsedMilliseconds;

                DrawStats(camera);
            }

            PostDispatch(scene, camera);
        }

        /// <summary>
        /// Dispatches the given traverse list for rendering</summary>
        /// <param name="traverseList">The traverse list</param>
        /// <param name="scene">The Scene to dispatch</param>
        /// <param name="camera">The camera</param>
        public virtual void Dispatch(ICollection<TraverseNode> traverseList, Scene scene, Camera camera)
        {
            Util3D.RenderStats.ResetFrame();
            PreDispatch(scene, camera);

            // Dispatch traverse list
            if (m_width > 0 && m_height > 0)
            {
                foreach (TraverseNode node in traverseList)
                {
                    PushMatrix(node.Transform, false);
                    node.RenderObject.Dispatch(node.GraphPath, node.RenderState, this, camera);
                    PopMatrix();
                }
            }

            PostDispatch(scene, camera);
        }

        /// <summary>
        /// Builds a traverse list from the given scene</summary>
        /// <param name="camera">The camera</param>
        /// <param name="scene">The Scene for which to build the traverse list</param>
        /// <returns>The traverse list</returns>
        public ICollection<TraverseNode> BuildTraverseList(Camera camera, Scene scene)
        {
            m_traverseList.Clear();
            BuildTraverseList(camera, m_traverseList, scene);
            return m_traverseList;
        }

        /// <summary>
        /// Clears out references to all the objects that were used by Dispatch and BuildTraverseList methods
        /// to prevent large amounts of managed memory from being held on to unnecessarily</summary>
        public virtual void Clear()
        {
            m_traverseList.Clear();

            m_matrixStack.Clear();
            m_matrixStack.Add(Matrix4F.Identity);

            m_renderStack.Clear();
            RenderState defaultRenderState = new RenderState();
            defaultRenderState.RenderMode = RenderMode.Smooth;
            m_renderStack.Push(defaultRenderState);
            
            m_graphPath.Clear();
            m_renderObject = null;
            ResetNodePool();
        }

        /// <summary>
        /// Gets the current RenderState</summary>
        public RenderState RenderState
        {
            get { return m_renderStack.Peek(); }
        }

        /// <summary>
        /// Gets the current TraverseState</summary>
        public TraverseState TraverseState
        {
            get { return m_traverseState; }
        }

        /// <summary>
        /// Gets the current RenderObject being traversed</summary>
        public IRenderObject RenderObject
        {
            get { return m_renderObject; }
        }

        /// <summary>
        /// Gets top matrix in the matrix stack</summary>
        public Matrix4F TopMatrix
        {
            get
            {
                int top = m_matrixStack.Count - 1;
                if (top < 0)
                    throw new InvalidOperationException("Matrix stack is empty");

                return m_matrixStack[top];
            }
        }

        /// <summary>
        /// Pushes a matrix onto the matrix stack</summary>
        /// <param name="matrix">The matrix</param>
        /// <param name="multiply">If true, multiply matrix by top matrix</param>
        public void PushMatrix(Matrix4F matrix, bool multiply)
        {
            if (m_matrixStackFreeze)
                throw new InvalidOperationException(
                    "the IRenderObject does not implement ISetsLocalTransform");

            int count = m_matrixStack.Count;
            Matrix4F topMatrix = (count > 0) ? m_matrixStack[count - 1] : Matrix4F.Identity;

            Matrix4F newMatrix = new Matrix4F();
            if (multiply)
                newMatrix.Mul(matrix, topMatrix);
            else
                newMatrix.Set(matrix);

            m_matrixStack.Add(newMatrix);
        }

        /// <summary>
        /// Pops a matrix from the matrix stack. Throws an InvalidOperationException
        /// if the stack is empty.</summary>
        /// <returns>The topmost matrix on the stack</returns>
        public Matrix4F PopMatrix()
        {
            int count = m_matrixStack.Count;
            if (count <= 0)
                throw new InvalidOperationException("Matrix stack is empty");

            Matrix4F matrix = m_matrixStack[count - 1];
            m_matrixStack.RemoveAt(count - 1);
            return matrix;
        }

        /// <summary>
        /// Gets a matrix from the matrix stack</summary>
        /// <param name="relativeIndex">The relative index starting from the top.
        /// 0 is the top, -1 is the next matrix below, etc.</param>
        /// <returns>The matrix at the specified relative position</returns>
        /// <exception cref="InvalidOperationException">Invalid relative index</exception>
        public Matrix4F GetMatrixAt(int relativeIndex)
        {
            int count = m_matrixStack.Count;
            int index = count - 1 - relativeIndex;
            if (index < 0 || index >= count)
                throw new InvalidOperationException("Invalid relative index");

            return (m_matrixStack[index]);

        }

        /// <summary>
        /// Returns either a new or previously used TraverseNode for use by IRenderObject's
        /// Traverse(). The caller must place this node on the traverse list and not maintain
        /// a permanent reference to it.</summary>
        /// <returns>Either a new or previously used TraverseNode</returns>
        public TraverseNode GetUnusedNode()
        {
            TraverseNode node;
            if (m_nextUnusedNode < m_nodePool.Count)
            {
                node = m_nodePool[m_nextUnusedNode];
            }
            else
            {
                node = new TraverseNode();
                m_nodePool.Add(node);
            }
            m_nextUnusedNode++;

            return node;
        }

        /// <summary>
        /// Resets the pool of TraverseNodes, assuming that all of these can be reused</summary>
        private void ResetNodePool()
        {
            int i = 0;
            foreach (TraverseNode node in m_nodePool)
            {
                if (i++ == m_nextUnusedNode)
                    break;
                node.Reset();
            }
            m_nextUnusedNode = 0;
        }

        /// <summary>
        /// Gets the render state guardian</summary>
        public RenderStateGuardian RenderStateGuardian
        {
            get { return m_renderStateGuardian; }
        }
        
        /// <summary>
        /// Gets and sets the title</summary>
        public string Title
        {
            get { return m_title; }
            set { m_title = value; }
        }

        /// <summary>
        /// Gets and sets the viewport width</summary>
        public int ViewportWidth
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        /// Gets and sets the viewport height</summary>
        public int ViewportHeight
        {
            get { return m_height; }
            set { m_height = value; }
        }
        #endregion

        /// <summary>
        /// Gets the current frame number</summary>
        public int Frame
        {
            get { return Util3D.RenderStats.FrameCount; }
        }

        /// <summary>
        /// Called after each pass by the Dispatch() method</summary>
        /// <param name="pass">The pass number</param>
        /// <param name="traverseList">The travesae list being dispatched</param>
        protected virtual void OnPostRenderPass(int pass, ICollection<TraverseNode> traverseList)
        {
        }

        /// <summary>
        /// Dispatches the traverse list (called for each pass by the Dispatch() method).
        /// Updates the matrix stack for each render object in the traverse list and 
        /// then calls IRenderObject.Dispatch.</summary>
        /// <param name="traverseList">Traverse list</param>
        /// <param name="camera">The camera</param>
        protected void RenderPass(IEnumerable<TraverseNode> traverseList, Camera camera)
        {
            foreach (TraverseNode node in traverseList)
            {
                PushMatrix(node.Transform, false);
                //Console.WriteLine("sending node: {0}", node.RenderState.RenderMode);
                node.RenderObject.Dispatch(node.GraphPath, node.RenderState, this, camera);
                PopMatrix();
            }
        }

        /// <summary>
        /// Traverses the render graph in depth-first order and builds the traverse list</summary>
        /// <param name="camera">The camera</param>
        /// <param name="traverseList">The traverse list being built</param>
        /// <param name="node">The root graph node to traverse</param>
        protected void BuildTraverseList(Camera camera,
            ICollection<TraverseNode> traverseList,
            SceneNode node)
        {
            if (m_reentryGuard.HasEntered == false)
                ResetNodePool();

            using(m_reentryGuard.EnterAndExitMultiple())
                _BuildTraverseList(camera, traverseList, node);
        }
        // A recursive private function. Requires that ResetNodePool() be called first.
        private void _BuildTraverseList(Camera camera,
            ICollection<TraverseNode> traverseList,
            SceneNode node)
        {
            if (!node.IsVisibile)
                return;

            // Append node to graph path
            m_graphPath.Push(node);

            if (node.StateStack.Count > 0)
            {
                RenderState newState = node.StateStack.ComposedRenderState;
                RenderState parent = m_renderStack.Peek();
                newState.ComposeFrom(parent);
                m_renderStack.Push(newState);
            }            

            m_traverseState = TraverseState.None;

            foreach (IRenderObject renderObject in node.RenderObjects)
            {
                if (!(renderObject is ISetsLocalTransform))
                    m_matrixStackFreeze = true;
                
                m_renderObject = renderObject;
                m_traverseState = renderObject.Traverse(m_graphPath, this, camera, traverseList);

                m_matrixStackFreeze = false;
            }

            if (m_traverseState != TraverseState.Cull)
            {
                foreach (SceneNode child in node.Children)
                {
                    _BuildTraverseList(camera, traverseList, child);
                }
            }

            foreach (IRenderObject renderObject in node.RenderObjects)
            {
                m_renderObject = renderObject;
                renderObject.PostTraverse(m_graphPath, this, camera, traverseList);
            }

            if (node.StateStack.Count > 0)
            {
                m_renderStack.Pop();
            }

            // Remove node from graph path
            m_graphPath.Pop();
        }

        /// <summary>
        /// Traverses the given sub-graph and populates the traverse list</summary>
        /// <param name="camera">The camera</param>
        /// <param name="traverseList">The traverse list being built</param>
        /// <param name="node">The root graph node to traverse</param>
        /// <param name="transform">The local transform matrix, i.e., the transform from the parent to this render object</param>
        public void TraverseSubGraph(Camera camera, ICollection<TraverseNode> traverseList,
            SceneNode node, Matrix4F transform)
        {
            PushMatrix(transform, true);

            BuildTraverseList(camera, traverseList, node);

            PopMatrix();
        }

        /// <summary>
        /// Called before dispatching the scene from the Dispatch method</summary>
        /// <param name="scene">The scene being dispatched</param>
        /// <param name="camera">The Camera</param>
        protected virtual void PreDispatch(Scene scene, Camera camera)
        {
            SetupProjection(camera);
            SetupView(camera);

            // set the clear values
            Color backgroundColor = scene.BackgroundColor;
            Gl.glClearColor(
                backgroundColor.R * (1.0f / 255),
                backgroundColor.G * (1.0f / 255),
                backgroundColor.B * (1.0f / 255),
                0);
            Gl.glClearDepth(1.0);

            // ensure that the buffers are writeable
            Gl.glDepthMask(Gl.GL_TRUE);
            Gl.glColorMask(Gl.GL_TRUE, Gl.GL_TRUE, Gl.GL_TRUE, Gl.GL_TRUE);

            // clear the buffers
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
        }

        /// <summary>
        /// Sets up projection, given a Camera</summary>
        /// <param name="camera">Camera that determines projection</param>
        protected virtual void SetupProjection(Camera camera)
        {
            Gl.glViewport(0, 0, m_width, m_height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Util3D.glMultMatrixf(camera.ProjectionMatrix);
        }

        /// <summary>
        /// Sets up the view matrix, given a Camera</summary>
        /// <param name="camera">Camera that determines view matrix</param>
        protected virtual void SetupView(Camera camera)
        {
            Gl.glMatrixMode(Gl.GL_TEXTURE);
            Gl.glLoadIdentity();
            Gl.glMultMatrixf(TextureMatrix);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Util3D.glMultMatrixf(camera.ViewMatrix);
        }

        /// <summary>
        /// Texture matrix</summary>
        public static float[] TextureMatrix = new float[]
            {
                1, 0, 0, 0,
                0, -1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

        /// <summary>
        /// Called after dispatching the scene from the Dispatch method</summary>
        /// <param name="scene">The scene being dispatched</param>
        /// <param name="camera">A Camera</param>
        protected virtual void PostDispatch(Scene scene, Camera camera)
        {
        }

        private void DrawStats(Camera camera)
        {
            string message;
            if (Util3D.RenderStats.Enabled == true)
            {
                message = string.Format("{0}; {1}", m_title, Util3D.RenderStats.GetDisplayString());
            }
            else
            {
                message = m_title;
            }

            // Choose the z value to be slightly in front of the near clipping plane.
            // Remember that Near is negative in an orthographic camera.
            double nearP = camera.Frustum.Near + Math.Abs(camera.Frustum.Near * 0.05);

            double height, width;
            if (camera.Frustum.IsOrtho)
            {
                width = (camera.Frustum.Right - camera.Frustum.Left) / 2 * 0.95f;
                height = (camera.Frustum.Top - camera.Frustum.Bottom) / 2 * 0.95f;
            }
            else
            {
                width = nearP * Math.Tan(camera.Frustum.FovX / 2) * 0.95;
                height = nearP * Math.Tan(camera.Frustum.FovY / 2) * 0.95;
            }

            RenderState rs = new RenderState();
            rs.RenderMode = RenderMode.Smooth | RenderMode.SolidColor;
            rs.SolidColor = new Vec4F(1, 1, 1, 1);
            m_renderStateGuardian.Commit(rs);

            Gl.glPushMatrix();
            Gl.glLoadIdentity();
            Gl.glTranslated(-width, height, -nearP);
            Gl.glRasterPos2i(0, 0);
            OpenGlCore.DrawText(message);
            Gl.glPopMatrix();
        }

        /// <summary>
        /// Render state sorter</summary>
        protected RenderStateSorter m_traverseList = new RenderStateSorter();

        /// <summary>
        /// Matrix stack</summary>
        protected List<Matrix4F> m_matrixStack = new List<Matrix4F>();

        /// <summary>
        /// Graph path</summary>
        protected Stack<SceneNode> m_graphPath = new Stack<SceneNode>();

        /// <summary>
        /// Render stack</summary>
        protected Stack<RenderState> m_renderStack = new Stack<RenderState>();

        /// <summary>
        /// Traverse stack</summary>
        protected TraverseState m_traverseState;

        /// <summary>
        /// Render object</summary>
        protected IRenderObject m_renderObject;

        /// <summary>
        /// Width</summary>
        protected int m_width;

        /// <summary>
        /// Height</summary>
        protected int m_height;

        /// <summary>
        /// Title</summary>
        protected string m_title;

        /// <summary>
        /// Current pass number</summary>
        protected int m_curPass;

        /// <summary>
        /// Render state guardian</summary>
        protected RenderStateGuardian m_renderStateGuardian;

        /// <summary>
        /// Matrix stack freeze</summary> 
        protected bool m_matrixStackFreeze;

        private readonly ReentryGuard m_reentryGuard = new ReentryGuard();
        
        /// <summary>
        /// Stop watch</summary>
        protected static Stopwatch s_stopWatch = new Stopwatch();

        // These work together. m_nodePool is treated as an array of nodes that is
        //  indexed by m_nextUnusedNode. To reset the pool, m_nextUnusedNode is
        //  simply set to zero. Documentation says that List<>[index] is O(1).
        private readonly List<TraverseNode> m_nodePool = new List<TraverseNode>();
        private int m_nextUnusedNode;
    }
}
