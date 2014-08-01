//Sony Computer Entertainment Confidential

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
            ResourceUtil.RegistrationStarted = true;
            ResourceUtil.Register(typeof(Sce.Atf.Resources), "Resources/");
            ResourceUtil.Register(typeof(Resources), "Resources/");
        }

        /// <summary>
        /// Call this dummy function during your application's start to force the static constructor to
        /// register the resources in this class.</summary>
        public static void Register()
        {
        }

        /// <summary>
        /// Causes the resource utility to automatically load and merge Styles.xaml
        /// into the application resources</summary>
        [ResourceDictionaryResourceAttribute("Styles.xaml")]
        public static readonly string StylesDictionary = null;

        /// <summary>
        /// Causes the resource utility to automatically load and merge PropertyEditors.xaml
        /// into the application resources</summary>
        [ResourceDictionaryResourceAttribute("PropertyEditors.xaml")]
        public static readonly string PropertyEditorsDictionary = null;

        [ImageResource("dialog_error.xaml")]
        public static readonly ResourceKey DialogErrorImageKey;
        [ImageResource("dialog_information.xaml")]
        public static readonly ResourceKey DialogInformationImageKey;
        [ImageResource("dialog_question.xaml")]
        public static readonly ResourceKey DialogQuestionImageKey;
        [ImageResource("dialog_warning.xaml")]
        public static readonly ResourceKey DialogWarningImageKey;

        public static readonly ResourceKey DialogRootBorderStyleKey
            = new ComponentResourceKey(typeof(Resources), "DialogRootBorderStyle");

        public static readonly ResourceKey SwitchToDialogKey
            = new ComponentResourceKey(typeof(Resources), "SwitchToDialog");

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
        /// Resource key used in XAML files for the toolbar item style</summary>
        public static readonly ResourceKey ToolBarItemTemplateKey
            = new ComponentResourceKey(typeof(Resources), "ToolBarItemTemplate");

        /// <summary>
        /// Resource key used in XAML files for the menu style</summary>
        public static readonly ResourceKey MenuStyleKey
            = new ComponentResourceKey(typeof(Resources), "MenuStyle");

        /// <summary>
        ///  Resource key used in XAML files for the context menu style</summary>
        public static readonly ResourceKey ContextMenuStyleKey
            = new ComponentResourceKey(typeof(Resources), "ContextMenuStyle");

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
        /// Resource key used in XAML files for the list view item style</summary>
        public static readonly ResourceKey ListViewItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "ListViewItemStyle");

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
        /// Resource key used in XAML files for the treeview icon style</summary>
        public static readonly ResourceKey TreeViewIconStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewIconStyle");

        /// <summary>
        /// Resource key used in XAML files for the treeview label text block style</summary>
        public static readonly ResourceKey TreeViewLabelTextBlockStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeViewLabelTextBlockStyle");

        /// <summary>
        /// Resource key used in XAML files for the tree list view style</summary>
        public static readonly ResourceKey TreeListViewStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeListViewStyle");

        /// <summary>
        /// Resource key used in XAML files for the tree list view item style</summary>
        public static readonly ResourceKey TreeListViewItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "TreeListViewItemStyle");

        /// <summary>
        /// Resource key used in XAML files for the tile view style</summary>
        public static readonly ResourceKey TileViewStyleKey
            = new ComponentResourceKey(typeof(Resources), "TileView");

        /// <summary>
        /// Resource key used in XAML files for the tile view item style</summary>
        public static readonly ResourceKey TileViewItemStyleKey
            = new ComponentResourceKey(typeof(Resources), "TileViewItem");

        /// <summary>
        /// Resource key used in XAML files for the radio button list style</summary>
        public static readonly ResourceKey RadioButtonListStyleKey
            = new ComponentResourceKey(typeof(Resources), "RadioButtonListStyle");

        /// <summary>
        /// Resource key used in XAML files for the error image</summary>
        [ImageResource("error.ico")]
        public static readonly ResourceKey ErrorImageKey = null;

        /// <summary>
        /// Resource key used in XAML files for the info image</summary>
        [ImageResource("info.ico")]
        public static readonly ResourceKey InfoImageKey = null;

        /// <summary>
        /// Resource key used in XAML files for the warning image</summary>
        [ImageResource("warning.ico")]
        public static readonly ResourceKey WarningImageKey = null;

        /// <summary>
        /// Resource key used in XAML files for the clear image</summary>
        [ImageResource("clear.bmp")]
        public static readonly ResourceKey ClearImageKey = null;

        /// <summary>
        /// Resource key used in XAML files for the alphabetical image</summary>
        [ImageResource("Alphabetical.png")]
        public static readonly ResourceKey AlphabeticalImageKey = null;

        /// <summary>
        /// Resource key used in XAML files for the by category image</summary>
        [ImageResource("ByCategory.png")]
        public static readonly ResourceKey ByCategoryImageKey = null;
    }
}
