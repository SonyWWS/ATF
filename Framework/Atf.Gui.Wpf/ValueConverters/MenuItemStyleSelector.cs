//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// Selector for menu item styles</summary>
    public class MenuItemStyleSelector : StyleSelector
    {
        /// <summary>
        /// Returns menu item's container's style</summary>
        /// <param name="item">Menu item</param>
        /// <param name="container">Menu item's container</param>
        /// <returns>Menu item's style</returns>
        public override Style SelectStyle(object item, DependencyObject container)
        {
            object key = Resources.SubMenuItemStyleKey;

            if (item is ICommandItem)
                key = Resources.CommandMenuItemStyleKey;
            else if (item is Models.Separator)
                key = Resources.MenuSeparatorStyleKey;

            return Application.Current.FindResource(key) as Style;
        }    
    }
}
