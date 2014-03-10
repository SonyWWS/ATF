//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for an instance of a module template
    /// </summary>
    public class ModuleInstance : Module,IReference<Module>, IReference<DomNode>
    {
        #region IReference members
        public bool CanReference(Module item)
        {
            return true;
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        public Module Target
        {
            get { return GetReference<Module>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }

        bool IReference<DomNode>.CanReference(DomNode item)
        {
            return item.Is<Module>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        DomNode IReference<DomNode>.Target
        {
            get { return GetReference<DomNode>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }

        #endregion

 
        public override ICircuitElementType Type
        {
            get { return Target.Type; }
        }

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        private AttributeInfo RefAttribute
        {
            get { return Schema.moduleTemplateRefType.typeRefAttribute; }
        }

        // ICircuitElementType
        public Size InteriorSize
        {
            get { return Target.Type.InteriorSize; }
        }


        public Image Image
        {
            get { return Target.Type.Image; }
        }

        public IList<ICircuitPin> Inputs
        {
            get { return Target.Type.Inputs; }
        }

        public IList<ICircuitPin> Outputs
        {
            get { return Target.Type.Outputs; }
        }

        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            if (pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur
            var result = Target.Cast<Module>().MatchPinTarget(pinTarget, inputSide);
            if (result.First != null)
                result.First = this;
            return result;
        }

        public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            return MatchPinTarget(pinTarget, inputSide);
        }
    }
}
