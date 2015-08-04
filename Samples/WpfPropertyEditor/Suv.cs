//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Drawing;

namespace WpfPropertyEditor
{
    public class Suv
    {
        public enum Configurations 
        {
            Standard,
            Plus,
            Limited      
        }

        public string Name
        {
            get { return m_name; }
            internal set { m_name = value; }
        }

        public bool Awd { get; set; }
        public float Price { get; set; }

        [Browsable(false)]
        public float Min { get; set; }
        [Browsable(false)]
        public float Max { get; set; }
        [Browsable(false)]
        public Color Color { get; set; }

        public Configurations Config { get; set; }
        
        private string m_name;
    }
}
