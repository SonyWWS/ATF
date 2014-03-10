//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;

namespace StatechartEditorSample
{
    /// <summary>
    /// DomNode adapter for history pseudo-state</summary>
    public class HistoryState : StateBase
    {
        /// <summary>
        /// Gets the history type, shallow or deep</summary>
        public override StateType Type
        {
            get
            {
                string type = (string)DomNode.GetAttribute(Schema.historyStateType.typeAttribute);
                if (string.Compare(type, "deep", true) == 0)
                    return StateType.DeepHistory;

                return StateType.ShallowHistory;
            }
        }
    }
}
