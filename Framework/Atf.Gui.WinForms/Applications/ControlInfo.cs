//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class that holds information about controls hosted by IControlHostService
    /// implementations</summary>
    public class ControlInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Control name, which may be displayed as the title of a hosting
        /// control or form</param>
        /// <param name="description">Control description, which may be displayed as a tooltip
        /// when the user hovers over a hosting control or form</param>
        /// <param name="group">Desired initial control hosting group</param>
        public ControlInfo(string name, string description, StandardControlGroup group)
            : this(name, description, group, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Control name, which may be displayed as the title of a hosting
        /// control or form</param>
        /// <param name="description">Control description, which may be displayed as a tooltip
        /// when the user hovers over a hosting control or form</param>
        /// <param name="group">Desired initial control hosting group</param>
        /// <param name="image">Control image, which may be displayed on a hosting control
        /// or form</param>
        /// <param name="helpUrl">URL to open when the user presses F1 and this Control has focus.
        /// If null or the empty string, then this feature is disabled. It is better to use this
        /// parameter than to use the WebHelp object directly, so that the container in the docking
        /// framework will have a WebHelp object attached.</param>
        public ControlInfo(string name, string description, StandardControlGroup group, Image image,
            string helpUrl = null)
        {
            m_name = name;
            m_description = description;
            m_group = group;
            m_image = image;
            m_helpUrl = helpUrl;
        }

        /// <summary>
        /// Event that is raised before the info changes</summary>
        public event EventHandler Changing;

        /// <summary>
        /// Event that is raised after the info changes</summary>
        public event EventHandler Changed;

        /// <summary>
        /// Gets or sets the control's name, which may be displayed as the title of
        /// a hosting control or form</summary>
        public string Name
        {
            get { return m_name; }
            set
            {
                if (value != m_name)
                {
                    Changing.Raise(this, EventArgs.Empty);
                    m_name = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the control's description, which may be displayed as a tooltip
        /// when the user hovers over a hosting control or form</summary>
        public string Description
        {
            get { return m_description; }
            set
            {
                if (value != m_description)
                {
                    Changing.Raise(this, EventArgs.Empty);
                    m_description = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the desired initial control group</summary>
        public StandardControlGroup Group
        {
            get { return m_group; }
            set { m_group = value; }
        }

        /// <summary>
        /// Gets or sets the control's image, which may be displayed on a hosting control
        /// or form</summary>
        public Image Image
        {
            get { return m_image; }
            set
            {
                if (value != m_image)
                {
                    Changing.Raise(this, EventArgs.Empty);
                    m_image = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the client that registered this control</summary>
        /// <remarks>Should only be set by IControlHostService implementations</remarks>
        public IControlHostClient Client
        {
            get { return m_client; }
            set { m_client = value; }
        }

        /// <summary>
        /// Gets or sets the control that was registered with this info</summary>
        /// <remarks>Should only be set by IControlHostService implementations</remarks>
        public Control Control
        {
            get { return m_control; }
            set { m_control = value; }
        }

        /// <summary>
        /// Gets or sets the control that is currently hosting the client control. This
        /// can be used to subscribe to drag and drop, mouse clicks, and other events.</summary>
        /// <remarks>Should only be set by IControlHostService implementations</remarks>
        public Control HostControl
        {
            get { return m_hostControl; }
            set { m_hostControl = value; }
        }

        /// <summary>
        /// Gets or sets the original Dock property of the Control prior to IControlHostService's
        /// RegisterControl setting it to DockStyle.Fill</summary>
        /// <remarks>Should only be used by IControlHostService implementations</remarks>
        public DockStyle OriginalDock
        {
            get { return m_originalDock; }
            set { m_originalDock = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the control is hosted in the active hosting
        /// panel</summary>
        /// <remarks>Should only be set by IControlHostService implementations</remarks>
        public bool InActiveGroup
        {
            get { return m_inActiveGroup; }
            set { m_inActiveGroup = value; }
        }

        /// <summary>
        /// Gets or sets a value determining if a menu command is registered to show/hide 
        /// this control</summary>
        /// <remarks>The default value is true.</remarks>
        public bool ShowInMenu
        {
            get { return m_showInMenu; }
            set { m_showInMenu = value; }
        }

        /// <summary>
        /// If this nullable has a value, then that value will determine the behavior when the user
        /// clicks on the 'X' to close the Control. If Value is true, the Control will be unregistered
        /// and the corresponding menu item will be removed. If Value is false, then the Control will
        /// be hidden and the corresponding menu item will be unchecked.
        /// If this nullable does not have a value (the default) then when a Form/Control is closed
        /// because the user clicked on the 'X' button, the Control is unregistered if the Control is
        /// in the center group (either StandardControlGroup.Center or StandardControlGroup.CenterPermanent),
        /// otherwise the Control is simply hidden.</summary>
        public bool? UnregisterOnClose
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this control should be considered a document. If HasValue is true
        /// and if Value is true, then the following special behavior occurs: 1) The Window layout
        /// will be preserved if the document is eventually opened. 2) The Description will be used
        /// in the menu name instead of DockContent.Text. 3) The menu command will appear in a separate
        /// group (StandardCommandGroup.WindowDocuments). This System.Nullable.HasValue is false by default.</summary>
        /// <remarks>Description for a document control by convention is the full path of the document.</remarks>
        public bool? IsDocument
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this control should be visible when initially registered</summary>
        /// <remarks>The default value is true.</remarks>
        public bool VisibleByDefault
        {
            get { return m_visibleByDefault; }
            set { m_visibleByDefault = value; }
        }

        /// <summary>
        /// Gets or sets whether this Control should keep its appearance settings, such as for colors
        /// and fonts. If true, the Control's appearance is not changed by IControlHostService.
        /// If false, the Control's appearance settings is changed to match application-wide settings.
        /// The default is false.</summary>
        public bool UseCustomAppearance
        {
            get;
            set;
        }

        /// <summary>
        /// Class that provides options for controlling the docking behavior of the control.</summary>
        public class DockingInfo
        {
            /// <summary>
            /// Gets or sets the areas where the control can be docked.</summary>
            public StandardDockAreas DockAreas
            {
                get { return m_dockAreas; }
                set { m_dockAreas = value; }
            }

            /// <summary>
            /// Gets or sets a suggestion for the relative location of this control in the prospective tabbed container.</summary>
            public int Order { get; set; }

            /// <summary>
            /// Gets or sets an arbitrary object value that can be used to designate the prospective tabbed container.</summary>
            public object GroupTag { get; set; }

            private StandardDockAreas m_dockAreas = StandardDockAreas.Default;
        }

        /// <summary>
        /// Gets or sets the optional docking options object. The default is null.</summary>
        public DockingInfo Docking { get; set; }

        /// <summary>
        /// Gets the URL that is opened when the user presses F1 while this control has focus.
        /// If null or the empty string, then this feature is disabled.</summary>
        public string HelpUrl
        {
            get { return m_helpUrl; }
        }

        private IControlHostClient m_client;
        private Control m_control;
        private Control m_hostControl;
        private string m_name;
        private string m_description;
        private StandardControlGroup m_group;
        private Image m_image;
        private DockStyle m_originalDock;
        private bool m_inActiveGroup;
        private bool m_showInMenu = true;
        private bool m_visibleByDefault = true;
        private readonly string m_helpUrl;
    }
}
