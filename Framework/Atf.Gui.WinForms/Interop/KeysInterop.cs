//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using AtfKeys = Sce.Atf.Input.Keys;
using WfKeys = System.Windows.Forms.Keys;

namespace Sce.Atf
{
    /// <summary>
    /// Enum for supported key set types</summary>
    public enum SupportedTypes
    {
        Atf,
        WinForms,
        Wpf
    }

    /// <summary>
    /// Converts key codes between ATF and Windows to support interoperability for events between Windows and ATF</summary>
    public static class KeysInterop
    {
        /// <summary>
        /// Converts WfKeys to AtfKeys</summary>
        /// <param name="keys">WfKeys</param>
        /// <returns>AtfKeys</returns>
        public static AtfKeys ToAtf(WfKeys keys)
        {
            return (AtfKeys)keys;
        }
        /// <summary>
        /// Converts an enumeration of WfKeys to an enumeration of AtfKeys</summary>
        /// <param name="keys">Enumeration of WfKeys</param>
        /// <returns>Enumeration of AtfKeys</returns>
        public static IEnumerable<AtfKeys> ToAtf(IEnumerable<WfKeys> keys)
        {
            foreach(WfKeys wfKey in keys)
                yield return (AtfKeys)wfKey;
        }
        /// <summary>
        /// Converts AtfKeys to WfKeys</summary>
        /// <param name="keys">AtfKeys</param>
        /// <returns>WfKeys</returns>
        public static WfKeys ToWf(AtfKeys keys)
        {
            return (WfKeys)keys;
        }
        /// <summary>
        /// Converts an enumeration of AtfKeys to an enumeration of WfKeys</summary>
        /// <param name="keys">Enumeration of AtfKeys</param>
        /// <returns>Enumeration of WfKeys</returns>
        public static IEnumerable<WfKeys> ToWf(IEnumerable<AtfKeys> keys)
        {
            foreach(AtfKeys atfKey in keys)
                yield return (WfKeys)atfKey;
        }
    }
}