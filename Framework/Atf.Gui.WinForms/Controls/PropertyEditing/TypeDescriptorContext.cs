//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Lightweight implementation of ITypeDescriptorContext, for convenient interaction
    /// with the System.ComponentModel framework</summary>
    public class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="next">Next IServiceProvider in the chain</param>
        public TypeDescriptorContext(
            object owner,
            PropertyDescriptor descriptor,
            IServiceProvider next)
        {
            m_owner = owner;
            m_descriptor = descriptor;
            m_next = next;
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type</summary>
        /// <param name="serviceType">An object that specifies the type of service object to get</param>
        /// <returns>A service object of type serviceType, or null if there is no service object of the given type</returns>
        public object GetService(Type serviceType)
        {
            if (typeof(IWindowsFormsEditorService).IsAssignableFrom(serviceType) ||
                typeof(ITypeDescriptorContext).IsAssignableFrom(serviceType))
                return this;

            if (m_next != null)
                return m_next.GetService(serviceType);

            return null;
        }

        #endregion

        #region ITypeDescriptorContext Members

        /// <summary>
        /// Gets the container representing this <see cref="T:System.ComponentModel.TypeDescriptor"></see> request</summary>
        /// <returns>An <see cref="T:System.ComponentModel.IContainer"></see> with the set of objects for this <see cref="T:System.ComponentModel.TypeDescriptor"></see>; otherwise, null if there is no container or if the <see cref="T:System.ComponentModel.TypeDescriptor"></see> does not use outside objects</returns>
        public IContainer Container
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the object that is connected with this type descriptor request</summary>
        /// <returns>The object that invokes the method on the <see cref="T:System.ComponentModel.TypeDescriptor"></see>; otherwise, null if there is no object responsible for the call</returns>
        public object Instance
        {
            get
            {
                ICustomTypeDescriptor customTypeDescriptor = m_owner as ICustomTypeDescriptor;
                return (customTypeDescriptor != null) ? customTypeDescriptor.GetPropertyOwner(m_descriptor) : m_owner;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanged"></see> event</summary>
        public void OnComponentChanged()
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanging"></see> event</summary>
        /// <returns><c>True</c> if this object can be changed</returns>
        public bool OnComponentChanging()
        {
            return true;
        }

        /// <summary>
        /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"></see> that is associated with the given context item</summary>
        /// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"></see> that describes the given context item; otherwise, null if there is no <see cref="T:System.ComponentModel.PropertyDescriptor"></see> responsible for the call</returns>
        public PropertyDescriptor PropertyDescriptor
        {
            get { return m_descriptor; }
        }

        #endregion

        private readonly object m_owner;
        private readonly PropertyDescriptor m_descriptor;
        private readonly IServiceProvider m_next;
    }
}
