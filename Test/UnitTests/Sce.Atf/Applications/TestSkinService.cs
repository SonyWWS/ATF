//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

using Sce.Atf.Applications;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestSkinService
    {
        [Test]
        public void TestSkinApplying()
        {
            var skinService = new SkinService();

            var earlyControl = new TestControlA();
            Color originalBackColor = earlyControl.BackColor;
            SkinService.ApplyActiveSkin(earlyControl);

            skinService.OpenSkinFile(".\\Resources\\TestSkin.xml");

            // The new value should not be applied yet.
            Assert.IsTrue(earlyControl.BackColor == originalBackColor);

            skinService.ApplyActiveSkin();

            // The new color should be applied now. http://tracker.ship.scea.com/jira/browse/WWSATF-1028
            Assert.IsTrue((uint)earlyControl.BackColor.ToArgb() == 0xff00000a);

            var controlA = new TestControlA();
            SkinService.ApplyActiveSkin(controlA);
            Assert.IsTrue((uint)controlA.BackColor.ToArgb() == 0xff00000a);
            Assert.IsTrue(string.Equals(controlA.StringA, "TestSkinValueA"));

            var controlB = new TestControlB();
            SkinService.ApplyActiveSkin(controlB);
            Assert.IsTrue((uint)controlB.BackColor.ToArgb() == 0xff00000b);
            Assert.IsTrue(string.Equals(controlB.StringA, "TestSkinValueA"));
            Assert.IsTrue(string.Equals(controlB.StringB, "TestSkinValueB"));

            var controlC = new TestControlC();
            SkinService.ApplyActiveSkin(controlC);
            Assert.IsTrue((uint)controlC.BackColor.ToArgb() == 0xff00000c);
            Assert.IsTrue(string.Equals(controlC.StringA, "TestSkinValueA"));
            Assert.IsTrue(string.Equals(controlC.StringB, "TestSkinValueB"));
            Assert.IsTrue(string.Equals(controlC.StringC, "TestSkinValueC"));

            var controlX = new TestControlX();
            SkinService.ApplyActiveSkin(controlX);
            Assert.IsTrue(string.Equals(controlX.StringA, "TestSkinValueX"));

            var controlY = new TestControlY();
            SkinService.ApplyActiveSkin(controlY);
            Assert.IsTrue(string.Equals(controlY.StringA, "TestSkinValueY"));

            var controlZ = new TestControlZ();
            SkinService.ApplyActiveSkin(controlZ);
            Assert.IsTrue(string.Equals(controlZ.StringA, "TestSkinValueZ"));

            //DerivedUndefinedControl derives from TestControlC, so should receive C's style
            var undefinedControl = new DerivedUndefinedControl();
            SkinService.ApplyActiveSkin(undefinedControl);
            Assert.IsTrue((uint)undefinedControl.BackColor.ToArgb() == 0xff00000c);
            Assert.IsTrue(string.Equals(undefinedControl.StringA, "TestSkinValueA"));
            Assert.IsTrue(string.Equals(undefinedControl.StringB, "TestSkinValueB"));
            Assert.IsTrue(string.Equals(undefinedControl.StringC, "TestSkinValueC"));

            // Make sure that the skins are reset correctly.
            skinService.ResetSkin(); //sets the active skin to null
            Assert.IsTrue(controlA.BackColor == originalBackColor);
            Assert.IsTrue(controlA.StringA == null);

            Assert.IsTrue(controlB.BackColor == originalBackColor);
            Assert.IsTrue(controlB.StringA == null);
            Assert.IsTrue(controlB.StringB == null);

            Assert.IsTrue(controlC.BackColor == originalBackColor);
            Assert.IsTrue(controlC.StringA == null);
            Assert.IsTrue(controlC.StringB == null);
            Assert.IsTrue(controlC.StringC == null);

            Assert.IsTrue(undefinedControl.BackColor == originalBackColor);
            Assert.IsTrue(undefinedControl.StringA == null);
            Assert.IsTrue(undefinedControl.StringB == null);
            Assert.IsTrue(undefinedControl.StringC == null);

            // Apply the skin again, this time with new original values.
            // To-do: it's kind of odd that we have to reload the xml file. What if clients wanted to rapidly
            //  switch back and forth programmatically?
            controlA.StringA = "OriginalA";
            controlB.StringB = "OriginalB";
            controlC.StringC = "OriginalC";
            undefinedControl.StringC = "OriginalC";
            skinService.OpenSkinFile(".\\Resources\\TestSkin.xml");
            skinService.ApplyActiveSkin();
            SkinService.ApplyActiveSkin(controlA);
            SkinService.ApplyActiveSkin(controlB);
            SkinService.ApplyActiveSkin(controlC);
            SkinService.ApplyActiveSkin(undefinedControl);
            skinService.ResetSkin();
            Assert.IsTrue(controlA.StringA == "OriginalA");
            Assert.IsTrue(controlB.StringB == "OriginalB");
            Assert.IsTrue(controlC.StringC == "OriginalC");
            Assert.IsTrue(undefinedControl.StringC == "OriginalC");
        }
        
        [Test]
        public void TestSkinningNonControls()
        {
            var skinService = new SkinService();
            var generalA = new GeneralObjectA();

            skinService.OpenSkinFile(".\\Resources\\TestSkin.xml");
            skinService.ApplyActiveSkin();
            SkinService.ApplyActiveSkin(generalA);
            Assert.IsTrue(generalA.StringA == "TestSkinValueA");
            skinService.ResetSkin();
            Assert.IsTrue(generalA.StringA == null);
        }
    }

    public class TestControlA : Control
    {
        public string StringA { get; set; }
    }

    public class TestControlB : TestControlA
    {
        public string StringB { get; set; }
    }

    public class TestControlC : TestControlB
    {
        public string StringC { get; set; }
    }

    public class TestControlX : TestControlA
    {
    }

    public class TestControlY : TestControlA
    {
    }

    public class TestControlZ : TestControlA
    {
    }

    public class DerivedUndefinedControl : TestControlC
    {
    }

    public class GeneralObjectA
    {
        public string StringA { get; set; }
    }
}
