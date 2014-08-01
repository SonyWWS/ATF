//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// WPF ListView misbehaves when applying our custom styles (an intentional feature!):
    /// http://social.msdn.microsoft.com/Forums/en/wpf/thread/9cff5483-9608-4f33-98f3-a186de4fa306
    /// 
    /// The actual issue appears when applying an ItemContainerStyle override, and will cause some ListViewItems to 
    /// have the style applied and others not.
    /// </summary>
    public class StylableListView : ListView
    {
        /// <summary>
        /// Set the style for the ListViewItem</summary>
        /// <param name="element">The ListViewItem to style</param>
        /// <param name="item">The object used to create the ListViewItem</param>
        protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var listViewItem = element as ListViewItem;
            if (listViewItem != null && View != null && View is GridView)
            {
                element.SetValue(
                    StyleProperty,
                    TryFindResource(GridView.GridViewItemContainerStyleKey));
            }
        }
    }
}
