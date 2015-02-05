//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Provides control host services. 
    /// Class to adapt Sce.Atf.Wpf.Applications.IControlHostService to Sce.Atf.Applications.IControlHostService.
    /// This allows WinForms-based applications to be run in a WPF based application.</summary>
    [Export(typeof(Sce.Atf.Applications.IControlHostService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ControlHostServiceAdapter : Sce.Atf.Applications.IControlHostService
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptee">IControlHostService client</param>
        [ImportingConstructor]
        public ControlHostServiceAdapter(IControlHostService adaptee)
        {
            m_adaptee = adaptee;
        }

        private IControlHostService m_adaptee;

        #region Sce.Atf.Applications.IControlHostService Members

        /// <summary>
        /// Registers the control and adds it to a visible form</summary>
        /// <param name="control">Control</param>
        /// <param name="controlInfo">Control display information</param>
        /// <param name="client">Client that owns the control and receives notifications
        /// about its status, or null if no notifications are needed</param>
        /// <remarks>If IControlHostClient.Close() has been called, the IControlHostService
        /// also calls UnregisterControl. Call RegisterControl again to re-register the Control.</remarks>
        public void RegisterControl(System.Windows.Forms.Control control, Sce.Atf.Applications.ControlInfo controlInfo, Sce.Atf.Applications.IControlHostClient client)
        {
            var clientAdapter = GetOrCreateClientAdapter(client);

            // WPF control host service requires a unique control ID in order to be able to reload window layouts
            // on app startup.
            // This is a problem as we do not have one here.
            // The best we can do is try and generate a unique hash
            int uniqueId = GenerateId(control, controlInfo, client);

            IControlInfo contentInfo = m_adaptee.RegisterControl(control, controlInfo.Name, controlInfo.Description, controlInfo.Group, uniqueId.ToString(), clientAdapter);

            controlInfo.Control = control;

            m_info.Add(controlInfo, contentInfo);
            TransferControlInfoValues(controlInfo);
            controlInfo.Changed += (s, e) => TransferControlInfoValues(s as Sce.Atf.Applications.ControlInfo);
        }

        /// <summary>
        /// Unregisters the control and removes it from its containing form</summary>
        /// <param name="control">Control to be unregistered</param>
        /// <remarks>This method is called by IControlHostService after IControlHostClient.Close() is called.</remarks>
        public void UnregisterControl(System.Windows.Forms.Control control)
        {
            m_adaptee.UnregisterContent(control);

            var controlInfo = m_info.Keys.FirstOrDefault<Sce.Atf.Applications.ControlInfo>(x => x.Control == control);
            if (controlInfo != null)
            {
                controlInfo.Changed -= (s, e) => TransferControlInfoValues(s as Sce.Atf.Applications.ControlInfo);
                m_info.Remove(controlInfo);
            }
        }

        /// <summary>
        /// Makes a registered control visible</summary>
        /// <param name="control">Control to be made visible</param>
        public void Show(System.Windows.Forms.Control control)
        {
            m_adaptee.Show(control);
        }

        /// <summary>
        /// Gets the open controls, in order of least-recently-active to the active control</summary>
        /// <remarks>IControlRegistry has additional functionality related to the active control.</remarks>
        public IEnumerable<Sce.Atf.Applications.ControlInfo> Controls
        {
            get { return m_info.Keys; }
        }

        #endregion

        private ControlHostClientAdapter GetOrCreateClientAdapter(Sce.Atf.Applications.IControlHostClient client)
        {
            ControlHostClientAdapter adapter;
            if (!m_clientAdapters.TryGetValue(client, out adapter))
            {
                adapter = new ControlHostClientAdapter(client);
                m_clientAdapters.Add(client, adapter);
            }
            return adapter;
        }

        private int GenerateId(System.Windows.Forms.Control control, Atf.Applications.ControlInfo controlInfo, Atf.Applications.IControlHostClient client)
        {
            int clientType = client.GetType().Name.GetHashCode();
            int controlType = control.GetType().Name.GetHashCode();
            int controlName = controlInfo.Name.GetHashCode();
            return clientType ^ controlType ^ controlName;
        }

        private void TransferControlInfoValues(Sce.Atf.Applications.ControlInfo controlInfo)
        {
            IControlInfo contentInfo;
            if (m_info.TryGetValue(controlInfo, out contentInfo))
            {
                contentInfo.Name = controlInfo.Name;
                contentInfo.Description = controlInfo.Description;
                contentInfo.ImageSourceKey = controlInfo.Image;

                if (controlInfo.Image != null)
                {
                    // Creates a WPF image source resource and adds it to App resources
                    // with key or old image
                    Util.GetOrCreateResourceForEmbeddedImage(controlInfo.Image);
                }
            }
        }

        private Dictionary<Sce.Atf.Applications.IControlHostClient, ControlHostClientAdapter> m_clientAdapters
            = new Dictionary<Sce.Atf.Applications.IControlHostClient, ControlHostClientAdapter>();

        private Dictionary<Sce.Atf.Applications.ControlInfo, IControlInfo> m_info
            = new Dictionary<Sce.Atf.Applications.ControlInfo, IControlInfo>();
    }
}
