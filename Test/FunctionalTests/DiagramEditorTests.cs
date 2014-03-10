//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

namespace FunctionalTests
{
    [TestFixture]
    public class DiagramEditorTests : TestBase
    {
        protected override string GetAppName()
        {
            return "DiagramEditor";
        }

        [Test]
        [Category(Consts.SmokeTestCategory)]
        public void CreateNewDocuments()
        {
            ExecuteFullTest(ConstructScriptPath());
        }

        #region CircuitEditor specific tests

        [Test]
        public void CircuitEditorAddAllItems()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_circuitEditorFolder, "AddAllItems.py"));
            RunTest(scriptPath, m_circuitEditorSetupStatements);
        }

        [Test]
        public void CircuitEditorEditSaveCloseAndReopen()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_circuitEditorFolder, "EditSaveCloseAndReopen.py"));
            RunTest(scriptPath, m_circuitEditorSetupStatements);
        }

        [Test]
        public void CircuitEditorTestLayers()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_circuitEditorFolder, "TestLayers.py"));
            RunTest(scriptPath, m_circuitEditorSetupStatements);
        }

        #endregion //CircuitEditor
        
        #region FsmEditor specific tests

        [Test]
        public void FsmEditorCopyPaste()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_fsmEditorFolder, "CopyPaste.py"));
            RunTest(scriptPath, m_fsmEditorSetupStatements);
        }

        [Test]
        public void FsmEditorDeleteUndoAndRedoTransitions()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_fsmEditorFolder, "DeleteUndoAndRedoTransitions.py"));
            RunTest(scriptPath, m_fsmEditorSetupStatements);
        }

        [Test]
        public void FsmEditorEditSaveCloseAndReopen()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_fsmEditorFolder, "EditSaveCloseAndReopen.py"));
            RunTest(scriptPath, m_fsmEditorSetupStatements);
        }

        [Test]
        public void FsmEditorInsertComments()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_fsmEditorFolder, "InsertComments.py"));
            RunTest(scriptPath, m_fsmEditorSetupStatements);
        }

        [Test]
        public void FsmEditorInsertTransitions()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_fsmEditorFolder, "InsertTransitions.py"));
            RunTest(scriptPath, m_fsmEditorSetupStatements);
        }

        [Test]
        public void FsmEditorInsertUndoAndRedoTransitions()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_fsmEditorFolder, "InsertUndoAndRedoTransitions.py"));
            RunTest(scriptPath, m_fsmEditorSetupStatements);
        }

        #endregion //FsmEditor
        
        #region StatechartEditor specific tests

        [Test]
        public void StatechartAddAllItems()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_stateChartEditorFolder, "AddAllItems.py"));
            RunTest(scriptPath, m_stateChartEditorSetupStatements);
        }

        [Test]
        public void StatechartEditSaveCloseAndReopen()
        {
            string scriptPath = Path.GetFullPath(Path.Combine(m_stateChartEditorFolder, "EditSaveCloseAndReopen.py"));
            RunTest(scriptPath, m_stateChartEditorSetupStatements);
        }

        #endregion //StatechartEditor

        private void RunTest(string scriptPath, IEnumerable<string> setupStatements)
        {
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("Script file not found: " + scriptPath);
            }
            Console.WriteLine("Starting test script: {0}", scriptPath);

            //Must setup our own app settings before launching the process
            SetupAppSettings();
            int port;
            LaunchTestApplication(GetAppExePath(), out port);
            Connect(port);
            SetupScript(scriptPath);

            foreach (string statement in setupStatements)
            {
                ExecuteStatementSafe(statement);
            }

            ProcessScript(scriptPath);

            CloseApplication();
            VerifyApplicationClosed();
        }

        private const string m_circuitEditorFolder = "CircuitEditorTestScripts";
        private readonly string[] m_circuitEditorSetupStatements =
                                                                {
                                                                    "import CircuitEditorSample",
                                                                    "editor = circuitEditor",
                                                                    "Schema = CircuitEditorSample.Schema",
                                                                    "Annotation = CircuitEditorSample.Annotation",
                                                                };

        private const string m_fsmEditorFolder = "FsmEditorTestScripts";
        private readonly string[] m_fsmEditorSetupStatements =
                                                                {
                                                                    "import FsmEditorSample",
                                                                    "editor = fsmEditor",
                                                                    "Schema = FsmEditorSample.Schema",
                                                                    "Annotation = FsmEditorSample.Annotation",
                                                                };

        private const string m_stateChartEditorFolder = "StatechartEditorTestScripts";
        private readonly string[] m_stateChartEditorSetupStatements =
                                                                {
                                                                    "import StatechartEditorSample",
                                                                    "editor = stateChartEditor",
                                                                    "Schema = StatechartEditorSample.Schema",
                                                                    "State = StatechartEditorSample.State",
                                                                    "Annotation = StatechartEditorSample.Annotation",
                                                                };
}
}
