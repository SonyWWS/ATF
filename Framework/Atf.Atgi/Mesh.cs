//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Mesh</summary>
    public class Mesh : DomNodeAdapter, IMesh, IBoundable, IQueryObjectStats
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Box boxValue = DomNodeUtil.GetBox(DomNode, Schema.meshType.boundingBoxAttribute);
            if (boxValue.IsEmpty)
                m_boundingBox = new Cached<Box>(CalculateBoundingBox); // don't set value and force to compute
            else
                m_boundingBox = new Cached<Box>(CalculateBoundingBox, boxValue); // non-default value found, use it
        }

        /// <summary>
        /// Gets and sets the Mesh name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.meshType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.meshType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of Mesh DataSets</summary>
        public IEnumerable<IDataSet> DataSets
        {
            get
            {
                // Get the top level vertex array object
                DomNode vertexArrayObject = DomNode.GetChild(Schema.meshType.vertexArrayChild);
                IList<DomNode> arrays = vertexArrayObject.GetChildList(Schema.meshType_vertexArray.arrayChild);
                return arrays.AsIEnumerable<IDataSet>();
            }
        }

        /// <summary>
        /// Gets the list of Mesh PrimitiveSets</summary>
        public IEnumerable<IPrimitiveSet> PrimitiveSets
        {
            get
            {
                // Get the top level vertex array object
                DomNode vertexArrayObject = DomNode.GetChild(Schema.meshType.vertexArrayChild);
                IList<DomNode> primitives = vertexArrayObject.GetChildList(Schema.meshType_vertexArray.primitivesChild);
                return primitives.AsIEnumerable<IPrimitiveSet>();
            }
        }

        #region IBoundable Members

        /// <summary>
        /// Calculates the geometry's bounding box</summary>
        /// <returns>Bounding box for geometry</returns>
        public Box CalculateBoundingBox()
        {
            Box box = new Box();
            foreach (DataSet dataSet in DataSets)
            {
                if (dataSet.Name == "position")
                {
                    box.Extend(dataSet.Data);
                    break;
                }
            }
            DomNodeUtil.SetBox(DomNode, Schema.meshType.boundingBoxAttribute, box);
            return box;
        }

        /// <summary>
        /// Gets the geometry's bounding box</summary>
        public Box BoundingBox
        {
            get { return m_boundingBox.Value;  }
        }

        /// <summary>
        /// Gets or sets whether the object is visible</summary>
        public bool Visible
        {
            get { return true; }
            set { }
        }

        #endregion

        #region IQueryObjectStats Members

        /// <summary>
        /// Gets object statistics</summary>
        /// <param name="stats">Statistics</param>
        public void GetStats(ObjectStats stats)
        {
            // Extract vertex array
            foreach (DataSet dataSet in DataSets)
            {
                if (dataSet.Name == "position")
                {
                    stats.VertexCount += dataSet.Data.Length;
                    break;
                }
            }

            // Extract prim sizes array for each prim set
            foreach (IPrimitiveSet primSet in PrimitiveSets)
            {
                int[] primSizes = primSet.PrimitiveSizes;
                stats.PrimCount += primSizes.Length;
            }

        }

        #endregion

        private Cached<Box> m_boundingBox;

#if MEMORY_DEBUG
        public static int NumMesh;
        public Mesh()
        {
            lock (s_lock)
                NumMesh++;
        }
        ~Mesh()
        {
            lock (s_lock)
                NumMesh--;
        }
        private static object s_lock = new object();
#endif
    }
}

