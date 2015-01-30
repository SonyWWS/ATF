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
        /// <param name="columnWidth">Column width. 0 means use the global width.</param>
        /// <param name="disableSort">Whether or not to disable column sorting</param>
        /// <param name="disableDragging">Whether or not to disable column dragging</param>
        /// <param name="disableResize">Whether or not to disable column resizing</param>
        /// <param name="disableEditing">Whether or not to disable column editing</param>
        /// <param name="hideDisplayName">Whether or not to hide the column name</param>
        /// <param name="horizontalEditorOffset">The number of pixels that the editing control or value
        /// is shifted to the right of the start of the row, in the 2-column property editor. A
        /// negative number means "use the default" which is to use the user-adjustable
        /// splitter between the name and the value columns.</param>
        /// <param name="nameHasWholeRow">Whether or not the name of the property is given the
        /// whole row in the 2-column property editor</param>
        public CustomizeAttribute(string propertyName, int columnWidth = 0, bool disableSort = false,
            bool disableDragging = false, bool disableResize = false, bool disableEditing = false,
            bool hideDisplayName = false, int horizontalEditorOffset = -1, bool nameHasWholeRow = false)
        {
            PropertyName = propertyName;
            ColumnWidth = columnWidth;
            DisableSort = disableSort;
            DisableDragging = disableDragging;
            DisableResize = disableResize;
            DisableEditing = disableEditing;
            HideDisplayName = hideDisplayName;
            HorizontalEditorOffset = horizontalEditorOffset;
            NameHasWholeRow = nameHasWholeRow;
        }

        /// <summary>
        /// The default column width. This can be overridden by the persisted settings.</summary>
        public readonly int ColumnWidth;

        /// <summary>
        /// The number of pixels that the editing control or value
        /// is shifted to the right of the start of the row, in the 2-column property editor. A
        /// negative number means "use the default" which is to use the user-adjustable
        /// splitter between the name and the value columns. Default is -1.</summary>
        public readonly int HorizontalEditorOffset = -1;

        /// <summary>
        /// Whether or not the name of the property is given the whole row in the 2-column
        /// property editor. Default is false.</summary>
        /// <remarks>If HorizontalEditorOffset is a small positive number, then it may be
        /// useful to set this property to 'true' so that the property name can be fully
        /// displayed. If 'true', the 2-column property editor will take more vertical space
        /// but can save a lot of horizontal space when HorizontalEditorOffset is used.</remarks>
        public readonly bool NameHasWholeRow;

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