//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA animation</summary>
    class Animation : DomNodeAdapter, IAnim
    {

        #region IAnim Members

        /// <summary>
        /// Gets a list of IAnim children</summary>
        public IList<IAnim> Animations
        {
            get { return GetChildList<IAnim>(Schema.animation.animationChild); }
        }

        /// <summary>
        /// Gets the child AnimChannel list</summary>
        public IList<IAnimChannel> Channels
        {
            get { return GetChildList<IAnimChannel>(Schema.animation.channelChild); }
        }

        #endregion
    }
}
