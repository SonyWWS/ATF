//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Sce.Atf
{
    /// <summary>
    /// Utilities for working with resources, such as Images, Icons, and Cursors.</summary>
    public static class ResourceUtil
    {
        /// <summary>
        /// Registers a keyed image</summary>
        /// <param name="id">Image identifier</param>
        /// <param name="image">Image, of any size</param>
        public static void RegisterImage(string id, Image image)
        {
            if (image == null)
                throw new ArgumentNullException("image");

            if (image.Width == 13 && image.Height == 13)
            {
                int keyIndex = s_images13.Images.IndexOfKey(id);
                if (keyIndex == -1)
                    s_images13.Images.Add(id, image);
                else
                    s_images13.Images[keyIndex] = image;
            }
            else if (image.Width == 16 && image.Height == 16)
            {
                int keyIndex = s_images16.Images.IndexOfKey(id);
                if (keyIndex == -1)
                    s_images16.Images.Add(id, image);
                else
                    s_images16.Images[keyIndex] = image;
            }
            else if (image.Width == 24 && image.Height == 24)
            {
                int keyIndex = s_images24.Images.IndexOfKey(id);
                if (keyIndex == -1)
                    s_images24.Images.Add(id, image);
                else
                    s_images24.Images[keyIndex] = image;
            }
            else if (image.Width == 32 && image.Height == 32)
            {
                int keyIndex = s_images32.Images.IndexOfKey(id);
                if (keyIndex == -1)
                    s_images32.Images.Add(id, image);
                else
                    s_images32.Images[keyIndex] = image;
            }

            s_images[id] = image;
        }

        /// <summary>
        /// Registers a keyed image in three standard icon resolutions</summary>
        /// <param name="id">Image identifier</param>
        /// <param name="image16">16 x 16 image</param>
        /// <param name="image24">24 x 24 image</param>
        /// <param name="image32">32 x 32 image</param>
        /// <remarks>Null may be passed if no image is available, but at least one
        /// image must be non-null</remarks>
        public static void RegisterImage(string id, Image image16, Image image24, Image image32)
        {
            Image bestImage = null;
            if (image16 != null)
            {
                image16 = GdiUtil.ResizeImage(image16, 16);
                s_images16.Images.Add(id, image16);
                bestImage = image16;
            }

            if (image24 != null)
            {
                image24 = GdiUtil.ResizeImage(image24, 24);
                s_images24.Images.Add(id, image24);
                bestImage = image24;
            }

            if (image32 != null)
            {
                image32 = GdiUtil.ResizeImage(image32, 32);
                s_images32.Images.Add(id, image32);
                bestImage = image32;
            }

            if (bestImage == null)
                throw new ArgumentNullException("at least one image must be non-null");

            s_images[id] = bestImage;
        }

        /// <summary>
        /// Gets a registered image given an ID</summary>
        /// <param name="id">Image identifier</param>
        /// <returns>Registered image, or null if not found</returns>
        public static Image GetImage(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");

            Image image;
            s_images.TryGetValue(id, out image);
            return image;
        }

        /// <summary>
        /// Gets a registered 13 x 13 image, given an ID</summary>
        /// <param name="id">Image identifier</param>
        /// <returns>Registered image, or scaled image, or null if not found</returns>
        public static Image GetImage13(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");

            Image image = s_images13.Images[id];
            if (image == null)
            {
                image = GetImage(id);
                if (image != null)
                    image = GdiUtil.ResizeImage(image, 13);
            }
            return image;
        }

        /// <summary>
        /// Gets a registered 16 x 16 image, given an ID</summary>
        /// <param name="id">Image identifier</param>
        /// <returns>Registered image, or scaled image, or null if not found</returns>
        public static Image GetImage16(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");

            Image image = s_images16.Images[id];
            if (image == null)
            {
                image = GetImage(id);
                if (image != null)
                    image = GdiUtil.ResizeImage(image, 16);
            }
            return image;

        }

        /// <summary>
        /// Gets a registered 24 x 24 image, given an ID</summary>
        /// <param name="id">Image identifier</param>
        /// <returns>Registered image, or scaled image, or null if not found</returns>
        public static Image GetImage24(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");

            Image image = s_images24.Images[id];
            if (image == null)
            {
                image = GetImage(id);
                if (image != null)
                    image = GdiUtil.ResizeImage(image, 24);
            }
            return image;
        }

        /// <summary>
        /// Gets a registered 32 x 32 image, given an ID</summary>
        /// <param name="id">Image identifier</param>
        /// <returns>Registered image, or scaled image, or null if not found</returns>
        public static Image GetImage32(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("Image id is null. Call Sce.Atf.WinForms.Resources.Register() to force registration of image resources.");

            Image image = s_images32.Images[id];
            if (image == null)
            {
                image = GetImage(id);
                if (image != null)
                    image = GdiUtil.ResizeImage(image, 32);
            }
            return image;
        }

        /// <summary>
        /// Gets the ImageList with all registered 13 x 13 images</summary>
        /// <returns>ImageList with all registered 13 x 13 images</returns>
        /// <remarks>The returned ImageList is a shared read-only resource.</remarks>
        public static ImageList GetImageList13()
        {
            return s_images13;
        }

        /// <summary>
        /// Gets the ImageList with all registered 16 x 16 images</summary>
        /// <returns>ImageList with all registered 16 x 16 images</returns>
        /// <remarks>The returned ImageList is a shared read-only resource.</remarks>
        public static ImageList GetImageList16()
        {
            return s_images16;
        }

        /// <summary>
        /// Gets the ImageList with all registered 24 x 24 images</summary>
        /// <returns>ImageList with all registered 24 x 24 images</returns>
        /// <remarks>The returned ImageList is a shared read-only resource.</remarks>
        public static ImageList GetImageList24()
        {
            return s_images24;
        }

        /// <summary>
        /// Gets the ImageList with all registered 32 x 32 images</summary>
        /// <returns>ImageList with all registered 32 x 32 images</returns>
        /// <remarks>The returned ImageList is a shared read-only resource.</remarks>
        public static ImageList GetImageList32()
        {
            return s_images32;
        }

        /// <summary>
        /// Gets a registered Cursor, given an ID</summary>
        /// <param name="id">Cursor identifier</param>
        /// <returns>Registered Cursor, or null if not found</returns>
        public static Cursor GetCursor(string id)
        {
            Cursor cursor;
            s_cursors.TryGetValue(id, out cursor);
            return cursor;
        }

        /// <summary>
        /// Registers any attributed fields found on the type and assumes that the namespace of the
        /// embedded resource is the same as that of the given type</summary>
        /// <param name="type">Type to scan for the attributes Sce.Atf.ImageResourceAttribute and
        /// Sce.Atf.CursorResourceAttribute, which are usually on static public readonly fields</param>
        /// <example>If the full name of 'type' is MyApp.Resources then:
        /// 1. The Project's namespace should be MyApp.
        /// 2. The embedded resources should be in a folder named Resources.</example>
        public static void Register(Type type)
        {
            string resourcePath = type.FullName + ".";
            Register(type, resourcePath);
        }

        /// <summary>
        /// Registers any attributed fields found on the type</summary>
        /// <param name="type">Type to scan for the attributes Sce.Atf.ImageResourceAttribute and
        /// Sce.Atf.CursorResourceAttribute, which are usually on static public readonly fields</param>
        /// <param name="resourcePath">Path to resources in the given type's assembly, deliminated by '.'
        /// and ending in a '.'</param>
        public static void Register(Type type, string resourcePath)
        {
            Assembly resourceAssembly = type.Assembly;
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Static | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {
                object[] attributes;

                attributes = field.GetCustomAttributes(typeof(ImageResourceAttribute), false);
                if (attributes.Length > 0)
                {
                    ImageResourceAttribute attribute = attributes[0] as ImageResourceAttribute;
                    string name1 = resourcePath + attribute.ImageName1;
                    field.SetValue(null, name1);
                    string name2 = attribute.ImageName2 != null ? resourcePath + attribute.ImageName2 : null;
                    string name3 = attribute.ImageName3 != null ? resourcePath + attribute.ImageName3 : null;
                    RegisterImage(resourceAssembly, name1, name2, name3);

                    continue;
                }

                attributes = field.GetCustomAttributes(typeof(CursorResourceAttribute), false);
                if (attributes.Length > 0)
                {
                    CursorResourceAttribute attribute = attributes[0] as CursorResourceAttribute;
                    string name = resourcePath + attribute.CursorName;
                    field.SetValue(null, name);
                    RegisterCursor(resourceAssembly, name);

                    continue;
                }
            }
        }

        /// <summary>
        /// Used by auto-registration code in Atf.Gui.Resources to ensure that automatic registration 
        /// is only done once. Does not affect explicit Register calls.</summary>
        public static bool RegistrationStarted
        {
            get;
            set;
        }

        private static void RegisterCursor(Assembly resourceAssembly, string name)
        {
            Cursor cursor = null;
            System.IO.Stream strm = null;
            try
            {
                strm = resourceAssembly.GetManifestResourceStream(name);
                cursor = new Cursor(strm);
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
            }
            finally
            {
                if (strm != null)
                    strm.Close();
            }

            if (cursor != null && !s_cursors.ContainsKey(name))
                s_cursors.Add(name, cursor);
        }

        private static void RegisterImage(
            Assembly resourceAssembly,
            string name1,
            string name2,
            string name3)
        {
            if (name1 != null)
            {
                if (name2 == null || name3 == null)
                {
                    RegisterImage(name1, GdiUtil.GetImage(resourceAssembly, name1));
                }
                else
                {
                    RegisterImage(
                        name1,
                        GdiUtil.GetImage(resourceAssembly, name1),
                        GdiUtil.GetImage(resourceAssembly, name2),
                        GdiUtil.GetImage(resourceAssembly, name3));
                }
            }
        }

        static ResourceUtil()
        {
            s_images13.ColorDepth = ColorDepth.Depth32Bit;
            s_images13.TransparentColor = Color.Transparent;
            s_images13.ImageSize = new Size(13, 13);

            s_images16.ColorDepth = ColorDepth.Depth32Bit;
            s_images16.TransparentColor = Color.Transparent;
            s_images16.ImageSize = new Size(16, 16);

            s_images24.ColorDepth = ColorDepth.Depth32Bit;
            s_images24.TransparentColor = Color.Transparent;
            s_images24.ImageSize = new Size(24, 24);

            s_images32.ColorDepth = ColorDepth.Depth32Bit;
            s_images32.TransparentColor = Color.Transparent;
            s_images32.ImageSize = new Size(32, 32);

            Register(typeof(Sce.Atf.Resources));
        }

        // all images; image is always the largest available for the given key
        private static readonly Dictionary<string, Image> s_images = new Dictionary<string, Image>();

        // ImageLists, in 4 standard sizes
        private static readonly ImageList s_images13 = new ImageList();
        private static readonly ImageList s_images16 = new ImageList();
        private static readonly ImageList s_images24 = new ImageList();
        private static readonly ImageList s_images32 = new ImageList();

        private static readonly Dictionary<string, Cursor> s_cursors = new Dictionary<string, Cursor>();
    }
}
