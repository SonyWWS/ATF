//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Comparer for sorting commands by menu, group, and command tags</summary>
    public class CommandComparer : IComparer<ICommandItem>, IComparer<IMenuItem>
    {
        /// <summary>
        /// Static constructor</summary>
        static CommandComparer()
        {
            // force standard and framework items into their places at beginning or end
            m_endingTags.Add(StandardCommand.FileClose);

            m_beginningTags.Add(StandardCommand.FileSave);
            m_beginningTags.Add(StandardCommand.FileSaveAs);
            m_beginningTags.Add(StandardCommand.FileSaveAll);

            m_beginningTags.Add(StandardCommand.EditUndo);
            m_beginningTags.Add(StandardCommand.EditRedo);

            m_beginningTags.Add(StandardCommand.EditCut);
            m_beginningTags.Add(StandardCommand.EditCopy);
            m_beginningTags.Add(StandardCommand.EditPaste);
            m_endingTags.Add(StandardCommand.EditDelete);

            m_beginningTags.Add(StandardCommand.EditSelectAll);
            m_beginningTags.Add(StandardCommand.EditDeselectAll);
            m_beginningTags.Add(StandardCommand.EditInvertSelection);

            m_beginningTags.Add(StandardCommand.EditGroup);
            m_beginningTags.Add(StandardCommand.EditUngroup);
            m_beginningTags.Add(StandardCommand.EditLock);
            m_beginningTags.Add(StandardCommand.EditUnlock);

            m_beginningTags.Add(StandardCommand.ViewZoomIn);
            m_beginningTags.Add(StandardCommand.ViewZoomOut);
            m_beginningTags.Add(StandardCommand.ViewZoomExtents);

            m_beginningTags.Add(StandardCommand.WindowSplitHoriz);
            m_beginningTags.Add(StandardCommand.WindowSplitVert);
            m_beginningTags.Add(StandardCommand.WindowRemoveSplit);

            m_endingTags.Add(StandardCommand.HelpAbout);

            m_beginningTags.Add(StandardCommandGroup.FileNew);
            m_beginningTags.Add(StandardCommandGroup.FileSave);
            m_beginningTags.Add(StandardCommandGroup.FileOther);
            m_endingTags.Add(StandardCommandGroup.FileRecentlyUsed);
            m_endingTags.Add(StandardCommandGroup.FileExit);

            m_beginningTags.Add(StandardCommandGroup.EditUndo);
            m_beginningTags.Add(StandardCommandGroup.EditCut);
            m_beginningTags.Add(StandardCommandGroup.EditSelectAll);
            m_beginningTags.Add(StandardCommandGroup.EditGroup);
            m_beginningTags.Add(StandardCommandGroup.EditOther);
            m_endingTags.Add(StandardCommandGroup.EditPreferences);

            m_beginningTags.Add(StandardCommandGroup.ViewZoomIn);
            m_beginningTags.Add(StandardCommandGroup.ViewControls);

            m_beginningTags.Add(StandardCommandGroup.WindowLayout);
            m_beginningTags.Add(StandardCommandGroup.WindowSplit);
            m_endingTags.Add(StandardCommandGroup.WindowDocuments);

            m_endingTags.Add(StandardCommandGroup.HelpAbout);

            m_beginningTags.Add(CommandId.FileRecentlyUsed1);
            m_beginningTags.Add(CommandId.FileRecentlyUsed2);
            m_beginningTags.Add(CommandId.FileRecentlyUsed3);
            m_beginningTags.Add(CommandId.FileRecentlyUsed4);

            m_endingTags.Add(StandardCommand.FileExit);

            m_endingTags.Add(CommandId.EditPreferences);
            m_endingTags.Add(CommandId.EditDocumentPreferences);

            m_beginningTags.Add(StandardMenu.File);
            m_beginningTags.Add(StandardMenu.Edit);
            m_beginningTags.Add(StandardMenu.Format);
            m_beginningTags.Add(StandardMenu.View);
            m_beginningTags.Add(StandardMenu.Modify);
            m_endingTags.Add(StandardMenu.Help);
            m_endingTags.Add(StandardMenu.Window);

            // Force subitems with the same menu and group to sort themselves by menu name, rather than creation index
            m_defaultSortByMenuLabel.Add(StandardCommandGroup.WindowDocuments);
        }

        #region IComparer<ICommandItem> Members

        /// <summary>
        /// Compares two ICommandItems and returns a value indicating whether one is less than, 
        /// equal to, or greater than the other</summary>
        /// <param name="x">ICommandItem 1</param>
        /// <param name="y">ICommandItem 2</param>
        /// <returns>A signed integer that indicates the relative values of the ICommandItems:
        /// Less than zero: ICommandItem 1 is less than ICommandItem 2.
        /// Zero: ICommandItem 1 equals ICommandItem 2. 
        /// Greater than zero: ICommandItem 1 is greater than ICommandItem 2.</returns>
        public int Compare(ICommandItem x, ICommandItem y)
        {
            return CompareCommands(x, y);
        }

        #endregion

        #region IComparer<IMenuItem> Members

        /// <summary>
        /// Compares two IMenuItems and returns a value indicating whether one is less than, 
        /// equal to, or greater than the other</summary>
        /// <param name="x">IMenuItem 1</param>
        /// <param name="y">IMenuItem 2</param>
        /// <returns>A signed integer that indicates the relative values of the IMenuItem:
        /// Less than zero: IMenuItem 1 is less than IMenuItem 2.
        /// Zero: IMenuItem 1 equals IMenuItem 2 . 
        /// Greater than zero: IMenuItem 1 is greater than IMenuItem 2.</returns>
        public int Compare(IMenuItem x, IMenuItem y)
        {
            // if either item is a separator then return zero
            if (x == null || y == null)
                return 0;

            var commandX = x as ICommandItem;
            var commandY = y as ICommandItem;

            // If both items are commands then do normal compare
            if(commandX != null && commandY != null)
                return CompareCommands(commandX, commandY);
            
            // If first item is command and second is a menu then
            // put menus first and vica versa
            if (commandX != null)
                return 1;

            if (commandY != null)
                return -1;


            return 0;

        }

        #endregion

        /// <summary>
        /// Returns whether two tags are equal; both must be null or the same object</summary>
        /// <param name="tag1">First object</param>
        /// <param name="tag2">Second object</param>
        /// <returns>True iff objects are equal</returns>
        public static bool TagsEqual(object tag1, object tag2)
        {
            if (tag1 == null)
                return tag2 == null;

            return tag1.Equals(tag2);
        }

        /// <summary>
        /// Compare two commands</summary>
        /// <param name="x">First command</param>
        /// <param name="y">Second command</param>
        /// <returns>Less than zero if x before y, 0 if commands identical, greater than zero if x after y</returns>
        public static int CompareCommands(ICommandItem x, ICommandItem y)
        {
            int result = CompareTags(x.MenuTag, y.MenuTag);
            if (result == 0)
                result = CompareTags(x.GroupTag, y.GroupTag);
            if (result == 0)
                result = CompareTags(x.CommandTag, y.CommandTag);
            // finally use either the displayed menu or text registration index to ensure a stable sort
            if (result == 0)
            {
                if (x.GroupTag != null && m_defaultSortByMenuLabel.Contains(x.GroupTag))
                    result = CompareTags(x.Text, y.Text);
                else
                    result = x.Index - y.Index;
            }

            return result;
        }

        /// <summary>
        /// Compare two tags</summary>
        /// <param name="tag1">First tag</param>
        /// <param name="tag2">Second tag</param>
        /// <returns>Less than zero: tag1 is before tag2.
        /// Zero: tag1 equals tag2. 
        /// Greater than zero: tag1 is after tag2.</returns>
        public static int CompareTags(object tag1, object tag2)
        {
            bool tag1First = false, tag2First = false, tag1Last = false, tag2Last = false;
            if (tag1 != null)
            {
                tag1First = m_beginningTags.Contains(tag1);
                tag1Last = m_endingTags.Contains(tag1);
            }
            if (tag2 != null)
            {
                tag2First = m_beginningTags.Contains(tag2);
                tag2Last = m_endingTags.Contains(tag2);
            }

            if (tag1First && !tag2First)
                return -1;

            if (tag2First && !tag1First)
                return 1;

            if (tag1Last && !tag2Last)
                return 1;

            if (tag2Last && !tag1Last)
                return -1;

            if (tag1 is Enum && tag2 is Enum)
                return ((int)tag1).CompareTo((int)tag2);

            if (tag1 is Enum)
                return -1;

            if (tag2 is Enum)
                return 1;

            if (tag1 is string && tag2 is string)
                return StringUtil.CompareNaturalOrder((string)tag1, (string)tag2);

            IComparable comparable1 = tag1 as IComparable;
            IComparable comparable2 = tag2 as IComparable;
            if (comparable1 != null)
            {
                int result = comparable1.CompareTo(tag2);
                if (result != 0)
                    return result;
            }
            if (comparable2 != null)
            {
                int result = comparable2.CompareTo(tag1);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        private static readonly HashSet<object> m_beginningTags = new HashSet<object>();
        private static readonly HashSet<object> m_endingTags = new HashSet<object>();
        private static readonly HashSet<object> m_defaultSortByMenuLabel = new HashSet<object>();
    }
}
