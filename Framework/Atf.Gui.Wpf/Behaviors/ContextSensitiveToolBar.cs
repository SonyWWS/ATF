//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Toolbar behavior to display context sensitive information based on the current selection</summary>
    public class ContextSensitiveToolBarBehavior : Behavior<ToolBarTray>
    {
        /// <summary>
        /// Dependency property for context sensitive toolbar to display</summary>
        public static readonly DependencyProperty ResourceKeyProperty =
            DependencyProperty.Register("ResourceKey",
                                        typeof(object), typeof(ContextSensitiveToolBarBehavior),
                                        new PropertyMetadata(default(object),
                                                             (d, e) => ((ContextSensitiveToolBarBehavior)d).OnResourceKeyChanged(e)));

        /// <summary>
        /// Gets and sets the ResourceKeyProperty dependency property</summary>
        public object ResourceKey
        {
            get { return GetValue(ResourceKeyProperty); }
            set { SetValue(ResourceKeyProperty, value); }
        }

        private void OnResourceKeyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject == null)
                return;

            if (m_toolBar != null)
            {
                AssociatedObject.ToolBars.Remove(m_toolBar);
                m_toolBar = null;
            }

            if (e.NewValue != null)
            {
                var toolBar = AssociatedObject.TryFindResource(e.NewValue) as ToolBar;
                if (toolBar != null)
                {
                    AssociatedObject.ToolBars.Add(toolBar);
                    m_toolBar = toolBar;
                }
            }
        }

        private ToolBar m_toolBar;
    }
}