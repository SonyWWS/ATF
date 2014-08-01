//Sony Computer Entertainment Confidential

using System;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Attribute used to set default column attributes for property editing</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CustomizeAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="columnWidth">Column width</param>
        public CustomizeAttribute(string propertyName, int columnWidth)
        {
            PropertyName = propertyName;
            ColumnWidth = columnWidth;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="propertyName">Property name</param>
        /// <param name="columnWidth">Column width</param>
        /// <param name="disableSort">Whether or not to disable column sorting</param>
        /// <param name="disableDragging">Whether or not to disable column dragging</param>
        /// <param name="disableResize">Whether or not to disable column resizing</param>
        /// <param name="disableEditing">Whether or not to disable column editing</param>
        /// <param name="hideDisplayName">Whether or not to hide the column name</param>
        public CustomizeAttribute(string propertyName, int columnWidth, bool disableSort, bool disableDragging,
            bool disableResize, bool disableEditing, bool hideDisplayName)
            : this(propertyName, columnWidth)
        {
            DisableSort = disableSort;
            DisableDragging = disableDragging;
            DisableResize = disableResize;
            DisableEditing = disableEditing;
            HideDisplayName = hideDisplayName;
        }

        /// <summary>
        /// The default column width. This can be overridden by the persisted settings.</summary>
        public readonly int ColumnWidth;

        /// <summary>
        /// The disable sort option. This can be overridden by the persisted settings.</summary>
        public readonly bool DisableSort;

        /// <summary>
        /// The disable dragging option. This can be overridden by the persisted settings.</summary>
        public readonly bool DisableDragging;

        /// <summary>
        /// The disable resize option. This can be overridden by the persisted settings.</summary>
        public readonly bool DisableResize;

        /// <summary>
        /// The disable editing option. This is different from read only. A property can be read only, but still present an edit
        /// control for the user to interact with. For instance, a read only string may present a read only text edit box to allow copying, but not pasting.
        /// This option is used when no edit control is desired. This can be overridden by the persisted settings.</summary>
        public readonly bool DisableEditing;

        /// <summary>
        /// Flag to disable rendering of the display name. This can be overridden by the persisted settings.</summary>
        public readonly bool HideDisplayName;

        /// <summary>
        /// The property's name (not the display name) and is equivalent, for example,
        /// to System.ComponentModel.MemberDescriptor's Name property</summary>
        public readonly string PropertyName;
    }
}