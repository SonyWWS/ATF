//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI animation clip</summary>
    public class AnimClip : DomNodeAdapter, IAnimClip
    {
        /// <summary>
        /// Gets and sets the AnimClip name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.animclipType.nameAttribute); }
            set { SetAttribute(Schema.animclipType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of Anims held by this clip</summary>
        public IList<IAnim> Anims
        {
            get { return GetChildList<IAnim>(Schema.animclipType.animChild); }
        }
    }
}

