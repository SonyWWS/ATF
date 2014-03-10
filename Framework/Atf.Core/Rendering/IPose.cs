//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for ATGI Pose</summary>
    public interface IPose : INameable
    {
        /// <summary>
        /// Gets the child element list</summary>
        IList<IPoseElement> Elements
        {
            get;
        }
        
        /// <summary>
        /// Gets and sets the bind pose attribute</summary>
        bool BindPose
        {
            get;
            set;
        }
    }
}
