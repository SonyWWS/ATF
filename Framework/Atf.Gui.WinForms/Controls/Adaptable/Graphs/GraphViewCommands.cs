//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Commands to interactively increase or decrease the magnification of the graph using zoom presets.</summary>
    /// <remarks>This component can be used for graph display using adaptable control.</remarks>
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(GraphViewCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GraphViewCommands : ICommandClient, IInitializable
    {
         /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public GraphViewCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.ViewZoomIn, this);
            m_commandService.RegisterCommand(CommandInfo.ViewZoomOut, this);
            m_commandService.RegisterCommand(CommandInfo.ViewZoomReset, this);
            //m_commandService.RegisterCommand(CommandInfo.ViewZoomExtents, this); // TODO
        }

        #endregion

        /// <summary>
        /// Gets or sets the predefined zoom levels.</summary>
        public float [] ZoomPresets
        {
            get { return m_zoomPresets; }
            set { m_zoomPresets = value; }
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True if client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool canDo = false;
            if (commandTag is StandardCommand)
            {
                var viewingContext = m_contextRegistry.GetActiveContext<IViewingContext>();
                if (viewingContext == null)
                    return false;
                var adaptableControl = viewingContext.As<AdaptableControl>();
                if (adaptableControl == null)
                    return false;
                var transformAdapter = adaptableControl.As<ITransformAdapter>();

                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewZoomIn:
                    case StandardCommand.ViewZoomOut:
                    case StandardCommand.ViewZoomReset:
                        canDo = transformAdapter != null;
                        break;
                }
            }

            return canDo;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewZoomIn:
                        ZoomIn();
                        break;

                    case StandardCommand.ViewZoomOut:
                        ZoomOut();
                        break;
                    case StandardCommand.ViewZoomReset:
                        ZoomReset();
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        // increase zoom level
        private void ZoomIn()
        {
            var viewingContext = m_contextRegistry.GetActiveContext<IViewingContext>();
            var adaptableControl = viewingContext.As<AdaptableControl>();
            var transformAdapter = adaptableControl.As<ITransformAdapter>();

            PointF scale = transformAdapter.Scale;

            foreach (float zoom in m_zoomPresets)
            {
                if (zoom > scale.X && zoom > scale.Y)
                {
                    scale.X = zoom;
                    scale.Y = zoom;
                    break;
                }
            }

            ZoomView(scale, adaptableControl);
        }

        // decrease zoom level
        private void ZoomOut()
        {
            var viewingContext = m_contextRegistry.GetActiveContext<IViewingContext>();
            var adaptableControl = viewingContext.As<AdaptableControl>();
            var transformAdapter = adaptableControl.As<ITransformAdapter>();

            PointF scale = transformAdapter.Scale;

            for (int i= m_zoomPresets.Length-1; i>=0; --i)
            {
                float zoom = m_zoomPresets[i];
                if (zoom < scale.X && zoom < scale.Y)
                {
                    scale.X = zoom;
                    scale.Y = zoom;
                    break;
                }

            }

            ZoomView(scale, adaptableControl);

        }

        private void ZoomView(PointF newScale, AdaptableControl adaptableControl)
        {
            Point clientPoint = adaptableControl.PointToClient(Cursor.Position);
            if (!adaptableControl.ClientRectangle.Contains(clientPoint))
                clientPoint = new Point(adaptableControl.Width/2, adaptableControl.Height/2); // use control center

            var transformAdapter = adaptableControl.As<ITransformAdapter>();
            Point anchorPoint = GdiUtil.InverseTransform(transformAdapter.Transform, clientPoint);

            PointF scale = transformAdapter.ConstrainScale(newScale);
            var translation = new PointF(
                (transformAdapter.Scale.X - scale.X) * anchorPoint.X + transformAdapter.Translation.X,
                (transformAdapter.Scale.Y - scale.Y) * anchorPoint.Y + transformAdapter.Translation.Y);

            transformAdapter.SetTransform(scale.X, scale.Y, translation.X, translation.Y);

        }

        private void ZoomReset()
        {
            var viewingContext = m_contextRegistry.GetActiveContext<IViewingContext>();
            var adaptableControl = viewingContext.As<AdaptableControl>();
            var transformAdapter = adaptableControl.As<ITransformAdapter>();
            transformAdapter.SetTransform(1, 1, 0, 0);
        }

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
        private float [] m_zoomPresets = {0.25f, 0.50f, 0.75f, 1.00f, 1.25f, 1.50f, 1.75f, 2.00f, 3.0f};
    }
}
