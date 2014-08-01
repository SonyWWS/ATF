//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using AtfKeys = Sce.Atf.Input.Keys;
using WfKeys = System.Windows.Forms.Keys;
using WpfKey = System.Windows.Input.Key;
using WpfModifierKeys = System.Windows.Input.ModifierKeys;

namespace Sce.Atf.Wpf.Interop
{
    /// <summary>
    /// Static utility methods for converting among different key types</summary>
    public static class KeysInterop
    {
        /// <summary>
        /// Converts a System.Windows.Forms.Keys to an Sce.Atf.Input.Keys</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static AtfKeys ToAtf(WfKeys keys)
        {
            return (AtfKeys)keys;
        }

        /// <summary>
        /// Converts an enumerable list of System.Windows.Forms.Keys to an
        /// enumerable list of Sce.Atf.Input.Keys</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static IEnumerable<AtfKeys> ToAtf(IEnumerable<WfKeys> keys)
        {
            foreach (WfKeys wfKey in keys)
                yield return (AtfKeys)wfKey;
        }

        /// <summary>
        /// Converts a Sce.Atf.Input.Keys to a System.Windows.Forms.Keys</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static WfKeys ToWf(AtfKeys keys)
        {
            return (WfKeys)keys;
        }

        /// <summary>
        /// Converts an enumerable list of Sce.Atf.Input.Keys to an
        /// enumerable list of System.Windows.Forms.Keys</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static IEnumerable<WfKeys> ToWf(IEnumerable<AtfKeys> keys)
        {
            foreach (AtfKeys atfKey in keys)
                yield return (WfKeys)atfKey;
        }

        /// <summary>
        /// Converts a System.Windows.Input.Key to an Sce.Atf.Input.Keys</summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static AtfKeys ToAtf(WpfKey wpfKey)
        {
            return (AtfKeys)System.Windows.Input.KeyInterop.VirtualKeyFromKey(wpfKey);
        }

        /// <summary>
        /// Converts a System.Windows.Input.ModifierKey to an Sce.Atf.Input.Keys</summary>
        /// <param name="wpfKey"></param>
        /// <returns></returns>
        public static AtfKeys ToAtf(WpfModifierKeys wpfKey)
        {
            var atfKey = AtfKeys.None;

            if ((wpfKey & WpfModifierKeys.Shift) != 0)
                atfKey |= AtfKeys.Shift;
            if ((wpfKey & WpfModifierKeys.Control) != 0)
                atfKey |= AtfKeys.Control;
            if ((wpfKey & WpfModifierKeys.Alt) != 0)
                atfKey |= AtfKeys.Alt;

            return atfKey;
        }

        /// <summary>
        /// Converts a Sce.Atf.Input.Keys to a System.Windows.Input.Key</summary>
        /// <remarks>Warning: this will only return a single Key as the WPF Key 
        /// structure is not flags based</remarks>
        /// <param name="atfKey"></param>
        /// <returns></returns>
        public static WpfKey ToWpf(AtfKeys atfKey)
        {
            var keys = atfKey & ~AtfKeys.Modifiers;
            return System.Windows.Input.KeyInterop.KeyFromVirtualKey((int)keys);
        }

        /// <summary>
        /// Converts a Sce.Atf.Input.Keys to a System.Windows.Input.ModifierKeys</summary>
        /// <param name="atfKeys"></param>
        /// <returns></returns>
        public static WpfModifierKeys ToWpfModifiers(AtfKeys atfKeys)
        {
            // TODO: need to verify this all works correctly
            var modifiers = atfKeys &= AtfKeys.Modifiers;
            WpfModifierKeys result = WpfModifierKeys.None;

            if ((modifiers & AtfKeys.Alt) > 0)
                result |= WpfModifierKeys.Alt;
            if ((modifiers & AtfKeys.Shift) > 0)
                result |= WpfModifierKeys.Shift;
            if ((modifiers & AtfKeys.Control) > 0)
                result |= WpfModifierKeys.Control;

            if ((atfKeys & AtfKeys.RWin) > 0 || (atfKeys & AtfKeys.RWin) > 0)
                result |= WpfModifierKeys.Windows;

            return result;
        }
    }
}