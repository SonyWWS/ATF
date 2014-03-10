//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter to run a context menu on MouseUp with the right button</summary>
    public class ContextMenuAdapter : ControlAdapter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="providers">Enumeration of context menu command providers</param>
        public ContextMenuAdapter(
            ICommandService commandService,
            IEnumerable<Lazy<IContextMenuCommandProvider>> providers)
        {
            m_commandService = commandService;
            m_providers = providers;
        }

      
        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseUp += control_MouseUp;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.MouseUp -= control_MouseUp;
        }

        /// <summary>
        /// Gets or sets the coordinates of context menu location</summary>
        /// <remarks>The value is null if context menu is not opened</remarks> 
        public Point? TriggeringLocation
        {
            get; set;
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && ((Control.ModifierKeys & Keys.Alt) == 0))
            {
                Point p = new Point(e.X, e.Y);
                object target = null;

                foreach (IPickingAdapter pickingAdapter in AdaptedControl.AsAll<IPickingAdapter>())
                {
                    DiagramHitRecord hitRecord = pickingAdapter.Pick(p);
                    if (hitRecord.Item != null)
                    {
                        target = hitRecord.Item;
                        break;
                    }
                }

                // iter over IPickingAdapter
                if (target == null)
                {
                    foreach (IPickingAdapter2 pickingAdapter in AdaptedControl.AsAll<IPickingAdapter2>())
                    {
                        DiagramHitRecord hitRecord = pickingAdapter.Pick(p);
                        if (hitRecord.Item != null)
                        {
                            target = hitRecord.Item;
                            break;
                        }
                    }
                }

                object context = AdaptedControl.Context;
                TriggeringLocation = p;
                var commands = new List<object>(m_providers.GetCommands(context, target));

                OnContextMenuOpening(commands);


                Point screenP = AdaptedControl.PointToScreen(p);
                m_commandService.RunContextMenu(commands, screenP);
             }
             
        }

        /// <summary>
        /// Performs custom actions on command list before menu is displayed</summary>
        /// <param name="commands">Context menu commands to be displayed</param>
        protected virtual void OnContextMenuOpening(IList<object> commands)
        {
        }

        
        private readonly ICommandService m_commandService;
        private readonly IEnumerable<Lazy<IContextMenuCommandProvider>> m_providers;
    }
}
