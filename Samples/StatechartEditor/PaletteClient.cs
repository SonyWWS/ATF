//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace StatechartEditorSample
{
    /// <summary>
    /// Component that populates the palette with the basic FSM types</summary>
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

        void IInitializable.Initialize()
        {
            string statechartCategory = Localizer.Localize("Statecharts");

            // add annotation type to palette
            NodeTypePaletteItem annotationItem = Schema.annotationType.Type.GetTag<NodeTypePaletteItem>();
            if (annotationItem != null)
                m_paletteService.AddItem(Schema.annotationType.Type, statechartCategory, this);

            // add state and pseudostate types to palette
            foreach (DomNodeType stateType in m_schemaLoader.GetNodeTypes(Schema.stateBaseType.Type))
            {
                NodeTypePaletteItem stateItem = stateType.GetTag<NodeTypePaletteItem>();
                if (stateItem != null)
                    m_paletteService.AddItem(stateType, statechartCategory, this);
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
            DomNodeType nodeType = (DomNodeType)item;
            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            if (paletteItem != null)
            {
                info.Label = paletteItem.Name;
                info.Description = paletteItem.Description;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
                info.HoverText = paletteItem.Description;
            }
        }

        /// <summary>
        /// Converts the palette item into an object that can be inserted into an
        /// IInstancingContext</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object that can be inserted into an IInstancingContext</returns>
        object IPaletteClient.Convert(object item)
        {
            DomNodeType nodeType = (DomNodeType)item;
            DomNode node = new DomNode(nodeType);

            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            AttributeInfo idAttribute = nodeType.IdAttribute;
            if (paletteItem != null &&
                idAttribute != null)
            {
                node.SetAttribute(idAttribute, paletteItem.Name);
            }
            return node;
        }

        #endregion

        private IPaletteService m_paletteService;
        private SchemaLoader m_schemaLoader;
    }
}
