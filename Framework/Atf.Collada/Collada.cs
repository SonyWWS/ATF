//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA utilities</summary>
    public class Collada : DomNodeAdapter,  ISceneGraphHierarchy
    {
        /// <summary>
        /// Computes absolute path from the given URI</summary>        
        public string GetAbsolutePath(Uri uri)
        {            
            string texPath = null;
            if (uri.IsAbsoluteUri)
            {
                texPath = uri.LocalPath;
            }
            else
            {
                DomResource domRes = this.As<DomResource>();
                //i absUri = 
                Uri absUri = new Uri(domRes.Uri, uri);
                texPath = absUri.LocalPath;                
            }

            texPath = System.Uri.UnescapeDataString(texPath);
            texPath = texPath.Replace("/", "\\");
            return texPath;            
        }

        #region ISceneGraphHierarchy Members

        /// <summary>
        /// Gets enumeration of the children of the object</summary>
        /// <returns>Enumeration of the children of the object</returns>
        public IEnumerable<object> GetChildren()
        {
            yield return this.DomNode.GetChild(Schema.COLLADA.sceneChild);
        }

        #endregion
    } 
}
