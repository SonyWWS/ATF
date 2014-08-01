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
        /// Gets and sets the generic type name (e.g. Dictionary, for the Dictionary<K,V> case)</summary>
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        /// <summary>
        /// Constructor</summary>
        public GenericExtension()
        {
        }

        /// <summary>
        /// Constructor</summary>
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
#if CS_4
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
#else
            IXamlTypeResolver xamlTypeResolver = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
            if (xamlTypeResolver == null)
                throw new Exception("The Generic markup extension requires an IXamlTypeResolver service provider");
            
            // Get e.g. "Collection`1" type
            string name = _typeName +"`" + TypeArguments.Count.ToString();
            Type genericType = xamlTypeResolver.Resolve(name);
            
            // Get an array of the type arguments
            Type[] typeArgumentArray = new Type[TypeArguments.Count];
            TypeArguments.CopyTo(typeArgumentArray, 0);

            // Create the concrete type, e.g. Collection<String>
            Type concreteType = genericType.MakeGenericType(typeArgumentArray);

            // Create an instance of that type
            return Activator.CreateInstance(concreteType);
#endif
        }

        #region IValueConverter Members

        /// <summary>
        /// Not implemented</summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not implemented</summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion

        private Collection<Type> _typeArguments = new Collection<Type>();

        private string _typeName;

    }
}
