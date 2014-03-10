//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI Texture</summary>
    public class Texture : DomNodeAdapter, ITexture
    {
        /// <summary>
        /// Gets and sets the Texture name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.textureType.nameAttribute); }
            set { SetAttribute(Schema.textureType.nameAttribute, value); }
        }

        /// From ITexture
        /// <summary>
        /// Gets or sets the texture path name</summary>
        public string PathName
        {
            get
            {
                Uri uri = (Uri)DomNode.GetAttribute(Schema.textureType.uriAttribute);
                
                // If possible, compose an absolute URI by combining the root's 
                // resource URI with the textures relative URI
                IResource resource = DomNode.GetRoot().As<IResource>();
                if (resource != null)
                    return new Uri(resource.Uri, uri).LocalPath;
                
                return uri.LocalPath;
            }
            set
            {
                Uri uri = new Uri(value, UriKind.Absolute);
                DomNode.SetAttribute(Schema.textureType.uriAttribute, uri);
            }
        }
    }
}

