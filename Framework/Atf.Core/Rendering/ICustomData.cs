//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for ATGI custom data</summary>
    public interface ICustomData
    {
        /// <summary>
        /// Gets the list of custom attributes</summary>
        IList<ICustomDataAttribute> CustomAttributes
        {
            get;
        }
    }
}
