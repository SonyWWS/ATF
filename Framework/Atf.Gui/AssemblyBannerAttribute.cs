//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Attribute to allow specification of a banner image for an assembly</summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public sealed class AssemblyBannerAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        public AssemblyBannerAttribute()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Path to the banner image, e.g. "Resources/Amadeus_500x70.bmp"</param>
        public AssemblyBannerAttribute(string path)
        {
            m_path = path;
        }

        /// <summary>
        /// Gets the path to the banner image</summary>
        public string BannerPath
        {
            get { return m_path; }
        }

        private readonly string m_path;
    }
}
