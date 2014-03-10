//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Input;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Command definition</summary>
    public class CommandDef
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandTag">Unique command ID</param>
        public CommandDef(object commandTag)
        {
            Requires.NotNull(commandTag, "commandTag");
            CommandTag = commandTag;
        }

        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="commandTag">Command ID</param>
        /// <param name="menuTag">Unique ID for menu command attached to</param>
        /// <param name="groupTag">Unique ID for command's group</param>
        /// <param name="text">User visible command text, as on menu item</param>
        /// <param name="description">Command description</param>
        public CommandDef(
            object commandTag,
            object menuTag,
            object groupTag,
            string text,
            string description)
            : this(commandTag)
        {
            MenuTag = menuTag;
            GroupTag = groupTag;
            Text = text;
            Description = description;
        }

        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="commandTag">Command ID</param>
        /// <param name="menuTag">Unique ID for menu command attached to</param>
        /// <param name="groupTag">Unique ID for command's group</param>
        /// <param name="text">User visible command text, as on menu item</param>
        /// <param name="menuPath">String array describing menu path</param>
        /// <param name="description">Command description</param>
        /// <param name="imageSourceKey">Image resource for command</param>
        /// <param name="inputGestures">Sequence of input device gestures to execute command</param>
        /// <param name="visibility">Flags indicating where command is visible: on toolbar, menus, etc.</param>
        public CommandDef(
            object commandTag,
            object menuTag,
            object groupTag,
            string text,
            string[] menuPath,
            string description,
            object imageSourceKey,
            InputGesture[] inputGestures,
            CommandVisibility visibility)
            : this(commandTag, menuTag, groupTag, text, description)
        {

            ImageSourceKey = imageSourceKey;
            if(menuPath != null)
                MenuPath = menuPath;
            if (inputGestures != null)
                InputGestures = inputGestures;
            
            Visibility = visibility;
        }

        /// <summary>
        /// Command ID</summary>
        public readonly object CommandTag;
        /// <summary>
        /// ID for menu command attached to</summary>
        public readonly object MenuTag;
        /// <summary>
        /// ID for command's group</summary>
        public readonly object GroupTag;
        /// <summary>
        /// User visible command text, as on menu item</summary>
        public readonly string Text;
        /// <summary>
        /// String array describing menu path</summary>
        public readonly string[] MenuPath = EmptyArray<string>.Instance;
        /// <summary>
        /// Command description</summary>
        public readonly string Description;
        /// <summary>
        /// Image resource for command</summary>
        public readonly object ImageSourceKey;
        /// <summary>
        /// Sequence of input device gestures to execute command</summary>
        public readonly InputGesture[] InputGestures = EmptyArray<InputGesture>.Instance;
        /// <summary>
        /// Flags indicating where command is visible: on toolbar, menus, etc.</summary>
        public readonly CommandVisibility Visibility = CommandVisibility.Menu;
    }
}
