//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Data Set</summary>
    public class DataSet : DomNodeAdapter, IDataSet //, IListable
    {
        /// <summary>
        /// Gets and sets the DataSet name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.vertexArray_array.nameAttribute); }
            set { SetAttribute(Schema.vertexArray_array.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the data array</summary>
        public float[] Data
        {
            get { return GetAttribute<float[]>(Schema.vertexArray_array.Attribute); }
            set { SetAttribute(Schema.vertexArray_array.Attribute, value); }
        }

        /// <summary>
        /// Gets and sets the DataSet element size</summary>
        public int ElementSize
        {
            get { return GetAttribute<int>(Schema.vertexArray_array.strideAttribute); }
            set { SetAttribute(Schema.vertexArray_array.strideAttribute, value); }
        }

        //#region IListable Members

        ///// <summary>
        ///// Gets display info for Dom object</summary>
        ///// <param name="info">Item info, to be filled out</param>
        //public virtual void GetInfo(Sce.Atf.Applications.ItemInfo info)
        //{
        //    info.Label = (string)InternalObject.GetAttribute(Node.NameAttribute);
        //    info.ImageIndex = info.GetImageList().Images.IndexOfKey(StandardIcon.Data);
        //}

        //#endregion
    }
}

