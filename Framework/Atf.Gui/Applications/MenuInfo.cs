//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Container for menu information</summary>
    public class MenuInfo
    {
        /// <summary>
        /// Unique menu tag, identifying the menu</summary>
        public readonly object MenuTag;

        /// <summary>
        /// Menu text</summary>
        public readonly string MenuText;

        /// <summary>
        /// Menu description</summary>
        public readonly string Description;

        /// <summary>
        /// Gets or sets the ICommandService to which this menu info is registered</summary>
        public ICommandService CommandService 
        { 
            get { return m_commandService; } 
            set
            {
                if (m_commandService != null)
                    throw new InvalidOperationException("MenuInfo already has been registered");
                m_commandService = value;
            }
        }

        /// <summary>
        /// Standard File Menu</summary>
        public static MenuInfo File =
            new MenuInfo(StandardMenu.File, "File".Localize("this is the name of a menu"), "File Commands".Localize());

        /// <summary>
        /// Standard Edit Menu</summary>
        public static MenuInfo Edit =
            new MenuInfo(StandardMenu.Edit, "Edit".Localize("this is the name of a menu"), "Editing Commands".Localize());

        /// <summary>
        /// Standard View Menu</summary>
        public static MenuInfo View =
            new MenuInfo(StandardMenu.View, "View".Localize("this is the name of a menu"),
                "View Commands".Localize("'View' is a noun. This text is a description of the View menu"));

        /// <summary>
        /// Standard Modify Menu</summary>
        public static MenuInfo Modify =
            new MenuInfo(StandardMenu.Modify, "Modify".Localize("this is the name of a menu"), "Modify Commands".Localize());

        /// <summary>
        /// Standard Format Menu</summary>
        public static MenuInfo Format =
            new MenuInfo(StandardMenu.Format, "Format".Localize("this is the name of a menu"), "Formatting Commands".Localize());

        /// <summary>
        /// Standard Window Menu</summary>
        public static MenuInfo Window =
            new MenuInfo(StandardMenu.Window, "Window".Localize("this is the name of a menu"), "Window Management Commands".Localize());

        /// <summary>
        /// Standard Help Menu</summary>
        public static MenuInfo Help =
            new MenuInfo(StandardMenu.Help, "Help".Localize("this is the name of a menu"), "Help Commands".Localize());

        public MenuInfo(
            object menuTag,
            string menuText,
            string description)
        {
            MenuTag = menuTag;
            MenuText = menuText;
            Description = description;
        }

        /// <summary>
        /// Number of commands in this menu</summary>
        public int Commands;

        private ICommandService m_commandService;
    }
}
