//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// 
    /// </summary>
    public class MissingElementType : ICircuitElementType
    {
        public MissingElementType(  string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public Size InteriorSize
        {
            get { return (Image != null) ? new Size(Image.Width + 18, Image.Height +18) : new Size(); }
        }

        public Image Image { get;  set; }

        public IList<ICircuitPin> Inputs
        {
            get { return m_inputs; }
        }

        public IList<ICircuitPin> Outputs 
        {
            get
            {
                return m_outputs;                 
            } 
        }

        private MissingPinList m_inputs = new MissingPinList("In");
        private MissingPinList m_outputs = new MissingPinList("Out");

        private class MissingPinList : IList<ICircuitPin>
        {
            public MissingPinList(string name)
            {
                m_missingPin = new MissingPin(name, "MissingPin", 0);
            }

            public IEnumerator<ICircuitPin> GetEnumerator()
            {
                for (int i=0;  i< Count; ++i)
                    yield return m_missingPin;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(ICircuitPin item)
            {
                //throw new System.NotImplementedException();
            }

            public void Clear()
            {
                //throw new System.NotImplementedException();
            }

            public bool Contains(ICircuitPin item)
            {
                //throw new System.NotImplementedException();
                return false;
            }

            public void CopyTo(ICircuitPin[] array, int arrayIndex)
            {
                //throw new System.NotImplementedException();
            }

            public bool Remove(ICircuitPin item)
            {
                //throw new System.NotImplementedException();
                return false;
            }

            public int Count
            {
                get { return m_count; }
            }
            public bool IsReadOnly { get; private set; }
            public int IndexOf(ICircuitPin item)
            {
                throw new System.NotImplementedException();
            }

            public void Insert(int index, ICircuitPin item)
            {
                //throw new System.NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                //throw new System.NotImplementedException();
            }

            public ICircuitPin this[int index]
            {
                get
                {
					// dynamically update count
                    if (index + 1 > m_count)
                        m_count = index + 1; 
                    return m_missingPin;
                }
                set { throw new System.NotImplementedException(); }
            }

            private MissingPin m_missingPin;
            private int m_count;
        }

        /// <summary>
        /// Pin for an ElementType</summary>
        public class MissingPin : ICircuitPin
        {
            /// <summary>
            /// Constructor specifying Pin type's attributes</summary>
            /// <param name="name">Pin's name</param>
            /// <param name="typeName">Pin's type's name</param>
            /// <param name="index">Index of pin on module</param>
            public MissingPin(string name, string typeName, int index)
            {
                m_name = name;
                m_typeName = typeName;
                m_index = index;
            }

            /// <summary>
            /// Gets pin's name</summary>
            public string Name
            {
                get { return m_name; }
            }

            /// <summary>
            /// Gets pin's type's name</summary>
            public string TypeName
            {
                get { return m_typeName; }
            }

            /// <summary>
            /// Gets index of pin on module</summary>
            public int Index
            {
                get { return m_index; }
            }

            /// <summary>
            /// Gets whether connection fan in to pin is allowed</summary>
            public bool AllowFanIn
            {
                get { return true; }
            }

            /// <summary>
            /// Gets whether connection fan out from pin is allowed</summary>
            public bool AllowFanOut
            {
                get { return true; }
            }

            private string m_name;
            private string m_typeName;
            private int m_index;
        }
    }
}
