//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component to provide property editing commands that can be used inside
    /// PropertyGrid-like controls. Currently, it defines context-menu only
    /// commands to reset the current property and all properties.</summary>
    [Export(typeof(PropertyEditingCommands))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PropertyEditingCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service used to add context menu commands Reset Current and Reset All</param>
        [ImportingConstructor]
        public PropertyEditingCommands(ICommandService commandService)
        {
            m_commandService = commandService;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                Command.CopyProperty,
                null,
                null,
                "Copy Property".Localize("Copies this property's value to the local clipboard"),
                "Copies this property's value to the to local clipboard".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);

            m_commandService.RegisterCommand(
               Command.PasteProperty,
               null,
               null,
               "Paste Property".Localize("Pastes the local clipboard into this property's value"),
               "Pastes the local clipboard into this property's value".Localize(),
               Keys.None,
               null,
               CommandVisibility.ContextMenu, // context menu only
               this);

            m_commandService.RegisterCommand(
                Command.ResetProperty,
                null,
                null,
                "Reset Property".Localize("Reset the current property to its default value"),
                "Reset the current property to its default value".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);


            m_commandService.RegisterCommand(
                Command.CopyAll,
                null,
                null,
                "Copy All".Localize("Copies all properties to the local clipboard"),
                "Copies all properties to the local clipboard".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);


            m_commandService.RegisterCommand(
                Command.PasteAll,
                null,
                null,
                "Paste All".Localize("Pastes the local clipboard into all properties"),
                "Pastes the local clipboard into all properties".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);

            m_commandService.RegisterCommand(
                Command.ResetAll,
                null,
                null,
                "Reset All".Localize("Reset all properties to their default values"),
                "Reset all properties to their default values".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);
           
            m_commandService.RegisterCommand(
               Command.ViewInTextEditor,
               null,
               null,
               "View In Text Editor".Localize("Open the file in the associated text editor"),
               "Open the file in the associated text editor".Localize(),
               Keys.None,
               null,
               CommandVisibility.ContextMenu, // context menu only
               this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            if (commandTag is Command && m_editingContext != null)
            {
                switch ((Command)commandTag)
                {
                    case Command.CopyProperty:
                        return m_descriptor != null 
                            && !(m_descriptor is ChildPropertyDescriptor)
                            && !(m_descriptor is ChildAttributeCollectionPropertyDescriptor);

                    case Command.PasteProperty:
                        {
                            var lastObject = m_editingContext.Items.LastOrDefault();

                            return m_descriptor != null && CanPaste(m_copyValue, m_copyDescriptor, m_descriptor,
                                m_descriptor.GetValue(lastObject));
                        }

                    case Command.ResetProperty:
                        return CanResetValue(m_editingContext.Items, m_descriptor);                        

                    case Command.CopyAll:
                        {
                            foreach (var descriptor in m_editingContext.PropertyDescriptors)
                            {
                                if ( (descriptor is ChildPropertyDescriptor)
                                    || (descriptor is ChildAttributeCollectionPropertyDescriptor))
                                    continue;
                                AttributePropertyDescriptor attr = descriptor as AttributePropertyDescriptor;
                                if (attr != null && attr.AttributeInfo.IsIdAttribute)
                                    continue;
                                return true;
                            }
                            break;
                        }
                   
                    case Command.PasteAll:
                        return m_descriptorToValue.Count > 0;

                    case Command.ResetAll:
                        foreach (PropertyDescriptor descriptor in m_editingContext.PropertyDescriptors)
                        {
                            if (CanResetValue(m_editingContext.Items, descriptor))
                                return true;
                        }
                        break;

                       
                    case Command.ViewInTextEditor:
                        if (m_descriptor != null && m_descriptor.GetEditor(typeof(UITypeEditor)) is FileUriEditor)
                            return true;
                        break;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            ITransactionContext transactionContext = m_editingContext.As<ITransactionContext>();
            switch ((Command)commandTag)
            {
                case Command.CopyProperty:
                    {
                        if (!(m_descriptor is ChildPropertyDescriptor))
                        {
                            var lastObject = m_editingContext.Items.LastOrDefault();
                            m_copyDescriptor = m_descriptor;
                            m_copyValue = m_descriptor.GetValue(lastObject);
                        }
                    }
                    break;

                case Command.PasteProperty:
                    {
                        transactionContext.DoTransaction(delegate
                        {
                            foreach (object item in m_editingContext.Items)
                            {
                                PropertyUtils.SetProperty(item, m_descriptor, m_copyValue);
                            }
                        },
                        string.Format("Paste: {0}".Localize("'Paste' is a verb and this is the name of a command"),
                            m_descriptor.DisplayName));
                    }
                    break;

                case Command.ResetProperty:
                    transactionContext.DoTransaction(delegate
                        {
                            PropertyUtils.ResetProperty(m_editingContext.Items, m_descriptor);
                        },
                        string.Format("Reset: {0}".Localize("'Reset' is a verb and this is the name of a command"),
                            m_descriptor.DisplayName));
                    break;               
                case Command.CopyAll:
                    {
                        m_descriptorToValue.Clear();
                        var lastObject = m_editingContext.Items.LastOrDefault();                      
                        foreach (var descriptor in m_editingContext.PropertyDescriptors)
                        {
                            if( (descriptor is ChildPropertyDescriptor))
                                continue;

                            AttributePropertyDescriptor attr = descriptor as AttributePropertyDescriptor;
                            if(attr != null && attr.AttributeInfo.IsIdAttribute)
                                continue;

                            m_descriptorToValue.Add(descriptor.GetPropertyDescriptorKey(),
                                descriptor.GetValue(lastObject));
                        }                        
                    }
                    break;

                case Command.PasteAll:
                    {
                        transactionContext.DoTransaction(delegate
                        {                            
                            foreach (var descriptor in m_editingContext.PropertyDescriptors)
                            {
                                if (descriptor.IsReadOnly) continue; ;
                                object value;
                                if (m_descriptorToValue.TryGetValue(descriptor.GetPropertyDescriptorKey(), out value))
                                {                                    
                                    foreach (object item in m_editingContext.Items)
                                    {
                                        PropertyUtils.SetProperty(item,
                                            descriptor,
                                            value);
                                    }
                                }
                            }

                        }, "Paste All".Localize("'Paste' is a verb and this is the name of a command"));                        
                    }
                    break;

                case Command.ResetAll:
                    transactionContext.DoTransaction(delegate
                    {
                        foreach (PropertyDescriptor descriptor in m_editingContext.PropertyDescriptors)
                        {
                            foreach (object item in m_editingContext.Items)
                            {
                                if (descriptor.CanResetValue(item))
                                    descriptor.ResetValue(item);
                            }
                        }
                    },
                        "Reset All Properties".Localize("'Reset' is a verb and this is the name of a command"));
                    break;
               
                case Command.ViewInTextEditor:
                    {
                        var fileUriEditor = m_descriptor.GetEditor(typeof(UITypeEditor)) as FileUriEditor;
                        var fileUri = m_descriptor.GetValue(m_editingContext.Items.LastOrDefault()) as Uri;
                        if (fileUri != null && File.Exists(fileUri.LocalPath))
                            Process.Start(fileUriEditor.AssociatedTextEditor, fileUri.LocalPath);
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update. See <see cref="CommandState"/></param>
        public virtual void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        /// <returns>Enumeration of command tags for context menu</returns>
        public IEnumerable<object> GetCommands(object context, object target)
        {
            m_editingContext = null;
            m_descriptor = null;
            PropertyDescriptor descriptor = target as PropertyDescriptor;

            if (context != null)
            {
                // first try to get a client-defined IPropertyEditingContext
                m_editingContext = context.As<IPropertyEditingContext>();
                if (m_editingContext == null)
                {
                    // otherwise, try to get a client-defined ISelectionContext and adapt it
                    ISelectionContext selectionContext = context.As<ISelectionContext>();
                    m_defaultContext.SelectionContext = selectionContext;
                    if (selectionContext != null)
                        m_editingContext = m_defaultContext;
                }                
            }

            if(m_editingContext != null)
            {
                 if (descriptor != null && m_editingContext.PropertyDescriptors.Contains(descriptor))
                 {
                    m_descriptor = descriptor;
                    yield return Command.CopyProperty;
                    yield return Command.PasteProperty;
                    yield return Command.ResetProperty;
                    yield return Command.CopyAll;
                    yield return Command.PasteAll;
                    yield return Command.ResetAll;
                    yield return Command.ViewInTextEditor;
                 }
                 else if(m_editingContext.Items.LastOrDefault() != null)
                 {
                    yield return Command.CopyAll;
                    yield return Command.PasteAll;
                    yield return Command.ResetAll;
                 }
            }
        }

        private bool CanPaste(object srcValue, 
            PropertyDescriptor srcDescriptor, 
            PropertyDescriptor destDescriptor, 
            object destValue)
        {            
            if (srcDescriptor == null
                || destDescriptor == null               
                || destDescriptor.IsReadOnly
                || srcDescriptor.PropertyType != destDescriptor.PropertyType
                || (destDescriptor is ChildAttributeCollectionPropertyDescriptor)
                || (destDescriptor is ChildPropertyDescriptor)) return false;

            if (destDescriptor.PropertyType.IsArray && destValue != null && srcValue != null)
            {
                Array srcArray = (Array)srcValue;
                Array destArray = (Array)destValue;

                if (srcArray.Rank != destArray.Rank) return false;
                for (int r = 0; r < srcArray.Rank; ++r)
                {
                    if (srcArray.GetLowerBound(r) != destArray.GetLowerBound(r) ||
                        srcArray.GetUpperBound(r) != destArray.GetUpperBound(r))
                    {
                        return false;
                    }
                }
            }

            
            return true;
        }

        private bool CanResetValue(IEnumerable<object> items, PropertyDescriptor descriptor)
        {
            if (descriptor != null && !descriptor.IsReadOnly)
            {
                foreach (object item in items)
                    if (descriptor.CanResetValue(item))
                        return true;
            }

            return false;
        }

        #endregion
        private enum Command
        {          
            CopyProperty,
            PasteProperty,
            ResetProperty,
            CopyAll,
            PasteAll,
            ResetAll,
            ViewInTextEditor, 
        }

        private IPropertyEditingContext m_editingContext;
        private PropertyDescriptor m_descriptor;

        // For copy/paste support from one property descriptor to another. This is different than
        //  copying and pasting to an edit box -- that uses the system clipboard.

        // used for copy/paste for single property.
        private PropertyDescriptor m_copyDescriptor;
        private object m_copyValue;

        // used for copy all and paste all.
        private readonly Dictionary<string, object>
            m_descriptorToValue = new Dictionary<string, object>();
        
        private readonly ICommandService m_commandService;
        private readonly SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();
    }
}