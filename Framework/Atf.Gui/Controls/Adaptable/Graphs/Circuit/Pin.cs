//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to a pin in a circuit; used in mastering and as a base
    /// class for GroupPin</summary>
    public abstract class Pin : DomNodeAdapter, ICircuitPin
    {
        /// <summary>
        /// Gets type attribute of Pin</summary>
        protected abstract AttributeInfo TypeAttribute { get; }

        /// <summary>
        /// Gets name attribute of Pin</summary>
        protected abstract AttributeInfo NameAttribute { get; }

        #region ICircuitPin Members

        /// <summary>
        /// Gets pin name</summary>
        public virtual string Name
        {
            get { return GetAttribute<string>(NameAttribute); }
            set { SetAttribute(NameAttribute, value); }
        }

        /// <summary>
        /// Gets pin type name</summary>
        public virtual string TypeName
        {
            get { return GetAttribute<string>(TypeAttribute); }
            set { SetAttribute(TypeAttribute, value); }
        }

        /// <summary>
        /// Gets index of this pin in the owning ICircuitElementType's input/output list</summary>
        public virtual int Index
        {
             get { return m_index; }
             set { m_index = value; }
        }

        #endregion

        #region ElementType.Pin Members

        /// <summary>
        /// Gets whether connection fan in to pin is allowed</summary>
        public virtual bool AllowFanIn
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether connection fan out from pin is allowed</summary>
        public virtual bool AllowFanOut
        {
            get { return true; }
        }

        #endregion

        private int m_index;
     }
}
