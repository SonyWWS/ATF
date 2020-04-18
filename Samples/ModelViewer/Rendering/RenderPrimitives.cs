//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

using OpenTK.Graphics.OpenGL;

namespace ModelViewerSample.Rendering
{
    /// <summary>
    /// RenderObject for rendering a primitive set implementing the IPrimitiveSet interface.
    /// Builds the display lists (the OpenGl vertex buffer objects) the first time that this object
    /// is to be displayed. Updates rendering statistics.</summary>
    public class RenderPrimitives : RenderObject, IRenderPick, IRenderThumbnail
    {
        /// <summary>
        /// Performs one-time initialization to create primitive and submesh lists
        /// when this adapter's DomNode property is set.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_primitives = this.As<IPrimitiveSet>();
            m_subMesh = this.As<ISubMesh>();       
        }

        /// <summary>
        /// Traverses the specified graph path.</summary>
        /// <param name="graphPath">The graph path</param>
        /// <param name="action">The render action</param>
        /// <param name="camera">The camera</param>
        /// <param name="list">The list</param>
        /// <returns></returns>
        public override TraverseState Traverse(Stack<SceneNode> graphPath, IRenderAction action,
            Camera camera, ICollection<TraverseNode> list)
        {
            // If invisible then cull
            if (!m_primitives.Visible)
                return TraverseState.Cull;

            return base.Traverse(graphPath, action, camera, list);
        }

        /// <summary>
        /// Gets and sets the name of the array element representing the normals</summary>
        public static string NormalsTag = "normal";

        /// <summary>
        /// Gets and sets the name of the array element representing the UV coordinates for
        /// the diffuse texture</summary>
        public static string TextureCoordinatesTag = "map1";

        /// <summary>
        /// Gets and sets the name of the array element representing the vertex colors</summary>
        public static string ColorsTag = "color";

        /// <summary>
        /// Gets and sets the name of the array element representing the vertices</summary>
        public static string VerticesTag = "position";

