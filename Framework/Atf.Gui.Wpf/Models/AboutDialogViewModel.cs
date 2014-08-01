//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for AboutDialog</summary>
    public class AboutDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        public AboutDialogViewModel()
        {
            Title = "About".Localize() + " " + ProductTitle;

            Assemblies = new List<string>();
            var entryAssembly = Assembly.GetEntryAssembly();
            foreach (var assembly in entryAssembly.GetReferencedAssemblies())
            {
                Assemblies.Add(assembly.Name + ", Version=" + assembly.Version);
            }
        }

        /// <summary>
        /// Gets the assemblies referenced from the current assembly</summary>
        public List<string> Assemblies { get; private set; } 

        /// <summary>
        /// Gets the Title property, which is the About dialog's window title</summary>
        public string ProductTitle
        {
            get
            {
                string result = Product;
                if (string.IsNullOrEmpty(result))
                {
                    // otherwise, just get the name of the assembly itself.
                    result = System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the application's version information to show in the About dialog</summary>
        public string Version
        {
            get
            {
                string result = string.Empty;
                var assm = Assembly.GetEntryAssembly();
                object[] attributes = assm.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                if (attributes.Length > 0)
                {
                    result = (attributes[0] as AssemblyInformationalVersionAttribute).InformationalVersion;
                }
                else
                {
                    Version version = assm.GetName().Version;
                    if (version != null)
                    {
                        result = version.ToString();
                    }
                    else
                    {
                        // if that fails, try to get the version from a resource in the Application.
                        result = GetLogicalResourceString(m_xPathVersion);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the application's file version information to show in the About dialog. 
        /// This version may be different than the application's version.</summary>
        public string FileVersion
        {
            get
            {
                string result = string.Empty;
                var assm = Assembly.GetEntryAssembly();
                object[] attributes = assm.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if (attributes.Length > 0)
                {
                    result = (attributes[0] as AssemblyFileVersionAttribute).Version;
                }
                else
                {
                    Version version = assm.GetName().Version;
                    if (version != null)
                    {
                        result = version.ToString();
                    }
                    else
                    {
                        // if that fails, try to get the version from a resource in the Application.
                        result = GetLogicalResourceString(m_xPathVersion);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the application's description</summary>
        public string Description
        {
            get { return CalculatePropertyValue<AssemblyDescriptionAttribute>(m_propertyNameDescription, m_xPathDescription); }
        }

        /// <summary>
        ///  Gets the product's full name</summary>
        public string Product
        {
            get { return CalculatePropertyValue<AssemblyProductAttribute>(m_propertyNameProduct, m_xPathProduct); }
        }

        /// <summary>
        /// Gets the copyright information for the product</summary>
        public string Copyright
        {
            get { return CalculatePropertyValue<AssemblyCopyrightAttribute>(m_propertyNameCopyright, m_xPathCopyright); }
        }

        /// <summary>
        /// Gets the product's company name</summary>
        public string Company
        {
            get { return CalculatePropertyValue<AssemblyCompanyAttribute>(m_propertyNameCompany, m_xPathCompany); }
        }

        /// <summary>
        /// Gets the link text to display in the About dialog</summary>
        public string LinkText
        {
            get { return GetLogicalResourceString(m_xPathLink); }
        }

        /// <summary>
        /// Gets the link URI that is the navigation target of the link</summary>
        public string LinkUri
        {
            get { return GetLogicalResourceString(m_xPathLinkUri); }
        }

        /// <summary>
        /// Gets the banner image path.
        /// </summary>
        public Uri Banner
        {
            get
            {
                Uri result = null;

                var assembly = Assembly.GetEntryAssembly();
                AssemblyName assmName = assembly.GetName();

                // first, try to get the property value from an attribute.
                object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyBannerAttribute), false);
                if (attributes.Length > 0)
                {
                    var attrib = (AssemblyBannerAttribute)attributes[0];
                    PropertyInfo property = attrib.GetType().GetProperty("BannerPath", BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        var path = property.GetValue(attributes[0], null) as string;
                        if (!string.IsNullOrEmpty(path))
                        {
                            string relativePackUriBase = "/" + assmName.Name + ";v" + assmName.Version + ";component/" + path;
                            string absolutePackUriBase = "pack://application:,,," + relativePackUriBase;

                            result = new Uri(absolutePackUriBase);
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the XmlDataProvider's document from the resource dictionary</summary>
        protected virtual XmlDocument ResourceXmlDocument
        {
            get
            {
                if (m_xmlDoc == null)
                {
                    // if we haven't already found the resource XmlDocument, then try to find it.
                    XmlDataProvider provider = Application.Current.TryFindResource("aboutProvider") as XmlDataProvider;
                    if (provider != null)
                    {
                        // save away the XmlDocument, so we don't have to get it multiple times.
                        m_xmlDoc = provider.Document;
                    }
                }

                return m_xmlDoc;
            }
        }

        /// <summary>
        /// Gets the specified data element from the XmlDataProvider in the resource dictionary</summary>
        /// <param name="xpathQuery">An XPath query to the XML element to retrieve</param>
        /// <returns>The resulting string value for the specified XML element. 
        /// Returns empty string if resource element couldn't be found.</returns>
        protected virtual string GetLogicalResourceString(string xpathQuery)
        {
            string result = string.Empty;
            // get the About xml information from the resources.
            XmlDocument doc = ResourceXmlDocument;
            if (doc != null)
            {
                // if we found the XmlDocument, then look for the specified data. 
                XmlNode node = doc.SelectSingleNode(xpathQuery);
                if (node != null)
                {
                    if (node is XmlAttribute)
                    {
                        // only an XmlAttribute has a Value set.
                        result = node.Value;
                    }
                    else
                    {
                        // otherwise, need to just return the inner text.
                        result = node.InnerText;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the specified property value either from a specific attribute, or from a resource dictionary</summary>
        /// <typeparam name="T">Attribute type that we're trying to retrieve</typeparam>
        /// <param name="propertyName">Property name to use on the attribute</param>
        /// <param name="xpathQuery">XPath to the element in the XML data resource</param>
        /// <returns>The resulting string to use for a property.
        /// Returns null if no data could be retrieved.</returns>
        private string CalculatePropertyValue<T>(string propertyName, string xpathQuery)
        {
            string result = string.Empty;
            // first, try to get the property value from an attribute.
            object[] attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                T attrib = (T)attributes[0];
                PropertyInfo property = attrib.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    result = property.GetValue(attributes[0], null) as string;
                }
            }

            // if the attribute wasn't found or it did not have a value, then look in an xml resource.
            if (result == string.Empty)
            {
                // if that fails, try to get it from a resource.
                result = GetLogicalResourceString(xpathQuery);
            }

            return result;
        }

        private XmlDocument m_xmlDoc = null;

        private const string m_propertyNameTitle = "Title";
        private const string m_propertyNameDescription = "Description";
        private const string m_propertyNameProduct = "Product";
        private const string m_propertyNameCopyright = "Copyright";
        private const string m_propertyNameCompany = "Company";
        private const string m_xPathRoot = "ApplicationInfo/";
        private const string m_xPathTitle = m_xPathRoot + m_propertyNameTitle;
        private const string m_xPathVersion = m_xPathRoot + "Version";
        private const string m_xPathDescription = m_xPathRoot + m_propertyNameDescription;
        private const string m_xPathProduct = m_xPathRoot + m_propertyNameProduct;
        private const string m_xPathCopyright = m_xPathRoot + m_propertyNameCopyright;
        private const string m_xPathCompany = m_xPathRoot + m_propertyNameCompany;
        private const string m_xPathLink = m_xPathRoot + "Link";
        private const string m_xPathLinkUri = m_xPathRoot + "Link/@Uri";

    }
}
