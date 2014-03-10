//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property display sort orderings</summary>
    [Flags]
    public enum PropertySorting
    {
        /// <summary>
        /// Display properties in the order they are presented</summary>
        None = 0,

        /// <summary>
        /// Group properties by category</summary>
        Categorized = 1,

        /// <summary>
        /// Sort properties alphabetically by display name</summary>
        Alphabetical = 2,

        /// <summary>
        /// Sort properties alphabetically by category name</summary>
        CategoryAlphabetical = 4,

        /// <summary>
        /// Sort properties using a custom ordering specified by a list of property names via SetCustomPropertySortOrder()</summary>
        Custom = 8,

        /// <summary>
        /// Group properties by category, sort by category name, alphabetize in category, then display</summary>
        ByCategory = Categorized | Alphabetical | CategoryAlphabetical,
    }
}
