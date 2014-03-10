//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;

using AtfKeys = Sce.Atf.Input.Keys;
using WfKeys = System.Windows.Forms.Keys;
using AtfKeyEventArgs = Sce.Atf.Input.KeyEventArgs;
using WfKeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Sce.Atf
{
    /// <summary>
    /// Defines utility methods for WfKeys (Windows key codes and modifiers)</summary>
    public static class KeysUtil
    {
        /// <summary>Converts numeric pad AtfKeys to digit AtfKeys</summary>   
        /// <param name="keys">AtfKeys number pad keys</param>
        public static AtfKeys NumPadToNum(AtfKeys keys)
        {
            return Input.KeysUtil.NumPadToNum(keys);
        }

        /// <summary>Converts numeric pad WfKeys to digit WfKeys</summary>   
        /// <param name="keys">WfKeys number pad keys</param>
        public static WfKeys NumPadToNum(WfKeys keys)
        {
            return KeysInterop.ToWf(Input.KeysUtil.NumPadToNum(KeysInterop.ToAtf(keys)));
        }

        /// <summary>
        /// Converts a AtfKeys value to a string</summary>
        /// <param name="k">AtfKeys value</param>
        /// <param name="digitOnly">If true, for numeric pad or digit keys, only return 
        /// the number. For example, numpad0 becomes 0, and D0 becomes 0.</param>
        /// <returns>String representation of the AtfKeys value</returns>
        public static string KeysToString(AtfKeys k, bool digitOnly)
        {
            return Input.KeysUtil.KeysToString(k, digitOnly);
        }

        /// <summary>
        /// Converts a WfKeys value to a string</summary>
        /// <param name="k">WfKeys value</param>
        /// <param name="digitOnly">If true, for numeric pad or digit keys, only return 
        /// the number. For example, numpad0 becomes 0, and D0 becomes 0.</param>
        /// <returns>String representation of the WfKeys value</returns>
        public static string KeysToString(WfKeys k, bool digitOnly)
        {
            return Input.KeysUtil.KeysToString(KeysInterop.ToAtf(k), digitOnly);
        }

        /// <summary>
        /// Converts an enumeration of AtfKeys to a string</summary>
        /// <param name="k">Collection of AtfKeys</param>
        /// <param name="digitOnly">If true, for numeric pad or digit keys, only return 
        /// the number. For example, numpad0 becomes 0, and D0 becomes 0.</param>
        /// <returns>String representation of the AtfKeys enumeration, with each
        /// AtfKeys string separated by a comma and space (", ")</returns>
        public static string KeysToString(IEnumerable<AtfKeys> k, bool digitOnly)
        {
            return Input.KeysUtil.KeysToString(k, digitOnly);
        }

        /// <summary>
        /// Converts an enumeration of WfKeys to a string</summary>
        /// <param name="k">Collection of WfKeys</param>
        /// <param name="digitOnly">If true, for numeric pad or digit keys, only return 
        /// the number. For example, numpad0 becomes 0, and D0 becomes 0.</param>
        /// <returns>String representation of the WfKeys enumeration, with each
        /// WfKeys string separated by a comma and space (", ")</returns>
        public static string KeysToString(IEnumerable<WfKeys> k, bool digitOnly)
        {
            return Input.KeysUtil.KeysToString(KeysInterop.ToAtf(k), digitOnly);
        }

        /// <summary>
        /// Converts a WfKeyEventArgs to AtfKeys value</summary>
        /// <param name="ke">WfKeyEventArgs</param>
        /// <returns>AtfKeys enum</returns>
        public static AtfKeys KeyArgToKeys(WfKeyEventArgs ke)
        {
            return Input.KeysUtil.KeyArgToKeys(KeyEventArgsInterop.ToAtf(ke));
        }

        /// <summary>
        /// Converts a AtfKeyEventArgs to AtfKeys value</summary>
        /// <param name="ke">AtfKeyEventArgs</param>
        /// <returns>AtfKeys enum</returns>
        public static AtfKeys KeyArgToKeys(AtfKeyEventArgs ke)
        {
            return Input.KeysUtil.KeyArgToKeys(ke);
        }

        /// <summary>
        /// Converts AtfKeyEventArgs value to string</summary>
        /// <param name="ke">AtfKeyEventArgs</param>
        /// <returns>String representation of the KeyEventArgs value</returns>
        public static string KeyArgToString(AtfKeyEventArgs ke)
        {
            return Input.KeysUtil.KeyArgToString(ke);
        }

        /// <summary>
        /// Returns whether or not the given AtfKeys represents a human-readable character that could
        /// be inserted into a text box, for example</summary>
        /// <param name="k">AtfKeys value</param>
        /// <returns>True iff the given AtfKeys value represents a human-readable character</returns>
        public static bool IsPrintable(AtfKeys k)
        {
            return Input.KeysUtil.IsPrintable(k);
        }

        /// <summary>
        /// Determines whether or not the given AtfKeys should be considered a TextBox input and be handled
        /// by that control. For example, if this method returns true, ProcessCmdKey should return false
        /// and not call base.ProcessCmdKey.</summary>
        /// <param name="isMultiline">Whether the control (typically a TextBox or ComboBox) that received the key press is multiline</param>
        /// <param name="k">AtfKeys value</param>
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
        public static bool IsTextBoxInput(bool isMultiline, AtfKeys k)
        {
            return Input.KeysUtil.IsTextBoxInput(isMultiline, k);
        }

        /// <summary>
        /// Determines whether or not the given WfKeys should be considered a TextBox input and be handled
        /// by that control. For example, if this method returns true, ProcessCmdKey should return false
        /// and not call base.ProcessCmdKey.</summary>
        /// <param name="control">TextBox control. This control (typically a TextBox or ComboBox) is checked to determine if it is multiline, 
        /// which is one of the determinants of the return value.</param>
        /// <param name="k">WfKeys value</param>
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
        public static bool IsTextBoxInput(Control control, WfKeys k)
        {
            bool isMultiline = (control is TextBoxBase && ((TextBoxBase)control).Multiline);
            return Input.KeysUtil.IsTextBoxInput(isMultiline, KeysInterop.ToAtf(k));
        }

        /// <summary>
        /// Modifies the given collection with the given item based on standard Windows convention
        /// of using the Control key to toggle the selection of the given item, Shift key to add the
        /// item, and otherwise to set the selection to the item</summary>
        /// <typeparam name="T">The type of items contained in the selection</typeparam>
        /// <param name="collection">Collection of already selected items that is modified</param>
        /// <param name="item">Item to add or remove from collection</param>
        /// <param name="modifiers">Selection modifier AtfKeys</param>
        public static void Select<T>(ICollection<T> collection, T item, AtfKeys modifiers)
        {
            Input.KeysUtil.Select(collection, item, modifiers);
        }

        /// <summary>
        /// Modifies the given collection with the given item based on standard Windows convention
        /// of using the Control key to toggle the selection of the given item, Shift key to add the
        /// item, and otherwise to set the selection to the item</summary>
        /// <typeparam name="T">The type of items contained in the selection</typeparam>
        /// <param name="collection">Collection of already selected items that is modified</param>
        /// <param name="item">Item to add or remove from collection</param>
        /// <param name="modifiers">Selection modifier WfKeys</param>
        public static void Select<T>(ICollection<T> collection, T item, WfKeys modifiers)
        {
            Input.KeysUtil.Select(collection, item, KeysInterop.ToAtf(modifiers));
        }

        /// <summary>
        /// Modifies the given collection with the given items based on standard Windows convention
        /// of using the Control key to toggle the selection of items, Shift key to add the items,
        /// and otherwise to set the selection to the items</summary>
        /// <typeparam name="T">The type of items contained in the selection</typeparam>
        /// <param name="collection">Collection of already selected items that is modified</param>
        /// <param name="items">Items to add or remove from collection, or to set the collection to</param>
        /// <param name="modifiers">Selection modifier AtfKeys</param>
        public static void Select<T>(ICollection<T> collection, IEnumerable<T> items, AtfKeys modifiers)
        {
            Input.KeysUtil.Select(collection, items, modifiers);
        }

        /// <summary>
        /// Modifies the given collection with the given items based on standard Windows convention
        /// of using the Control key to toggle the selection of items, Shift key to add the items,
        /// and otherwise to set the selection to the items</summary>
        /// <typeparam name="T">The type of items contained in the selection</typeparam>
        /// <param name="collection">Collection of already selected items that is modified</param>
        /// <param name="items">Items to add or remove from collection, or to set the collection to</param>
        /// <param name="modifiers">Selection modifier WfKeys</param>
        public static void Select<T>(ICollection<T> collection, IEnumerable<T> items, WfKeys modifiers)
        {
            Input.KeysUtil.Select(collection, items, KeysInterop.ToAtf(modifiers));
        }

        /// <summary>
        /// Returns whether or not the given modifier AtfKeys should clear an existing selection
        /// when selecting a new item</summary>
        /// <param name="modifiers">Modifier AtfKeys</param>
        /// <returns>Whether or not the given modifier AtfKeys should clear an existing selection</returns>
        public static bool ClearsSelection(AtfKeys modifiers)
        {
            return Input.KeysUtil.ClearsSelection(modifiers);
        }

        /// <summary>
        /// Returns whether or not the given modifier WfKeys should clear an existing selection
        /// when selecting a new item</summary>
        /// <param name="modifiers">Modifier WfKeys</param>
        /// <returns>Whether or not the given modifier WfKeys should clear an existing selection</returns>
        public static bool ClearsSelection(WfKeys modifiers)
        {
            return Input.KeysUtil.ClearsSelection(KeysInterop.ToAtf(modifiers));
        }

        /// <summary>
        /// Returns whether or not the given modifier AtfKeys should toggle an item from a selection</summary>
        /// <param name="modifiers">Modifier AtfKeys</param>
        /// <returns>Whether or not the given modifier AtfKeys should toggle an item from a selection</returns>
        public static bool TogglesSelection(AtfKeys modifiers)
        {
            return Input.KeysUtil.TogglesSelection(modifiers);
        }

        /// <summary>
        /// Returns whether or not the given modifier WfKeys should toggle an item from a selection</summary>
        /// <param name="modifiers">Modifier WfKeys</param>
        /// <returns>Whether or not the given modifier WfKeys should toggle an item from a selection</returns>
        public static bool TogglesSelection(WfKeys modifiers)
        {
            return Input.KeysUtil.TogglesSelection(KeysInterop.ToAtf(modifiers));
        }

        /// <summary>
        /// Returns whether or not the given modifier AtfKeys should add an item from a selection</summary>
        /// <param name="modifiers">Modifier AtfKeys</param>
        /// <returns>Whether or not the given modifier AtfKeys should add an item from a selection</returns>
        public static bool AddsSelection(AtfKeys modifiers)
        {
            return Input.KeysUtil.AddsSelection(modifiers);
        }
    }
}
