using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Nested collections editor</summary>
    public class NestedCollectionEditor: UITypeEditor, IPropertyEditor
    {
        /// <summary>
        /// Delegate for receiving the CollectionChanged event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="instance">Changed instance</param>
        /// <param name="value">New value</param>
        public delegate void CollectionChangedEventHandler(object sender, object instance, object value);
        /// <summary>
        /// Event that is raised after the collection is changed by the user</summary>
        public event CollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Event that is raised before the collection editor is opened</summary>
        public event EventHandler<EditingEventArgs> EditorOpening;


        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public Control GetEditingControl(PropertyEditorControlContext context)
        {
            // try to hold selection context here for transaction support in property editing
            m_selectionContext = context.TransactionContext.As<ISelectionContext>(); 
            return null;
        }

        /// <summary>
        /// Callback when creating a collection object</summary>
        /// <param name="parentProperty"></param>
        /// <returns>The object to be added to the collection when, for example, the user clicks the Add
        /// button on the NestedCollectionEditorForm.</returns>
        public delegate object CreateCollectionObject(object parentProperty);

        /// <summary>
        /// Event data for editing the collection</summary>
        public class  EditingEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="value">Collection to edit</param>
            public EditingEventArgs(object value)
            {
                Value = value;
            }

            /// <summary>
            /// The collection value</summary>
            public readonly object Value;
            /// <summary>
            /// Delegate for getting item's display information</summary>
            public Func<object, ItemInfo, bool> GetItemInfo;
            /// <summary>
            /// Delegate for getting available types and constructor arguments (object []) to create and add to this collection and its sub-collections</summary>
            public Func<Path<object>, IEnumerable<Pair<Type, CreateCollectionObject>>> GetCollectionItemCreators; 
        }


        /// <summary>
        /// Edits the specified object value using the editor style provided by GetEditorStyle.
        /// A service provider is provided so that any required editing services can be obtained.</summary>
        /// <param name="context">A type descriptor context that can be used to provide additional context information</param>
        /// <param name="provider">A service provider object through which editing services may be obtained</param>
        /// <param name="value">An instance of the value being edited</param>
        /// <returns>The new value of the object. If the value of the object hasn't changed,
        /// this should return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) 
        {
            if (context != null    && context.Instance != null    && provider != null) 
            {
                IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

                if (editorService != null) 
                {
                    EditingEventArgs e = new EditingEventArgs(value);
                    OnEditorOpening(e);
                    NestedCollectionEditorForm collEditorFrm = CreateForm(context, m_selectionContext, value, e.GetCollectionItemCreators, e.GetItemInfo);
         
                    context.OnComponentChanging();
                    
 
                    if (editorService.ShowDialog(collEditorFrm) == DialogResult.OK)
                    {
                        OnCollectionChanged(context.Instance, value);
                        context.OnComponentChanged();
                    }                                                
                }
            }

            return value;
        }


        /// <summary>
        /// Gets the editing style of the Edit method. If the method is not supported, this returns <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"></see>.</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <returns>An enum value indicating the provided editing style</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
        {
            if (context != null && context.Instance != null) 
            {
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(context);
        }

        /// <summary>
        /// Raises the EditorOpening event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnEditorOpening(EditingEventArgs e)
        {
            EditorOpening.Raise(this, e);
        }

        /// <summary>
        /// Calls the CollectionChanged event handler if it exists</summary>
        /// <param name="instance">Instance that changed</param>
        /// <param name="value">Original value of instance being edited</param>
        protected virtual void OnCollectionChanged(object instance, object value)
        {
            if(CollectionChanged !=null)
            {
                CollectionChanged(this, instance,value);
            }
        }

        /// <summary>
        /// Creates NestedCollectionEditorForm</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <param name="selectionContext">Selection context</param>
        /// <param name="value">An instance of the value being edited</param>
        /// <param name="getCollectionItemCreators">Callback for getting available types and constructor arguments (object []) 
        /// to create and add to this collection and its sub-collections</param>
        /// <param name="getItemInfo">Callback for getting item's display information</param>
        /// <returns>NestedCollectionEditorForm</returns>
        protected virtual NestedCollectionEditorForm CreateForm(ITypeDescriptorContext context, ISelectionContext  selectionContext, object value, Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> getCollectionItemCreators, Func<object, ItemInfo, bool> getItemInfo)
        {
            return new NestedCollectionEditorForm(context, selectionContext, value, getCollectionItemCreators, getItemInfo);
        }

        private ISelectionContext m_selectionContext;
    }
}
