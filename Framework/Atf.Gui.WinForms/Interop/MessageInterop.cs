using System;

using AtfMessage = Sce.Atf.Input.Message;
using WfMessage = System.Windows.Forms.Message;

namespace Sce.Atf
{
    /// <summary>
    /// Converts messages between ATF and Windows to support interoperability between Windows and ATF</summary>
    class MessageInterop
    {
        /// <summary>
        /// Constructor</summary>
        private MessageInterop() {}

        /// <summary>
        /// Constructor with all message parameters</summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="msg">Message ID number</param>
        /// <param name="wparam">WParam field of the message</param>
        /// <param name="lparam">LParam field of the message</param>
        /// <param name="result">Result of handling message</param>
        public MessageInterop(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam, IntPtr result)
        {
            HWnd = hWnd;
            Msg = msg;
            WParam = wparam;
            LParam = lparam;
            Result = result;
        }

        /// <summary>
        /// Constructor with ATF message</summary>
        /// <param name="msg">ATF message</param>
        public MessageInterop(AtfMessage msg)
            : this(msg.HWnd, msg.Msg, msg.WParam, msg.LParam, msg.Result)
        {
        }

        /// <summary>
        /// Constructor with Windows message</summary>
        /// <param name="msg">Windows message</param>
        public MessageInterop(WfMessage msg)
            : this(msg.HWnd, msg.Msg, msg.WParam, msg.LParam, msg.Result)
        {
        }

        /// <summary>
        /// Implicit conversion operator from AtfMessage to MessageInterop instance for given message</summary>
        /// <param name="msg">AtfMessage</param>
        /// <returns>MessageInterop instantiated from AtfMessage</returns>
        public static implicit operator MessageInterop(AtfMessage msg) { return new MessageInterop(msg); }

        /// <summary>
        /// Implicit conversion operator from WfMessage to MessageInterop instance for given message</summary>
        /// <param name="msg">WfMessage</param>
        /// <returns>MessageInterop instantiated from WfMessage</returns>
        public static implicit operator MessageInterop(WfMessage msg) { return new MessageInterop(msg); }

        /// <summary>
        /// Implicit conversion operator from MessageInterop instance to AtfMessage</summary>
        /// <param name="msg">MessageInterop instance</param>
        /// <returns>AtfMessage</returns>
        public static implicit operator AtfMessage(MessageInterop msg) { return ToAtf(msg); }

        /// <summary>
        /// Implicit conversion operator from MessageInterop instance to WfMessage</summary>
        /// <param name="msg">MessageInterop instance</param>
        /// <returns>WfMessage</returns>
        public static implicit operator WfMessage(MessageInterop msg) { return ToWf(msg); }

        /// <summary>
        /// Converts AtfMessage to WfMessage</summary>
        /// <param name="msg">AtfMessage</param>
        /// <returns>WfMessage</returns>
        public static WfMessage ToWf(AtfMessage msg)
        {
            var newMsg = new WfMessage();
            newMsg.HWnd = msg.Result;
            newMsg.Msg = msg.Msg;
            newMsg.WParam = msg.WParam;
            newMsg.LParam = msg.LParam;
            newMsg.Result = msg.Result;
            return newMsg;
        }

        /// <summary>
        /// Converts WfMessage to AtfMessage</summary>
        /// <param name="msg">WfMessage</param>
        /// <returns>AtfMessage</returns>
        public static AtfMessage ToAtf(WfMessage msg)
        {
            var newMsg = new AtfMessage();
            newMsg.HWnd = msg.Result;
            newMsg.Msg = msg.Msg;
            newMsg.WParam = msg.WParam;
            newMsg.LParam = msg.LParam;
            newMsg.Result = msg.Result;
            return newMsg;
        }

        /// <summary>
        /// Gets window handle</summary>
        public IntPtr HWnd { get; private set; }
        /// <summary>
        /// Gets message ID number</summary>
        public int Msg { get; private set; }
        /// <summary>
        /// Gets wParam field of the message</summary>
        public IntPtr WParam { get; private set; }
        /// <summary>
        /// Gets lParam field of the message</summary>
        public IntPtr LParam { get; private set; }
        /// <summary>
        /// Gets result of handling message</summary>
        public IntPtr Result { get; private set; } 
    }
}
