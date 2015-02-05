//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Dom
{
    /// <summary>
    /// Utility class used to adapt FrameworkElement DataContext using ATF IAdaptable pattern.
    /// This class overrides the DataContext property metadata for TElement, allowing it to coerce
    /// DataContext value changes and attempt adaptation.
    /// </summary>
    /// <example>In order to use this class, call Initialize() before the framework element is used,
    /// for example, in the static constructor:
    /// <code>
    ///     static MyUserControl()
    ///     {
    ///         DataContextAdapter&lt;MyUserControl, MyViewModel&gt;.Register();
    ///     }
    /// </code>
    /// </example>
    /// <typeparam name="Telement">Type of element</typeparam>
    /// <typeparam name="Tdata">Type to adapt data context to</typeparam>
    public static class DataContextAdapter<Telement, Tdata>
        where Telement : FrameworkElement
        where Tdata : class
    {
        /// <summary>
        /// Static constructor</summary>
        static DataContextAdapter()
        {
            FrameworkElement.DataContextProperty.OverrideMetadata(typeof(Telement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, null,
                    CoerceDataContextValue));
        }

        /// <summary>
        /// Function to force static initialization</summary>
        public static void Register()
        {
        }

        private static object CoerceDataContextValue(DependencyObject d, object baseValue)
        {
            if (!(baseValue is Tdata))
            {
                var adaptable = baseValue as IAdaptable;
                if (adaptable != null)
                {
                    Tdata adapted = adaptable.As<Tdata>();
                    if(adapted != null)
                        return adapted;
                }
            }
            return baseValue;
        }
    }
}
