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
    }
}
