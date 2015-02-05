//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts the circuit to IDocument and synchronizes URI and dirty bit changes to the
    /// ControlInfo instance used to register the viewing control in the UI</summary>
    public class CircuitDocument : Sce.Atf.Controls.Adaptable.Graphs.CircuitDocument
    {
        /// <summary>
        /// This property is obsolete as of ATF 3.9. It will probably be removed for ATF 3.10.</summary>
        protected override ChildInfo SubCircuitChildInfo
        {
            get { return Schema.circuitDocumentType.subCircuitChild; }
        }
    }
}
