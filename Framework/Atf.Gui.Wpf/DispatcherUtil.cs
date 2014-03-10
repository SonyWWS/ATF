//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Threading;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Dispatcher utilities providing a set of commonly used Dispatcher extension methods</summary>
    internal static class DispatcherUtil
    {
        /// <summary>
        /// A simple threading extension method to invoke a delegate
        /// on the correct thread if it is not currently on the correct thread.
        /// This method can be used with DispatcherObject types.</summary>
        /// <param name="dispatcher">The Dispatcher object on which to perform the Invoke</param>
        /// <param name="action">The delegate to run</param>
        internal static void InvokeIfRequired(this Dispatcher dispatcher, Action action)
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

        internal static TResult InvokeIfRequired<TResult>(this Dispatcher dispatcher, Func<TResult> action)
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

        internal static void BeginInvokeIfRequired(this Dispatcher dispatcher, Action action)
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

        internal static void WaitForPriority(this Dispatcher dispatcher, DispatcherPriority priority)
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
