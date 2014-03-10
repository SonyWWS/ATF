//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Menu definition</summary>
    public class MenuDef
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="menuTag">Unique menu ID</param>
        /// <param name="text">Menu's text displayed to user</param>
        /// <param name="description">Menu's description</param>
        public MenuDef(object menuTag, string text, string description)
        {
            MenuTag = menuTag;
            Text = text;
            Description = description;
        }

        /// <summary>
        /// Unique menu ID</summary>
        public readonly object MenuTag;
        /// <summary>
        /// Menu's text displayed to user</summary>
        public readonly string Text;
        /// <summary>
        /// Menu's description</summary>
        public readonly string Description;
    }
}
