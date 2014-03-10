//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Base class for diagram hit records, which specify the item and the item part
    /// that were hit by a picking operation</summary>
    public class DiagramHitRecord
    {
        /// <summary>
        /// Constructor</summary>
        public DiagramHitRecord()
        {
        }

        /// <summary>
        /// Constructor using hit item</summary>
        /// <param name="item">Item that was hit</param>
        public DiagramHitRecord(object item)
        {
            m_item = item;
        }

        /// <summary>
        /// Constructor using hit item and part</summary>
        /// <param name="item">Item that was hit</param>
        /// <param name="part">Item part that was hit</param>
        public DiagramHitRecord(object item, object part)
        {
            m_item = item;
            m_part = part;
        }

        /// <summary>
        /// Gets or sets (protected) the item that was hit</summary>
        public object Item
        {
            get { return m_item; }
            protected set { m_item = value; }
        }

        /// <summary>
        /// Gets or sets the subitem that was hit</summary>
        /// <remarks>Subitems are used to describe child objects in the hierarchical Item.</remarks>
        public object SubItem
        {
            get { return m_subItem; }
            set { m_subItem = value; }
        }

        /// <summary>
        /// Gets or sets (protected) the item part that was hit</summary>
        public object Part
        {
            get { return m_part; }
            set { m_part = value; }
        }

        /// <summary>
        /// Gets or sets (protected) the subitem part that was hit</summary>
        public object SubPart
        {
            get { return m_subPart; }
            set { m_subPart = value; }
        }

        /// <summary>
        /// Gets or sets the default item part to edit when no item part is hit</summary>
        /// <remarks>Usually used for F2 on a selected item</remarks>
        public object DefaultPart
        {
            get { return m_defaultPart; }
            set { m_defaultPart = value; }
        }

        /// <summary>
        /// Gets or sets the hit path</summary>
        public AdaptablePath<object> HitPath
        {
            get { return m_hitPath; }
            set { m_hitPath = value; }
        }

        private object m_item;
        private object m_subItem;
        private object m_part;
        private object m_subPart;
        private object m_defaultPart;
        private AdaptablePath<object> m_hitPath;
    }
}
