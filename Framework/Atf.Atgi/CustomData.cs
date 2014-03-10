//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI CustomData</summary>
    public class CustomData : DomNodeAdapter, ICustomData
    {
        /// <summary>
        /// Gets the list of custom attributes</summary>
        public IList<ICustomDataAttribute> CustomAttributes
        {
            get 
            { 
                return GetChildList<ICustomDataAttribute>(Schema.customDataType.attributeChild); 
            }
        }
    }
}

