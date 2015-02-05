//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Markup;
using Sce.Atf.Wpf.ValueConverters;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// Converts between type names and Types for source and target types</summary>
    public sealed class TypeConverterExtension : MarkupExtension
    {
        /// <summary>
        /// Gets or sets the source type for the <see cref="TypeConverter"/></summary>
        [ConstructorArgument("sourceType")]
        public Type SourceType
        {
            get { return m_sourceTypeExtension.Type; }
            set { m_sourceTypeExtension.Type = value; }
        }

        /// <summary>
        /// Gets or sets the target type for the <see cref="TypeConverter"/></summary>
        [ConstructorArgument("targetType")]
        public Type TargetType
        {
            get { return m_targetTypeExtension.Type; }
            set { m_targetTypeExtension.Type = value; }
        }

        /// <summary>
        /// Gets or sets the name of the source type for the <see cref="TypeConverter"/></summary>
        [ConstructorArgument("sourceTypeName")]
        public string SourceTypeName
        {
            get { return m_sourceTypeExtension.TypeName; }
            set { m_sourceTypeExtension.TypeName = value; }
        }

        /// <summary>
        /// Gets or sets the name of the target type for the <see cref="TypeConverter"/></summary>
        [ConstructorArgument("targetTypeName")]
        public string TargetTypeName
        {
            get { return m_targetTypeExtension.TypeName; }
            set { m_targetTypeExtension.TypeName = value; }
        }

        /// <summary>
        /// Constructs a default instance of the <c>TypeConverterExtension</c> class</summary>
        public TypeConverterExtension()
        {
            m_sourceTypeExtension = new TypeExtension();
            m_targetTypeExtension = new TypeExtension();
        }

        /// <summary>
        /// Constructs an instance of <c>TypeConverterExtension</c> with the specified source 
        /// and target types</summary>
        /// <param name="sourceType">The source type for the <see cref="TypeConverter"/>.</param>
        /// <param name="targetType">The target type for the <see cref="TypeConverter"/>.</param>
        public TypeConverterExtension(Type sourceType, Type targetType)
        {
            m_sourceTypeExtension = new TypeExtension(sourceType);
            m_targetTypeExtension = new TypeExtension(targetType);
        }

        /// <summary>
        /// Constructs an instance of <c>TypeConverterExtension</c> with the specified source 
        /// and target types</summary>
        /// <param name="sourceTypeName">The source type name for the <see cref="TypeConverter"/>.</param>
        /// <param name="targetTypeName">The target type name for the <see cref="TypeConverter"/>.</param>
        public TypeConverterExtension(string sourceTypeName, string targetTypeName)
        {
            m_sourceTypeExtension = new TypeExtension(sourceTypeName);
            m_targetTypeExtension = new TypeExtension(targetTypeName);
        }

        /// <summary>
        /// Provides an instance of <see cref="TypeConverter"/> based on this <c>TypeConverterExtension</c></summary>
        /// <param name="serviceProvider">An object that can provide services</param>
        /// <returns>The instance of <see cref="TypeConverter"/></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Type sourceType = null;
            Type targetType = null;

            if ((m_sourceTypeExtension.Type != null) || (m_sourceTypeExtension.TypeName != null))
            {
                sourceType = m_sourceTypeExtension.ProvideValue(serviceProvider) as Type;
            }

            if ((m_targetTypeExtension.Type != null) || (m_targetTypeExtension.TypeName != null))
            {
                targetType = m_targetTypeExtension.ProvideValue(serviceProvider) as Type;
            }

            //just let the TypeExtensions do the type resolving via the service provider
            return new TypeConverter(sourceType, targetType);
        }

        private readonly TypeExtension m_sourceTypeExtension;
        private readonly TypeExtension m_targetTypeExtension;
    }
}
