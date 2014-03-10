//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA prim input</summary>
    public class PrimInput : IDataSet
    {        
        /// <summary>
        /// Constructor</summary>
        /// <param name="input">DomNode</param>
        /// <param name="mesh">Mesh</param>
        public PrimInput(DomNode input, Mesh mesh)
        {
            AttributeInfo srcAttrib = null;
            AttributeInfo offsetAttrib = null;
            AttributeInfo semantiAttrib = null;

            if (input.Type == Schema.InputLocalOffset.Type)
            {
                srcAttrib = Schema.InputLocalOffset.sourceAttribute;
                offsetAttrib = Schema.InputLocalOffset.offsetAttribute;
                semantiAttrib = Schema.InputLocalOffset.semanticAttribute;                
            }
            else if (input.Type == Schema.InputLocal.Type)
            {
                srcAttrib = Schema.InputLocal.sourceAttribute;                
                semantiAttrib = Schema.InputLocal.semanticAttribute;                
            }
            else
            {
                throw new ArgumentException(input.Type.ToString() + " is not supported");
            }


            // find the source for this input.
            string srcId = (string)input.GetAttribute(srcAttrib);
            srcId = srcId.Replace("#", "").Trim();

            foreach (Source src in mesh.Sources)
            {
                if (src.Id == srcId)
                {
                    m_source = src;
                    break;
                }
            }

            if (offsetAttrib != null)
                m_offset = Convert.ToInt32(input.GetAttribute(offsetAttrib));
            else
                m_offset = -1;

            m_semantic = (string)input.GetAttribute(semantiAttrib);

            switch (m_semantic)
            {
                case "POSITION":
                    m_atgiName = "position";
                    break;
                case "NORMAL":
                    m_atgiName = "normal";
                    break;
                case "TEXCOORD":
                    m_atgiName = "map1";
                    break;
                case "COLOR":
                    m_atgiName = "color";
                    break;
            }      

        }

        /// <summary>
        /// Constructor using PrimInput and offset</summary>
        /// <param name="input">PrimInput</param>
        /// <param name="offset">Offset</param>
        public PrimInput(PrimInput input, int offset)
        {
            m_semantic = input.Semantic;
            m_offset = offset;
            m_source = input.DataSource;
            m_atgiName = input.Name;
        }

        private string m_semantic;
        /// <summary>
        /// Gets semantics</summary>
        public string Semantic
        {
            get { return m_semantic;}
        }

        private int m_offset;
        /// <summary>
        /// Gets offset</summary>
        public int Offset
        {
            get{return m_offset;}
        }

        private Source m_source;
        public Source DataSource
        {
            get{ return m_source;}
        }

        #region IDataSet Members

        /// <summary>
        /// Gets and sets the data array</summary>
        public float[] Data
        {
            get { return m_source.Data; }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Gets and sets the data element size</summary>
        public int ElementSize
        {
            get { return m_source.stride; }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        #region INameable Members

        private string m_atgiName;
        /// <summary>
        /// Gets and sets DOM object's name</summary>
        public string Name
        {
            get { return m_atgiName; }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        /// <summary>
        /// Gets adapter for given type</summary>
        /// <param name="type">Type for which to find adapter</param>
        /// <returns>Adapter for given type</returns>
        public object GetAdapter(Type type)
        {
            return null;
        }
    
    }
}
