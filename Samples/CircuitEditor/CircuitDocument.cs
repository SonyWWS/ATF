//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class CircuitDocument : Sce.Atf.Controls.Adaptable.Graphs.CircuitDocument
    {
        protected override ChildInfo SubCircuitChildInfo
        {
            get { return Schema.circuitDocumentType.subCircuitChild; }
        }
    }
}
