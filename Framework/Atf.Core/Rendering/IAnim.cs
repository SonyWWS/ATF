//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for animation</summary>
    public interface IAnim
    {
        /// <summary>
        /// Gets the child AnimChannel list</summary>
        IList<IAnimChannel> Channels
        {
            get;
        }

        /// <summary>
        /// Gets a list of IAnim children</summary>
        IList<IAnim> Animations
        {
            get;
        }
    }
}
