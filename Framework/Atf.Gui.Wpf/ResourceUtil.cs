//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Resource keys for styles and images</summary>
    public static class ResourceUtil
    {
        /// <summary>
        /// Registers any attributed fields found on the type</summary>
        /// <param name="owningType">Type to scan for the attributes Sce.Atf.ImageResourceAttribute and
        /// Sce.Atf.CursorResourceAttribute, which are usually on static public readonly fields</param>
        public static void Register(Type owningType)
        {
            Register(owningType, "Resources/");
        }

        /// <summary>
        /// Registers any attributed fields found on the type</summary>
        /// <param name="owningType">Type to scan for the attributes Sce.Atf.ImageResourceAttribute and
        /// Sce.Atf.CursorResourceAttribute, which are usually on static public readonly fields</param>
        /// <param name="subPath">Path to resources, e.g., "Resources/"</param>
        public static void Register(Type owningType, string subPath)
        {
            if (s_registeredTypes.ContainsKey(owningType))
                throw new Exception("Cannot register the resources of the same type twice");

            s_registeredTypes[owningType] = owningType;

            if (string.IsNullOrEmpty(subPath))
                throw new ArgumentNullException("subPath");

            var imageResources = new ResourceDictionary();

            FieldInfo[] fields = owningType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (TryGetImageResource(owningType, field, imageResources, subPath))
                    continue;

                if (TryGetResourceDictionaryResource(owningType, field))
                    continue;
            }

            if (imageResources.Count > 0)
                Application.Current.Resources.MergedDictionaries.Add(imageResources);
        }

        /// <summary>
        /// Used by auto-registration code in Atf.Gui.Resources to ensure that automatic registration 
        /// is only done once. Does not affect explicit Register calls.</summary>
        public static bool RegistrationStarted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the ResourceKey, if available, from the image name. This allows WPF apps to use both
        /// WPF-based resources and embedded WinForms style resources.</summary>
        /// <remarks>
        /// WPF resources in Atf.Gui.Wpf, and any other WPF resources explicitly registered by calling 
        /// ResourceUtil.Register, will appear in s_resourceKeys and so the ResourceKey will be returned. 
        /// WinForms style embedded resources, including those in Atf.Gui (such as ATF's standard 
        /// application and menu icons), are not WPF resources and are not included in s_resourceKeys, 
        /// so the name of the resource is returned instead of a ResourceKey. For an example, see the
        /// usage in CommandService.RegisterCommandInfo, which allows both WPF and WinForms resources
        /// to be used as menu/toolbar icons.</remarks>
        /// <param name="imageName">Name of the image</param>
        /// <returns>The image's resource key if it exists, otherwise returns imageName.</returns>
        public static object GetKeyFromImageName(string imageName)
        {
            if (s_resourceKeys.ContainsKey(imageName))
                return s_resourceKeys[imageName];
            return imageName;
        }

        /// <summary>
        /// Retrieves the custom attribute of the specified type applied to the assembly</summary>
        /// <typeparam name="T">Type of custom attribute</typeparam>
        /// <param name="assembly">Assembly the attribute is applied to</param>
        /// <param name="value">Function called with the attribute as a parameter</param>
        /// <returns>The value returned by the value.Invoke call</returns>
        /// <remarks>
        /// For example:
        ///     var company = assembly.GetAssemblyAttribute<AssemblyCompanyAttribute>(v => v.Company);
        /// </remarks>
        public static string GetAssemblyAttribute<T>(this Assembly assembly, Func<T, string> value)
            where T : Attribute
        {
            var attribute = (T)Attribute.GetCustomAttribute(assembly, typeof(T));
            return value.Invoke(attribute);
        }

        /// <summary>
        /// Gets ImageSource for image given name</summary>
        /// <param name="name">Image name</param>
        /// <returns>ImageSource for image with specified name</returns>
        public static ImageSource GetImage(string name)
        {
            return string.IsNullOrEmpty(name) ? null : Application.Current.Resources[name] as ImageSource;
        }

        private static bool TryGetImageResource(Type owningType, FieldInfo field, ResourceDictionary imageResources, string subPath)
        {
            if (string.IsNullOrEmpty(subPath))
                throw new ArgumentNullException("subPath");

            // Get Image attributes
            object[] attributes = field.GetCustomAttributes(typeof(ImageResourceAttribute), false);

            if (attributes.Length == 0)
                return false;

            var attribute = (ImageResourceAttribute)attributes[0];
            Freezable img = null;

            if (string.IsNullOrEmpty(attribute.ImageName1))
                throw new InvalidOperationException("Cannot have an image attribute whose first image name is null or empty");
            var extension = Path.GetExtension(attribute.ImageName1);
            if (string.IsNullOrEmpty(extension))
                throw new InvalidOperationException("Cannot have an image attribute whose first image name has no extension");
            extension = extension.ToLower();

            // Generate packUri
            var assmName = owningType.Assembly.GetName();
            var basePackUriRel = "/" + assmName + ";component/" + subPath;
            if (!basePackUriRel.EndsWith("/"))
                basePackUriRel += "/";

            if (extension == ".xaml")
            {
                var uri = new Uri(basePackUriRel + attribute.ImageName1, UriKind.Relative);
                img = Application.LoadComponent(uri) as Freezable;
                if (img == null)
                    throw new InvalidOperationException("Invalid xaml image resource: " + attribute.ImageName1);
            }
            else if (extension == ".cur")
            {
                var uri = new Uri(basePackUriRel + attribute.ImageName1, UriKind.Relative);

                try
                {
                    var info = Application.GetResourceStream(uri);
                    img = new FreezableCursor { Cursor = new System.Windows.Input.Cursor(info.Stream) };
                }
                catch (IOException ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
            else if (extension == ".bmp" || extension == ".png" || extension == ".ico")
            {
                // First attempt to retrieve the image as an 'Embedded Resource' (ie legacy WinForms-era resource compilation)
                var stream = owningType.Assembly.GetManifestResourceStream(owningType + "." + attribute.ImageName1);
                if (stream != null)
                    img = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
                else
                {
                    // image must be compiled in as a 'Linked Resource' (ie WPF-era resource compilation)
                    var basePackUriAbs = "pack://application:,,," + basePackUriRel;
                    var uri = new Uri(basePackUriAbs + attribute.ImageName1);
                    try
                    {
                        img = new BitmapImage(uri);
                    }
                    catch (IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }
                }
            }
            else
                throw new Exception(
                    "Unrecognized extension '" + extension + "' on image resource '" + field.Name + "'");

            if (img == null)
                throw new Exception("Failed to create image from image resource '" + field.Name + "'");

            // Might it be better to keep the img as a DrawingGroup and then assign to relevant Image type later?
            if (img is DrawingGroup)
            {
                var brush = new DrawingBrush(img as DrawingGroup);
                brush.Stretch = Stretch.Uniform;
                img = brush;
            }

            if (img.CanFreeze && !img.IsFrozen)
                img.Freeze();

            object imageKey = owningType.FullName + "." + attribute.ImageName1;

            if (field.FieldType.IsAssignableFrom(typeof(ResourceKey)))
            {
                var componentResourceKey = new ComponentResourceKey(owningType, imageKey);
                s_resourceKeys[attribute.ImageName1] = componentResourceKey;
                imageKey = componentResourceKey;
            }

            field.SetValue(owningType, imageKey);
            if (!imageResources.Contains(imageKey))
                imageResources.Add(imageKey, img);

            return true;
        }

        private static bool TryGetResourceDictionaryResource(Type owningType, FieldInfo field)
        {
            // Get ResourceDictionary attributes
            if (field.FieldType != typeof (string))
                return false;

            object[] attributes = field.GetCustomAttributes(typeof(ResourceDictionaryResourceAttribute), false);

            if (attributes.Length == 0)
                return false;

            var attribute = attributes[0] as ResourceDictionaryResourceAttribute;
            var dictionary = new ResourceDictionary();
            var resourcesAssy = owningType.Assembly;
            var uri = new Uri("pack://application:,,,/" + resourcesAssy.GetName() + ";component/Resources/" + attribute.Path);
            dictionary.Source = uri;

            Application.Current.Resources.MergedDictionaries.Add(dictionary);            
            return true;
        }

        private static readonly Dictionary<string, ResourceKey> s_resourceKeys = new Dictionary<string, ResourceKey>();

        private static readonly Dictionary<Type, Type> s_registeredTypes = new Dictionary<Type, Type>();
    }
}
