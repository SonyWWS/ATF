//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;

namespace ModelViewerSample
{   
    /// <summary>
    /// A MEF component for providing user commands related to the RenderView component</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(RenderCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RenderCommands : ICommandClient, IInitializable
    {        
        /// <summary>
        /// Rendering modes</summary>
        protected enum Command
        {
            Fit,
            RenderSmooth,
            RenderWireFrame,
            RenderOutlined,

            RenderTextured,
            RenderLight,
            RenderBackFace,
            RenderCycle
        }

        #region IInitializable Members

        /// <summary>
        /// Registers rendering-mode commands</summary>
        public virtual void Initialize()
        {
            m_commandService.RegisterCommand(
               Command.Fit,
               StandardMenu.View,
               CommandGroup,
               "Fit",
               "Fit All",
               Keys.F,
               null,
               CommandVisibility.Menu,
               this);

            m_commandService.RegisterCommand(                
                Command.RenderSmooth,
                StandardMenu.View,
                CommandGroup,
                "Smooth",
                "Smooth shading",
                Keys.None,
                Resources.SmoothImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(                
                Command.RenderWireFrame,
                StandardMenu.View,
                CommandGroup,
                "Wireframe",
                "Wireframe rendering",
                Keys.None,
                Resources.WireframeImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(                
                Command.RenderOutlined,
                StandardMenu.View,
                CommandGroup,
                "Outlined",
                "Smooth shading with wireframe outline",
                Keys.None,
                Resources.OutlinedImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(                
                Command.RenderTextured,
                StandardMenu.View,
                CommandGroup,
                "Textured",
                "Textured rendering",
                Keys.T,
                Resources.TexturedImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(                
                Command.RenderLight,
                StandardMenu.View,
                CommandGroup,
                "Lighting",
                "Lighting",
                Keys.L,
                Resources.LightImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(                
                Command.RenderBackFace,
                StandardMenu.View,
                CommandGroup,
                "BackFace",
                "Render backFaces",
                Keys.B,
                Resources.BackfaceImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(                
                Command.RenderCycle,
                StandardMenu.View,
                CommandGroup,
                "CycleRenderModes",
                "Cycle render modes",
                Keys.Space,
                null,
                CommandVisibility.Menu,
                this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            DesignControl activeControl = m_renderView.ViewControl;
            if (activeControl == null)
                return false;
            
            switch ((Command)commandTag)
            {
                case Command.Fit:
                case Command.RenderSmooth:
                case Command.RenderWireFrame:
                case Command.RenderOutlined:
                case Command.RenderLight:
                case Command.RenderBackFace:
                case Command.RenderCycle:
                    return true;

                case Command.RenderTextured:
                    return (activeControl.RenderState.RenderMode & RenderMode.Smooth) != 0;
            }

            return false;
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
                DesignControl control = m_renderView.ViewControl;
                switch ((Command)commandTag)
                {
                    case Command.Fit:
                        m_renderView.Fit();
                        break;
                    case Command.RenderSmooth:
                        control.RenderState.RenderMode &= ~RenderMode.Wireframe;
                        control.RenderState.RenderMode |= (RenderMode.Smooth | RenderMode.SolidColor |
                            RenderMode.Lit | RenderMode.CullBackFace | RenderMode.Textured);
                        control.Invalidate();
                        break;

                    case Command.RenderWireFrame:
                        control.RenderState.RenderMode |= (RenderMode.Wireframe | RenderMode.WireframeColor);
                        control.RenderState.RenderMode &= ~(RenderMode.Smooth | RenderMode.SolidColor |
                            RenderMode.Lit | RenderMode.Textured | RenderMode.CullBackFace);
                        control.Invalidate();
                        break;

                    case Command.RenderOutlined:
                        control.RenderState.RenderMode |= (RenderMode.Wireframe | RenderMode.Smooth |
                            RenderMode.Lit | RenderMode.WireframeColor | RenderMode.SolidColor |
                            RenderMode.CullBackFace | RenderMode.Textured);
                        control.Invalidate();
                        break;

                    case Command.RenderTextured:
                        control.RenderState.RenderMode ^= RenderMode.Textured;
                        control.Invalidate();
                        break;

                    case Command.RenderLight:
                        control.RenderState.RenderMode ^= RenderMode.Lit;
                        control.Invalidate();
                        break;

                    case Command.RenderBackFace:
                        control.RenderState.RenderMode ^= RenderMode.CullBackFace;
                        control.Invalidate();
                        break;

                    case Command.RenderCycle:
                        RenderState renderState = control.RenderState;
                        if ((renderState.RenderMode & RenderMode.Smooth) != 0 &&
                            (renderState.RenderMode & RenderMode.Wireframe) != 0)
                        {
                            // outlined -> smooth
                            goto case Command.RenderSmooth;
                        }
                        else if ((renderState.RenderMode & RenderMode.Smooth) != 0)
                        {
                            // smooth -> wireframe
                            goto case Command.RenderWireFrame;
                        }
                        else
                        {
                            // wireframe -> outlined
                            goto case Command.RenderOutlined;
                        }                    
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState state)
        {
            if (commandTag is Command)
            {
                DesignControl activeControl = m_renderView.ViewControl;
                if (activeControl == null)
                    return;

                switch ((Command)commandTag)
                {
                    case Command.RenderSmooth:
                        state.Check = (activeControl.RenderState.RenderMode & RenderMode.Smooth) != 0;
                        break;

                    case Command.RenderWireFrame:
                        state.Check = (activeControl.RenderState.RenderMode & RenderMode.Wireframe) != 0;
                        break;

                    case Command.RenderOutlined:
                        state.Check = (activeControl.RenderState.RenderMode & RenderMode.Smooth) != 0 &&
                                      (activeControl.RenderState.RenderMode & RenderMode.Wireframe) != 0;
                        break;

                    case Command.RenderTextured:
                        state.Check = (activeControl.RenderState.RenderMode & RenderMode.Textured) != 0;
                        break;

                    case Command.RenderLight:
                        state.Check = ((activeControl.RenderState.RenderMode & RenderMode.Lit) != 0);
                        break;

                    case Command.RenderBackFace:
                        state.Check = ((activeControl.RenderState.RenderMode & RenderMode.CullBackFace) == 0);
                        break;
                }
            }
        }

        #endregion

        [Import(AllowDefault=false)]
        private ICommandService m_commandService;

        [Import(AllowDefault=false)]
        private RenderView m_renderView;

        private static string CommandGroup = "RenderingModes";
    }
}
