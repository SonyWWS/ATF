//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

using Sce.Atf;
using Sce.Atf.Wpf.Controls.PropertyEditing;

namespace WpfPropertyEditor
{
    class PropertyFactory : IPropertyFactory
    {
        public PropertyNode CreateProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable,
            ITransactionContext context)
        {
            var node = new PropertyNode();
            node.Initialize(instance, descriptor, isEnumerable);

            // set up value editors
            if (descriptor.Name == "Price" && descriptor.PropertyType == typeof (float))
            {
                var items = (object[])instance;
                var item = (Suv)items[0];
                var valueEditor = new SliderValueEditor();
                valueEditor.SetRange(item.Min, item.Max);
                node.SetCustomEditor(valueEditor);
            }

            return node;
        }
    }
}
