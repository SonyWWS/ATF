//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// Extension for working with generic types</summary>
    [ContentProperty("TypeArguments")]
    public class GenericExtension : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Gets the collection of type arguments for the generic type</summary>
        public Collection<Type> TypeArguments
        {
            get { return _typeArguments; }
        }

        /// <summary>
        /// Gets or sets the generic type name (e.g. Dictionary, for the Dictionary&lt;K,V&gt; case)</summary>
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        /// <summary>
        /// Default constructor</summary>
        public GenericExtension()
        {
        }

        /// <summary>
        /// Constructor with type name</summary>
        /// <param name="typeName">The generic type name</param>
        public GenericExtension(string typeName)
        {
            TypeName = typeName;
        }

        /// <summary>
        /// ProvideValue, which returns the concrete object of the generic type</summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <returns>The instance of the type</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            string[] baseTypeArray = _typeName.Split(':');

            var namespaceResolver =
                serviceProvider.GetService(typeof(IXamlTypeResolver)) as System.Xaml.IXamlNamespaceResolver;
            if (namespaceResolver == null)
                throw new Exception("The Generic markup extension requires an IXamlNamespaceResolver service provider");

            var schemaContextProvider = 
                serviceProvider.GetService(typeof(System.Xaml.IXamlSchemaContextProvider)) as System.Xaml.IXamlSchemaContextProvider;
            if (schemaContextProvider == null)
                throw new Exception("The Generic markup extension requires an IXamlSchemaContextProvider service provider");

            var xamlNamespace = namespaceResolver.GetNamespace(baseTypeArray[0]);
            string name = baseTypeArray[1] + "`" + TypeArguments.Count.ToString();
            var xamlTypeName = new System.Xaml.Schema.XamlTypeName(xamlNamespace, name);
            var xamlType = schemaContextProvider.SchemaContext.GetXamlType(xamlTypeName);

            if (xamlType == null)
                throw new Exception("The type could not be resolved");
            
            Type genericType = xamlType.UnderlyingType;
            
            // Get an array of the type arguments
            Type[] typeArgumentArray = new Type[TypeArguments.Count];
            TypeArguments.CopyTo(typeArgumentArray, 0);

            // Create the concrete type, e.g. Collection<String>
            var concreteType = genericType.MakeGenericType(typeArgumentArray);
            
            // Create an instance of that type
            return Activator.CreateInstance(concreteType);
        }

        #region IValueConverter Members

        /// <summary>
        /// Converts a value. Not implemented</summary>
        /// <exception cref="NotSupportedException"> is raised</exception>
        /// <param name="value">Value produced by the binding source</param>
        /// <param name="targetType">Type of the binding target property</param>
        /// <param name="parameter">Converter parameter to use</param>
        /// <param name="culture">Culture to use in the converter</param>
        /// <returns>Converted value. If the method returns null, the valid null value is used.</returns>
        public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Converts back a value. Not implemented</summary>
        /// <exception cref="NotSupportedException"> is raised</exception>
        /// <param name="value">Value that is produced by the binding target</param>
        /// <param name="targetType">Type to convert to</param>
        /// <param name="parameter">Converter parameter to use</param>
        /// <param name="culture">Culture to use in the converter</param>
        /// <returns>Converted value from the target value back to the source value. If the method returns null, the valid null value is used.</returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        private Collection<Type> _typeArguments = new Collection<Type>();

        private string _typeName;

    }
}
