//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using AtfMouseButtons = Sce.Atf.Input.MouseButtons;
using AtfMouseEventArgs = Sce.Atf.Input.MouseEventArgs;

using WfMouseButtons = System.Windows.Forms.MouseButtons;
using WfMouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Sce.Atf
{
    /// <summary>
    /// Converts mouse event arguments between ATF and Windows to support interoperability for events between Windows and ATF.
    /// These mouse event arguments provide data for System.Windows.Forms.Control.MouseUp, System.Windows.Forms.Control.MouseDown,
    /// and System.Windows.Forms.Control.MouseMove events.</summary>
    public class MouseEventArgsInterop
    {
        /// <summary>
        /// Constructor with AtfMouseButtons and event data</summary>
        /// <param name="button">AtfMouseButtons button clicked</param>
        /// <param name="clicks">Number of times button was pressed and released</param>
        /// <param name="x">X-coordinate of mouse click</param>
        /// <param name="y">Y-coordinate of mouse click</param>
        /// <param name="delta">Signed count of the number of detents the mouse wheel has rotated</param>
        public MouseEventArgsInterop(AtfMouseButtons button, int clicks, int x, int y, int delta)
        {
            Button = button;
            Clicks = clicks;
            X = x;
            Y = y;
            Delta = delta;
        }

        /// <summary>
        /// Constructor with WfMouseButtons and event data</summary>
        /// <param name="button">WfMouseButtons button clicked</param>
        /// <param name="clicks">Number of times button was pressed and released</param>
        /// <param name="x">X-coordinate of mouse click</param>
        /// <param name="y">Y-coordinate of mouse click</param>
        /// <param name="delta">Signed count of the number of detents the mouse wheel has rotated</param>
        public MouseEventArgsInterop(WfMouseButtons button, int clicks, int x, int y, int delta)
            : this(MouseButtonsInterop.ToAtf(button), clicks, x, y, delta) { }

        /// <summary>
        /// Constructor with AtfMouseEventArgs</summary>
        /// <param name="args">AtfMouseEventArgs</param>
        public MouseEventArgsInterop(AtfMouseEventArgs args)
            : this(args.Button, args.Clicks, args.X, args.Y, args.Delta) {}

        /// <summary>
        /// Constructor with WfMouseEventArgs</summary>
        /// <param name="args">WfMouseEventArgs</param>
        public MouseEventArgsInterop(WfMouseEventArgs args)
            : this(args.Button, args.Clicks, args.X, args.Y, args.Delta) {}

        /// <summary>
        /// Implicit conversion operator from WfMouseEventArgs to MouseEventArgsInterop instance for given event arguments</summary>
        /// <param name="mouseEventArgs">WfMouseEventArgs</param>
        /// <returns>MouseEventArgsInterop instantiated from WfMouseEventArgs</returns>
        public static implicit operator MouseEventArgsInterop(WfMouseEventArgs mouseEventArgs)
        {
            return new MouseEventArgsInterop(mouseEventArgs);
        }

        /// <summary>
        /// Implicit conversion operator from MouseEventArgsInterop instance to AtfMouseButtons in that instance</summary>
        /// <param name="args">MouseEventArgsInterop instance</param>
        /// <returns>AtfMouseButtons from MouseEventArgsInterop instance</returns>
        public static implicit operator AtfMouseButtons(MouseEventArgsInterop args)
        {
            return args.Button;
        }

        /// <summary>
        /// Implicit conversion operator from MouseEventArgsInterop instance to MouseButtonsInterop instance</summary>
        /// <param name="args">MouseEventArgsInterop instance</param>
        /// <returns>MouseButtonsInterop instance</returns>
        public static implicit operator WfMouseButtons(MouseEventArgsInterop args)
        {
            return new MouseButtonsInterop(args.Button);
        }

        /// <summary>
        /// Implicit conversion operator from MouseEventArgsInterop instance to WfMouseEventArgs instance</summary>
        /// <param name="args">MouseEventArgsInterop instance</param>
        /// <returns>WfMouseEventArgs instance constructed from data in MouseEventArgsInterop instance</returns>
        public static implicit operator WfMouseEventArgs(MouseEventArgsInterop args)
        {
            return new WfMouseEventArgs(args, args.Clicks, args.X, args.Y, args.Delta);
        }

        /// <summary>
        /// Creates AtfMouseEventArgs instance from data in WfMouseEventArgs instance</summary>
        /// <param name="args">WfMouseEventArgs instance</param>
        /// <returns>AtfMouseEventArgs instance</returns>
        public static AtfMouseEventArgs ToAtf(WfMouseEventArgs args)
        {
            return new AtfMouseEventArgs(MouseButtonsInterop.ToAtf(args.Button), args.Clicks, args.X, args.Y, args.Delta);
        }

        /// <summary>
        /// Creates WfMouseEventArgs instance from data in AtfMouseEventArgs instance</summary>
        /// <param name="args">AtfMouseEventArgs instance</param>
        /// <returns>WfMouseEventArgs instance</returns>
        public static WfMouseEventArgs ToWf(AtfMouseEventArgs args)
        {
            return new WfMouseEventArgs(MouseButtonsInterop.ToWf(args.Button), args.Clicks, args.X, args.Y, args.Delta);
        }
    
        // public static AtfMouseEventArgs ToAtf(WfMouseEventArgs)

        /// <summary>
        /// Gets AtfMouseButtons</summary>
        public AtfMouseButtons Button { get; private set; }
        /// <summary>
        /// Gets Number of times button was pressed and released</summary>
        public int Clicks { get; private set; }
        /// <summary>
        /// Gets X-coordinate of mouse click</summary>
        public int X { get; private set; }
        /// <summary>
        /// Gets Y-coordinate of mouse click</summary>
        public int Y { get; private set; }
        /// <summary>
        /// Gets Signed count of the number of detents the mouse wheel has rotated</summary>
        public int Delta { get; private set; }
    }
}