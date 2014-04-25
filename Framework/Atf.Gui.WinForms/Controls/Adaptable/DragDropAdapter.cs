//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter to add drag and drop support to adapted control</summary>
    public class DragDropAdapter : ControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="statusService">Status service, or null</param>
        public DragDropAdapter(IStatusService statusService)
        {
            m_statusService = statusService;
        }

        /// <summary>
        /// Gets whether a drop is in progress</summary>
        public bool IsDropping
        {
            get { return m_isDropping; }
        }

        /// <summary>
        /// Gets the mouse position in the AdaptedControl's client coordinates</summary>
        public Point MousePosition
        {
            get { return m_mousePosition; }
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.DragOver += control_DragOver;
            control.DragDrop += control_DragDrop;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.DragOver -= control_DragOver;
            control.DragDrop -= control_DragDrop;
        }

        private void control_DragOver(object sender, DragEventArgs e)
        {
            SetMousePosition(e);

            e.Effect = DragDropEffects.None;
            IInstancingContext instancingContext = AdaptedControl.ContextAs<IInstancingContext>();
            if (instancingContext != null &&
                instancingContext.CanInsert(e.Data))
            {
                    OnDragOver(e);
            }
        }

        /// <summary>
        /// Performs actions during drag-over events</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDragOver(DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void control_DragDrop(object sender, DragEventArgs e)
        {
            SetMousePosition(e);

            IInstancingContext instancingContext = AdaptedControl.ContextAs<IInstancingContext>();
            if (instancingContext != null &&
                instancingContext.CanInsert(e.Data))
            {
                try
                {
                    m_isDropping = true;

                    string name = "Drag and Drop".Localize();

                    ITransactionContext transactionContext = AdaptedControl.ContextAs<ITransactionContext>();
                    transactionContext.DoTransaction(
                        delegate
                        {
                            instancingContext.Insert(e.Data);
                            
                            if (m_statusService != null)
                                m_statusService.ShowStatus(name);
                        }, name);

                    AdaptedControl.Focus();
                }
                finally
                {
                    m_isDropping = false;
                }
            }
        }

        /// <summary>
        /// Updates mouse position</summary>
        /// <param name="e">Drag event arguments</param>
        protected void SetMousePosition(DragEventArgs e)
        {
            m_mousePosition = AdaptedControl.PointToClient(new Point(e.X, e.Y));
        }

        private readonly IStatusService m_statusService;
        /// <summary>
        /// Mouse position in the AdaptedControl's client coordinates</summary>
        protected Point m_mousePosition;
        private bool m_isDropping;
    }
}
