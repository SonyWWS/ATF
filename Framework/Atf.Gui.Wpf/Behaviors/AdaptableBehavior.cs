//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Interactivity;

using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Base adaptable behavior class.
    /// On attached, attempts to get IAdaptableControl interface on AssociatedObject and register itself. 
    /// AssociatedObject is the object to which this behavior is attached.</summary>
    /// <typeparam name="T">DependencyObject, which is an object that participates in the dependency property system</typeparam>
    public abstract class AdaptableBehavior<T> : Behavior<T>
        where T : System.Windows.DependencyObject
    {
        /// <summary>
        /// Raises behavior Attached event and performs custom actions.
        /// Attempts to get IAdaptableControl interface on AssociatedObject and register itself.</summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            var adaptableControl = AssociatedObject as IAdaptableControl;
            if (adaptableControl != null)
            {
                adaptableControl.Attach(this);
            }
        }

        /// <summary>
        /// Raises behavior Detaching event and performs custom actions.
        /// Attempts to get IAdaptableControl interface on AssociatedObject and unregister itself.</summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            var adaptableControl = AssociatedObject as IAdaptableControl;
            if (adaptableControl != null)
            {
                adaptableControl.Detach(this);
            }
        }
    }
}
