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
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(container);

            if (item is ICommandItem)
            {
                return ic.FindResource(Resources.CommandMenuItemStyleKey) as Style;
            }
            else if (item is Sce.Atf.Wpf.Models.Separator)
            {
                return ic.FindResource(Resources.MenuSeparatorStyleKey) as Style;
            }

            return ic.FindResource(Resources.SubMenuItemStyleKey) as Style;
        }    
    }
}
