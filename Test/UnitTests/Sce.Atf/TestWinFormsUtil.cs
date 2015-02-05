//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestWinFormsUtil
    {
        [Test]
        public void TestUpdateBounds()
        {
            var original = new Rectangle(1, 2, 3, 4);
            var replacement = new Rectangle(5, 6, 7, 8);
            Rectangle updated;

            updated = WinFormsUtil.UpdateBounds(original, replacement, BoundsSpecified.All);
            Assert.True(updated == replacement);

            updated = WinFormsUtil.UpdateBounds(original, replacement, BoundsSpecified.Location);
            Assert.True(updated.Location == replacement.Location);
            Assert.True(updated.Size == original.Size);

            updated = WinFormsUtil.UpdateBounds(original, replacement, BoundsSpecified.Width | BoundsSpecified.Height);
            Assert.True(updated.Size == replacement.Size);
            Assert.True(updated.Location == original.Location);

            updated = WinFormsUtil.UpdateBounds(original, replacement, BoundsSpecified.X);
            Assert.True(updated.X == replacement.X);
            Assert.True(updated.Y == original.Y);
            Assert.True(updated.Width == original.Width);
            Assert.True(updated.Y == original.Y);
        }

        [Test]
        public void TestUpdateScrollbars()
        {
            var vBar = new VScrollBar();
            var hBar = new HScrollBar();
            Rectangle visibleArea, contentArea, finalVisible;

            // Visible area surrounds content area, so scrollbars are not required
            contentArea = new Rectangle(0, 0, 100, 100);
            visibleArea = contentArea;
            visibleArea.Inflate(20, 20);
            finalVisible = WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.False(vBar.Visible);
            Assert.False(hBar.Visible);
            Assert.AreEqual(visibleArea, finalVisible);

            // Visible area is inside content area, so scrollbars are required
            contentArea = new Rectangle(0, 0, 100, 100);
            visibleArea = contentArea;
            visibleArea.Inflate(-20, -20);
            finalVisible = WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.True(vBar.Visible);
            Assert.True(hBar.Visible);
            Assert.True(finalVisible.Width < visibleArea.Width);
            Assert.True(finalVisible.Height < visibleArea.Height);
            Assert.True(hBar.Minimum == contentArea.Left);
            Assert.True(hBar.Maximum == contentArea.Right);
            Assert.True(vBar.Minimum == contentArea.Top);
            Assert.True(vBar.Maximum == contentArea.Bottom);

            // Visible area surrounds content area, so scrollbars are not required. Uses negative coordinates.
            contentArea = new Rectangle(-1000, 0, 100, 100);
            visibleArea = contentArea;
            visibleArea.Inflate(20, 20);
            finalVisible = WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.False(vBar.Visible);
            Assert.False(hBar.Visible);
            Assert.AreEqual(visibleArea, finalVisible);

            // Visible area is inside content area, so scrollbars are required. Uses negative coordinates.
            contentArea = new Rectangle(-1000, 0, 100, 100);
            visibleArea = contentArea;
            visibleArea.Inflate(-20, -20);
            finalVisible = WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.True(vBar.Visible);
            Assert.True(hBar.Visible);
            Assert.True(finalVisible.Width < visibleArea.Width);
            Assert.True(finalVisible.Height < visibleArea.Height);
            Assert.True(hBar.Minimum == contentArea.Left);
            Assert.True(hBar.Maximum == contentArea.Right);
            Assert.True(vBar.Minimum == contentArea.Top);
            Assert.True(vBar.Maximum == contentArea.Bottom);

            // Visible area is to the left of content area, so scrollbars are required
            contentArea = new Rectangle(1000, 0, 100, 100);
            visibleArea = new Rectangle(0, 0, 100, 100);
            finalVisible = WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.True(vBar.Visible);
            Assert.True(hBar.Minimum == visibleArea.Left);
            Assert.True(hBar.Maximum == contentArea.Right);
            Assert.True(vBar.Minimum == contentArea.Top);
            Assert.True(vBar.Maximum == contentArea.Bottom);

            // Zero visible area!
            contentArea = new Rectangle(1000, 1000, 100, 100);
            visibleArea = new Rectangle();
            WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.True(vBar.Visible);
            Assert.True(hBar.Visible);

            // Zero content area, off screen.
            visibleArea = new Rectangle(1000, 1000, 100, 100);
            contentArea = new Rectangle(10, 20, 0, 0);
            WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.True(vBar.Visible);
            Assert.True(hBar.Visible);

            // Zero content area, on screen.
            visibleArea = new Rectangle(1000, 1000, 100, 100);
            contentArea = new Rectangle(1000, 1000, 0, 0);
            WinFormsUtil.UpdateScrollbars(vBar, hBar, visibleArea, contentArea);
            Assert.False(vBar.Visible);
            Assert.False(hBar.Visible);

            // The scrollbars are optional.
            contentArea = new Rectangle(-1000, 0, 100, 100);
            visibleArea = contentArea;
            visibleArea.Inflate(-20, -20);
            finalVisible = WinFormsUtil.UpdateScrollbars(null, null, visibleArea, contentArea);
            Assert.AreEqual(visibleArea, finalVisible);
        }
    }
}
