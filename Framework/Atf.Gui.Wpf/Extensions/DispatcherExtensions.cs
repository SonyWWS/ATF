//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Threading;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Provides a set of commonly used Dispatcher extension methods
    /// </summary>
    public static class DispatcherExtensions
    {
        /// <summary>
        /// A simple threading extension method, to invoke a delegate
        /// on the correct thread if it is not currently on the correct thread
        /// which can be used with DispatcherObject types.
        /// </summary>
        /// <param name="dispatcher">The Dispatcher object on which to 
        /// perform the Invoke</param>
        /// <param name="action">The delegate to run</param>
        /// <param name="priority">The DispatcherPriority for the invoke.</param>
        public static void InvokeIfRequired(this Dispatcher dispatcher,
            Action action, DispatcherPriority priority)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(priority, action);
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// A simple threading extension method, to invoke a delegate
        /// on the correct thread if it is not currently on the correct thread
        /// which can be used with DispatcherObject types.
        /// </summary>
        /// <param name="dispatcher">The Dispatcher object on which to 
        /// perform the Invoke</param>
        /// <param name="action">The delegate to run</param>
        public static void InvokeIfRequired(this Dispatcher dispatcher, Action action)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }

        public static void InvokeIfRequired<T>(this Dispatcher dispatcher, Action<T> action, T arg)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.Invoke(DispatcherPriority.Normal, action, arg);
            }
            else
            {
                action(arg);
            }
        }

        public static TResult InvokeIfRequired<TResult>(this Dispatcher dispatcher, Func<TResult> action)
        {
            if (!dispatcher.CheckAccess())
            {
                return (TResult)dispatcher.Invoke(DispatcherPriority.Normal, action);
            }
            else
            {
                return action();
            }
        }

        public static TResult InvokeIfRequired<T, TResult>(this Dispatcher dispatcher, Func<T, TResult> action, T arg)
        {
            if (!dispatcher.CheckAccess())
            {
                return (TResult)dispatcher.Invoke(DispatcherPriority.Normal, action, arg);
            }
            else
            {
                return action(arg);
            }
        }


        public static void BeginInvokeIfRequired(this Dispatcher dispatcher,
            Action action, DispatcherPriority priority)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(priority, action);
            }
            else
            {
                action();
            }
        }

        public static void BeginInvokeIfRequired(this Dispatcher dispatcher, Action action)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
            }
            else
            {
                action();
            }
        }

        public static void BeginInvokeIfRequired<T>(this Dispatcher dispatcher, Action<T> action, T arg)
        {
            if (!dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(DispatcherPriority.Normal, action, arg);
            }
            else
            {
                action(arg);
            }
        }

        public static void WaitForPriority(this Dispatcher dispatcher, DispatcherPriority priority)
        {
            DispatcherFrame frame = new DispatcherFrame();
            DispatcherOperation dispatcherOperation = dispatcher.BeginInvoke(priority, new DispatcherOperationCallback(ExitFrameOperation), frame);
            Dispatcher.PushFrame(frame);
            if (dispatcherOperation.Status != DispatcherOperationStatus.Completed)
            {
                dispatcherOperation.Abort();
            }
        }

        private static object ExitFrameOperation(object obj)
        {
            ((DispatcherFrame)obj).Continue = false;
            return null;
        }
    }
}
