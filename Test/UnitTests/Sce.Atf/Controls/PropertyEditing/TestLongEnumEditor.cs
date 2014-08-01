//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Sce.Atf.Controls.PropertyEditing;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestLongEnumEditor
    {
        [Test]
        public void Test()
        {
            Monitor.Enter(m_lock);

            // Only a single-threaded apartment can create the ComboBox. Run the test on an STA thread.
            var t = new Thread(TestOnStaThread);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            // Join() doesn't work here, on our desktop computers! The 2nd thread starts, but execution
            //  on this thread continues and then the tests don't get run.
            //t.Join();

            Monitor.Wait(m_lock);
            Monitor.Exit(m_lock);

            if (m_exception != null)
                throw new Exception("TestOnStaThread() threw the following exception", m_exception);
        }

        // Call only on a single-threaded apartment thread.
        private void TestOnStaThread()
        {
            try
            {
                Monitor.Enter(m_lock);

                // Basic test using strings to define the list.
                var longEnumEditor = new LongEnumEditor();
                longEnumEditor.DefineEnum(new[] {"One", "Two", "Three"});
                longEnumEditor.MaxDropDownItems = 50;

                Control editingControl = longEnumEditor.GetEditingControl(null);
                var comboBox = editingControl as ComboBox;
                if (comboBox != null)
                {
                    Assert.IsTrue(comboBox.Items.Count == 3);
                    Assert.IsTrue(comboBox.Items[0].ToString() == "One");
                    Assert.IsTrue(comboBox.Items[1].ToString() == "Two");
                    Assert.IsTrue(comboBox.Items[2].ToString() == "Three");
                }

                // Use display names that are different than enum names.
                longEnumEditor = new LongEnumEditor();
                longEnumEditor.DefineEnum(new[] {"One==Neo", "Two==Zwei", "Three==Tres"});
                longEnumEditor.MaxDropDownItems = 50;

                editingControl = longEnumEditor.GetEditingControl(null);
                comboBox = editingControl as ComboBox;
                if (comboBox != null)
                {
                    Assert.IsTrue(comboBox.Items.Count == 3);
                    Assert.IsTrue(comboBox.Items[0].ToString() == "Neo");
                    Assert.IsTrue(comboBox.Items[1].ToString() == "Zwei");
                    Assert.IsTrue(comboBox.Items[2].ToString() == "Tres");
                }

                // Programmatically add a new item to the list.
                longEnumEditor.DefineEnum(new[] {"One==Neo", "Two==Zwei", "Three==Tres", "Four"});
                editingControl = longEnumEditor.GetEditingControl(null);
                comboBox = editingControl as ComboBox;
                if (comboBox != null)
                {
                    Assert.IsTrue(comboBox.Items.Count == 4);
                    Assert.IsTrue(comboBox.Items[0].ToString() == "Neo");
                    Assert.IsTrue(comboBox.Items[1].ToString() == "Zwei");
                    Assert.IsTrue(comboBox.Items[2].ToString() == "Tres");
                    Assert.IsTrue(comboBox.Items[3].ToString() == "Four");
                }

                // Use an enum.
                longEnumEditor = new LongEnumEditor(typeof (EnumTest));
                editingControl = longEnumEditor.GetEditingControl(null);
                comboBox = editingControl as ComboBox;
                if (comboBox != null)
                {
                    Assert.IsTrue(comboBox.Items.Count == 4);
                    Assert.IsTrue(comboBox.Items[0].ToString() == "Zero");
                    Assert.IsTrue(comboBox.Items[1].ToString() == "One");
                    Assert.IsTrue(comboBox.Items[2].ToString() == "Two");
                    Assert.IsTrue(comboBox.Items[3].ToString() == "Three");
                }

                // Test converter.           
                var converter = new IntEnumTypeConverter(typeof (EnumText2));

                // convert from string to int.
                Assert.IsTrue(converter.CanConvertFrom(typeof (string)));
                object result = converter.ConvertFrom("Five");
                Assert.IsTrue(result != null && (int) result == 5);

                // convert from int to string.
                Assert.IsTrue(converter.CanConvertTo(typeof (string)));
                object result2 = converter.ConvertTo(7, typeof (string));
                Assert.IsTrue(result2 != null && (string) result2 == "");

                result2 = converter.ConvertTo(8, typeof (string));
                Assert.IsTrue(result2 != null && (string) result2 == "Eight");
            }
            catch (Exception e)
            {
                m_exception = e;
            }
            finally
            {
                Monitor.Pulse(m_lock);
                Monitor.Exit(m_lock);
            }
        }

        private enum EnumTest
        {
            Zero,
            One,
            Two,
            Three
        }

        private enum EnumText2
        {
            Zero = 0,
            Three = 3,
            Five = 5,
            Six = 6,
            Eight = 8
        }

        private object m_lock = new object();
        private Exception m_exception;
    }
}
