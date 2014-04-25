//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for a pin on a group module, with extra information needed to associate the pin
    /// on the group with the internal module where it was connected before grouping.
    /// A group pin is a pin on a grouped sub-circuit; it extends the information
    /// of a pin to preserve the internal pin/module which is connected to the outside circuit.</summary>
    public class GroupPin : Sce.Atf.Controls.Adaptable.Graphs.GroupPin, ICircuitGroupPin<Module>
    {
        // for bases class Pin
        /// <summary>
        /// Gets type attribute for group pin</summary>
        protected override AttributeInfo TypeAttribute
        {
            get { return Schema.pinType.typeAttribute; }
        }

        /// <summary>
        /// Gets name attribute for group pin</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.pinType.nameAttribute; }
        }

        /// <summary>
        /// Gets index (pin order in its sub-graph owner) attribute for group pin</summary>
        protected override AttributeInfo IndexAttribute
        {
            get { return Schema.groupPinType.indexAttribute; }
        }

        /// <summary>
        /// Gets floating y-coordinate attribute for group pin. 
        /// Floating pin location y value is user defined (x value is auto-generated).</summary>
        protected override AttributeInfo PinYAttribute
        {
            get { return Schema.groupPinType.pinYAttribute; }
        }

        /// <summary>
        /// Gets module (associated internal subelement) attribute for group pin</summary>
        protected override AttributeInfo ElementAttribute
        {
            get { return Schema.groupPinType.moduleAttribute; }
        }

        /// <summary>
        /// Gets pin (associated internal subpin) attribute for group pin</summary>
        protected override AttributeInfo PinAttribute
        {
            get { return Schema.groupPinType.pinAttribute; }
        }

        /// <summary>
        /// Gets pinned attribute for group pin</summary>
        protected override AttributeInfo PinnedAttribute
        {
            get { return Schema.groupPinType.pinnedAttribute; }
        }

        /// <summary>
        /// Gets visible attribute for group pin</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.groupPinType.visibleAttribute; }
        }

        #region ICircuitGroupPin members (needed for drawing)

        /// <summary>
        /// Gets the internal module corresponding to this group pin</summary>
        Module ICircuitGroupPin<Module>.InternalElement
        {
            get { return GetReference<Module>(Schema.groupPinType.moduleAttribute); }
        
        }    

        #endregion

       
    }
}
