//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Base class for editing a value with a control</summary>
    public abstract class ValueEditor : DependencyObject
    {
        /// <summary>
        /// Gets whether this editor uses a custom context</summary>
        public virtual bool UsesCustomContext
        {
            get { return false; }
        }

        /// <summary>
        /// Gets custom context for PropertyNode</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns>Custom context for editor</returns>
        public virtual object GetCustomContext(PropertyNode node)
        {
            return null;
        }

        /// <summary>
        /// Can this editor edit a given PropertyNode?</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns>True iff editor can edit given PropertyNode</returns>
        public virtual bool CanEdit(PropertyNode node)
        {
            return true;
        }

        /// <summary>
        /// Gets style for PropertyNode</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns>Style for PropertyNode</returns>
        public virtual Style GetStyle(PropertyNode node)
        {
            return null;
        }

        /// <summary>
        /// Gets DataTemplate for PropertyNode</summary>
        /// <param name="node">PropertyNode</param>
        /// <returns>DataTemplate for PropertyNode</returns>
        public abstract DataTemplate GetTemplate(PropertyNode node);

    }
}
