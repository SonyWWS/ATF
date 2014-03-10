//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class FsmEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "FsmEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocument()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(@".\CommonTestScripts", "CreateNewDocument.py"));
            ExecuteFullTest(scriptPath);
        }

        [Test]
        public void CopyPaste()
        {
            TimeOutInSecs = 500;
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void CutPaste()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void DeleteUndoAndRedoComments()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void DeleteUndoAndRedoStates()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void DeleteUndoAndRedoTransitions()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void EditSaveCloseAndReopen()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        //[Test]
        [ExpectedException(typeof(AssertionException))]
        [Ignore("Ignore if NUnit output is logged (at UnitTestListener::TestOutput in Program.cs)")]
        public void ExpectedFailureAssert()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        [ExpectedException(typeof(FunctionalTestException))]
        public void ExpectedFailureNoSuccessMessage()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ExpectedFailureScriptDoesNotExist()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        [ExpectedException(typeof(TimeoutException))]
        public void ExpectedTimeout()
        {
            TimeOutInSecs = 10;
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void InsertComments()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void InsertStates()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void InsertTransitions()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void InsertUndoAndRedoComments()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void InsertUndoAndRedoStates()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        [Test]
        public void InsertUndoAndRedoTransitions()
        {
            ExecuteFullTest(ConstructScriptPath());
        }
    }
}
