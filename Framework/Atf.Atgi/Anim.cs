//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Anim</summary>
    public class Anim : DomNodeAdapter, IAnim
    {
        /// <summary>
        /// Gets and sets the Anim name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.animType.nameAttribute); }
            set { SetAttribute(Schema.animType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets and sets the Anim target</summary>
        public string Target
        {
            get { return GetAttribute<string>(Schema.animType.targetAttribute); }
            set { SetAttribute(Schema.animType.targetAttribute, value); }
        }

        #region IAnim Members
        
        /// <summary>
        /// Gets the child AnimChannel list</summary>
        public IList<IAnimChannel> Channels
        {
            get { return GetChildList<IAnimChannel>(Schema.animType.animChannelChild); }
        }

        /// <summary>
        /// Gets a list of IAnim children</summary>
        public IList<IAnim> Animations
        {
            get
            {
                List<IAnim> list = new List<IAnim>();
                return list;
            }
        }

        #endregion
    }
}

