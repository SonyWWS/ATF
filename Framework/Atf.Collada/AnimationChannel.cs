//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA animation channel</summary>
    class AnimationChannel : DomNodeAdapter, IAnimChannel
    {

        #region IAnimChannel Members

        /// <summary>
        /// Gets or sets the channel name</summary>
        public string Channel
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the child AnimData list</summary>
        public IAnimData Data
        {
            get { return DomNode.GetAttribute(Schema.channel.sourceAttribute).As<IAnimData>(); }
        }

        /// <summary>
        /// Gets or sets this animation to be enabled or disabled</summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the input channel name</summary>
        public string InputChannel
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the input object name</summary>
        public string InputObject
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the target DOM object</summary>
        public object Target { get; set; }

        /// <summary>
        /// Gets or sets the value index</summary>
        public int ValueIndex
        {
            get { return m_valueIndex; }
            set { m_valueIndex = value; }
        }

        #endregion

        /// <summary>
        /// Performs initialization when the adapter's node is set.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Enabled = true;
            Target = ResolveTarget(GetAttribute<string>(Schema.channel.targetAttribute), out m_valueIndex);
        }

        private static DomNode FindSubelement(DomNode node, string[] address, uint index)
        {
            foreach (DomNode child in node.Children)
            {
                foreach (AttributeInfo attribute in child.Type.Attributes)
                    if (attribute.Name == "sid")
                        if (child.GetAttribute(attribute) as string == address[index])
                            return child;

                if (index < address.Length - 1)
                {
                    DomNode element = FindSubelement(child, address, index + 1);
                    if (element != null)
                        return element;
                }
            }

            return null;
        }

        private object ResolveTarget(string target, out int valueIndex)
        {
            // See Chapter 3-2 COLLADA – Digital Asset Schema Release 1.4.1 for details
            string[] address = target.Split(s_addressDelimiters);

            DomNode id = Tools.FindNode(address[0], DomNode);
            DomNode element = FindSubelement(id, address, 1);

            if (element == null)
            {
                valueIndex = -1;
                return null;
            }

            switch (address[address.Length - 1].ToUpper())
            {
                case "R":
                case "S":
                case "U":
                case "X":
                    valueIndex = 0;
                    break;

                case "G":
                case "T":
                case "V":
                case "Y":
                    valueIndex = 1;
                    break;

                case "B":
                case "P":
                case "Z":
                    valueIndex = 2;
                    break;

                case "A":
                case "ANGLE":
                case "Q":
                case "W":
                    valueIndex = 3;
                    break;

                default:
                    valueIndex = -1;
                    break;
            }

            return element;
        }

        private int m_valueIndex;
        private static readonly char[] s_addressDelimiters = { '.', '/', '(', ')' };
    }
}
