//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;

namespace Sce.Atf.Wpf.Skins
{
    /// <summary>
    /// Information about a skin including a URI so it can be referenced from another assembly</summary>
    public sealed class ReferencedAssemblySkin : Skin
    {
        /// <summary>
        /// Gets the URI of the skin</summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Skin name</param>
        /// <param name="resourceUri">Skin URI</param>
        public ReferencedAssemblySkin(string name, Uri resourceUri)
            : base(name)
        {
            Uri = resourceUri;
        }

        /// <summary>
        /// Loads the resources for the referenced skin</summary>
        protected override void LoadResources()
        {
            var resource = (ResourceDictionary)Application.LoadComponent(Uri);
            Resources.Add(resource);
        }

    }
}
