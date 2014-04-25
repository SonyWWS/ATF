//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to sub-circuit instance, which is an instance of a mastered SubCircuit</summary>
    public abstract class SubCircuitInstance : Element
    {
        /// <summary>
        /// Gets sub-circuit type attribute</summary>
        protected abstract AttributeInfo TypeAttribute { get; }
      
        /// <summary>
        /// Gets or sets the SubCircuit</summary>
        public SubCircuit SubCircuit
        {
            get { return GetReference<SubCircuit>(TypeAttribute); }
            set { SetReference(TypeAttribute, value); }
        }

        /// <summary>
        /// Gets the instance as a ICircuitElementType</summary>
        public override ICircuitElementType Type
        {
            get { return SubCircuit; }
        }

    }
}
