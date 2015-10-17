//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Applications.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that displays the rendering performance of the currently active
    /// Control. Clients can customize how the target Control is measured or even
    /// monitor the performance of other objects entirely.</summary>
    [Export(typeof(PerformanceMonitor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PerformanceMonitor : IControlHostClient, IInitializable
    {
        /// <summary>
        /// Importing constructor with parameters</summary>
        /// <param name="controlRegistry">IControlRegistry</param>
        /// <param name="controlHostService">IControlHostService</param>
        [ImportingConstructor]
        public PerformanceMonitor(IControlRegistry controlRegistry, IControlHostService controlHostService)
        {
            m_controlRegistry = controlRegistry;
            m_controlHostService = controlHostService;
            m_controlInfo = new ControlInfo(
                "Performance Monitor", //Is the ID in the layout. We'll localize DisplayName instead.
                "Displays performance data on the currently active Control".Localize(),
                StandardControlGroup.Floating)
            {
                DisplayName = "Performance Monitor".Localize()
            };
            m_controlInfo.VisibleByDefault = false;
        }

        /// <summary>
        /// Constructor without parameters</summary>
        public PerformanceMonitor()
        {
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_controlHostService.RegisterControl(m_perfControl, m_controlInfo, this);
            m_controlRegistry.ActiveControlChanged += ActiveControlChanged;
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        void IControlHostClient.Activate(Control control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns><c>True</c> if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Sets the target whose performance we are monitoring. Raises the OnTargetChanging event.</summary>
        /// <param name="target">Target whose performance we are monitoring</param>
        /// <param name="name">>User readable name of the target</param>
        public void SetTarget(object target, string name)
        {
            var performanceTarget = target as IPerformanceTarget;
            if (performanceTarget == null)
            {
                var controlTarget = target as Control;
                if (controlTarget != null)
                    performanceTarget = new PerformanceMonitorControl.ControlAdapter(controlTarget);
            }

            var targetEventArgs = new TargetChangeEventArgs(performanceTarget, name);
            OnTargetChanging(targetEventArgs);

            if (!targetEventArgs.Cancel)
                m_perfControl.Bind(targetEventArgs.Target, targetEventArgs.Name);
        }

        /// <summary>
        /// Event argument for when this performance monitor is changing from one target
        /// window to another. Listeners have the option of changing how the target window
        /// is monitored. The default is to listen to the Control's Paint event. Listeners
        /// can also cancel this event to prevent changing from the previous target.</summary>
        public class TargetChangeEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="target">Target whose performance is monitored. Can be null.</param>
            /// <param name="name">User readable name of the target</param>
            public TargetChangeEventArgs(IPerformanceTarget target, string name)
            {
                Target = target;
                Name = name;
            }

            /// <summary>
            /// The new binding info, which can be null</summary>
            public IPerformanceTarget Target;

            /// <summary>
            /// The name of the target, which is displayed to the user</summary>
            public string Name;
        }

        /// <summary>
        /// Delegate for receiving the TargetChanging event</summary>
        /// <param name="source">PerformanceMonitorService object</param>
        /// <param name="e">Target change event argument, which can be modified or cancelled</param>
        public delegate void TargetChangeEventHandler(object source, TargetChangeEventArgs e);

        /// <summary>
        /// Event that is raised before the target whose performance is being monitored is changed</summary>
        public event TargetChangeEventHandler TargetChanging;

        /// <summary>
        /// Raises the TargetChanging event, giving listeners an opportunity to cancel the event
        /// or to modify the target info</summary>
        /// <param name="e">Target change event args</param>
        protected virtual void OnTargetChanging(TargetChangeEventArgs e)
        {
            if (TargetChanging != null)
            {
                foreach (Delegate listener in TargetChanging.GetInvocationList())
                {
                    listener.DynamicInvoke(this, e);
                    if (e.Cancel)
                        return;
                }
            }
        }

        /// <summary>
        /// Gets PerformanceMonitorControl</summary>
        protected PerformanceMonitorControl PerformanceMonitorControl
        {
            get { return m_perfControl; }
        }

        /// <summary>
        /// Gets ControlInfo</summary>
        protected ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

        private void ActiveControlChanged(object sender, EventArgs e)
        {
            ControlInfo targetInfo = m_controlRegistry.ActiveControl;
            if (targetInfo != null &&
                targetInfo.Control != m_perfControl)
            {
                SetTarget(targetInfo.Control, targetInfo.Name);
            }
        }

        private readonly PerformanceMonitorControl m_perfControl = new PerformanceMonitorControl();
        private readonly ControlInfo m_controlInfo;
        private readonly IControlRegistry m_controlRegistry;
        private readonly IControlHostService m_controlHostService;
    }
}
