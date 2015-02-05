//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Wpf.Applications;

using IPaletteService = Sce.Atf.Wpf.Applications.IPaletteService;
using NodeTypePaletteItem = Sce.Atf.Wpf.Models.NodeTypePaletteItem;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// Component that populates the palette with the basic DOM types</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(PaletteClient))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PaletteClient : IPaletteClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="paletteService">Palette service</param>
        /// <param name="schemaLoader">Schema loader</param>
        [ImportingConstructor]
        public PaletteClient(
            IPaletteService paletteService,
            SchemaLoader schemaLoader)
        {
            m_paletteService = paletteService;
            m_schemaLoader = schemaLoader;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding event and resource items to palette</summary>
        void IInitializable.Initialize()
        {
            NodeTypePaletteItem eventTag = Schema.eventType.Type.GetTag<NodeTypePaletteItem>();
            if (eventTag != null)
                m_paletteService.AddItem(eventTag, eventTag.Category, this);

            foreach (DomNodeType resourceType in m_schemaLoader.GetNodeTypes(Schema.resourceType.Type))
            {
                NodeTypePaletteItem resourceTag = resourceType.GetTag<NodeTypePaletteItem>();
                if (resourceTag != null)
                    m_paletteService.AddItem(resourceTag, resourceTag.Category, this);
            }
        }

        #endregion

        #region IPaletteClient Members

        /// <summary>
        /// Gets display information for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Information object, which client can fill out</param>
        void IPaletteClient.GetInfo(object item, ItemInfo info)
        {
            NodeTypePaletteItem paletteItem = item.As<NodeTypePaletteItem>();
            if (paletteItem != null)
            {
                info.Label = paletteItem.Name;
                info.Description = paletteItem.Description;
            }
        }

        /// <summary>
        /// Converts the palette item into an object that can be inserted into an
        /// IInstancingContext</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object that can be inserted into an IInstancingContext</returns>
        object IPaletteClient.Convert(object item)
        {
            DomNode node = null;
            NodeTypePaletteItem paletteItem = item.As<NodeTypePaletteItem>();
            if (paletteItem != null)
            {
                DomNodeType nodeType = paletteItem.NodeType;
                node = new DomNode(nodeType);

                if (nodeType.IdAttribute != null)
                    node.SetAttribute(nodeType.IdAttribute, paletteItem.Name); // unique id, for referencing

                if (nodeType == Schema.eventType.Type)
                    node.SetAttribute(Schema.eventType.nameAttribute, paletteItem.Name);
                else if (Schema.resourceType.Type.IsAssignableFrom(nodeType))
                    node.SetAttribute(Schema.resourceType.nameAttribute, paletteItem.Name);
            }
            return node;
        }

        #endregion

        private IPaletteService m_paletteService;
        private SchemaLoader m_schemaLoader;
    }
}
