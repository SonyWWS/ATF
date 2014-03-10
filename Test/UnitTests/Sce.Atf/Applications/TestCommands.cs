//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using NUnit.Framework;
using Sce.Atf.Applications;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestCommands
    {
        [Test]
        public void TestCommandHistory()
        {
            PositiveInt x = new PositiveInt();
            CommandHistory test = new CommandHistory();

            // test initial state
            Assert.IsTrue(!test.CanUndo);
            Assert.IsTrue(!test.CanRedo);
            Assert.IsTrue(!test.Dirty);

            // test adding command
            Command cmd = new IncCommand(x);
            test.Add(cmd);
            cmd.Do();
            Assert.IsTrue(x.Value == 1);
            Assert.IsTrue(test.CanUndo);
            Assert.IsTrue(!test.CanRedo);
            Assert.IsTrue(test.Dirty);

            test.Undo();
            Assert.IsTrue(x.Value == 0);
            Assert.IsTrue(!test.CanUndo);
            Assert.IsTrue(test.CanRedo);
            Assert.IsTrue(!test.Dirty);

            test.Redo();
            Assert.IsTrue(x.Value == 1);
            Assert.IsTrue(test.CanUndo);
            Assert.IsTrue(!test.CanRedo);
            Assert.IsTrue(test.Dirty);

            test.Dirty = false;
            Assert.IsTrue(!test.Dirty);

            cmd = new IncCommand(x);
            test.Add(cmd);
            cmd.Do();
            Assert.IsTrue(test.Dirty);
            test.Undo();
            Assert.IsTrue(!test.Dirty);

            test.Dirty = true;
            Assert.IsTrue(test.Dirty);
        }

        [Test]
        public void TestCompositeCommand()
        {
            PositiveInt x = new PositiveInt();

            CompositeCommand composite1 = new CompositeCommand("1", new Command[] { new IncCommand(x), new IncCommand(x) });

            composite1.Do();
            Assert.IsTrue(x.Value == 2);
            composite1.Undo();
            Assert.IsTrue(x.Value == 0);

            CompositeCommand composite2 = new CompositeCommand("1", new Command[] { new IncCommand(x), new DecCommand(x), new DecCommand(x) });

            // Try to execute a composite that will cause an exception
            try
            {
                composite2.Do();
                Assert.Fail();
            }
            catch
            {
            }
            Assert.IsTrue(x.Value == 0); // make sure command was backed out
        }

        /// <summary>
        /// Simple class to act like data repository</summary>
        private class PositiveInt
        {
            public int Value
            {
                get { return _value; }
                set
                {
                    _value = value;

                    if (Set != null)
                        Set(this, EventArgs.Empty);
                }
            }

            public void Inc()
            {
                _value++;

                if (Incremented != null)
                    Incremented(this, EventArgs.Empty);
            }

            public void Dec()
            {
                if (_value == 0)
                    throw new InvalidOperationException();
                _value--;

                if (Decremented != null)
                    Decremented(this, EventArgs.Empty);
            }

            public event EventHandler Incremented;
            public event EventHandler Decremented;
            public event EventHandler Set;

            private int _value = 0;
        }

        /// <summary>
        /// Simple class to implement command on repository</summary>
        private class IncCommand : Command
        {
            public IncCommand(PositiveInt target)
                : base("IncCommand")
            {
                this.target = target;
            }

            public override void Do() { target.Inc(); }

            public override void Undo() { target.Dec(); }

            private PositiveInt target;
        }

        /// <summary>
        /// Simple class to implement command on repository</summary>
        private class DecCommand : Command
        {
            public DecCommand(PositiveInt target)
                : base("DecCommand")
            {
                this.target = target;
            }

            public override void Do() { target.Dec(); }

            public override void Undo() { target.Inc(); }

            private PositiveInt target;
        }
    }
}
