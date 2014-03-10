//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to a UI Reference, which holds a reference to a UIObject. A separate DomNode to hold
    /// a reference is necessary to represent lists of references.</summary>
    public class UIRef : DomNodeAdapter
    {
        /// <summary>
        /// Creates a new UIRef adapter and underlying DomNode</summary>
        /// <param name="uiObj">UIObject being referenced</param>
        /// <returns>New UIRef adapter</returns>
        public static UIRef New(UIObject uiObj)
        {
            DomNode node = new DomNode(UISchema.UIRefType.Type);
            UIRef uiRef = node.As<UIRef>();
            uiRef.UIObject = uiObj;
            return uiRef;
        }

        /// <summary>
        /// Gets or sets the referenced UI object</summary>
        public UIObject UIObject
        {
            get { return GetReference<UIObject>(UISchema.UIRefType.refAttribute); }
            set { SetReference(UISchema.UIRefType.refAttribute, value); }
        }
    }
}
