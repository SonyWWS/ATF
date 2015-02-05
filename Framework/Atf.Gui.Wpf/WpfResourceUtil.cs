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
    /// WPF resource utilities</summary>
    [Obsolete("Please use Sce.Atf.Wpf.ResourceUtil instead.")]
    public static class WpfResourceUtil
    {
        /// <summary>
        /// Retrieves the custom attribute of the specified type applied to the assembly</summary>
        /// <typeparam name="T">Type of custom attribute</typeparam>
        /// <param name="assembly">Assembly the attribute is applied to</param>
        /// <param name="value">Function called with the attribute as a parameter</param>
        /// <returns>The value returned by the value.Invoke call</returns>
        /// <example>
        /// An example of GetAssemblyAttribute usage:
        /// <code>var company = assembly.GetAssemblyAttribute&lt;AssemblyCompanyAttribute&gt;(v => v.Company);</code>
        /// </example>
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

            ResourceDictionary imageResources = null;

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
                        try
                        {
                            img = new BitmapImage(uri);
                        }
                        catch (IOException ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
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
                    else if (extension == ".cur")
                    {
                        var uri = new Uri(relativePackUriBase + attribute.ImageName, UriKind.Relative);
                        
                        try
                        {
                            var info = Application.GetResourceStream(uri);
                            img = new FreezableCursor {Cursor = new System.Windows.Input.Cursor(info.Stream)};
                        }
                        catch (IOException ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Unrecognized Wpf image resource file extension for file: " + attribute.ImageName);
                    }

                    if (img != null)
                    {
                        // Might it be better to keep the img as a DrawingGroup and then assign to relevant Image type later?
                        if (img is DrawingGroup)
                        {
                            var brush = new DrawingBrush(img as DrawingGroup);
                            brush.Stretch = Stretch.Uniform;
                            img = brush;
                        }

                        if (img.CanFreeze && !img.IsFrozen)
                            img.Freeze();

                        object imageKey = type.FullName + "." + attribute.ImageName;

                        if (field.FieldType.IsAssignableFrom(typeof(ResourceKey)))
                        {
                            imageKey = new ComponentResourceKey(type, imageKey);
                        }

                        field.SetValue(type, imageKey);

                        if (imageResources == null)
                            imageResources = new ResourceDictionary();

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

            if (imageResources != null)
                Application.Current.Resources.MergedDictionaries.Add(imageResources);
        }

        private static readonly HashSet<Type> s_registeredTypes = new HashSet<Type>();
    }
}
