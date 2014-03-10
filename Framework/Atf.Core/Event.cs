//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf
{
    /// <summary>
    /// Event extension methods</summary>
    public static class Event
    {
        /// <summary>
        /// Raises an event</summary>
        /// <param name="handler">Event or null</param>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
                handler(sender, e);
        }

        /// <summary>
        /// Raises an event</summary>
        /// <typeparam name="T">Event type, derived from EventArgs</typeparam>
        /// <param name="handler">Handler or null</param>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        public static void Raise<T>(this EventHandler<T> handler, object sender, T e)
            where T : EventArgs
        {
            if (handler != null)
                handler(sender, e);
        }
        
        /// <summary>
        /// Raises a cancellable event</summary>
        /// <param name="handler">Handler or null</param>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        /// <returns>True iff the event was cancelled</returns>
        public static bool RaiseCancellable(this CancelEventHandler handler, object sender, CancelEventArgs e)
        {
            if (handler != null)
            {
                foreach (CancelEventHandler h in handler.GetInvocationList())
                {
                    h(sender, e);
                    if (e.Cancel)
                        break;
                }
            }

            return e.Cancel;
        }

        /// <summary>
        /// Raises a cancellable event</summary>
        /// <typeparam name="T">Event type, derived from CancelEventArgs</typeparam>
        /// <param name="handler">Handler or null</param>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        /// <returns>True iff the event was cancelled</returns>
        public static bool RaiseCancellable<T>(this EventHandler<T> handler, object sender, T e)
            where T : CancelEventArgs
        {
            if (handler != null)
            {
                foreach (EventHandler<T> h in handler.GetInvocationList())
                {
                    h(sender, e);
                    if (e.Cancel)
                        break;
                }
            }

            return e.Cancel;
        }
    }
}
