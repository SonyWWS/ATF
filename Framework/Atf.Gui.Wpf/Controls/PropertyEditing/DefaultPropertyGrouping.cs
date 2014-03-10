//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Default property group description</summary>
    public static class DefaultPropertyGrouping
    {
        /// <summary>
        /// No group description</summary>
        public static GroupDescription None
        {
            get { return null; }
        }

        /// <summary>
        /// Category group description</summary>
        public static GroupDescription ByCategory
        {
            get { return new PropertyGroupDescription("Category", null, StringComparison.CurrentCultureIgnoreCase); }
        }
    }
}
