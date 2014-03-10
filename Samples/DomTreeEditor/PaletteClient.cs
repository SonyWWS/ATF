//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Component that populates the palette with the basic UI types</summary>
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
            // add item types to palette
            string name = Localizer.Localize("UI Objects");
            m_paletteService.AddItem(UISchema.UIPackageType.Type, name, this);
            m_paletteService.AddItem(UISchema.UIFormType.Type, name, this);
            m_paletteService.AddItem(UISchema.UIShaderType.Type, name, this);
            m_paletteService.AddItem(UISchema.UITextureType.Type, name, this);
            m_paletteService.AddItem(UISchema.UIFontType.Type, name, this);
            m_paletteService.AddItem(UISchema.UISpriteType.Type, name, this);
            m_paletteService.AddItem(UISchema.UITextItemType.Type, name, this);
            m_paletteService.AddItem(UISchema.UIAnimationType.Type, name, this);
        }

        #endregion

        #region IPaletteClient Members

        /// <summary>
        /// Gets display information for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Information object, which client can fill out</param>
        public void GetInfo(object item, ItemInfo info)
        {
            DomNodeType nodeType = (DomNodeType)item;
            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            if (paletteItem != null)
            {
                info.Label = paletteItem.Name;
                info.Description = paletteItem.Description;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
            }
        }

        /// <summary>
        /// Converts the palette item into an object that can be inserted into an
        /// IInstancingContext</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Object that can be inserted into an IInstancingContext</returns>
        public object Convert(object item)
        {
            DomNodeType nodeType = (DomNodeType)item;
            DomNode node = new DomNode(nodeType);
            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            if (paletteItem != null)
            {
                node.SetAttribute(nodeType.IdAttribute, paletteItem.Name);
            }            
            return node;
        }

        #endregion

        private IPaletteService m_paletteService;
    }
}
