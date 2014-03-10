//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using AtfKeys = Sce.Atf.Input.Keys;
using AtfKeyEventArgs = Sce.Atf.Input.KeyEventArgs;

using WfKeys = System.Windows.Forms.Keys;
using WfKeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace Sce.Atf
{
    /// <summary>
    /// Converts key event arguments between ATF and Windows to support interoperability for events between Windows and ATF</summary>
    public static class KeyEventArgsInterop
    {
/*
        private KeyEventArgsInterop() {}


        public KeyEventArgsInterop(AtfKeys keys)
        {
            KeyData = keys;
        }

        public KeyEventArgsInterop(AtfKeyEventArgs keyEventArgs)
        {
            KeyData = keyEventArgs.KeyData;
        }


        public KeyEventArgsInterop(WfKeys keys)
        {
            KeyData = keys;
        }

        public KeyEventArgsInterop(WfKeyEventArgs keyEventArgs)
        {
            KeyData = keyEventArgs.KeyData;
        }


        public static implicit operator KeyEventArgsInterop(AtfKeys keys)
        {
            return new KeyEventArgsInterop(keys);
        }

        public static implicit operator KeyEventArgsInterop(AtfKeyEventArgs keyEventArgs)
        {
            return new KeyEventArgsInterop(keyEventArgs);
        }

        public static implicit operator KeyEventArgsInterop(WfKeys keys)
        {
            return new KeyEventArgsInterop(keys);
        }

        public static implicit operator KeyEventArgsInterop(WfKeyEventArgs keyEventArgs)
        {
            return new KeyEventArgsInterop(keyEventArgs);
        }


        public static implicit operator AtfKeys(KeyEventArgsInterop keyEventArgsInterop)
        {
            return keyEventArgsInterop.KeyData;
        }

        public static implicit operator AtfKeyEventArgs(KeyEventArgsInterop keyEventArgsInterop)
        {
            return new AtfKeyEventArgs(keyEventArgsInterop.KeyData);
        }

        public static implicit operator WfKeys(KeyEventArgsInterop keyEventArgsInterop)
        {
            return keyEventArgsInterop.KeyData;
        }

        public static implicit operator WfKeyEventArgs(KeyEventArgsInterop keyEventArgsInterop)
        {
            return new WfKeyEventArgs(keyEventArgsInterop.KeyData);
        }
*/


        /// <summary>
        /// Converts WfKeyEventArgs to AtfKeyEventArgs</summary>
        /// <param name="args">WfKeyEventArgs</param>
        /// <returns>AtfKeyEventArgs</returns>
        public static AtfKeyEventArgs ToAtf(WfKeyEventArgs args)
        {
            return new AtfKeyEventArgs(KeysInterop.ToAtf(args.KeyData));
        }

        /// <summary>
        /// Converts AtfKeyEventArgs to WfKeyEventArgs</summary>
        /// <param name="args">AtfKeyEventArgs</param>
        /// <returns>WfKeyEventArgs</returns>
        public static WfKeyEventArgs ToWf(AtfKeyEventArgs args)
        {
            return new WfKeyEventArgs(KeysInterop.ToWf(args.KeyData));
        }


/*
        private KeysInterop KeyData{ get; set; }
*/
    }
}