//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts DomNode to UI form, which holds a list of UI controls</summary>
    public class UIForm : UIObject
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the UI form's DomNode,
        /// getting list of children</summary>
        protected override void OnNodeSet()
        {
            m_UIControls = GetChildList<UIControl>(UISchema.UIControlType.ControlChild);
        }

        /// <summary>
        /// Gets the list of all Controls on the form</summary>
        public IList<UIControl> Controls
        {
            get { return m_UIControls; }
        }

        private IList<UIControl> m_UIControls;
    }
}
