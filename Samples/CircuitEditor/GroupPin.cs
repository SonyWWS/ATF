//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class GroupPin : Sce.Atf.Controls.Adaptable.Graphs.GroupPin, ICircuitGroupPin<Module>
    {
        // for bases class Pin
        protected override AttributeInfo TypeAttribute
        {
            get { return Schema.pinType.typeAttribute; }
        }

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.pinType.nameAttribute; }
        }

        protected override AttributeInfo IndexAttribute
        {
            get { return Schema.groupPinType.indexAttribute; }
        }

        protected override AttributeInfo PinYAttribute
        {
            get { return Schema.groupPinType.pinYAttribute; }
        }

        protected override AttributeInfo ElementAttribute
        {
            get { return Schema.groupPinType.moduleAttribute; }
        }

        protected override AttributeInfo PinAttribute
        {
            get { return Schema.groupPinType.pinAttribute; }
        }

        protected override AttributeInfo PinnedAttribute
        {
            get { return Schema.groupPinType.pinnedAttribute; }
        }

        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.groupPinType.visibleAttribute; }
        }

        #region ICircuitGroupPin memebers(needed for drawing)

        /// <summary>
        /// Gets the internal module corresponding to this group pin</summary>
        Module ICircuitGroupPin<Module>.InternalElement
        {
            get { return GetReference<Module>(Schema.groupPinType.moduleAttribute); }
        
        }    

        #endregion

       
    }
}
