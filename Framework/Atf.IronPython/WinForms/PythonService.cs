//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Python service that provides a dockable command console for entering Python commands
    /// and imports many common .NET and ATF types into the Python namespace. If no command
    /// console is needed, then use the BasicPythonService MEF component.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ScriptingService))]
    [Export(typeof(PythonService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PythonService : BasicPythonService, IInitializable
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // If the ScriptConsole is already a MEF component, then we don't need to do anything since it will
            //  initialize itself. But if no ScriptConsole is present, we need to add one.
            if (m_controlHostService != null && m_scriptConsole == null)
            {
                m_scriptConsole = new ScriptConsole(this, m_controlHostService);
            }
        }

        #endregion

        /// <summary>
        /// Gets the underlying Control that should be registered with a control host service</summary>
        protected Control Control
        {
            get { return m_scriptConsole != null ? m_scriptConsole.Control : null; }
        }


#pragma warning disable 649 // Field is never assigned to and will always have its default value

        [Import(AllowDefault = true)]
        private ScriptConsole m_scriptConsole;

        [Import(AllowDefault = true)]
        private IControlHostService m_controlHostService;

#pragma warning restore 649
    }
}
