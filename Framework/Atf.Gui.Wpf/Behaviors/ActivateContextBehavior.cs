//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to catch mouse down events and set ATF context registry active context to the current 
    /// data context of the element</summary>
    public class ActivateContextBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Handle Attached event</summary>
        protected override void OnAttached()
        {
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
        }

        void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            SetActiveContext();
        }

        void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetActiveContext();
        }

        private void SetActiveContext()
        {
            var context = AssociatedObject.SafeGetDataContext();
            if (context != null)
            {
                var service = Composer.Current.Container.GetExportedValueOrDefault<IContextRegistry>();
                if (service != null)
                {
                    service.ActiveContext = context;
                }
            }
        }
    }
}
