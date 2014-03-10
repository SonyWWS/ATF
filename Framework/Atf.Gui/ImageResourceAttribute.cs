//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Attribute to mark fields, for automatically loading of images by ResourceUtil</summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class ImageResourceAttribute : Attribute
    {
        /// <summary>
        /// Constructor for single image resource</summary>
        /// <param name="imageName">Image's name</param>
        public ImageResourceAttribute(string imageName)
            : this(imageName, null, null)
        {
        }

        /// <summary>
        /// Constructor, for 16-24-32 toolbar button image resources</summary>
        /// <param name="imageName16">16x16 image's name</param>
        /// <param name="imageName24">24x24 image's name</param>
        /// <param name="imageName32">32x32 image's name</param>
        public ImageResourceAttribute(string imageName16, string imageName24, string imageName32)
        {
            m_imageName1 = imageName16;
            m_imageName2 = imageName24;
            m_imageName3 = imageName32;
        }

        /// <summary>
        /// Gets the first image name, which is always non-null</summary>
        public string ImageName1
        {
            get { return m_imageName1; }
        }

        /// <summary>
        /// Gets the second image name, or null if none</summary>
        public string ImageName2
        {
            get { return m_imageName2; }
        }

        /// <summary>
        /// Gets the third image name, or null if none</summary>
        public string ImageName3
        {
            get { return m_imageName3; }
        }

        private readonly string m_imageName1;
        private readonly string m_imageName2;
        private readonly string m_imageName3;
    }
}
