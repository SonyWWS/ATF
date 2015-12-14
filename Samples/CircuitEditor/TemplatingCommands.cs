//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Component to add "Add Template Folder" command to application</summary>
    public class TemplatingCommands : Sce.Atf.Dom.TemplatingCommands
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="templateLister">Template library lister</param>
        [ImportingConstructor]
        public TemplatingCommands(ICommandService commandService, IContextRegistry contextRegistry, TemplateLister templateLister) : 
            base(commandService, contextRegistry, templateLister)
        {
        }

        /// <summary>
        /// Gets type of template folder</summary>
        protected override DomNodeType TemplateFolderType
        {
            get { return Schema.templateFolderType.Type; }
        }

        /// <summary>
        /// Gets whether the target can be promoted to template library.
        /// Items can be promoted when the active context is CircuitEditingContext and all the items are selected modules.</summary>
        /// <param name="items">Items to promote</param>
        /// <returns>True iff the target can be promoted to template library</returns>
        public override bool CanPromoteToTemplateLibrary(IEnumerable<object> items)
        {
            var circuitEditingContext = ContextRegistry.GetActiveContext<CircuitEditingContext>();
            if (circuitEditingContext != null && items.Any())
            {
                foreach (var item in items)
                {
                    if (item.Is<IReference<Module>>())
                        return false; // guess we don't need nested referencing
                    bool validCandiate = circuitEditingContext.Selection.Contains(item) && item.Is<Module>() ;
                    if (!validCandiate)
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Promotes objects to template library.
        /// Items can be promoted when the active context is CircuitEditingContext and all the items are selected modules.</summary>
        /// <param name="items">Items to promote</param>
        public override void PromoteToTemplateLibrary(IEnumerable<object> items)
        {
            var itemsArray = items.ToArray();
          
 
            // cache the external connections
            var externalConnectionsDict = new Dictionary<Element, List<Wire>>();
            var circuitEditingContext = ContextRegistry.GetActiveContext<CircuitEditingContext>();
            var graphContainer = circuitEditingContext.CircuitContainer;

            foreach (var item in itemsArray)
            {
                var modules = new HashSet<Element>();
                var internalConnections = new List<Wire>();
                var externalConnections = new List<Wire>();
                CircuitUtil.GetSubGraph(graphContainer, new[] { item }, modules, internalConnections, externalConnections, externalConnections);
                externalConnectionsDict.Add(item.Cast<Element>(), externalConnections);
            }

            // check source guid for templates to be replaced
            var templatingItems = new List<object>();
            var replacingItems = new List<object>();
            foreach (var item in itemsArray)
            {
                if (item.Is<Module>())
                {
                    var module = item.Cast<Module>();
                    if (module.SourceGuid != Guid.Empty)
                    {
                        var existingTemplate = TemplatingContext.SearchForTemplateByGuid(TemplatingContext.RootFolder,  module.SourceGuid);
                        if (existingTemplate != null)
                        {
                            string message = string.Format(
                                "Overwrite the existing \"{0}\"  Template with \"{1}\", or Add new one?\n".Localize(),
                                    existingTemplate.Name, module.Name);

                            var dialog = new ConfirmationDialog("Overwrite / Add".Localize(), message);
                            dialog.YesButtonText = "Overwrite".Localize();
                            dialog.NoButtonText = "Add".Localize();
                            dialog.Size = new System.Drawing.Size(300, 100);
                            DialogResult result = dialog.ShowDialog();
                            if (result == DialogResult.Yes)
                            {
                                TemplatingContext.ReplaceTemplateModel(existingTemplate, module.Cast<DomNode>());
                                replacingItems.Add(item);
                            }
                            else if (result == DialogResult.No)
                                templatingItems.Add(item);
                            //otherwise the item is skipped
                        }
                    }
                    else 
                        templatingItems.Add(item);
                }
            }

            // pack objects in IDataObject format 
            var dataObject = new DataObject();
            dataObject.SetData(typeof(object[]), templatingItems.ToArray());

            // Insert() expects IDataObject
            TemplatingContext.Insert(dataObject);

            // replace the original items with the template instances 
            foreach (var originalItem in templatingItems.Concat(replacingItems))
            {
                var template = TemplatingContext.LastPromoted(originalItem);
                var instance = TemplatingContext.CreateReference(template);

                var originalModule = originalItem.Cast<Element>();
                var replacedModule = instance.Cast<Element>();
                replacedModule.Bounds = originalModule.Bounds;
                replacedModule.Position = originalModule.Position;
 
                // reroute external connections from original modules to replaced template instances.
                var externalConnections = externalConnectionsDict[originalModule];
                foreach (var connection in externalConnections)
                {
                    if (connection.InputElement.DomNode == originalModule.DomNode)
                    {
                        // input pin, i.e. pin on element that receives connection as input
                        int pinIndex = connection.InputPin.Index;
                        connection.InputPin = replacedModule.Type.GetInputPin(pinIndex);
                        connection.InputElement = replacedModule;
                        connection.InputPinTarget = null; // reset
                    }
                    else if (connection.OutputElement.DomNode == originalModule.DomNode)//output pin, i.e., pin on element that receives connection as output
                    {
                        connection.OutputPin = replacedModule.Type.GetOutputPin(connection.OutputPin.Index);
                        connection.OutputElement = replacedModule;
                        connection.OutputPinTarget = null; 
                   
                    }
                    else
                         Debug.Assert(false);
                    
                 
                }
                circuitEditingContext.CircuitContainer.Elements.Remove(originalItem.Cast<Element>());
                circuitEditingContext.CircuitContainer.Elements.Add(replacedModule);
            }
        }

        /// <summary>
        /// Gets whether the target can be demoted from reference instances to copy instances.
        /// Items can be demoted when the active context is CircuitEditingContext and
        /// all the items are selected references.</summary>
        /// <param name="items">Items to demote</param>
        /// <returns>True iff the target can be demoted</returns>
        public override bool CanDemoteToCopyInstance(IEnumerable<object> items)
        {
            var circuitEditingContext = ContextRegistry.GetActiveContext<CircuitEditingContext>();
            if (circuitEditingContext != null && items.Any())
            {
                DomNode parent = null;
                foreach (var item in items)
                {
                    bool validCandiate = circuitEditingContext.Selection.Contains(item) &&
                        (item.Is<GroupReference>() || item.Is<ModuleReference>());
                    if (!validCandiate)
                        return false;
                    if (CircuitUtil.IsTemplateTargetMissing(item))
                        return false;

                    var currentParent = item.Cast<DomNode>().Parent;
                    if (parent == null)
                        parent = currentParent;
                    else if (parent != currentParent) // items limit to same parent 
                        return false;          
                }
                return parent.Is<ICircuitContainer>();
            }
            return false;
        }

        /// <summary>
        /// Demotes items from reference instances to copy instances.
        /// Items can be demoted when the active context is CircuitEditingContext and
        /// all the items are selected references.</summary>
        /// <param name="items">Items to demote</param>
        /// <returns>Array of copy instances</returns>
        public override DomNode[] DemoteToCopyInstance(IEnumerable<object> items)
        {
            // cache the external connections
            var externalConnectionsDict = new Dictionary<Element, List<Wire>>();
            var graphContainer = items.First().Cast<DomNode>().Parent.Cast<ICircuitContainer>();
            var modules = new HashSet<Element>();
            var internalConnections = new List<Wire>();
            var externalConnections = new List<Wire>();
            foreach (var item in items)
            {
                CircuitUtil.GetSubGraph(graphContainer, new[] { item }, modules, internalConnections, externalConnections, externalConnections);
                externalConnectionsDict.Add(item.Cast<Element>(), externalConnections);
            }


            var originalRefs = items.Select(x => x.Cast<DomNode>()).ToArray();
            var domNodes = items.Select(x => x.Cast<IReference<Module>>().Target.DomNode).ToArray();
            var itemCopies = DomNode.Copy(domNodes); // DOM deep copy
             // Position and Expanded properties copy from the referencing nodes
            for (int i = 0; i < itemCopies.Length; ++i)
            {
                var copy = itemCopies[i];
                copy.Cast<Module>().Bounds = originalRefs[i].Cast<Module>().Bounds;
                copy.Cast<Module>().Position = originalRefs[i].Cast<Module>().Position;
                copy.Cast<Module>().SourceGuid = originalRefs[i].Cast<Module>().SourceGuid;
                if (originalRefs[i].Is<GroupReference>())
                {
                    copy.Cast<Group>().Expanded = originalRefs[i].Cast<GroupReference>().Expanded;
                }

                // reroute external connections from original modules to replaced template instances.
                externalConnections = externalConnectionsDict[originalRefs[i].Cast<Module>()];
                foreach (var connection in externalConnections)
                {
                    if (connection.InputElement.DomNode == originalRefs[i])
                    {
                        // input pin, i.e. pin on element that receives connection as input
                        int pinIndex = connection.InputPin.Index;
                        connection.InputPin = copy.Cast<Module>().Type.GetInputPin(pinIndex);
                        connection.InputElement = copy.Cast<Module>();
                        connection.InputPinTarget = null; // reset
                    }
                    else if (connection.OutputElement.DomNode == originalRefs[i])//output pin, i.e., pin on element that receives connection as output
                    {
                        connection.OutputPin = copy.Cast<Module>().Type.GetOutputPin(connection.OutputPin.Index);
                        connection.OutputElement = copy.Cast<Module>();
                        connection.OutputPinTarget = null;

                    }
                    else
                        Debug.Assert(false);


                }
                graphContainer.Elements.Remove(originalRefs[i].Cast<Element>());
                graphContainer.Elements.Add(copy.Cast<Element>());
            }

            return itemCopies;
        }

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        public override IEnumerable<object> GetCommands(object context, object target)
        {
            foreach (object command in base.GetCommands(context, target))
                yield return command;

            if (context.Is<CircuitEditingContext>())
            {
                yield return CommandTag.PromoteToTemplateLibrary;
                yield return CommandTag.DemoteToCopyInstance;
            }
        }

        /// <summary>
        ///  Load circuit templates stored in an external file</summary>
        /// <param name="uri">Document URI, or null to present file open dialog to user</param>
        /// <returns>Returns the file path used to load the external templates.
        /// An empty string indicates no templates were loaded</returns>
        protected override ImportedContent LoadExternalTemplateLibrary(Uri uri)
        {
 
            string filePath = string.Empty;
            if (uri == null)
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "Circuit Template File (*.circuit)|*.circuit".Localize();
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    uri = new Uri(dlg.FileName, UriKind.RelativeOrAbsolute);
                    filePath = dlg.FileName;
                }

            }
            else
                filePath = uri.LocalPath;

            if (File.Exists(filePath))
            {
                if (TemplatingContext.ValidateNewFolderUri(uri))
                {
                    // read existing document using standard XML reader
                    using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var reader = new CircuitReader(m_schemaLoader);
                        var root = reader.Read(stream, uri);
                        var toFolder = CreateTemplateFolder();
                        reader.ImportTemplates(toFolder.DomNode, root, uri);
                        return new ImportedContent(toFolder.DomNode, uri);
                    }                 
                }
            }

            return new ImportedContent(null, null);           
        }

        /// <summary>
        /// Rescan all referenced template documents </summary>
        protected override void ReloadExternalTemplates()
        {
            // 1st, scan the template hierarchy tree and remember all the existing template's guid -> DomNode map
            var oldNodeGuidDictionary = new Dictionary<string, DomNode>();
            foreach (var node in TemplatingContext.RootFolder.DomNode.Subtree)
            {
                if (node.Is<Template>())
                {
                    var template = node.Cast<Template>();
                    if (template.Guid != Guid.Empty)
                        oldNodeGuidDictionary[template.Guid.ToString()] = node;
                }
            }

            // 2nd, scan the template hierarchy tree again and reload all the external templates  
            var newNodeGuidDictionary = new Dictionary<string, DomNode>();

            foreach (var node in TemplatingContext.RootFolder.DomNode.Subtree)
            {
                var templateFolder = node.As<Sce.Atf.Dom.TemplateFolder>();
                if (templateFolder == null || templateFolder.Url == null)
                    continue;
                if (File.Exists(templateFolder.Url.LocalPath))
                {
                    using (FileStream stream = new FileStream(templateFolder.Url.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        var reader = new CircuitReader(m_schemaLoader);
                        var rootNode = reader.Read(stream, templateFolder.Url);

                        foreach (var newnode in rootNode.Subtree)
                        {
                            var guidAttr = newnode.Type.GetAttributeInfo("guid");
                            if (guidAttr == null)
                                continue;
                            var guidStr = newnode.GetAttribute(guidAttr) as string;
                            if (!string.IsNullOrEmpty(guidStr))
                                newNodeGuidDictionary[guidStr] = newnode;
                        }
                        reader.ImportTemplates(templateFolder.DomNode, rootNode, templateFolder.Url);
                    }
                }
            }

            // 3rd, replace original nodes with newly loaded, matched by GUIDs
            foreach (var node in TemplatingContext.RootFolder.DomNode.GetRoot().Subtree)
            {
                // currently two types that reference templates: GroupInstance or ModuleInstance
                DomNode refNode;

                var groupInstance = node.As<GroupReference>();
                if (groupInstance != null)
                {
                    if (newNodeGuidDictionary.TryGetValue(groupInstance.Template.Guid.ToString(), out refNode))
                    {
                        groupInstance.Template = refNode.As<Template>();
   
                        // need to reset pin targets due to DomNode replacements 
                        var graphContainer = groupInstance.DomNode.Parent.Cast<ICircuitContainer>();
                        foreach (var edge in graphContainer.Wires)
                        {
                            if (edge.OutputElement.DomNode == groupInstance.DomNode)
                                edge.OutputPinTarget = null;

                            if (edge.InputElement.DomNode == groupInstance.DomNode)
                                edge.InputPinTarget = null;

                        }
                        continue;
                    }                 
                }

                var moduleReference = node.As<ModuleReference>();
                if (moduleReference != null)
                {
                    if (newNodeGuidDictionary.TryGetValue(moduleReference.Template.Guid.ToString(), out refNode))
                    {
                        moduleReference.Template = refNode.As<Template>();

                        // need to reset pin targets due to DomNode replacements 
                        var graphContainer = moduleReference.DomNode.Parent.Cast<ICircuitContainer>();
                        foreach (var edge in graphContainer.Wires)
                        {
                            if (edge.OutputElement.DomNode == moduleReference.DomNode)
                                edge.OutputPinTarget = null;

                            if (edge.InputElement.DomNode == moduleReference.DomNode)
                                edge.InputPinTarget = null;
                        }
                    }
                }
            }

            // finally, prune the old nodes
            foreach (var keyValue in newNodeGuidDictionary)
            {
                DomNode oldNode;
                if (oldNodeGuidDictionary.TryGetValue(keyValue.Key, out oldNode))
                {
                    oldNode.RemoveFromParent();
                }
            }
        }

        [Import]
        private SchemaLoader m_schemaLoader= null;
    }
}
