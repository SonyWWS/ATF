//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that uses an IPickingAdapter and a timer to generate events
    /// when the user hovers over items</summary>
    public class HoverAdapter : ControlAdapter, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        public HoverAdapter()
        {
            m_hoverTimer = new Timer();
            m_hoverTimer.Interval = 10;        // Set to a low value so a tooltip can be shown with a short delay
            m_hoverTimer.Tick += hoverTimer_Tick;
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        public void Dispose()
        {
            StopHover();
            m_hoverTimer.Dispose();
        }

        #endregion

        /// <summary>
        /// Event that is raised after user starts hovering</summary>
        public event EventHandler<HoverEventArgs<object, object>> HoverStarted;

        /// <summary>
        /// Performs actions after hover started</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnHoverStarted(HoverEventArgs<object, object> e)
        {
        }

        /// <summary>
        /// Event that is raised after user stops hovering</summary>
        public event EventHandler HoverStopped;

        /// <summary>
        /// Performs actions after hover stopped</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnHoverStopped(EventArgs e)
        {
        }

        /// <summary>
        /// Gets whether the user is hovering over an item</summary>
        public bool Hovering
        {
            get { return m_hovering; }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            control.ContextChanged += control_ContextChanged;
            m_pickingAdapters = control.AsAll<IPickingAdapter2>().ToArray();
            Array.Reverse(m_pickingAdapters);

        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            m_hoverItem = null;
            m_hoverPart = null;
            StopHover();            
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseDown += control_MouseDown;
            control.MouseMove += control_MouseMove;
            control.MouseLeave += control_MouseLeave;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            StopHover();
            m_pickingAdapters = null;           
            control.MouseDown -= control_MouseDown;
            control.MouseMove -= control_MouseMove;
            control.MouseLeave -= control_MouseLeave;
            control.ContextChanged -= control_ContextChanged;
        }

        private void control_MouseMove(object sender, MouseEventArgs e)
        {           
            if (e.Button == MouseButtons.None && AdaptedControl.Focused)
            {                
                DiagramHitRecord hitRecord = null;
                foreach (var pickingAdapter in m_pickingAdapters)
                {
                    hitRecord = pickingAdapter.Pick(e.Location);
                    if (hitRecord.Item != null) break;                    
                }
               
                if (hitRecord != null &&
                    (hitRecord.Item != m_hoverItem || hitRecord.Part != m_hoverPart ||
                    hitRecord.SubItem != m_hoverSubItem || hitRecord.SubPart != m_hoverSubPart))
                    StartHover(hitRecord);
            }
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            StopHover();
        }

        private void control_MouseLeave(object sender, EventArgs e)
        {
            StopHover();
        }
        
        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            StopHover();
            if (m_hoverItem != null)
            {
                m_hovering = true;
                var hoverArgs = new HoverEventArgs<object, object>(m_hoverItem, m_hoverPart, m_hoverSubItem, m_hoverSubPart, AdaptedControl);
                OnHoverStarted(hoverArgs);
                HoverStarted.Raise(this, hoverArgs);
            }
        }

        private void StartHover(DiagramHitRecord hitRecord)
        {
            m_hoverItem = hitRecord.Item;
            m_hoverPart = hitRecord.Part;
            m_hoverSubItem = hitRecord.SubItem;
            m_hoverSubPart = hitRecord.SubPart;
            StopHover();
            m_hoverTimer.Start();
        }

        private void StopHover()
        {            
            if (m_hovering)
            {
                m_hovering = false;
                OnHoverStopped(EventArgs.Empty);
                HoverStopped.Raise(this, EventArgs.Empty);
            }
            m_hoverTimer.Stop();
        }

        private IPickingAdapter2[] m_pickingAdapters;
        private readonly Timer m_hoverTimer;
        private object m_hoverItem;
        private object m_hoverPart;
        private object m_hoverSubItem;
        private object m_hoverSubPart;
        private bool m_hovering;
    }
}
