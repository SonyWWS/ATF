//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Keys = Sce.Atf.Input.Keys;

namespace CircuitEditorSample
{
    /// <summary>
    /// Provides context commands in a property editor for adding, editing, and
    /// removing dynamic properties on objects.</summary>
    /// <remarks>The implementation was originally copied from PropertyEditingCommands.</remarks>
    [Export(typeof (DynamicPropertyCommands))]
    [Export(typeof (IInitializable))]
    [Export(typeof (IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DynamicPropertyCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service used to add context menu commands Reset Current and Reset All</param>
        [ImportingConstructor]
        public DynamicPropertyCommands(ICommandService commandService)
        {
            CommandService = commandService;
        }

        /// <summary>
        /// The command enums, to be used in CanDoCommand() and DoCommand()</summary>
        public enum Command
        {
            Add,
            Edit,
            Remove
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            CommandService.RegisterCommand(
                Command.Add,
                null,
                null,
                "Add Property".Localize(""),
                "Adds a new dynamic property".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);

            CommandService.RegisterCommand(
                Command.Edit,
                null,
                null,
                "Edit Property".Localize(""),
                "Edits the dynamic property".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);

            CommandService.RegisterCommand(
                Command.Remove,
                null,
                null,
                "Remove Property".Localize(""),
                "Removes the dynamic property".Localize(),
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
        /// <returns>True if client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            if (commandTag is Command && EditingContext != null)
            {
                switch ((Command) commandTag)
                {
                    case Command.Add:
                        return EditingContext.Items.Any<Module>() &&
                            EditingContext.Items.All<Module>();

                    case Command.Edit:
                    case Command.Remove:
                        return Descriptor != null &&
                            Descriptor.Path.Last().IsEquivalent(Schema.moduleType.dynamicPropertyChild);
                }
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            switch ((Command) commandTag)
            {
                case Command.Add:
                    AddNewProperty();
                    break;

                case Command.Edit:
                    EditProperty();
                    break;

                case Command.Remove:
                    RemoveProperty();
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

        protected virtual void AddNewProperty()
        {
            using (var form = new DynamicPropertyForm())
            {
                do
                {
                    form.ShowDialog();
                    if (form.DialogResult == DialogResult.OK)
                        ApplyDialogResults(form, true);
                } while (form.DialogResult == DialogResult.Retry);
            }
        }

        protected virtual void EditProperty()
        {
            using (var form = new DynamicPropertyForm())
            {
                form.Text = "Edit Dynamic Property".Localize();
                form.PropertyName.Text = Descriptor.DisplayName;
                form.Category.Text = Descriptor.Category;
                form.Description.Text = Descriptor.Description;

                string valueType = Descriptor.AttributeInfo.Name;
                if (valueType == "stringValue")
                    form.StringType.Checked = true;
                else if (valueType == "floatValue")
                    form.FloatingPointType.Checked = true;
                else if (valueType == "vector3Value")
                    form.VectorType.Checked = true;
                else if (valueType == "boolValue")
                    form.BooleanType.Checked = true;
                else if (valueType == "intValue")
                    form.IntegerType.Checked = true;
                else
                    throw new InvalidOperationException("Unknown type of dynamic property: " +
                        valueType);

                // Don't let the user change the type of the dynamic property; this currently
                //  can cause a crash because PropertyView will use the old UI property editor
                //  which won't be compatible with the new value.
                form.StringType.Enabled = false;
                form.FloatingPointType.Enabled = false;
                form.VectorType.Enabled = false;
                form.BooleanType.Enabled = false;
                form.IntegerType.Enabled = false;
                
                do
                {

                    form.ShowDialog();
                    if (form.DialogResult == DialogResult.OK)
                        ApplyDialogResults(form, false);
                } while (form.DialogResult == DialogResult.Retry);
            }
        }

        protected virtual void ApplyDialogResults(DynamicPropertyForm form, bool add)
        {
            string propertyName = form.PropertyName.Text;

            if (string.IsNullOrEmpty(propertyName))
            {
                MessageBox.Show("The property name can't be empty".Localize(), "", MessageBoxButtons.OK);
                form.DialogResult = DialogResult.Retry;
                return;
            }

            string description = form.Description.Text;
            string category = form.Category.Text;
            bool success;

            if (form.StringType.Checked)
            {
                success = AddOrSetDynamicPropertyDomNode(
                    propertyName,
                    category,
                    description,
                    "", //type converter
                    "", //UI editor
                    Schema.dynamicPropertyType.stringValueAttribute.Name,
                    add);
            }
            else if (form.FloatingPointType.Checked)
            {
                success = AddOrSetDynamicPropertyDomNode(
                    propertyName,
                    category,
                    description,
                    "", //type converter
                    "", //UI editor
                    Schema.dynamicPropertyType.floatValueAttribute.Name,
                    add);
            }
            else if (form.IntegerType.Checked)
            {
                success = AddOrSetDynamicPropertyDomNode(
                    propertyName,
                    category,
                    description,
                    "", //type converter
                    "", //UI editor
                    Schema.dynamicPropertyType.intValueAttribute.Name,
                    add);
            }
            else if (form.BooleanType.Checked)
            {
                success = AddOrSetDynamicPropertyDomNode(
                    propertyName,
                    category,
                    description,
                    "", //type converter
                    "Sce.Atf.Controls.PropertyEditing.BoolEditor, Atf.Gui.WinForms", //UI editor
                    Schema.dynamicPropertyType.boolValueAttribute.Name,
                    add);
            }
            else if (form.VectorType.Checked)
            {
                success = AddOrSetDynamicPropertyDomNode(
                    propertyName,
                    category,
                    description,
                    "Sce.Atf.Controls.PropertyEditing.FloatArrayConverter, Atf.Gui", //type converter
                    "Sce.Atf.Controls.PropertyEditing.NumericTupleEditor, Atf.Gui.WinForms:System.Single,x,y,z", //UI editor
                    Schema.dynamicPropertyType.vector3ValueAttribute.Name,
                    add);
            }
            else
                throw new InvalidOperationException("A radio button wasn't handled");

            if (!success)
                form.DialogResult = DialogResult.Retry;
        }

        /// <summary>
        /// Adds a child DomNode representing a user-defined dynamic property to each of the currently selected objects,
        /// or updates the existing dynamic properties.</summary>
        /// <param name="propertyName">The user-defined dynamic property's name that is displayed in a property editor</param>
        /// <param name="category">The optional property category name</param>
        /// <param name="description">The optional description of the user-defined dynamic property</param>
        /// <param name="converter">e.g., "Sce.Atf.Controls.PropertyEditing.FloatArrayConverter, Atf.Gui"</param>
        /// <param name="editor">e.g., "Sce.Atf.Controls.PropertyEditing.NumericTupleEditor, Atf.Gui.WinForms:System.Single,x,y,z"</param>
        /// <param name="valueType">e.g., Schema.customAttributeType.stringValueAttribute.Name</param>
        /// <param name="add">Whether to add a new dynamic property or to update an existing one</param>
        /// <returns>True if successful and false if there was a problem with the input, in which case no
        /// changes were made to the DOM.</returns>
        protected virtual bool AddOrSetDynamicPropertyDomNode(string propertyName, string category, string description,
            string converter, string editor, string valueType, bool add)
        {
            // Check for duplicate dynamic properties.
            foreach (Module module in EditingContext.Items.AsIEnumerable<Module>())
            {
                if (!add)
                {
                    DomNode node = GetDynamicDomNode(module);
                    if (node == module.DomNode)
                        continue;
                }

                foreach (DomNode childNode in module.DomNode.GetChildren(Schema.moduleType.dynamicPropertyChild))
                {
                    // Copies the logic of PropertyUtil.PropertyDescriptorsEqual()
                    if ((string) childNode.GetAttribute(Schema.dynamicPropertyType.nameAttribute) == propertyName &&
                        (string) childNode.GetAttribute(Schema.dynamicPropertyType.categoryAttribute) == category &&
                        (string) childNode.GetAttribute(Schema.dynamicPropertyType.valueTypeAttribute) == valueType)
                    {
                        MessageBox.Show("This dynamic property is a duplicate of another dynamic property on the same object".Localize());
                        return false;
                    }
                }
            }

            // Construct the command name for the undo/redo menu.
            string commandName = add
                ? string.Format("Add: {0}".Localize(), category + '/' + propertyName)
                : string.Format("Edit: {0}".Localize(), category + '/' + propertyName);

            // Make the DOM changes.
            ITransactionContext transactionContext = EditingContext.Cast<ITransactionContext>();
            bool success = transactionContext.DoTransaction(delegate
            {
                foreach (Module module in EditingContext.Items.AsIEnumerable<Module>())
                {
                    DomNode node;
                    if (add)
                        node = new DomNode(Schema.dynamicPropertyType.Type, Schema.moduleType.dynamicPropertyChild);
                    else
                        node = GetDynamicDomNode(module);
                        
                    SetDynamicPropertyDomNode(propertyName, category, description, converter, editor, valueType, node);

                    if (add)
                        module.DomNode.GetChildList(Schema.moduleType.dynamicPropertyChild).Add(node);
                }
            }, commandName);
            
            return success;
        }

        protected virtual void SetDynamicPropertyDomNode(string propertyName, string category, string description,
            string converter, string editor, string valueType, DomNode node)
        {
            node.SetAttribute(Schema.dynamicPropertyType.nameAttribute, propertyName);
            node.SetAttribute(Schema.dynamicPropertyType.categoryAttribute, category);
            node.SetAttribute(Schema.dynamicPropertyType.descriptionAttribute, description);
            node.SetAttribute(Schema.dynamicPropertyType.converterAttribute, converter);
            node.SetAttribute(Schema.dynamicPropertyType.editorAttribute, editor);
            node.SetAttribute(Schema.dynamicPropertyType.valueTypeAttribute, valueType);
        }

        protected virtual bool RemoveProperty()
        {
            ITransactionContext transactionContext = EditingContext.Cast<ITransactionContext>();
            bool success = transactionContext.DoTransaction(delegate
            {
                foreach (Module module in EditingContext.Items.AsIEnumerable<Module>())
                {
                    DomNode node = GetDynamicDomNode(module);
                    node.RemoveFromParent();
                }
            }, string.Format("Remove: {0}".Localize(), Descriptor.Category + '/' + Descriptor.DisplayName));
            
            return success;
        }

        protected DomNode GetDynamicDomNode(Module module)
        {
            ChildAttributePropertyDescriptor dynamicPropertyDescriptor = Descriptor;
            if (m_multiPropertyDescriptor != null)
            {
                dynamicPropertyDescriptor = (ChildAttributePropertyDescriptor)
                    m_multiPropertyDescriptor.FindDescriptor(module);
            }

            if (dynamicPropertyDescriptor != null)
                return dynamicPropertyDescriptor.GetNode(module);
            return null;
        }

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        /// <returns>Enumeration of command tags for context menu</returns>
        public IEnumerable<object> GetCommands(object context, object target)
        {
            EditingContext = context.As<IPropertyEditingContext>(); //ok if context is null
            Descriptor = target as ChildAttributePropertyDescriptor;
            m_multiPropertyDescriptor = null;
            
            // Check for MultiPropertyDescriptor in case multiple objects are selected.
            if (Descriptor == null)
            {
                var multi = target as MultiPropertyDescriptor;
                if (multi != null)
                {
                    bool allAreGood = true;
                    ChildAttributePropertyDescriptor oneDynamic = null;
                    foreach (var single in multi.GetDescriptors())
                    {
                        oneDynamic = single as ChildAttributePropertyDescriptor;
                        if (oneDynamic == null)
                        {
                            allAreGood = false;
                            break;
                        }
                    }
                    if (allAreGood && oneDynamic != null)
                    {
                        Descriptor = oneDynamic;
                        m_multiPropertyDescriptor = multi;
                    }
                }
            }

            // Try to get a client-defined IPropertyEditingContext.
            if (EditingContext == null)
            {
                // Otherwise, try to get a client-defined ISelectionContext and adapt it.
                ISelectionContext selectionContext = context.As<ISelectionContext>();
                DefaultContext.SelectionContext = selectionContext;
                if (selectionContext != null)
                    EditingContext = DefaultContext;
            }

            if (CanDoCommand(Command.Add))
                yield return Command.Add;
            if (CanDoCommand(Command.Edit))
                yield return Command.Edit;
            if (CanDoCommand(Command.Remove))
                yield return Command.Remove;
        }

        #endregion

        protected IPropertyEditingContext EditingContext { get; private set; }
        protected ChildAttributePropertyDescriptor Descriptor { get; private set; }

        protected readonly ICommandService CommandService;
        protected readonly SelectionPropertyEditingContext DefaultContext = new SelectionPropertyEditingContext();
        private MultiPropertyDescriptor m_multiPropertyDescriptor;
    }
}
