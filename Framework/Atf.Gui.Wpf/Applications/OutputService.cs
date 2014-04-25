//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Threading;

using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service that displays text output to user</summary>
    [Export(typeof(OutputService))]
    [Export(typeof(IOutputWriter))]
    [Export(typeof(IControlHostClient))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OutputService : IOutputWriter, IControlHostClient, IInitializable, Sce.Atf.Applications.ICommandClient
    {
        /// <summary>
        /// Gets or sets IControlHostService</summary>
        [Import]
        protected IControlHostService ControlHostService { get; set; }

        #region IInitializable

        /// <summary>
        /// Finishes initializing component by creating and registering OutputView control</summary>
        public void Initialize()
        {
            m_view = new OutputView();
            m_viewModel = new OutputVm();
            m_view.DataContext = m_viewModel;

            ControlHostService.RegisterControl(
                m_view, 
                "Output".Localize(),
                "View errors, warnings, and informative messages".Localize(), 
                Sce.Atf.Applications.StandardControlGroup.Bottom, 
                kId.ToString(),
                this);

            m_uiDispatcher = Dispatcher.CurrentDispatcher;
        }


        #endregion

        #region IOutputWriter Members

        /// <summary>
        /// Writes an output message of the given type</summary>
        /// <param name="type">Message type (Error, Warning or Info)</param>
        /// <param name="message">Message</param>
        public void Write(OutputMessageType type, string message)
        {
            m_uiDispatcher.BeginInvokeIfRequired(() =>
            {
                m_viewModel.OutputItems.Add(new OutputItemVm(DateTime.Now, type, message));
            });
        }

        /// <summary>
        /// Clears the writer</summary>
        public void Clear()
        {
            m_uiDispatcher.BeginInvokeIfRequired(() =>
            {
                m_viewModel.OutputItems.Clear();
            });
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(object control)
        {
            if (m_commandService != null)
                m_commandService.SetActiveClient(this);
        }

        /// <summary>
        /// Notifies the client that its control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(object control)
        {
            if (m_commandService != null)
                m_commandService.SetActiveClient(null);
        }

        /// <summary>
        /// Requests permission to close the client's control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <param name="mainWindowClosing">True if the application main window is closing</param>
        /// <returns>True if the control can close, or false to cancel.</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService will call its own
        /// UnregisterContent. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this control.</remarks>
        public bool Close(object control, bool mainWindowClosing)
        {
            return true;
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandObj">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandObj">Command to be done</param>
        public void DoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState commandState) { }

        #endregion

        private OutputVm m_viewModel;
        private OutputView m_view;
        private Dispatcher m_uiDispatcher;
        private static Guid kId = new Guid(0xd4d197e1, 0xbf89, 0x4f80, 0x97, 0x71, 0x5b, 0x79, 0x32, 0x63, 0x4e, 0x38);
       
        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;
    }
}
