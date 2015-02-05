//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Interaction logic for PropertyGridView.xaml.</summary>
    public partial class PropertyGridView : PropertyView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridView"/> class.</summary>
        public PropertyGridView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the data context for the view as an IPropertyEditingContext.</summary>
        public IPropertyEditingContext Context {
            get { return m_propertyGrid.DataContext.As<IPropertyEditingContext>(); }
            set 
            {
                // Reset the DataContext to force a refresh
                m_propertyGrid.DataContext = null;
                m_propertyGrid.DataContext = value; 
            }
        }

        /// <summary>
        /// Handles the Reloaded event of the observableContext and updates the view's data context.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void observableContext_Reloaded(object sender, EventArgs e)
        {
            base.observableContext_Reloaded(sender, e);
            Context = EditingContext;
        }

        private void m_propertyGrid_PropertyEdited(object sender, PropertyEditedEventArgs e)
        {
            var context = EditingContext.As<IHistoryContext>();
            if (context != null)
            {
                context.Dirty = true;
            }
        }

    }
}
