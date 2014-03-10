//Sony Computer Entertainment Confidential

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Handles mesh data</summary>
    class Mesh : DomNodeAdapter, IMesh, IBoundable // IMesh should implement IBoundable interface
    {
        /// <summary>
        /// Calculates bounding box of mesh's DataSet</summary>
        /// <returns>Bounding box of mesh's DataSet</returns>
        public Box CalculateBoundingBox()
        {
            var box = new Box();
            foreach (DataSet dataSet in DataSets)
            {
                if (dataSet.Name != "position") continue;

                box.Extend(dataSet.Data);
                break;
            }
            DomNodeUtil.SetBox(DomNode, Schema.meshType.boundingBoxAttribute, box);

            return box;
        }

        #region IMesh Members

        /// <summary>
        /// Gets the DataSet list</summary>
        public IEnumerable<IDataSet> DataSets
        {
            get
            {
                DomNode mesh = DomNode.GetChild(Schema.meshType.vertexArrayChild);
                IList<DomNode> arrays = mesh.GetChildList(Schema.meshType_vertexArray.arrayChild);
                return arrays.AsIEnumerable<IDataSet>();
            }
        }

        /// <summary>
        /// Gets the PrimitiveSets list</summary>
        public IEnumerable<IPrimitiveSet> PrimitiveSets
        {
            get
            {
                DomNode mesh = DomNode.GetChild(Schema.meshType.vertexArrayChild);
                IList<DomNode> primitives = mesh.GetChildList(Schema.meshType_vertexArray.primitivesChild);
                return primitives.AsIEnumerable<IPrimitiveSet>();
            }
        }

        #endregion

        #region IBoundable Members

        /// <summary>
        /// Gets a bounding box in local space</summary>
        public Box BoundingBox
        {
            get { return m_boundingBox.Value; }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.meshType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.meshType.nameAttribute, value); }
        }

        #endregion

        /// <summary>
        /// Raises the NodeSet event and performs custom processing</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Box boxValue = DomNodeUtil.GetBox(DomNode, Schema.meshType.boundingBoxAttribute);
            m_boundingBox = boxValue.IsEmpty ? new Cached<Box>(CalculateBoundingBox) : new Cached<Box>(CalculateBoundingBox, boxValue);
        }

        private Cached<Box> m_boundingBox;
    }
}
