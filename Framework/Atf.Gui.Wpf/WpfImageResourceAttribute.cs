//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf
{

    /// <summary>
    /// Attribute to mark fields, for automatic loading of Resource Dictionaries by WpfResourceUtil</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class WpfImageResourceAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="imageName">Image name</param>
        public WpfImageResourceAttribute(string imageName)
        {
            ImageName = imageName;
        }

        /// <summary>
        /// Gets image name</summary>
        public string ImageName { get; private set; }
    }
}
