//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Object statistics class</summary>
    public class ObjectStats
    {
        /// <summary>
        /// Gets or sets number of primitives</summary>
        public int PrimCount
        {
            get { return m_nPrimitives; }
            set { m_nPrimitives = value; }
        }

        /// <summary>
        /// Gets or sets number of vertices</summary>
        public int VertexCount
        {
            get { return m_nVertices; }
            set { m_nVertices = value; }
        }

        private int m_nPrimitives;
        private int m_nVertices;
    }
}
