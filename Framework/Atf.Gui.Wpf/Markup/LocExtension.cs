//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// Represents a localization markup extension.
    /// Use this in XAML to hook into ATF localization system.</summary>
    [MarkupExtensionReturnType(typeof(string))]
    [ContentProperty("Key")]
    public class LocExtension : MarkupExtension
    {
        /// <summary>
        /// Constructor</summary>
        public LocExtension() { }

        /// <summary>
        /// Constructor with resource key</summary>
        /// <param name="key">The resource key</param>
        public LocExtension(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Gets or sets the resource key</summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the formatting string to use</summary>
        public string Format { get; set; }

        /// <summary>
        /// Returns the object that corresponds to the resource key</summary>
        /// <param name="serviceProvider">An object that can provide services for the markup extension</param>
        /// <returns>The object that corresponds to the specified resource key</returns>
        /// <remarks>See http://msdn.microsoft.com/en-us/library/system.windows.markup.markupextension.providevalue.aspx. </remarks>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key))
                return string.Empty;

            return string.IsNullOrEmpty(Format) ? Key.Localize() : string.Format(Format, Key.Localize());
        }
    }
}
