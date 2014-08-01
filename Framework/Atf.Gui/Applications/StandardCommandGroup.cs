//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Standard command groups in menus and toolbars</summary>
    public enum StandardCommandGroup
    {
        /// <summary>
        /// Command group starting with FileNew command</summary>
        FileNew,

        /// <summary>
        /// Command group starting with FileSave command</summary>
        FileSave,

        /// <summary>
        /// Command group containing custom File commands</summary>
        FileOther,

        /// <summary>
        /// Command group starting with Print commands</summary>
        FilePrint,

        /// <summary>
        /// Command group containing recently used file commands</summary>
        FileRecentlyUsed,

        /// <summary>
        /// Command group starting with FileExit command</summary>
        FileExit,

        /// <summary>
        /// Command group starting with EditUndo command</summary>
        EditUndo,

        /// <summary>
        /// Command group starting with EditCut command</summary>
        EditCut,

        /// <summary>
        /// Command group starting with EditSelectAll command</summary>
        EditSelectAll,

        /// <summary>
        /// Command group starting with EditGroup command</summary>
        EditGroup,

        /// <summary>
        /// Command group containing custom edit commands</summary>
        EditOther,

        /// <summary>
        /// Command group containing preference editing commands</summary>
        EditPreferences,

        /// <summary>
        /// Command group containing View show commands</summary>
        ViewShow,

        /// <summary>
        /// Command group starting with ViewZoomIn command</summary>
        ViewZoomIn,

        /// <summary>
        /// Command group containing View commands to show/hide controls</summary>
        ViewControls,

        /// <summary>
        /// Command group containing View commands to switch camera types</summary>
        ViewCamera,

        /// <summary>
        /// Command group containing View commands to set, choose, and edit camera "bookmarks".
        /// These are camera views (position, orientation, look-at, projection mode) that
        /// the user can define so that they can be used later. They're stored in the level
        /// file.</summary>
        ViewCameraBookmarks,

        /// <summary>
        /// Command group containing Format commands for 2D/3D formatting</summary>
        FormatAlign,

        /// <summary>
        /// Command group containing window layout commands</summary>
        WindowLayout,

        /// <summary>
        /// Command group containing the user window layouts</summary>
        WindowLayoutItems,

        /// <summary>
        /// Command group containing commands to split windows</summary>
        WindowSplit,

        /// <summary>
        /// Command group containing Window commands to tile panes in the Dock Panel</summary>
        WindowTile,

        /// <summary>
        /// Command group containing Window commands to show documents</summary>
        WindowDocuments,

        /// <summary>
        /// Window menu items other than documents</summary>
        WindowGeneral,

        /// <summary>
        /// Command group containing Help commands to check for update and bug reporting</summary>
        HelpUpdate,

        /// <summary>
        /// Command group containing with HelpAbout command</summary>
        HelpAbout,

        /// <summary>
        /// Command group containing the User Interface Lock command</summary>
        UILayout,
    }
}
