//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// Custom data attribute interface implementation</summary>
    public class CustomDataAttribute : DomNodeAdapter, ICustomDataAttribute, INameable //, IListable
    {
        /// <summary>
        /// Gets or sets the data type name</summary>
        public string DataType
        {
            get { return GetAttribute<string>(Schema.customDataAttributeType.typeAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.typeAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the value attribute name</summary>
        public string ValueAttr
        {
            get { return GetAttribute<string>(Schema.customDataAttributeType.valueAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.valueAttribute, value); }
        }

        /// <summary>
        /// Gets and sets default value</summary>
        public string Default
        {
            get { return GetAttribute<string>(Schema.customDataAttributeType.defaultAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.defaultAttribute, value); }
        }

        /// <summary>
        /// Gets and sets minimum value</summary>
        public string Min
        {
            get { return GetAttribute<string>(Schema.customDataAttributeType.minAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.minAttribute, value); }
        }

        /// <summary>
        /// Gets and sets maximum value</summary>
        public string Max
        {
            get { return GetAttribute<string>(Schema.customDataAttributeType.maxAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.maxAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the count</summary>
        public int Count
        {
            get { return GetAttribute<int>(Schema.customDataAttributeType.countAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.countAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the index</summary>
        public int Index
        {
            get { return GetAttribute<int>(Schema.customDataAttributeType.indexAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.indexAttribute, value); }
        }

        /// <summary>
        /// Gets or sets whether value is an array</summary>
        public bool isArray
        {
            get { return GetAttribute<bool>(Schema.customDataAttributeType.isArrayAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.isArrayAttribute, value); }
        }

        /// <summary>
        /// Gets and sets any element data</summary>
        public object Value
        {
            get { return DomNode.GetAttribute(Schema.customDataAttributeType.Attribute); }
            set { DomNode.SetAttribute(Schema.customDataAttributeType.Attribute, value); }
        }

        #region INameable Members
        
        /// <summary>
        /// Gets and sets the name attribute</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.customDataAttributeType.nameAttribute); }
            set { SetAttribute(Schema.customDataAttributeType.nameAttribute, value); }
        }
        
        #endregion
        
        //#region IListable Members

        ///// <summary>
        ///// Gets display info for Dom object</summary>
        ///// <param name="info">Item info, to be filled out</param>
        //public virtual void GetInfo(Sce.Atf.Applications.ItemInfo info)
        //{
        //    info.Label = (string)InternalObject.GetAttribute(NameAttribute);
        //    info.ImageIndex = info.GetImageList().Images.IndexOfKey(StandardIcon.Data);
        //}

        //#endregion
    }
}
