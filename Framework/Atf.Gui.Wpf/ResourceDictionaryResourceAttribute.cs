//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Attribute to mark fields, for automatic loading of resource dictionaries by WpfResourceUtil</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ResourceDictionaryResourceAttribute: Attribute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="path">XAML file</param>
        public ResourceDictionaryResourceAttribute(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets or sets XAML file</summary>
        public string Path { get; private set; }
    }
}
