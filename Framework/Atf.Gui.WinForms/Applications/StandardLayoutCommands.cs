//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that provides standard layout commands, like align left, or make
    /// widths equal. The plugin tries to work against the active context, and requires
    /// both the ISelectionContext and ILayoutContext interfaces to be implemented on it.
    /// Further, it examines the selected items to see if the layout context can set their
    /// x, y, width and height. Any commands that can be done on the selection will be
    /// enabled in the GUI. All commands are also available programmatically through
    /// public methods.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(StandardLayoutCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardLayoutCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public StandardLayoutCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        /// <summary>
        /// Initializes this MEF component by registering the standard layout commands</summary>
        /// <remarks>Derived classes can call UnregisterCommand and pass in the command tag,
        /// e.g., StandardCommand.FormatAlignLefts, to remove unwanted commands.</remarks>
        public virtual void Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.FormatAlignLefts, this);
            m_commandService.RegisterCommand(CommandInfo.FormatAlignTops, this);
            m_commandService.RegisterCommand(CommandInfo.FormatAlignRights, this);
            m_commandService.RegisterCommand(CommandInfo.FormatAlignCenters, this);
            m_commandService.RegisterCommand(CommandInfo.FormatAlignBottoms, this);
            m_commandService.RegisterCommand(CommandInfo.FormatAlignMiddles, this);

            m_commandService.RegisterCommand(CommandInfo.FormatMakeWidthEqual, this);
            m_commandService.RegisterCommand(CommandInfo.FormatMakeHeightEqual, this);
            m_commandService.RegisterCommand(CommandInfo.FormatMakeSizeEqual, this);
        }

        #endregion

        /// <summary>
        /// Moves the items so that their bounding boxes all have the Left (x) value
        /// of the left-most item</summary>
        /// <param name="items">Items to move</param>
        /// <param name="layoutContext">Layout context of items to move</param>
        public virtual void AlignLefts(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Rectangle bounds;
            LayoutContexts.GetBounds(layoutContext, items, out bounds);
            foreach (object item in items)
            {
                layoutContext.SetBounds(item, bounds, BoundsSpecified.X);
            }
        }

        /// <summary>
        /// Moves the items so that their bounding boxes all have the Top (y) value of the
        /// top-most item</summary>
        /// <param name="items">Items to move</param>
        /// <param name="layoutContext">Layout context of items to move</param>
        public virtual void AlignTops(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Rectangle bounds;
            LayoutContexts.GetBounds(layoutContext, items, out bounds);
            foreach (object item in items)
            {
                layoutContext.SetBounds(item, bounds, BoundsSpecified.Y);
            }
        }

        /// <summary>
        /// Moves the items so that their bounding boxes all have the Right (x + Width) value
        /// of the right-most item</summary>
        /// <param name="items">Items to move</param>
        /// <param name="layoutContext">Layout context of items to move</param>
        public virtual void AlignRights(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Rectangle bounds;
            LayoutContexts.GetBounds(layoutContext, items, out bounds);
            foreach (object item in items)
            {
                Rectangle itemBounds;
                layoutContext.GetBounds(item, out itemBounds);
                itemBounds.X = bounds.Right - itemBounds.Width;
                layoutContext.SetBounds(item, itemBounds, BoundsSpecified.X);
            }
        }

        /// <summary>
        /// Moves the items so that their bounding boxes all have the Bottom (y + Height) value
        /// of the bottom-most item</summary>
        /// <param name="items">Items to move</param>
        /// <param name="layoutContext">Layout context of items to move</param>
        public virtual void AlignBottoms(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Rectangle bounds;
            LayoutContexts.GetBounds(layoutContext, items, out bounds);
            foreach (object item in items)
            {
                Rectangle itemBounds;
                layoutContext.GetBounds(item, out itemBounds);
                itemBounds.Y = bounds.Bottom - itemBounds.Height;
                layoutContext.SetBounds(item, itemBounds, BoundsSpecified.Y);
            }
        }

        /// <summary>
        /// Moves the items left or right so that their bounding box centers all have the same
        /// X value, which is the X value of the center of the bounding box around the items'
        /// original positions</summary>
        /// <param name="items">Items to move</param>
        /// <param name="layoutContext">Layout context of items to move</param>
        public virtual void AlignCenters(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Rectangle bounds;
            LayoutContexts.GetBounds(layoutContext, items, out bounds);
            int boundsCenter = (bounds.Left + bounds.Right) / 2;
            foreach (object item in items)
            {
                Rectangle itemBounds;
                layoutContext.GetBounds(item, out itemBounds);
                itemBounds.X = boundsCenter - itemBounds.Width / 2;
                layoutContext.SetBounds(item, itemBounds, BoundsSpecified.X);
            }
        }

        /// <summary>
        /// Moves the items up or down so that their bounding box centers all have the same
        /// Y value, which is the Y value of the center of the bounding box around the items'
        /// original positions</summary>
        /// <param name="items">Items to move</param>
        /// <param name="layoutContext">Layout context of items to move</param>
        public virtual void AlignMiddles(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Rectangle bounds;
            LayoutContexts.GetBounds(layoutContext, items, out bounds);
            int boundsMiddle = (bounds.Top + bounds.Bottom) / 2;
            foreach (object item in items)
            {
                Rectangle itemBounds;
                layoutContext.GetBounds(item, out itemBounds);
                itemBounds.Y = boundsMiddle - itemBounds.Height / 2;
                layoutContext.SetBounds(item, itemBounds, BoundsSpecified.Y);
            }
        }

        /// <summary>
        /// Sets the size of all the items to that of the largest item</summary>
        /// <param name="items">Items to resize</param>
        /// <param name="layoutContext">Layout context of items to resize</param>
        public virtual void MakeSizeEqual(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Size maxSize = GetMaxSize(items, layoutContext);
            foreach (object item in items)
            {
                Rectangle bounds;
                layoutContext.GetBounds(item, out bounds);
                bounds.Size = maxSize;
                layoutContext.SetBounds(item, bounds, BoundsSpecified.Size);
            }
        }

        /// <summary>
        /// Sets the width of all the items to that of the widest item</summary>
        /// <param name="items">Items to resize</param>
        /// <param name="layoutContext">Layout context of items to resize</param>
        public virtual void MakeWidthEqual(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Size maxSize = GetMaxSize(items, layoutContext);
            foreach (object item in items)
            {
                Rectangle bounds;
                layoutContext.GetBounds(item, out bounds);
                bounds.Width = maxSize.Width;
                layoutContext.SetBounds(item, bounds, BoundsSpecified.Width);
            }
        }

        /// <summary>
        /// Sets the height of all of the items to that of the tallest item</summary>
        /// <param name="items">Items to resize</param>
        /// <param name="layoutContext">Layout context of items to resize</param>
        public virtual void MakeHeightEqual(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Size maxSize = GetMaxSize(items, layoutContext);
            foreach (object item in items)
            {
                Rectangle bounds;
                layoutContext.GetBounds(item, out bounds);
                bounds.Height = maxSize.Height;
                layoutContext.SetBounds(item, bounds, BoundsSpecified.Height);
            }
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            bool canDo = false;

            ILayoutContext layoutContext = m_contextRegistry.GetActiveContext<ILayoutContext>();
            ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();

            if (layoutContext != null &&
                selectionContext != null)
            {
                BoundsSpecified itemFlags = GetBoundsSpecified(layoutContext, selectionContext.Selection);
                if (itemFlags != BoundsSpecified.None)
                {
                    switch ((StandardCommand)commandTag)
                    {
                        case StandardCommand.FormatAlignLefts:
                        case StandardCommand.FormatAlignCenters: //horizontal middle
                        case StandardCommand.FormatAlignRights:
                            canDo = (itemFlags & BoundsSpecified.X) != 0;
                            break;

                        case StandardCommand.FormatAlignTops:
                        case StandardCommand.FormatAlignMiddles: //vertical middle
                        case StandardCommand.FormatAlignBottoms:
                            canDo = (itemFlags & BoundsSpecified.Y) != 0;
                            break;

                        case StandardCommand.FormatMakeWidthEqual:
                            canDo = (itemFlags & BoundsSpecified.Width) != 0;
                            break;

                        case StandardCommand.FormatMakeHeightEqual:
                            canDo = (itemFlags & BoundsSpecified.Height) != 0;
                            break;

                        case StandardCommand.FormatMakeSizeEqual:
                            canDo = (itemFlags & BoundsSpecified.Size) == BoundsSpecified.Size;
                            break;
                    }
                }
            }

            return canDo;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
            ILayoutContext layoutContext = m_contextRegistry.GetActiveContext<ILayoutContext>();

            if (layoutContext != null &&
                selectionContext != null)
            {
                IEnumerable<object> items = selectionContext.Selection;
                BoundsSpecified itemFlags = GetBoundsSpecified(layoutContext, selectionContext.Selection);
                if (itemFlags != BoundsSpecified.None)
                {
                    string commandName = null;
                    switch ((StandardCommand)commandTag)
                    {
                        case StandardCommand.FormatAlignLefts:
                            commandName = CommandInfo.FormatAlignLefts.MenuText;
                            break;

                        case StandardCommand.FormatAlignRights:
                            commandName = CommandInfo.FormatAlignRights.MenuText;
                            break;

                        case StandardCommand.FormatAlignCenters: //horizontal middle
                            commandName = CommandInfo.FormatAlignCenters.MenuText;
                            break;

                        case StandardCommand.FormatAlignTops: //horizontal middle
                            commandName = CommandInfo.FormatAlignTops.MenuText;
                            break;

                        case StandardCommand.FormatAlignBottoms:
                            commandName = CommandInfo.FormatAlignBottoms.MenuText;
                            break;

                        case StandardCommand.FormatAlignMiddles: //vertical middle
                            commandName = CommandInfo.FormatAlignMiddles.MenuText;
                            break;

                        case StandardCommand.FormatMakeWidthEqual:
                            commandName = CommandInfo.FormatMakeWidthEqual.MenuText;
                            break;

                        case StandardCommand.FormatMakeHeightEqual:
                            commandName = CommandInfo.FormatMakeHeightEqual.MenuText;
                            break;

                        case StandardCommand.FormatMakeSizeEqual:
                            commandName = CommandInfo.FormatMakeSizeEqual.MenuText;
                            break;
                    }

                    ITransactionContext transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
                    transactionContext.DoTransaction(delegate
                        {
                            switch ((StandardCommand)commandTag)
                            {
                                case StandardCommand.FormatAlignLefts:
                                    AlignLefts(items, layoutContext);
                                    break;

                                case StandardCommand.FormatAlignRights:
                                    AlignRights(items, layoutContext);
                                    break;

                                case StandardCommand.FormatAlignCenters: // horizontal center
                                    AlignCenters(items, layoutContext);
                                    break;

                                case StandardCommand.FormatAlignTops:
                                    AlignTops(items, layoutContext);
                                    break;

                                case StandardCommand.FormatAlignBottoms:
                                    AlignBottoms(items, layoutContext);
                                    break;

                                case StandardCommand.FormatAlignMiddles: // vertical center
                                    AlignMiddles(items, layoutContext);
                                    break;

                                case StandardCommand.FormatMakeWidthEqual:
                                    MakeWidthEqual(items, layoutContext);
                                    break;

                                case StandardCommand.FormatMakeHeightEqual:
                                    MakeHeightEqual(items, layoutContext);
                                    break;

                                case StandardCommand.FormatMakeSizeEqual:
                                    MakeSizeEqual(items, layoutContext);
                                    break;
                            }
                        },
                        commandName);
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private BoundsSpecified GetBoundsSpecified(ILayoutContext layoutContext, IEnumerable<object> items)
        {
            BoundsSpecified itemFlags = BoundsSpecified.None;
            foreach (object item in items)
                itemFlags |= layoutContext.CanSetBounds(item);

            return itemFlags;
        }

        private Size GetMaxSize(IEnumerable<object> items, ILayoutContext layoutContext)
        {
            Size size = new Size();
            foreach (object item in items)
            {
                Rectangle bounds;
                layoutContext.GetBounds(item, out bounds);
                size = new Size(
                    Math.Max(size.Width, bounds.Width),
                    Math.Max(size.Height, bounds.Height));
            }
            return size;
        }

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
