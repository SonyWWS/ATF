//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Component to add "Add Layer" command to application. Command is accessible only
    /// by right click (context menu).</summary>
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(IContextMenuCommandProvider))]
    [InheritedExport(typeof(LayeringCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public abstract class LayeringCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        // required DomNodeType
        /// <summary>
        /// Gets type of layer folder</summary>
        protected abstract DomNodeType LayerFolderType { get; }
    

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="layerLister">Layer lister</param>
        [ImportingConstructor]
        public LayeringCommands(
            ICommandService commandService,
            IContextRegistry contextRegistry,
            LayerLister layerLister)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_layerLister = layerLister;
        }

      
        private enum CommandTag
        {
            AddLayerFolder,
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
              new CommandInfo(
                  CommandTag.AddLayerFolder,
                  null,
                  null,
                  "Add Layer".Localize(),
                  "Creates a new layer folder".Localize()),
              this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            return
                CommandTag.AddLayerFolder.Equals(commandTag)
                && m_targetRef != null
                && m_targetRef.Target != null
                && (m_targetRef.Target.Is<LayerFolder>() || m_targetRef.Target.Is<ILayeringContext>());
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (CommandTag.AddLayerFolder.Equals(commandTag))
            {
                LayerFolder newLayer = new DomNode(LayerFolderType).As<LayerFolder>();
                newLayer.Name = "New Layer".Localize();

                IList<LayerFolder> layerList = null;
                object target = m_targetRef.Target;
                if (target != null)
                {
                    LayerFolder parentLayer = target.As<LayerFolder>();
                    if (parentLayer != null)
                        layerList = parentLayer.Folders;
                    else
                    {
                        LayeringContext layeringContext = target.As<LayeringContext>();
                        if (layeringContext != null)
                            layerList = layeringContext.Layers;
                    }
                }

                if (layerList != null)
                {
                    ILayeringContext layeringContext = m_contextRegistry.GetMostRecentContext<ILayeringContext>();
                    ITransactionContext transactionContext = layeringContext.As<ITransactionContext>();
                    transactionContext.DoTransaction(() => layerList.Add(newLayer), "Add Layer".Localize());
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

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {
            m_targetRef = null;

            if (context.Is<LayeringContext>() && m_layerLister.TreeControl.Focused)
            {
                m_targetRef = new WeakReference(target);
                yield return CommandTag.AddLayerFolder;
            }
        }

        #endregion
     
        private readonly LayerLister m_layerLister;
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
        private WeakReference m_targetRef;
    }
}
