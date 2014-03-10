//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Container for standard menu information</summary>
    public class MenuInfo
    {
        /// <summary>
        /// Unique menu tag identifying the menu</summary>
        public readonly object MenuTag;

        /// <summary>
        /// Menu text displayed to user</summary>
        public readonly string MenuText;

        /// <summary>
        /// Menu description</summary>
        public readonly string Description;

        /// <summary>
        /// Standard File menu</summary>
        public static MenuInfo File =
            new MenuInfo(StandardMenu.File, "_File".Localize(), "File Commands".Localize());

        /// <summary>
        /// Standard Edit menu</summary>
        public static MenuInfo Edit =
            new MenuInfo(StandardMenu.Edit, "_Edit".Localize(), "Editing Commands".Localize());

        /// <summary>
        /// Standard View menu</summary>
        public static MenuInfo View =
            new MenuInfo(StandardMenu.View, "_View".Localize(), "View Commands".Localize());

        /// <summary>
        /// Standard Modify menu</summary>
        public static MenuInfo Modify =
            new MenuInfo(StandardMenu.Modify, "Modify".Localize(), "Modify Commands".Localize());

        /// <summary>
        /// Standard Format menu</summary>
        public static MenuInfo Format =
            new MenuInfo(StandardMenu.Format, "_Format".Localize(), "Formatting Commands".Localize());

        /// <summary>
        /// Standard Window menu</summary>
        public static MenuInfo Window =
            new MenuInfo(StandardMenu.Window, "_Window".Localize(), "Window Management Commands".Localize());

        /// <summary>
        /// Standard Help menu</summary>
        public static MenuInfo Help =
            new MenuInfo(StandardMenu.Help, "_Help".Localize(), "Help Commands".Localize());

        internal MenuInfo(
            object menuTag,
            string menuText,
            string description)
        {
            Requires.NotNull(menuTag, "menuTag");

            MenuTag = menuTag;
            MenuText = menuText;
            Description = description;
        }
    }
}
