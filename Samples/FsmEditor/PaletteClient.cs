//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace FsmEditorSample
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
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            string category = "FSM";
            // add state type to palette
            DomNodeType stateType = Schema.stateType.Type;
            NodeTypePaletteItem paletteItem = stateType.GetTag<NodeTypePaletteItem>();
            if (paletteItem != null)
                m_paletteService.AddItem(stateType, category, this);

            // add annotation type to palette
            NodeTypePaletteItem annotationItem = Schema.annotationType.Type.GetTag<NodeTypePaletteItem>();
            if (annotationItem != null)
                m_paletteService.AddItem(Schema.annotationType.Type, category, this);
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
            if (paletteItem != null)
            {
                if (nodeType.IdAttribute != null)
                    node.SetAttribute(nodeType.IdAttribute, paletteItem.Name); // unique id, for referencing

                if (nodeType == Schema.stateType.Type)
                    node.SetAttribute(Schema.stateType.labelAttribute, paletteItem.Name);   // user visible name for state
            }
            return node;
        }

        #endregion

        private IPaletteService m_paletteService;
    }
}
