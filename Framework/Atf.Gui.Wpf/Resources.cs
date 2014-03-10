//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Resource keys for styles and images</summary>
    public static class Resources
    {
        /// <summary>
        /// Static constructor</summary>
        static Resources()
        {
            Register();
        }

        /// <summary>
        /// Registers resource keys. When resources are accessed in XAML it is possible that ResourceUtil.Register
        /// will not have been called.
        /// Call this dummy function on application start to force static registration.</summary>
        public static void Register()
        {
            WpfResourceUtil.Register(typeof(Resources), ResourceDir);
        }

        private const string ResourceDir = "Resources/";
        
        /// <summary>
        /// Causes the resource utility to automatically load and merge Styles.xaml
        /// into the application resources</summary>
        [ResourceDictionaryResourceAttribute("Styles.xaml")]
        public static readonly string StylesDictionary;

        /// <summary>
        /// Causes the resource utility to automatically load and merge PropertyEditors.xaml
        /// into the application resources</summary>
        [ResourceDictionaryResourceAttribute("PropertyEditors.xaml")]
        public static readonly string PropertyEditorsDictionary;

        /// <summary>
        /// Resource key used in XAML files for the toolbar tray style</summary>
        public static readonly ResourceKey ToolBarTrayStyleKey 
            = new ComponentResourceKey(typeof(Resources), "ToolBarTrayStyle");

        /// <summary>
        /// Resource key used in XAML files for the toolbar style</summary>
        public static readonly ResourceKey ToolBarStyleKey
            = new ComponentResourceKey(typeof(Resources), "ToolBarStyle");

        /// <summary>
        /// Resource key used in XAML files for the toolbar button style</summary>
        public static readonly ResourceKey ToolBarButtonStyleKey
            = new ComponentResourceKey(typeof(Resources), "ToolBarButtonStyle");

        /// <summary>
        /// Resource key used in XAML files for the menu style</summary>
        public static readonly ResourceKey MenuStyleKey
            = new ComponentResourceKey(typeof(Resources), "MenuStyle");

        /// <summary>
        /// Resource key used in XAML files for the submenu item style</summary>
        public static readonly ResourceKey SubMenuItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "SubMenuItemStyle");

        /// <summary>
        /// Resource key used in XAML files for the command menu item style</summary>
        public static readonly ResourceKey CommandMenuItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "CommandMenuItemStyle");

        /// <summary>
        /// Resource key used in XAML files for the menu separator style</summary>
        public static readonly ResourceKey MenuSeparatorStyleKey
            = new ComponentResourceKey(typeof(Resources), "MenuSeparatorStyle");

        /// <summary>
        /// Resource key used in XAML files for the menu item image style</summary>
        public static readonly ResourceKey MenuItemImageStyleKey
            = new ComponentResourceKey(typeof(Resources), "MenuItemImageStyle");

        /// <summary>
        /// Resource key used in XAML files for the status bar style</summary>
        public static readonly ResourceKey StatusBarStyleKey
            = new ComponentResourceKey(typeof(Resources), "StatusBarStyle");

        /// <summary>
        /// Resource key used in XAML files for the status bar item style</summary>
        public static readonly ResourceKey StatusBarItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "StatusBarItemStyle");

        /// <summary>
        /// Resource key used in XAML files for the auto grey style</summary>
        public static readonly ResourceKey AutoGreyStyleKey
            = new ComponentResourceKey(typeof(Resources), "AutoGreyStyle");

        /// <summary>
        /// Resource key used in XAML files for the dialog button style</summary>
        public static readonly ResourceKey DialogButtonStyleKey
            = new ComponentResourceKey(typeof(Resources), "DialogButtonStyle");

        /// <summary>
        /// Resource key used in XAML files for the treeview style</summary>
        public static readonly ResourceKey TreeViewStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewStyle");

        /// <summary>
        /// Resource key used in XAML files for the tree view item style</summary>
        public static readonly ResourceKey TreeViewItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewItemStyle");

        /// <summary>
        /// Resource key used in XAML files for the default tree view item template</summary>
        public static readonly ResourceKey DefaultTreeViewItemTemplateKey
            = new ComponentResourceKey(typeof(Resources), "DefaultTreeViewItemTemplate");

        /// <summary>
        /// Resource key used in XAML files for the treeview state image style</summary>
        public static readonly ResourceKey TreeViewStateImageStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewStateImageStyle");

        /// <summary>
        /// Resource key used in XAML files for the treeview item expander style</summary>
        public static readonly ResourceKey TreeViewItemExpanderStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewItemExpanderStyle");

        /// <summary>
        /// Resource key used in XAML files for the treeview image style</summary>
        public static readonly ResourceKey TreeViewImageStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewImageStyle");

        /// <summary>
        /// Resource key used in XAML files for the treeview label text block style</summary>
        public static readonly ResourceKey TreeViewLabelTextBlockStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewLabelTextBlockStyle");

        /// <summary>
        /// Resource key used in XAML files for the radio button list style</summary>
        public static readonly ResourceKey RadioButtonListStyleKey
            = new ComponentResourceKey(typeof(Resources), "RadioButtonListStyle");

        /// <summary>
        /// Resource key used in XAML files for the error image</summary>
        [WpfImageResource("error.ico")]
        public static readonly ResourceKey ErrorImageKey;

        /// <summary>
        /// Resource key used in XAML files for the info image</summary>
        [WpfImageResource("info.ico")]
        public static readonly ResourceKey InfoImageKey;

        /// <summary>
        /// Resource key used in XAML files for the warning image</summary>
        [WpfImageResource("warning.ico")]
        public static readonly ResourceKey WarningImageKey;

        /// <summary>
        /// Resource key used in XAML files for the clear image</summary>
        [WpfImageResource("clear.bmp")]
        public static readonly ResourceKey ClearImageKey;

        /// <summary>
        /// Resource key used in XAML files for the alphabetical image</summary>
        [WpfImageResource("Alphabetical.png")]
        public static readonly ResourceKey AlphabeticalImageKey;

        /// <summary>
        /// Resource key used in XAML files for the by category image</summary>
        [WpfImageResource("ByCategory.png")]
        public static readonly ResourceKey ByCategoryImageKey;
    }
}
