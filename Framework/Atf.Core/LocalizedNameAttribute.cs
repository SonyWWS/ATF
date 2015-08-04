using System.ComponentModel;

namespace Sce.Atf
{
    /// <summary>
    /// This attribute can be put on properties, methods, classes, and events, to give
    /// them a display name that is different than the code name. ATF's LocalizableStringExtractor
    /// tool will look for instances of LocalizedName("My Custom Name") used as an attribute
    /// (note that you must not include the "Attribute" part of the type name).</summary>
    public class LocalizedNameAttribute : DisplayNameAttribute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="displayName">The display name that will be automatically localized</param>
        public LocalizedNameAttribute(string displayName)
            : base(displayName)
        {
        }

        /// <summary>
        /// Gets the display name, localized</summary>
        public override string DisplayName
        {
            get
            {
                return base.DisplayName.Localize();
            }
        }
    }
}
