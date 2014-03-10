//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Interop
{
    internal class ControlHostClientAdapter : IControlHostClient
    {
        public ControlHostClientAdapter(Sce.Atf.Applications.IControlHostClient adaptee)
        {
            m_adaptee = adaptee;
        }

        private Sce.Atf.Applications.IControlHostClient m_adaptee;

        #region IControlHostClient Members

        public void Activate(object control)
        {
            m_adaptee.Activate((System.Windows.Forms.Control)control);

        }

        public void Deactivate(object control)
        {
            m_adaptee.Deactivate((System.Windows.Forms.Control)control);
        }

        public bool Close(object control, bool mainWindowClosing)
        {
            return m_adaptee.Close((System.Windows.Forms.Control)control);
        }

        #endregion
    }
}
