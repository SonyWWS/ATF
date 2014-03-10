//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to a text item, which is a UIControl that renders text</summary>
    public class UITextItem : UIControl
    {
        /// <summary>
        /// Gets or sets the text item's font</summary>
        public UIFont Font
        {
            get 
            {
                UIRef uiRef = GetChild<UIRef>(UISchema.UITextItemType.FontChild);
                if (uiRef != null)
                    return uiRef.UIObject as UIFont;
                return null;
            }
            set
            {
                SetChild(UISchema.UITextItemType.FontChild, UIRef.New(value));
            }
        }
    }
}
