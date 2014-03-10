//Sony Computer Entertainment Confidential

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Handles DataSets for DomNodes that are dynamically cast to more convenient forms</summary>
    class DataSet : DomNodeAdapter, IDataSet
    {

        #region IDataSet Members

        /// <summary>
        /// Gets and sets the data array</summary>
        public float[] Data
        {
            get { return GetAttribute<float[]>(Schema.vertexArray_array.Attribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_array.Attribute, value); }
        }

        /// <summary>
        /// Gets and sets the data element size</summary>
        public int ElementSize
        {
            get { return GetAttribute<int>(Schema.vertexArray_array.strideAttribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_array.strideAttribute, value); }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.vertexArray_array.nameAttribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_array.nameAttribute, value); }
        }

        #endregion
    }
}
