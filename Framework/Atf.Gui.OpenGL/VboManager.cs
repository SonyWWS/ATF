//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

//using Tao.OpenGl;
using OTK = OpenTK.Graphics;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// Vertex buffer manager</summary>
    public class VboManager
    {
        /// <summary>
        /// Gets the singleton instance of VboManager</summary>
        public static VboManager TheInstance
        {
            get
            {
                if (s_theInstance == null)
                    s_theInstance = new VboManager();
                return s_theInstance;
            }
        }

        /// <summary>
        /// Creates a VBO from the given data set</summary>
        /// <param name="dataId">Data identifier</param>
        /// <param name="contextId">Context identifier</param>
        /// <param name="data">Floating point data array</param>
        /// <returns>Internal handle for VBO</returns>
        public int CreateVbo(object dataId, object contextId, float[] data)
        {
            int name;
            if (!m_vboMap.TryGetValue(dataId, out name))
            {
                OTK.OpenGL.GL.GenBuffers(1, out name);
                OTK.OpenGL.GL.BindBuffer(OTK.OpenGL.BufferTarget.ArrayBuffer, name);
                OTK.OpenGL.GL.BufferData(OTK.OpenGL.BufferTarget.ArrayBuffer, new IntPtr(data.Length * 4), data, OTK.OpenGL.BufferUsageHint.StaticDraw);

                m_vboMap[dataId] = name;

                List<Binding> vboList;
                if (!m_vboContexts.TryGetValue(contextId, out vboList))
                {
                    vboList = new List<Binding>();
                    m_vboContexts[contextId] = vboList;
                }
                vboList.Add(new Binding(dataId, name));
                
            }
            return name;
        }

        /// <summary>
        /// Binds the VBO</summary>
        /// <param name="name">VBO handle, assigned by the manager</param>
        public void BindVbo(int name)
        {
            OTK.OpenGL.GL.BindBuffer(OTK.OpenGL.BufferTarget.ArrayBuffer, name);
        }

        /// <summary>
        /// Destroys the VBO</summary>
        /// <param name="name">VBO handle, assigned by the manager</param>
        public void DestroyVbo(int name)
        {
            OTK.OpenGL.GL.DeleteBuffers(1, ref name);
        }

        /// <summary>
        /// Unbinds all VBOs</summary>
        public void UnbindVbos()
        {
            try { OTK.OpenGL.GL.BindBuffer(OTK.OpenGL.BufferTarget.ArrayBuffer, 0); }
            catch
            {
                Console.WriteLine("UnbindVbos FAILED...\n");
            }
        }

        /// <summary>
        /// Releases all VBOs in the given context</summary>
        /// <param name="contextId">Context identifier</param>
        public void ReleaseVboList(object contextId)
        {
            List<Binding> vboList = null;
            if (m_vboContexts.TryGetValue(contextId, out vboList))
            {        
                int[] vbos = new int[vboList.Count];
                for (int i = 0; i < vbos.Length; i++)
                    vbos[i] = vboList[i].Name;

                OTK.OpenGL.GL.DisableClientState(OTK.OpenGL.ArrayCap.VertexArray);
                OTK.OpenGL.GL.DisableClientState(OTK.OpenGL.ArrayCap.NormalArray);
                OTK.OpenGL.GL.DisableClientState(OTK.OpenGL.ArrayCap.TextureCoordArray);

                OTK.OpenGL.GL.DeleteBuffers(vbos.Length, vbos);

                m_vboContexts.Remove(contextId);

                foreach (Binding binding in vboList)
                    m_vboMap.Remove(binding.DataId);
            }

        }

        private VboManager()
        {
        }

        private struct Binding
        {
            public Binding(object dataId, int name)
            {
                DataId = dataId;
                Name = name;
            }

            public readonly object DataId;
            public readonly int Name;
        }

        private readonly Dictionary<object, int> m_vboMap =
            new Dictionary<object, int>();

        private readonly Dictionary<object, List<Binding>> m_vboContexts =
            new Dictionary<object, List<Binding>>();

        private static VboManager s_theInstance;
    }
}
