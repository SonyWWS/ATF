//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for contexts that support positioning and sizing items. Contains methods
    /// for getting and setting item bounding rectangles, information on which parts of
    /// the bounds are meaningful, and which parts of bounds can be set.</summary>
    public interface ILayoutContext
    {
        /// <summary>
        /// Returns the smallest rectangle that bounds the item</summary>
        /// <param name="item">Item</param>
        /// <param name="bounds">Bounding rectangle of item</param>
        /// <returns>Value indicating which parts of bounding rectangle are meaningful</returns>
        BoundsSpecified GetBounds(object item, out Rect bounds);

        /// <summary>
        /// Returns a value indicating which parts of the item's bounds can be set</summary>
        /// <param name="item">Item</param>
        /// <returns>Value indicating which parts of the item's bounds can be set</returns>
        BoundsSpecified CanSetBounds(object item);

        /// <summary>
        /// Sets the bounds of the item</summary>
        /// <param name="item">Item</param>
        /// <param name="oldBounds">Old Item bounds</param>
        /// <param name="newBounds">New item bounds</param>
        /// <param name="specified">Which parts of bounds are being set</param>
        void SetBounds(object item, Rect oldBounds, Rect newBounds, BoundsSpecified specified);
    }

    /// <summary>
    /// Useful static/extension methods on ILayoutContext</summary>
    public static class LayoutContexts
    {
        /// <summary>
        /// Gets the smallest rectangle that bounds all of the items</summary>
        /// <param name="layoutContext">Layout context</param>
        /// <param name="items">Items</param>
        /// <param name="bounds">Bounding rectangle (output parameter)</param>
        /// <returns>Rectangle that bounds all of the items</returns>
        public static BoundsSpecified GetBounds(this ILayoutContext layoutContext, IEnumerable<object> items, out Rect bounds)
        {
            BoundsSpecified resultFlags = BoundsSpecified.None;
            Rect resultBounds = Rect.Empty;
            foreach (object item in items)
            {
                Rect itemBounds;
                BoundsSpecified itemFlags = layoutContext.GetBounds(item, out itemBounds);
                if (itemFlags == BoundsSpecified.All)
                {
                    if (resultBounds.IsEmpty)
                    {
                        resultBounds = itemBounds;
                        resultFlags = itemFlags;
                    }
                    else
                    {
                        resultBounds = Rect.Union(resultBounds, itemBounds);
                        resultFlags &= itemFlags;
                    }
                }
            }

            bounds = resultBounds;
            return resultFlags;
        }

        /// <summary>
        /// Moves the item, so that the center of its bounding box is at the given point</summary>
        /// <param name="layoutContext">Layout context containing item</param>
        /// <param name="item">Item to center</param>
        /// <param name="center">Centering point</param>
        public static void Center(this ILayoutContext layoutContext, object item, Point center)
        {
            Rect oldBounds;
            layoutContext.GetBounds(item, out oldBounds);
            Point topLeft = new Point(
                center.X - oldBounds.Width / 2,
                center.Y - oldBounds.Height / 2);

            layoutContext.SetBounds(item, oldBounds, new Rect(topLeft, oldBounds.Size), BoundsSpecified.Location);
        }

        /// <summary>
        /// Moves all items so that the center of their bounding box is at the given point</summary>
        /// <param name="layoutContext">Layout context containing item</param>
        /// <param name="items">Items to center</param>
        /// <param name="center">Centering point</param>
        public static void Center(this ILayoutContext layoutContext, IEnumerable<object> items, Point center)
        {
            Rect bounds;
            GetBounds(layoutContext, items, out bounds);

            // calculate offset
            Point offset = new Point(
                center.X - (bounds.Left + bounds.Width / 2),
                center.Y - (bounds.Top + bounds.Height / 2));

            foreach (object item in items)
            {
                Rect itemBounds;
                layoutContext.GetBounds(item, out itemBounds);

                Point topLeft = new Point(
                    itemBounds.Left + offset.X,
                    itemBounds.Top + offset.Y);

                layoutContext.SetBounds(item, itemBounds, new Rect(topLeft, itemBounds.Size), BoundsSpecified.Location);
            }
        }
    }
}
