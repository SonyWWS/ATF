//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// DomNodeAdapter which adds support for data binding to adapters obtained
    /// through the IAdaptable interface.</summary>
    /// <remarks>
    /// This DomNodeAdapter exposes a single property which can be bound to using, for example,
    /// a WPF binding.  This property returns a custom type descriptor object which returns a
    /// property descriptor for each adaptable interface found on the DomNode.
    /// 
    /// For example, to bind to an adaptable DomDocument on this DomNode, the XAML
    /// binding could look like this:
    /// 
    /// {Binding Path=As.DomDocument.IsDirty}
    /// 
    /// A limitation of this technique is that only the simple unqualified name of the adaptable
    /// types is used for the property path. (in this case DomDocument).  This could result in clashes
    /// if the DomNode has multiple similarly named adapters.</remarks>
    public class BindingAdapter : ObservableDomNodeAdapter
    {
        /// <summary>
        /// Bindable adapter object</summary>
        public object As
        {
            get
            {
                return m_adapterObject
                       ??
                       (m_adapterObject = new DomBindingAdapterObject(DomNode, s_enableOptimisation));
            }
        }

        /// <summary>
        /// Setting this flag to true will improve performance of the BindingAdapter
        /// by making the assumption that all DomNodes of a given DomNodeType will 
        /// always return the same set of adapter types
        /// Default value is True.</summary>
        public static bool EnableNodeTypeExtensionOptimisation
        {
            get { return s_enableOptimisation; }
            set { s_enableOptimisation = value; }
        }

        private Sce.Atf.Adaptation.BindingAdapterObject m_adapterObject;

        private static bool s_enableOptimisation = true;
    }
}
