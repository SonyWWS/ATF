//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Command to set a property on an object</summary>
    public class SetPropertyCommand : Command
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="component">Object holding property</param>
        /// <param name="propertyName">Property name</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        public SetPropertyCommand(
            string commandName,
            object component,
            string propertyName,
            object oldValue,
            object newValue)

            : base(commandName)
        {
            m_component = component;
            m_descriptor = TypeDescriptor.GetProperties(component)[propertyName];
            m_oldValue = oldValue;
            m_newValue = newValue;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="component">Object holding property</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="oldValue">Old property value</param>
        /// <param name="newValue">New property value</param>
        public SetPropertyCommand(
            string commandName,
            object component,
            PropertyDescriptor descriptor,
            object oldValue,
            object newValue)

            : base(commandName)
        {
            m_component = component;
            m_descriptor = descriptor;
            m_oldValue = oldValue;
            m_newValue = newValue;
        }

        /// <summary>
        /// Does/Redoes the command</summary>
        public override void Do()
        {
            m_descriptor.SetValue(m_component, m_newValue);
        }

        /// <summary>
        /// Undoes the command</summary>
        public override void Undo()
        {
            m_descriptor.SetValue(m_component, m_oldValue);
        }

        private readonly object m_component;
        private readonly PropertyDescriptor m_descriptor;
        private readonly object m_oldValue;
        private readonly object m_newValue;
    }
}
