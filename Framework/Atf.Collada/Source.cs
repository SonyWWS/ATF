//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Dom;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA source DOM object</summary>
    public class Source : DomNodeAdapter
    {
        private string[] m_nameData;
        /// <summary>
        /// Gets or sets source name data</summary>
        public string[] NameData
        {
            get
            {
                if (m_nameData == null)
                {
                    DomNode array = DomNode.GetChild(Schema.source.Name_arrayChild);
                    m_nameData = array.GetAttribute(Schema.Name_array.Attribute) as string[];
                }
                return m_nameData;
            }
            set { }
        }
     
        private float[] m_data;
        /// <summary>
        /// Gets or sets source data</summary>
        public float[] Data
        {
            get
            {
                if (m_data == null)
                {
                    DomNode array = DomNode.GetChild(Schema.source.float_arrayChild);
                    m_data = Tools.DoubleToFloat(array.GetAttribute(Schema.float_array.Attribute) as double[]);
                }
                return m_data;
            }
            set {  }
        }
        
        /// <summary>
        /// Gets source ID</summary>
        public string Id
        {
            get { return GetAttribute<string>(Schema.source.idAttribute); }
        }


        private int m_stride = -1;

        /// <summary>
        /// Gets stride</summary>
        public int stride
        {
            get
            {
                if (m_stride == -1)
                {
                    DomNode technique = DomNode.GetChild(Schema.source.technique_commonChild);
                    DomNode accessor = technique.GetChild(Schema.source_technique_common.accessorChild);
                    m_stride = Convert.ToInt32(accessor.GetAttribute(Schema.accessor.strideAttribute));
                }
                return m_stride;
            }            
        }
    }
}
