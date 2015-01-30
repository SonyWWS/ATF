//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestEvent
    {
        [Test]
        public void TestRaise()
        {
            Test += TestEvent_Test;

            // test static method
            m_calls = 0;
            m_eventArgs = new EventArgs();
            Test.Raise(this, m_eventArgs);
            Assert.True(m_calls == 1);

            // test extension method
            m_calls = 0;
            m_eventArgs = new EventArgs();
            Test.Raise(this, m_eventArgs);
            Assert.True(m_calls == 1);
        }

        private event EventHandler Test;

        private void TestEvent_Test(object sender, EventArgs e)
        {
            Assert.AreSame(sender, this);
            Assert.AreSame(e, m_eventArgs);
            m_calls++;
        }

        [Test]
        public void TestRaiseGeneric()
        {
            TestGeneric += TestEvent_TestGeneric;

            // test static method
            m_calls = 0;
            m_eventArgs = new EventArgs();
            TestGeneric.Raise(this, m_eventArgs);
            Assert.True(m_calls == 1);

            // test extension method
            m_calls = 0;
            m_eventArgs = new EventArgs();
            TestGeneric.Raise(this, m_eventArgs);
            Assert.True(m_calls == 1);
        }

        private event EventHandler<EventArgs> TestGeneric;

        private void TestEvent_TestGeneric(object sender, EventArgs e)
        {
            Assert.AreSame(sender, this);
            Assert.AreSame(e, m_eventArgs);
            m_calls++;
        }

        [Test]
        public void TestRaiseCancellable()
        {
            // set up 3 callbacks
            TestCancellable += TestEvent_TestCancellable;
            TestCancellable += TestEvent_TestCancellable;
            TestCancellable += TestEvent_TestCancellable;

            // test static method
            m_calls = 0;
            m_cancelEventArgs = new CancelEventArgs();
            Assert.True(TestCancellable.RaiseCancellable(this, m_cancelEventArgs));
            Assert.True(m_calls == 2);

            // test extension method
            m_calls = 0;
            m_cancelEventArgs = new CancelEventArgs();
            Assert.True(TestCancellable.RaiseCancellable(this, m_cancelEventArgs));
            Assert.True(m_calls == 2);
        }

        private event EventHandler<CancelEventArgs> TestCancellable;

        private void TestEvent_TestCancellable(object sender, CancelEventArgs e)
        {
            Assert.AreSame(sender, this);
            Assert.AreSame(e, m_cancelEventArgs);
            m_calls++;
            if (m_calls == 2)
                e.Cancel = true;
        }

        private EventArgs m_eventArgs;
        private CancelEventArgs m_cancelEventArgs;
        private int m_calls;
    }
}
