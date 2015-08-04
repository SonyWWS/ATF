//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf
{
    /// <summary>
    /// Attribute to tag a field as having a description, which can be localized by 
    /// development tool LocalizableStringExtractor.  For more specifics, refer to that project.
    /// When specified on a class element, instead of DescriptionAttribute, any code looking for 
    /// DescriptionAttribute class elements will receive the localized version of the description</summary>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        /// <summary>
        /// Default constructor, necessary to be XAML friendly</summary>
        public LocalizedDescriptionAttribute() { }

        /// <summary>
        /// Constructor used when [LocalizedDescription("Some description")] is declared before any class element</summary>
        public LocalizedDescriptionAttribute(string description) 
            : base(description) { }

        /// <summary>
        /// Returns the localized version of the description, as passed to the constructor</summary>
        public override string Description
        {
            get { return base.Description.Localize(); }
        }

        /// <summary>
        /// Returns the original description string, as passed to the constructor</summary>
        public string Key { get { return base.Description; } }
    }
}