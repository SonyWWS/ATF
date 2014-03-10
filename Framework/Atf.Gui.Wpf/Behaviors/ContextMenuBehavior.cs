//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

using Sce.Atf.Wpf.Interop;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to provide hook for ATF context menu service.
    /// Right clicking on the associated element queries all context menu
    /// providers for commands using the element's data context as the query target.
    /// If commands exist, a context menu is displayed.</summary>
    public class ContextMenuBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Performs custom actions on behavior Attached event</summary>
        protected override void OnAttached()
        {
            AssociatedObject.MouseRightButtonUp += Element_MouseRightButtonUp;
        }

        private static void Element_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            object context = ((FrameworkElement)sender).DataContext;
            object clickedData = null;

            var originalSource = e.OriginalSource as FrameworkElement;
            if (originalSource != null)
                clickedData = originalSource.DataContext;

            if (context != null)
            {
                var service = Composer.Current.Container.GetExportedValueOrDefault<IContextMenuService>();
                if (service != null)
                {
                    var providers = Composer.Current.Container.GetExportedValues<Sce.Atf.Applications.IContextMenuCommandProvider>();

                    IEnumerable<object> commands =
                       Sce.Atf.Applications.ContextMenuCommandProvider.GetCommands(
                           providers,
                           context,
                           clickedData);

                    service.RunContextMenu(commands);
                }
            }
        }
    }
}
