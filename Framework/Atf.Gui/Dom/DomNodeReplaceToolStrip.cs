//Sony Computer Entertainment Confidential

using System;
using System.Windows.Forms;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Toolstrip GUI for specifying a replacement pattern on DomNode parameter value matches</summary>
    public class DomNodeReplaceToolStrip : ReplaceToolStrip
    {
        /// <summary>
        /// Constructor</summary>
		public DomNodeReplaceToolStrip()
        {
            // Define query tree, which defines the layout of the search toolstrip GUI
            m_rootNode = new DomNodeQueryRoot();
            QueryTree.AddLabel(m_rootNode, "Replace with");

			m_replaceTextInput = QueryTree.AddReplaceTextInput(m_rootNode, null, false);
            QueryTree.AddSeparator(m_rootNode);
            m_rootNode.RegisterReplaceButtonPress(QueryTree.AddButton(m_rootNode, "Replace"));

            // Entering text into the toolstrip will trigger a search, changing an option rebuilds the toolstrip GUI
            m_rootNode.ReplaceTextEntered += replaceSubStrip_ReplaceTextEntered;

            // 
            // Build toolStrip GUI by retrieving toolstrip item list from tree, and adding 
            // them to ToolStrip.Items
            // 
            SuspendLayout();
            List<ToolStripItem> toolStripItems = new List<ToolStripItem>();
            m_rootNode.GetToolStripItems(toolStripItems);
            Items.AddRange(toolStripItems.ToArray());

            // Initialize ToolStrip
            Location = new System.Drawing.Point(0, 0);
            Name = "Event Sequence Document Replace";
            Size = new System.Drawing.Size(292, 25);
            TabIndex = 0;
            Text = "Event Sequence Document Replace";
			GripStyle = ToolStripGripStyle.Hidden;

            // Done
			ResumeLayout(false);

			if (UIChanged != null) { }
		}

        /// <summary>
        /// Event that is raised after UI has changed</summary>
		public override event EventHandler UIChanged;

		/// <summary>
        /// Event handler called when user has entered text into the toolstrip</summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void replaceSubStrip_ReplaceTextEntered(object sender, System.EventArgs e)
        {
			DoReplace();
        }

		/// <summary>
		/// Returns an object that defines how search matches will be modified in replacement</summary>
		public override object GetReplaceInfo()
		{
			return m_replaceTextInput.InputText;
		}

		private DomNodeQueryRoot m_rootNode;
		private QueryTextInput m_replaceTextInput;
    }
}

