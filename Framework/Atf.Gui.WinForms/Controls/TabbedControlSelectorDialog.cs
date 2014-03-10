using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog that enumerates all controls accessable by the specified IControlHostService.
    /// 
    /// A TabbedControlSelectorDialog persists until the Ctrl button is released. Until then, 
    /// it consumes Ctrl+[Tab|Up|Down|Left|Right] key presses to switch the currently selected 
    /// control in the enumeration. When Ctrl is released, the control corresponding to the 
    /// selection in the enumeration is given input focus.
    /// 
    /// Enumeration of controls is separated into two lists: one including all controls in the currently
    /// active pane, and another for all other controls (i.e., those NOT in the active pane). Use
    /// Ctrl+Left and Ctrl+Right to jump between the two lists.</summary>
    public partial class TabbedControlSelectorDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="incrementForward">Increment forward</param>
        public TabbedControlSelectorDialog(IControlHostService controlHostService, bool incrementForward)
        {
            m_controlHostService = controlHostService;

            // Retrieve the list of controls in the currently-focused pane
            m_focusedPaneControlList = new List<ControlInfo>();
            m_unfocusedPaneControlList = new List<ControlInfo>();
            foreach (ControlInfo ci in m_controlHostService.Controls)
            {
                if (ci.InActiveGroup)
                    m_focusedPaneControlList.Add(ci);
                else
                    m_unfocusedPaneControlList.Add(ci);
            }

            m_focusedPaneControlList.Reverse();
            m_unfocusedPaneControlList.Reverse();

            InitializeComponent();
 
            // Populate ListBox with list of controls from focused pane 
            focusedPaneListBox.DataSource = m_focusedPaneControlList;
            focusedPaneListBox.DisplayMember = "Name";
            if (m_focusedPaneControlList.Count > 0)
                focusedPaneListBox.SetSelected(0, true);

            // Populate ListBox with list of controls from unfocused panes
            unfocusedPaneListBox.DataSource = m_unfocusedPaneControlList;
            unfocusedPaneListBox.DisplayMember = "Name";

            // List of controls in focused pane should initially be selectable
            SetActiveListBox(eListBoxType.FocusedPaneListBox);

            // The act of opening this dialog increments selection next most recently used control
            IncrementSelection(incrementForward);
        }

        /// <summary>
        /// Defines operations that need to occur when focus switches from one list of controls to another</summary>
        enum eListBoxType { FocusedPaneListBox, UnfocusedPaneListBox }
        private void SetActiveListBox(eListBoxType listBox)
        {
            switch(listBox)
            {
                case eListBoxType.FocusedPaneListBox:
                    m_activeListBox = focusedPaneListBox;
                    m_activeList = m_focusedPaneControlList;
                    unfocusedPaneListBox.Enabled = false;
                    break;
                    
                case eListBoxType.UnfocusedPaneListBox:
                    m_activeListBox = unfocusedPaneListBox;
                    m_activeList = m_unfocusedPaneControlList;
                    focusedPaneListBox.Enabled = false;
                    break;

                default:
                    throw new System.ApplicationException("SetActiveListBox() - unhandled list box type specified");
            }

            m_activeListBox.Enabled = true;
            UpdateToolStripStatus();
        }

        /// <summary>
        /// Handles field key down events. Ignores any event if the control button isn't pressed.
        /// Tab: Increment selection in currently active control list.
        ///      (decrement if shift button is down).
        /// Down/Up: Increment/decrement selection.
        /// Left/Right: Switch focus to other control list.</summary>
        private void TabbedControlSelectorDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control)
                return;

            e.Handled = true;
            switch(e.KeyCode)
            {
                case Keys.Tab:
                    IncrementSelection(!e.Shift);
                    break;

                case Keys.Up:
                    IncrementSelection(false);
                    break;

                case Keys.Down:
                    IncrementSelection(true);
                    break;

                case Keys.Left:
                case Keys.Right:
                    SetActiveListBox((m_activeListBox == focusedPaneListBox) ? eListBoxType.UnfocusedPaneListBox : eListBoxType.FocusedPaneListBox);
                    break;

                default:
                    e.Handled = false;
                    break;
            }
        }

        /// <summary>
        /// Selects next/previous item in currently active control list box.
        /// Loops to begin/end if we go past the bottom/top.</summary>
        private void IncrementSelection(bool forward)
        {
            if (m_activeListBox.SelectedIndices.Count > 0)
            {
                int nextIndex = m_activeListBox.SelectedIndices[0];
                if (forward)
                {
                    nextIndex = (nextIndex + 1) % m_activeListBox.Items.Count;
                }
                else // backward
                {
                    nextIndex = (nextIndex > 0) ? nextIndex-1 : m_activeListBox.Items.Count - 1;
                }

                if (nextIndex != m_activeListBox.SelectedIndices[0])
                {
                    m_activeListBox.SetSelected(nextIndex, true);
                    UpdateToolStripStatus();
                }
            }
        }

        /// <summary>
        /// Sets the tool strip text to the description of the currently selected control in the list box</summary>
        private void UpdateToolStripStatus()
        {
            toolStripStatusLabel.Text = (m_activeList.Count > 0) ? (m_activeList[SelectedControlIndex]).Description : "";
        }

        /// <summary>
        /// Closes the dialog when the selection has been made. Before doing so, bring the selected control into focus.</summary>
        private void SelectionMade()
        {
            if (m_activeList.Count > 0)
                m_controlHostService.Show((m_activeList[SelectedControlIndex]).Control);
            Close();
        }

        /// <summary>
        /// Fields the release of Ctrl key, so that we can close the dialog</summary>
        private void TabbedControlSelectorDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                SelectionMade();
        }

        /// <summary>
        /// Gets index of selected control in the active control list</summary>
        private int SelectedControlIndex
        {
            get { return m_activeListBox.SelectedIndices[0]; }
        }

        /// <summary>
        /// Finalizes the selection when the user mouse-clicks on the control list. Equivalent to 
        /// releasing the Ctrl key</summary>
        private void focusedPaneListBox_SelectionChanged(object sender, EventArgs e)
        {
            SetActiveListBox(eListBoxType.FocusedPaneListBox);
            if (focusedPaneListBox.Focused)
                SelectionMade();
        }

        /// <summary>
        /// Finalizes the selection when the user mouse-clicks on the control list. Equivalent to 
        /// releasing the Ctrl key</summary>
        private void unfocusedPaneListBox_SelectionChanged(object sender, EventArgs e)
        {
            SetActiveListBox(eListBoxType.UnfocusedPaneListBox);
            if (unfocusedPaneListBox.Focused)
                SelectionMade();
        }

        private readonly IControlHostService m_controlHostService;
        private readonly List<ControlInfo> m_focusedPaneControlList;
        private readonly List<ControlInfo> m_unfocusedPaneControlList;
        private ListBox m_activeListBox;
        private List<ControlInfo> m_activeList;
    }
}
