//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// ToolStrip GUI for specifying a replacement pattern on DomNode parameter value matches</summary>
    public class DomNodeReplaceToolStrip : ReplaceToolStrip
    {
        /// <summary>
        /// Constructor</summary>
        public DomNodeReplaceToolStrip()
        {
            // Define query tree, which defines the layout of the search toolstrip GUI
            m_rootNode = new DomNodeQueryRoot();
            m_rootNode.AddLabel("Replace with");

            m_replaceTextInput = m_rootNode.AddReplaceTextInput(null, false);
            m_rootNode.AddSeparator();
            m_rootNode.RegisterReplaceButtonPress(m_rootNode.AddButton("Replace"));

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
        /// Event raised by client when UI has graphically changed</summary>
        public override event EventHandler UIChanged;

        /// <summary>
        /// Event handler called when user has entered text into the ToolStrip</summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        private void replaceSubStrip_ReplaceTextEntered(object sender, System.EventArgs e)
        {
            DoReplace();
        }

        /// <summary>
        /// Returns an object that defines how search matches are modified in replacement</summary>
        public override object GetReplaceInfo()
        {
            return m_replaceTextInput.InputText;
        }

        /// <summary>
        /// Performs the pattern replace, using the search and replace parameters from search Control and replace Control</summary>
        public override void DoReplace()
        {
            if (DomNodeSearchToolStrip != null && DomNodeSearchToolStrip.QueryWithEmptyFields)
                return; //Do nothing
            if (DomNodeSearchToolStrip != null && DomNodeSearchToolStrip.QueryDirty)
                DomNodeSearchToolStrip.DoSearch();
           base.DoReplace();
        }


        /// <summary>
        /// Associated search toolstrip</summary>
        internal DomNodeSearchToolStrip DomNodeSearchToolStrip { get; set; }
  

        private readonly DomNodeQueryRoot m_rootNode;
        private readonly QueryTextInput m_replaceTextInput;
    }
}

