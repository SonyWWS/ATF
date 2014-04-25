//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation; 
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Component that sets background color of annotations
    /// </summary>
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(IContextMenuCommandProvider))]
    [InheritedExport(typeof(AnnotatingCommands))]
    [PartCreationPolicy(CreationPolicy.Any)]  
    public class AnnotatingCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public AnnotatingCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Preset annotation color</summary>
        public class ColorPreset
        {
            /// <summary>
            /// Gets or sets annotation background color name</summary>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets annotation background color value</summary>
            public Color Color { get; set; }
        }

        /// <summary>
        /// Gets or sets color presets</summary>
        /// <remarks>Set custom presets before Initialize() is called</remarks>
        public ColorPreset[] ColorPresets
        {
            get { return m_colorPresets;  }
            set { m_colorPresets = value; }
        }
    
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            InitColorPresets();
            for (int i = 0; i < m_colorPresets.Length; ++i)
            {
                var cmdInfo = m_commandService.RegisterCommand(
                                m_colorPresets[i],
                                StandardMenu.Edit,
                                StandardCommandGroup.EditOther,
                                m_colorPresets[i].Name,
                                m_colorPresets[i].Name,
                                Keys.None,
                                null,
                                CommandVisibility.ContextMenu,
                                this);

                var menuItem = cmdInfo.GetMenuItem();
                menuItem.Image = CreateBackColorIcon(m_colorPresets[i].Color, 24, 24);
            }
 
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            bool enabled = false;
            if (commandTag is ColorPreset)
            {
                var context = m_contextRegistry.GetActiveContext<IColoringContext>();
                if (context != null)
                {
                    var selectionContext = context.As<ISelectionContext>();
                    if (selectionContext != null)
                    {
                        enabled = selectionContext.Selection.Any() &&
                                  selectionContext.Selection.All(x => x.Is<IAnnotation>());
                    }
                }
            }
            return enabled;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is ColorPreset)
            {
                var context = m_contextRegistry.GetActiveContext<IColoringContext>();
                var transactionContext = m_contextRegistry.ActiveContext.As<ITransactionContext>();
                var colorPreset = (ColorPreset)commandTag;

                transactionContext.DoTransaction(() =>
                    {
                        var selectionContext = context.As<ISelectionContext>();
                        foreach (var annotation in selectionContext.Selection.AsIEnumerable<IAnnotation>())
                            context.SetColor(ColoringTypes.BackColor, annotation,colorPreset.Color);
                    },"Annotation Color");
                   
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {
            if (context.Is<IColoringContext>())
            {              
                return m_colorPresets;             
            }
            return EmptyEnumerable<object>.Instance;
        }

        #endregion

        private void InitColorPresets()
        {
            if (m_colorPresets == null)
            {
                m_colorPresets = new ColorPreset[6];
                m_colorPresets[0] = new ColorPreset()
                    {
                        Name = "Yellow".Localize(),
                        Color = SystemColors.Info
                    };
                m_colorPresets[1] = new ColorPreset()
                    {
                        Name = "Blue".Localize(),
                        Color = Color.LightSkyBlue
                    };
                m_colorPresets[2] = new ColorPreset()
                    {
                        Name = "Green".Localize(),
                        Color = Color.FromArgb(178, 255, 161)
                    };
                m_colorPresets[3] = new ColorPreset()
                    {
                        Name = "Pink".Localize(),
                        Color = Color.LightPink
                    };
                m_colorPresets[4] = new ColorPreset()
                    {
                        Name = "Purple".Localize(),
                        Color = Color.FromArgb(182, 202, 255)
                    };
                m_colorPresets[5] = new ColorPreset()
                    {
                        Name = "Gray".Localize(),
                        Color = Color.LightGray
                    };
            }
        }

        private Image CreateBackColorIcon(Color color, int width, int height)
        {
            Image img = new Bitmap(width, height);
            Graphics drawing = Graphics.FromImage(img);
            drawing.Clear(color);
            drawing.Save();
            drawing.Dispose();
            return img;
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private ColorPreset[] m_colorPresets;
    }
}
