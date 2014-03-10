//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using AtfMouseButtons = Sce.Atf.Input.MouseButtons;

using WfMouseButtons = System.Windows.Forms.MouseButtons;

namespace Sce.Atf
{
    /// <summary>
    /// Converts mouse buttons between ATF and Windows to support interoperability for events between Windows and ATF</summary>
    public class MouseButtonsInterop
    {
        /// <summary>
        /// Constructor</summary>
        private MouseButtonsInterop() {}

        /// <summary>
        /// Constructor with AtfMouseButtons</summary>
        /// <param name="buttons">AtfMouseButtons</param>
        public MouseButtonsInterop(AtfMouseButtons buttons)
        {
            Value = (int)buttons;
            ValueType = SupportedTypes.Atf;
        }

        /// <summary>
        /// Constructor with WfMouseButtons</summary>
        /// <param name="buttons">WfMouseButtons</param>
        public MouseButtonsInterop(WfMouseButtons buttons)
        {
            Value = (int)buttons;
            ValueType = SupportedTypes.WinForms;
        }

        /// <summary>
        /// Implicit conversion operator from AtfMouseButtons to MouseButtonsInterop instance for given buttons</summary>
        /// <param name="buttons">AtfMouseButtons</param>
        /// <returns>MouseButtonsInterop instantiated from AtfMouseButtons</returns>
        public static implicit operator MouseButtonsInterop(AtfMouseButtons buttons)
        {
            return new MouseButtonsInterop(buttons);
        }

        /// <summary>
        /// Implicit conversion operator from WfMouseButtons to MouseButtonsInterop instance for given buttons</summary>
        /// <param name="buttons">WfMouseButtons</param>
        /// <returns>MouseButtonsInterop instantiated from WfMouseButtons</returns>
        public static implicit operator MouseButtonsInterop(WfMouseButtons buttons)
        {
            return new MouseButtonsInterop(buttons);
        }

        /// <summary>
        /// Implicit conversion operator from MouseButtonsInterop instance to AtfMouseButtons</summary>
        /// <param name="buttons">MouseButtonsInterop instance</param>
        /// <returns>AtfMouseButtons</returns>
        public static implicit operator AtfMouseButtons(MouseButtonsInterop buttons)
        {
            switch (buttons.ValueType)
            {
                case SupportedTypes.Atf:
                case SupportedTypes.WinForms:
                    return (AtfMouseButtons)buttons.Value;
                case SupportedTypes.Wpf:
                    throw new Exception("Interop for WPF mouse buttons not implemented yet");
                default:
                    throw new InvalidOperationException("Unhandled type specified");
            }
        }

        /// <summary>
        /// Implicit conversion operator from MouseButtonsInterop instance to WfMouseButtons</summary>
        /// <param name="buttons">MouseButtonsInterop instance</param>
        /// <returns>WfMouseButtons</returns>
        public static implicit operator WfMouseButtons(MouseButtonsInterop buttons)
        {
            switch (buttons.ValueType)
            {
                case SupportedTypes.Atf:
                case SupportedTypes.WinForms:
                    return (System.Windows.Forms.MouseButtons)buttons.Value;
                case SupportedTypes.Wpf:
                    throw new Exception("Interop for WPF mouse buttons not implemented yet");
                default:
                    throw new InvalidOperationException("Unhandled type specified");
            }
        }

        /// <summary>
        /// Converts WfMouseButtons to AtfMouseButtons</summary>
        /// <param name="buttons">WfMouseButtons</param>
        /// <returns>AtfMouseButtons</returns>
        public static AtfMouseButtons ToAtf(WfMouseButtons buttons) { return (AtfMouseButtons)buttons; }
        /// <summary>
        /// Converts AtfMouseButtons to WfMouseButtons</summary>
        /// <param name="buttons">AtfMouseButtons</param>
        /// <returns>WfMouseButtons</returns>
        public static WfMouseButtons ToWf(AtfMouseButtons buttons) { return (WfMouseButtons)buttons; }

        private long Value { get; set; }
        private SupportedTypes ValueType { get; set; }
    }
}