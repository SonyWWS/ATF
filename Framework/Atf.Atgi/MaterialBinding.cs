//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// Material binding interface</summary>
    public class MaterialBinding : DomNodeAdapter, IBinding
    {
        /// <summary>
        /// Gets and sets binding tag</summary>
        public string Tag
        {
            get { return GetAttribute<string>(Schema.materialType_binding.tagAttribute); }
            set { SetAttribute(Schema.materialType_binding.tagAttribute, value); }
        }

        /// <summary>
        /// Gets and sets binding type</summary>
        public string BindingType
        {
            get { return GetAttribute<string>(Schema.materialType_binding.typeAttribute); }
            set { SetAttribute(Schema.materialType_binding.typeAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the name of the DataSet</summary>
        public string DataSet
        {
            get { return GetAttribute<string>(Schema.materialType_binding.datasetAttribute); }
            set { SetAttribute(Schema.materialType_binding.datasetAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the count</summary>
        public int Count
        {
            get { return GetAttribute<int>(Schema.materialType_binding.countAttribute); }
            set { SetAttribute(Schema.materialType_binding.countAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the source DOM object</summary>
        public object Source
        {
            get { return DomNode.GetAttribute(Schema.materialType_binding.sourceAttribute) as DomNode; }
            set { DomNode.SetAttribute(Schema.materialType_binding.sourceAttribute, value); }
        }
    }
}
