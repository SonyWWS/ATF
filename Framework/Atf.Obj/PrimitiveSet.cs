//Sony Computer Entertainment Confidential

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Handles set of primitives</summary>
    class PrimitiveSet : DomNodeAdapter, IPrimitiveSet
    {

        #region IPrimitiveSet Members

        /// <summary>
        /// Gets the binding count</summary>
        public int BindingCount
        {
            get { return DomNode.GetChildList(Schema.vertexArray_primitives.bindingChild).Count; }
        }

        /// <summary>
        /// Finds the index of the given binding name</summary>
        /// <param name="binding">Binding name</param>
        /// <returns>Index of the given binding name</returns>
        public int FindBinding(string binding)
        {
            IList<DomNode> bindings = DomNode.GetChildList(Schema.vertexArray_primitives.bindingChild);
            for (int i = 0; i < bindings.Count; i++)
            {
                string current = (string)bindings[i].GetAttribute(Schema.primitives_binding.sourceAttribute);
                if (current == binding)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets the bindings list</summary>
        public IList<IBinding> Bindings
        {
            get { return GetChildList<IBinding>(Schema.vertexArray_primitives.bindingChild); }
        }

        /// <summary>
        /// Gets and sets the primitive type</summary>
        public string PrimitiveType
        {
            get { return GetAttribute<string>(Schema.vertexArray_primitives.typeAttribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_primitives.typeAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive sizes array</summary>
        public int[] PrimitiveSizes
        {
            get { return GetAttribute<int[]>(Schema.vertexArray_primitives.sizesAttribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_primitives.sizesAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive indices array</summary>
        public int[] PrimitiveIndices
        {
            get { return GetAttribute<int[]>(Schema.vertexArray_primitives.indicesAttribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_primitives.indicesAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the shader</summary>
        public IShader Shader
        {
            get { return GetChild<IShader>(Schema.vertexArray_primitives.shaderChild); }
            set { throw new System.NotImplementedException(); }
        }

        #endregion

        #region INameable Members

        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.vertexArray_primitives.nameAttribute); }
            set { DomNode.SetAttribute(Schema.vertexArray_primitives.nameAttribute, value); }
        }

        #endregion

        #region IVisible Members

        /// <summary>
        /// Gets or sets whether the object is visible</summary>
        public bool Visible { get; set; }

        #endregion

        /// <summary>
        /// Raises the NodeSet event and performs custom processing</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Visible = true;
        }
    }
}
