//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Class to manage the global palette of objects that can be dragged onto other controls</summary>
    [Export(typeof(IPaletteService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(PaletteService))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class PaletteService : IPaletteService, IControlHostClient, IInitializable
    {
        [Import]
        private IControlHostService m_controlHostService = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by creating and registering PaletteContent control</summary>
        public void Initialize()
        {
            m_controlInfo = m_controlHostService.RegisterControl(
                new PaletteContent(this),
                "Palette".Localize(),
                "Creates new instances".Localize(),
                Sce.Atf.Applications.StandardControlGroup.Left,
                s_paletteControl.ToString(), this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(object control)
        {
        }

        /// <summary>
        /// Notifies the client that its control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(object control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <param name="mainWindowClosing"><c>True</c> if the application main window is closing</param>
        /// <returns><c>True</c> if the control can close, or false to cancel.</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService will call its own
        /// UnregisterContent. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this control.</remarks>
        public bool Close(object control, bool mainWindowClosing)
        {
            return true;
        }

        #endregion

        #region IPaletteService Members

        /// <summary>
        /// Adds an item to the palette in the given category</summary>
        /// <param name="item">Palette item</param>
        /// <param name="categoryName">Category name</param>
        /// <param name="client">Client that instantiates item during drag-drop operations</param>
        public void AddItem(object item, string categoryName, Sce.Atf.Applications.IPaletteClient client)
        {
            if (m_objectClients.ContainsKey(item))
                throw new InvalidOperationException("duplicate item");
            
            m_objectClients.Add(item, client);
            PaletteContent palette = m_controlInfo.Content as PaletteContent;
            if (palette != null)
            {
                palette.AddItem(item, categoryName ?? string.Empty);
            }
        }

        /// <summary>
        /// Removes an item from the palette</summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(object item)
        {
            PaletteContent palette = m_controlInfo.Content as PaletteContent;
            if (palette != null)
            {
                palette.RemoveItem(item);
            }
            
            m_objectClients.Remove(item);
        }

        #endregion

        /// <summary>
        /// Converts from palette items to actual items</summary>
        /// <param name="items">Items to convert</param>
        /// <returns>Enumeration of actual items, converted from palette items</returns>
        public IEnumerable<object> Convert(IEnumerable<object> items)
        {
            List<object> convertedItems = new List<object>();
            foreach (object item in items)
            {
                Sce.Atf.Applications.IPaletteClient client;
                if (m_objectClients.TryGetValue(item, out client))
                {
                    object convertedItem = client.Convert(item);
                    if (convertedItem != null)
                        convertedItems.Add(convertedItem);
                }
            }

            return convertedItems;
        }

        private IControlInfo m_controlInfo;
        private Dictionary<object, Sce.Atf.Applications.IPaletteClient> m_objectClients =
            new Dictionary<object, Sce.Atf.Applications.IPaletteClient>();
        private static Guid s_paletteControl = new Guid("266a2ae4-a801-482d-9dd7-a8ca33b6beea");
    }
    
}
