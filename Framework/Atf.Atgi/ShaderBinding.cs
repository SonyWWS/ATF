//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI shader binding interface</summary>
    public class ShaderBinding : DomNodeAdapter, IBinding
    {
        /// <summary>
        /// Gets and sets binding tag</summary>
        public string Tag
        {
            get { return GetAttribute<string>(Schema.shaderType_binding.tagAttribute); }
            set { SetAttribute(Schema.shaderType_binding.tagAttribute, value); }
        }

        /// <summary>
        /// Gets and sets binding type</summary>
        public string BindingType
        {
            get { return GetAttribute<string>(Schema.shaderType_binding.typeAttribute); }
            set { SetAttribute(Schema.shaderType_binding.typeAttribute, value); }
        }

        /// <summary>
        /// Gets and sets name of DataSet</summary>
        public string DataSet
        {
            get { return GetAttribute<string>(Schema.shaderType_binding.datasetAttribute); }
            set { SetAttribute(Schema.shaderType_binding.datasetAttribute, value); }
        }

        /// <summary>
        /// Gets and sets count</summary>
        public int Count
        {
            get { return GetAttribute<int>(Schema.shaderType_binding.countAttribute); }
            set { SetAttribute(Schema.shaderType_binding.countAttribute, value); }
        }

        /// <summary>
        /// Gets and sets source DOM object</summary>
        public object Source
        {
            get { return DomNode.GetAttribute(Schema.shaderType_binding.sourceAttribute) as DomNode; }
            set { DomNode.SetAttribute(Schema.shaderType_binding.sourceAttribute, value); }
        }
    }
}
