//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Text;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Defines utility methods for Keys enum</summary>
    public static class KeysUtil
    {
        /// <summary>Converts numeric pad keys to digit keys</summary>   
        /// <param name="keys">Number pad keys</param>
        /// <returns>Digit key codes</returns>
        public static Keys NumPadToNum(Keys keys)
        {
            // extract keycode from Keys
            Keys kc = keys & Keys.KeyCode;
            if (kc >= Keys.NumPad0 && kc <= Keys.NumPad9)
            {
                // convert numpad key to digit key.
                const Keys KeyDelta = (Keys)(Keys.NumPad0 - Keys.D0);
                kc = (Keys)(kc - KeyDelta);
                // clear current key code
                keys = (keys & ~Keys.KeyCode) | kc;
            }

            return keys;
        }

        /// <summary>
        /// Converts a Keys value to a string</summary>
        /// <param name="k">Keys value</param>
        /// <param name="digitOnly">If true, for numeric pad or digit keys, only return 
        /// the number. For example, numpad0 becomes 0, and D0 becomes 0.</param>
        /// <returns>String representation of the Keys value</returns>
        public static string KeysToString(Keys k, bool digitOnly)
        {
            string str = string.Empty;

            // extract keycode from Keys
            Keys kc = k & Keys.KeyCode;

            // no key or Backspace are not valid
            if (k == Keys.None || kc == Keys.Back)
                return str;

            bool altCtrlShift =
                kc == Keys.Menu ||
                kc == Keys.ControlKey ||
                kc == Keys.ShiftKey;

            // modifiers key alone is not valid shortcut.
            if (altCtrlShift)
                return str;

            if ((k & Keys.Alt) == Keys.Alt)
            {
                str += "Alt";
            }

            if ((k & Keys.Control) == Keys.Control)
            {
                if (str.Length > 0)
                    str += "+";
                str += "Ctrl";
            }

            if ((k & Keys.Shift) == Keys.Shift)
            {
                if (str.Length > 0)
                    str += "+";
                str += "Shift";
            }

            if (str.Length > 0)
                str += "+";

            var keyCode = k & Keys.KeyCode;
            if (keyCode == Keys.Oemplus)          
                str += "+";           
            else if (keyCode == Keys.OemMinus)
                str += "-";    
            else
                str += keyCode;

            if (digitOnly)
            {
                if (kc >= Keys.NumPad0 && kc <= Keys.NumPad9)
                    str = str.Replace("NumPad", "");
                else if (kc >= Keys.D0 && kc <= Keys.D9)
                    str = str.Replace("D", "");
            }

            return str;
        }

        /// <summary>
        /// Converts an enumeration of Keys to a string</summary>
        /// <param name="k">Collection of Keys</param>
        /// <param name="digitOnly">If true, for numeric pad or digit keys, only return 
        /// the number. For example, numpad0 becomes 0, and D0 becomes 0.</param>
        /// <returns>String representation of the Keys enumeration, with each
        /// Keys string separated by a comma and space (", ")</returns>
        public static string KeysToString(IEnumerable<Keys> k, bool digitOnly)
        {
            var result = new StringBuilder();
            bool foundOne = false;
            foreach (Keys key in k)
            {
                if (foundOne)
                    result.Append(", ");
                result.Append(KeysToString(key, digitOnly));
                foundOne = true;
            }
            return result.ToString();
        }

        /// <summary>
        /// Converts a KeyArgEvent to Keys value</summary>
        /// <param name="ke">KeyEventArgs</param>
        /// <returns>Keys enum</returns>
        public static Keys KeyArgToKeys(KeyEventArgs ke)
        {
            Keys key = new Keys();

            // no key or Backspace are not valid
            if (ke.KeyCode == Keys.None || ke.KeyCode == Keys.Back)
                return key;

            bool altCtrlShift =
                ke.KeyCode == Keys.Menu ||
                ke.KeyCode == Keys.ControlKey ||
                ke.KeyCode == Keys.ShiftKey;

            // modifiers key alone is not valid shortcut.
            if (altCtrlShift)
                return key;

            if (ke.Alt)
                key |= Keys.Alt;

            if (ke.Control)
                key |= Keys.Control;

            if (ke.Shift)
                key |= Keys.Shift;

            key |= ke.KeyCode;
            return key;

        }

        /// <summary>
        /// Converts KeyEventArgs value to string</summary>
        /// <param name="ke">KeyEventArgs</param>
        /// <returns>String representation of the KeyEventArgs value</returns>
        public static string KeyArgToString(KeyEventArgs ke)
        {
            return KeysToString(KeyArgToKeys(ke), false);
        }

        /// <summary>
        /// Returns whether or not the given Keys represents a human-readable character that could
        /// be inserted into a text box, for example</summary>
        /// <param name="k">Keys value</param>
        /// <returns><c>True</c> if the given Keys represents a human-readable character</returns>
        public static bool IsPrintable(Keys k)
        {
            // Assume no printable character is generated when Alt or Control are pressed.
            if ((k & (Keys.Alt | Keys.Control)) != 0)
                return false;

            // Check for the various ranges and special cases in the Keys enum.
            Keys noMods = k & Keys.KeyCode;
            return
                (noMods >= Keys.A && noMods <= Keys.Z) ||
                (noMods >= Keys.D0 && noMods <= Keys.D9) ||
                noMods == Keys.Tab ||
                noMods == Keys.LineFeed ||
                noMods == Keys.Space ||
                (noMods >= Keys.NumPad0 && noMods <= Keys.Divide) ||
                (noMods >= Keys.Oem1 && noMods <= Keys.OemBackslash);
        }

        /// <summary>
        /// Determines whether or not the given key press should be considered a TextBox input and be handled
        /// by that control. For example, if this method returns true, ProcessCmdKey should return false
        /// and not call base.ProcessCmdKey.</summary>
        /// <param name="isMultiline">Whether the control (typically a TextBox or ComboBox) that received the key press is multiline</param>
        /// <param name="k">Keys value</param>
        /// <returns>Whether or not the given key press should be considered a TextBox input</returns>
        /// <remarks>
        /// Some examples from a class deriving from TextBox and what its ProcessCmdKey should do:
        /// Keypress | IsInputKey | ProcessCmdKey should      | Because
        /// --------   ----------   -------------------------   --------------------------------------
        /// s          false        return false                Even if this is a command shortcut, the TextBox input takes priority.
        /// 1          false        return false                Even if this is a command shortcut, the TextBox input takes priority.
        /// F10        false        return base.ProcessCmdKey   Might be a command shortcut.
        /// Ctrl+Z     false        return base.ProcessCmdKey   Might be a command shortcut and that takes priority over the TextBox's limited undo.
        /// Ctrl+C     false        return false                The TextBox copy command takes priority.
        /// Home       true         return false                The TextBox navigation takes priority.
        /// Delete     false        return false                The TextBox navigation takes priority. Why does IsInputKey() return false?! .NET bug?
        /// Up         false        return false                We want our IsInputKey override to receive this.
        /// </remarks>
        public static bool IsTextBoxInput(bool isMultiline, Keys k)
        {
            //The following keypresses need to be kept from base.ProcessCmdKey() because they will
            //  bubble up to owning forms and do things like copy DomNodes.
            switch (k)
            {
                //This commented out code is from TextBoxBase and indicates the shortcuts that it cares about.
                //  Some are so obscure, like the text justification shortcuts, that they shouldn't be consumed
                //  by default and should be available to ControlHostService, CommandService, etc. Ctrl+Back
                //  doesn't do anything in practice; not sure why it's in this list. Others, like Shift+Home and
                //  Shift+End are used by TextBox, so let's check for those.
                //shortcutsToDisable = new int[] {(int)Shortcut.CtrlZ, (int)Shortcut.CtrlC, (int)Shortcut.CtrlX,
                //(int)Shortcut.CtrlV, (int)Shortcut.CtrlA, (int)Shortcut.CtrlL, (int)Shortcut.CtrlR,
                //(int)Shortcut.CtrlE, (int)Shortcut.CtrlY, (int)Keys.Control + (int)Keys.Back,
                //(int)Shortcut.CtrlDel, (int)Shortcut.ShiftDel, (int)Shortcut.ShiftIns, (int)Shortcut.CtrlJ};

                case Keys.Control | Keys.C:     //copy
                case Keys.Control | Keys.X:     //cut
                case Keys.Control | Keys.V:     //paste
                case Keys.Control | Keys.A:     //select all the text
                case Keys.Control | Keys.Delete://delete the next word
                case Keys.Shift | Keys.Delete:  //delete the previous character. Too obscure?
                case Keys.Shift | Keys.Insert:  //paste. Too obscure?
                case Keys.Delete:               //delete the next character or the entire selection
                case Keys.Back:                 //delete the previous character or the entire selection
                case Keys.Home:                 //move the caret to the beginning
                case Keys.End:                  //move the caret to the end
                case Keys.Shift | Keys.Home:    //select everything from the current position to the beginning
                case Keys.Shift | Keys.End:     //select everything from the current position to the end
                case Keys.Up:                   //TextBox's IsInputKey override should return false to allow for special navigation.
                case Keys.Down:                 //TextBox's IsInputKey override should return false to allow for special navigation.
                case Keys.Right:
                case Keys.Left:
                    return true;

                case Keys.Return:
                    if (isMultiline)
                    {
                        return true;
                    }
                    break;
            }

            // If it's any kind of printable character, let the textbox have it.
            return IsPrintable(k);
        }

        /// <summary>
        /// Modifies the given collection with the given item based on standard Windows convention
        /// of using the Control key to toggle the selection of the given item, Shift key to add the
        /// item, and otherwise to set the selection to the item</summary>
        /// <typeparam name="T">The type of items contained in the selection</typeparam>
        /// <param name="collection">Collection of already selected items that is modified</param>
        /// <param name="item">Item to add or remove from collection</param>
        /// <param name="modifiers">Selection modifier keys</param>
        public static void Select<T>(ICollection<T> collection, T item, Keys modifiers)
        {
            Select(collection, new[] { item }, modifiers);
        }

        /// <summary>
        /// Modifies the given collection with the given items based on standard Windows convention
        /// of using the Control key to toggle the selection of items, Shift key to add the items,
        /// and otherwise to set the selection to the items</summary>
        /// <typeparam name="T">The type of items contained in the selection</typeparam>
        /// <param name="collection">Collection of already selected items that is modified</param>
        /// <param name="items">Items to add or remove from collection, or to set the collection to</param>
        /// <param name="modifiers">Selection modifier keys</param>
        public static void Select<T>(ICollection<T> collection, IEnumerable<T> items, Keys modifiers)
        {
            if (ClearsSelection(modifiers))
            {
                collection.Clear();
                foreach (T item in items)
                    collection.Add(item);
            }
            else
            {
                foreach (T item in items)
                {
                    if ((modifiers & Keys.Control) == Keys.Control)
                    {
                        // toggle the item from the collection
                        if (!collection.Remove(item))
                            collection.Add(item);
                    }
                    else // if ((modifiers & Keys.Shift) == Keys.Shift)
                    {
                        // strictly add the item, making it the "last selected"
                        collection.Remove(item);
                        collection.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether or not the given modifier keys should clear an existing selection
        /// when selecting a new item</summary>
        /// <param name="modifiers">Modifier keys</param>
        /// <returns>Whether or not the given modifier keys should clear an existing selection</returns>
        public static bool ClearsSelection(Keys modifiers)
        {
            return (modifiers & (Keys.Control | Keys.Shift)) == Keys.None;
        }

        /// <summary>
        /// Returns whether or not the given modifier keys should toggle an item from a selection</summary>
        /// <param name="modifiers">Modifier keys</param>
        /// <returns>Whether or not the given modifier keys should toggle an item from a selection</returns>
        public static bool TogglesSelection(Keys modifiers)
        {
            return (modifiers & Keys.Control) == Keys.Control;
        }

        /// <summary>
        /// Returns whether or not the given modifier keys should add an item from a selection</summary>
        /// <param name="modifiers">Modifier keys</param>
        /// <returns>Whether or not the given modifier keys should add an item from a selection</returns>
        public static bool AddsSelection(Keys modifiers)
        {
            return (modifiers & Keys.Shift) == Keys.Shift;
        }
    }
}
