//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using Keys = Sce.Atf.Input.Keys;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to edit and assign shortcuts to the registered commands</summary>
    public partial class CustomizeKeyboardDialog : Form
    {
        /// <summary>
        /// Constructor with specified table of m_command/shortcut pairs</summary>
        /// <param name="shortcuts">List of shortcut infos, which populate the dialog box and hold the resulting
        /// changed shortcut keys</param>
        /// <param name="reservedKeysAndExplanations">Dictionary of reserved keys and explanations for why that
        /// key is reserved</param>
        public CustomizeKeyboardDialog(
            IList<Shortcut> shortcuts,
            Dictionary<Keys, string> reservedKeysAndExplanations)
        {
            m_shortcuts = shortcuts;
            m_reservedKeys = reservedKeysAndExplanations;

            InitializeComponent();

            // populate listbox with commands
            lstCommand.Sorted = true;
            m_displayNameToShortcut = new Dictionary<string, Shortcut>(shortcuts.Count);
            foreach (Shortcut shortcut in shortcuts)
            {
                m_displayNameToShortcut[shortcut.DisplayPath] = shortcut;
                lstCommand.Items.Add(shortcut.DisplayPath);
            }

            btnAddShortcut.Enabled = false;
            btnAssignShortcut.Enabled = false;
            if (lstCommand.Items.Count > 0)
                lstCommand.SelectedIndex = 0;
            else
                grpNewShortcut.Enabled = false;

            txtNewShortcut.ContextMenu = cxMenu;
        }

        /// <summary>
        /// Gets whether the shortcuts have been modified by the user</summary>
        public bool Modified
        {
            get
            {
                foreach (Shortcut shortcut in m_shortcuts)
                    if (shortcut.KeysChanged)
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Shortcut and description</summary>
        public class Shortcut
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="info">CommandInfo to use to initialize this object</param>
            /// <param name="commandPath">The menu names plus command name, each separated by '/'</param>
            public Shortcut(CommandInfo info, string commandPath)
            {
                Info = info;
                CommandPath = commandPath;
                Keys = info.Shortcuts;
            }

            /// <summary>
            /// Gets the CommandInfo that this Shortcut object represents</summary>
            public CommandInfo Info
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the menu names plus command name, each separated by '/'</summary>
            public string CommandPath
            {
                get { return m_commandPath; }
                private set
                {
                    m_commandPath = value;
                    DisplayPath = value;
                    DisplayPath = DisplayPath.Replace("&", "");
                    DisplayPath = DisplayPath.Replace(".", "");
                }
            }

            /// <summary>
            /// Gets the menu and command name as a path, suitable for display to the user</summary>
            public string DisplayPath
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets and sets Shortcut keys for this command. Contains the results, after this dialog
            /// box exits. Setting makes a copy of the enumeration.</summary>
            public IEnumerable<Keys> Keys
            {
                get { return m_keys; }
                set { m_keys = new List<Keys>(value); }
            }

            /// <summary>
            /// Gets whether or not the Keys property is different than the CommandInfo's Shortcuts property</summary>
            public bool KeysChanged
            {
                get { return !Keys.SequenceEqual(Info.Shortcuts); }
            }

            /// <summary>
            /// Gets whether or not the Keys property is the same as the CommandInfo's DefaultShortcuts property</summary>
            public bool KeysAreDefault
            {
                get
                {
                    if (IsEmptyOrNone(m_keys) && IsEmptyOrNone(Info.DefaultShortcuts))
                        return true;
                    return m_keys.SequenceEqual(Info.DefaultShortcuts);
                }
            }

            private static bool IsEmptyOrNone(IEnumerable<Keys> shortcuts)
            {
                foreach (Keys key in shortcuts)
                    if (key != Input.Keys.None)
                        return false;
                return true;
            }

            private string m_commandPath;
            private List<Keys> m_keys;
        }

        private void lstCommand_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox listbox = (ListBox)sender;
            string strItem = (string)listbox.SelectedItem;
            m_currentShortcut = m_displayNameToShortcut[strItem];

            // reset
            m_newKey = Keys.None;
            UpdateControls();
        }

        private void btnSetToDefault_Click(object sender, EventArgs e)
        {
            if (m_currentShortcut == null)
                return;

            IEnumerable<Keys> newKeys = m_currentShortcut.Info.DefaultShortcuts;

            // remove shortcut from the other command that uses this shortcut
            foreach (Keys key in newKeys)
                RemoveShortcut(key);

            m_currentShortcut.Keys = newKeys;

            // reset
            m_newKey = Keys.None;
            UpdateControls();
        }

        private void btnAllDefault_Click(object sender, EventArgs e)
        {
            int numCommandsToReset = 0;
            var message = new StringBuilder();
            foreach (Shortcut shortcut in m_shortcuts)
            {
                if (!shortcut.KeysAreDefault)
                {
                    message.AppendLine(shortcut.CommandPath);
                    numCommandsToReset++;
                }
            }

            if (numCommandsToReset > 0)
            {
                if (numCommandsToReset < 10)
                {
                    message.Insert(0,
                        "These commands currently do not use their default shortcuts:".Localize()
                        + Environment.NewLine);
                }
                else
                {
                    message.Length = 0;
                    message.Append(
                        string.Format("{0} commands currently do not use their default shortcuts.".Localize("{0} is a number"),
                        numCommandsToReset));
                }

                if (MessageBox.Show(this, message.ToString(),
                    "Reset all commands to the default shortcuts?".Localize(),
                    MessageBoxButtons.OKCancel)
                    == DialogResult.OK)
                {
                    foreach (Shortcut shortcut in m_shortcuts)
                        shortcut.Keys = shortcut.Info.DefaultShortcuts;
                }
            }

            // reset
            m_newKey = Keys.None;
            UpdateControls();
        }

        // Removes a shortcut from the Keys property of all Shortcut objects
        private void RemoveShortcut(Keys key)
        {
            if (key == Keys.None)
                return;

            foreach (Shortcut shortcut in m_shortcuts)
            {
                if (shortcut.Keys.Contains(key))
                {
                    List<Keys> newKeys = new List<Keys>(shortcut.Keys);
                    newKeys.Remove(key);
                    shortcut.Keys = newKeys;
                }
            }
        }

        private void btnAddShortcut_Click(object sender, EventArgs e)
        {
            SetShortcuts(true);
        }

        /// <summary>
        /// Method called when shortcut is removed from current command</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void btnRemoveShortcut_Click(object sender, EventArgs e)
        {
            m_currentShortcut.Keys = new [] {Keys.None};
            UpdateControls();
        }

        /// <summary>
        /// Method called when assigning new shortcut to the selected command</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void btnAssignShortcut_Click(object sender, EventArgs e)
        {
            SetShortcuts(false);
        }

        // takes m_newKey and either replaces m_currentShortcut.Keys or adds to it, depending on 'addShortcut'
        private void SetShortcuts(bool addShortcut)
        {
            // only add when we have valid shortcut
            if (m_newKey == Keys.None)
                return;

            // don't assign reserved key
            if (m_reservedKeys.ContainsKey(m_newKey))
            {
                string newKeyString = KeysUtil.KeysToString(m_newKey, false);
                MessageBox.Show(this, newKeyString + " is reserved for " + m_reservedKeys[m_newKey], Text);
                return;
            }

            // remove shortcut from any other command that uses this shortcut
            RemoveShortcut(m_newKey);

            // assign the new key to the current command
            var newKeys = new List<Keys>();
            if (addShortcut)
            {
                foreach(Keys key in m_currentShortcut.Keys)
                    if (key != Keys.None)
                        newKeys.Add(key);
            }
            newKeys.Add(m_newKey);
            m_currentShortcut.Keys = newKeys;

            // reset
            m_newKey = Keys.None;
            UpdateControls();
        }

        /// <summary>
        /// Intercepts all key down strokes and populate textbox only on valid
        /// shortcut keys</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void txtNewShortcut_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            m_newKey = KeysUtil.KeyArgToKeys(e);
            m_newKey = KeysUtil.NumPadToNum(m_newKey);
            UpdateControls();
        }

        /// <summary>
        /// Method called when clearing and resetting new shortcut field</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void mnuClear_Click(object sender, EventArgs e)
        {
            m_newKey = Keys.None;
            UpdateControls();
        }

        /// <summary>
        /// Method called when updating the status of the context menu for new shortcut text field</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        private void cxMenue_Popup(object sender, EventArgs e)
        {
            try
            {
                mnuClear.Enabled = txtNewShortcut.Text.Trim().Length > 0;
            }
            catch (Exception ex)
            {
                mnuClear.Enabled = false;
                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
            }
        }

        private string GetUsedByText(Keys key)
        {
            var sb = new StringBuilder();
            if (key != Keys.None)
            {
                // linearly search for a command that uses this shortcut
                foreach (Shortcut shortcut in m_shortcuts)
                {
                    if (shortcut.Keys.Contains(key))
                    {
                        sb.AppendLine(shortcut.DisplayPath + "\r\n" + shortcut.Info.Description);
                        break;
                    }
                }
            }
            return sb.ToString();
        }

        // Updates the UI from the private non-UI fields
        private void UpdateControls()
        {
            string strKey = KeysUtil.KeysToString(m_currentShortcut.Keys, false);
            bool hasKey = !string.IsNullOrEmpty(strKey);
            btnRemoveShortcut.Enabled = hasKey;
            lblCurShortcut.Text = (hasKey) ? strKey : null;

            btnSetToDefault.Enabled = !m_currentShortcut.KeysAreDefault;
            lblCmdDescription.Text = m_currentShortcut.Info.Description;

            if (m_newKey == Keys.None)
            {
                txtNewShortcut.Text = string.Empty;
                btnAddShortcut.Enabled = false;
                btnAssignShortcut.Enabled = false;
                lblUsedBy.Text = null;
                grpShortUsed.Enabled = false;
            }
            else
            {
                txtNewShortcut.Text = KeysUtil.KeysToString(m_newKey, false);
                txtNewShortcut.SelectionStart = txtNewShortcut.Text.Length;
                btnAddShortcut.Enabled = true;
                btnAssignShortcut.Enabled = true;
                string otherCommands = GetUsedByText(m_newKey);
                lblUsedBy.Text = otherCommands;
                grpShortUsed.Enabled = otherCommands.Length > 0;
            }

            bool allCommandsAreDefault = true;
            foreach (Shortcut shortcut in m_shortcuts)
            {
                if (!shortcut.KeysAreDefault)
                {
                    allCommandsAreDefault = false;
                    break;
                }
            }
            btnAllDefault.Enabled = !allCommandsAreDefault;
        }

        private readonly Dictionary<string, Shortcut> m_displayNameToShortcut; //maps display name to shortcut info
        private readonly IList<Shortcut> m_shortcuts;
        private Shortcut m_currentShortcut; // currently selected Shortcut in listbox
        private Keys m_newKey; // a key to be assigned
        private readonly Dictionary<Keys, string> m_reservedKeys;
    }
}