//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA submesh</summary>
    public class SubMesh : DomNodeAdapter, ISubMesh, ISceneGraphHierarchy
    {

        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            Visible = true;
            DomNodeType domtype = this.DomNode.Type;
            ChildInfo inputChildInfo = domtype.GetChildInfo("input");
            Mesh mesh = GetParentAs<Mesh>();

            int maxoffset = -1;
            // get input child info.            
            foreach (DomNode inputNode in DomNode.GetChildList(inputChildInfo))
            {
                PrimInput pinput = new PrimInput(inputNode, mesh);
                if (pinput.Offset > maxoffset)
                    maxoffset = pinput.Offset;
                if (pinput.Semantic == "VERTEX")
                {                    
                    foreach (PrimInput vinput in mesh.VertexInputs)
                    {
                        PrimInput input = new PrimInput(vinput, pinput.Offset);
                        m_inputs.Add(input);
                    }                    
                }
                else
                {
                    m_inputs.Add(pinput);
                }
            }

            m_bindingCount = maxoffset + 1;
            
            // prim type
            string type = DomNode.ToString();
            m_primitiveType =  type.Substring(type.LastIndexOf(':') + 1).ToUpper();
            if (m_primitiveType == "POLYLIST")
                m_primitiveType = "POLYGONS";

            // indices
            AttributeInfo pattrib = DomNode.Type.GetAttributeInfo("p");
            m_indices = Tools.ULongToInt(GetAttribute<ulong[]>(pattrib));
            
            // material
            AttributeInfo mtrlAttrib =DomNode.Type.GetAttributeInfo("material");
            string mtrl = GetAttribute<string>(mtrlAttrib);
            Geometry geom = DomNode.Parent.Parent.Cast<Geometry>();
            m_effect = geom.Effects[mtrl];
                      
            // vcount, element sizes
            AttributeInfo vcountAttrib = DomNode.Type.GetAttributeInfo("vcount");
            if (vcountAttrib != null)
            {
                m_sizes = Tools.ULongToInt(GetAttribute<ulong[]>(vcountAttrib));                
            }
            else
                m_sizes = new int[]{3};

            //count
            AttributeInfo countAttrib = DomNode.Type.GetAttributeInfo("count");
            m_count =(int) GetAttribute<ulong>(countAttrib);

            // name
            AttributeInfo nameAttrib = DomNode.Type.GetAttributeInfo("name");
            m_name = GetAttribute<string>(nameAttrib);

          
        }

        /// <summary>
        /// Gets PrimInput enumeration</summary>
        public IEnumerable<PrimInput> Inputs
        {
            get { return m_inputs; }
        }

        #region ISubMesh Members

        /// <summary>
        /// Gets the DataSet for the submesh</summary>
        public IEnumerable<IDataSet> DataSets
        {
            get { return m_inputs.AsIEnumerable<IDataSet>(); }
        }

        /// <summary>
        /// Gets primitive count</summary>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Gets parent IMesh</summary>
        public IMesh Parent
        {
            get { return GetParentAs<IMesh>(); }
        }

        #endregion

        #region IPrimitiveSet Members

        private int m_bindingCount = -1;
        /// <summary>
        /// Gets the binding count</summary>
        public int BindingCount
        {
            get { return m_bindingCount; }
        }

        /// <summary>
        /// Finds the index of the given binding name</summary>
        /// <param name="binding">Binding name</param>
        /// <returns>Index of the given binding name</returns>
        public int FindBinding(string binding)
        {
            int offset = -1;
            foreach (PrimInput input in Inputs)
            {
                if (input.Name == binding)
                {
                    offset = input.Offset;
                    break;
                }
            }
            return offset;
        }

        /// <summary>
        /// Gets the bindings list</summary>
        public IList<IBinding> Bindings
        {
            get { return null; }
        }

        private string m_primitiveType;
        /// <summary>
        /// Gets and sets the primitive type</summary>
        public string PrimitiveType
        {
            get { return m_primitiveType; }
            set {throw new NotImplementedException();}
        }

        /// <summary>
        /// Primitive sizes array</summary>
        public int[] m_sizes;
        /// <summary>
        /// Gets the DOM object whose value is the primitive sizes array</summary>
        public int[] PrimitiveSizes
        {
            get{return m_sizes;}
            set { throw new NotImplementedException(); }
        }

        private int[] m_indices;
        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive indices array</summary>
        public int[] PrimitiveIndices
        {
            get{ return m_indices;}
            set{ throw new NotImplementedException();}
        }

        private Effect m_effect;
        /// <summary>
        /// Gets and sets the shader</summary>
        public IShader Shader
        {
            get{ return m_effect; }
            set {throw new NotImplementedException();}
        }

        #endregion

        #region INameable Members
        private string m_name;
        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return m_name; }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region IVisible Members

        /// <summary>
        /// Gets or sets whether the object is visible</summary>
        public bool Visible
        {
            get;
            set;
        }

        #endregion


        #region private fields
        private int m_count;
        private List<PrimInput> m_inputs = new List<PrimInput>();
        #endregion 
    
        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object. SceneGraphBuilder will stop traversing at this node.</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            return EmptyEnumerable<object>.Instance;
        }

        #endregion
    }
}
