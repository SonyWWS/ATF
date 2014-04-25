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
    /// Component to add "Add Template Folder" command to application</summary>
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

        /// <summary>
        /// Gets most recent TemplatingContext</summary>
        public virtual TemplatingContext TemplatingContext
        {
            get { return ContextRegistry.GetMostRecentContext<TemplatingContext>(); }
        }

        /// <summary>
        /// Content imported from external file</summary>
        protected struct ImportedContent
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="rootNode">Root DomNode of imported content</param>
            /// <param name="uri">URI of imported content</param>
            public ImportedContent(DomNode rootNode, Uri uri)
            {
                RootNode = rootNode;
                Uri = uri;
            }

            /// <summary>
            /// Root DomNode of imported content</summary>
            public readonly DomNode RootNode;
            /// <summary>
            /// URI of imported content</summary>
            public readonly Uri Uri;
        }


        // required  DomNodeType info
        /// <summary>
        /// Gets type of template folder</summary>
        protected abstract DomNodeType TemplateFolderType { get; }

        /// <summary>
        /// Gets whether the target can be promoted to template library.
        /// Items can be promoted when the active context is CircuitEditingContext and all the items are selected modules.</summary>
        /// <param name="items">Items to promote</param>
        /// <returns>True iff the target can be promoted to template library</returns>
        public abstract bool CanPromoteToTemplateLibrary(IEnumerable<object> items);

        /// <summary>
        /// Promotes objects to template library.
        /// Items can be promoted when the active context is CircuitEditingContext and all the items are selected modules.</summary>
        /// <param name="items">Items to promote</param>
        public abstract void PromoteToTemplateLibrary(IEnumerable<object> items);

        /// <summary>
        /// Gets whether the target can be demoted from reference instances to copy instances.
        /// Items can be demoted when the active context is CircuitEditingContext and
        /// all the items are selected references.</summary>
        /// <param name="items">Items to demote</param>
        /// <returns>True iff the target can be demoted</returns>
        public abstract bool CanDemoteToCopyInstance(IEnumerable<object> items);

        /// <summary>
        /// Demotes items from reference instances to copy instances.
        /// Items can be demoted when the active context is CircuitEditingContext and
        /// all the items are selected references.</summary>
        /// <param name="items">Items to demote</param>
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

        /// <summary>
        /// Gets context registry</summary>
        protected IContextRegistry ContextRegistry
        {
            get { return m_contextRegistry; }
        }

        /// <summary>
        /// Enumeration for template commands</summary>
        protected enum CommandTag
        {
            /// <summary>Add Template Folder command</summary>
            AddTemplateFolder,
            /// <summary>Add External Template Folder command</summary>
            AddExternalTemplateFolder,
            /// <summary>Promote To Template Library command</summary>
            PromoteToTemplateLibrary,
            /// <summary>Demote To Copy Instance command</summary>
            DemoteToCopyInstance,
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering template commands</summary>
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

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
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

        /// <summary>
        /// Creates template folder</summary>
        /// <returns>TemplateFolder object</returns>
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

        /// <summary>
        /// Creates template folder and add templates stored in an external file to it</summary>
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

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
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

        /// <summary>
        /// Imports templates and template folders stored in an external file</summary>
        /// <param name="parentTemplateFolder">Template folder in which to import templates and template folders</param>
        /// <param name="fromParent">Root of templates to import</param>
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
