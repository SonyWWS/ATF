//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using NUnit.Framework;

using Sce.Atf;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestReentryGuard
    {
        [Test]
        public void TestConstructor()
        {
            ReentryGuard test = new ReentryGuard();
            Assert.True(test.CanEnter);
            Assert.False(test.HasEntered);
        }

        [Test]
        public void TestEnterAndExit()
        {
            ReentryGuard test = new ReentryGuard();
            Assert.True(test.CanEnter);
            using (test.EnterAndExit())
            {
                Assert.True(!test.CanEnter);
            }
            Assert.True(test.CanEnter);
        }

        [Test]
        public void TestFailedEntry()
        {
            ReentryGuard test = new ReentryGuard();
            using (test.EnterAndExit())
            {
                Assert.Throws<InvalidOperationException>(delegate { test.EnterAndExit(); });
            }
        }

        [Test]
        public void TestEnterAndExitMultiple()
        {
            ReentryGuard test = new ReentryGuard();
            using (IDisposable guard = test.EnterAndExitMultiple())
            {
                Assert.True(test.HasEntered);
                Assert.False(test.CanEnter);
                Assert.DoesNotThrow(delegate
                {
                    using (guard)
                    {
                        using (guard)
                        {
                        }
                    }
                });
            }
        }
    }
}
