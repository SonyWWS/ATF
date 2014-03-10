//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Container for standard menu definitions</summary>
    public static class StandardMenus
    {
        /// <summary>
        /// Standard File menu</summary>
        public static MenuDef File =
            new MenuDef(StandardMenu.File, "_File".Localize(), "File Commands".Localize());

        /// <summary>
        /// Standard Edit menu</summary>
        public static MenuDef Edit =
            new MenuDef(StandardMenu.Edit, "_Edit".Localize(), "Editing Commands".Localize());

        /// <summary>
        /// Standard View menu</summary>
        public static MenuDef View =
            new MenuDef(StandardMenu.View, "_View".Localize(), "View Commands".Localize());

        /// <summary>
        /// Standard Modify menu</summary>
        public static MenuDef Modify =
            new MenuDef(StandardMenu.Modify, "Modify".Localize(), "Modify Commands".Localize());

        /// <summary>
        /// Standard Format menu</summary>
        public static MenuDef Format =
            new MenuDef(StandardMenu.Format, "_Format".Localize(), "Formatting Commands".Localize());

        /// <summary>
        /// Standard Window menu</summary>
        public static MenuDef Window =
            new MenuDef(StandardMenu.Window, "_Window".Localize(), "Window Management Commands".Localize());

        /// <summary>
        /// Standard Help menu</summary>
        public static MenuDef Help =
            new MenuDef(StandardMenu.Help, "_Help".Localize(), "Help Commands".Localize());

    }
}
