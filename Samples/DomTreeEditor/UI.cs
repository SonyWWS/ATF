//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace DomTreeEditorSample
{
    /// <summary>
    /// The root object in the UI data model. It contains multiple packages
    /// containing other UI objects.</summary>
    public class UI : UIObject
    {
        /// <summary>
        /// Gets the list of all packages in the UI</summary>
        public IList<UIPackage> Packages
        {
            get { return GetChildList<UIPackage>(UISchema.UIType.PackageChild); }
        }
    }
}
