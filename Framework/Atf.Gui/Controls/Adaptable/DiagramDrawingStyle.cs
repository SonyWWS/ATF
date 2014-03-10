//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Styles for drawing diagram objects</summary>
    public enum DiagramDrawingStyle
    {
        /// <summary>
        /// Drawing style for normal objects</summary>
        Normal,

        /// <summary>
        /// Drawing style for selected objects</summary>
        Selected,

        /// <summary>
        /// Drawing style for the last selected object</summary>
        LastSelected,

        /// <summary>
        /// Drawing style for hot objects; Hot is used to draw objects under the
        /// cursor</summary>
        Hot,

        /// <summary>
        /// Drawing style for objects responsible for the initiation of the Drag and Drop operation.</summary>
        DragSource,

        /// <summary>
        /// Drawing style for objects that wishes to accept drops during Drag and Drop operations.</summary> 
        DropTarget,

        /// <summary>
        /// Drawing style for ghosted objects; Ghosted is used to draw objects in
        /// their old state while they are being edited</summary>
        Ghosted,

        /// <summary>
        /// Drawing style for hidden objects; Hidden is used to draw objects that
        /// are hidden and not editable</summary>
        Hidden,

        /// <summary>
        /// Drawing style for a reference instance of a template; changes to a template  will affect
        /// all the templated instances that reference it</summary>
        TemplatedInstance,

        /// <summary>
        /// Drawing style for copy instances of templates; These copies only affect themselves</summary>
        CopyInstance,

        /// <summary>
        /// Drawing style for invalid objects; Error is used for objects that
        /// are in an invalid position or state</summary>
        Error,

        /// <summary>
        /// No style defined</summary>
        None,

        /// <summary>
        /// Unknown style</summary>
        Unknown
    }
}
