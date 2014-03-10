//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;


namespace Sce.Atf.Dom
{
    /// <summary>
    /// Component to add "Add Template Folder" command to app.</summary>
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(IContextMenuCommandProvider))]
    [InheritedExport(typeof(TemplatingCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public abstract class TemplatingCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="templateLister">Template library lister</param>
        [ImportingConstructor]
        protected TemplatingCommands(
            ICommandService commandService,
            IContextRegistry contextRegistry,
            TemplateLister templateLister)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_templateLister = templateLister;
        }

        public virtual TemplatingContext TemplatingContext
        {
            get { return ContextRegistry.GetMostRecentContext<TemplatingContext>(); }
        }

        protected struct ImportedContent
        {
            public ImportedContent(DomNode rootNode, Uri uri)
            {
                RootNode = rootNode;
                Uri = uri;
            }

            public readonly DomNode RootNode;
            public readonly Uri Uri;
        }


        // required  DomNodeType info
        protected abstract DomNodeType TemplateFolderType { get; }

        // whether the target can be promoted to template library
        public abstract bool CanPromoteToTemplateLibrary(IEnumerable<object> items);

        public abstract void PromoteToTemplateLibrary(IEnumerable<object> items);

        // whether the target can be demoted from reference instance to copy instance
        public abstract bool CanDemoteToCopyInstance(IEnumerable<object> items);

        public abstract DomNode[] DemoteToCopyInstance(IEnumerable<object> items);

        /// <summary>
        /// Client overrides this method to load templates stored in an external file</summary>
        /// <param name="uri">Document URI, or null to present file open dialog to user</param>
        /// <returns>Returns the file path used to load the external templates.
        /// An empty string indicates no templates were loaded</returns>
        protected virtual ImportedContent LoadExternalTemplateLibrary(Uri uri)
        {
            // Do nothing here; derived class is expected to override the method for
            // customized loading of the DOM-tree from the uri
            return new ImportedContent(null, uri); 
        }

        protected IContextRegistry ContextRegistry
        {
            get { return m_contextRegistry; }
        }

        protected enum CommandTag
        {
            AddTemplateFolder,
            AddExternalTemplateFolder,
            PromoteToTemplateLibrary,
            DemoteToCopyInstance,
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                new CommandInfo(
                    CommandTag.AddTemplateFolder,
                    null,
                    null,
                    "Add Template Folder".Localize(),
                    "Creates a new template folder".Localize()),
                this);

            m_commandService.RegisterCommand(
              new CommandInfo(
                  CommandTag.AddExternalTemplateFolder,
                  null,
                  null,
                  "Add Global Template Folder".Localize(),
                  "Creates a Global Template folder based off of a pre-existing .mcc".Localize()),
              this);

            m_commandService.RegisterCommand(
              CommandTag.PromoteToTemplateLibrary,
              StandardMenu.Edit,
              StandardCommandGroup.EditOther,
              "Promote To Template Library".Localize(),
              "Promote To Template Library".Localize(),
              Keys.None,
              null,
              CommandVisibility.ContextMenu,
              this);

            m_commandService.RegisterCommand(
              CommandTag.DemoteToCopyInstance,
              StandardMenu.Edit,
              StandardCommandGroup.EditOther,
              "Demote To Copy Instance".Localize(),
              "Demote To Copy Instance".Localize(),
              Keys.None,
              null,
              CommandVisibility.ContextMenu,
              this);

            if (m_scriptingService != null)
            {
                m_scriptingService.SetVariable("templateCmds", this);
            }
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            if (commandTag is CommandTag)
            {
                if (CommandTag.AddTemplateFolder.Equals(commandTag))
                    return true;
                if (CommandTag.AddExternalTemplateFolder.Equals(commandTag))
                    return true;
                if (CommandTag.PromoteToTemplateLibrary.Equals(commandTag))
                {
                    var context = m_contextRegistry.GetActiveContext<ISelectionContext>();
                    if (context != null)
                        return CanPromoteToTemplateLibrary(context.Selection);
                }
                else if (CommandTag.DemoteToCopyInstance.Equals(commandTag))
                {
                    var context = m_contextRegistry.GetActiveContext<ISelectionContext>();
                    if (context != null)
                        return CanDemoteToCopyInstance(context.Selection);
                }
            }
            return false;

        }

        protected virtual TemplateFolder CreateTemplateFolder()
        {
            var newFolder = new DomNode(TemplateFolderType).As<TemplateFolder>();
            newFolder.Name = "New Template Folder".Localize();

            var parentFolder = m_targetRef.Target.As<TemplateFolder>();
            if (parentFolder == null)
            {
                var templatinContext = m_targetRef.Target.As<TemplatingContext>();
                if (templatinContext != null)
                    parentFolder = templatinContext.RootFolder;
            }

            if (parentFolder != null)
            {
                parentFolder.Folders.Add(newFolder);
                   
            }

            return newFolder;
        }

        protected virtual void AddExternalTemplateFolder()
        {
            var importedLibaray = LoadExternalTemplateLibrary(null);
            if (importedLibaray.RootNode != null)
            {
                importedLibaray.RootNode.InitializeExtensions();

                var templateFolder = CreateTemplateFolder();
                templateFolder.Name = Path.GetFileNameWithoutExtension(importedLibaray.Uri.LocalPath);

                // try make file uri relative to the current document uri, which is nornally 
                // well, keep simple to use absolute uri for now, so we don't need to update relative uris when the SaveAs document to a different directory
                //if (TemplatingContext.RootFolder.DomNode.GetRoot().Is<IDocument>())
                //{
                //    var doc = TemplatingContext.RootFolder.DomNode.GetRoot().Cast<IDocument>();
                //    templateFolder.Url = importedLibaray.Uri.MakeRelativeUri(doc.Uri);
                //}
                //else
                templateFolder.Url = importedLibaray.Uri;
                ImportTemplates(templateFolder, importedLibaray.RootNode);
            }
        }

        public virtual void DoCommand(object commandTag)
        {
            var context = m_contextRegistry.GetActiveContext<ISelectionContext>();
            var transactionContext = context.As<ITransactionContext>();
            if (CommandTag.AddTemplateFolder.Equals(commandTag))
            {
                transactionContext.DoTransaction(
                    () => CreateTemplateFolder(), "Add Template Folder".Localize());
            }
            else if (CommandTag.AddExternalTemplateFolder.Equals(commandTag))
            {
                transactionContext.DoTransaction(
                  AddExternalTemplateFolder, "Add External Template Folder".Localize());
             
            }
            else if (CommandTag.PromoteToTemplateLibrary.Equals(commandTag))
            {
               
                transactionContext.DoTransaction(
                    () => PromoteToTemplateLibrary(context.Selection),
                    "Promote To Template Library".Localize());
            }
            else if (CommandTag.DemoteToCopyInstance.Equals(commandTag))
            {
                transactionContext.DoTransaction(
                    () => DemoteToCopyInstance(context.Selection),
                    "Demote To Copy Instance".Localize());
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        public virtual IEnumerable<object> GetCommands(object context, object target)
        {
            m_targetRef = null;

            if (context.Is<TemplatingContext>() && m_templateLister.TreeControl.Focused)
            {
                m_targetRef = new WeakReference(target);
                yield return CommandTag.AddTemplateFolder;
                yield return CommandTag.AddExternalTemplateFolder;                
            }

        }

        #endregion

        protected virtual void ImportTemplates(TemplateFolder parentTemplateFolder,  DomNode fromParent)
        {
            // assume all templates and their containing folders are children of a root template folder 
            foreach (var domNode in fromParent.LevelSubtree) // add top-level folders
            {
                if (domNode.Is<TemplateFolder>()) // this should be the root template folder of the imported DOM tree
                {
                    foreach (var child in domNode.Children.ToArray())
                    {
                        if (child.Is<TemplateFolder>())
                        {                        
                            parentTemplateFolder.Folders.Add(child.Cast<TemplateFolder>());
                        }
                        else if (child.Is<Template>())
                        {

                            parentTemplateFolder.Templates.Add(child.Cast<Template>());
                        }
                    }                   
                    break;
                }
              
            }
        }

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;
  
        private TemplateLister m_templateLister;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private WeakReference m_targetRef;
    }
}
