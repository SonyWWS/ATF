//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI primitive set. This is the lowest level object that can be rendered or selected in an
    /// ATGI model. This object does not actually do the rendering, though. See RenderPrimitives.</summary>
    public class PrimitiveSet : DomNodeAdapter, IPrimitiveSet
    {
        /// <summary>
        /// Gets and sets the PrimitiveSet name</summary>
        public string Name
        {
            get
            {
                return (string)DomNode.GetAttribute(Schema.vertexArray_primitives.nameAttribute);
            }
            set
            {
                DomNode.SetAttribute(Schema.vertexArray_primitives.nameAttribute, value);
            }
        }

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
            int bindingCount = bindings.Count;
            for (int i = 0; i < bindingCount; i++)
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
            get
            {
                return GetChildList<IBinding>(Schema.vertexArray_primitives.bindingChild);
            }
        }

        /// <summary>
        /// Gets and sets the primitive type</summary>
        public string PrimitiveType
        {
            get { return GetAttribute<string>(Schema.vertexArray_primitives.typeAttribute); }
            set { SetAttribute(Schema.vertexArray_primitives.typeAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive sizes array</summary>
        public int[] PrimitiveSizes
        {
            get { return GetAttribute<int[]>(Schema.vertexArray_primitives.sizesAttribute); }
            set { SetAttribute(Schema.vertexArray_primitives.sizesAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive indices array</summary>
        public int[] PrimitiveIndices
        {
            get { return GetAttribute<int[]>(Schema.vertexArray_primitives.indicesAttribute); }
            set { SetAttribute(Schema.vertexArray_primitives.indicesAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the shader</summary>
        public IShader Shader
        {
            get { return DomNode.GetAttribute(Schema.vertexArray_primitives.shaderAttribute).As<IShader>(); }
            set { DomNode.SetAttribute(Schema.vertexArray_primitives.shaderAttribute, value); }
        }

        #region IVisible Members
        /// <summary>
        /// Gets and sets whether the object is visible</summary>
        public bool Visible
        {
            get { return m_visible; }
            set { m_visible = value; }
        }
        #endregion

        #if MEMORY_DEBUG
        public static volatile int NumPrimitiveSets;
        public PrimitiveSet()
        {
            NumPrimitiveSets++;
        }
        ~PrimitiveSet()
        {
            NumPrimitiveSets--;
        }
        #endif

        private bool m_visible = true;
    }
}

