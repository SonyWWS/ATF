//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// WPF resource utilities</summary>
    public static class WpfResourceUtil
    {
        /// <summary>
        /// Gets ImageSource for image given name</summary>
        /// <param name="name">Image name</param>
        /// <returns>ImageSource for image with specified name</returns>
        public static ImageSource GetImage(string name)
        {
            return string.IsNullOrEmpty(name) ? null : Application.Current.Resources[name] as ImageSource;
        }

        /// <summary>
        /// Registers any attributed resource keys found on the type</summary>
        /// <param name="type">Type, usually a static set of keys</param>
        /// <param name="resourcePath">Path to resources</param>
        public static void Register(Type type, string resourcePath)
        {
            if (s_registeredTypes.Contains(type))
                return;
            s_registeredTypes.Add(type);

            // Generate packUri
            AssemblyName assmName = type.Assembly.GetName();
            string relativePackUriBase = "/" + assmName.Name + ";component/" + resourcePath;
            string absolutePackUriBase = "pack://application:,,," + relativePackUriBase;

            var imageResources = new ResourceDictionary();

            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                // Get Image attributes
                object[] attributes = field.GetCustomAttributes(typeof(WpfImageResourceAttribute), false);

                if (attributes.Length > 0)
                {
                    if(!field.FieldType.IsAssignableFrom(typeof(ResourceKey)))
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: WpfImageResourceAttribute used on a field which is not a ResourceKey");
                    }

                    var attribute = (WpfImageResourceAttribute)attributes[0];
                    Freezable img = null;
                    string extension = System.IO.Path.GetExtension(attribute.ImageName).ToLower();

                    if (extension == ".bmp" || extension == ".png" || extension == ".ico")
                    {
                        var uri = new Uri(absolutePackUriBase + attribute.ImageName);
                        img = new BitmapImage(uri);
                    }
                    else if (extension == ".xaml")
                    {
                        var uri = new Uri(relativePackUriBase + attribute.ImageName, UriKind.Relative);
                        img = Application.LoadComponent(uri) as Freezable;

                        if (img == null)
                        {
                            throw new InvalidOperationException("Invalid xaml image resource: " + attribute.ImageName);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unrecognized Wpf image resource file extension for file: " + attribute.ImageName);
                    }

                    if (img != null)
                    {
                        if (img.CanFreeze && !img.IsFrozen)
                            img.Freeze();

                        object imageKey = type.FullName + "." + attribute.ImageName;

                        if (field.FieldType.IsAssignableFrom(typeof(ResourceKey)))
                        {
                            imageKey = new ComponentResourceKey(type, imageKey);
                        }

                        field.SetValue(type, imageKey);
                        imageResources.Add(imageKey, img);
                    }
                }

                // Get ResourceDictionary attributes
                if(field.FieldType == typeof(string))
                {
                    attributes = field.GetCustomAttributes(typeof(ResourceDictionaryResourceAttribute), false);

                    if (attributes.Length > 0)
                    {
                        var attribute = attributes[0] as ResourceDictionaryResourceAttribute;

                        var dictionary = new ResourceDictionary();
                        string uri = absolutePackUriBase + attribute.Path;
                        dictionary.Source = new Uri(uri, UriKind.RelativeOrAbsolute);

                        Application.Current.Resources.MergedDictionaries.Add(dictionary);
                    }
                }
            }

            Application.Current.Resources.MergedDictionaries.Add(imageResources);
        }

        private static readonly HashSet<Type> s_registeredTypes = new HashSet<Type>();
    }
}
