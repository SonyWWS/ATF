//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to a circuit prototype, which contains modules and connections
    /// that can be instanced in a circuit</summary>
    public abstract class Prototype : DomNodeAdapter
    {
        protected abstract AttributeInfo NameAttribute { get; }

        // required  child info
        protected abstract ChildInfo ElementChildInfo { get; }
        protected abstract ChildInfo WireChildInfo { get; }

        /// <summary>
        /// Gets or sets the prototype name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(NameAttribute); }
            set { DomNode.SetAttribute(NameAttribute, value); }
        }

        /// <summary>
        /// Gets the modules in the prototype</summary>
        public IList<Element> Modules
        {
            get { return GetChildList<Element>(ElementChildInfo); }
        }

        /// <summary>
        /// Gets the connections in the prototype</summary>
        public IList<Wire> Connections
        {
            get { return GetChildList<Wire>(WireChildInfo); }
        }

   
     
    }
}
