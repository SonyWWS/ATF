//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Every control that can host other dockable layouts or contents implements this
    /// </summary>
    public interface IDockLayout
    {
        /// <summary>
        /// Will return the Root of the hierarchy
        /// </summary>
        DockPanel Root { get; }

        /// <summary>
        /// Will hit test the position and return content at that position
        /// </summary>
        /// <param name="position">Position to test</param>
        /// <returns>Content if hit, null otherwise</returns>
        DockContent HitTest(Point position);
        
        /// <summary>
        /// Will check if the layout contains the content as direct child
        /// </summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True if the content is child of this control, false otherwise</returns>
        bool HasChild(IDockContent content);
        
        /// <summary>
        /// Will check if the layout contains the content as child or descendent
        /// </summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True if content is child or descendent, false otherwise</returns>
        bool HasDescendant(IDockContent content);
        
        /// <summary>
        /// Will dock the new content next to content.
        /// </summary>
        /// <param name="nextTo">Next to what content to dock the new content</param>
        /// <param name="newContent">New content that has to be docked</param>
        /// <param name="dockTo">To wchich side of the nextTo content should the new content be docked</param>
        void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo);
        
        /// <summary>
        /// Undocks given content
        /// </summary>
        /// <param name="content">Content to undock</param>
        void Undock(IDockContent content);
        
        /// <summary>
        /// Undocks given child layout
        /// </summary>
        /// <param name="child">Child layout to undock</param>
        void Undock(IDockLayout child);
        
        /// <summary>
        /// Will replace the old layout with new layout child
        /// </summary>
        /// <param name="oldLayout">Old layout to be replaced</param>
        /// <param name="newLayout">New layout to replace with</param>
        void Replace(IDockLayout oldLayout, IDockLayout newLayout);
        
        /// <summary>
        /// Close the layout.
        /// </summary>
        void Close();

        /// <summary>
        /// Returns the content's parent as an IDockLayout
        /// </summary>
        /// <param name="content">The docked content whose parent is requested</param>
        /// <returns>The parent as IDockLayout</returns>
        IDockLayout FindParentLayout(IDockContent content);
    }
}
