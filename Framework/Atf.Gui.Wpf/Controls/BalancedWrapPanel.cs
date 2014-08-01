//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;


namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Positions child elements sequentially from left to right or top to bottom. When elements 
    /// extend beyond the panel edge, elements are positioned in the next row or column. </summary>
    public class BalancedWrapPanel : VirtualizingWrapPanel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the items in the last line
        /// should be aligned with the rest of the children (only when ItemWidth/
        /// ItemHeight is set). </summary>
        public bool AlignLastItems
        {
            get { return (bool)GetValue(AlignLastItemsProperty); }
            set { SetValue(AlignLastItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the AlignLastItems dependency property.</summary>
        public static readonly DependencyProperty AlignLastItemsProperty =
            DependencyProperty.Register(
                "AlignLastItems",
                typeof(bool),
                typeof(BalancedWrapPanel),
                new PropertyMetadata(false, OnAlignLastItemsPropertyChanged));

        /// <summary>
        /// Positions child elements based on the size of the panel.</summary>
        /// <param name="finalSize">The final area within the parent that this element 
        /// should use to arrange itself and its children.</param>
        /// <returns>Returns the original finalSize</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Variables tracking the size of the current line, and the maximum size available to fill.
            UVSize lineSize = new UVSize(this.Orientation);
            UVSize maximumSize = new UVSize(this.Orientation, finalSize.Width, finalSize.Height);
            
            double itemWidth = this.ItemWidth;
            double itemHeight = this.ItemHeight;
            double indirectOffset = 0.0;
            double itemU = (this.Orientation == Orientation.Horizontal) ? itemWidth : itemHeight;
            
            bool hasFixedWidth = !itemWidth.IsNaN();
            bool hasFixedHeight = !itemHeight.IsNaN();
            bool useItemU = (this.Orientation == Orientation.Horizontal) ? hasFixedWidth : hasFixedHeight;
            
            double? directDelta = (this.Orientation == Orientation.Horizontal) ?
                (hasFixedWidth ? (double?)itemWidth : null) :
                (hasFixedHeight ? (double?)itemHeight : null);

            // Measure each of the Children.  We will process the elements one
            // line at a time, just like during measure, but we will wait until
            // we've completed an entire line of elements before arranging them.
            // The lineStart and lineEnd variables track the size of the
            // currently arranged line.
            UIElementCollection internalChildren = base.InternalChildren;
            int count = internalChildren.Count;

            int lineStart = 0;
            for (int lineEnd = 0; lineEnd < count; lineEnd++)
            {
                UIElement element = internalChildren[lineEnd];
                if (element != null)
                {
                    // Get the size of the element
                    UVSize elementSize = new UVSize(
                        this.Orientation,
                        hasFixedWidth ? itemWidth : element.DesiredSize.Width,
                        hasFixedHeight ? itemHeight : element.DesiredSize.Height);

                    // If this element falls off the edge of the line
                    if (NumericUtil.IsGreaterThan(lineSize.U + elementSize.U, maximumSize.U))
                    {
                        // Then we just completed a line and we should arrange it
                        ArrangeLine(lineStart, lineEnd, directDelta, maximumSize.U, indirectOffset, lineSize.V);
                        
                        indirectOffset += lineSize.V;
                        lineSize = elementSize;

                        // If the current element is larger than the maximum size
                        if (NumericUtil.IsGreaterThan(elementSize.U, maximumSize.U))
                        {
                            // Arrange the element as a single line
                            ArrangeLine(lineEnd, ++lineEnd, directDelta, maximumSize.U, indirectOffset, elementSize.V);
                            
                            indirectOffset += elementSize.V;
                            lineSize = new UVSize(this.Orientation);
                        }

                        // Advance the start index to a new line after arranging
                        lineStart = lineEnd;
                    }
                    else
                    {
                        // Otherwise just add the element to the end of the line
                        lineSize.U += elementSize.U;
                        lineSize.V = Math.Max(elementSize.V, lineSize.V);
                    }
                }
            }

            if (lineStart < internalChildren.Count)
            {
                ArrangeLine(lineStart, count, directDelta, maximumSize.U, indirectOffset, lineSize.V);
            }
            
            return finalSize;
        }

        private void ArrangeLine(int lineStart, int lineEnd, double? directDelta, double directMaximum, double indirectOffset, double indirectGrowth)
        {
            bool isHorizontal = this.Orientation == Orientation.Horizontal;
            UIElementCollection children = base.InternalChildren;
            double directLength = 0.0;
            double itemCount = 0.0;
            double itemLength = isHorizontal ? ItemWidth : ItemHeight;

            if (AlignLastItems && !itemLength.IsNaN())
            {
                // Length is easy to calculate in this case
                itemCount = Math.Floor(directMaximum / itemLength);
                directLength = itemCount * itemLength;
            }
            else
            {
                // Make first pass to calculate the slack space
                itemCount = lineEnd - lineStart;
                for (int index = lineStart; index < lineEnd; index++)
                {
                    // Get the size of the element
                    UIElement element = children[index];
                    UVSize elementSize = new UVSize(this.Orientation, element.DesiredSize.Width,
                                                    element.DesiredSize.Height);

                    // Determine if we should use the element's desired size or the
                    // fixed item width or height
                    double directGrowth = directDelta != null
                                              ? directDelta.Value
                                              : elementSize.U;

                    // Update total length
                    directLength += directGrowth;
                }
            }

            // Determine slack
            double directSlack = directMaximum - directLength;
            double directSlackSlice = directSlack / (itemCount + 1.0);
            double directOffset = directSlackSlice;

            // Make second pass to arrange items
            for (int index = lineStart; index < lineEnd; index++)
            {
                // Get the size of the element
                UIElement element = children[index];
                UVSize elementSize = new UVSize(this.Orientation, element.DesiredSize.Width, element.DesiredSize.Height);

                // Determine if we should use the element's desired size or the
                // fixed item width or height
                double directGrowth = directDelta != null ?
                    directDelta.Value :
                    elementSize.U;

                // Arrange the element
                Rect bounds = isHorizontal ?
                    new Rect(directOffset, indirectOffset, directGrowth, indirectGrowth) :
                    new Rect(indirectOffset, directOffset, indirectGrowth, directGrowth);
                element.Arrange(bounds);

                // Update offset for next time
                directOffset += directGrowth + directSlackSlice;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UVSize
        {
            internal double U;
            internal double V;
            private Orientation _orientation;
            
            internal UVSize(Orientation orientation, double width, double height)
            {
                this.U = this.V = 0.0;
                this._orientation = orientation;
                this.Width = width;
                this.Height = height;
            }

            internal UVSize(Orientation orientation)
            {
                this.U = this.V = 0.0;
                this._orientation = orientation;
            }

            internal double Width
            {
                get 
                {
                    return (this._orientation != Orientation.Horizontal) ? 
                        this.V : 
                        this.U; 
                }
                set
                {
                    if (this._orientation == Orientation.Horizontal)
                    {
                        this.U = value;
                    }
                    else
                    {
                        this.V = value;
                    }
                }
            }
            
            internal double Height
            {
                get
                {
                    return (this._orientation != Orientation.Horizontal) ? 
                        this.U : 
                        this.V;
                }
                set
                {
                    if (this._orientation == Orientation.Horizontal)
                    {
                        this.V = value;
                    }
                    else
                    {
                        this.U = value;
                    }
                }
            }
        }

        /// <summary>
        /// Property changed handler for the AlignLastItems property.</summary>
        /// <param name="d">BalancedWrapPanel that changed.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnAlignLastItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (BalancedWrapPanel)d;
            // Only arrange is affected
            source.InvalidateArrange();
        }
    }
}
