//-----------------------------------------------------------------------
// <copyright file="ClippingBorder.cs" company="Microsoft Corporation copyright 2008.">
// (c) 2008 Microsoft Corporation. All rights reserved.
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// </copyright>
// <date>07-Oct-2008</date>
// <author>Martin Grayson</author>
// <summary>A border that clips its contents.</summary>
//-----------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// A border that clips its contents.</summary>
    public class ClippingBorder : ContentControl
    {
        /// <summary>
        /// The corner radius property.</summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ClippingBorder), new PropertyMetadata(CornerRadius_Changed));

        /// <summary>
        /// The clip content property.</summary>
        public static readonly DependencyProperty ClipContentProperty =
            DependencyProperty.Register("ClipContent", typeof(bool), typeof(ClippingBorder), new PropertyMetadata(ClipContent_Changed));

        /// <summary>
        /// ClippingBorder constructor.</summary>
        public ClippingBorder()
        {
            this.DefaultStyleKey = typeof(ClippingBorder);
            this.SizeChanged += this.ClippingBorder_SizeChanged;
        }

        /// <summary>
        /// Gets or sets the border corner radius.
        /// This is a thickness, as there is a problem parsing CornerRadius types.</summary>
        [System.ComponentModel.Category("Appearance"), System.ComponentModel.Description("Sets the corner radius on the border.")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content is clipped. </summary>
        [System.ComponentModel.Category("Appearance"), System.ComponentModel.Description("Sets whether the content is clipped or not.")]
        public bool ClipContent
        {
            get { return (bool)GetValue(ClipContentProperty); }
            set { SetValue(ClipContentProperty, value); }
        }

        /// <summary>
        /// Gets the UI elements out of the template. </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.m_border = this.GetTemplateChild("PART_Border") as Border;
            this.m_topLeftContentControl = this.GetTemplateChild("PART_TopLeftContentControl") as ContentControl;
            this.m_topRightContentControl = this.GetTemplateChild("PART_TopRightContentControl") as ContentControl;
            this.m_bottomRightContentControl = this.GetTemplateChild("PART_BottomRightContentControl") as ContentControl;
            this.m_bottomLeftContentControl = this.GetTemplateChild("PART_BottomLeftContentControl") as ContentControl;

            if (this.m_topLeftContentControl != null)
            {
                this.m_topLeftContentControl.SizeChanged += this.ContentControl_SizeChanged;
            }

            this.m_topLeftClip = this.GetTemplateChild("PART_TopLeftClip") as RectangleGeometry;
            this.m_topRightClip = this.GetTemplateChild("PART_TopRightClip") as RectangleGeometry;
            this.m_bottomRightClip = this.GetTemplateChild("PART_BottomRightClip") as RectangleGeometry;
            this.m_bottomLeftClip = this.GetTemplateChild("PART_BottomLeftClip") as RectangleGeometry;

            this.UpdateClipContent(this.ClipContent);

            this.UpdateCornerRadius(this.CornerRadius);
        }

        /// <summary>
        /// Sets the corner radius. </summary>
        /// <param name="newCornerRadius">The new corner radius.</param>
        internal void UpdateCornerRadius(CornerRadius newCornerRadius)
        {
            if (this.m_border != null)
            {
                this.m_border.CornerRadius = newCornerRadius;
            }

            if (this.m_topLeftClip != null)
            {
                this.m_topLeftClip.RadiusX = this.m_topLeftClip.RadiusY = newCornerRadius.TopLeft - (Math.Min(this.BorderThickness.Left, this.BorderThickness.Top) / 2);
            }

            if (this.m_topRightClip != null)
            {
                this.m_topRightClip.RadiusX = this.m_topRightClip.RadiusY = newCornerRadius.TopRight - (Math.Min(this.BorderThickness.Top, this.BorderThickness.Right) / 2);
            }

            if (this.m_bottomRightClip != null)
            {
                this.m_bottomRightClip.RadiusX = this.m_bottomRightClip.RadiusY = newCornerRadius.BottomRight - (Math.Min(this.BorderThickness.Right, this.BorderThickness.Bottom) / 2);
            }

            if (this.m_bottomLeftClip != null)
            {
                this.m_bottomLeftClip.RadiusX = this.m_bottomLeftClip.RadiusY = newCornerRadius.BottomLeft - (Math.Min(this.BorderThickness.Bottom, this.BorderThickness.Left) / 2);
            }

            this.UpdateClipSize(new Size(this.ActualWidth, this.ActualHeight));
        }

        /// <summary>
        /// Updates whether the content is clipped. </summary>
        /// <param name="clipContent">Whether the content is clipped.</param>
        internal void UpdateClipContent(bool clipContent)
        {
            if (clipContent)
            {
                if (this.m_topLeftContentControl != null)
                {
                    this.m_topLeftContentControl.Clip = this.m_topLeftClip;
                }

                if (this.m_topRightContentControl != null)
                {
                    this.m_topRightContentControl.Clip = this.m_topRightClip;
                }

                if (this.m_bottomRightContentControl != null)
                {
                    this.m_bottomRightContentControl.Clip = this.m_bottomRightClip;
                }

                if (this.m_bottomLeftContentControl != null)
                {
                    this.m_bottomLeftContentControl.Clip = this.m_bottomLeftClip;
                }

                this.UpdateClipSize(new Size(this.ActualWidth, this.ActualHeight));
            }
            else
            {
                if (this.m_topLeftContentControl != null)
                {
                    this.m_topLeftContentControl.Clip = null;
                }

                if (this.m_topRightContentControl != null)
                {
                    this.m_topRightContentControl.Clip = null;
                }

                if (this.m_bottomRightContentControl != null)
                {
                    this.m_bottomRightContentControl.Clip = null;
                }

                if (this.m_bottomLeftContentControl != null)
                {
                    this.m_bottomLeftContentControl.Clip = null;
                }
            }
        }

        /// <summary>
        /// Updates the corner radius. </summary>
        /// <param name="dependencyObject">The clipping border.</param>
        /// <param name="eventArgs">Dependency Property Changed Event Args</param>
        private static void CornerRadius_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ClippingBorder clippingBorder = (ClippingBorder)dependencyObject;
            clippingBorder.UpdateCornerRadius((CornerRadius)eventArgs.NewValue);
        }

        /// <summary>
        /// Updates the content clipping. </summary>
        /// <param name="dependencyObject">The clipping border.</param>
        /// <param name="eventArgs">Dependency Property Changed Event Args</param>
        private static void ClipContent_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ClippingBorder clippingBorder = (ClippingBorder)dependencyObject;
            clippingBorder.UpdateClipContent((bool)eventArgs.NewValue);
        }

        /// <summary>
        /// Updates the clips. </summary>
        /// <param name="sender">The clipping border</param>
        /// <param name="e">Size Changed Event Args.</param>
        private void ClippingBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ClipContent)
            {
                this.UpdateClipSize(e.NewSize);
            }
        }

        /// <summary>
        /// Updates the clip size. </summary>
        /// <param name="sender">A content control.</param>
        /// <param name="e">Size Changed Event Args</param>
        private void ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.ClipContent)
            {
                this.UpdateClipSize(new Size(this.ActualWidth, this.ActualHeight));
            }
        }

        /// <summary>
        /// Updates the clip size. </summary>
        /// <param name="size">The control size.</param>
        private void UpdateClipSize(Size size)
        {
            if (size.Width > 0 || size.Height > 0)
            {
                double contentWidth = Math.Max(0, size.Width - this.BorderThickness.Left - this.BorderThickness.Right);
                double contentHeight = Math.Max(0, size.Height - this.BorderThickness.Top - this.BorderThickness.Bottom);

                if (this.m_topLeftClip != null)
                {
                    this.m_topLeftClip.Rect = new Rect(0, 0, contentWidth + (this.CornerRadius.TopLeft * 2), contentHeight + (this.CornerRadius.TopLeft * 2));
                }

                if (this.m_topRightClip != null)
                {
                    this.m_topRightClip.Rect = new Rect(0 - this.CornerRadius.TopRight, 0, contentWidth + this.CornerRadius.TopRight, contentHeight + this.CornerRadius.TopRight);
                }

                if (this.m_bottomRightClip != null)
                {
                    this.m_bottomRightClip.Rect = new Rect(0 - this.CornerRadius.BottomRight, 0 - this.CornerRadius.BottomRight, contentWidth + this.CornerRadius.BottomRight, contentHeight + this.CornerRadius.BottomRight);
                }

                if (this.m_bottomLeftClip != null)
                {
                    this.m_bottomLeftClip.Rect = new Rect(0, 0 - this.CornerRadius.BottomLeft, contentWidth + this.CornerRadius.BottomLeft, contentHeight + this.CornerRadius.BottomLeft);
                }
            }
        }

        /// <summary>
        /// Stores the top left content control.</summary>
        private ContentControl m_topLeftContentControl;

        /// <summary>
        /// Stores the top right content control.</summary>
        private ContentControl m_topRightContentControl;

        /// <summary>
        /// Stores the bottom right content control.</summary>
        private ContentControl m_bottomRightContentControl;

        /// <summary>
        /// Stores the bottom left content control.</summary>
        private ContentControl m_bottomLeftContentControl;

        /// <summary>
        /// Stores the clip responsible for clipping the top left corner.</summary>
        private RectangleGeometry m_topLeftClip;

        /// <summary>
        /// Stores the clip responsible for clipping the top right corner.</summary>
        private RectangleGeometry m_topRightClip;

        /// <summary>
        /// Stores the clip responsible for clipping the bottom right corner.</summary>
        private RectangleGeometry m_bottomRightClip;

        /// <summary>
        /// Stores the clip responsible for clipping the bottom left corner.</summary>
        private RectangleGeometry m_bottomLeftClip;

        /// <summary>
        /// Stores the main border.</summary>
        private Border m_border;

    }
}