        /// <summary>
        /// Initializes the render object</summary>
        /// <returns>true, iff initialization was successful</returns>
        public override bool Init(SceneNode node)
        {
            IMesh mesh = TryGetMesh();

            if (mesh != null)
            {
                m_bindingCount = m_primitives.BindingCount;

                // determine primitive type
                if (m_primitives.PrimitiveType == "POLYGONS")
                    m_drawMode = (int)OpenTK.Graphics.OpenGL.All.Polygon;
                else if (m_primitives.PrimitiveType == "TRIANGLES")
                    m_drawMode = (int)OpenTK.Graphics.OpenGL.All.Triangles;
                else if (m_primitives.PrimitiveType == "TRISTRIPS")
                    m_drawMode = (int)OpenTK.Graphics.OpenGL.All.TriangleStrip;

                // add draw commands
                AddCommand( NormalsTag, Function.glNormal3fv);
                AddCommand( TextureCoordinatesTag, Function.glTexCoord2fv);
                AddCommand( ColorsTag, Function.glColor3fv);
                AddCommand( VerticesTag, Function.glVertex3fv);

                // get the shader
                IShader shader = m_primitives.Shader;
                IRenderStateCreator renderStateCreator = null;
                if (shader != null)
                    renderStateCreator = shader.As<IRenderStateCreator>();

                if (renderStateCreator != null)
                {
                    RenderState renderState = renderStateCreator.CreateRenderState();
                    node.StateStack.Push(renderState);
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the object space bounding box</summary>
        /// <returns>Object space bounding box</returns>
        protected override Box GetBoundingBoxObjectSpace()
        {
            IMesh mesh = TryGetMesh();
            if (mesh != null)
                return mesh.BoundingBox;

            return new Box(new Vec3F(0, 0, 0), new Vec3F(0, 0, 0));
        }

        /// <summary>
        /// Releases all resources</summary>
        public override void Release()
        {
            if (m_displayListId != 0)
            {
                GL.DeleteLists(m_displayListId, 1);
                m_displayListId = 0;
                #if MEMORY_DEBUG
                lock(s_lock) NumDisplayListIds--;
                #endif
            }
            base.Release();
        }

        /// <summary>
        /// Renders the specified graph path</summary>
        /// <param name="graphPath">The graph path</param>
        /// <param name="renderState">The render state</param>
        /// <param name="action">The render action</param>
        /// <param name="camera">The camera</param>
        protected override void Render(SceneNode[] graphPath, RenderState renderState, IRenderAction action, Camera camera)
        {
            // apply xform
            GL.PushMatrix();
            Util3D.glMultMatrixf(action.TopMatrix);

            if (m_displayListId == 0)
            {
                RenderStats globalStats = Util3D.RenderStats;
                RenderStats ourStats = new RenderStats();
                Util3D.RenderStats = ourStats;

                m_displayListId = GL.GenLists(1);
                #if MEMORY_DEBUG
                lock(s_lock) NumDisplayListIds++;
                #endif
                GL.NewList(m_displayListId, ListMode.Compile);
                Render(action);
                GL.EndList();

                m_numPrimitives = ourStats.PrimCount;
                m_numVertices = ourStats.VertexCount;
                Util3D.RenderStats = globalStats;
            }

            GL.CallList(m_displayListId);

            GL.PopMatrix();
            Util3D.RenderStats.PrimCount += m_numPrimitives;
            Util3D.RenderStats.VertexCount += m_numVertices;
        }

        /// <summary>
        /// Custom pick rendering</summary>
        /// <param name="graphPath">The graph path</param>
        /// <param name="renderState">The render state</param>
        /// <param name="action">The render action</param>
        /// <param name="camera">The camera</param>
        public void PickDispatch(SceneNode[] graphPath, RenderState renderState, IRenderAction action, Camera camera)
        {
            action.RenderStateGuardian.Commit(renderState);

            // apply xform
            GL.PushMatrix();
            Util3D.glMultMatrixf(action.TopMatrix);
            RenderVertices(action);
            GL.PopMatrix();
        }

        private IMesh TryGetMesh()
        {
            IMesh mesh = null;
            if (m_subMesh != null)
            {
                mesh = m_subMesh.Parent;
            }
            else
            {
                // Get the parent Mesh
                DomNode vertexArray = DomNode.Parent;
                if (vertexArray != null)
                {
                    DomNode meshObj = vertexArray.Parent;
                    if (meshObj != null)
                        mesh = meshObj.As<IMesh>();
                }
            }
            return mesh;
        }

        private void Render(IRenderAction action)
        {
            int[] sizes = m_primitives.PrimitiveSizes;
            int[] indices = m_primitives.PrimitiveIndices;

            int nPrims = (m_subMesh != null) ? m_subMesh.Count : sizes.Length;
            int primBaseIndex = 0;
          
            Util3D.RenderStats.PrimCount += nPrims;
            for (int i = 0; i < nPrims; i++)
            {
                int primSize = sizes[i % sizes.Length];
                Util3D.RenderStats.VertexCount += primSize;
                GL.Begin((PrimitiveType)m_drawMode);
                
                for (int j = 0; j < primSize; j++)
                {
                    foreach (DrawCommand cmd in m_commands)
                    {
                        int offset = indices[primBaseIndex + j * m_bindingCount + cmd.PrimitiveIndex] * cmd.Stride;
                        CallFunction(cmd, offset);
                    }
                }
                GL.End();
                primBaseIndex += primSize * m_bindingCount;
            }
        }

        private void RenderVertices(IRenderAction action)
        {
            int[] sizes = m_primitives.PrimitiveSizes;
            int[] indices = m_primitives.PrimitiveIndices;
            int nPrims = (m_subMesh != null) ? m_subMesh.Count : sizes.Length;

            int primBaseIndex = 0;

            Util3D.RenderStats.PrimCount += nPrims;
            for (int i = 0; i < nPrims; i++)
            {
                int primSize = sizes[i % sizes.Length];
                Util3D.RenderStats.VertexCount += primSize;
                GL.Begin((PrimitiveType)m_drawMode);
                for (int j = 0; j < primSize; j++)
                {
                    int offset = indices[primBaseIndex + j * m_bindingCount + m_vxCommand.PrimitiveIndex] * m_vxCommand.Stride;
                    CallFunction(m_vxCommand, offset);
                }
                GL.End();
                primBaseIndex += primSize * m_bindingCount;
            }
        }

        /// <summary>
        /// Executes the rendering function.</summary>
        /// <param name="cmd">The CMD.</param>
        /// <param name="offset">The offset.</param>
        internal void CallFunction(DrawCommand cmd, int offset)
        {
            float[] f = cmd.VertexBuffer;
            switch (cmd.Function)
            {
                case Function.glColor3fv:
                    GL.Color3(f[offset], f[offset + 1], f[offset + 2]);
                    break;
                case Function.glNormal3fv:
                    GL.Normal3(f[offset], f[offset + 1], f[offset + 2]);
                    break;
                case Function.glTexCoord2fv:
                    GL.TexCoord2(f[offset], f[offset + 1]);
                    break;
                case Function.glVertex3fv:
                    GL.Vertex3(f[offset], f[offset + 1], f[offset + 2]);
                    break;
            }
        }

        private void AddCommand(string bindingName, Function func)
        {

            ISubMesh submesh = this.As<ISubMesh>();
            IEnumerable<IDataSet> datasets;
            if (submesh != null)
            {
                datasets = submesh.DataSets;
            }
            else
            {
                IMesh mesh = TryGetMesh();
                datasets = mesh.DataSets;
            }
                 
            int bindingIndex = m_primitives.FindBinding(bindingName);
            if (bindingIndex != -1)
            {
                // Find the data Set
                foreach (IDataSet dataSet in datasets)
                {
                    if (dataSet.Name == bindingName)
                    {
                        DrawCommand cmd = new DrawCommand(func, bindingIndex, dataSet.Data, dataSet.ElementSize);
                        m_commands.Add(cmd);

                        if (func == Function.glVertex3fv)
                        {
                            m_vxCommand = cmd;
                        }
                        break;
                    }
                }
            }
        }

        internal enum Function
        {
            glNormal3fv,
            glTexCoord2fv,
            glColor3fv,
            glVertex3fv,
        }

        /// <summary>
        /// Helper class for dispatching a vertex array</summary>
        internal class DrawCommand
        {
            public DrawCommand(Function function, int primitiveIndex, float[] vertexBuffer, int stride)
            {
                Function = function;
                PrimitiveIndex = primitiveIndex;
                VertexBuffer = vertexBuffer;
                Stride = stride;
            }

            public Function Function;
            public int PrimitiveIndex;
            public float[] VertexBuffer;
            public int Stride;
        };

        #if MEMORY_DEBUG
        private static int NumDisplayListIds;
        private static object s_lock = new object();
        ~RenderPrimitives()
        {
            if (m_displayListId != 0)
                throw new InvalidOperationException("OpenGl display list was leaked");
        }
        #endif

        protected IPrimitiveSet m_primitives;

        private ISubMesh m_subMesh;
             
        internal List<DrawCommand> m_commands = new List<DrawCommand>();
        internal DrawCommand m_vxCommand;
        protected int m_bindingCount;
        private int m_drawMode;    
        private int m_displayListId;
        private int m_numPrimitives, m_numVertices; //for reporting statistics to RenderStats
    }
}
