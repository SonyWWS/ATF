//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Every control that can host other dockable layouts or contents implements this interface.</summary>
    public interface IDockLayout
    {
        /// <summary>
        /// Get the root of the hierarchy</summary>
        DockPanel Root { get; }

        /// <summary>
        /// Hit test the position and return content at that position</summary>
        /// <param name="position">Position to test</param>
        /// <returns>Content if hit, null otherwise</returns>
        DockContent HitTest(Point position);
        
        /// <summary>
        /// Check if the layout contains the content as direct child</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff the content is child of this control</returns>
        bool HasChild(IDockContent content);
        
        /// <summary>
        /// Check if the layout contains the content as child or descendant</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff content is child or descendant</returns>
        bool HasDescendant(IDockContent content);
        
        /// <summary>
        /// Dock the new content next to content</summary>
        /// <param name="nextTo">Dock content to add new content next to</param>
        /// <param name="newContent">New content to be docked</param>
        /// <param name="dockTo">Side of nextTo content where new content should be docked</param>
        void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo);
        
        /// <summary>
        /// Undock given content</summary>
        /// <param name="content">Content to undock</param>
        void Undock(IDockContent content);
        
        /// <summary>
        /// Undock given child layout</summary>
        /// <param name="child">Child layout to undock</param>
        void Undock(IDockLayout child);
        
        /// <summary>
        /// Replace the old layout with new layout child</summary>
        /// <param name="oldLayout">Old layout to be replaced</param>
        /// <param name="newLayout">New layout that replaces old layout</param>
        void Replace(IDockLayout oldLayout, IDockLayout newLayout);
        
        /// <summary>
        /// Close the layout</summary>
        void Close();

        /// <summary>
        /// Return the content's parent as an IDockLayout</summary>
        /// <param name="content">The docked content whose parent is requested</param>
        /// <returns>The parent as IDockLayout</returns>
        IDockLayout FindParentLayout(IDockContent content);
    }
}
